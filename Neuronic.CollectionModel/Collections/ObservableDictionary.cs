using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

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
        private readonly WeakReference _keys = new WeakReference(null);
        private readonly WeakReference _values = new WeakReference(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public ObservableDictionary() : this (null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ObservableDictionary(IDictionary<TKey, TValue> source)
        {
            Items = source ?? new Dictionary<TKey, TValue>();
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
            Items.Clear();
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
                    Items[key] = value;
                    var oldItem = new KeyValuePair<TKey, TValue>(key, oldValue);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        newItem, oldItem));
                }
                else
                    Add(key, value);
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => ProtectedKeys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => ProtectedValues;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IReadOnlyObservableCollection<TKey> Keys => ProtectedKeys;

        /// <summary>
        /// Gets the protected keys collection.
        /// </summary>
        /// <value>
        /// The protected keys collection.
        /// </value>
        protected DictionaryKeyCollection ProtectedKeys
        {
            get
            {
                var keys = _keys.Target as DictionaryKeyCollection;
                if (keys != null)
                    return keys;
                _keys.Target = keys = new DictionaryKeyCollection(this);
                return keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public IReadOnlyObservableCollection<TValue> Values => ProtectedValues;

        /// <summary>
        /// Gets the protected values collection.
        /// </summary>
        /// <value>
        /// The protected values collection.
        /// </value>
        protected DictionaryValueCollection ProtectedValues
        {
            get
            {
                var values = _values.Target as DictionaryValueCollection;
                if (values != null)
                    return values;
                _values.Target = values = new DictionaryValueCollection(this);
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

        /// <summary>
        /// Base class for the observable keys & values dictionary collections.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
        /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
        protected abstract class DictionaryCollectionBase<T> : INotifyPropertyChanged, INotifyCollectionChanged, ICollection<T>, IWeakEventListener
        {
            private readonly ICollection<T> _items;
            private ObservableDictionary<TKey, TValue> Owner { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DictionaryCollectionBase{T}"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            /// <param name="items">The items.</param>
            protected DictionaryCollectionBase(ObservableDictionary<TKey, TValue> owner, ICollection<T> items)
            {
                _items = items;
                Owner = owner;
                CollectionChangedEventManager.AddListener(Owner, this);
                PropertyChangedEventManager.AddListener(Owner, this, nameof(Count));
            }

            private void OwnerOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewItems.Count != e.OldItems.Count)
                            throw new InvalidOperationException("Element count mismatch at replace");
                        if (e.NewItems.Count == 1)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action,
                                ProjectItem(e.NewItems[0]), ProjectItem(e.OldItems[0])));
                        else if (e.NewItems.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, ProjectList(e.NewItems),
                                ProjectList(e.OldItems)));
                        break;
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems.Count == 1)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, ProjectItem(e.NewItems[0])));
                        else if (e.NewItems.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, ProjectList(e.NewItems)));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems.Count == 1)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, ProjectItem(e.OldItems[0])));
                        else if (e.OldItems.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(e.Action, ProjectList(e.OldItems)));
                        break;
                    default:
                        OnCollectionChanged(e);
                        break;
                }
            }

            /// <summary>
            /// Projects a list of <see cref="KeyValuePair{TKey,TValue}" /> to a list of <typeparamref name="T" />.
            /// </summary>
            /// <param name="pairs">The list to project.</param>
            /// <returns>
            /// The projected list.
            /// </returns>
            protected virtual IList ProjectList(IList pairs)
            {
                var list = new List<T>(pairs.Count);
                list.AddRange(from object pair in pairs select ProjectItem(pair));
                return list;
            }

            /// <summary>
            /// Projects a <see cref="KeyValuePair{TKey,TValue}" /> into a collection item.
            /// </summary>
            /// <param name="pair">The pair.</param>
            /// <returns>The projected item.</returns>
            protected abstract T ProjectItem(object pair);

            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref="System.NotSupportedException"></exception>
            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <exception cref="System.NotSupportedException"></exception>
            public void Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
            /// </returns>
            public bool Contains(T item) => _items.Contains(item);

            /// <summary>
            /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
            /// </summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
            /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
            public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </returns>
            /// <exception cref="System.NotSupportedException"></exception>
            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            public int Count => Owner.Count;
            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            public bool IsReadOnly => true;

            /// <summary>
            /// Occurs when the collection changes.
            /// </summary>
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            /// <summary>
            /// Occurs when a property value changes.
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

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// An enumerator that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (!ReferenceEquals(sender, Owner))
                    return false;
                if (managerType == typeof(CollectionChangedEventManager))
                    OwnerOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs) e);
                else if (managerType == typeof(PropertyChangedEventManager))
                    OnPropertyChanged((PropertyChangedEventArgs) e);
                else
                    return false;
                return true;
            }
        }

        /// <summary>
        /// An observable collection of dictionary keys.
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
        /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
        protected class DictionaryKeyCollection : DictionaryCollectionBase<TKey>, IReadOnlyObservableCollection<TKey>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DictionaryKeyCollection"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public DictionaryKeyCollection(ObservableDictionary<TKey, TValue> owner) : base(owner, owner.Items.Keys)
            {
            }

            /// <summary>
            /// Projects a <see cref="T:System.Collections.Generic.KeyValuePair`2" /> into a collection item.
            /// </summary>
            /// <param name="pair">The pair.</param>
            /// <returns>
            /// The projected item.
            /// </returns>
            protected override TKey ProjectItem(object pair)
            {
                return ((KeyValuePair<TKey, TValue>) pair).Key;
            }
        }

        /// <summary>
        /// An observable collection of dictionary values.
        /// </summary>
        /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
        /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{System.Collections.Generic.KeyValuePair{TKey, TValue}}" />
        protected class DictionaryValueCollection : DictionaryCollectionBase<TValue>, IReadOnlyObservableCollection<TValue>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DictionaryValueCollection"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            public DictionaryValueCollection(ObservableDictionary<TKey, TValue> owner) : base(owner, owner.Items.Values)
            {
            }

            /// <summary>
            /// Projects a <see cref="T:System.Collections.Generic.KeyValuePair`2" /> into a collection item.
            /// </summary>
            /// <param name="pair">The pair.</param>
            /// <returns>
            /// The projected item.
            /// </returns>
            protected override TValue ProjectItem(object pair)
            {
                return ((KeyValuePair<TKey, TValue>)pair).Value;
            }
        }
    }
}