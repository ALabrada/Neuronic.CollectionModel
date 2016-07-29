using System;

namespace Neuronic.CollectionModel
{
    public class IndexedItemContainer<T> : ItemContainer<T>
    {
        public IndexedItemContainer(T item, Predicate<T> filter) : base (item, filter)
        {
        }

        public int LocalIndex { get; set; }
        public int GlobalIndex { get; set; }
    }
}