using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    internal class CollectionWrapper<T> : EventSource, IReadOnlyCollection<T>
    {
        public CollectionWrapper(ICollection items) : this(items, nameof(Count))
        {
        }

        public CollectionWrapper(IEnumerable<T> items) : this(items, nameof(Count))
        {
        }

        public CollectionWrapper(ICollection items, params string[] propertyNames) : base(items, propertyNames)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Items = items;
        }

        public CollectionWrapper(IEnumerable<T> items, params string[] propertyNames) : base(items, propertyNames)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            Items = items.ToList();
        }

        protected ICollection Items { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Items.Count;
    }

    internal class ListWrapper<T> : CollectionWrapper<T>, IReadOnlyList<T>
    {
        public ListWrapper(IList list) : base(list, nameof(Count), "Item[]")
        {
        }

        public ListWrapper(IEnumerable<T> items) : base(items, nameof(Count), "Item[]")
        {
        }

        protected new IList Items => (IList) base.Items;

        public T this[int index] => (T) Items[index];
    }
}