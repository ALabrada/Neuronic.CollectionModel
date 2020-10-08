using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Base class for dynamic transforming collections.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TTarget">The type of the transformed items.</typeparam>
    public abstract class IndexedTransformingReadOnlyObservableListBase<TSource, TTarget> : IndexedReadOnlyObservableListBase<TSource, TTarget>, IWeakEventListener
    {
        private readonly IEqualityComparer<IndexedItemContainer<TSource, TTarget>> _sourceComparer;
        private readonly IEnumerable<TSource> _source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransformingReadOnlyObservableList{TSource, TTarget}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">The transformation to apply to the items in <paramref name="source" />.</param>
        /// <param name="onRemove">The callback to execute when an item is removed from the collection.</param>
        /// <param name="onChange">The callback to execute when a value changes in the collection.</param>
        /// <param name="sourceComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        public IndexedTransformingReadOnlyObservableListBase(IEnumerable<TSource> source,
            Func<TSource, IObservable<TTarget>> selector, Action<TTarget> onRemove = null, 
            Action<TTarget, TTarget> onChange = null, IEqualityComparer<TSource> sourceComparer = null)
            : base (source, selector, onRemove, onChange)
        {
            _source = source;
            _sourceComparer = new ContainerEqualityComparer<TSource, IndexedItemContainer<TSource, TTarget>>(sourceComparer);

            if (_source is INotifyCollectionChanged notifier)
                CollectionChangedEventManager.AddListener(notifier, this);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Items.UpdateCollection(_source, e, o => CreateContainer((TSource) o), RemoveContainer, _sourceComparer);
        }
    }
}