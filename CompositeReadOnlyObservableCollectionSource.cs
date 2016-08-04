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
            _view = new ViewCollection(Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="list">The list from which the elements are copied.</param>
        public CompositeReadOnlyObservableCollectionSource(List<CollectionContainer<T>> list) : base(list)
        {
            _view = new ViewCollection(Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeReadOnlyObservableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param>
        public CompositeReadOnlyObservableCollectionSource(IEnumerable<CollectionContainer<T>> collection) : base(collection)
        {
            _view = new ViewCollection(Items.SelectMany(c => c.Collection));
        }

        /// <summary>
        /// Gets the read-only composite collection. 
        /// </summary>
        /// <value>
        /// The view.
        /// </value>
        public IReadOnlyObservableCollection<T> View { get; }

        /// <summary>
        /// Handles changes in the composite view.
        /// </summary>
        /// <param name="newArgs">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnViewChanged(NotifyCollectionChangedEventArgs newArgs)
        {
            _view.Count += newArgs.NewItems.Count - newArgs.OldItems.Count;
            _view.OnCollectionChanged(newArgs);
        }

        class ViewCollection : IReadOnlyObservableCollection<T>
        {
            private readonly IEnumerable<T> _items; 
            private int _count;

            public ViewCollection(IEnumerable<T> items)
            {
                _items = items;
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