using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that contains the same elements as it's source, but with no elements repeated.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public class DistinctReadOnlyObservableCollection<T> : CustomReadOnlyObservableCollection<T>
    {
        private readonly ObservableDictionary<T, int> _containers;
        private readonly IReadOnlyObservableCollection<T> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="comparer">The equality comparer.</param>
        public DistinctReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source,
            IEqualityComparer<T> comparer = null) : this(source,
            new ObservableDictionary<T, int>(new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default)))
        {
        }

        private DistinctReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, ObservableDictionary<T, int> containers) : base (containers.Keys)
        {
            _source = source;
            _containers = containers;
            AddRange(_containers, _source);

            CollectionChangedEventManager.AddListener(_source, this);
        }

        /// <summary>
        /// Called when a change notification is received from the source.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <returns>
        /// <c>true</c> if the event was handled; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return base.OnReceiveWeakEvent(managerType, sender, e);
            HandleCollectionChange(_containers, _source, (NotifyCollectionChangedEventArgs) e);
            return true;
        }

        private static void HandleCollectionChange(IDictionary<T, int> containers, IEnumerable<T> source,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddRange(containers, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                        RemoveItem(containers, item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newEnum = e.NewItems.GetEnumerator();
                    var oldEnum = e.OldItems.GetEnumerator();
                    while (newEnum.MoveNext() && oldEnum.MoveNext())
                        if (RemoveItem(containers, (T) oldEnum.Current))
                            AddItem(containers, (T) newEnum.Current);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    containers.Clear();
                    AddRange(containers, source);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool RemoveItem(IDictionary<T, int> containers, T item)
        {
            int count;
            if (!containers.TryGetValue(item, out count))
                return false;
            count--;
            if (count == 0)
                containers.Remove(item);
            else
                containers[item] = count;
            return true;
        }

        private static void AddRange(IDictionary<T, int> containers, IEnumerable newItems)
        {
            foreach (T item in newItems)
                AddItem(containers, item);
        }

        private static void AddItem(IDictionary<T, int> containers, T item)
        {
            int count;
            if (containers.TryGetValue(item, out count))
                containers[item] = count + 1;
            else
                containers[item] = 1;
        }
    }
}