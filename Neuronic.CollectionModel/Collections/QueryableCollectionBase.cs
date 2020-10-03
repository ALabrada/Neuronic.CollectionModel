﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Neuronic.CollectionModel.Collections
{
    abstract class QueryableCollectionBase<T>
    {
        public Type ElementType => typeof(T);

        public virtual Expression Expression => Expression.Constant(this);

        public virtual IQueryProvider Provider => new QueryProvider<T>();
    }

    class QueryableCollection<T> : QueryableCollectionBase<T>, IQueryable<T>
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