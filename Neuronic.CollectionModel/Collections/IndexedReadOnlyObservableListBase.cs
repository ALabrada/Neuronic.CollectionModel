using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Abstraction of a collection that uses indexed containers.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TTarget">The type of the values in the containers.</typeparam>
    public abstract class IndexedReadOnlyObservableListBase<TSource, TTarget>
    {
        private readonly Func<TSource, IObservable<TTarget>> _selector;
        private readonly Action<TTarget> _onRemove;
        private readonly Action<TTarget, TTarget> _onChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedReadOnlyObservableListBase{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="source">The source elements of the containers.</param>
        /// <param name="selector">The function that obtains the values of the containers..</param>
        /// <param name="onRemove">The function that is called when removing an item.</param>
        /// <param name="onChange">The function that is called when the result assigned to a pair changes.</param>
        protected IndexedReadOnlyObservableListBase(
            IEnumerable<TSource> source, Func<TSource, IObservable<TTarget>> selector, 
            Action<TTarget> onRemove = null, Action<TTarget, TTarget> onChange = null)
        {
            _selector = selector;
            _onRemove = onRemove;
            _onChange = onChange;

            Items = source == null
                ? new ContainerCollection() 
                : new ContainerCollection(source.Select(CreateContainer));
            Items.CollectionChanged += ItemsOnCollectionChanged;
            (Items as INotifyPropertyChanged).PropertyChanged += ItemsOnPropertyChanged;
        }

        /// <summary>
        /// Gets the item containers.
        /// </summary>
        protected ObservableCollection<IndexedItemContainer<TSource, TTarget>> Items { get; }

        /// <summary>
        /// Called when a property of <see cref="IndexedTransformingReadOnlyObservableListBase{TSource,TTarget}.Items"/> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void ItemsOnPropertyChanged(object sender, PropertyChangedEventArgs e);

        /// <summary>
        /// Called when the content of <see cref="IndexedTransformingReadOnlyObservableListBase{TSource,TTarget}.Items"/> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e);

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
            return CreateContainer(source, _selector(source));
        }

        /// <summary>
        /// Creates a container with the specified content.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        /// <returns>The container.</returns>
        protected virtual IndexedItemContainer<TSource, TTarget> CreateContainer(TSource source, IObservable<TTarget> value)
        {
            var container = new IndexedItemContainer<TSource, TTarget>(source, value);
            container.ValueChanged += ContainerOnValueChanged;
            return container;
        }

        /// <summary>
        /// Handles a change in the observable content of an item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ValueChangedEventArgs{T}"/> instance containing the event data.</param>
        protected virtual void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TTarget> e)
        {
            _onChange?.Invoke(e.OldValue, e.NewValue);
        }

        protected class ContainerCollection : ObservableCollection<IndexedItemContainer<TSource, TTarget>>
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