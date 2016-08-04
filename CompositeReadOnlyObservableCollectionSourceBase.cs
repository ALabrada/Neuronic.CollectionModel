using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Base class for composite read-only collection sources.
    /// </summary>
    /// <typeparam name="T">Type of the view items.</typeparam>
    public abstract class CompositeReadOnlyObservableCollectionSourceBase<T> : ObservableCollection<CollectionContainer<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSourceBase{T}"/> class.
        /// </summary>
        protected CompositeReadOnlyObservableCollectionSourceBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSourceBase{T}"/> class.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        protected CompositeReadOnlyObservableCollectionSourceBase(List<CollectionContainer<T>> list) : base(list)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSourceBase{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        protected CompositeReadOnlyObservableCollectionSourceBase(IEnumerable<CollectionContainer<T>> collection) : base(collection)
        {
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.CollectionChanged -= ItemOnCollectionChanged;
                item.Offset = item.Index = -1;
            }

            base.ClearItems();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, CollectionContainer<T> item)
        {
            if (item.Offset >= 0 || item.Index >= 0)
                throw new ArgumentException("The container belongs to another collection", nameof(item));

            base.InsertItem(index, item);
            UpdateRange(index, Count);
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var oldItem = Items[index];
            oldItem.CollectionChanged -= ItemOnCollectionChanged;

            base.RemoveItem(index);
            UpdateRange(index, Count);

            oldItem.Index = oldItem.Offset = -1;
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            UpdateRange(Math.Min(oldIndex, newIndex), Count);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, CollectionContainer<T> item)
        {
            if (item.Offset >= 0 || item.Index >= 0)
                throw new ArgumentException("The container belongs to another collection", nameof(item));

            var oldItem = Items[index];
            oldItem.CollectionChanged -= ItemOnCollectionChanged;

            base.SetItem(index, item);
            UpdateRange(index, Count);

            oldItem.Index = oldItem.Offset = -1;
            item.CollectionChanged += ItemOnCollectionChanged;
        }

        private void UpdateRange(int start, int end)
        {
            var nextOffset = start == 0 ? 0 : Items[start - 1].Offset + Items[start - 1].Collection.Count;
            for (int i = start; i < end; i++)
            {
                nextOffset = (Items[i].Offset = nextOffset) + Items[i].Collection.Count;
                Items[i].Index = i;
            }
        }

        private void ItemOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var container = (CollectionContainer<T>)sender;
            var newStartingIndex = e.NewStartingIndex < 0 ? -1 : e.NewStartingIndex + container.Offset;
            var oldStartingIndex = e.OldStartingIndex < 0 ? -1 : e.OldStartingIndex + container.Offset;
            UpdateRange(container.Index + 1, Count);
            NotifyCollectionChangedEventArgs newArgs;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, newStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, oldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, newStartingIndex, oldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, e.OldStartingIndex);
                    break;
                default:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action);
                    break;
            }
            OnViewChanged(newArgs);
        }

        /// <summary>
        /// When implemented in a derived class, handles changes in the composite view.
        /// </summary>
        /// <param name="newArgs">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void OnViewChanged(NotifyCollectionChangedEventArgs newArgs);
    }
}