using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    ///     Stores an item and it's meta-data in a filtered collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class FilterItemContainer<TItem> : ObservableItemContainer<TItem, bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterItemContainer{TItem}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="observable">The observable.</param>
        public FilterItemContainer(TItem item, IObservable<bool> observable) : base(item, observable)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is included.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is included in the filtered collection; otherwise, <c>false</c>.
        /// </value>
        public bool IsIncluded { get; private set; }

        /// <summary>
        ///     Called when the observed value changes.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void OnValueChanged(bool value)
        {
            var wasIncluded = IsIncluded;
            IsIncluded = value;
            if (IsIncluded != wasIncluded)
                IsIncludedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Occurs when the <see cref="IsIncluded" /> property value changes.
        /// </summary>
        public event EventHandler IsIncludedChanged;
    }
}