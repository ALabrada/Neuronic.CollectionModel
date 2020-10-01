using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// Represents the result of an observable query that is updated when the source collection changes
    /// </summary>
    /// <typeparam name="TSource">The type of the source collection items.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="ObservableResult{T}" />
    public abstract class QueryObservableResult<TSource, TResult> : ObservableResult<TResult>, IWeakEventListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryObservableResult{TSource, TResult}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        protected QueryObservableResult(IReadOnlyObservableCollection<TSource> source)
        {
            Source = source;
            CollectionChangedEventManager.AddListener(source, this);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(Source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            try
            {
                OnSourceChanged(sender, (NotifyCollectionChangedEventArgs)e);
            }
            catch (Exception error)
            {
                OnError(new ErrorEventArgs(error));
            }
            return true;
        }

        /// <summary>
        /// Called when the source collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e);

        /// <summary>
        /// Gets the source collection.
        /// </summary>
        /// <value>
        /// The source collection.
        /// </value>
        protected IReadOnlyObservableCollection<TSource> Source { get; }
    }
}