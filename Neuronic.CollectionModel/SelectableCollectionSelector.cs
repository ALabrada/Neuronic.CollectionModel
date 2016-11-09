using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents an <see cref="ICollectionSelector{T}"/> of <see cref="ISelectableItem"/> that automatically updates the item's selection state.
    /// </summary>
    /// <typeparam name="T">Type of the items. It should implement <see cref="ISelectableItem"/>.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.CollectionSelector{T}" />
    public class SelectableCollectionSelector<T> : CollectionSelector<T> where T: ISelectableItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableCollectionSelector{T}"/> class.
        /// </summary>
        /// <param name="items">The source items.</param>
        public SelectableCollectionSelector(IReadOnlyObservableList<T> items) : base(items)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableCollectionSelector{T}"/> class.
        /// </summary>
        /// <param name="items">The source items. This instance is the only way to modify the selector's collection.</param>
        public SelectableCollectionSelector(ObservableCollection<T> items) : base(items)
        {
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
        /// Called when items are added to the source collection.
        /// </summary>
        /// <param name="newStartingIndex">New index of the starting.</param>
        /// <param name="newItems">The new items.</param>
        protected override void OnItemsAdded(int newStartingIndex, IList newItems)
        {
            base.OnItemsAdded(newStartingIndex, newItems);
            foreach (T selectableItem in newItems)
                SelectionChangedEventManager.AddHandler(selectableItem, ItemOnSelectionChanged);
        }

        /// <summary>
        /// Called when items are removed from the source collection.
        /// </summary>
        /// <param name="oldStartingIndex">Old index of the starting.</param>
        /// <param name="oldItems">The old items.</param>
        protected override void OnItemsRemoved(int oldStartingIndex, IList oldItems)
        {
            foreach (T selectableItem in oldItems)
                SelectionChangedEventManager.RemoveHandler(selectableItem, ItemOnSelectionChanged);
            base.OnItemsRemoved(oldStartingIndex, oldItems);
        }

        /// <summary>
        /// Called when the source collection is reseted.
        /// </summary>
        protected override void OnCollectionReset()
        {
            base.OnCollectionReset();
            foreach (var selectableItem in Items)
                SelectionChangedEventManager.AddHandler(selectableItem, ItemOnSelectionChanged);
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
                SelectionChangedEventManager.RemoveHandler(selectableItem, ItemOnSelectionChanged);
            base.OnItemsReplaced(oldStartingIndex, oldItems, newStartingIndex, newItems);
            foreach (T selectableItem in newItems)
                SelectionChangedEventManager.AddHandler(selectableItem, ItemOnSelectionChanged);
        }

        private void ItemOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            var item = (T) sender;
            if (item.IsSelected && !ReferenceEquals(item, SelectedItem))
                Select(item);
        }
    }
}