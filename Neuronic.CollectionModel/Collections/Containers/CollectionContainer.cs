using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Neuronic.CollectionModel.Extras;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Collection wrapper for use with <see cref="CompositeReadOnlyObservableCollectionSourceBase{T}"/> and its derived classes.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class CollectionContainer<T> : EventSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionContainer{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public CollectionContainer(IReadOnlyObservableCollection<T> collection) : this ((IReadOnlyCollection<T>) collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionContainer{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public CollectionContainer(IReadOnlyCollection<T> collection) : base (collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionContainer{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public CollectionContainer(IEnumerable<T> collection) : this(
            collection as IReadOnlyCollection<T> ?? new CollectionWrapper<T>(collection))
        {
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <value>
        /// The collection.
        /// </value>
        public virtual IReadOnlyCollection<T> Collection { get; }

        internal int Index { get; set; } = -1;

        internal int Offset { get; set; } = -1;
    }

    class MutableCollectionContainer<T> : CollectionContainer<T>, IObserver<IEnumerable<T>>, IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly SwitchableCollectionSource<T> _switcher;

        private MutableCollectionContainer(SwitchableCollectionSource<T> switcher, IObservable<IEnumerable<T>> collection) : base (switcher)
        {
            _switcher = switcher;
            _subscription = collection.Subscribe(this);
        }

        public MutableCollectionContainer(IObservable<IEnumerable<T>> collection) : this (new SwitchableCollectionSource<T>(), collection)
        {

        }

        public void OnNext(IEnumerable<T> value)
        {
            _switcher.Source = value as IReadOnlyCollection<T> ?? new CollectionWrapper<T>(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}