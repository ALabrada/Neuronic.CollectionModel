using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Neuronic.CollectionModel.Collections.Linq
{
    abstract class QueryableCollectionBase<T>
    {
        public Type ElementType => typeof(T);

        public virtual Expression Expression => Expression.Constant(this);

        public virtual IQueryProvider Provider => new QueryProvider<T>();
    }

    internal interface IQueryableCollection<out T> : IQueryable<T>
    {
        IEnumerable<T> Source { get; }
    }

    class QueryableCollection<T> : QueryableCollectionBase<T>, IQueryableCollection<T>
    {
        public QueryableCollection(IEnumerable<T> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public virtual IEnumerable<T> Source { get; }

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