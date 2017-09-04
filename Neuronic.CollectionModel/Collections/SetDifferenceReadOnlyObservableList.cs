using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable list that is the set difference of two other observable collections.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableList{T}" />
    /// <remarks>
    /// Although this is a list, the order of the elements is not necessarily related
    /// to the order of the source elements. Therefore, you should be using 
    /// <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> instead of this class.
    /// </remarks>
    public class SetDifferenceReadOnlyObservableList<T> : CustomReadOnlyObservableList<T>
    {
        private readonly ObservableDictionary<T, RefCountItemContainer<T>> _containers;
        private readonly IEnumerable<T> _originalSource;
        private readonly IEnumerable<T> _substractedSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableList(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource, IEqualityComparer<T> comparer = null) : this(
            originalSource,
            substractedSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableDictionary<T, RefCountItemContainer<T>>(
                new Dictionary<T, RefCountItemContainer<T>>(comparer ?? EqualityComparer<T>.Default)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableList(IReadOnlyObservableCollection<T> originalSource,
            IReadOnlyObservableCollection<T> substractedSource, IEqualityComparer<T> comparer = null) : this(
            originalSource,
            substractedSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableDictionary<T, RefCountItemContainer<T>>(
                new Dictionary<T, RefCountItemContainer<T>>(comparer ?? EqualityComparer<T>.Default)))
        {
        }

        private SetDifferenceReadOnlyObservableList(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource,
            IEqualityComparer<T> comparer,
            ObservableDictionary<T, RefCountItemContainer<T>> containers) : base(
            new FilteredReadOnlyObservableList<RefCountItemContainer<T>>(containers.Values,
                    container => container.CountOnFirst > 0 && container.CountOnSecond == 0, 
                    new ContainerEqualityComparer<T, RefCountItemContainer<T>>(comparer),
                    nameof(RefCountItemContainer<T>.CountOnFirst), nameof(RefCountItemContainer<T>.CountOnSecond))
                .ListSelect(container => container.Item))
        {
            _originalSource = originalSource;
            _containers = containers;
            _substractedSource = substractedSource;
            AddRange(_containers, _substractedSource, RefCountItemContainer<T>.CreateFromSecond);
            AddRange(_containers, _originalSource, RefCountItemContainer<T>.CreateFromFirst);

            var originalNotify = _originalSource as INotifyCollectionChanged;
            if (originalNotify != null)
                CollectionChangedEventManager.AddListener(originalNotify, this);
            var substractedNotify = _substractedSource as INotifyCollectionChanged;
            if (substractedNotify != null)
                CollectionChangedEventManager.AddListener(substractedNotify, this);
        }

        /// <summary>
        /// Called when a change notification is received from a source.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <returns>
        /// <c>true</c> if the event was handled; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return base.OnReceiveWeakEvent(managerType, sender, e);
            var source = _originalSource.Select(RefCountItemContainer<T>.CreateFromFirst).Concat(_substractedSource.Select(RefCountItemContainer<T>.CreateFromSecond));
            if (ReferenceEquals(_originalSource, sender))
                HandleCollectionChange(_containers, source,
                    RefCountItemContainer<T>.CreateFromFirst, (NotifyCollectionChangedEventArgs)e);
            else if (ReferenceEquals(_substractedSource, sender))
                HandleCollectionChange(_containers, source,
                    RefCountItemContainer<T>.CreateFromSecond, (NotifyCollectionChangedEventArgs)e);
            else
                return base.OnReceiveWeakEvent(managerType, sender, e);
            return true;
        }

        private static void HandleCollectionChange(IDictionary<T, RefCountItemContainer<T>> containers,
            IEnumerable<RefCountItemContainer<T>> source, Func<T, RefCountItemContainer<T>> containerFactory, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddRange(containers, e.NewItems, containerFactory);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                        RemoveItem(containers, containerFactory(item));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newEnum = e.NewItems.GetEnumerator();
                    var oldEnum = e.OldItems.GetEnumerator();
                    while (newEnum.MoveNext() && oldEnum.MoveNext())
                        if (RemoveItem(containers, containerFactory((T)oldEnum.Current)))
                            AddItem(containers, containerFactory((T)newEnum.Current));
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    containers.Clear();
                    foreach (var container in source)
                        AddItem(containers, container);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool RemoveItem(IDictionary<T, RefCountItemContainer<T>> containers, RefCountItemContainer<T> container)
        {
            RefCountItemContainer<T> other;
            if (!containers.TryGetValue(container.Item, out other))
                return false;
            other.Decrement(container);
            if (other.CountOnFirst == 0 && other.CountOnSecond == 0)
                containers.Remove(other.Item);
            return true;
        }

        private static void AddRange(IDictionary<T, RefCountItemContainer<T>> containers, IEnumerable newItems,
            Func<T, RefCountItemContainer<T>> containerFactory)
        {
            foreach (T item in newItems)
                AddItem(containers, containerFactory(item));
        }

        private static void AddItem(IDictionary<T, RefCountItemContainer<T>> containers, RefCountItemContainer<T> container)
        {
            RefCountItemContainer<T> other;
            if (containers.TryGetValue(container.Item, out other))
                other.Increment(container);
            else
                containers[container.Item] = container;
        }
    }
}