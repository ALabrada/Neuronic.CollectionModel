using System;
using System.Collections.Specialized;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// A simple observable query that re-evaluated the query every time the source changes.
    /// </summary>
    /// <typeparam name="TSource">The type of the source collection.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="QueryObservableResult{TSource,TResult}" />
    /// <remarks>
    /// <para>
    /// This class is useful for O(1) queries (i.e. FirstOrDefault) and queries that
    /// would require to always be re-evaluated when the source collection changes. 
    /// </para>
    /// </remarks>
    public class SimpleQueryObservableResult<TSource, TResult> : QueryObservableResult<TSource, TResult>
    {
        private readonly Func<IReadOnlyObservableCollection<TSource>, TResult> _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQueryObservableResult{TSource, TResult}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="query">The query.</param>
        public SimpleQueryObservableResult(IReadOnlyObservableCollection<TSource> source, Func<IReadOnlyObservableCollection<TSource>, TResult> query) : base(source)
        {
            _query = query;
            CurrentValue = _query(source);
        }

        /// <summary>
        /// Called when the source collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CurrentValue = _query((IReadOnlyObservableCollection<TSource>) sender);
        }
    }
}