using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents a read-only observable list that contains the transformed items and their indexes.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <remarks>
    ///     Use this class instead of <see cref="TransformingReadOnlyObservableList{TSource,TTarget}"/>
    ///     if the transforming function can produce different results for the same item at different times.
    /// </remarks>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TTarget}" />
    /// <seealso cref="Neuronic.CollectionModel.WeakEventPattern.IWeakEventListener" />
    public class IndexedTransformingReadOnlyObservableList<TSource, TTarget> :
        IndexedTransformingReadOnlyObservableListBase<TSource, TTarget>,
        IReadOnlyObservableList<IndexedItemContainer<TSource, TTarget>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedTransformingReadOnlyObservableList{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="selector">The transformation to apply to the items in <paramref name="source" />.</param>
        /// <param name="onRemove">The callback to execute when an item is removed from the collection.</param>
        /// <param name="onChange">The callback to execute when a value changes in the collection.</param>
        /// <param name="sourceComparer">A comparer for the list items. This is only used if the source collection is not a list
        /// and does not provide index information in its <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> events.</param>
        public IndexedTransformingReadOnlyObservableList(IEnumerable<TSource> source, Func<TSource, IObservable<TTarget>> selector, Action<TTarget> onRemove = null, Action<TTarget, TTarget> onChange = null, IEqualityComparer<TSource> sourceComparer = null) : base(source, selector, onRemove, onChange, sourceComparer)
        {
        }

        /// <summary>
        /// Called when a property of <see cref="P:Neuronic.CollectionModel.Collections.IndexedTransformingReadOnlyObservableListBase`2.Items" /> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Called when the content of <see cref="P:Neuronic.CollectionModel.Collections.IndexedTransformingReadOnlyObservableListBase`2.Items" /> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => Items.Count;

        /// <summary>
        /// Gets the container at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The container</returns>
        public IndexedItemContainer<TSource, TTarget> this[int index] => Items[index];

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerator<IndexedItemContainer<TSource, TTarget>> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}