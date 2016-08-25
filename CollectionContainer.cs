using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Collection wrapper for use with <see cref="CompositeReadOnlyObservableCollectionSourceBase{T}"/> and its derived classes.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class CollectionContainer<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionContainer{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public CollectionContainer(IReadOnlyObservableCollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Collection = collection;
            CollectionChangedEventManager.AddHandler(collection, (sender, args) => OnCollectionChanged(args));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionContainer{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public CollectionContainer(IReadOnlyCollection<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Collection = collection;
            var notify = collection as INotifyCollectionChanged;
            if (notify != null)
                CollectionChangedEventManager.AddHandler(notify, (sender, args) => OnCollectionChanged(args));
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

        /// <summary>
        /// Occurs when the container's collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}