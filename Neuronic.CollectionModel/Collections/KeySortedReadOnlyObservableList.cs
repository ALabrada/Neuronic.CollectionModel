using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Observables;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Represents a read-only observable list that has the same elements as its source, but ordered according to some key function.
    /// </summary>
    /// <typeparam name="TSource">The type of the collection's elements.</typeparam>
    /// <typeparam name="TKey">The type of the sorting keys.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TSource}" />
    public class KeySortedReadOnlyObservableList<TSource, TKey> :  
        IReadOnlyObservableList<TSource>, IWeakEventListener
    {
        private readonly ContainerCollection _items;
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, IObservable<TKey>> _keySelector;
        private readonly IEqualityComparer<Container> _eqComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedReadOnlyObservableList{TSource}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">The key generator.</param>
        /// <param name="comparison">The comparison used to establish the order of elements (and keys).</param>
        /// <param name="comparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        public KeySortedReadOnlyObservableList(IEnumerable<TSource> source, Func<TSource, IObservable<TKey>> keySelector, 
            Comparison<TKey> comparison, IEqualityComparer<TSource> comparer)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            _eqComparer = new ContainerEqualityComparer<TSource, Container>(comparer);
            _items = new ContainerCollection(from item in source select new Container(item, keySelector(item)),
                new ContainerComparer(comparison));
            _items.SortedCollectionChanged += (sender, args) => RaiseEvents(args);
            
            if (_source is INotifyCollectionChanged notifier)
                CollectionChangedEventManager.AddListener(notifier, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedReadOnlyObservableList{TSource}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">The key generator.</param>
        /// <param name="comparer">The comparison used to establish the order of elements (and keys).</param>
        public KeySortedReadOnlyObservableList(IEnumerable<TSource> source, Expression<Func<TSource, TKey>> keySelector,
            IComparer<TKey> comparer) : this (source, 
            item => PropertyObservableFactory<TSource, TKey>.CreateFrom(keySelector).Observe(item), 
            comparer == null ? null as Comparison<TKey> : comparer.Compare,
            null)
        {
            
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TSource> GetEnumerator()
        {
            return _items.Sorted().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the amount of elements in the collection.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _items.Count;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when the property values changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public TSource this[int index] => _items.GetSortedItem(index);

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _items.UpdateCollection(_source, e, o => new Container((TSource)o, _keySelector((TSource) o)), comparer: _eqComparer);
        }

        private void RaiseEvents(NotifyCollectionChangedEventArgs newArgs)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(newArgs);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        private sealed class ContainerCollection : ObservableCollection<Container>
        {
            private readonly ContainerComparer _comparer;
            private readonly List<Container> _sortedItems;

            public ContainerCollection(IEnumerable<Container> collection, ContainerComparer comparer)
                : base(collection)
            {
                _comparer = comparer;
                _sortedItems = new List<Container>(Count);
                _sortedItems.AddRange(Items);
                foreach (var item in Items)
                {
                    item.ValueChanged += ItemOnTriggerPropertyChanged;
                }
                UpdateItems(0, Count);
                _sortedItems.Sort(_comparer);
            }

            public IEnumerable<TSource> Sorted()
            {
                return _sortedItems.Select(item => item.Item);
            }

            public TSource GetSortedItem(int index)
            {
                return _sortedItems[index].Item;
            }

            protected override void InsertItem(int index, Container item)
            {
                base.InsertItem(index, item);
                UpdateItems(index, Count);

                var sortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(sortedIndex < 0);
                sortedIndex = ~sortedIndex;
                _sortedItems.Insert(sortedIndex, item);

                item.ValueChanged += ItemOnTriggerPropertyChanged;

                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    item.Item, sortedIndex));
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                var item = Items[oldIndex];
                var oldSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(oldSortedIndex >= 0);
                _sortedItems.RemoveAt(oldSortedIndex);

                base.MoveItem(oldIndex, newIndex);
                UpdateItems(Math.Min(oldIndex, newIndex), Math.Max(oldIndex, newIndex) + 1);

                var newSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(newSortedIndex < 0);
                newSortedIndex = ~newSortedIndex;
                _sortedItems.Insert(newSortedIndex, item);

                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                    item.Item, newSortedIndex, oldSortedIndex));
            }

            protected override void RemoveItem(int index)
            {
                var item = Items[index];
                item.Dispose();
                item.ValueChanged -= ItemOnTriggerPropertyChanged;

                var oldSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(oldSortedIndex >= 0);
                _sortedItems.RemoveAt(oldSortedIndex);

                base.RemoveItem(index);
                UpdateItems(index, Count);
                item.SourceIndex = -1;

                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item.Item,
                    oldSortedIndex));
            }

            protected override void SetItem(int index, Container newItem)
            {
                var oldItem = Items[index];
                oldItem.Dispose();
                oldItem.ValueChanged -= ItemOnTriggerPropertyChanged;

                var oldSortedIndex = _sortedItems.BinarySearch(oldItem, _comparer);
                Debug.Assert(oldSortedIndex >= 0);
                _sortedItems.RemoveAt(oldSortedIndex);

                base.SetItem(index, newItem);
                oldItem.SourceIndex = -1;

                newItem.SourceIndex = index;
                var newSortedIndex = _sortedItems.BinarySearch(newItem, _comparer);
                Debug.Assert(newSortedIndex < 0);
                newSortedIndex = ~newSortedIndex;
                _sortedItems.Insert(newSortedIndex, newItem);

                newItem.ValueChanged += ItemOnTriggerPropertyChanged;

                if (oldSortedIndex == newSortedIndex)
                    OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, newItem.Item, oldItem.Item, oldSortedIndex));
                else
                {
                    OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                        oldItem.Item, oldSortedIndex));
                    OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                        newItem.Item, newSortedIndex));
                }
            }

            protected override void ClearItems()
            {
                foreach (var item in Items)
                {
                    item.Dispose();
                    item.ValueChanged -= ItemOnTriggerPropertyChanged;
                    item.SourceIndex = -1;
                }

                base.ClearItems();
                _sortedItems.Clear();

                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            private void ItemOnTriggerPropertyChanged(object sender, EventArgs eventArgs)
            {
                var item = (Container)sender;
                var prevIndex = _sortedItems.IndexOf(item);
                if (prevIndex < 0)
                    return;
                _sortedItems.RemoveAt(prevIndex);
                var newSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(newSortedIndex < 0);
                newSortedIndex = ~newSortedIndex;
                _sortedItems.Insert(newSortedIndex, item);

                if (prevIndex != newSortedIndex)
                    OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                        item.Item, newSortedIndex, prevIndex));
            }

            private void UpdateItems(int start, int end)
            {
                for (var i = start; i < end; i++)
                    Items[i].SourceIndex = i;
            }

            public event NotifyCollectionChangedEventHandler SortedCollectionChanged;

            private void OnSortedCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                SortedCollectionChanged?.Invoke(this, e);
            }
        }

        [DebuggerDisplay("[{SourceIndex}] {Item}")]
        private class Container : ObservableItemContainer<TSource, TKey>
        {
            public Container(TSource item, IObservable<TKey> observable) : base(item, observable)
            {
            }

            public int SourceIndex { get; set; }

            public override bool Equals(object obj)
            {
                return obj is Container x && x.SourceIndex == SourceIndex;
            }

            public override int GetHashCode()
            {
                return 0;
            }

        }

        private class ContainerComparer : IComparer<Container>
        {
            private readonly Comparison<TKey> _comparison;

            public ContainerComparer(Comparison<TKey> comparison)
            {
                _comparison = comparison ?? Comparer<TKey>.Default.Compare;
            }

            public int Compare(Container x, Container y)
            {
                var cmp = _comparison(x.Value, y.Value);
                return cmp == 0 ? x.SourceIndex.CompareTo(y.SourceIndex) : cmp;
            }
        }
    }
}