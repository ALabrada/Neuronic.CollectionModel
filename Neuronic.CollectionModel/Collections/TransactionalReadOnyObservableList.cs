﻿using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Utility class that allows to momentarily withhold change notification in the underlying lists.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="TransactionalReadOnlyObservableCollection{T}" />
    /// <seealso cref="Neuronic.CollectionModel.ITransactionalReadOnlyObservableList{T}" />
    public class TransactionalReadOnyObservableList<T> : TransactionalReadOnlyObservableCollection<T>,
        ITransactionalReadOnlyObservableList<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionalReadOnyObservableList{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public TransactionalReadOnyObservableList(IReadOnlyObservableList<T> source) : base(source)
        {
        }

        /// <summary>
        ///     Gets the source list.
        /// </summary>
        /// <value>
        ///     The source list.
        /// </value>
        public new IReadOnlyObservableList<T> Source => (IReadOnlyObservableList<T>) base.Source;

        /// <inheritdoc />
        public T this[int index] => Source[index];

        /// <summary>
        ///     Handles <see cref="E:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.PropertyChanged" /> events from
        ///     <see cref="P:Neuronic.CollectionModel.Collections.ManualReadOnlyObservableCollection`1.Source" />.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void HandlePropertyEvents(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Item[]":
                    OnPropertyChanged(e);
                    break;
                default:
                    base.HandlePropertyEvents(e);
                    break;
            }
        }
    }
}