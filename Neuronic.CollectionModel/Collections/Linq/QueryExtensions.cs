using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neuronic.CollectionModel.Extras;
using Neuronic.CollectionModel.Observables;

namespace Neuronic.CollectionModel.Collections.Linq
{
    static class QueryExtensions
    {
        internal static readonly Dictionary<MethodInfo, MethodInfo> Mapping = new Dictionary<MethodInfo, MethodInfo>();

        static QueryExtensions()
        {
            Type GetType(ParameterInfo p) => p.ParameterType;
            MethodInfo MakeGeneric(MethodInfo m, Type[] p) => m.IsGenericMethodDefinition
                ? m.MakeGenericMethod(p) : m;
            var sourceType = typeof(System.Linq.Queryable);
#if NETSTD
            var destinationType = typeof(QueryExtensions).GetTypeInfo();
            var sourceMethods = sourceType.GetTypeInfo().DeclaredMethods.Where(m => m.IsStatic && m.IsPublic);
#else
            var destinationType = typeof(QueryExtensions);
            var sourceMethods = sourceType.GetMethods(BindingFlags.Public | BindingFlags.Static);
#endif

            foreach (var srcMethod in sourceMethods)
            {
                var genericParameters = srcMethod.GetGenericArguments();
                var parameters = srcMethod.GetParameters().Select(GetType).ToArray();
#if NETSTD

                var dstMethod = destinationType.GetDeclaredMethods(srcMethod.Name).FirstOrDefault(m =>
                    m.GetGenericArguments().Length == genericParameters.Length &&
                    MakeGeneric(m, genericParameters).GetParameters().Select(GetType).SequenceEqual(parameters));
#else
                var dstMethod = destinationType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                        m.Name.Equals(srcMethod.Name, StringComparison.Ordinal) &&
                        m.GetGenericArguments().Length == genericParameters.Length &&
                        MakeGeneric(m, genericParameters).GetParameters().Select(GetType).SequenceEqual(parameters));
#endif
                if (dstMethod != null)
                    Mapping[srcMethod] = dstMethod;
            }
        }

        public static bool TryFindMethod(this MethodInfo source, out MethodInfo result)
        {
            var method = source;
            if (method.IsGenericMethod)
                method = method.GetGenericMethodDefinition();

            if (Mapping.TryGetValue(method, out result))
            {
                if (result.IsGenericMethod)
                    result = result.MakeGenericMethod(source.GetGenericArguments());

                return true;
            }

            return false;
        }

        private static IEnumerable<TSource> ExtractSource<TSource>(this IQueryable<TSource> source)
        {
            return (source as IQueryableCollection<TSource>)?.Source ?? throw new ArgumentException("Invalid query source.", nameof(source));
        }

        /// <summary>Determines whether a sequence contains any elements.</summary>
        /// <param name="source">A sequence to check for being empty.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static bool Any<TSource>(this IQueryable<TSource> source)
        {
            var items = ExtractSource(source);
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.Count > 0;
            return items.Any();
        }

