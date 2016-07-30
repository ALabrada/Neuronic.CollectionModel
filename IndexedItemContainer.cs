using System;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Stores an item and it's meta-data in a filtered collection, including its indexes.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.ItemContainer{T}" />
    public class IndexedItemContainer<T> : ItemContainer<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IndexedItemContainer{T}" /> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="filter">The filter.</param>
        public IndexedItemContainer(T item, Predicate<T> filter) : base(item, filter)
        {
        }

        /// <summary>
        ///     Gets or sets the local index.
        /// </summary>
        /// <value>
        ///     The items's index in the filtered collection, if it belong to that collection;
        ///     otherwise the index that it would be in, should it be included in the filtered collection.
        /// </value>
        public int LocalIndex { get; set; }

        /// <summary>
        ///     Gets or sets the global index.
        /// </summary>
        /// <value>
        ///     The items's index in the source collection.
        /// </value>
        public int GlobalIndex { get; set; }
    }
}