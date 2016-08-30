using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents a read-only collection that is generated chaining several read-only sub-collections.
    /// </summary>
    /// <remarks>
    /// This class is lighter than <see cref="CompositeReadOnlyObservableListSource{T}"/>, so use it 
    /// rather than the former when you don't require a list as view.
    /// </remarks>
    /// <typeparam name="T">Type of the view items.</typeparam>
    public class CompositeReadOnlyObservableCollectionSource<T> : CompositeReadOnlyObservableCollectionSourceBase<T>
    {
        private readonly ViewCollection _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSource{T}"/> class.
        /// </summary>
        public CompositeReadOnlyObservableCollectionSource()
        {
            _view = new ViewCollection(this, Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        public CompositeReadOnlyObservableCollectionSource(List<CollectionContainer<T>> list) : base(list)
        {
            _view = new ViewCollection(this, Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public CompositeReadOnlyObservableCollectionSource(IEnumerable<CollectionContainer<T>> collection) : base(collection)
        {
            _view = new ViewCollection(this, Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Gets the read-only composite collection. 
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public IReadOnlyObservableCollection<T> View => _view;

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnViewChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        protected override void InsertItem(int index, CollectionContainer<T> item)
        {
            base.InsertItem(index, item);
            OnViewChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item.Collection.ToList(), item.Offset));
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var vIndex = Items[index].Offset;
            var items = Items[index].Collection.ToList();
            base.RemoveItem(index);
            OnViewChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, vIndex));
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            var vIndex = Items[oldIndex].Offset;
            var items = Items[oldIndex].Collection.ToList();
            base.MoveItem(oldIndex, newIndex);
            OnViewChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, Items[newIndex].Offset, vIndex));
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, CollectionContainer<T> item)
        {
            var items = Items[index].Collection.ToList();
            base.SetItem(index, item);
            OnViewChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item.Collection.ToList(), items, Items[index].Offset));
        }

        /// <summary>
        /// Handles changes in the composite view.
        /// </summary>
        /// <param name="newArgs">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnViewChanged(NotifyCollectionChangedEventArgs newArgs)
        {
            _view.Count += (newArgs.NewItems?.Count ?? 0) - (newArgs.OldItems?.Count ?? 0);
            _view.OnCollectionChanged(newArgs);
        }

        class ViewCollection : IReadOnlyObservableCollection<T>
        {
            private readonly CompositeReadOnlyObservableCollectionSource<T> _parent;
            private readonly IEnumerable<T> _items; 
            private int _count;

            public ViewCollection(CompositeReadOnlyObservableCollectionSource<T> parent, IEnumerable<T> items)
            {
                _parent = parent;
                _items = items;
                _count = _items.Count();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count
            {
                get { return _count; }
                set
                {
                    if (_count == value)
                        return;
                    _count = value;
                    OnPropertyChanged();
                }
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}