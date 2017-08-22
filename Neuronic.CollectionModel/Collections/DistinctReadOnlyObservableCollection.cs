using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that contains the same elements as it's source, but with no elements repeated.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public class DistinctReadOnlyObservableCollection<T> : IReadOnlyObservableCollection<T>
    {
        private readonly ObservableDictionary<T, int> _containers;
        private readonly IReadOnlyObservableCollection<T> _keys;
        private readonly IReadOnlyObservableCollection<T> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="comparer">The equality comparer.</param>
        public DistinctReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source,
            IEqualityComparer<T> comparer = null)
        {
            _source = source;
            _containers =
                new ObservableDictionary<T, int>(new Dictionary<T, int>(comparer ?? EqualityComparer<T>.Default));
            AddRange(_containers, _source);
            _keys = _containers.Keys;

            CollectionChangedEventManager.AddHandler(_source,
                (sender, args) => HandleCollectionChange(_containers, _source, args));
            CollectionChangedEventManager.AddHandler(_keys, (sender, args) => OnCollectionChanged(args));
            PropertyChangedEventManager.AddHandler(_keys, (sender, args) => OnPropertyChanged(args), nameof(Count));
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _keys.Count;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// Occurs when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
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