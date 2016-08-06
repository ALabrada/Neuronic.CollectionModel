using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Editable list selection mechanism.
    /// </summary>
    /// <remarks>
    ///     The selector's collection can be modified through the source collection or the selector's methods.
    /// </remarks>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.CollectionSelector{T}" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    public class EditableCollectionSelector<T> : ICollectionSelector<T>, IList<T>
    {
        private readonly ICollectionSelector<T> _internalSelector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableCollectionSelector{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public EditableCollectionSelector(IEnumerable<T> items = null)
            : this(items == null ? new ObservableCollection<T>() : new ObservableCollection<T>(items))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableCollectionSelector{T}" /> class.
        /// </summary>
        /// <param name="items">The source items.</param>
        public EditableCollectionSelector(ObservableCollection<T> items) : this(items, new CollectionSelector<T>(items))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EditableCollectionSelector{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="internalSelector">The internal selector.</param>
        protected EditableCollectionSelector(ObservableCollection<T> items, ICollectionSelector<T> internalSelector)
        {
            ItemsCore = items;
            _internalSelector = internalSelector;
            _internalSelector.SelectedItemChanged += (sender, args) => OnSelectedItemChanged();
            _internalSelector.SelectedItemChanging += (sender, args) => OnSelectedItemChanging();
            _internalSelector.PropertyChanged += (sender, args) => OnPropertyChanged(args);
        }

        /// <summary>
        ///     Gets the editable source collection.
        /// </summary>
        /// <value>
        ///     The selector's items.
        /// </value>
        protected ObservableCollection<T> ItemsCore { get; }

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets the index of the selected item.
        ///     Changes to this value should rise the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />
        ///     event.
        /// </summary>
        /// <value>
        ///     The index of the selected item.
        /// </value>
        public int SelectedIndex
        {
            get { return _internalSelector.SelectedIndex; }
            set { _internalSelector.SelectedIndex = value; }
        }

        /// <summary>
        ///     Gets the selected item.
        ///     Changes to this value should rise the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" />
        ///     and
        ///     <see cref="E:Neuronic.CollectionModel.ICollectionSelector`1.SelectedItemChanged" /> events.
        /// </summary>
        /// <value>
        ///     The selected item.
        /// </value>
        public T SelectedItem => _internalSelector.SelectedItem;

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The item list.
        /// </value>
        public IReadOnlyObservableList<T> Items => _internalSelector.Items;

        /// <summary>
        /// Occurs before the selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanging;

        /// <summary>
        ///     Occurs when the selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanged;

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public virtual void Add(T item)
        {
            ItemsCore.Add(item);
        }

        /// <summary>
        ///     Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        ///     The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return ItemsCore.IndexOf(item);
        }

        /// <summary>
        ///     Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public virtual void Insert(int index, T item)
        {
            ItemsCore.Insert(index, item);
        }

        /// <summary>
        ///     Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public virtual void RemoveAt(int index)
        {
            ItemsCore.RemoveAt(index);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
        ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public virtual bool Remove(T item)
        {
            var index = ItemsCore.IndexOf(item);
            if (index < 0)
                return false;
            ItemsCore.RemoveAt(index);
            return true;
        }

        /// <summary>
        ///     Gets or sets the <see cref="T" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="T" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at <paramref name="index" />.</returns>
        public T this[int index]
        {
            get { return ItemsCore[index]; }
            set { ItemsCore[index] = value; }
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ItemsCore.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count => ItemsCore.Count;
        bool ICollection<T>.IsReadOnly => false;

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public virtual void Clear()
        {
            ItemsCore.Clear();
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return ItemsCore.Contains(item);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ItemsCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds an item to the collection and selects it.
        /// </summary>
        /// <param name="item">The object to add.</param>
        public virtual void AddAndSelect(T item)
        {
            ItemsCore.Add(item);
            SelectedIndex = ItemsCore.Count - 1;
        }

        /// <summary>
        ///     Inserts an item to the selector's collection at the specified index and selects it.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted. index.</param>
        /// <param name="item">The object to insert.</param>
        public virtual void InsertAndSelect(int index, T item)
        {
            ItemsCore.Insert(index, item);
            SelectedIndex = index;
        }

        /// <summary>
        ///     Removes the selected item from the selector's collection.
        /// </summary>
        public virtual void RemoveSelected()
        {
            if (SelectedIndex >= 0 && SelectedIndex < ItemsCore.Count)
                ItemsCore.RemoveAt(SelectedIndex);
        }

        /// <summary>
        ///     Replaces an item in the selector's collection.
        /// </summary>
        /// <param name="prevItem">The item to replace.</param>
        /// <param name="newItem">The item to replace <paramref name="prevItem" /> with.</param>
        /// <returns>true if the item could be replaced; otherwise, false.</returns>
        public virtual bool Replace(T prevItem, T newItem)
        {
            var index = ItemsCore.IndexOf(prevItem);
            if (index < 0)
                return false;
            ItemsCore[index] = newItem;
            return true;
        }

        /// <summary>
        ///     Replaces an item in the selector's collection and selects the new item.
        /// </summary>
        /// <param name="prevItem">The item to replace.</param>
        /// <param name="newItem">The item to replace <paramref name="prevItem" /> with.</param>
        /// <returns>true if the item could be replaced; otherwise, false.</returns>
        public virtual bool ReplaceAndSelect(T prevItem, T newItem)
        {
            var index = ItemsCore.IndexOf(prevItem);
            if (index < 0)
                return false;
            if (index == SelectedIndex)
                SelectedIndex = -1;
            ItemsCore[index] = newItem;
            SelectedIndex = index;
            return true;
        }

        /// <summary>
        ///     Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="OnSelectedItemChanged" /> event.
        /// </summary>
        protected virtual void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="SelectedItemChanging"/> event.
        /// </summary>
        protected virtual void OnSelectedItemChanging()
        {
            SelectedItemChanging?.Invoke(this, EventArgs.Empty);
        }
    }
}