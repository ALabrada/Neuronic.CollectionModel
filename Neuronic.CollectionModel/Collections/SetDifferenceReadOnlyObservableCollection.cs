using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that is the set difference of two other observable collections.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
    public class SetDifferenceReadOnlyObservableCollection<T> : CustomReadOnlyObservableCollection<T>
    {
        private readonly ObservableDictionary<T, RefCountItemContainer> _containers;
        private readonly IEnumerable<T> _originalSource;
        private readonly IEnumerable<T> _substractedSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableCollection(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource, IEqualityComparer<T> comparer = null) : this(
            originalSource,
            substractedSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableDictionary<T, RefCountItemContainer>(
                new Dictionary<T, RefCountItemContainer>(comparer ?? EqualityComparer<T>.Default)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableCollection(IReadOnlyObservableCollection<T> originalSource,
            IReadOnlyObservableCollection<T> substractedSource, IEqualityComparer<T> comparer = null) : this(
            originalSource,
            substractedSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableDictionary<T, RefCountItemContainer>(
                new Dictionary<T, RefCountItemContainer>(comparer ?? EqualityComparer<T>.Default)))
        {
        }

        private SetDifferenceReadOnlyObservableCollection(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource,
            IEqualityComparer<T> comparer,
            ObservableDictionary<T, RefCountItemContainer> containers) : base(
            new FilteredReadOnlyObservableList<RefCountItemContainer>(containers.Values,
                    container => container.OriginalCount > 0 && container.SubstractedCount == 0, 
                    new ContainerEqualityComparer<T, RefCountItemContainer>(comparer),
                    nameof(RefCountItemContainer.OriginalCount), nameof(RefCountItemContainer.SubstractedCount))
                .ListSelect(container => container.Item))
        {
            _originalSource = originalSource;
            _containers = containers;
            _substractedSource = substractedSource;
            AddRange(_containers, _originalSource, RefCountItemContainer.CreateOriginal);
            AddRange(_containers, _substractedSource, RefCountItemContainer.CreateSubstracted);

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
            var source = _originalSource.Select(RefCountItemContainer.CreateOriginal).Concat(_substractedSource.Select(RefCountItemContainer.CreateSubstracted));
            if (ReferenceEquals(_originalSource, sender))
                HandleCollectionChange(_containers, source,
                    RefCountItemContainer.CreateOriginal, (NotifyCollectionChangedEventArgs) e);
            else if (ReferenceEquals(_substractedSource, sender))
                HandleCollectionChange(_containers, source,
                    RefCountItemContainer.CreateSubstracted, (NotifyCollectionChangedEventArgs) e);
            else
                return base.OnReceiveWeakEvent(managerType, sender, e);
            return true;
        }

        private static void HandleCollectionChange(IDictionary<T, RefCountItemContainer> containers,
            IEnumerable<RefCountItemContainer> source, Func<T, RefCountItemContainer> containerFactory, NotifyCollectionChangedEventArgs e)
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
                        if (RemoveItem(containers, containerFactory((T) oldEnum.Current)))
                            AddItem(containers, containerFactory((T) newEnum.Current));
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

        private static bool RemoveItem(IDictionary<T, RefCountItemContainer> containers, RefCountItemContainer container)
        {
            RefCountItemContainer other;
            if (!containers.TryGetValue(container.Item, out other))
                return false;
            other.Decrement(container);
            if (other.OriginalCount == 0 && other.SubstractedCount == 0)
                containers.Remove(other.Item);
            return true;
        }

        private static void AddRange(IDictionary<T, RefCountItemContainer> containers, IEnumerable newItems,
            Func<T, RefCountItemContainer> containerFactory)
        {
            foreach (T item in newItems)
                AddItem(containers, containerFactory(item));
        }

        private static void AddItem(IDictionary<T, RefCountItemContainer> containers, RefCountItemContainer container)
        {
            RefCountItemContainer other;
            if (containers.TryGetValue(container.Item, out other))
                containers[container.Item].Increment(container);
            else
                containers[container.Item] = container;
        }

        /// <summary>
        /// Container that contains meta-data for the collection elements.
        /// </summary>
        /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
        protected class RefCountItemContainer : ItemContainer<T>, INotifyPropertyChanged
        {
            private int _originalCount;
            private int _substractedCount;

            private RefCountItemContainer(T item, int originalCount, int substractedCount) : base(item)
            {
                _originalCount = originalCount;
                _substractedCount = substractedCount;
            }

            /// <summary>
            /// Creates a container for an item from the original source.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns>The container of <paramref name="item"/>.</returns>
            public static RefCountItemContainer CreateOriginal(T item) => new RefCountItemContainer(item, 1, 0);

            /// <summary>
            /// Creates a container for an item from the substracted source.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns>The container of <paramref name="item"/>.</returns>
            public static RefCountItemContainer CreateSubstracted(T item) => new RefCountItemContainer(item, 0, 1);

            /// <summary>
            /// Gets or sets the number of times the item appears in the original source.
            /// </summary>
            /// <value>
            /// The number of time the item appears in the original source.
            /// </value>
            public int OriginalCount
            {
                get { return _originalCount; }
                set { Set(nameof(OriginalCount), ref _originalCount, value); }
            }

            /// <summary>
            /// Gets or sets the number of times the item appears in the substracted source.
            /// </summary>
            /// <value>
            /// The number of times the item appears in the substracted source.
            /// </value>
            public int SubstractedCount
            {
                get { return _substractedCount; }
                set { Set(nameof(SubstractedCount), ref _substractedCount, value); }
            }

            private bool Set<T>(string propertyName, ref T field, T value)
            {
                if (Equals(field, value))
                    return false;
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                return true;
            }

            /// <summary>
            /// Increments the counters using information from another container.
            /// </summary>
            /// <param name="other">The other.</param>
            public void Increment(RefCountItemContainer other)
            {
                OriginalCount += other.OriginalCount;
                SubstractedCount += other.SubstractedCount;
            }

            /// <summary>
            /// Decrements the counters using information from another container.
            /// </summary>
            /// <param name="other">The other.</param>
            public void Decrement(RefCountItemContainer other)
            {
                OriginalCount -= other.OriginalCount;
                SubstractedCount -= other.SubstractedCount;
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

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
}