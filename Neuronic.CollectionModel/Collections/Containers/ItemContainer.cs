namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Abstraction of an item and it's meta-data in some collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class ItemContainer<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemContainer{TItem}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        protected ItemContainer(TItem item)
        {
            Item = item;
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public TItem Item { get; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Item?.Equals(obj) ?? false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Item?.GetHashCode() ?? 0;
        }
    }
}