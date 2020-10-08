using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Represents a read-only observable list that is obtained by transforming all the items in a source collection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <remarks>
    ///     Use this class instead of <see cref="TransformingReadOnlyObservableList{TSource,TTarget}"/>
    ///     if the transforming function can produce different results for the same item at different times.
    /// </remarks>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TTarget}" />
    public class DynamicTransformingReadOnlyObservableList<TSource, TTarget> 
        : IndexedTransformingReadOnlyObservableListBase<TSource, TTarget>, IReadOnlyObservableList<TTarget>
    {
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
        public DynamicTransformingReadOnlyObservableList(IEnumerable<TSource> source,
            Func<TSource, IObservable<TTarget>> selector, Action<TTarget> onRemove = null, 
            Action<TTarget, TTarget> onChange = null, IEqualityComparer<TSource> sourceComparer = null) : base(source, selector, onRemove, onChange, sourceComparer)
        {
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => Items.Count;

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public TTarget this[int index] => Items[index].Value;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TTarget> GetEnumerator()
        {
            return Items.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when a property of <see cref="P:Neuronic.CollectionModel.Collections.IndexedTransformingReadOnlyObservableListBase`2.Items" /> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        /// <summary>
        /// Called when the content of <see cref="P:Neuronic.CollectionModel.Collections.IndexedTransformingReadOnlyObservableListBase`2.Items" /> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<TTarget>(e.NewItems?.Count ?? 0);
            if (e.NewItems != null && e.NewItems.Count > 0)
                newItems.AddRange(e.NewItems.OfType<IndexedItemContainer<TSource, TTarget>>().Select(x => x.Value));

            var oldItems = new List<TTarget>(e.OldItems?.Count ?? 0);
            if (e.OldItems != null && e.OldItems.Count > 0)
                oldItems.AddRange(e.OldItems.OfType<IndexedItemContainer<TSource, TTarget>>().Select(x => x.Value));

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItems, e.NewStartingIndex, e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset when newItems.Count > 0 && e.NewStartingIndex >= 0:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newItems, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Handles a change in the observable content of an item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:Neuronic.CollectionModel.Collections.Containers.ValueChangedEventArgs`1" /> instance containing the event data.</param>
        protected override void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TTarget> e)
        {
            var container = (IndexedItemContainer<TSource, TTarget>) sender;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue, container.Index));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            base.ContainerOnValueChanged(sender, e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}