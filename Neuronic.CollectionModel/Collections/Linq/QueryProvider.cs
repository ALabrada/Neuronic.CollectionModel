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

            if (mc.Method.TryFindMethod(out var method))
            {
                var arguments = mc.Arguments.Select(x => 
                    LambdaFinder.FindIn(x) ?? ConstantFinder<object>.FindIn(x)).ToArray();
                return (IQueryable<TElement>) method.Invoke(null, arguments);
            }

            var source = new Lazy<IEnumerable<TSource>>(() => (queryable as IQueryableCollection<TSource>)?.Source ?? queryable);
            var collection = new Lazy<IReadOnlyObservableCollection<TSource>>(() => source.Value as IReadOnlyObservableCollection<TSource>);
            var list = new Lazy<IReadOnlyObservableList<TSource>>(() => source.Value as IReadOnlyObservableList<TSource>);

            switch (mc.Method.Name)
            {
                case "OfType" when mc.Arguments.Count == 1 && collection.Value != null:
                    return list.Value != null 
                        ? new CastingReadOnlyObservableList<TSource,TElement>(list.Value.ListWhere(x => x is TElement)).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(source.Value.ListWhere(x => x is TElement)).AsQueryableCollection();
                case "Cast" when mc.Arguments.Count == 1 && collection.Value != null:
                    return list.Value != null 
                        ? new CastingReadOnlyObservableList<TSource, TElement>(list.Value).AsQueryableCollection() 
                        : new CastingReadOnlyObservableCollection<TSource, TElement>(collection.Value).AsQueryableCollection();
            }

            var updater = new QueryModifier();
            expression = updater.Visit(expression);
            if (updater.Source == null)
                throw new InvalidOperationException();
            var result = updater.Source.Provider.CreateQuery<TElement>(expression);
            return result;
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
                if (mc.Method.TryFindMethod(out var method))
                {
                    var arguments = mc.Arguments.Select(x =>
                        LambdaFinder.FindIn(x) ?? ConstantFinder<object>.FindIn(x)).ToArray();
                    return (TResult) method.Invoke(null, arguments);
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
                    Source = (source as IQueryableCollection<TSource>)?.Source.AsQueryable() ?? source;
                    return Expression.Constant(Source);
                }

                return base.VisitConstant(node);
            }
        }
    }

    class InvertedComparer<T> : Comparer<T>
    {
        private readonly IComparer<T> _other;

        public InvertedComparer(IComparer<T> other)
        {
            _other = other ?? Default;
        }

        public override int Compare(T x, T y)
        {
            return -_other.Compare(x, y);
        }
    }
}