using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Base class for observable collections that represent the result of a binary operation between two sets.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
    public abstract class SetOperationReadOnlyObservableCollection<T> : CustomReadOnlyObservableCollection<T>
    {
        private readonly ObservableDictionary<T, RefCountItemContainer<T>> _containers;
        private readonly ObservableSet<T> _items;
        private readonly IEnumerable<T> _firstSource;
        private readonly IEnumerable<T> _secondSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetOperationReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="firstSource">The first source.</param>
        /// <param name="secondSource">The second source.</param>
        /// <param name="comparer">The equality comparer.</param>
        /// <param name="items">The set that contains the operation's result.</param>
        protected SetOperationReadOnlyObservableCollection(IEnumerable<T> firstSource,
            IEnumerable<T> secondSource,
            IEqualityComparer<T> comparer,
            ObservableSet<T> items) : base(items)
        {
            _firstSource = firstSource;
            _items = items;
            _containers = new ObservableDictionary<T, RefCountItemContainer<T>>(
                new Dictionary<T, RefCountItemContainer<T>>(comparer ?? EqualityComparer<T>.Default));
            _secondSource = secondSource;
            AddRange(_secondSource, RefCountItemContainer<T>.CreateFromSecond);
            AddRange(_firstSource, RefCountItemContainer<T>.CreateFromFirst);

            var originalNotify = _firstSource as INotifyCollectionChanged;
            if (originalNotify != null)
                CollectionChangedEventManager.AddListener(originalNotify, this);
            var substractedNotify = _secondSource as INotifyCollectionChanged;
            if (substractedNotify != null)
                CollectionChangedEventManager.AddListener(substractedNotify, this);
        }

        /// <summary>
        /// When implemented in a derived class, determines if the specified container's item should be included in the result.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns><c>true</c> if <paramref name="container"/> belongs in the collection; otherwise, false.</returns>
        protected abstract bool ShouldIncludeContainer(RefCountItemContainer<T> container);

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
            var source = _firstSource.Select(RefCountItemContainer<T>.CreateFromFirst).Concat(_secondSource.Select(RefCountItemContainer<T>.CreateFromSecond));
            if (ReferenceEquals(_firstSource, sender))
                HandleCollectionChange(source,
                    RefCountItemContainer<T>.CreateFromFirst, (NotifyCollectionChangedEventArgs)e);
            else if (ReferenceEquals(_secondSource, sender))
                HandleCollectionChange(source,
                    RefCountItemContainer<T>.CreateFromSecond, (NotifyCollectionChangedEventArgs)e);
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
                        if (RemoveItem(containerFactory((T)oldEnum.Current)))
                            AddItem(containerFactory((T)newEnum.Current));
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
            if (other.CountOnFirst == 0 && other.CountOnSecond == 0)
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