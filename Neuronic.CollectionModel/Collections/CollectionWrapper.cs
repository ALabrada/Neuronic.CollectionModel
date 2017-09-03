using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    internal class CollectionWrapper<T> : EventSource, IReadOnlyObservableCollection<T>
    {
        public CollectionWrapper(ICollection items) : this(items, items, nameof(Count))
        {
        }

        public CollectionWrapper(IEnumerable<T> items) : this(items, items as ICollection, nameof(Count))
        {
        }

        protected CollectionWrapper(IEnumerable source, ICollection items, params string[] propertyNames) : base (source, propertyNames)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Items = items ?? source.Cast<T>().ToList();
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

    internal class ListWrapper<T> : CollectionWrapper<T>, IReadOnlyObservableList<T>
    {
        public ListWrapper(IList list) : base(list, list, nameof(Count), "Item[]")
        {
        }

        public ListWrapper(IEnumerable<T> items) : base(items, items as IList, nameof(Count), "Item[]")
        {
        }

        protected new IList Items => (IList)base.Items;

        public T this[int index] => (T)Items[index];
    }
}