using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Neuronic.CollectionModel
{
    public class EditableCollectionSelector<T> : CollectionSelector<T>, IList<T>
    {
        public EditableCollectionSelector(IEnumerable<T> items = null)
            : this(items == null ? new ObservableCollection<T>() : new ObservableCollection<T>(items))
        {
        }

        public EditableCollectionSelector(ObservableCollection<T> items) : base (items)
        {
            ItemsCore = items;
        }

        protected ObservableCollection<T> ItemsCore { get; } 

        public void Add(T item)
        {
            ItemsCore.Add(item);
        }

        public void AddAndSelect(T item)
        {
            ItemsCore.Add(item);
            SelectedIndex = ItemsCore.Count - 1;
        }

        public int IndexOf(T item)
        {
            return ItemsCore.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ItemsCore.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ItemsCore.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return ItemsCore[index]; }
            set { ItemsCore[index] = value; }
        }

        public void InsertAndSelect(int index, T item)
        {
            ItemsCore.Insert(index, item);
            SelectedIndex = index;
        }

        public bool Replace(T prevItem, T newItem)
        {
            var index = ItemsCore.IndexOf(prevItem);
            if (index < 0)
                return false;
            ItemsCore[index] = newItem;
            return true;
        }

        public bool ReplaceAndSelect(T prevItem, T newItem)
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

        public void CopyTo(T[] array, int arrayIndex)
        {
            ItemsCore.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = ItemsCore.IndexOf(item);
            if (index < 0)
                return false;
            ItemsCore.RemoveAt(index);
            return true;
        }

        int ICollection<T>.Count => ItemsCore.Count;
        bool ICollection<T>.IsReadOnly => false;

        public void Clear()
        {
            ItemsCore.Clear();
        }

        public bool Contains(T item)
        {
            return ItemsCore.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ItemsCore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}