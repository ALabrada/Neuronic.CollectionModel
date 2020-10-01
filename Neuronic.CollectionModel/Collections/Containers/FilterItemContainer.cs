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
        public FilterItemContainer(TItem item, IObservable<bool> observable, IEqualityComparer<bool> valueComparer = null) : base(item, observable, valueComparer)
        {
        }
    }
}