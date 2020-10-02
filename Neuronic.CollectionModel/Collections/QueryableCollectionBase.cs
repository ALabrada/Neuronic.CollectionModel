using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Neuronic.CollectionModel.Collections
{
    abstract class QueryableCollectionBase<T>
    {
        public Type ElementType => typeof(T);

        public virtual Expression Expression => Expression.Constant(this);

        public virtual IQueryProvider Provider => new QueryProvider<T>();
    }

    class QueryableCollection<T> : QueryableCollectionBase<T>, IOrderedQueryable<T>
    {
        public QueryableCollection(IEnumerable<T> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IEnumerable<T> Source { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Source).GetEnumerator();
        }
    }
}