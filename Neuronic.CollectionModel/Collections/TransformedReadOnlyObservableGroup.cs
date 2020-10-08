using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    class TransformedReadOnlyObservableGroup<TSource, TKey, TTarget> 
        : DynamicTransformingReadOnlyObservableList<TSource, TTarget>, IGrouping<TKey, TTarget>
    {
        public TransformedReadOnlyObservableGroup(TKey key,
            IEnumerable<TSource> source,
            Func<TSource, IObservable<TTarget>> selector,
            Action<TTarget> onRemove = null, Action<TTarget, TTarget> onChange = null,
            IEqualityComparer<TSource> sourceComparer = null) 
            : base(source, selector, onRemove, onChange, sourceComparer)
        {
            Key = key;
        }

        public TKey Key { get; }
    }

    class GroupingComparer<TKey, TElement> : EqualityComparer<IGrouping<TKey, TElement>>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;

        public GroupingComparer() : this(null)
        {
        }

        public GroupingComparer(IEqualityComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
        }

        public override bool Equals(IGrouping<TKey, TElement> x, IGrouping<TKey, TElement> y)
        {
            return _keyComparer.Equals(x.Key, y.Key);
        }

        public override int GetHashCode(IGrouping<TKey, TElement> obj)
        {
            return _keyComparer.GetHashCode(obj.Key);
        }
    }
}