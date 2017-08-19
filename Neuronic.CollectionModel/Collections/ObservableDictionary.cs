using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// A dictionary that notifies changes.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
        IReadOnlyObservableCollection<KeyValuePair<TKey, TValue>>
    {
        private readonly WeakReference<DictionaryKeyCollection> _keys = new WeakReference<DictionaryKeyCollection>(null);
        private readonly WeakReference<DictionaryValueCollection> _values = new WeakReference<DictionaryValueCollection>(null);

        /// <summary>
        /// Gets the item comparer.
        /// </summary>
        /// <value>
        /// The comparer that can be used to compare the items for equality..
        /// </value>
        public IEqualityComparer<KeyValuePair<TKey, TValue>> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ObservableDictionary(Dictionary<TKey, TValue> source) : this (source, source.Comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="keyComparer">The </param>
        public ObservableDictionary(IDictionary<TKey, TValue> source, IEqualityComparer<TKey> keyComparer)
        {
            Comparer = new PairComparer(keyComparer);
            Items = source;
        }

        /// <summary>
        /// Gets the internal items.
        /// </summary>
        /// <value>
        /// The internal items dictionary.
        /// </value>
        protected IDictionary<TKey, TValue> Items { get; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            OnCountChanged();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            if (Items.Count == 0)
                return;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnCountChanged();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item) => Items.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey key) => Items.ContainsKey(key);

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public bool Remove(TKey key)
        {
            TValue value;
            if (!Items.TryGetValue(key, out value) || !Items.Remove(key))
                return false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                new KeyValuePair<TKey, TValue>(key, value)));
            OnCountChanged();
            return true;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value) => Items.TryGetValue(key, out value);

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get { return Items[key]; }
            set
            {
                TValue oldValue;
                var newItem = new KeyValuePair<TKey, TValue>(key, value);
                if (Items.TryGetValue(key, out oldValue))
                {
                    var oldItem = new KeyValuePair<TKey, TValue>(key, oldValue);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        newItem, oldItem));
                }
                else
                {
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem));
                    OnCountChanged();
                }
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Items.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Items.Values;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IReadOnlyObservableCollection<TKey> Keys
        {
            get
            {
                DictionaryKeyCollection keys;
                if (_keys.TryGetTarget(out keys))
                    return keys;
                keys = new DictionaryKeyCollection(this);
                _keys.SetTarget(keys);
                return keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IReadOnlyObservableCollection<TValue> Values
        {
            get
            {
                DictionaryValueCollection values;
                if (_values.TryGetTarget(out values))
                    return values;
                values = new DictionaryValueCollection(this);
                _values.SetTarget(values);
                return values;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => Items.Count;

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
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event when <see cref="Count"/> changes.
        /// </summary>
        protected void OnCountChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        class PairComparer : IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            private readonly IEqualityComparer<TKey> _keyComparer;

            public PairComparer(IEqualityComparer<TKey> keyComparer)
            {
                _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            }

            public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return _keyComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return _keyComparer.GetHashCode(obj.Key);
            }
        }

        abstract class DictionaryCollectionBase : INotifyPropertyChanged, INotifyCollectionChanged
        {
            protected ObservableDictionary<TKey, TValue> Owner { get; }

            protected DictionaryCollectionBase(ObservableDictionary<TKey, TValue> owner)
            {
                Owner = owner;
                CollectionChangedEventManager.AddHandler(Owner, OwnerOnCollectionChanged);
                PropertyChangedEventManager.AddHandler(Owner, (sender, args) => OnPropertyChanged(args), nameof(Count));
            }

            private void OwnerOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, Project(e.NewItems),
                            Project(e.OldItems)));
                        break;
                    case NotifyCollectionChangedAction.Add:
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, Project(e.NewItems)));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, Project(e.OldItems)));
                        break;
                    default:
                        OnCollectionChanged(e);
                        break;
                }
            }

            protected abstract IList Project(IList pairs);

            public int Count => Owner.Count;

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }

            protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(this, e);
            }
        }

        class DictionaryKeyCollection : DictionaryCollectionBase, IReadOnlyObservableCollection<TKey>
        {
            public DictionaryKeyCollection(ObservableDictionary<TKey, TValue> owner) : base(owner)
            {
            }

            public IEnumerator<TKey> GetEnumerator() => Owner.Items.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            protected override IList Project(IList pairs)
            {
                var list = new List<TKey>(pairs.Count);
                list.AddRange(from pair in pairs.Cast<KeyValuePair<TKey, TValue>>() select pair.Key);
                return list;
            }
        }

        class DictionaryValueCollection : DictionaryCollectionBase, IReadOnlyObservableCollection<TValue>
        {
            public DictionaryValueCollection(ObservableDictionary<TKey, TValue> owner) : base(owner)
            {
            }

            public IEnumerator<TValue> GetEnumerator() => Owner.Items.Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            protected override IList Project(IList pairs)
            {
                var list = new List<TValue>(pairs.Count);
                list.AddRange(from pair in pairs.Cast<KeyValuePair<TKey, TValue>>() select pair.Value);
                return list;
            }
        }
    }
}