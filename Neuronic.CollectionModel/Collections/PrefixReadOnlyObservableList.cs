using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Observables;
using Neuronic.CollectionModel.WeakEventPattern;
using System.Windows;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents an observable list that contains the first consecutive elements
    ///     from another list that meet a condition. 
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public class PrefixReadOnlyObservableList<T>: IndexedReadOnlyObservableListBase<T, bool>, 
        IReadOnlyObservableList<T>, IWeakEventListener
    {
        private readonly IReadOnlyList<T> _source;
        private readonly PrefixList _items;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrefixReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="selector">The condition.</param>
        /// <param name="onRemove">The function that is called when removing an item.</param>
        /// <param name="onChange">The function that is called when the result assigned to a pair changes.</param>
        public PrefixReadOnlyObservableList(IReadOnlyList<T> source, Func<T, IObservable<bool>> selector, 
            Action<bool> onRemove = null, Action<bool, bool> onChange = null) 
            : base(null, selector, onRemove, onChange)
        {
            _source = source;

            _items = new PrefixList(this);
            if (_source is INotifyCollectionChanged notifier)
                CollectionChangedEventManager.AddListener(notifier, this);
        }

        private PrefixReadOnlyObservableList(IReadOnlyList<T> source, PropertyObservableFactory<T, bool> selector,
            Action<bool> onRemove = null, Action<bool, bool> onChange = null)
            : this(source, selector.Observe, onRemove, onChange)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrefixReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source list.</param>
        /// <param name="selector">The condition.</param>
        /// <param name="onRemove">The function that is called when removing an item.</param>
        /// <param name="onChange">The function that is called when the result assigned to a pair changes.</param>
        public PrefixReadOnlyObservableList(IReadOnlyList<T> source, Expression<Func<T, bool>> selector,
            Action<bool> onRemove = null, Action<bool, bool> onChange = null)
            : this(source, PropertyObservableFactory<T,bool>.FindIn(selector), onRemove, onChange)
        {
        }

        public int Count => Items.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public T this[int index] => Items[index].Item;

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Select(x => x.Item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void RemoveContainer(IndexedItemContainer<T, bool> container)
        {
            if (container != null)
                base.RemoveContainer(container);
        }

        protected override void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        protected override void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<T>(e.NewItems?.Count ?? 0);
            if (e.NewItems != null && e.NewItems.Count > 0)
                newItems.AddRange(e.NewItems.OfType<IndexedItemContainer<T, bool>>().Select(x => x.Item));

            var oldItems = new List<T>(e.OldItems?.Count ?? 0);
            if (e.OldItems != null && e.OldItems.Count > 0)
                oldItems.AddRange(e.OldItems.OfType<IndexedItemContainer<T, bool>>().Select(x => x.Item));

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

        protected override void ContainerOnValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            base.ContainerOnValueChanged(sender, e);
            var container = (IndexedItemContainer<T, bool>)sender;
            if (container.Index >= Items.Count || !Equals(Items[container.Index], container)) return;
            if (e.NewValue)
                _items.Grow();
            else
                _items.ReduceUpTo(container);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;
            if (ReferenceEquals(_source, sender))
            {
                _items.UpdateCollection(_source, (NotifyCollectionChangedEventArgs) e, 
                    o => CreateContainer((T) o), RemoveContainer);
                _items.Grow();
            }
            return true;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        class PrefixList : IList<IndexedItemContainer<T, bool>>
        {
            private readonly PrefixReadOnlyObservableList<T> _owner;

            public PrefixList(PrefixReadOnlyObservableList<T> owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                Count = Source.Count;
                Grow();
            }

            private IReadOnlyList<T> Source => _owner._source;
            private ObservableCollection<IndexedItemContainer<T, bool>> Containers => _owner.Items;
            private IndexedItemContainer<T, bool> Border { get; set; }

            public void Grow()
            {
                while (Containers.Count < Source.Count && Border is null)
                {
                    var container = _owner.CreateContainer(Source[Containers.Count]);
                    container.Index = Containers.Count;
                    if (container.Value)
                        Containers.Add(container);
                    else
                        Border = container;
                }
            }

            private void ReduceUpTo(int index)
            {
                for (int i = Containers.Count - 1; i >= index; i--)
                {
                    _owner.RemoveContainer(Containers[i]);
                    Containers.RemoveAt(i);
                }
            }

            public void ReduceUpTo(IndexedItemContainer<T, bool> item)
            {
                _owner.RemoveContainer(Border);
                Border = item;
                ReduceUpTo(item.Index);
            }

            public void Add(IndexedItemContainer<T, bool> item)
            {
                item.Index = Containers.Count;
                if (Border is null)
                {
                    if (item.Value)
                        Containers.Add(item);
                    else
                        Border = item;
                }

                Count++;
            }

            public void Clear()
            {
                Containers.Clear();
                Border = null;
                Count = 0;
            }

            public bool Contains(IndexedItemContainer<T, bool> item)
            {
                return Source.Contains(item.Item);
            }

            public void CopyTo(IndexedItemContainer<T, bool>[] array, int arrayIndex)
            {
                Containers.CopyTo(array, arrayIndex);
            }

            public bool Remove(IndexedItemContainer<T, bool> item)
            {
                Count--;
                if (Containers.Remove(item))
                {
                    if (Border != null)
                        Border.Index--;
                }
                else if (Equals(Border, item))
                    Border = null;
                return true;
            }

            public void RemoveAt(int index)
            {
                Count--;
                if (index < Containers.Count)
                {
                    Containers.RemoveAt(index);
                    if (Border != null)
                        Border.Index--;
                }
                else if (index == Containers.Count)
                    Border = null;
            }

            public void Insert(int index, IndexedItemContainer<T, bool> item)
            {
                item.Index = index;
                if (index <= Containers.Count)
                {
                    if (Border != null)
                        Border.Index--;
                    if (item.Value)
                        Containers.Insert(index, item);
                    else
                        ReduceUpTo(item);
                }

                Count++;
            }

            public IndexedItemContainer<T, bool> this[int index]
            {
                get => index < Containers.Count ? Containers[index] : null;
                set
                {
                    value.Index = index;
                    if (index > Containers.Count) return;
                    if (value.Value)
                    {
                        if (index < Containers.Count)
                            Containers[index] = value;
                        else
                        {
                            Containers.Add(value);
                            _owner.RemoveContainer(Border);
                            Border = null;
                        }
                    }
                    else
                        ReduceUpTo(value);
                }
            }

            public int Count { get; private set; }

            public bool IsReadOnly => false;

            public int IndexOf(IndexedItemContainer<T, bool> item)
            {
                return Source.IndexOf(item.Item);
            }

            public IEnumerator<IndexedItemContainer<T, bool>> GetEnumerator()
            {
                return Containers.Concat(Source.Skip(Containers.Count).Select(_owner.CreateContainer)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Containers).GetEnumerator();
            }
        }
    }
}