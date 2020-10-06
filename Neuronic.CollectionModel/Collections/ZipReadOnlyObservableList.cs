using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Observables;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     An observable list that pairs the elements from two lists by index.
    /// </summary>
    /// <typeparam name="TOuter">The type of the outer list.</typeparam>
    /// <typeparam name="TInner">The type of the inner list.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.IndexedReadOnlyObservableListBase{System.Tuple{TOuter, TInner}, TResult}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TResult}" />
    /// <seealso cref="Neuronic.CollectionModel.WeakEventPattern.IWeakEventListener" />
    public class ZipReadOnlyObservableList<TOuter, TInner, TResult> : IndexedReadOnlyObservableListBase<Tuple<TOuter, TInner>, TResult>, 
        IReadOnlyObservableList<TResult>, IWeakEventListener
    {
        private readonly IReadOnlyList<TOuter> _outerSource;
        private readonly IReadOnlyList<TInner> _innerSource;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZipReadOnlyObservableList{TOuter, TInner, TResult}"/> class.
        /// </summary>
        /// <param name="outerSource">The outer list.</param>
        /// <param name="innerSource">The inner list.</param>
        /// <param name="selector">The function that creates a result from each pair of items.</param>
        /// <param name="onRemove">The function that is called when removing an item.</param>
        /// <param name="onChange">The function that is called when the result assigned to a pair changes.</param>
        public ZipReadOnlyObservableList(
            IReadOnlyList<TOuter> outerSource, IReadOnlyList<TInner> innerSource,
            Func<TOuter, TInner, IObservable<TResult>> selector, 
            Action<TResult> onRemove = null, Action<TResult, TResult> onChange = null) 
            : base(outerSource.Zip(innerSource, Tuple.Create), t => selector(t.Item1, t.Item2), onRemove, onChange)
        {
            _outerSource = outerSource;
            _innerSource = innerSource;

            if (_innerSource is INotifyCollectionChanged innerNotifier)
                CollectionChangedEventManager.AddListener(innerNotifier, this);
            if (_outerSource is INotifyCollectionChanged outerNotifier)
                CollectionChangedEventManager.AddListener(outerNotifier, this);
        }

        private ZipReadOnlyObservableList(
            IReadOnlyList<TOuter> outerSource, IReadOnlyList<TInner> innerSource,
            PropertyObservableFactory<TOuter, TInner, TResult> selector,
            Action<TResult> onRemove = null, Action<TResult, TResult> onChange = null)
            : this(outerSource, innerSource, selector.Observe, onRemove, onChange)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZipReadOnlyObservableList{TOuter, TInner, TResult}"/> class.
        /// </summary>
        /// <param name="outerSource">The outer list.</param>
        /// <param name="innerSource">The inner list.</param>
        /// <param name="selector">The function that creates a result from each pair of items.</param>
        /// <param name="onRemove">The function that is called when removing an item.</param>
        /// <param name="onChange">The function that is called when the result assigned to a pair changes.</param>
        public ZipReadOnlyObservableList(
            IReadOnlyList<TOuter> outerSource, IReadOnlyList<TInner> innerSource,
            Expression<Func<TOuter, TInner, TResult>> selector,
            Action<TResult> onRemove = null, Action<TResult, TResult> onChange = null)
            : this(outerSource, innerSource, selector.FindProperties(), onRemove, onChange)
        {
        }

        public int Count => Math.Min(_outerSource.Count, _innerSource.Count);

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public TResult this[int index] => Items[index].Value;

        public IEnumerator<TResult> GetEnumerator()
        {
            return Items.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        protected override void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<TResult>(e.NewItems?.Count ?? 0);
            if (e.NewItems != null && e.NewItems.Count > 0)
                newItems.AddRange(e.NewItems.OfType<IndexedItemContainer<Tuple<TOuter, TInner>, TResult>>().Select(x => x.Value));

            var oldItems = new List<TResult>(e.OldItems?.Count ?? 0);
            if (e.OldItems != null && e.OldItems.Count > 0)
                oldItems.AddRange(e.OldItems.OfType<IndexedItemContainer<Tuple<TOuter, TInner>, TResult>>().Select(x => x.Value));

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItems, e.NewStartingIndex, e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset when newItems.Count > 0 && e.NewStartingIndex >= 0:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        protected override void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TResult> e)
        {
            var container = (IndexedItemContainer<Tuple<TOuter, TInner>, TResult>) sender;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue, container.Index));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            base.ContainerOnValueChanged(sender, e);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;
            if (ReferenceEquals(_outerSource, sender))
            {
                SourceOnCollectionChanged((NotifyCollectionChangedEventArgs) e);
            }
            else if (ReferenceEquals(_innerSource, sender))
            {
                SourceOnCollectionChanged((NotifyCollectionChangedEventArgs) e);
            }
            return true;
        }

        private void SourceOnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            int start;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewStartingIndex >= 0:
                    start = e.NewStartingIndex;
                    for (int i = 0; i < e.NewItems.Count && Items.Count < Count; i++, start++)
                        Items.Insert(start, CreateContainerAt(start));
                    UpdateContainers(start, Count - start);
                    break;

                case NotifyCollectionChangedAction.Remove when e.OldStartingIndex >= 0:
                    start = e.OldStartingIndex;
                    for (int i = 0; i < e.OldItems.Count && Items.Count > Count; i++)
                    {
                        RemoveContainer(Items[start]);
                        Items.RemoveAt(start);
                    }
                    UpdateContainers(start, Count - start);
                    break;

                case NotifyCollectionChangedAction.Replace when e.OldStartingIndex >= 0:
                    start = e.OldStartingIndex;
                    UpdateContainers(start, Math.Min(Count - start, e.NewItems.Count));
                    break;

                case NotifyCollectionChangedAction.Move:
                    var min = Math.Min(e.NewStartingIndex, e.OldStartingIndex);
                    var max = Math.Max(e.NewStartingIndex, e.OldStartingIndex);
                    UpdateContainers(min, Math.Min(Count - min, max - min + 1));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    UpdateContainers(0, Count);
                    break;

                default:
                    throw new NotSupportedException("The collection needs a list as input. Order it first if needed.");
            }
        }

        private void UpdateContainers(int start, int count, int step = 1)
        {
            for (int i = 0; i < count; i++)
            {
                var index = start + i * step;
                RemoveContainer(Items[index]);
                Items[index] = CreateContainerAt(index);
            }
        }

        private IndexedItemContainer<Tuple<TOuter, TInner>, TResult> CreateContainerAt(int index)
        {
            return CreateContainer(Tuple.Create(_outerSource[index], _innerSource[index]));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}