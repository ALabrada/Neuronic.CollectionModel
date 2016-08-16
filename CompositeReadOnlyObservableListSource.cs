using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents a read-only list that is generated chaining several read-only sub-collections.
    /// </summary>
    /// <typeparam name="T">Type of the view items.</typeparam>
    public class CompositeReadOnlyObservableListSource<T> : CompositeReadOnlyObservableCollectionSourceBase<T>
    {
        private readonly ObservableCollection<T> _viewItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableListSource{T}"/> class.
        /// </summary>
        public CompositeReadOnlyObservableListSource()
        {
            _viewItems = new ObservableCollection<T>();
            View = new ReadOnlyObservableList<T>(_viewItems);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableListSource{T}"/> class.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        public CompositeReadOnlyObservableListSource(List<CollectionContainer<T>> list) : base(list)
        {
            _viewItems = new ObservableCollection<T>();
            View = new ReadOnlyObservableList<T>(_viewItems);
            InitializeViewItems(Items);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableListSource{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public CompositeReadOnlyObservableListSource(IEnumerable<CollectionContainer<T>> collection) : base(collection)
        {
            _viewItems = new ObservableCollection<T>();
            View = new ReadOnlyObservableList<T>(_viewItems);
            InitializeViewItems(Items);
        }

        /// <summary>
        /// Gets the read-only composite collection. 
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public IReadOnlyObservableList<T> View { get; }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            _viewItems.Clear();
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="System.ArgumentException">The container belongs to another collection</exception>
        protected override void InsertItem(int index, CollectionContainer<T> item)
        {
            base.InsertItem(index, item);

            var viewIndex = item.Offset;
            foreach (var viewItem in item.Collection)
                _viewItems.Insert(viewIndex++, viewItem);
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var movedItem = Items[oldIndex];
            var oldOffset = movedItem.Offset;

            base.MoveItem(oldIndex, newIndex);

            var newOffset = Items[newIndex].Offset;
            if (newOffset < oldOffset)
                for (int i = 0; i < movedItem.Collection.Count; i++)
                    _viewItems.Move(oldOffset++, newOffset++);
            else
                for (int i = 0; i < movedItem.Collection.Count; i++)
                    _viewItems.Move(oldOffset, newOffset + movedItem.Collection.Count - 1);
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var oldItem = Items[index];
            var offset = oldItem.Offset;

            base.RemoveItem(index);

            for (int i = 0; i < oldItem.Collection.Count; i++)
                _viewItems.RemoveAt(offset);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        /// <exception cref="System.ArgumentException">The container belongs to another collection</exception>
        protected override void SetItem(int index, CollectionContainer<T> item)
        {
            var oldItem = Items[index];

            base.SetItem(index, item);

            _viewItems.ReplaceItems(oldItem.Collection, item.Collection, item.Offset);
        }

        private void InitializeViewItems(IEnumerable<CollectionContainer<T>> list)
        {
            var nextIndex = 0;
            var nextOffset = 0;
            foreach (var item in list)
            {
                item.Index = nextIndex++;
                item.Offset = nextOffset;
                nextOffset += item.Collection.Count;
                foreach (var viewItem in item.Collection)
                    _viewItems.Add(viewItem);
            }
        }

        /// <summary>
        /// Handles changes in the composite view.
        /// </summary>
        /// <param name="newArgs">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnViewChanged(NotifyCollectionChangedEventArgs newArgs)
        {
            _viewItems.UpdateCollection(Items.SelectMany(c => c.Collection), newArgs);
        }
    }
}