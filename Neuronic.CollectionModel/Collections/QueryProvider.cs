using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neuronic.CollectionModel.Collections
{
    class QueryProvider<TSource>: IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            var result = updater.Source.Provider.CreateQuery(expression);
            return result;
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (!(expression is MethodCallExpression mc))
                throw new ArgumentException("Invalid expression", nameof(expression));
            var queryable = ConstantFinder<IQueryable<TSource>>.FindIn(mc.Arguments[0]);
            if (queryable == null)
                throw new InvalidOperationException("Cannot find source.");
            var source = (queryable as QueryableCollection<TSource>)?.Source ?? queryable;
            var collection = source as IReadOnlyObservableCollection<TSource>;
            var list = source as IReadOnlyObservableList<TSource>;

            Expression<Func<TSource, bool>> predicate;
            switch (mc.Method.Name)
            {
                case "Where" when mc.Arguments.Count == 2 && typeof(TElement) == typeof(TSource):
                    predicate = LambdaFinder.FindIn(mc.Arguments[1]) as Expression<Func<TSource, bool>>;
                    return (IQueryable<TElement>)source.ListWhereObservable(item => new FunctionObservable<TSource, bool>(item, predicate)).AsQueryableCollection();
                case "OfType" when mc.Arguments.Count == 1 && collection != null:
                    return list != null 
                        ? new CastingReadOnlyObservableList<TSource,TElement>(list.ListWhere(x => x is TElement)).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(source.ListWhere(x => x is TElement)).AsQueryableCollection();
                case "Cast" when mc.Arguments.Count == 1 && collection != null:
                    return list != null 
                        ? new CastingReadOnlyObservableList<TSource, TElement>(list).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(collection).AsQueryableCollection();
                case "Select" when mc.Arguments.Count == 2:
                    var selector = LambdaFinder.FindIn(mc.Arguments[1]) as Expression<Func<TSource, TElement>>;
                    return source.ListSelectObservable(item => new FunctionObservable<TSource, TElement>(item, selector))
                        .AsQueryableCollection();
                case "SelectMany" when mc.Arguments.Count == 2:
                    var collectionSelector = LambdaFinder.FindIn(mc.Arguments[1]) as Expression<Func<TSource, IEnumerable<TElement>>>;
                    return source.ListSelectMany(collectionSelector.Compile()) // TODO: observe this
                        .AsQueryableCollection();
                case "OrderBy":
                    return (IQueryable<TElement>) OrderBy(source,
                        LambdaFinder.FindIn(mc.Arguments[1]), 
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null
                        , false);
                case "ThenBy":
                    throw new NotImplementedException();
                case "OrderByDescending":
                    return (IQueryable<TElement>) OrderBy(source,
                        LambdaFinder.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null,
                        true);
                case "ThenByDescending":
                    throw new NotImplementedException();
                case "Take":
                    return (IQueryable<TElement>)list?.ListTake(ConstantFinder<int>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                case "TakeWhile":
                    throw new NotImplementedException();
                case "Skip":
                    return (IQueryable<TElement>)list?.ListSkip(ConstantFinder<int>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                case "SkipWhile":
                    throw new NotImplementedException();
                case "GroupBy" when mc.Method.GetGenericArguments().Length == 2: // TODO: Handle group selector
                    return (IQueryable<TElement>) GroupBy(source,
                        LambdaFinder.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null);
                case "Distinct" when collection != null:
                    return (IQueryable<TElement>) collection.CollectionDistinct(
                        mc.Arguments.Count > 1 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[1]) : null)
                        .AsQueryableCollection();
                case "Concat":
                    return (IQueryable<TElement>) source.ListConcat(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                case "Zip":
                    throw new NotImplementedException();
                case "Union":
                    return (IQueryable<TElement>)source.CollectionUnion(
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null,
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                case "Intersect":
                    return (IQueryable<TElement>)source.CollectionIntersect(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null)
                        .AsQueryableCollection();
                case "Except":
                    return (IQueryable<TElement>)source.CollectionExcept(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null)
                        .AsQueryableCollection();
                case "Reverse":
                    throw new NotImplementedException();
                default:
                    var updater = new QueryModifier();
                    expression = updater.Visit(expression);
                    var result = updater.Source.Provider.CreateQuery<TElement>(expression);
                    return result;
            }
        }

        private static object OrderBy(IEnumerable<TSource> source, Expression sortKeySelector, Expression comparer, bool invert)
        {
#if NET40
            var delegateType = sortKeySelector.GetType().GetGenericArguments()[0];
            var keyType = delegateType.GetGenericArguments()[1];
#else
            var delegateType = sortKeySelector.GetType().GetTypeInfo().GenericTypeArguments[0];
            var keyType = delegateType.GetTypeInfo().GenericTypeArguments[1];
#endif
            var targetType = typeof(KeySortedReadOnlyObservableList<,>).MakeGenericType(typeof(TSource), keyType);

            var actualComparer = ConstantFinder<object>.FindIn(comparer);
            if (invert && !(actualComparer is null))
            {
                var comparerType = typeof(InvertedComparer<>).MakeGenericType(keyType);
                actualComparer = Activator.CreateInstance(comparerType, actualComparer);
            }

            var result = Activator.CreateInstance(targetType, source, sortKeySelector, actualComparer);
            return new QueryableCollection<TSource>((IReadOnlyObservableList<TSource>) result);
        }

        private static object GroupBy(IEnumerable<TSource> source, Expression sortKeySelector, Expression comparer)
        {
#if NET40
            var delegateType = sortKeySelector.GetType().GetGenericArguments()[0];
            var keyType = delegateType.GetGenericArguments()[1];
#else
            var delegateType = sortKeySelector.GetType().GetTypeInfo().GenericTypeArguments[0];
            var keyType = delegateType.GetTypeInfo().GenericTypeArguments[1];
#endif
            var targetType = typeof(GroupingReadOnlyObservableCollectionSource<,>).MakeGenericType(typeof(TSource), keyType);
            var actualComparer = ConstantFinder<object>.FindIn(comparer);

            var result = Activator.CreateInstance(targetType, source, sortKeySelector, actualComparer, null);

            var resultType = typeof(IGrouping<,>).MakeGenericType(keyType, typeof(TSource));
            var queryType = typeof(QueryableCollection<>).MakeGenericType(resultType);
            return Activator.CreateInstance(queryType, result);
        }

        public object Execute(Expression expression)
        {
            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            var result = updater.Source.Provider.Execute(expression);
            return result;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            var result = updater.Source.Provider.Execute<TResult>(expression);
            return result;
        }

        class ConstantFinder<T> : ExpressionVisitor
        {
            public T Result { get; private set; }

            public static T FindIn(Expression expression)
            {
                if (expression == null)
                    return default(T);
                var finder = new ConstantFinder<T>();
                finder.Visit(expression);
                return finder.Result;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is T result)
                    Result = result;
                return base.VisitConstant(node);
            }
        }

        class LambdaFinder : ExpressionVisitor
        {
            public LambdaExpression Result { get; private set; }

            public static LambdaExpression FindIn(Expression expression)
            {
                if (expression == null)
                    return null;
                var finder = new LambdaFinder();
                finder.Visit(expression);
                return finder.Result;
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Result = node;
                return base.VisitLambda(node);
            }
        }

        class QueryModifier : ExpressionVisitor
        {
            public IQueryable<TSource> Source { get; private set; }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (Source == null && node.Value is QueryableCollection<TSource> source)
                {
#if NETSTD
                    throw new NotSupportedException("Cannot execute this operation directly in IQueryable<>. Convert to IEnumerable<> first.");
#else
                    Source = source.Source.AsQueryableCollection();
                    return Expression.Constant(Source);
#endif
                }

                return base.VisitConstant(node);
            }
        }
    }

    class InvertedComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> _other;

        public InvertedComparer(IComparer<T> other)
        {
            _other = other ?? Comparer<T>.Default;
        }

        public int Compare(T x, T y)
        {
            return -_other.Compare(x, y);
        }
    }
}