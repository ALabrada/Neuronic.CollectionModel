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
    /// <seealso cref="Neuronic.CollectionModel.WeakEventPattern.IWeakEventListener" />
    public abstract class IndexedTransformingReadOnlyObservableListBase<TSource, TTarget> : IWeakEventListener
    {
        private readonly Action<TTarget> _onRemove;
        private readonly Action<TTarget, TTarget> _onChange;
        private readonly IEqualityComparer<IndexedItemContainer<TSource, TTarget>> _sourceComparer;
        private readonly Func<TSource, IObservable<TTarget>> _selector;
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
        {
            _source = source;
            _selector = selector;
            _onRemove = onRemove;
            _onChange = onChange;
            _sourceComparer = new ContainerEqualityComparer<TSource, IndexedItemContainer<TSource, TTarget>>(sourceComparer);

            Items = new ContainerCollection(source.Select(CreateContainer));
            Items.CollectionChanged += ItemsOnCollectionChanged;
            (Items as INotifyPropertyChanged).PropertyChanged += ItemsOnPropertyChanged;

            if (_source is INotifyCollectionChanged notifier)
                CollectionChangedEventManager.AddListener(notifier, this);
        }

        /// <summary>
        /// Gets the item containers.
        /// </summary>
        protected ObservableCollection<IndexedItemContainer<TSource, TTarget>> Items { get; }

        /// <summary>
        /// Called when a property of <see cref="Items"/> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e);
        /// <summary>
        /// Called when the content of <see cref="Items"/> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

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

        /// <summary>
        /// Disposes the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        protected virtual void RemoveContainer(IndexedItemContainer<TSource, TTarget> container)
        {
            container.ValueChanged -= ContainerOnValueChanged;
            container.Dispose();
            _onRemove?.Invoke(container.Value);
        }

        /// <summary>
        /// Creates a container with the specified content.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The container.</returns>
        protected virtual IndexedItemContainer<TSource, TTarget> CreateContainer(TSource source)
        {
            var container = new IndexedItemContainer<TSource, TTarget>(source, _selector(source));
            container.ValueChanged += ContainerOnValueChanged;
            return container;
        }

        /// <summary>
        /// Handles a change in the observable content of an item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs{TTarget}"/> instance containing the event data.</param>
        protected virtual void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TTarget> e)
        {
            _onChange?.Invoke(e.OldValue, e.NewValue);
        }

        private class ContainerCollection : ObservableCollection<IndexedItemContainer<TSource, TTarget>>
        {
            public ContainerCollection()
            {
            }

            public ContainerCollection(IEnumerable<IndexedItemContainer<TSource, TTarget>> collection) : base(collection)
            {
                for (int i = 0; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void InsertItem(int index, IndexedItemContainer<TSource, TTarget> item)
            {
                base.InsertItem(index, item);
                item.Index = index;
                for (int i = index + 1; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                base.MoveItem(oldIndex, newIndex);
                for (int i = Math.Min(oldIndex, newIndex); i < Math.Max(oldIndex, newIndex); i++)
                    Items[i].Index = i;
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                for (int i = index; i < Count; i++)
                    Items[i].Index = i;
            }

            protected override void SetItem(int index, IndexedItemContainer<TSource, TTarget> item)
            {
                base.SetItem(index, item);
                item.Index = index;
            }
        }
    }
}