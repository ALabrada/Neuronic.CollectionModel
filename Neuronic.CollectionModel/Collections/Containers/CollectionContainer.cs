using System;
using System.Collections.Generic;
using System.Collections.Specialized;

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
        /// Gets the collection.
        /// </summary>
        /// <value>
        /// The collection.
        /// </value>
        public IReadOnlyCollection<T> Collection { get; }

        internal int Index { get; set; } = -1;

        internal int Offset { get; set; } = -1;
    }
}