using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Stores an item and it's meta-data in a filtered collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ItemContainer<TItem>
    {
        private readonly Predicate<TItem> _filter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemContainer{TItem}" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="filter">The filter.</param>
        public ItemContainer(TItem item, Predicate<TItem> filter)
        {
            _filter = filter;
            Item = item;
            IsIncluded = _filter(Item);
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public TItem Item { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is included.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is included in the filtered collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsIncluded { get; private set; }

        /// <summary>
        ///     Attaches the event handlers that listen to changes in the trigger properties.
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        public void AttachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.AddHandler(notify, ItemOnTriggerPropertyChanged, name);
        }

        /// <summary>
        ///     Detaches the event handlers that listen to changes in the trigger properties.
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        public void DetachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.RemoveHandler(notify, ItemOnTriggerPropertyChanged, name);
        }

        private void ItemOnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var wasIncluded = IsIncluded;
            IsIncluded = _filter(Item);
            if (IsIncluded != wasIncluded)
                IsIncludedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Occurs when the <see cref="IsIncluded" /> property value changes.
        /// </summary>
        public event EventHandler IsIncludedChanged;
    }
}