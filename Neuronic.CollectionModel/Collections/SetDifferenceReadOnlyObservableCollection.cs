using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly ObservableDictionary<T, RefCountItemContainer<T>> _containers;
        private readonly ObservableSet<T> _items;
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
            new ObservableSet<T>(new HashSet<T>(comparer)))
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
            new ObservableSet<T>(new HashSet<T>(comparer)))
        {
        }

        private SetDifferenceReadOnlyObservableCollection(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource,
            IEqualityComparer<T> comparer,
            ObservableSet<T> items) : base(items)
        {
            _originalSource = originalSource;
            _items = items;
            _containers = new ObservableDictionary<T, RefCountItemContainer<T>>(
                new Dictionary<T, RefCountItemContainer<T>>(comparer ?? EqualityComparer<T>.Default));
            _substractedSource = substractedSource;
            AddRange(_substractedSource, RefCountItemContainer<T>.CreateSubstracted);
            AddRange(_originalSource, RefCountItemContainer<T>.CreateOriginal);

            var originalNotify = _originalSource as INotifyCollectionChanged;
            if (originalNotify != null)
                CollectionChangedEventManager.AddListener(originalNotify, this);
            var substractedNotify = _substractedSource as INotifyCollectionChanged;
            if (substractedNotify != null)
                CollectionChangedEventManager.AddListener(substractedNotify, this);
        }

        private static bool ShouldIncludeContainer(RefCountItemContainer<T> container)
        {
            return container.OriginalCount > 0 && container.SubstractedCount == 0;
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
            var source = _originalSource.Select(RefCountItemContainer<T>.CreateOriginal).Concat(_substractedSource.Select(RefCountItemContainer<T>.CreateSubstracted));
            if (ReferenceEquals(_originalSource, sender))
                HandleCollectionChange(source,
                    RefCountItemContainer<T>.CreateOriginal, (NotifyCollectionChangedEventArgs) e);
            else if (ReferenceEquals(_substractedSource, sender))
                HandleCollectionChange(source,
                    RefCountItemContainer<T>.CreateSubstracted, (NotifyCollectionChangedEventArgs) e);
            else
                return base.OnReceiveWeakEvent(managerType, sender, e);
            return true;
        }

        private void HandleCollectionChange(IEnumerable<RefCountItemContainer<T>> source,
            Func<T, RefCountItemContainer<T>> containerFactory, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddRange(e.NewItems, containerFactory);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                        RemoveItem(containerFactory(item));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newEnum = e.NewItems.GetEnumerator();
                    var oldEnum = e.OldItems.GetEnumerator();
                    while (newEnum.MoveNext() && oldEnum.MoveNext())
                        if (RemoveItem(containerFactory((T) oldEnum.Current)))
                            AddItem(containerFactory((T) newEnum.Current));
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _containers.Clear();
                    _items.Clear();
                    foreach (var container in source)
                        AddItem(container);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool RemoveItem(RefCountItemContainer<T> container)
        {
            RefCountItemContainer<T> other;
            if (!_containers.TryGetValue(container.Item, out other))
                return false;
            var wasIncluded = ShouldIncludeContainer(other);
            other.Decrement(container);
            var isIncluded = ShouldIncludeContainer(other);
            if (other.OriginalCount == 0 && other.SubstractedCount == 0)
                _containers.Remove(other.Item);
            if (wasIncluded && !isIncluded)
                _items.Remove(other.Item);
            else if (!wasIncluded && isIncluded)
                _items.Add(other.Item);
            return true;
        }

        private void AddRange(IEnumerable newItems, Func<T, RefCountItemContainer<T>> containerFactory)
        {
            foreach (T item in newItems)
                AddItem(containerFactory(item));
        }

        private void AddItem(RefCountItemContainer<T> container)
        {
            RefCountItemContainer<T> other;
            if (_containers.TryGetValue(container.Item, out other))
            {
                var wasIncluded = ShouldIncludeContainer(other);
                other.Increment(container);
                var isIncluded = ShouldIncludeContainer(other);
                if (wasIncluded && !isIncluded)
                    _items.Remove(other.Item);
                else if (!wasIncluded && isIncluded)
                    _items.Add(other.Item);
            }
            else
            {
                _containers[container.Item] = container;
                if (ShouldIncludeContainer(container))
                    _items.Add(container.Item);
            }
        }
    }
}