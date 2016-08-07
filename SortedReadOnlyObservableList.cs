using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents a read-only observable list that has the same elements as its source, but in a different, predefined order.
    /// </summary>
    /// <typeparam name="T">The type of the collection's elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    public class SortedReadOnlyObservableList<T> : IReadOnlyObservableList<T>
    {
        private readonly ContainerCollection _items;
        private readonly IReadOnlyObservableCollection<T> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortedReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="comparison">The comparison used to establish the order of elements.</param>
        /// <param name="triggers">The name of <typeparamref name="T"/>'s properties that can alter the collection's order.</param>
        public SortedReadOnlyObservableList(IReadOnlyObservableCollection<T> source, Comparison<T> comparison,
            params string[] triggers)
        {
            _source = source;
            _items = new ContainerCollection(from item in source select new Container(item),
                new ContainerComparer(comparison), triggers);
            _items.SortedCollectionChanged += (sender, args) => RaiseEvents(args);
            CollectionChangedEventManager.AddHandler(_source, SourceOnCollectionChanged);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
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

        /// <summary>
        /// Gets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index] => _items.GetSortedItem(index);

        /// <summary>
        /// Creates a <see cref="SortedReadOnlyObservableList{T}"/> from the same collection used by CollectionViewSource.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="descriptions">The descriptions of the sorting function.</param>
        /// <returns>Sorted collection.</returns>
        public static SortedReadOnlyObservableList<T> FromDescriptions(IReadOnlyObservableCollection<T> source,
            SortDescriptionCollection descriptions)
        {
            var properties =
                descriptions.Select(
                    d =>
                    {
                        var propertyInfo = typeof (T).GetProperty(d.PropertyName);
                        return
                            new
                            {
                                d.Direction,
                                Comparer =
                                    (IComparer)
                                        typeof (Comparer<>).MakeGenericType(propertyInfo.PropertyType)
                                            .GetProperty("Default")
                                            .GetGetMethod()
                                            .Invoke(null, null),
                                Method = new Func<T, object>(x => propertyInfo.GetGetMethod().Invoke(x, null))
                            };
                    });
            var comparison = new Comparison<T>((x, y) => (from property in properties
                let xValue = property.Method(x)
                let yValue = property.Method(y)
                let comp = property.Comparer.Compare(xValue, yValue)
                where comp != 0
                select property.Direction == ListSortDirection.Ascending ? comp : -comp).FirstOrDefault());
            var triggers = descriptions.Select(d => d.PropertyName).ToArray();
            return new SortedReadOnlyObservableList<T>(source, comparison, triggers);
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _items.UpdateCollection(_source, e, o => new Container((T) o));
        }

        private void RaiseEvents(NotifyCollectionChangedEventArgs newArgs)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(newArgs);
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
            private readonly string[] _triggers;

            public ContainerCollection(IEnumerable<Container> collection, ContainerComparer comparer, string[] triggers)
                : base(collection)
            {
                _comparer = comparer;
                _triggers = triggers;
                _sortedItems = new List<Container>(Count);
                _sortedItems.AddRange(Items);
                _sortedItems.Sort();
            }

            public IEnumerable<T> Sorted()
            {
                return _sortedItems.Select(item => item.Item);
            }

            public T GetSortedItem(int index)
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

                item.AttachTriggers(_triggers);
                item.TriggerPropertyChanged += ItemOnTriggerPropertyChanged;

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
                item.DetachTriggers(_triggers);
                item.TriggerPropertyChanged -= ItemOnTriggerPropertyChanged;

                var oldSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                Debug.Assert(oldSortedIndex >= 0);
                _sortedItems.RemoveAt(oldSortedIndex);

                base.RemoveItem(index);
                UpdateItems(index, Count);
                item.SourceIndex = -1;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item.Item,
                    oldSortedIndex));
            }

            protected override void SetItem(int index, Container newItem)
            {
                var oldItem = Items[index];
                oldItem.DetachTriggers(_triggers);
                oldItem.TriggerPropertyChanged -= ItemOnTriggerPropertyChanged;

                var oldSortedIndex = _sortedItems.BinarySearch(oldItem, _comparer);
                Debug.Assert(oldSortedIndex >= 0);
                _sortedItems.RemoveAt(oldSortedIndex);

                base.SetItem(index, oldItem);
                oldItem.SourceIndex = -1;

                var newSortedIndex = _sortedItems.BinarySearch(newItem, _comparer);
                Debug.Assert(newSortedIndex < 0);
                newSortedIndex = ~newSortedIndex;
                _sortedItems.Insert(newSortedIndex, newItem);

                newItem.AttachTriggers(_triggers);
                newItem.TriggerPropertyChanged += ItemOnTriggerPropertyChanged;

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    oldItem.Item, oldSortedIndex));
                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    newItem.Item, newSortedIndex));
            }

            protected override void ClearItems()
            {
                foreach (var item in Items)
                {
                    item.DetachTriggers(_triggers);
                    item.TriggerPropertyChanged -= ItemOnTriggerPropertyChanged;
                    item.SourceIndex = -1;
                }

                base.ClearItems();
                _sortedItems.Clear();

                OnSortedCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            private void ItemOnTriggerPropertyChanged(object sender, EventArgs eventArgs)
            {
                var item = (Container) sender;
                if (_sortedItems.Remove(item))
                {
                    var newSortedIndex = _sortedItems.BinarySearch(item, _comparer);
                    Debug.Assert(newSortedIndex < 0);
                    newSortedIndex = ~newSortedIndex;
                    _sortedItems.Insert(newSortedIndex, item);
                }
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

        private class Container : ItemContainer<T>
        {
            public Container(T item) : base(item)
            {
            }

            public int SourceIndex { get; set; }
            public event EventHandler TriggerPropertyChanged;

            protected override void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
            {
                TriggerPropertyChanged?.Invoke(this, args);
            }
        }

        private class ContainerComparer : IComparer<Container>
        {
            private readonly Comparison<T> _comparison;

            public ContainerComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(Container x, Container y)
            {
                var cmp = _comparison(x.Item, y.Item);
                return cmp == 0 ? x.SourceIndex.CompareTo(y.SourceIndex) : cmp;
            }
        }
    }
}