using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Neuronic.CollectionModel.Collections;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    ///     List selection mechanism.
    /// </summary>
    /// <remarks>
    ///     This class can handle changes in the collection of items, but not through the selector instance itself.
    ///     To modify the selector's collection, use the source collection instead, the one used in class constructors.
    /// </remarks>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <seealso cref="ICollectionSelector{T}" />
    public class CollectionSelector<T> : ICollectionSelector<T>, IWeakEventListener
    {
        private int _selectedIndex = -1;
        private T _selectedItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CollectionSelector{T}" /> class.
        /// </summary>
        /// <param name="items">The source items.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="items" /> is <strong>null</strong>.</exception>
        public CollectionSelector(IReadOnlyObservableList<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Items = items;
            CollectionChangedEventManager.AddListener(Items, this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CollectionSelector{T}" /> class.
        /// </summary>
        /// <param name="items">The source items. This instance is the only way to modify the selector's collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="items" /> is <strong>null</strong>.</exception>
        public CollectionSelector(ObservableCollection<T> items)
            : this(items == null ? null : new ReadOnlyObservableList<T>(items))
        {
        }
        
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return OnReceiveWeakEvent(managerType, sender, e);
        }

        /// <summary>
        /// Handles a notification.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if the event was handled; otherwise, <c>false</c>.</returns>
        protected virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(Items, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            ItemsOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs) e);
            return true;
        }

        /// <summary>
        ///     Gets the selected item.
        ///     Changes to this value rise the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> and
        ///     <see cref="E:Neuronic.CollectionModel.Extras.ICollectionSelector`1.SelectedItemChanged" /> events.
        /// </summary>
        /// <value>
        ///     The selected item.
        /// </value>
        public T SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                if (EqualityComparer<T>.Default.Equals(_selectedItem, value))
                    return;
                OnSelectedItemChanging();
                _selectedItem = value;
                OnSelectedItemChanged();
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The selector's collection of items.
        /// </value>
        public IReadOnlyObservableList<T> Items { get; }

        /// <summary>
        ///     Gets or sets the index of the selected item.
        ///     Changes to this value rise the <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <value>
        ///     The index of the selected item.
        /// </value>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedIndex)));
                UpdateSelectedItem();
            }
        }

        /// <summary>
        ///     Occurs when the selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Updates the selected item.
        /// </summary>
        protected void UpdateSelectedItem()
        {
            SelectedItem = SelectedIndex < 0 || SelectedIndex >= Items.Count
                ? default(T)
                : Items[SelectedIndex];
        }

        /// <summary>
        ///     Called when the selected item changes.
        /// </summary>
        protected virtual void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called before the selected item chanes.
        /// </summary>
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

        /// <summary>
        ///     Called when the source collection is reseted.
        /// </summary>
        protected virtual void OnCollectionReset()
        {
            SelectedIndex = Items.Count == 0 ? -1 : 0;
        }

        /// <summary>
        ///     Called when items are moved in the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected virtual void OnItemsMoved(int oldStartingIndex, IList oldItems, int newStartingIndex, IList newItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
                SelectedIndex = newStartingIndex + (SelectedIndex - oldStartingIndex);
        }

        /// <summary>
        ///     Called when items are replaced in the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected virtual void OnItemsReplaced(int oldStartingIndex, IList oldItems, int newStartingIndex,
            IList newItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
                UpdateSelectedItem();
            else if (SelectedIndex > oldStartingIndex)
                SelectedIndex += newItems.Count - oldItems.Count;
        }

        /// <summary>
        ///     Called when items are removed from the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        protected virtual void OnItemsRemoved(int oldStartingIndex, IList oldItems)
        {
            if (SelectedIndex >= oldStartingIndex &&
                SelectedIndex < oldStartingIndex + oldItems.Count)
            {
                SelectedIndex = Math.Min(oldStartingIndex, ((ICollection) Items).Count - 1);
                UpdateSelectedItem();
            }
            else if (SelectedIndex > oldStartingIndex)
                SelectedIndex -= oldItems.Count;
        }

        /// <summary>
        ///     Called when items are added to the source collection.
        /// </summary>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected virtual void OnItemsAdded(int newStartingIndex, IList newItems)
        {
            if (newStartingIndex <= SelectedIndex)
                SelectedIndex += newItems.Count;
        }

        /// <summary>
        ///     Occurs before the selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanging;

        /// <summary>
        ///     Selects the specified item.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <returns>Whether the item is included in the collection.</returns>
        public bool Select(T item)
        {
            return this.TrySelect(item);
        }


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