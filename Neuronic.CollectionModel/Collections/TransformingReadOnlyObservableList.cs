using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents a read-only observable list that is obtained by transforming all the items in a source collection.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
    /// <typeparam name="TTarget">The type of the elements in the collection.</typeparam>
    /// <seealso cref="ReadOnlyObservableList{T}" />
    public class TransformingReadOnlyObservableList<TSource, TTarget> : ReadOnlyObservableList<TTarget>, IWeakEventListener
    {
        private readonly Action<TTarget> _onRemove;
        private readonly IEqualityComparer<TTarget> _sourceComparer;
        private readonly Func<TSource, TTarget> _selector;
        private readonly IReadOnlyObservableCollection<TSource> _source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransformingReadOnlyObservableList{TSource, TTarget}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">The transformation to apply to the items in <paramref name="source" />.</param>
        /// <param name="onRemove">The callback to execute when an item is removed from the collection.</param>
        /// <param name="targetComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        public TransformingReadOnlyObservableList(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, TTarget> selector, Action<TTarget> onRemove = null, IEqualityComparer<TTarget> targetComparer = null)
            : base(new ObservableCollection<TTarget>(source.Select(selector)))
        {
            _source = source;
            _selector = selector;
            _onRemove = onRemove;
            _sourceComparer = targetComparer ?? EqualityComparer<TTarget>.Default;
            CollectionChangedEventManager.AddListener(source, this);
        }

        private ObservableCollection<TTarget> ObservableItems => (ObservableCollection<TTarget>) Items;

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableItems.UpdateCollection(_source, e, o => _selector((TSource) o), _onRemove, _sourceComparer);
        }
    }
}