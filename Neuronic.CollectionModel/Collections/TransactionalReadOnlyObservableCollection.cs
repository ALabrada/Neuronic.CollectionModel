using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Utility class that allows to momentarily withhold change notification in the underlying collections.
    /// </summary>
    /// <remarks>
    ///     This class is useful when you need to perform many operations in the source collection
    ///     and minimize the performance hit.
    /// </remarks>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="ManualReadOnlyObservableCollection{T}" />
    public class TransactionalReadOnlyObservableCollection<T> : ManualReadOnlyObservableCollection<T>,
        ITransactionalReadOnlyObservableCollection<T>, IDisposable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionalReadOnlyObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public TransactionalReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source) : base(source)
        {
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            EndTransaction();
        }

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
        public bool IsDirty { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is in a transaction.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is in a transaction; otherwise, <c>false</c>.
        /// </value>
        public bool IsInTransaction { get; private set; }

        /// <summary>
        ///     Begins the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            IsInTransaction = true;
        }

        /// <summary>
        ///     Ends the transaction.
        /// </summary>
        /// <remarks>
        ///     If an instance is <see cref="IsDirty">dirty</see> when the transaction ends,
        ///     a <see cref="NotifyCollectionChangedAction.Reset" /> notification is sent,
        ///     refreshing all the collections that depend on this instance.
        /// </remarks>
        public void EndTransaction()
        {
            IsInTransaction = false;
            if (IsDirty)
            {
                IsDirty = false;
                Reset();
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.CollectionChanged" /> event
        ///     with the <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />
        ///     action.
        /// </summary>
        public override void Reset()
        {
            IsDirty = false;
            base.Reset();
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="TransactionalReadOnlyObservableCollection{T}" /> class.
        /// </summary>
        ~TransactionalReadOnlyObservableCollection()
        {
            Dispose();
        }

        /// <summary>
        ///     Handles <see cref="E:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.CollectionChanged" /> events
        ///     from <see cref="P:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.Source" />.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance
        ///     containing the event data.
        /// </param>
        protected override void HandleCollectionEvents(NotifyCollectionChangedEventArgs e)
        {
            if (IsInTransaction)
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        IsDirty = false;
                        break;
                    default:
                        IsDirty = true;
                        break;
                }
            else
                base.HandleCollectionEvents(e);
        }

        /// <summary>
        ///     Handles <see cref="E:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.PropertyChanged" /> events from
        ///     <see cref="P:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.Source" />.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void HandlePropertyEvents(PropertyChangedEventArgs e)
        {
            if (!IsInTransaction)
                base.HandlePropertyEvents(e);
        }
    }
}