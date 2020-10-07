using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Neuronic.CollectionModel.Collections;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Collections.Linq;
using Neuronic.CollectionModel.Extras;
using Neuronic.CollectionModel.Observables;
using Neuronic.CollectionModel.Results;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Provides a set of extension methods for collections and sequences.
    /// </summary>
    public static class CollectionExtensions
    {
        #region Internal Operations
        /// <summary>
        ///     Updates the specified observable collection.
        /// </summary>
        /// <remarks>
        ///     This method can be used to synchronize two observable collections, calling this
        ///     method in the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </remarks>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="list">The collection to update.</param>
        /// <param name="source">
        ///     The source collection. This is usually the <strong>sender</strong>
        ///     parameter in the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </param>
        /// <param name="e">
        ///     The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data,
        ///     obtained from the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </param>
        /// <param name="select">
        ///     A function that can be used to create new items for <paramref name="list" />
        ///     based on the items in <paramref name="source" />. By default, the same items are used.
        /// </param>
        /// <param name="onRemove">
        ///     A callback procedure that can be used to free resources when items
        ///     are removed from <paramref name="list" />.
        /// </param>
        /// <param name="comparer">
        ///     A comparer for the list items. This is only used if the source collection is not a list
        ///     and does not provide index information in <paramref name="e"/>. If this parameter is <c>null</c>,
        ///     <see cref="EqualityComparer{T}.Default"/> is used.
        /// </param>
        public static void UpdateCollection<T>(this ObservableCollection<T> list, IEnumerable source,
            NotifyCollectionChangedEventArgs e,
            Func<object, T> select = null, Action<T> onRemove = null,
            IEqualityComparer<T> comparer = null)
        {
            select = select ?? (o => (T)o);
            // apply the change to the snapshot
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if ((e.NewStartingIndex < 0) || (list.Count <= e.NewStartingIndex))
                        foreach (var item in e.NewItems)
                            list.Add(select(item));
                    else
                        for (var i = e.NewItems.Count - 1; i >= 0; --i) // insert
                            list.Insert(e.NewStartingIndex, select(e.NewItems[i]));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 0)
                        RemoveItems(list, e.OldItems, comparer, select, onRemove);
                    else
                        for (int i = e.OldItems.Count - 1, index = e.OldStartingIndex + i; i >= 0; --i, --index)
                        {
                            var item = list[index];
                            list.RemoveAt(index);
                            onRemove?.Invoke(item);
                        }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewStartingIndex != e.OldStartingIndex)
                        throw new InvalidOperationException("Old and new indexes mismatch on replace.");
                    if (e.NewStartingIndex < 0)
                        ReplaceItems(list, e.OldItems, e.NewItems, comparer, select, onRemove);
                    else
                        for (int i = e.NewItems.Count - 1, index = e.NewStartingIndex + i; i >= 0; --i, --index)
                        {
                            var item = list[index];
                            list[index] = select(e.NewItems[i]);
                            onRemove?.Invoke(item);
                        }
                    break;

                case NotifyCollectionChangedAction.Move:
                    if (e.NewStartingIndex < 0)
                        throw new InvalidOperationException("The index cannot be a negative value.");

                    if (e.OldStartingIndex < e.NewStartingIndex)
                        for (int i = e.OldItems.Count - 1,
                            oldIndex = e.OldStartingIndex + i,
                            newIndex = e.NewStartingIndex + i;
                            i >= 0;
                            --i, --oldIndex, --newIndex)
                            list.Move(oldIndex, newIndex);
                    else
                        for (int i = 0,
                            oldIndex = e.OldStartingIndex + i,
                            newIndex = e.NewStartingIndex + i;
                            i < e.OldItems.Count;
                            ++i, ++oldIndex, ++newIndex)
                            list.Move(oldIndex, newIndex);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (onRemove != null)
                        foreach (var item in list)
                            onRemove(item);
                    list.Clear();
                    foreach (var item in source)
                        list.Add(select(item));
                    break;
            }
        }

        /// <summary>
        ///     Updates the specified observable collection.
        /// </summary>
        /// <remarks>
        ///     This method can be used to synchronize two observable collections, calling this
        ///     method in the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </remarks>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="collection">The collection to update.</param>
        /// <param name="source">
        ///     The source collection. This is usually the <strong>sender</strong>
        ///     parameter in the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </param>
        /// <param name="e">
        ///     The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data,
        ///     obtained from the <see cref="INotifyCollectionChanged.CollectionChanged" /> event handler.
        /// </param>
        /// <param name="select">
        ///     A function that can be used to create new items for <paramref name="list" />
        ///     based on the items in <paramref name="source" />. By default, the same items are used.
        /// </param>
        /// <param name="onRemove">
        ///     A callback procedure that can be used to free resources when items
        ///     are removed from <paramref name="collection" />.
        /// </param>
        /// <param name="comparer">
        ///     A comparer for the list items. This is only used if the source collection is not a list
        ///     and does not provide index information in <paramref name="e"/>. If this parameter is <c>null</c>,
        ///     <see cref="EqualityComparer{T}.Default"/> is used.
        /// </param>
        public static void UpdateCollection<T>(this ICollection<T> collection, IEnumerable source,
            NotifyCollectionChangedEventArgs e,
            Func<object, T> select = null, Action<T> onRemove = null,
            IEqualityComparer<T> comparer = null)
        {
            if (collection is ObservableCollection<T> oc)
            {
                UpdateCollection(oc, source, e, select, onRemove, comparer);
                return;
            }

            select = select ?? (o => (T)o);
            var list = collection as IList<T>;
            // apply the change to the snapshot
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex >= 0 && (collection.Count > e.NewStartingIndex) && list != null)
                        for (var i = e.NewItems.Count - 1; i >= 0; --i) // insert
                            list.Insert(e.NewStartingIndex, @select(e.NewItems[i]));
                    else
                        foreach (var item in e.NewItems)
                            collection.Add(@select(item));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex >= 0 && list != null)
                        for (int i = e.OldItems.Count - 1, index = e.OldStartingIndex + i; i >= 0; --i, --index)
                        {
                            var item = list[index];
                            list.RemoveAt(index);
                            onRemove?.Invoke(item);
                        }
                    else
                        RemoveItems(collection, e.OldItems, comparer, @select, onRemove);

                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.NewStartingIndex != e.OldStartingIndex)
                        throw new InvalidOperationException("Old and new indexes mismatch on replace.");
                    if (e.NewStartingIndex >= 0 && list != null)
                        for (int i = e.NewItems.Count - 1, index = e.NewStartingIndex + i; i >= 0; --i, --index)
                        {
                            var item = list[index];
                            list[index] = @select(e.NewItems[i]);
                            onRemove?.Invoke(item);
                        }
                    else
                        ReplaceItems(collection, e.OldItems, e.NewItems, comparer, @select, onRemove);

                    break;

                case NotifyCollectionChangedAction.Move when list != null:
                    if (e.NewStartingIndex < 0)
                        throw new InvalidOperationException("The index cannot be a negative value.");

                    if (e.OldStartingIndex < e.NewStartingIndex)
                        for (int i = e.OldItems.Count - 1,
                            oldIndex = e.OldStartingIndex + i,
                            newIndex = e.NewStartingIndex + i;
                            i >= 0;
                            --i, --oldIndex, --newIndex)
                        {
                            var item = list[oldIndex];
                            list.RemoveAt(oldIndex);
                            onRemove?.Invoke(item);
                            item = @select(e.NewItems[i]);
                            list.Insert(newIndex, item);
                        }
                    else
                        for (int i = 0,
                            oldIndex = e.OldStartingIndex + i,
                            newIndex = e.NewStartingIndex + i;
                            i < e.OldItems.Count;
                            ++i, ++oldIndex, ++newIndex)
                        {
                            var item = list[oldIndex];
                            list.RemoveAt(oldIndex);
                            onRemove?.Invoke(item);
                            item = @select(e.NewItems[i]);
                            list.Insert(newIndex, item);
                        }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    if (onRemove != null)
                        foreach (var item in collection)
                            onRemove(item);
                    collection.Clear();
                    foreach (var item in source)
                        collection.Add(select(item));
                    break;
            }
        }

        private static void RemoveItems<T>(this ICollection<T> collection, IEnumerable oldItems,
            IEqualityComparer<T> comparer, Func<object, T> select, Action<T> onRemove)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            select = select ?? (o => (T)o);
            var setToRemove = new HashSet<T>(oldItems.Cast<object>().Select(select), comparer);
            if (collection is IList<T> list)
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var item = list[i];
                    if (setToRemove.Contains(item))
                    {
                        onRemove?.Invoke(item);
                        list.RemoveAt(i);
                    }
                }
            else
                foreach (var item in setToRemove)
                    if (collection.Remove(item))
                        onRemove?.Invoke(item);
        }

        private static void ReplaceItems<T>(this ICollection<T> collection, IEnumerable oldItems, IEnumerable newItems,
            IEqualityComparer<T> comparer, Func<object, T> select, Action<T> onRemove)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            select = select ?? (o => (T)o);
            var setToRemove = new HashSet<T>(oldItems.Cast<object>().Select(select),
                comparer);

            var newItemsEnumerator = newItems.GetEnumerator();
            var newItemsLeft = newItemsEnumerator.MoveNext();
            if (collection is IList<T> list)
            {
                for (int i = 0; i < list.Count && newItemsLeft; i++)
                {
                    var oldItem = list[i];
                    if (setToRemove.Contains(oldItem))
                    {
                        var newItem = select(newItemsEnumerator.Current);
                        newItemsLeft = newItemsEnumerator.MoveNext();
                        list[i] = newItem;
                        onRemove?.Invoke(oldItem);
                    }
                } 
            }
            else
                foreach (var oldItem in setToRemove.TakeWhile(_ => newItemsLeft))
                    if (collection.Remove(oldItem))
                    {
                        var newItem = select(newItemsEnumerator.Current);
                        newItemsLeft = newItemsEnumerator.MoveNext();
                        collection.Add(newItem);
                        onRemove?.Invoke(oldItem);
                    }
        }

        /// <summary>
        ///     Replaces the specified items in the specified list.
        /// </summary>
        /// <typeparam name="T">Type of the collection elements</typeparam>
        /// <param name="list">The list to update.</param>
        /// <param name="oldItems">The replaced items.</param>
        /// <param name="newItems">The new items.</param>
        /// <param name="index">The starting index.</param>
        /// <param name="select">
        ///     A function that can be used to create new items for <paramref name="list" />
        ///     based on the items in <paramref name="oldItems" /> and <paramref name="newItems" />. By default, the same items are
        ///     used.
        /// </param>
        /// <param name="onRemove">
        ///     A callback procedure that can be used to free resources when items
        ///     are removed from <paramref name="list" />.
        /// </param>
        internal static void ReplaceItems<T>(this IList<T> list, IEnumerable oldItems, IEnumerable newItems, int index,
            Func<object, T> select = null, Action<T> onRemove = null)
        {
            select = select ?? (o => (T)o);
            bool thereIsOldItems, thereIsNewItems;
            IEnumerator oldItemsEnumerator = oldItems.GetEnumerator(), newItemsEnumerator = newItems.GetEnumerator();
            while ((thereIsNewItems = newItemsEnumerator.MoveNext()) &
                   (thereIsOldItems = oldItemsEnumerator.MoveNext()))
            {
                var oldItem = list[index];
                list[index++] = select(newItemsEnumerator.Current);
                onRemove?.Invoke(oldItem);
            }
            while (thereIsNewItems)
            {
                list.Insert(index++, select(newItemsEnumerator.Current));
                thereIsNewItems = newItemsEnumerator.MoveNext();
            }
            while (thereIsOldItems)
            {
                var oldItem = list[index];
                list.RemoveAt(index);
                onRemove?.Invoke(oldItem);
                thereIsOldItems = oldItemsEnumerator.MoveNext();
            }
        }
        #endregion

        /// <summary>
        ///     Casts the boolean result, enabling to use boolean operators.
        /// </summary>
        /// <param name="result">The source operation result.</param>
        /// <returns>The casted operation result.</returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static BooleanObservableResult Cast(this IObservableResult<bool> result)
        {
            return result as BooleanObservableResult ?? new BooleanObservableResult(result);
        }

        /// <summary>
        ///     Tries to select the specified item in the selector.
        /// </summary>
        /// <typeparam name="T">Type of the selector's elements.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <param name="item">The item to select.</param>
        /// <returns>true, if the item was found; otherwise, false.</returns>
        public static bool TrySelect<T>(this ICollectionSelector<T> selector, T item)
        {
            var list = selector.Items as IList;
            var index = list?.IndexOf(item) ?? selector.Items.IndexOf(item);
            if (index < 0)
                return false;
            selector.SelectedIndex = index;
            return true;
        }

        /// <summary>
        ///     Treats the collection as an <see cref="IQueryable{T}"/> whose LINQ extension methods
        ///     have the same effects as their counterpart collection extensions defined in this
        ///     class.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns>The LINQ queryable.</returns>
        public static IQueryable<T> AsQueryableCollection<T>(this IEnumerable<T> items)
        {
            return new QueryableCollection<T>(items);
        }

        /// <summary>
        ///     Creates an observable view of a normal read-only list.
        /// </summary>
        /// <typeparam name="T">The type of the list's elements.</typeparam>
        /// <param name="items">The list.</param>
        /// <returns></returns>
        public static IReadOnlyObservableList<T> ListAsObservable<T>(this IEnumerable<T> items)
        {
            switch (items)
            {
                case IReadOnlyObservableList<T> self:
                    return self;
                case IQueryableCollection<T> queryable:
                    return queryable.Source.ListAsObservable();
                case ObservableCollection<T> observableCollection:
                    return new ReadOnlyObservableList<T>(observableCollection);
                case IReadOnlyList<T> list:
                    return new CustomReadOnlyObservableList<T>(list);
                default:
                    return new ListWrapper<T>(items);
            }
        }

        /// <summary>
        ///     Creates an observable view of a normal read-only collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection's elements.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns></returns>
        public static IReadOnlyObservableCollection<T> CollectionAsObservable<T>(this IEnumerable<T> items)
        {
            switch (items)
            {
                case IReadOnlyObservableCollection<T> self:
                    return self;
                case IQueryableCollection<T> queryable:
                    return queryable.Source.CollectionAsObservable();
                case ObservableCollection<T> observableCollection:
                    return new ReadOnlyObservableList<T>(observableCollection);
                case IReadOnlyCollection<T> collection:
                    return new CustomReadOnlyObservableCollection<T>(collection);
                default:
                    return new CollectionWrapper<T>(items);
            }
        }

        /// <summary>
        ///     Creates an observable list that contains a single item.
        /// </summary>
        /// <typeparam name="T">Type type of the collection's element.</typeparam>
        /// <param name="item">The single item.</param>
        /// <returns>An observable list that contains only <paramref name="item"/>.</returns>
        public static IReadOnlyObservableList<T> ObservableAsList<T>(this IObservable<T> item)
        {
            return new SingleItemObservableList<T>(item);
        }
        
        /// <summary>
        ///     Creates an observable list that contains a single item.
        /// </summary>
        /// <typeparam name="T">Type type of the collection's element.</typeparam>
        /// <param name="item">The single item.</param>
        /// <returns>An observable list that contains only <paramref name="item"/>.</returns>
        public static IReadOnlyObservableList<T> AsList<T>(this T item)
        {
            return new SingleItemObservableList<T>(item);
        }

        /// <summary>
        ///     Creates an observable list by from an observable collection. 
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="comparer">
        ///     The item comparer. Only needed if the collection does not provide index information in it's events.
        ///     If it is <c>null</c>, the default comparer will be used.
        /// </param>
        /// <returns>An indexable wrapper of <paramref name="collection"/>.</returns>
        public static IReadOnlyObservableList<T> ListFromCollection<T>(this IReadOnlyObservableCollection<T> collection,
            IEqualityComparer<T> comparer = null)
        {
            return collection as IReadOnlyObservableList<T> ??
                   new TransformingReadOnlyObservableList<T, T>(collection, x => x, targetComparer: comparer);
        }

        /// <summary>
        ///     Creates a list proxy that can be used to manually refresh the lists that depend on it.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="list">The source list.</param>
        /// <returns>A list that can be reseted from external code.</returns>
        public static IManualReadOnlyObservableList<T> ListAsManual<T>(this IReadOnlyObservableList<T> list)
        {
            return new TransactionalReadOnyObservableList<T>(list);
        }

        /// <summary>
        ///     Creates a collection proxy that can be used to manually refresh the collections that depend on it.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <returns>A collection that can be reseted from external code.</returns>
        public static IManualReadOnlyObservableCollection<T> CollectionAsManual<T>(
            this IReadOnlyObservableCollection<T> collection)
        {
            return new ManualReadOnlyObservableCollection<T>(collection);
        }

        /// <summary>
        ///     Creates a list proxy that can be used to optimize performance when performing multiple
        ///     modifications to a source list.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="list">The source list.</param>
        /// <returns>The list proxy.</returns>
        /// <remarks>
        ///     <para>
        ///         During a transaction, the notifications corresponding to the modifications in the source
        ///         collections will not reach the collections that depend on this instance. When the transaction
        ///         finishes, the depending collections will be refreshed if needed.
        ///         See <see cref="TransactionalReadOnlyObservableCollection{T}" /> for more details.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="ITransactionalReadOnlyObservableCollection{T}.BeginTransaction" /> and
        ///         <see cref="ITransactionalReadOnlyObservableCollection{T}.EndTransaction" /> methods to create
        ///         transactions. You can also use <see cref="CreateTransaction{T}" /> or <see cref="Transaction{T}" />
        ///         directly to create a transaction inside a <c>using</c> block.
        ///     </para>
        ///     <para>
        ///         The transactional collections are also manual collections, so you can use
        ///         <see cref="IManualReadOnlyObservableCollection{T}.Reset"/> to refresh the dependent
        ///         collections either during or outside a transaction.
        ///     </para>
        /// </remarks>
        public static ITransactionalReadOnlyObservableList<T> ListAsTransactional<T>(
            this IReadOnlyObservableList<T> list)
        {
            return new TransactionalReadOnyObservableList<T>(list);
        }

        /// <summary>
        ///     Creates a collection proxy that can be used to optimize performance when performing multiple
        ///     modifications to a source collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <returns>The collection proxy.</returns>
        /// <remarks>
        ///     <para>
        ///         During a transaction, the notifications corresponding to the modifications in the source
        ///         collections will not reach the collections that depend on this instance. When the transaction
        ///         finishes, the depending collections will be refreshed if needed.
        ///         See <see cref="TransactionalReadOnlyObservableCollection{T}" /> for more details.
        ///     </para>
        ///     <para>
        ///         Use the <see cref="ITransactionalReadOnlyObservableCollection{T}.BeginTransaction" /> and
        ///         <see cref="ITransactionalReadOnlyObservableCollection{T}.EndTransaction" /> methods to create
        ///         transactions. You can also use <see cref="CreateTransaction{T}" /> or <see cref="Transaction{T}" />
        ///         directly to create a transaction inside a <c>using</c> block.
        ///     </para>
        ///     <para>
        ///         The transactional collections are also manual collections, so you can use
        ///         <see cref="IManualReadOnlyObservableCollection{T}.Reset"/> to refresh the dependent
        ///         collections either during or outside a transaction.
        ///     </para>
        /// </remarks>
        public static ITransactionalReadOnlyObservableCollection<T> CollectionAsTransactional<T>(
            this IReadOnlyObservableCollection<T> collection)
        {
            return new TransactionalReadOnlyObservableCollection<T>(collection);
        }

        /// <summary>
        ///     Begins a transaction with a <see cref="ITransactionalReadOnlyObservableCollection{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The transactional collection.</param>
        /// <returns><see cref="Transaction{T}.Dispose">Dispose</see> this instance to end the transaction.</returns>
        public static Transaction<T> CreateTransaction<T>(this ITransactionalReadOnlyObservableCollection<T> collection)
        {
            return new Transaction<T>(collection);
        }

        /// <summary>
        ///     Simulates contra-variance by casting the list's items.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static IReadOnlyObservableList<TTarget> ListCast<TSource, TTarget>(
            this IReadOnlyObservableList<TSource> items) where TTarget : TSource
        {
            return items == null ? null : new CastingReadOnlyObservableList<TSource, TTarget>(items);
        }

        /// <summary>
        ///     Simulates contra-variance by casting the list's items.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static IReadOnlyObservableCollection<TTarget> CollectionCast<TSource, TTarget>(
            this IReadOnlyObservableCollection<TSource> items) where TTarget : TSource
        {
            return items == null ? null : new CastingReadOnlyObservableCollection<TSource, TTarget>(items);
        }

        /// <summary>
        ///     Creates an observable list by extracting the elements of a specific type from a source collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of the elements to extract.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns>The list of all the elements in <paramref name="source" /> that are of type <typeparamref name="TTarget" />.</returns>
        public static IReadOnlyObservableList<TTarget> ListOfType<TSource, TTarget>(
            this IReadOnlyObservableCollection<TSource> source) where TTarget : TSource
        {
            var filter = new FilteredReadOnlyObservableList<TSource>(source, item => item is TTarget);
            return new CastingReadOnlyObservableList<TSource, TTarget>(filter);
        }

        #region OrderBy
        /// <summary>
        ///     Creates a sorted view of an observable collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="triggers">The name of the properties of <typeparamref name="T" /> that the collection's order depends on.</param>
        /// <returns>Sorted observable list.</returns>
        public static IReadOnlyObservableList<T> ListOrderBy<T>(this IReadOnlyObservableCollection<T> collection,
            Comparison<T> comparison, params string[] triggers)
        {
            return new SortedReadOnlyObservableList<T>(collection, comparison, triggers);
        }

        /// <summary>
        ///     Creates a sorted view of an observable collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="eqComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <param name="triggers">The name of the properties of <typeparamref name="T" /> that the collection's order depends on.</param>
        /// <returns>Sorted observable list.</returns>
        public static IReadOnlyObservableList<T> ListOrderBy<T>(this IReadOnlyObservableCollection<T> collection,
            Comparison<T> comparison, IEqualityComparer<T> eqComparer, params string[] triggers)
        {
            return new SortedReadOnlyObservableList<T>(collection, comparison, eqComparer, triggers);
        }

        /// <summary>
        ///     Creates an ordered view of an observable collection using key-based ordering.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection items.</typeparam>
        /// <typeparam name="TKey">The type of the sorting keys.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="keySelector">The key function.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="triggers">The name of the properties of <typeparamref name="TSource" /> that the collection's order depends on.</param>
        /// <returns>Sorted observable list.</returns>
        public static IReadOnlyObservableList<TSource> ListOrderBy<TSource, TKey>(this IReadOnlyObservableCollection<TSource> collection,
            Func<TSource, TKey> keySelector,
            Comparison<TKey> comparison, params string[] triggers)
        {
            var observableFactory = new PropertyObservableFactory<TSource, TKey>(keySelector, triggers);
            return new KeySortedReadOnlyObservableList<TSource, TKey>(collection,
                item => observableFactory.Observe(item), comparison, null);
        }

        /// <summary>
        ///     Creates an ordered view of an observable collection using key-based ordering.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection items.</typeparam>
        /// <typeparam name="TKey">The type of the sorting keys.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="keySelector">The key function.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="eqComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <param name="triggers">The name of the properties of <typeparamref name="TSource" /> that the collection's order depends on.</param>
        /// <returns>Sorted observable list.</returns>
        public static IReadOnlyObservableList<TSource> ListOrderBy<TSource, TKey>(this IReadOnlyObservableCollection<TSource> collection, 
            Func<TSource, TKey> keySelector,
            Comparison<TKey> comparison, IEqualityComparer<TSource> eqComparer, params string[] triggers)
        {
            var factory = new PropertyObservableFactory<TSource, TKey>(keySelector, triggers);
            return new KeySortedReadOnlyObservableList<TSource, TKey>(collection, factory.Observe, comparison, eqComparer);
        }

        /// <summary>
        ///     Creates an ordered view of an observable collection using key-based ordering.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection items.</typeparam>
        /// <typeparam name="TKey">The type of the sorting keys.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="keySelector">The key function.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="eqComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <returns>Sorted observable list.</returns>
        public static IReadOnlyObservableList<TSource> ListOrderByObservable<TSource, TKey>(this IReadOnlyObservableCollection<TSource> collection,
            Func<TSource, IObservable<TKey>> keySelector, Comparison<TKey> comparison = null, 
            IEqualityComparer<TSource> eqComparer = null)
        {
            return new KeySortedReadOnlyObservableList<TSource, TKey>(collection, keySelector, comparison, eqComparer);
        }

        /// <summary>
        ///     Creates an ordered view of an observable collection using key-based ordering. 
        ///     The method automatically obtains the properties of <typeparam name="TSource" /> that can affect the predicate's outcome.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection items.</typeparam>
        /// <typeparam name="TKey">The type of the sorting keys.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="keySelector">The key function.</param>
        /// <param name="comparison">The comparison function.</param>
        /// <param name="eqComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <returns>Sorted observable list.</returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableList<TSource> ListOrderByAuto<TSource, TKey>(this IReadOnlyObservableCollection<TSource> collection,
            Expression<Func<TSource, TKey>> keySelector, Comparison<TKey> comparison = null,
            IEqualityComparer<TSource> eqComparer = null) where TSource: INotifyPropertyChanged
        {
            var factory = keySelector.FindProperties();
            return new KeySortedReadOnlyObservableList<TSource, TKey>(collection, item => factory.Observe(item), comparison, eqComparer);
        }
        #endregion

        #region Select
        /// <summary>
        ///     Creates an observable view of a collection by applying a transformation to each item in the corresponding order.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of the target view.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="selector">The transforming function.</param>
        /// <param name="onRemove">The optional callback used to destroy the created <typeparamref name="TTarget" /> instances.</param>
        /// <param name="targetComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <returns>
        /// A list with the projections of the source elements.
        /// </returns>
        public static IReadOnlyObservableList<TTarget> ListSelect<TSource, TTarget>(
            this IReadOnlyObservableCollection<TSource> collection, Func<TSource, TTarget> selector,
            Action<TTarget> onRemove = null, IEqualityComparer<TTarget> targetComparer = null)
        {
            return new TransformingReadOnlyObservableList<TSource, TTarget>(collection, selector, onRemove, targetComparer);
        }

        /// <summary>
        ///     Creates an observable view of a collection by applying a transformation to each item in the corresponding order.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of the target view.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="selector">The transforming function.</param>
        /// <param name="onRemove">The optional callback used to destroy the created <typeparamref name="TTarget" /> instances.</param>
        /// <param name="onChange">The optional callback executed with <paramref name="selector"/> modifies a transformed value.</param>
        /// <param name="sourceComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <returns>
        /// A list with the projections of the source elements.
        /// </returns>
        public static IReadOnlyObservableList<TTarget> ListSelectObservable<TSource, TTarget>(
            this IEnumerable<TSource> collection, Func<TSource, IObservable<TTarget>> selector,
            Action<TTarget> onRemove = null, Action<TTarget, TTarget> onChange = null,
            IEqualityComparer<TSource> sourceComparer = null)
        {
            return new DynamicTransformingReadOnlyObservableList<TSource, TTarget>(collection, selector, onRemove, onChange, sourceComparer);
        }

        /// <summary>
        ///     Creates an observable view of a collection by applying a transformation to each item in the corresponding order.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of the target view.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="selector">The transforming function.</param>
        /// <param name="onRemove">The optional callback used to destroy the created <typeparamref name="TTarget" /> instances.</param>
        /// <param name="onChange">The optional callback executed with <paramref name="selector"/> modifies a transformed value.</param>
        /// <param name="sourceComparer">
        /// A comparer for the list items. This is only used if the source collection is not a list 
        /// and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.
        /// </param>
        /// <returns>
        /// A list with the projections of the source elements.
        /// </returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableList<TTarget> ListSelectAuto<TSource, TTarget>(
            this IEnumerable<TSource> collection, Expression<Func<TSource, TTarget>> selector,
            Action<TTarget> onRemove = null, Action<TTarget, TTarget> onChange = null,
            IEqualityComparer<TSource> sourceComparer = null) where TSource: INotifyPropertyChanged
        {
            var factory = selector.FindProperties();
            return new DynamicTransformingReadOnlyObservableList<TSource, TTarget>(collection, item => factory.Observe(item), onRemove, onChange, sourceComparer);
        }
        #endregion

        #region Join        
        /// <summary>
        ///     Creates a read-only observable collection by correlating the elements of two sequences based on matching keys. 
        /// </summary>
        /// <typeparam name="TOuter">The type of the outer items.</typeparam>
        /// <typeparam name="TInner">The type of the inner items.</typeparam>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TResult">The type of the result items.</typeparam>
        /// <param name="outerSource">The first source sequence.</param>
        /// <param name="innerSource">The second source sequence.</param>
        /// <param name="outerSelector">A function that assigns keys to the items of the first sequence.</param>
        /// <param name="innerSelector">A function that assigns keys to the items of the second sequence.</param>
        /// <param name="resultSelector">A function that combines items with matching keys.</param>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="outerComparer">The comparer of items from the first sequence, used in change events.</param>
        /// <param name="innerComparer">The comparer of items from the second sequence, used in change events..</param>
        /// <returns>An observable collection that contains the items with matching keys.</returns>
        public static IReadOnlyObservableCollection<TResult> CollectionJoinObservable<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outerSource, IEnumerable<TInner> innerSource, 
            Func<TOuter, IObservable<TKey>> outerSelector, Func<TInner, IObservable<TKey>> innerSelector, 
            Func<TOuter, TInner, IObservable<TResult>> resultSelector, IEqualityComparer<TKey> keyComparer = null,
            IEqualityComparer<TOuter> outerComparer = null, IEqualityComparer<TInner> innerComparer = null)
        {
            return new InnerJoinReadOnlyObservableCollection<TOuter, TInner, TKey, TResult>(
                outerSource, innerSource, outerSelector, innerSelector, resultSelector,
                keyComparer, outerComparer, innerComparer);
        }

        /// <summary>
        ///     Creates a read-only observable collection by correlating the elements of two sequences based on matching keys. 
        /// </summary>
        /// <typeparam name="TOuter">The type of the outer items.</typeparam>
        /// <typeparam name="TInner">The type of the inner items.</typeparam>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TResult">The type of the result items.</typeparam>
        /// <param name="outerSource">The first source sequence.</param>
        /// <param name="innerSource">The second source sequence.</param>
        /// <param name="outerSelector">A function that assigns keys to the items of the first sequence.</param>
        /// <param name="innerSelector">A function that assigns keys to the items of the second sequence.</param>
        /// <param name="resultSelector">A function that combines items with matching keys.</param>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="outerComparer">The comparer of items from the first sequence, used in change events.</param>
        /// <param name="innerComparer">The comparer of items from the second sequence, used in change events..</param>
        /// <returns>An observable collection that contains the items with matching keys.</returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableCollection<TResult> CollectionJoinAuto<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outerSource, IEnumerable<TInner> innerSource,
            Expression<Func<TOuter, TKey>> outerSelector, Expression<Func<TInner, TKey>> innerSelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector, IEqualityComparer<TKey> keyComparer = null,
            IEqualityComparer<TOuter> outerComparer = null, IEqualityComparer<TInner> innerComparer = null)
        {
            return new InnerJoinReadOnlyObservableCollection<TOuter, TInner, TKey, TResult>(
                outerSource, innerSource, outerSelector, innerSelector, resultSelector,
                keyComparer, outerComparer, innerComparer);
        }
        #endregion

        /// <summary>
        ///     Creates an observable list by concatenating two or more collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The first collection.</param>
        /// <param name="others">One or more collections to concatenate after <paramref name="items" />, in the specified order.</param>
        /// <returns>
        ///     An observable list that contains the elements in <paramref name="items" /> and then, the items of each
        ///     collection in <paramref name="others" /> in that order.
        /// </returns>
        public static IReadOnlyObservableList<T> ListConcat<T>(this IEnumerable<T> items,
            params IEnumerable<T>[] others)
        {
            var containers = new List<CollectionContainer<T>>(others.Length + 1)
            {
                new CollectionContainer<T>(items)
            };
            containers.AddRange(others.Select(c => new CollectionContainer<T>(c)));
            var composite = new CompositeReadOnlyObservableListSource<T>(containers);
            return composite.View;
        }

        /// <summary>
        ///     Creates an observable collection by concatenating two or more collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The first collection.</param>
        /// <param name="others">One or more collections to concatenate after <paramref name="items" />, in the specified order.</param>
        /// <returns>
        ///     An observable collection that contains the elements in <paramref name="items" /> and then, the items of each
        ///     collection in <paramref name="others" /> in that order.
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionConcat<T>(this IEnumerable<T> items,
            params IEnumerable<T>[] others)
        {
            var containers = new List<CollectionContainer<T>>(others.Length + 1) {new CollectionContainer<T>(items)};
            containers.AddRange(others.Select(c => new CollectionContainer<T>(c)));
            var composite = new CompositeReadOnlyObservableCollectionSource<T>(containers);
            return composite.View;
        }

        /// <summary>
        ///     Creates an observable collection with no repeated elements.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="comparer">The equality comparer, or <c>null</c> for the default equality comparer.</param>
        /// <returns>
        ///     An observable collection with the elements in <paramref name="items"/>, but no repetitions.
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionDistinct<T>(
            this IReadOnlyObservableCollection<T> items, IEqualityComparer<T> comparer = null)
        {
            return new DistinctReadOnlyObservableCollection<T>(items, comparer);
        }

        /// <summary>
        ///     Creates an observable collection that is the set union of two or more collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The first collection.</param>
        /// <param name="others">One or more collections to merge with <paramref name="items" />.</param>
        /// <returns>
        ///     An observable collection with all the elements from the source collections, 
        ///     but no repetitions. The default comparer is used to determine equality.
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionUnion<T>(this IEnumerable<T> items,
            params IEnumerable<T>[] others)
        {
            return items.CollectionConcat(others).CollectionDistinct();
        }

        /// <summary>
        ///     Creates an observable collection that is the set union of two or more collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The first collection. Can be observable or not.</param>
        /// <param name="comparer">The equality comparer, or <c>null</c> for the default equality comparer.</param>
        /// <param name="others">One or more collections to merge with <paramref name="items" />. Can be observable or not.</param>
        /// <returns>
        ///     An observable collection with all the elements from the source collections, 
        ///     but no repetitions. 
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionUnion<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer,
            params IEnumerable<T>[] others)
        {
            return items.CollectionConcat(others).CollectionDistinct(comparer);
        }

        /// <summary>
        ///     Creates an observable collection that is the set difference of two collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="first">The first collection. Can be observable or not.</param>
        /// <param name="second">The collection of elements to exclude. Can be observable or not.</param>
        /// <param name="comparer">The equality comparer to use. If it is <c>null</c>, the default comparer is used.</param>
        /// <returns>
        ///     An observable collection that contains all the elements from <paramref name="first"/>, except those
        ///     that also appear in <paramref name="second"/>.
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionExcept<T>(this IEnumerable<T> first,
            IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            return new SetDifferenceReadOnlyObservableCollection<T>(first, second, comparer);
        }

        /// <summary>
        ///     Creates an observable collection that is the set intersection of two collections.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="first">The first collection. Can be observable or not.</param>
        /// <param name="second">The collection of elements to exclude. Can be observable or not.</param>
        /// <param name="comparer">The equality comparer to use. If it is <c>null</c>, the default comparer is used.</param>
        /// <returns>
        ///     An observable collection that contains all the elements that appear both in <paramref name="first"/> and <paramref name="second"/>.
        /// </returns>
        public static IReadOnlyObservableCollection<T> CollectionIntersect<T>(this IEnumerable<T> first,
            IEnumerable<T> second, IEqualityComparer<T> comparer = null)
        {
            return new SetIntersectionReadOnlyObservableCollection<T>(first, second, comparer);
        }

        #region SelectMany
        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one list.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable list obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableList<TTarget> ListSelectMany<TSource, TTarget>(
            this IEnumerable<TSource> items, Func<TSource, IEnumerable<TTarget>> selector)
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var composite = new CompositeReadOnlyObservableListSource<TTarget>(from item in items
                                                                               select new CollectionContainer<TTarget>(selector(item)));
            if (collection == null)
                return composite.View;
            return new ListUpdater<TSource, TTarget>(collection, selector, composite);
        }

        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable collection obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableCollection<TTarget> CollectionSelectMany<TSource, TTarget>(
            this IEnumerable<TSource> items, Func<TSource, IEnumerable<TTarget>> selector)
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var composite = new CompositeReadOnlyObservableCollectionSource<TTarget>(from item in items
                                                                                     select new CollectionContainer<TTarget>(selector(item)));
            if (collection == null)
                return composite.View;
            return new CollectionUpdater<TSource, TTarget>(collection, selector, composite);
        }

        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one list.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable list obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableList<TTarget> ListSelectManyObservable<TSource, TTarget>(
            this IEnumerable<TSource> items, Func<TSource, IObservable<IEnumerable<TTarget>>> selector)
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var composite = new CompositeReadOnlyObservableListSource<TTarget>(
                from item in items
                select new MutableCollectionContainer<TTarget>(selector(item)));
            if (collection == null)
                return composite.View;
            return new ListUpdater<TSource, TTarget>(collection, selector, composite);
        }

        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable collection obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableCollection<TTarget> CollectionSelectManyObservable<TSource, TTarget>(
            this IEnumerable<TSource> items, Func<TSource, IObservable<IEnumerable<TTarget>>> selector)
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var composite = new CompositeReadOnlyObservableCollectionSource<TTarget>(
                from item in items
                select new MutableCollectionContainer<TTarget>(selector(item)));
            if (collection == null)
                return composite.View;
            return new CollectionUpdater<TSource, TTarget>(collection, selector, composite);
        }

        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one list.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable list obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableList<TTarget> ListSelectManyAuto<TSource, TTarget>(
            this IEnumerable<TSource> items, Expression<Func<TSource, IEnumerable<TTarget>>> selector)
            where TSource: INotifyPropertyChanged
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var factory = selector.FindProperties();
            var newSelector = new Func<TSource, IObservable<IEnumerable<TTarget>>>(item => factory.Observe(item));
            var composite = new CompositeReadOnlyObservableListSource<TTarget>(
                from item in items
                select new MutableCollectionContainer<TTarget>(newSelector(item)));
            if (collection == null)
                return composite.View;
            return new ListUpdater<TSource, TTarget>(collection, newSelector, composite);
        }

        /// <summary>
        ///     Projects each element of a sequence to a <see cref="IEnumerable{T}" /> and flattens the resulting
        ///     collections
        ///     into one collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the source collection items.</typeparam>
        /// <typeparam name="TTarget">The type of the target collection items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>
        ///     The observable collection obtained by applying <paramref name="selector" /> to each element of
        ///     <paramref name="items" /> and then concatenating the results.
        /// </returns>
        public static IReadOnlyObservableCollection<TTarget> CollectionSelectManyAuto<TSource, TTarget>(
            this IEnumerable<TSource> items, Expression<Func<TSource, IEnumerable<TTarget>>> selector) 
            where TSource : INotifyPropertyChanged
        {
            var collection = items as IReadOnlyObservableCollection<TSource>;
            var factory = selector.FindProperties();
            var newSelector = new Func<TSource, IObservable<IEnumerable<TTarget>>>(item => factory.Observe(item));
            var composite = new CompositeReadOnlyObservableCollectionSource<TTarget>(
                from item in items
                select new MutableCollectionContainer<TTarget>(newSelector(item)));
            if (collection == null)
                return composite.View;
            return new CollectionUpdater<TSource, TTarget>(collection, newSelector, composite);
        }
        #endregion

        #region Where
        /// <summary>
        ///     Creates an observable view by filtering a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="triggers">
        ///     The name of the properties of <typeparamref name="T" /> that can
        ///     influence in <paramref name="predicate" />.
        /// </param>
        /// <returns>
        ///     An observable list that always contains the elements from <paramref name="items" />
        ///     that satisfy <paramref name="predicate" />.
        /// </returns>
        public static IReadOnlyObservableList<T> ListWhere<T>(this IEnumerable<T> items, Predicate<T> predicate,
            params string[] triggers)
        {
            return new FilteredReadOnlyObservableList<T>(items, predicate, triggers);
        }

        /// <summary>
        ///     Creates an observable view by filtering a sequence of values based on an observable predicate.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="predicate">The observable predicate.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>
        ///     An observable list that always contains the elements from <paramref name="items" />
        ///     that satisfy <paramref name="predicate" />.
        /// </returns>
        public static IReadOnlyObservableList<T> ListWhereObservable<T>(this IEnumerable<T> items, Func<T, IObservable<bool>> predicate,
            IEqualityComparer<T> comparer = null)
        {
            return new FilteredReadOnlyObservableList<T>(items, predicate, comparer);
        }

        /// <summary>
        ///     Creates an observable view by filtering a sequence of values based on a predicate.
        ///     The method automatically obtains the properties of <typeparam name="T" /> that can affect the predicate's outcome.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>
        ///     An observable list that always contains the elements from <paramref name="items" />
        ///     that satisfy <paramref name="predicate" />.
        /// </returns>
        /// <see cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableList<T> ListWhereAuto<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate,
            IEqualityComparer<T> comparer = null) where T: INotifyPropertyChanged
        {
            var factory = predicate.FindProperties();
            return new FilteredReadOnlyObservableList<T>(items, item => factory.Observe(item), comparer);
        }
        #endregion

        #region GroupBy
        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items, Func<TSource, TKey> selector, params string[] triggers)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, selector, null, null, triggers)
            {
                IncludeImplicitGroups = true
            };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">The comparer used for key comparison.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items, Func<TSource, TKey> selector, IEqualityComparer<TKey> comparer,
            params string[] triggers)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, selector, comparer, null, triggers)
            {
                IncludeImplicitGroups = true
            };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, TKey> selector, params string[] triggers)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, explicitGroups, selector, null, null,
                triggers)
            { IncludeImplicitGroups = includeImplict };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">The comparer used for key comparison.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, TKey> selector, IEqualityComparer<TKey> comparer, params string[] triggers)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, explicitGroups, selector, comparer,
                null,
                triggers)
            { IncludeImplicitGroups = includeImplict };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items, Func<TSource, TKey> selector, params string[] triggers)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, selector, null, null, triggers)
            {
                IncludeImplicitGroups = true
            };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">The comparer used for key comparison.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items, Func<TSource, TKey> selector, IEqualityComparer<TKey> comparer,
            params string[] triggers)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, selector, comparer, null, triggers)
            {
                IncludeImplicitGroups = true
            };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, TKey> selector, params string[] triggers)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, explicitGroups, selector, null, null,
                    triggers)
            { IncludeImplicitGroups = includeImplict };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">The comparer used for key comparison.</param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="selector" />.
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupBy<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, TKey> selector, IEqualityComparer<TKey> comparer, params string[] triggers)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, explicitGroups, selector, comparer,
                    null,
                    triggers)
            { IncludeImplicitGroups = includeImplict };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list 
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.  
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupByObservable<TSource, TKey>(
            this IEnumerable<TSource> items,
            Func<TSource, IObservable<TKey>> selector,
            IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TSource> sourceComparer = null)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, selector, keyComparer, sourceComparer);
        }

        /// <summary>
        ///     Creates several observable collection by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param> 
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list 
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.  
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupByObservable<TSource, TKey>(
            this IEnumerable<TSource> items,
            Func<TSource, IObservable<TKey>> selector,
            IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TSource> sourceComparer = null)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, selector, keyComparer, sourceComparer);
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param>
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list 
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.  
        /// </param>
        /// <returns>An observable list of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>> ListGroupByObservable<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, IObservable<TKey>> selector, 
            IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TSource> sourceComparer = null)
        {
            return new GroupingReadOnlyObservableListSource<TSource, TKey>(items, explicitGroups, selector, keyComparer, sourceComparer)
            { IncludeImplicitGroups = includeImplict };
        }

        /// <summary>
        ///     Creates several observable lists by grouping a source sequence according to some criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the source items.</typeparam>
        /// <typeparam name="TKey">The type of the keys used to group items.</typeparam>
        /// <param name="items">The source items.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="includeImplict">
        ///     Whether to include implicit groups (
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />).
        /// </param>
        /// <param name="selector">The function used to obtain keys that represent the items.</param> 
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list 
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.  
        /// </param>
        /// <returns>An observable collection of observable groups.</returns>
        /// <remarks>
        /// <para>
        ///     If <paramref name="items"/> is a <see cref="IReadOnlyObservableCollection{T}"/> with no
        ///     implicit element order (an <see cref="ObservableSet{T}"/>, for example), you should
        ///     override <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/> in <typeparamref name="TSource"/>
        ///     or provide a <see cref="IEqualityComparer{TSource}"/> through the constructor of
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>. 
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> CollectionGroupByObservable<TSource, TKey>(
            this IEnumerable<TSource> items,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups, bool includeImplict,
            Func<TSource, IObservable<TKey>> selector, 
            IEqualityComparer<TKey> keyComparer = null, IEqualityComparer<TSource> sourceComparer = null)
        {
            return new GroupingReadOnlyObservableCollectionSource<TSource, TKey>(items, explicitGroups, selector, keyComparer, sourceComparer)
            { IncludeImplicitGroups = includeImplict };
        }
        #endregion

        #region Zip        
        /// <summary>
        ///     Creates a read-only observable list by correlating the elements of two lists based on matching indexes. 
        /// </summary>
        /// <typeparam name="TOuter">The type of the outer items.</typeparam>
        /// <typeparam name="TInner">The type of the inner items.</typeparam>
        /// <typeparam name="TResult">The type of the result items.</typeparam>
        /// <param name="outerSource">The first source sequence.</param>
        /// <param name="innerSource">The second source sequence.</param>
        /// <param name="resultSelector">A function that combines items with matching indexes.</param>
        /// <returns>An observable lists that contains the items with matching indexes.</returns>
        public static IReadOnlyObservableCollection<TResult> ListZipObservable<TOuter, TInner, TResult>(
            this IReadOnlyList<TOuter> outerSource, IReadOnlyList<TInner> innerSource,
            Func<TOuter, TInner, IObservable<TResult>> resultSelector)
        {
            return new ZipReadOnlyObservableList<TOuter,TInner,TResult>(outerSource, innerSource, resultSelector);
        }

        /// <summary>
        ///     Creates a read-only observable list by correlating the elements of two lists based on matching indexes. 
        /// </summary>
        /// <typeparam name="TOuter">The type of the outer items.</typeparam>
        /// <typeparam name="TInner">The type of the inner items.</typeparam>
        /// <typeparam name="TResult">The type of the result items.</typeparam>
        /// <param name="outerSource">The first source sequence.</param>
        /// <param name="innerSource">The second source sequence.</param>
        /// <param name="resultSelector">A function that combines items with matching indexes.</param>
        /// <returns>An observable lists that contains the items with matching indexes.</returns> 
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableCollection<TResult> ListZipAuto<TOuter, TInner, TResult>(
            this IReadOnlyList<TOuter> outerSource, IReadOnlyList<TInner> innerSource,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return new ZipReadOnlyObservableList<TOuter, TInner, TResult>(outerSource, innerSource, resultSelector);
        }
        #endregion

        /// <summary>
        ///     Creates an observable list from another by bypassing a specific number of elements and taking the rest.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="count">The number of items to bypass.</param>
        /// <returns>An observable list that contains all the items in <paramref name="items" /> after <paramref name="count" />.</returns>
        public static IReadOnlyObservableList<T> ListSkip<T>(this IReadOnlyObservableList<T> items, int count)
        {
            return new RangedReadOnlyObservableList<T>(items, count);
        }

        #region SkipWhile
        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="triggers">
        ///     The name of the properties of <typeparamref name="T" /> that can
        ///     influence in <paramref name="predicate" />.
        /// </param> 
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        public static IReadOnlyObservableList<T> ListSkipWhile<T>(this IReadOnlyList<T> items,
            Predicate<T> predicate, params string[] triggers)
        {
            var factory = new PropertyObservableFactory<T, bool>(x => predicate(x), triggers);
            return items.ListReverse()
                .ListTakeWhileObservable(x => factory.Observe(x).Select(v => !v))
                .ListReverse();
        }

        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="comparer">The comparer of source items.</param> 
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        public static IReadOnlyObservableList<T> ListSkipWhileObservable<T>(this IReadOnlyList<T> items,
            Func<T, IObservable<bool>> predicate, IEqualityComparer<T> comparer = null)
        {
            return items.ListReverse()
                .ListTakeWhileObservable(x => predicate(x).Select(v => !v), comparer)
                .ListReverse();
        }

        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="comparer">The comparer of source items.</param>
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableList<T> ListSkipWhileAuto<T>(this IReadOnlyList<T> items,
            Expression<Func<T, bool>> predicate, IEqualityComparer<T> comparer = null)
        {
            var factory = PropertyObservableFactory<T, bool>.FindIn(predicate);
            return items.ListReverse()
                .ListTakeWhileObservable(x => factory.Observe(x).Select(v => !v), comparer)
                .ListReverse();
        }
        #endregion

        /// <summary>
        ///     Creates an observable list from another by taking a specific number of elements and discarding the rest.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="count">The number of items to take.</param>
        /// <returns>An observable list that contains the first <paramref name="count" /> elements of <paramref name="items" />.</returns>
        public static IReadOnlyObservableList<T> ListTake<T>(this IReadOnlyObservableList<T> items, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            return new RangedReadOnlyObservableList<T>(items, maxCount: count);
        }

        #region TakeWhile
        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="triggers">
        ///     The name of the properties of <typeparamref name="T" /> that can
        ///     influence in <paramref name="predicate" />.
        /// </param> 
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        public static IReadOnlyObservableList<T> ListTakeWhile<T>(this IReadOnlyList<T> items, 
            Predicate<T> predicate, params string[] triggers)
        {
            var factory = new PropertyObservableFactory<T, bool>(x => predicate(x), triggers);
            return new PrefixReadOnlyObservableList<T>(items, factory.Observe);
        }

        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="comparer">The comparer of source items.</param> 
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        public static IReadOnlyObservableList<T> ListTakeWhileObservable<T>(this IReadOnlyList<T> items,
            Func<T, IObservable<bool>> predicate, IEqualityComparer<T> comparer = null)
        {
            return new PrefixReadOnlyObservableList<T>(items, predicate, comparer);
        }

        /// <summary>
        ///     Creates an observable list from another by taking the first consecutive elements that meed the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="comparer">The comparer of source items.</param>
        /// <returns>
        ///     An observable list that contains the first consecutive elements from
        ///     <paramref name="items"/> that meed <paramref name="predicate"/>.
        /// </returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IReadOnlyObservableList<T> ListTakeWhileAuto<T>(this IReadOnlyList<T> items,
            Expression<Func<T, bool>> predicate, IEqualityComparer<T> comparer = null)
        {
            return new PrefixReadOnlyObservableList<T>(items, predicate, comparer);
        }
        #endregion

        /// <summary>
        ///     Creates an observable list from another by taking a sub-sequence of it's items and discarding the rest.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <param name="offset">The index of the first element of the sub-sequence.</param>
        /// <param name="count">The number of elements in the sub-sequence.</param>
        /// <returns>
        ///     An observable list that contains only the <paramref name="count" /> elements long sub-sequence of
        ///     <paramref name="items" /> that starts at <paramref name="offset" />.
        /// </returns>
        public static IReadOnlyObservableList<T> ListRange<T>(this IReadOnlyObservableList<T> items, int offset,
            int count)
        {
            return new RangedReadOnlyObservableList<T>(items, offset, count);
        }

        /// <summary>
        ///     Creates an observable list from another by reversing it's order.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="items">The source list.</param>
        /// <returns>
        ///     An observable list that contains the elements in <paramref name="items"/>
        ///     in reversed order. This operation is O(1), as it only changes the index mapping.
        /// </returns>
        public static IReadOnlyObservableList<T> ListReverse<T>(this IReadOnlyList<T> items)
        {
            return new ReversedReadOnlyObservableList<T>(items);
        }

        /// <summary>
        ///     Creates an observable query that determines if the observable collection has any elements.
        /// </summary>
        /// <typeparam name="T">The type of the collection's elements.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <returns><c>true</c> if the collection has any elements, otherwise <c>false</c>.</returns>
        public static IObservableResult<bool> ObservableAny<T>(this IReadOnlyObservableCollection<T> items)
        {
            return new SimpleQueryObservableResult<T, bool>(items, collection => collection.Count > 0);
        }

        /// <summary>
        ///     Creates an observable query that determines if the sequence has any elements satisfying some condition.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="triggers">
        ///     The names of the properties of <typeparamref name="T" /> that can affect
        ///     <paramref name="predicate" />.
        /// </param>
        /// <returns><c>true</c> if the collection has any elements that satisfy the condition, otherwise <c>false</c>.</returns>
        public static IObservableResult<bool> ObservableAny<T>(this IEnumerable<T> items, Predicate<T> predicate,
            params string[] triggers)
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, predicate, triggers);
            return new SimpleQueryObservableResult<T, bool>(filter, collection => collection.Count > 0);
        }

        /// <summary>
        ///     Creates an observable query that determines if all the elements in a sequence satisfy some condition.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="triggers">
        ///     The names of the properties of <typeparamref name="T" /> that can affect
        ///     <paramref name="predicate" />.
        /// </param>
        /// <returns><c>true</c> if all the elements in <paramref name="items"/> satisfy <paramref name="predicate"/>, otherwise <c>false</c>.</returns>
        public static IObservableResult<bool> ObservableAll<T>(this IEnumerable<T> items, Predicate<T> predicate,
            params string[] triggers)
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, i => !predicate(i), triggers);
            return new SimpleQueryObservableResult<T, bool>(filter, collection => collection.Count == 0);
        }

        /// <summary>
        ///     Creates an observable result that determines if an element is present in a collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="items">The collection of items.</param>
        /// <param name="value">The value to find.</param>
        /// <param name="comparer">The equality comparer. If none is specified, the default comparer is used.</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains <paramref name="value"/>; otherwise, false.</returns>
        public static IObservableResult<bool> ObservableContains<T>(this IReadOnlyObservableCollection<T> items,
            T value, IEqualityComparer<T> comparer = null)
        {
            return new ContainsObservableResult<T>(items, value, comparer);
        }

        #region FirstOrDefault
        /// <summary>
        ///     Creates an observable query that stores the first element of a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection's elements.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <returns>The first element of the collection or <paramref name="defaultValue" />.</returns>
        public static IObservableResult<T> ObservableFirstOrDefault<T>(this IReadOnlyObservableCollection<T> items,
            T defaultValue = default(T))
        {
            return new SimpleQueryObservableResult<T, T>(items,
                collection => collection.Count == 0 ? defaultValue : collection.First());
        }

        /// <summary>
        ///     Creates an observable query the stores the first element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="triggers">
        ///     The names of the properties of <typeparamref name="T" /> that can affect
        ///     <paramref name="predicate" />.
        /// </param>
        /// <returns>The first element of the sequence that satisfies <paramref name="predicate" />.</returns>
        public static IObservableResult<T> ObservableFirstOrDefault<T>(this IEnumerable<T> items,
            Predicate<T> predicate, params string[] triggers)
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, predicate, triggers);
            return new SimpleQueryObservableResult<T, T>(filter, collection => collection.FirstOrDefault());
        }

        /// <summary>
        ///     Creates an observable query the stores the first element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>The first element of the sequence that satisfies <paramref name="predicate" />.</returns>
        public static IObservableResult<T> ObservableFirstOrDefault<T>(this IEnumerable<T> items,
            Func<T, IObservable<bool>> predicate, IEqualityComparer<T> comparer = null,
            T defaultValue = default(T))
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, predicate, comparer);
            return new SimpleQueryObservableResult<T, T>(filter, collection => collection.Count == 0 ? defaultValue : collection.First());
        }

        /// <summary>
        ///     Creates an observable query the stores the first element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>The first element of the sequence that satisfies <paramref name="predicate" />.</returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IObservableResult<T> ObservableFirstOrDefaultAuto<T>(this IEnumerable<T> items,
            Expression<Func<T, bool>> predicate, IEqualityComparer<T> comparer = null,
            T defaultValue = default(T)) where T: INotifyPropertyChanged
        {
            var factory = predicate.FindProperties();
            var filter = new FilteredReadOnlyObservableList<T>(items, item => factory.Observe(item), comparer);
            return new SimpleQueryObservableResult<T, T>(filter, collection => collection.Count == 0 ? defaultValue : collection.First());
        }
        #endregion

        #region LastOrDefault
        /// <summary>
        ///     Creates an observable query that stores the last element of a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection's elements.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <returns>The last element of the collection or <paramref name="defaultValue" />.</returns>
        public static IObservableResult<T> ObservableLastOrDefault<T>(this IReadOnlyObservableCollection<T> items,
            T defaultValue = default(T))
        {
            if (items is IReadOnlyObservableList<T>)
                return new SimpleQueryObservableResult<T, T>(items,
                    collection =>
                        collection.Count == 0
                            ? defaultValue
                            : ((IReadOnlyObservableList<T>)collection)[collection.Count - 1]);
            return new LastElementQueryResult<T>(items, defaultValue);
        }

        /// <summary>
        ///     Creates an observable query the stores the last element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="triggers">
        ///     The names of the properties of <typeparamref name="T" /> that can affect
        ///     <paramref name="predicate" />.
        /// </param>
        /// <returns>The last element of the sequence that satisfies <paramref name="predicate" />.</returns>
        public static IObservableResult<T> ObservableLastOrDefault<T>(this IEnumerable<T> items,
            Predicate<T> predicate, params string[] triggers)
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, predicate, triggers);
            if (items is IReadOnlyObservableList<T>)
                return new SimpleQueryObservableResult<T, T>(filter,
                    collection =>
                        collection.Count == 0
                            ? default(T)
                            : ((IReadOnlyObservableList<T>)collection)[collection.Count - 1]);
            return new LastElementQueryResult<T>(filter);
        }

        /// <summary>
        ///     Creates an observable query the stores the last element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>The last element of the sequence that satisfies <paramref name="predicate" />.</returns>
        public static IObservableResult<T> ObservableLastOrDefault<T>(this IEnumerable<T> items,
            Func<T, IObservable<bool>> predicate, IEqualityComparer<T> comparer = null,
            T defaultValue = default(T))
        {
            var filter = new FilteredReadOnlyObservableList<T>(items, predicate, comparer);
            if (items is IReadOnlyObservableList<T>)
                return new SimpleQueryObservableResult<T, T>(filter,
                    collection =>
                        collection.Count == 0
                            ? defaultValue
                            : ((IReadOnlyObservableList<T>)collection)[collection.Count - 1]);
            return new LastElementQueryResult<T>(filter, defaultValue);
        }

        /// <summary>
        ///     Creates an observable query the stores the last element that satisfies a condition in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence's elements.</typeparam>
        /// <param name="items">The source sequence.</param>
        /// <param name="predicate">The predicate that represents the condition.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <param name="comparer">The equality comparer for the items, in case <paramref name="items"/> is an index-less collection.</param>
        /// <returns>The last element of the sequence that satisfies <paramref name="predicate" />.</returns>
        /// <seealso cref="ObservableExtensions.Observe{T}"/>
        public static IObservableResult<T> ObservableLastOrDefaultAuto<T>(this IEnumerable<T> items,
            Expression<Func<T, bool>> predicate, IEqualityComparer<T> comparer = null,
            T defaultValue = default(T)) where T: INotifyPropertyChanged
        {
            var factory = predicate.FindProperties();
            var filter = new FilteredReadOnlyObservableList<T>(items, item => factory.Observe(item), comparer);
            if (items is IReadOnlyObservableList<T>)
                return new SimpleQueryObservableResult<T, T>(filter,
                    collection =>
                        collection.Count == 0
                            ? defaultValue
                            : ((IReadOnlyObservableList<T>)collection)[collection.Count - 1]);
            return new LastElementQueryResult<T>(filter, defaultValue);
        }
        #endregion

        /// <summary>
        ///     Creates an observable query that stores the element at the specified index in a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection's elements.</typeparam>
        /// <param name="items">The source collection.</param>
        /// <param name="index">The index.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <returns>The first element of the collection or <paramref name="defaultValue" />.</returns>
        public static IObservableResult<T> ObservableElementAtOrDefault<T>(this IReadOnlyObservableCollection<T> items,
            int index,
            T defaultValue = default(T))
        {
            if (items is IReadOnlyObservableList<T>)
                return new SimpleQueryObservableResult<T, T>(items,
                    collection =>
                    {
                        var list = (IReadOnlyObservableList<T>) collection;
                        return list.Count > index ? list[index] : defaultValue;
                    });
            return new SimpleQueryObservableResult<T, T>(items,
                collection => collection.Count > index ? collection.ElementAt(index) : defaultValue);
        }

        /// <summary>
        /// Returns the amount of elements in a collection as an observable result.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="items">The collection.</param>
        /// <returns>
        /// An <see cref="IObservableResult{T}"/> that points to the 
        /// <see cref="IReadOnlyCollection{T}.Count"/> property of <paramref name="items"/>.
        /// </returns>
        [Obsolete("Use ObservableExtensions.Observe instead.")]
        public static IObservableResult<int> ObservableCount<T>(this IReadOnlyObservableCollection<T> items)
        {
            return new ObjectPropertyResult<IReadOnlyObservableCollection<T>, int>(items, x => x.Count);
        }

        /// <summary>
        /// Creates a list that contains the content of either one of two sources based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        /// <returns>
        /// A list with the content of <paramref name="positiveSource"/> when <paramref name="condition"/> is True 
        /// and <paramref name="negativeSource"/>, when it is False.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method works similar to a ternary operator (?), but with one advantage: the returned list will 
        /// monitor the value of the condition. So, if after the method's execution the value of the condition
        /// changes, the list will update it's content and notify it's clients.
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableList<T> ListBasedOnCondition<T>(this IObservable<bool> condition,
            IReadOnlyObservableList<T> positiveSource, IReadOnlyObservableList<T> negativeSource)
        {
            return new ConditionalSwitchableListSource<T>(condition, positiveSource, negativeSource);
        }

        /// <summary>
        /// Creates a collection that contains the content of either one of two sources based on a condition.
        /// </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        /// <returns>
        /// A collection with the content of <paramref name="positiveSource"/> when <paramref name="condition"/> is True 
        /// and <paramref name="negativeSource"/>, when it is False.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method works similar to a ternary operator (?), but with one advantage: the returned collection will 
        /// monitor the value of the condition. So, if after the method's execution the value of the condition
        /// changes, the list will update it's content and notify it's clients.
        /// </para>
        /// </remarks>
        public static IReadOnlyObservableCollection<T> CollectionBasedOnCondition<T>(
            this IObservable<bool> condition,
            IReadOnlyObservableCollection<T> positiveSource, IReadOnlyObservableCollection<T> negativeSource)
        {
            return new ConditionalSwitchableCollectionSource<T>(condition, positiveSource, negativeSource);
        }

        private abstract class CollectionUpdaterBase<TSource, TTarget> : IReadOnlyObservableCollection<TTarget>, IWeakEventListener
        {
            private readonly CompositeReadOnlyObservableCollectionSourceBase<TTarget> _composite;
            private readonly Func<TSource, IObservable<IEnumerable<TTarget>>> _selector;
            private readonly IReadOnlyObservableCollection<TSource> _source;

            protected CollectionUpdaterBase(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IObservable<IEnumerable<TTarget>>> selector,
                CompositeReadOnlyObservableCollectionSourceBase<TTarget> composite)
            {
                _source = source;
                _composite = composite;
                _selector = selector;

                CollectionChangedEventManager.AddListener(_source, this);
            }

            public IEnumerator<TTarget> GetEnumerator() => GetView().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => GetView().Count;
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;
            protected abstract IReadOnlyObservableCollection<TTarget> GetView();

            protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                CollectionChanged?.Invoke(this, e);
            }

            protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(this, e);
            }

            public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (!Equals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                    return false;
                UpdateCollection(_composite, _source, (NotifyCollectionChangedEventArgs) e,
                    o => new MutableCollectionContainer<TTarget>(_selector((TSource) o)), 
                    onRemove: o => (o as IDisposable)?.Dispose());
                return true;
            }
        }

        private class CollectionUpdater<TSource, TTarget> : CollectionUpdaterBase<TSource, TTarget>
        {
            private readonly CompositeReadOnlyObservableCollectionSource<TTarget> _composite;

            public CollectionUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IObservable<IEnumerable<TTarget>>> selector,
                CompositeReadOnlyObservableCollectionSource<TTarget> composite) : base(source, selector, composite)
            {
                _composite = composite;
                CollectionChangedEventManager.AddListener(_composite.View, this);
                PropertyChangedEventManager.AddListener(_composite.View, this, string.Empty);
            }

            public CollectionUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IEnumerable<TTarget>> selector,
                CompositeReadOnlyObservableCollectionSource<TTarget> composite,
                params string[] triggers) : base(source, 
                item => new PropertyObservableFactory<TSource, IEnumerable<TTarget>>(selector, triggers).Observe(item), composite)
            {
                _composite = composite;
                CollectionChangedEventManager.AddListener(_composite.View, this);
                PropertyChangedEventManager.AddListener(_composite.View, this, string.Empty);
            }

            public CollectionUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IObservable<IEnumerable<TTarget>>> selector)
                : this(
                    source, selector,
                    new CompositeReadOnlyObservableCollectionSource<TTarget>(from item in source
                        select new MutableCollectionContainer<TTarget>(selector(item))))
            {
            }

            protected override IReadOnlyObservableCollection<TTarget> GetView()
            {
                return _composite.View;
            }

            public override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (!ReferenceEquals(sender, _composite.View))
                    return base.ReceiveWeakEvent(managerType, sender, e);
                if (managerType == typeof(CollectionChangedEventManager))
                    OnCollectionChanged((NotifyCollectionChangedEventArgs) e);
                else if (managerType == typeof(PropertyChangedEventManager))
                    OnPropertyChanged((PropertyChangedEventArgs) e);
                else
                    return false;
                return true;
            }
        }

        private class ListUpdater<TSource, TTarget> : CollectionUpdaterBase<TSource, TTarget>,
            IReadOnlyObservableList<TTarget>
        {
            private readonly CompositeReadOnlyObservableListSource<TTarget> _composite;

            public ListUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IObservable<IEnumerable<TTarget>>> selector,
                CompositeReadOnlyObservableListSource<TTarget> composite) : base(source, selector, composite)
            {
                _composite = composite;
                CollectionChangedEventManager.AddListener(_composite.View, this);
                PropertyChangedEventManager.AddListener(_composite.View, this, string.Empty);
            }

            public ListUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IEnumerable<TTarget>> selector,
                CompositeReadOnlyObservableListSource<TTarget> composite,
                params string[] triggers) : base(source, 
                item => new PropertyObservableFactory<TSource, IEnumerable<TTarget>>(selector, triggers).Observe(item),
                composite)
            {
                _composite = composite;
                CollectionChangedEventManager.AddListener(_composite.View, this);
                PropertyChangedEventManager.AddListener(_composite.View, this, string.Empty);
            }

            public ListUpdater(IReadOnlyObservableCollection<TSource> source,
                Func<TSource, IObservable<IEnumerable<TTarget>>> selector)
                : this(
                    source, selector,
                    new CompositeReadOnlyObservableListSource<TTarget>(from item in source
                        select new MutableCollectionContainer<TTarget>(selector(item))))
            {
            }

            public TTarget this[int index] => _composite.View[index];

            protected override IReadOnlyObservableCollection<TTarget> GetView()
            {
                return _composite.View;
            }

            public override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (!ReferenceEquals(sender, _composite.View))
                    return base.ReceiveWeakEvent(managerType, sender, e);
                if (managerType == typeof(CollectionChangedEventManager))
                    OnCollectionChanged((NotifyCollectionChangedEventArgs)e);
                else if (managerType == typeof(PropertyChangedEventManager))
                    OnPropertyChanged((PropertyChangedEventArgs)e);
                else
                    return false;
                return true;
            }
        }
    }
}