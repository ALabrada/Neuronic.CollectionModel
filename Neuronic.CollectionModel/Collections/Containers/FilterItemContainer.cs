using System;
using System.Collections.Generic;
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
        /// <param name="valueComparer">The value comparer.</param>
        public FilterItemContainer(TItem item, IObservable<bool> observable, IEqualityComparer<bool> valueComparer = null) : base(item, observable, valueComparer)
        {
        }

        /// <inheritdoc />
        protected override void OnNewValue(bool value)
        {
            if (value == Value)
                return;
            base.OnNewValue(value);
        }
    }
}