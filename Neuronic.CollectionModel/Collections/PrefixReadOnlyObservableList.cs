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
    public class PrefixReadOnlyObservableList<T>: IndexedReadOnlyObservableListBase<T, bool>, 
        IReadOnlyObservableList<T>, IWeakEventListener
    {
        private readonly IReadOnlyList<T> _source;
        private readonly PrefixList _items;

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

        public PrefixReadOnlyObservableList(IReadOnlyList<T> source, Expression<Func<T, bool>> selector,
            Action<bool> onRemove = null, Action<bool, bool> onChange = null)
            : this(source, PropertyObservableFactory<T,bool>.FindIn(selector), onRemove, onChange)
        {
        }

        public int Count => Items.Count - 1;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public T this[int index] => Items[index].Item;

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Take(Count).Select(x => x.Item).GetEnumerator();
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
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            if (e.NewValue)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, container.Item, container.Index));
                _items.Grow();
            }
            else
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, container.Item, container.Index));
                _items.ReduceUpTo(container.Index);
            }
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
                Grow();
            }

            protected IReadOnlyList<T> Source => _owner._source;
            protected ObservableCollection<IndexedItemContainer<T, bool>> Containers => _owner.Items;

            public void Grow()
            {
                while (Containers.Count < Source.Count && 
                       (Containers.Count == 0 || Containers[Containers.Count - 1].Value))
                {
                    var container = _owner.CreateContainer(Source[Containers.Count]);
                    Containers.Add(container);
                }
            }

            public void ReduceUpTo(int index)
            {
                for (int i = Containers.Count - 1; i > index; i--)
                {
                    _owner.RemoveContainer(Containers[i]);
                    Containers.RemoveAt(i);
                }
            }

            public void Add(IndexedItemContainer<T, bool> item)
            {
                if (Containers.Count == 0 || Containers[Containers.Count - 1].Value)
                    Containers.Add(item);
            }

            public void Clear()
            {
                Containers.Clear();
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
                Containers.Remove(item);
                return true;
            }

            public void RemoveAt(int index)
            {
                if (index < Containers.Count)
                    Containers.RemoveAt(index);
            }

            public void Insert(int index, IndexedItemContainer<T, bool> item)
            {
                if (index > Containers.Count)
                    return;
                Containers.Insert(index, item);
                if (!item.Value)
                    ReduceUpTo(index);
            }

            public int Count => Source.Count;

            public bool IsReadOnly => false;

            public int IndexOf(IndexedItemContainer<T, bool> item)
            {
                return Source.IndexOf(item.Item);
            }

            public IndexedItemContainer<T, bool> this[int index]
            {
                get => index < Containers.Count ? Containers[index] : _owner.CreateContainer(Source[index]);
                set
                {
                    Containers[index] = value;
                    if (!value.Value)
                        ReduceUpTo(index);
                }
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