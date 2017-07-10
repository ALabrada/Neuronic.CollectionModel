using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents a read-only observable list that is obtained by transforming all the items in a source collection.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
    /// <typeparam name="TTarget">The type of the elements in the collection.</typeparam>
    /// <seealso cref="ReadOnlyObservableList{T}" />
    public class TransformingReadOnlyObservableList<TSource, TTarget> : ReadOnlyObservableList<TTarget>
    {
        private readonly Action<TTarget> _onRemove;
        private readonly Func<TSource, TTarget> _selector;
        private readonly IReadOnlyObservableCollection<TSource> _source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransformingReadOnlyObservableList{TSource, TTarget}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">The transformation to apply to the items in <paramref name="source" />.</param>
        /// <param name="onRemove">The callback to execute when an item is removed from the collection.</param>
        public TransformingReadOnlyObservableList(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, TTarget> selector, Action<TTarget> onRemove = null)
            : base(new ObservableCollection<TTarget>(source.Select(selector)))
        {
            _source = source;
            _selector = selector;
            _onRemove = onRemove;
            CollectionChangedEventManager.AddHandler(source, SourceOnCollectionChanged);
        }

        private ObservableCollection<TTarget> ObservableItems => (ObservableCollection<TTarget>) Items;

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableItems.UpdateCollection(_source, e, o => _selector((TSource) o), _onRemove);
        }
    }
}