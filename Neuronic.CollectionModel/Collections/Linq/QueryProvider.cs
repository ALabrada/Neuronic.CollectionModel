using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Observables;

namespace Neuronic.CollectionModel.Collections.Linq
{
    struct QueryProvider<TSource>: IQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            if (updater.Source == null)
                throw new InvalidOperationException();
            var result = updater.Source.Provider.CreateQuery(expression);
            return result;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (!(expression is MethodCallExpression mc))
                throw new ArgumentException("Invalid expression", nameof(expression));
            var queryable = ConstantFinder<IQueryable<TSource>>.FindIn(mc.Arguments[0]);
            if (queryable == null)
                throw new InvalidOperationException("Cannot find source.");
            var source = new Lazy<IEnumerable<TSource>>(() => (queryable as QueryableCollection<TSource>)?.Source ?? queryable);
            var collection = new Lazy<IReadOnlyObservableCollection<TSource>>(() => source.Value as IReadOnlyObservableCollection<TSource>);
            var list = new Lazy<IReadOnlyObservableList<TSource>>(() => source.Value as IReadOnlyObservableList<TSource>);

            LambdaExpression lambda;
            switch (mc.Method.Name)
            {
                case "Where" when mc.Arguments.Count == 2 && typeof(TElement) == typeof(TSource):
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    var whereQuery = Filter(source.Value, lambda);
                    if (whereQuery != null) return (IQueryable<TElement>) whereQuery;
                    break;
                case "OfType" when mc.Arguments.Count == 1 && collection.Value != null:
                    return list.Value != null 
                        ? new CastingReadOnlyObservableList<TSource,TElement>(list.Value.ListWhere(x => x is TElement)).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(source.Value.ListWhere(x => x is TElement)).AsQueryableCollection();
                case "Cast" when mc.Arguments.Count == 1 && collection.Value != null:
                    return list.Value != null 
                        ? new CastingReadOnlyObservableList<TSource, TElement>(list.Value).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(collection.Value).AsQueryableCollection();
                case "Select" when mc.Arguments.Count == 2:
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    var selectQuery = Select<TElement>(source.Value, lambda);
                    if (selectQuery != null) return selectQuery;
                    break;
                case "SelectMany" when mc.Arguments.Count == 2:
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    var selectManyQuery = SelectMany<TElement>(source.Value, lambda);
                    if (selectManyQuery != null) return selectManyQuery;
                    break;
                case "OrderBy":
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    if (lambda == null) break;
                    return (IQueryable<TElement>) OrderBy(source.Value, lambda, 
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null
                        , false);
                case "Join" when mc.Arguments.Count >= 5:
                    return Join<TElement>(source.Value, ConstantFinder<object>.FindIn(mc.Arguments[1]),
                        LambdaFinder.FindIn(mc.Arguments[2]), LambdaFinder.FindIn(mc.Arguments[3]),
                        LambdaFinder.FindIn(mc.Arguments[4]),
                        mc.Arguments.Count > 5 ? ConstantFinder<object>.FindIn(mc.Arguments[5]) : null);
                case "ThenBy":
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    if (lambda == null) break;
                    return (IQueryable<TElement>)OrderBy(queryable, lambda,
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null
                        , false);
                case "OrderByDescending":
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    if (lambda == null) break;
                    return (IQueryable<TElement>) OrderBy(source.Value, lambda,
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null,
                        true);
                case "ThenByDescending":
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    if (lambda == null) break;
                    return (IQueryable<TElement>)OrderBy(queryable, lambda,
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null,
                        true);
                case "Take" when list.Value != null:
                    return (IQueryable<TElement>)list.Value.ListTake(ConstantFinder<int>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                //case "TakeWhile": // TODO
                case "Skip" when list.Value != null:
                    return (IQueryable<TElement>)list.Value.ListSkip(ConstantFinder<int>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                //case "SkipWhile": // TODO
                case "GroupBy" when mc.Method.GetGenericArguments().Length == 2: // TODO: Handle group selector
                    lambda = LambdaFinder.FindIn(mc.Arguments[1]);
                    if (lambda == null) break;
                    return (IQueryable<TElement>) GroupBy(source.Value, lambda,
                        mc.Arguments.Count > 2 ? mc.Arguments[2] : null);
                case "Distinct" when collection.Value != null:
                    return (IQueryable<TElement>) collection.Value.CollectionDistinct(
                        mc.Arguments.Count > 1 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[1]) : null)
                        .AsQueryableCollection();
                case "Concat":
                    return (IQueryable<TElement>) source.Value.ListConcat(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                //case "Zip": // TODO
                case "Union":
                    return (IQueryable<TElement>)source.Value.CollectionUnion(
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null,
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]))
                        .AsQueryableCollection();
                case "Intersect":
                    return (IQueryable<TElement>)source.Value.CollectionIntersect(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null)
                        .AsQueryableCollection();
                case "Except":
                    return (IQueryable<TElement>)source.Value.CollectionExcept(
                        ConstantFinder<IEnumerable<TSource>>.FindIn(mc.Arguments[1]),
                        mc.Arguments.Count > 2 ? ConstantFinder<IEqualityComparer<TSource>>.FindIn(mc.Arguments[2]) : null)
                        .AsQueryableCollection();
                case "Reverse" when list.Value != null:
                    return (IQueryable<TElement>) list.Value.ListReverse().AsQueryableCollection();
            }

            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            if (updater.Source == null)
                throw new InvalidOperationException();
            var result = updater.Source.Provider.CreateQuery<TElement>(expression);
            return result;
        }

        private static IQueryable<TElement> Select<TElement>(IEnumerable<TSource> source, LambdaExpression lambda)
        {
            var selector = lambda as Expression<Func<TSource, TElement>>;
            var indexedSelector = lambda as Expression<Func<TSource, int, TElement>>;
            if (selector != null)
            {
                var factory = PropertyObservableFactory<TSource, TElement>.FindIn(selector);
                return source.ListSelectObservable(item => factory.Observe(item))
                    .AsQueryableCollection();
            }

            if (indexedSelector != null)
            {
                var factory = indexedSelector.FindProperties();
                var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(source,
                    x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
                return indexed.ListSelectObservable(
                        item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                    .AsQueryableCollection();
            }

            return null;
        }

        private static IQueryable<TElement> SelectMany<TElement>(IEnumerable<TSource> source, LambdaExpression lambda)
        {
            var selector = lambda as Expression<Func<TSource, IEnumerable<TElement>>>;
            var indexedSelector = lambda as Expression<Func<TSource, int, IEnumerable<TElement>>>;
            if (selector != null)
            {
                var factory = PropertyObservableFactory<TSource, IEnumerable<TElement>>.FindIn(selector);
                return source.ListSelectManyObservable(item => factory.Observe(item))
                    .AsQueryableCollection();
            }

            if (indexedSelector != null)
            {
                var factory = indexedSelector.FindProperties();
                var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(source,
                    x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
                return indexed.ListSelectManyObservable(
                        item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                    .AsQueryableCollection();
            }

            return null;
        }       

        private static IQueryable Filter(IEnumerable<TSource> source, LambdaExpression lambda)
        {
            var predicate = lambda as Expression<Func<TSource, bool>>;
            var indexedPredicate = lambda as Expression<Func<TSource, int, bool>>;
            if (predicate != null)
            {
                var factory = PropertyObservableFactory<TSource, bool>.FindIn(predicate);
                return source.ListWhereObservable(item => factory.Observe(item))
                    .AsQueryableCollection();
            }

            if (indexedPredicate != null)
            {
                var factory = PropertyObservableFactory<TSource, int, bool>.FindIn(indexedPredicate);
                var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(source, 
                    x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
                return indexed.ListWhereObservable(
                        item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                    .AsQueryableCollection();
            }

            return null;
        }

        private static IQueryable<TElement> Join<TElement>(IEnumerable<TSource> outerSource, object innerSource,
           LambdaExpression outerSelector, LambdaExpression innerSelector, LambdaExpression resultSelector, object comparer)
        {
#if NET40
            var innerSelectorType = innerSelector.GetType().GetGenericArguments()[0];
            var args = innerSelectorType.GetGenericArguments();
#else
            var innerSelectorType = innerSelector.GetType().GetTypeInfo().GenericTypeArguments[0];
            var args = innerSelectorType.GetTypeInfo().GenericTypeArguments;
#endif
            var innerType = args[0];
            var keyType = args[1];

            var collectionType = typeof(InnerJoinReadOnlyObservableCollection<,,,>)
                .MakeGenericType(typeof(TSource), innerType, keyType, typeof(TElement));
            var collection = (IReadOnlyObservableCollection<TElement>) Activator.CreateInstance(collectionType, 
                outerSource, innerSource, outerSelector, innerSelector, resultSelector, 
                comparer, null, null);

            return collection.AsQueryableCollection();
        }

        private static IQueryable OrderBy(IEnumerable<TSource> source, LambdaExpression sortKeySelector, Expression comparer, bool invert)
        {
#if NET40
            var delegateType = sortKeySelector.GetType().GetGenericArguments()[0];
            var keyType = delegateType.GetGenericArguments()[1];
#else
            var delegateType = sortKeySelector.GetType().GetTypeInfo().GenericTypeArguments[0];
            var keyType = delegateType.GetTypeInfo().GenericTypeArguments[1];
#endif

            var actualComparer = ConstantFinder<object>.FindIn(comparer);
            if (invert)
            {
                var comparerType = typeof(InvertedComparer<>).MakeGenericType(keyType);
                actualComparer = Activator.CreateInstance(comparerType, actualComparer);
            }

            var definitionType = typeof(Definition<,>).MakeGenericType(typeof(TSource), keyType);
            var definition = (IDefinition<TSource>) Activator.CreateInstance(definitionType, sortKeySelector, actualComparer);

            return new KeySortingQueryableCollection<TSource>(source, definition);
        }

        private static IQueryable GroupBy(IEnumerable<TSource> source, LambdaExpression sortKeySelector, Expression comparer)
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
            return (IQueryable) Activator.CreateInstance(queryType, result);
        }

        public object Execute(Expression expression)
        {
            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            if (updater.Source == null)
                throw new InvalidOperationException();
            var result = updater.Source.Provider.Execute(expression);
            return result;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression is MethodCallExpression mc)
            {
                var queryable = ConstantFinder<IQueryable<TSource>>.FindIn(mc.Arguments[0]);
                var source = new Lazy<IEnumerable<TSource>>(() => (queryable as QueryableCollection<TSource>)?.Source ?? queryable);
                var collection = new Lazy<IReadOnlyObservableCollection<TSource>>(() => source.Value as IReadOnlyObservableCollection<TSource>);
                var list = new Lazy<IReadOnlyObservableList<TSource>>(() => source.Value as IReadOnlyObservableList<TSource>);
                if (queryable != null)
                {
                    switch (mc.Method.Name)
                    {
                        case "Count" when mc.Arguments.Count == 1 && collection.Value != null:
                            return (TResult)(object) collection.Value.Count;
                        case "Last" when mc.Arguments.Count == 1 && list.Value != null && list.Value.Count > 0:
                            return (TResult)(object) list.Value[list.Value.Count - 1];
                        case "LastOrDefault" when mc.Arguments.Count == 1 && list.Value != null && list.Value.Count > 0:
                            return (TResult)(object)list.Value[list.Value.Count - 1];
                    }
                }
            }

            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            if (updater.Source == null)
                throw new InvalidOperationException();
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
                if (Source == null && node.Value is IQueryable<TSource> source)
                {
#if NETSTANDARD1_1
                    throw new NotSupportedException("Cannot execute this operation directly in IQueryable<>. Convert to IEnumerable<> first.");
#else
                    Source = (source as QueryableCollection<TSource>)?.Source.AsQueryable() ?? source;
                    return Expression.Constant(Source);
#endif
                }

                return base.VisitConstant(node);
            }
        }
    }

    class InvertedComparer<T> : Comparer<T>
    {
        private readonly Comparer<T> _other;

        public InvertedComparer(Comparer<T> other)
        {
            _other = other ?? Comparer<T>.Default;
        }

        public override int Compare(T x, T y)
        {
            return -_other.Compare(x, y);
        }
    }
}