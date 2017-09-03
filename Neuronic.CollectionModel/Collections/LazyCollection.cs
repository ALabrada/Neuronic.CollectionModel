using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that is the result of transforming a source collection, but behaves in a lazy way.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.EventSource" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{TResult}" />
    /// <remarks>
    /// <para>
    /// This collection does not store any data cache and does not any processing when the
    /// source collection is updated. It does, however, need to traverse the collection when
    /// <see cref="Count"/> is asked and it's notification support is very limited.
    /// Every time the source collection is modified it will trigger a <see cref="NotifyCollectionChangedAction.Reset"/>
    /// event which will almost always require at least O(n) processing in the clients.
    /// Also, it can only handle notifications from the source collection and not from the
    /// collection items itself.
    /// </para>
    /// <para>
    /// This collection is ideal for collections that are frequently modified, but rarely consulted.
    /// It is also not recommended to use other collection or list operations using this one
    /// as their source, as the result will be counter-productive. You can use LINQ extension methods
    /// to build the transformation query. 
    /// </para>
    /// </remarks>
    public class LazyCollection<TSource, TResult> : EventSource, IReadOnlyObservableCollection<TResult>
    {
        private readonly Func<IEnumerable<TSource>, IEnumerable<TResult>> _query;
        private int? _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyCollection{TSource, TResult}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="query">The transformation query.</param>
        public LazyCollection(IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> query) : base(source, query)
        {
            _query = query;
            Source = source;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        protected IEnumerable<TSource> Source { get; }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TResult> GetEnumerator() => _query(Source).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        /// <value>
        /// The number of elements.
        /// </value>
        /// <remarks>
        /// This operation is O(n) the first time it is asked. Any sub-sequential
        /// access will be O(1) until the source collection is modified. 
        /// </remarks>
        public int Count => _count ?? (_count = Enumerable.Count(this)).Value;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _count = null;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }
    }
}