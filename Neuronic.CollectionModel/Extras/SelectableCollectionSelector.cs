using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// Represents an <see cref="ICollectionSelector{T}"/> of <see cref="ISelectableItem"/> that automatically updates the item's selection state.
    /// </summary>
    /// <typeparam name="T">Type of the items. It should implement <see cref="ISelectableItem"/>.</typeparam>
    /// <seealso cref="CollectionSelector{T}" />
    public class SelectableCollectionSelector<T> : CollectionSelector<T> where T: ISelectableItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableCollectionSelector{T}"/> class.
        /// </summary>
        /// <param name="items">The source items.</param>
        public SelectableCollectionSelector(IReadOnlyObservableList<T> items) : base(items)
        {
            foreach (var selectableItem in Items)
                SelectionChangedEventManager.AddListener(selectableItem, this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableCollectionSelector{T}"/> class.
        /// </summary>
        /// <param name="items">The source items. This instance is the only way to modify the selector's collection.</param>
        public SelectableCollectionSelector(ObservableCollection<T> items) : base(items)
        {
            foreach (var selectableItem in Items)
                SelectionChangedEventManager.AddListener(selectableItem, this);
        }

        /// <summary>
        /// Called before the selected item chanes.
        /// </summary>
        protected override void OnSelectedItemChanging()
        {
            base.OnSelectedItemChanging();
            if (SelectedItem != null)
                SelectedItem.IsSelected = false;
        }

        /// <summary>
        /// Called when the selected item changes.
        /// </summary>
        protected override void OnSelectedItemChanged()
        {
            if (SelectedItem != null)
                SelectedItem.IsSelected = true;
            base.OnSelectedItemChanged();
        }

        /// <summary>
        /// Handles a notification.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <returns>
        ///   <c>true</c> if the event was handled; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(SelectionChangedEventManager) || !(sender is T))
                return base.OnReceiveWeakEvent(managerType, sender, e);
            ItemOnSelectionChanged(sender, e);
            return true;
        }

        /// <summary>
        /// Called when items are added to the source collection.
        /// </summary>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected override void OnItemsAdded(int newStartingIndex, IList newItems)
        {
            base.OnItemsAdded(newStartingIndex, newItems);
            foreach (T selectableItem in newItems)
                SelectionChangedEventManager.AddListener(selectableItem, this);
        }

        /// <summary>
        /// Called when items are removed from the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        protected override void OnItemsRemoved(int oldStartingIndex, IList oldItems)
        {
            foreach (T selectableItem in oldItems)
                SelectionChangedEventManager.RemoveListener(selectableItem, this);
            base.OnItemsRemoved(oldStartingIndex, oldItems);
        }

        /// <summary>
        /// Called when the source collection is reseted.
        /// </summary>
        protected override void OnCollectionReset()
        {
            base.OnCollectionReset();
            foreach (var selectableItem in Items)
                SelectionChangedEventManager.AddListener(selectableItem, this);
        }

        /// <summary>
        /// Called when items are replaced in the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected override void OnItemsReplaced(int oldStartingIndex, IList oldItems, int newStartingIndex, IList newItems)
        {
            foreach (T selectableItem in oldItems)
                SelectionChangedEventManager.RemoveListener(selectableItem, this);
            base.OnItemsReplaced(oldStartingIndex, oldItems, newStartingIndex, newItems);
            foreach (T selectableItem in newItems)
                SelectionChangedEventManager.AddListener(selectableItem, this);
        }

        private void ItemOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var item = (T) sender;
            if (item.IsSelected && !ReferenceEquals(item, SelectedItem))
                Select(item);
        }
    }
}