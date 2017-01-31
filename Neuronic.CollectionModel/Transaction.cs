using System;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Utility class that can be used to begin and end a transaction in
    /// a <see cref="ITransactionalReadOnlyObservableCollection{T}"/> collection
    /// in the boundaries of a <c>using</c> block.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="System.IDisposable" />
    public class Transaction<T> : IDisposable
    {
        private readonly ITransactionalReadOnlyObservableCollection<T> _collection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction{T}"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public Transaction(ITransactionalReadOnlyObservableCollection<T> collection)
        {
            _collection = collection;
            _collection.BeginTransaction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _collection.EndTransaction();
        }
    }
}