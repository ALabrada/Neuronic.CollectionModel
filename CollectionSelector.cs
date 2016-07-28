using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel
{
    public class CollectionSelector<T> : ICollectionSelector<T>
    {
        private int _selectedIndex = -1;
        private T _selectedItem;

        public CollectionSelector(IReadOnlyObservableList<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Items = items;
            CollectionChangedEventManager.AddHandler(Items, ItemsOnCollectionChanged);
        }

        public CollectionSelector(ObservableCollection<T> items)
            : this(items == null ? null : new ReadOnlyObservableList<T>(items))
        {

        }

        /// <summary>
        /// Sets and gets the SelectedItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public T SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            private set
            {
                if (EqualityComparer<T>.Default.Equals(_selectedItem, value))
                    return;
                OnSelectedItemChanging();
                _selectedItem = value;
                OnSelectedItemChanged();
                OnPropertyChanged();
            }
        }

        public IReadOnlyObservableList<T> Items { get; }

        /// <summary>
        ///     Sets and gets the SelectedIndex property.
        ///     Changes to that property's value raise the PropertyChanged event.
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnPropertyChanged();
                UpdateSelectedItem();
            }
        }

        protected void UpdateSelectedItem()
        {
            SelectedItem = SelectedIndex < 0 || SelectedIndex >= Items.Count
                ? default(T)
                : Items[SelectedIndex];
        }

        protected virtual void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectedItemChanging()
        {
            SelectedItemChanging?.Invoke(this, EventArgs.Empty);
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var newStartingIndex = args.NewStartingIndex;
            var oldStartingIndex = args.OldStartingIndex;
            var newItems = args.NewItems;
            var oldItems = args.OldItems;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnItemsAdded(newStartingIndex, newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnItemsRemoved(oldStartingIndex, oldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnItemsReplaced(oldStartingIndex, oldItems, newStartingIndex, newItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnItemsMoved(oldStartingIndex, oldItems, newStartingIndex, newItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionReset();
                    break;
            }
        }

        protected virtual void OnCollectionReset()
        {
            SelectedIndex = Items.Count == 0 ? -1 : 0;
        }

        protected virtual void OnItemsMoved(int oldStartingIndex, IList oldItems, int newStartingIndex, IList newItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
                SelectedIndex = newStartingIndex + (SelectedIndex - oldStartingIndex);
        }

        protected virtual void OnItemsReplaced(int oldStartingIndex, IList oldItems, int newStartingIndex, IList newItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
                UpdateSelectedItem();
            else if (SelectedIndex > oldStartingIndex)
                SelectedIndex += newItems.Count - oldItems.Count;
        }

        protected virtual void OnItemsRemoved(int oldStartingIndex, IList oldItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
                SelectedIndex = Math.Min(oldStartingIndex, ((ICollection) Items).Count - 1);
            else if (SelectedIndex > oldStartingIndex)
                SelectedIndex -= oldItems.Count;
        }

        protected virtual void OnItemsAdded(int newStartingIndex, IList newItems)
        {
            if (newStartingIndex <= SelectedIndex)
                SelectedIndex += newItems.Count;
        }

        public event EventHandler SelectedItemChanged;
        public event EventHandler SelectedItemChanging;

        public bool Select(T item)
        {
            var list = Items as IList;
            var index = list?.IndexOf(item) ?? Items.IndexOf(item);
            if (index < 0)
                return false;
            SelectedIndex = index;
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}