        /// <param name="source"></param>
        /// <param name="element"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IQueryable<TSource> Append<TSource>(
            this IQueryable<TSource> source,
            TSource element)
        {
            var items = ExtractSource(source);
            return items.ListConcat(element.AsList()).AsQueryableCollection();
        }

        /// <summary>Concatenates two sequences.</summary>
        /// <param name="source1">The first sequence to concatenate.</param>
        /// <param name="source2">The sequence to concatenate to the first sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the concatenated elements of the two input sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Concat<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2)
        {
            var items = source1.ExtractSource();
            return items.ListConcat(source2).AsQueryableCollection();
        }

        /// <summary>Returns the number of elements in a sequence.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1"></see> that contains the elements to be counted.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>The number of elements in the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source">source</paramref> is larger than <see cref="F:System.Int32.MaxValue"></see>.</exception>
        public static int Count<TSource>(this IQueryable<TSource> source)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyCollection<TSource> collection)
                return collection.Count;
            return items.Count();
        }

        /// <summary>Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1"></see> to return a default value for if empty.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains default(<paramref name="TSource">TSource</paramref>) if <paramref name="source">source</paramref> is empty; otherwise, <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> DefaultIfEmpty<TSource>(this IQueryable<TSource> source)
        {
            return DefaultIfEmpty(source, default(TSource));
        }

        /// <summary>Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1"></see> to return the specified value for if empty.</param>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains <paramref name="defaultValue">defaultValue</paramref> if <paramref name="source">source</paramref> is empty; otherwise, <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> DefaultIfEmpty<TSource>(
            this IQueryable<TSource> source,
            TSource defaultValue)
        {
            var items = ExtractSource(source);
            if (items is IReadOnlyObservableList<TSource> list)
                return new ConditionalSwitchableListSource<TSource>(list.Observe(l => l.Count == 0),
                    defaultValue.AsList(), list).AsQueryableCollection();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return new ConditionalSwitchableCollectionSource<TSource>(collection.Observe(l => l.Count == 0),
                    defaultValue.AsList(), collection).AsQueryableCollection();
            return source.AsEnumerable().DefaultIfEmpty(defaultValue).AsQueryable();
        }

        /// <summary>Returns distinct elements from a sequence by using the default equality comparer to compare values.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1"></see> to remove duplicates from.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains distinct elements from <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> Distinct<TSource>(this IQueryable<TSource> source)
        {
            var items = ExtractSource(source);
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionDistinct().AsQueryableCollection();
            return items.AsEnumerable().Distinct().AsQueryable();
        }

        /// <summary>Returns distinct elements from a sequence by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</summary>
        /// <param name="source">The <see cref="T:System.Linq.IQueryable`1"></see> to remove duplicates from.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains distinct elements from <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IQueryable<TSource> Distinct<TSource>(
            this IQueryable<TSource> source,
            IEqualityComparer<TSource> comparer)
        {
            var items = ExtractSource(source);
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionDistinct(comparer).AsQueryableCollection();
            return items.AsEnumerable().Distinct(comparer).AsQueryable();
        }

        /// <summary>Returns the element at a specified index in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>The element at the specified position in <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index">index</paramref> is less than zero.</exception>
        public static TSource ElementAt<TSource>(this IQueryable<TSource> source, int index)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return list[index];
            return items.ElementAt(index);
        }

        /// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return an element from.</param>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>default(<paramref name="TSource">TSource</paramref>) if <paramref name="index">index</paramref> is outside the bounds of <paramref name="source">source</paramref>; otherwise, the element at the specified position in <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, int index)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return index >= 0 && index < list.Count ? list[index] : default(TSource);
            return items.ElementAt(index);
        }

        /// <summary>Produces the set difference of two sequences by using the default equality comparer to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements that are not also in source2 will be returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1"></see> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the set difference of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Except<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionExcept(source2).AsQueryableCollection();
            return items.Except(source2).AsQueryableCollection();
        }

        /// <summary>Produces the set difference of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements that are not also in source2 will be returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1"></see> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the set difference of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Except<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2,
            IEqualityComparer<TSource> comparer)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionExcept(source2, comparer).AsQueryableCollection();
            return items.Except(source2, comparer).AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <returns><p sourcefile="System.Linq.Queryable.yml" sourcestartlinenumber="1" sourceendlinenumber="1">An <code>IQueryable&lt;&gt;<_tkey2c_ tsource="">&gt;</_tkey2c_></code> in C# or <code>IQueryable(Of IGrouping(Of TKey, TSource))</code> in Visual Basic where each <xref href="System.Linq.IGrouping`2"></xref> object contains a sequence of objects and a key.</p>
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> is null.</exception>
        public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var items = source.ExtractSource();
            var factory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            return items.CollectionGroupByObservable(factory.Observe).AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <returns><p sourcefile="System.Linq.Queryable.yml" sourcestartlinenumber="1" sourceendlinenumber="1">An <code>IQueryable&lt;&gt;<_tkey2c_ tsource="">&gt;</_tkey2c_></code> in C# or <code>IQueryable(Of IGrouping(Of TKey, TSource))</code> in Visual Basic where each <xref href="System.Linq.IGrouping`2"></xref> contains a sequence of objects and a key.</p>
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IQueryable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var factory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            return items.CollectionGroupByObservable(factory.Observe, comparer).AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2"></see>.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"></see>.</typeparam>
        /// <returns><p sourcefile="System.Linq.Queryable.yml" sourcestartlinenumber="1" sourceendlinenumber="1">An <code>IQueryable&lt;&gt;<_tkey2c_ telement="">&gt;</_tkey2c_></code> in C# or <code>IQueryable(Of IGrouping(Of TKey, TElement))</code> in Visual Basic where each <xref href="System.Linq.IGrouping`2"></xref> contains a sequence of objects of type <code data-dev-comment-type="paramref">TElement</code> and a key.</p>
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="elementSelector">elementSelector</paramref> is null.</exception>
        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var selectorFactory = PropertyObservableFactory<TSource, TElement>.FindIn(elementSelector);
            var groupComparer = new GroupingComparer<TKey, TElement>();
            return items.CollectionGroupByObservable(keyFactory.Observe)
                .ListSelect(g => 
                    new TransformedReadOnlyObservableGroup<TSource, TKey, TElement>(g.Key, g, selectorFactory.Observe),
                    targetComparer: groupComparer)
                .AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence and projects the elements for each group by using a specified function. Key values are compared by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2"></see>.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"></see>.</typeparam>
        /// <returns><p sourcefile="System.Linq.Queryable.yml" sourcestartlinenumber="1" sourceendlinenumber="1">An <code>IQueryable&lt;&gt;<_tkey2c_ telement="">&gt;</_tkey2c_></code> in C# or <code>IQueryable(Of IGrouping(Of TKey, TElement))</code> in Visual Basic where each <xref href="System.Linq.IGrouping`2"></xref> contains a sequence of objects of type <code data-dev-comment-type="paramref">TElement</code> and a key.</p>
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="elementSelector">elementSelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IQueryable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var selectorFactory = PropertyObservableFactory<TSource, TElement>.FindIn(elementSelector);
            var groupComparer = new GroupingComparer<TKey, TElement>(comparer);
            return items.CollectionGroupByObservable(keyFactory.Observe, comparer)
                .ListSelect(g =>
                    new TransformedReadOnlyObservableGroup<TSource, TKey, TElement>(g.Key, g, selectorFactory.Observe),
                    targetComparer: groupComparer)
                .AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult">TResult</paramref> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var selectorFactory = resultSelector.FindProperties();
            return items.CollectionGroupByObservable(keyFactory.Observe)
                .ListSelectObservable(g => selectorFactory.Observe(g.Key, g))
                .AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Keys are compared by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult">TResult</paramref> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TKey, IEnumerable<TSource>, TResult>> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var selectorFactory = resultSelector.FindProperties();
            return items.CollectionGroupByObservable(keyFactory.Observe, comparer)
                .ListSelectObservable(g => selectorFactory.Observe(g.Key, g))
                .AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2"></see>.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"></see>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult">TResult</paramref> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="elementSelector">elementSelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var elementFactory = PropertyObservableFactory<TSource, TElement>.FindIn(elementSelector);
            var resultFactory = resultSelector.FindProperties();
            var groupComparer = new GroupingComparer<TKey, TElement>();
            return items.CollectionGroupByObservable(keyFactory.Observe)
                .ListSelect(g =>
                        new TransformedReadOnlyObservableGroup<TSource, TKey, TElement>(g.Key, g, elementFactory.Observe),
                    targetComparer: groupComparer)
                .ListSelectObservable(g => resultFactory.Observe(g.Key, g))
                .AsQueryableCollection();
        }

        /// <summary>Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Keys are compared by using a specified comparer and the elements of each group are projected by using a specified function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <param name="elementSelector">A function to map each source element to an element in an <see cref="T:System.Linq.IGrouping`2"></see>.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented in keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="T:System.Linq.IGrouping`2"></see>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by resultSelector.</typeparam>
        /// <returns>An T:System.Linq.IQueryable`1 that has a type argument of <paramref name="TResult">TResult</paramref> and where each element represents a projection over a group and its key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="elementSelector">elementSelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IQueryable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            Expression<Func<TKey, IEnumerable<TElement>, TResult>> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var keyFactory = PropertyObservableFactory<TSource, TKey>.FindIn(keySelector);
            var elementFactory = PropertyObservableFactory<TSource, TElement>.FindIn(elementSelector);
            var resultFactory = resultSelector.FindProperties();
            var groupComparer = new GroupingComparer<TKey, TElement>(comparer);
            return items.CollectionGroupByObservable(keyFactory.Observe, comparer)
                .ListSelect(g =>
                        new TransformedReadOnlyObservableGroup<TSource, TKey, TElement>(g.Key, g, elementFactory.Observe),
                    targetComparer: groupComparer)
                .ListSelectObservable(g => resultFactory.Observe(g.Key, g))
                .AsQueryableCollection();
        }

        /// <summary>Correlates the elements of two sequences based on key equality and groups the results. The default equality comparer is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements of type <paramref name="TResult">TResult</paramref> obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="outer">outer</paramref> or <paramref name="inner">inner</paramref> or <paramref name="outerKeySelector">outerKeySelector</paramref> or <paramref name="innerKeySelector">innerKeySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector)
        {
            throw new NotImplementedException();
        }

        /// <summary>Correlates the elements of two sequences based on key equality and groups the results. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <param name="comparer">A comparer to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements of type <paramref name="TResult">TResult</paramref> obtained by performing a grouped join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="outer">outer</paramref> or <paramref name="inner">inner</paramref> or <paramref name="outerKeySelector">outerKeySelector</paramref> or <paramref name="innerKeySelector">innerKeySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, IEnumerable<TInner>, TResult>> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            throw new NotImplementedException();
        }

        /// <summary>Produces the set intersection of two sequences by using the default equality comparer to compare values.</summary>
        /// <param name="source1">A sequence whose distinct elements that also appear in source2 are returned.</param>
        /// <param name="source2">A sequence whose distinct elements that also appear in the first sequence are returned.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>A sequence that contains the set intersection of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Intersect<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionIntersect(source2).AsQueryableCollection();
            return items.Intersect(source2).AsQueryableCollection();
        }

        /// <summary>Produces the set intersection of two sequences by using the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</summary>
        /// <param name="source1">An <see cref="T:System.Linq.IQueryable`1"></see> whose distinct elements that also appear in source2 are returned.</param>
        /// <param name="source2">An <see cref="T:System.Collections.Generic.IEnumerable`1"></see> whose distinct elements that also appear in the first sequence are returned.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the set intersection of the two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Intersect<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2,
            IEqualityComparer<TSource> comparer)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionIntersect(source2, comparer).AsQueryableCollection();
            return items.Intersect(source2, comparer).AsQueryableCollection();
        }

        /// <summary>Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that has elements of type <paramref name="TResult">TResult</paramref> obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="outer">outer</paramref> or <paramref name="inner">inner</paramref> or <paramref name="outerKeySelector">outerKeySelector</paramref> or <paramref name="innerKeySelector">innerKeySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            var items = outer.ExtractSource();
            var outerKeyFactory = PropertyObservableFactory<TOuter, TKey>.FindIn(outerKeySelector);
            var innerKeyFactory = PropertyObservableFactory<TInner, TKey>.FindIn(innerKeySelector);
            var resultFactory = PropertyObservableFactory<TOuter, TInner, TResult>.FindIn(resultSelector);
            return items.CollectionJoinObservable(inner,
                    outerKeyFactory.Observe, innerKeyFactory.Observe, resultFactory.Observe)
                .AsQueryableCollection();
        }

        /// <summary>Correlates the elements of two sequences based on matching keys. A specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> is used to compare keys.</summary>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to hash and compare keys.</param>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that has elements of type <paramref name="TResult">TResult</paramref> obtained by performing an inner join on two sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="outer">outer</paramref> or <paramref name="inner">inner</paramref> or <paramref name="outerKeySelector">outerKeySelector</paramref> or <paramref name="innerKeySelector">innerKeySelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> Join<TOuter, TInner, TKey, TResult>(
            this IQueryable<TOuter> outer,
            IEnumerable<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            IEqualityComparer<TKey> comparer)
        {
            var items = outer.ExtractSource();
            var outerKeyFactory = PropertyObservableFactory<TOuter, TKey>.FindIn(outerKeySelector);
            var innerKeyFactory = PropertyObservableFactory<TInner, TKey>.FindIn(innerKeySelector);
            var resultFactory = PropertyObservableFactory<TOuter, TInner, TResult>.FindIn(resultSelector);
            return items.CollectionJoinObservable(inner,
                    outerKeyFactory.Observe, innerKeyFactory.Observe, resultFactory.Observe, comparer)
                .AsQueryableCollection();
        }

        /// <summary>Returns the last element in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return the last element of.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>The value at the last position in <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
        public static TSource Last<TSource>(this IQueryable<TSource> source)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return list[list.Count - 1];
            return items.Last();
        }

        /// <summary>Returns the last element in a sequence, or a default value if the sequence contains no elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return the last element of.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>default(<paramref name="TSource">TSource</paramref>) if <paramref name="source">source</paramref> is empty; otherwise, the last element in <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static TSource LastOrDefault<TSource>(this IQueryable<TSource> source)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return list.Count == 0 ? default(TSource) : list[list.Count - 1];
            return items.LastOrDefault();
        }

        /// <summary>Returns an <see cref="T:System.Int64"></see> that represents the total number of elements in a sequence.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> that contains the elements to be counted.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>The number of elements in <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        /// <exception cref="T:System.OverflowException">The number of elements exceeds <see cref="F:System.Int64.MaxValue"></see>.</exception>
        public static long LongCount<TSource>(this IQueryable<TSource> source)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyCollection<TSource> collection)
                return collection.Count;
            return items.LongCount();
        }

        /// <summary>Sorts the elements of a sequence in ascending order according to a key.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var items = source.ExtractSource();
            var definition = new Definition<TSource, TKey>(keySelector);
            return new KeySortingQueryableCollection<TSource>(items, definition);
        }

        /// <summary>Sorts the elements of a sequence in ascending order by using a specified comparer.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var definition = new Definition<TSource, TKey>(keySelector, comparer);
            return new KeySortingQueryableCollection<TSource>(items, definition);
        }

        /// <summary>Sorts the elements of a sequence in descending order according to a key.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var items = source.ExtractSource();
            var comparer = new InvertedComparer<TKey>(null);
            var definition = new Definition<TSource, TKey>(keySelector, comparer);
            return new KeySortingQueryableCollection<TSource>(items, definition);
        }

        /// <summary>Sorts the elements of a sequence in descending order by using a specified comparer.</summary>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IComparer<TKey> comparer)
        {
            var items = source.ExtractSource();
            var newComparer = new InvertedComparer<TKey>(comparer);
            var definition = new Definition<TSource, TKey>(keySelector, newComparer);
            return new KeySortingQueryableCollection<TSource>(items, definition);
        }

        /// <param name="source"></param>
        /// <param name="element"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IQueryable<TSource> Prepend<TSource>(
            this IQueryable<TSource> source,
            TSource element)
        {
            var items = source.ExtractSource();
            return element.AsList().ListConcat(items).AsQueryableCollection();
        }

        /// <summary>Inverts the order of the elements in a sequence.</summary>
        /// <param name="source">A sequence of values to reverse.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements correspond to those of the input sequence in reverse order.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> Reverse<TSource>(this IQueryable<TSource> source)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyObservableList<TSource> list)
                return list.ListReverse().AsQueryableCollection();
            return items.Reverse().AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence into a new form.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by selector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking a projection function on each element of <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="selector">selector</paramref> is null.</exception>
        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            var items = source.ExtractSource();
            var factory = PropertyObservableFactory<TSource, TResult>.FindIn(selector);
            return items.ListSelectObservable(factory.Observe).AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by selector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking a projection function on each element of <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="selector">selector</paramref> is null.</exception>
        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, TResult>> selector)
        {
            var items = source.ExtractSource();
            var factory = selector.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
            return indexed.ListSelectObservable(
                    item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                .AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1"></see> and combines the resulting sequences into one sequence.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by selector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="selector">selector</paramref> is null.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, IEnumerable<TResult>>> selector)
        {
            var items = source.ExtractSource();
            var factory = PropertyObservableFactory<TSource, IEnumerable<TResult>>.FindIn(selector);
            return items.ListSelectManyObservable(factory.Observe).AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1"></see> and combines the resulting sequences into one sequence. The index of each source element is used in the projected form of that element.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element; the second parameter of this function represents the index of the source element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by selector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="selector">selector</paramref> is null.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, IEnumerable<TResult>>> selector)
        {
            var items = source.ExtractSource();
            var factory = selector.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
            return indexed.ListSelectManyObservable(
                    item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                .AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1"></see> and invokes a result selector function on each element therein. The resulting values from each intermediate sequence are combined into a single, one-dimensional sequence and returned.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector">A projection function to apply to each element of each intermediate sequence.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by collectionSelector.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector">collectionSelector</paramref> on each element of <paramref name="source">source</paramref> and then mapping each of those sequence elements and their corresponding <paramref name="source">source</paramref> element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="collectionSelector">collectionSelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector,
            Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            var items = source.ExtractSource();
            var collectionFactory = PropertyObservableFactory<TSource, IEnumerable<TCollection>>.FindIn(collectionSelector);
            var resultFactory = resultSelector.FindProperties();
            return items.ListSelectManyObservable(item => collectionFactory.Observe(item).Select(l => l.Select(x => Tuple.Create(item, x))))
                .ListSelectObservable(t => resultFactory.Observe(t.Item1, t.Item2))
                .AsQueryableCollection();
        }

        /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1"></see> that incorporates the index of the source element that produced it. A result selector function is invoked on each element of each intermediate sequence, and the resulting values are combined into a single, one-dimensional sequence and returned.</summary>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="collectionSelector">A projection function to apply to each element of the input sequence; the second parameter of this function represents the index of the source element.</param>
        /// <param name="resultSelector">A projection function to apply to each element of each intermediate sequence.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by collectionSelector.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector">collectionSelector</paramref> on each element of <paramref name="source">source</paramref> and then mapping each of those sequence elements and their corresponding <paramref name="source">source</paramref> element to a result element.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="collectionSelector">collectionSelector</paramref> or <paramref name="resultSelector">resultSelector</paramref> is null.</exception>
        public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector,
            Expression<Func<TSource, TCollection, TResult>> resultSelector)
        {
            var items = source.ExtractSource();
            var collectionFactory = collectionSelector.FindProperties();
            var resultFactory = resultSelector.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, collectionFactory.FirstTriggers));
            return indexed.ListSelectManyObservable(
                    item => item.ObserveAll().Select(x => collectionFactory.Function(x.Value, x.Index)).Select(l => l.Select(x => Tuple.Create(item.Item, x))))
                .ListSelectObservable(t => resultFactory.Observe(t.Item1, t.Item2))
                .AsQueryableCollection();
        }

        /// <summary>Bypasses a specified number of elements in a sequence and then returns the remaining elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return elements from.</param>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements that occur after the specified index in the input sequence.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, int count)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyObservableList<TSource> list)
                return list.ListSkip(count).AsQueryableCollection();
            return items.Skip(count).AsQueryableCollection();
        }

        /// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from <paramref name="source">source</paramref> starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate">predicate</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> SkipWhile<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return list.ListSkipWhileAuto(predicate).AsQueryableCollection();
            return items.SkipWhile(predicate.Compile()).AsQueryableCollection();
        }

        /// <summary>Bypasses elements in a sequence as long as a specified condition is true and then returns the remaining elements. The element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of this function represents the index of the source element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from <paramref name="source">source</paramref> starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate">predicate</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> SkipWhile<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, bool>> predicate)
        {
            var items = source.ExtractSource();
            var factory = predicate.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
            return indexed.ListSkipWhileObservable(
                    item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                .ListSelect(x => x.Item)
                .AsQueryableCollection();
        }

        /// <summary>Returns a specified number of contiguous elements from the start of a sequence.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the specified number of elements from the start of <paramref name="source">source</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> is null.</exception>
        public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, int count)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyObservableList<TSource> list)
                return list.ListTake(count).AsQueryableCollection();
            return items.Skip(count).AsQueryableCollection();
        }

        /// <summary>Returns elements from a sequence as long as a specified condition is true.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate">predicate</paramref> no longer passes.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> TakeWhile<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate)
        {
            var items = source.ExtractSource();
            if (items is IReadOnlyList<TSource> list)
                return list.ListTakeWhileAuto(predicate).AsQueryableCollection();
            return items.TakeWhile(predicate.Compile()).AsQueryableCollection();
        }

        /// <summary>Returns elements from a sequence as long as a specified condition is true. The element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">The sequence to return elements from.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element in the source sequence.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate">predicate</paramref> no longer passes.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> TakeWhile<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, bool>> predicate)
        {
            var items = source.ExtractSource();
            var factory = predicate.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
            return indexed.ListTakeWhileObservable(
                    item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                .ListSelect(x => x.Item)
                .AsQueryableCollection();
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1"></see> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var sortedCollection = source as KeySortingQueryableCollection<TSource>
                                   ?? throw new ArgumentException("Invalid source", nameof(source));
            var definition = new Definition<TSource, TKey>(keySelector);
            return new KeySortingQueryableCollection<TSource>(sortedCollection, definition);
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in ascending order by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1"></see> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IComparer<TKey> comparer)
        {
            var sortedCollection = source as KeySortingQueryableCollection<TSource>
                                   ?? throw new ArgumentException("Invalid source", nameof(source));
            var definition = new Definition<TSource, TKey>(keySelector, comparer);
            return new KeySortingQueryableCollection<TSource>(sortedCollection, definition);
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1"></see> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by the function represented by keySelector.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1"></see> whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            var sortedCollection = source as KeySortingQueryableCollection<TSource>
                                   ?? throw new ArgumentException("Invalid source", nameof(source));
            var newComparer = new InvertedComparer<TKey>(null);
            var definition = new Definition<TSource, TKey>(keySelector, newComparer);
            return new KeySortingQueryableCollection<TSource>(sortedCollection, definition);
        }

        /// <summary>Performs a subsequent ordering of the elements in a sequence in descending order by using a specified comparer.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IOrderedQueryable`1"></see> that contains elements to sort.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IComparer`1"></see> to compare keys.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key that is returned by the keySelector function.</typeparam>
        /// <returns>A collection whose elements are sorted in descending order according to a key.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="keySelector">keySelector</paramref> or <paramref name="comparer">comparer</paramref> is null.</exception>
        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedQueryable<TSource> source,
            Expression<Func<TSource, TKey>> keySelector,
            IComparer<TKey> comparer)
        {
            var sortedCollection = source as KeySortingQueryableCollection<TSource>
                                   ?? throw new ArgumentException("Invalid source", nameof(source));
            var newComparer = new InvertedComparer<TKey>(comparer);
            var definition = new Definition<TSource, TKey>(keySelector, newComparer);
            return new KeySortingQueryableCollection<TSource>(sortedCollection, definition);
        }

        /// <summary>Produces the set union of two sequences by using the default equality comparer.</summary>
        /// <param name="source1">A sequence whose distinct elements form the first set for the union operation.</param>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Union<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionUnion(source2).AsQueryableCollection();
            return items.Union(source2).AsQueryableCollection();
        }

        /// <summary>Produces the set union of two sequences by using a specified <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see>.</summary>
        /// <param name="source1">A sequence whose distinct elements form the first set for the union operation.</param>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"></see> to compare values.</param>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains the elements from both input sequences, excluding duplicates.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TSource> Union<TSource>(
            this IQueryable<TSource> source1,
            IEnumerable<TSource> source2,
            IEqualityComparer<TSource> comparer)
        {
            var items = source1.ExtractSource();
            if (items is IReadOnlyObservableCollection<TSource> collection)
                return collection.CollectionUnion(comparer, source2).AsQueryableCollection();
            return items.Union(source2, comparer).AsQueryableCollection();
        }

        /// <summary>Filters a sequence of values based on a predicate.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate">predicate</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate)
        {
            var items = source.ExtractSource();
            var factory = PropertyObservableFactory<TSource, bool>.FindIn(predicate);
            return items.ListWhereObservable(factory.Observe).AsQueryableCollection();
        }

        /// <summary>Filters a sequence of values based on a predicate. Each element's index is used in the logic of the predicate function.</summary>
        /// <param name="source">An <see cref="T:System.Linq.IQueryable`1"></see> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition; the second parameter of the function represents the index of the element in the source sequence.</param>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate">predicate</paramref>.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source">source</paramref> or <paramref name="predicate">predicate</paramref> is null.</exception>
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source,
            Expression<Func<TSource, int, bool>> predicate)
        {
            var items = source.ExtractSource();
            var factory = predicate.FindProperties();
            var indexed = new IndexedTransformingReadOnlyObservableList<TSource, TSource>(items,
                x => new NotifyObservable<TSource>(x, factory.FirstTriggers));
            return indexed.ListWhereObservable(
                    item => item.ObserveAll().Select(x => factory.Function(x.Value, x.Index)))
                .ListSelect(x => x.Item)
                .AsQueryableCollection();
        }

        /// <summary>Merges two sequences by using the specified predicate function.</summary>
        /// <param name="source1">The first sequence to merge.</param>
        /// <param name="source2">The second sequence to merge.</param>
        /// <param name="resultSelector">A function that specifies how to merge the elements from the two sequences.</param>
        /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
        /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <returns>An <see cref="T:System.Linq.IQueryable`1"></see> that contains merged elements of two input sequences.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="source1">source1</paramref> or <paramref name="source2">source2</paramref> is null.</exception>
        public static IQueryable<TResult> Zip<TFirst, TSecond, TResult>(
            this IQueryable<TFirst> source1,
            IEnumerable<TSecond> source2,
            Expression<Func<TFirst, TSecond, TResult>> resultSelector)
        {
            var items = source1.ExtractSource();
            var factory = resultSelector.FindProperties();
            return items.ListAsObservable().ListZipObservable(source2.ListAsObservable(), factory.Observe).AsQueryableCollection();
        }
    }
}