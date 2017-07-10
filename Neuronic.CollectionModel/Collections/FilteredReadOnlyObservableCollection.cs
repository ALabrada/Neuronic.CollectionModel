using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Filtered read-only collection.
    /// </summary>
    /// <remarks>
    ///     This class is faster and lighter than <see cref="FilteredReadOnlyObservableList{T}" /> but does not provide
    ///     index information in the <see cref="INotifyCollectionChanged.CollectionChanged" /> event, so use it carefully.
    ///     Other than that, both classes provide the same functionalities.
    /// </remarks>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class FilteredReadOnlyObservableCollection<T> : FilteredReadOnlyObservableCollectionBase<T, FilterItemContainer<T>>
    {
        private int _count;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="triggers">
        ///     The names of the item's properties that can cause <paramref name="filter" /> to change its
        ///     value.
        /// </param>
        public FilteredReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, Predicate<T> filter,
            params string[] triggers) : base(source, filter, triggers)
        {
            Items.CollectionChanged += ItemsOnCollectionChanged;
            _count = Items.Count(c => c.IsIncluded);
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public override int Count => _count;

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs newArgs = null;
            FilterItemContainer<T> newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (FilterItemContainer<T>) e.NewItems[0];
                    if (newContainer.IsIncluded)
                    {
                        SetCount(Count + 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            newContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (FilterItemContainer<T>) e.OldItems[0];
                    if (oldContainer.IsIncluded)
                    {
                        SetCount(Count - 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (FilterItemContainer<T>) e.NewItems[0];
                    oldContainer = (FilterItemContainer<T>) e.OldItems[0];
                    if (newContainer.IsIncluded && oldContainer.IsIncluded)
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            newContainer.Item, oldContainer.Item);
                    else if (newContainer.IsIncluded)
                    {
                        SetCount(Count + 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            newContainer.Item);
                    }
                    else if (oldContainer.IsIncluded)
                    {
                        SetCount(Count - 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SetCount(Items.Count(c => c.IsIncluded));
                    newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
            }
            if (newArgs != null)
                OnCollectionChanged(newArgs);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
        {
            return (from container in Items where container.IsIncluded select container.Item).GetEnumerator();
        }

        private void SetCount(int value)
        {
            if (_count == value) return;
            _count = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>
        ///     Creates a container for an item that is included in the source collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     Container for <paramref name="item" />.
        /// </returns>
        protected override FilterItemContainer<T> CreateContainer(T item)
        {
            var container = new FilterItemContainer<T>(item, Filter);
            container.IsIncludedChanged += ContainerOnIsIncludedChanged;
            container.AttachTriggers(Triggers);
            return container;
        }

        /// <summary>
        ///     Destroys a container when it's item is removed from the source collection.
        /// </summary>
        /// <param name="container">The container.</param>
        protected override void DestroyContainer(FilterItemContainer<T> container)
        {
            container.DetachTriggers(Triggers);
            container.IsIncludedChanged -= ContainerOnIsIncludedChanged;
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs e)
        {
            var container = (FilterItemContainer<T>) sender;
            NotifyCollectionChangedAction action;
            if (container.IsIncluded)
            {
                action = NotifyCollectionChangedAction.Add;
                SetCount(Count + 1);
            }
            else
            {
                action = NotifyCollectionChangedAction.Remove;
                SetCount(Count - 1);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, container.Item));
        }
    }
}