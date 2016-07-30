using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Filtered read-only list.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    public class FilteredReadOnlyObservableList<T> :
        FilteredReadOnlyObservableCollectionBase<T, IndexedItemContainer<T>>, IReadOnlyObservableList<T>, IList<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableList{T}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="triggers">
        ///     The names of the item's properties that can cause <paramref name="filter" /> to change its
        ///     value.
        /// </param>
        public FilteredReadOnlyObservableList(IReadOnlyObservableCollection<T> source, Predicate<T> filter,
            params string[] triggers) : base(source, filter, triggers)
        {
            UpdateIndexes(0, Items.Count);
            FilteredItems =
                new ObservableCollection<T>(from container in Items where container.IsIncluded select container.Item);

            Items.CollectionChanged += ItemsOnCollectionChanged;
            FilteredItems.CollectionChanged += (sender, args) => OnCollectionChanged(args);
            ((INotifyPropertyChanged) FilteredItems).PropertyChanged += (sender, args) => OnPropertyChanged(args);
        }

        /// <summary>
        ///     Gets the filtered items.
        /// </summary>
        /// <value>
        ///     The filtered items.
        /// </value>
        protected ObservableCollection<T> FilteredItems { get; }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        ///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return FilteredItems.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        T IList<T>.this[int index]
        {
            get { return this[index]; }

            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return FilteredItems.GetEnumerator();
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public override int Count => FilteredItems.Count;

        /// <summary>
        ///     Gets the <see cref="T" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="T" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index] => FilteredItems[index];

        /// <summary>
        ///     Creates a container for an item that is included in the source collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     Container for <paramref name="item" />.
        /// </returns>
        protected override IndexedItemContainer<T> CreateContainer(T item)
        {
            var container = new IndexedItemContainer<T>(item, Filter);
            container.IsIncludedChanged += ContainerOnIsIncludedChanged;
            container.AttachTriggers(Triggers);
            return container;
        }

        /// <summary>
        ///     Destroys a container when it's item is removed from the source collection.
        /// </summary>
        /// <param name="container">The container.</param>
        protected override void DestroyContainer(IndexedItemContainer<T> container)
        {
            container.DetachTriggers(Triggers);
            container.IsIncludedChanged -= ContainerOnIsIncludedChanged;
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs eventArgs)
        {
            var container = (IndexedItemContainer<T>) sender;
            if (container.IsIncluded)
            {
                UpdateIndexes(container.GlobalIndex, Items.Count);
                FilteredItems.Insert(container.LocalIndex, container.Item);
            }
            else
            {
                var localIndex = container.LocalIndex;
                UpdateIndexes(container.GlobalIndex, Items.Count);
                FilteredItems.RemoveAt(localIndex);
            }
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IndexedItemContainer<T> newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    OnContainerAddedToItems(newContainer, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (IndexedItemContainer<T>) e.OldItems[0];
                    OnContainerRemovedFromItems(oldContainer, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    OnContainerMovedInItems(newContainer, e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    oldContainer = (IndexedItemContainer<T>) e.OldItems[0];
                    Debug.Assert(e.OldStartingIndex == e.NewStartingIndex);
                    OnContainerReplacedInItems(newContainer, oldContainer, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    FilteredItems.Clear();
                    Debug.Assert(Items.Count == 0);
                    break;
            }
        }

        private void OnContainerReplacedInItems(IndexedItemContainer<T> newItem, IndexedItemContainer<T> oldItem,
            int index)
        {
            if (newItem.IsIncluded && !oldItem.IsIncluded)
            {
                UpdateIndexes(index, Items.Count);
                FilteredItems.Insert(newItem.LocalIndex, newItem.Item);
            }
            else if (oldItem.IsIncluded && !newItem.IsIncluded)
            {
                UpdateIndexes(index, Items.Count);
                FilteredItems.RemoveAt(oldItem.LocalIndex);
            }
            else
            {
                newItem.GlobalIndex = oldItem.GlobalIndex;
                newItem.LocalIndex = oldItem.LocalIndex;
                FilteredItems[newItem.LocalIndex] = newItem.Item;
            }
            oldItem.GlobalIndex = int.MinValue;
            oldItem.LocalIndex = int.MinValue;
        }

        private void OnContainerMovedInItems(IndexedItemContainer<T> item, int oldIndex, int newIndex)
        {
            var oldLocalIndex = item.LocalIndex;
            if (oldIndex > newIndex)
                UpdateIndexes(newIndex, oldIndex);
            else
                UpdateIndexes(oldIndex, newIndex + 1);
            var newLocalIndex = item.LocalIndex;

            if (item.IsIncluded)
                FilteredItems.Move(oldLocalIndex, newLocalIndex);
        }

        private void OnContainerRemovedFromItems(IndexedItemContainer<T> item, int index)
        {
            UpdateIndexes(index, Items.Count);
            if (item.IsIncluded)
                FilteredItems.RemoveAt(item.LocalIndex);
            item.LocalIndex = item.GlobalIndex = int.MinValue;
        }

        private void OnContainerAddedToItems(IndexedItemContainer<T> item, int index)
        {
            UpdateIndexes(index, Items.Count);
            if (item.IsIncluded)
                FilteredItems.Insert(item.LocalIndex, item.Item);
        }

        private void UpdateIndexes(int start, int end)
        {
            var lastLocal = start == 0 ? -1 : Items[start - 1].LocalIndex;
            for (var i = start; i < end; i++)
            {
                Items[i].GlobalIndex = i;
                lastLocal = Items[i].LocalIndex = Items[i].IsIncluded ? lastLocal + 1 : lastLocal;
                    // The index it occupies or the one it should.
            }
        }
    }
}