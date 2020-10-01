using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents a read-only observable list that is obtained by transforming all the items in a source collection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <remarks>
    ///     Use this class instead of <see cref="TransformingReadOnlyObservableList{TSource,TTarget}"/>
    ///     if the transforming function can produce different results for the same item at different times.
    /// </remarks>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TTarget}" />
    /// <seealso cref="Neuronic.CollectionModel.WeakEventPattern.IWeakEventListener" />
    public class DynamicTransformingReadOnlyObservableList<TSource, TTarget> : IReadOnlyObservableList<TTarget>,
        IWeakEventListener
    {
        private readonly Action<TTarget> _onRemove;
        private readonly Action<TTarget, TTarget> _onChange;
        private readonly IEqualityComparer<Container> _sourceComparer;
        private readonly Func<TSource, IObservable<TTarget>> _selector;
        private readonly IReadOnlyObservableCollection<TSource> _source;
        private readonly ObservableCollection<Container> _items;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransformingReadOnlyObservableList{TSource, TTarget}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">The transformation to apply to the items in <paramref name="source" />.</param>
        /// <param name="onRemove">The callback to execute when an item is removed from the collection.</param>
        /// <param name="onChange">The callback to execute when a value changes in the collection.</param>
        /// <param name="sourceComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        public DynamicTransformingReadOnlyObservableList(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, IObservable<TTarget>> selector, Action<TTarget> onRemove = null, 
            Action<TTarget, TTarget> onChange = null, IEqualityComparer<TSource> sourceComparer = null)
        {
            _source = source;
            _selector = selector;
            _onRemove = onRemove;
            _onChange = onChange;
            _sourceComparer = new ContainerEqualityComparer<TSource, Container>(sourceComparer);

            _items = new ContainerCollection(source.Select(CreateContainer));
            _items.CollectionChanged += ItemsOnCollectionChanged;
            (_items as INotifyPropertyChanged).PropertyChanged += ItemsOnPropertyChanged;

            CollectionChangedEventManager.AddListener(source, this);
        }

        public int Count => _items.Count;

        public TTarget this[int index] => _items[index].Value;

        public IEnumerator<TTarget> GetEnumerator()
        {
            return _items.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<TTarget>(e.NewItems?.Count ?? 0);
            if (e.NewItems != null && e.NewItems.Count > 0)
                newItems.AddRange(e.NewItems.OfType<Container>().Select(x => x.Value));

            var oldItems = new List<TTarget>(e.OldItems?.Count ?? 0);
            if (e.OldItems != null && e.OldItems.Count > 0)
                oldItems.AddRange(e.OldItems.OfType<Container>().Select(x => x.Value));

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

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _items.UpdateCollection(_source, e, o => CreateContainer((TSource) o), RemoveContainer, _sourceComparer);
        }

        private void RemoveContainer(Container container)
        {
            container.ValueChanged -= ContainerOnValueChanged;
            container.Dispose();
            _onRemove?.Invoke(container.Value);
        }

        private Container CreateContainer(TSource source)
        {
            var container = new Container(source, _selector(source));
            container.ValueChanged += ContainerOnValueChanged;
            return container;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        private void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TTarget> e)
        {
            var container = (Container) sender;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue, container.Index));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            _onChange?.Invoke(e.OldValue, e.NewValue);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        class ContainerCollection : ObservableCollection<Container>
        {
            public ContainerCollection()
            {
            }

            public ContainerCollection(IEnumerable<Container> collection) : base(collection)
            {
                for (int i = 0; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void InsertItem(int index, Container item)
            {
                base.InsertItem(index, item);
                item.Index = index;
                for (int i = index + 1; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                base.MoveItem(oldIndex, newIndex);
                for (int i = Math.Min(oldIndex, newIndex); i < Math.Max(oldIndex, newIndex); i++)
                    Items[i].Index = i;
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                for (int i = index; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void SetItem(int index, Container item)
            {
                base.SetItem(index, item);
                item.Index = index;
            }
        }

        class Container : ObservableItemContainer<TSource, TTarget>
        {
            public Container(TSource item, IObservable<TTarget> observable, IEqualityComparer<TTarget> valueComparer = null) : base(item, observable, valueComparer)
            {
            }

            public int Index { get; set; }
        }
    }
}