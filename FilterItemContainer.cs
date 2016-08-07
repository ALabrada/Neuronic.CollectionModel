using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Stores an item and it's meta-data in a filtered collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class FilterItemContainer<TItem> : ItemContainer<TItem>
    {
        private readonly Predicate<TItem> _filter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilterItemContainer{TItem}" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="filter">The filter.</param>
        public FilterItemContainer(TItem item, Predicate<TItem> filter) : base (item)
        {
            _filter = filter;
            IsIncluded = _filter(Item);
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is included.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is included in the filtered collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsIncluded { get; private set; }

        /// <summary>
        /// Called when the value of any of the trigger property changes for this item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
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