using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Abstraction of a class that allows to momentarily withhold change notification in the underlying collections.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IManualReadOnlyObservableCollection{T}" />
    public interface ITransactionalReadOnlyObservableCollection<out T> : IManualReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is dirty.
        /// </summary>
        /// <remarks>
        ///     The instance is dirty if the underlying collection was modified while a
        ///     transaction was active. If an instance is dirty when the transaction ends,
        ///     a <see cref="NotifyCollectionChangedAction.Reset" /> notification is sent,
        ///     refreshing all the collections that depend on this instance.
        /// </remarks>
        /// <value>
        ///     <c>true</c> if this instance is dirty; otherwise, <c>false</c>.
        /// </value>
        bool IsDirty { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is in a transaction.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is in a transaction; otherwise, <c>false</c>.
        /// </value>
        bool IsInTransaction { get; }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        ///     Ends the transaction.
        /// </summary>
        /// <remarks>
        ///     If an instance is <see cref="TransactionalReadOnlyObservableCollection{T}.IsDirty">dirty</see> when the transaction
        ///     ends,
        ///     a <see cref="NotifyCollectionChangedAction.Reset" /> notification is sent,
        ///     refreshing all the collections that depend on this instance.
        /// </remarks>
        void EndTransaction();
    }
}