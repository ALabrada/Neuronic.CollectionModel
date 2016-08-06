using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Provides a set of extension methods for collections and sequences.
    /// </summary>
    public static class CollectionExtensions
    {
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
        public static void UpdateCollection<T>(this ObservableCollection<T> list, IEnumerable source,
            NotifyCollectionChangedEventArgs e,
            Func<object, T> select = null, Action<T> onRemove = null)
        {
            select = select ?? (o => (T) o);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                        list.Insert(e.NewStartingIndex + i, @select(e.NewItems[i]));
                    break;
                case NotifyCollectionChangedAction.Move:
                    for (var i = 0; i < e.OldItems.Count; i++)
                        list.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var item = list[e.OldStartingIndex];
                        list.RemoveAt(e.OldStartingIndex);
                        onRemove?.Invoke(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceItems(list, e.OldItems, e.NewItems, e.OldStartingIndex, select, onRemove);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (onRemove != null)
                        foreach (var item in list)
                            onRemove(item);
                    list.Clear();
                    foreach (var item in source)
                        list.Add(@select(item));
                    break;
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
        public static void ReplaceItems<T>(this IList<T> list, IEnumerable oldItems, IEnumerable newItems, int index,
            Func<object, T> select = null, Action<T> onRemove = null)
        {
            select = select ?? (o => (T) o);
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

        /// <summary>
        ///     Creates a new item sequence by chaining two sequences of the same type.
        /// </summary>
        /// <typeparam name="T">The type of the sequences.</typeparam>
        /// <param name="items">The first sequence: <strong>(a1, a2, ..., an)</strong>.</param>
        /// <param name="others">The second sequence: <strong>(b1, b2, ..., bm)</strong>.</param>
        /// <returns>The sequence <strong>(a1, a2, ..., an, b1, b2, ..., bm)</strong>.</returns>
        public static IEnumerable<T> Chain<T>(this IEnumerable<T> items, IEnumerable<T> others)
        {
            foreach (var item in items)
                yield return item;
            foreach (var item in others)
                yield return item;
        }

        /// <summary>
        ///     Creates a new item sequence by appending a sequence after a single-item sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="item">The item contained in the single-item sequence: <strong>a</strong>.</param>
        /// <param name="others">The sequence of items: <strong>(b1, b2, ..., bm)</strong>.</param>
        /// <returns>The sequence <strong>(a, b1, b2, ..., bm)</strong>.</returns>
        public static IEnumerable<T> Chain<T>(this T item, IEnumerable<T> others)
        {
            yield return item;
            foreach (var other in others)
                yield return other;
        }

        /// <summary>
        ///     Creates a new item sequence by appending some items after an item sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="items">The item sequence: <strong>(a1, a2, ..., an)</strong>.</param>
        /// <param name="others">The items to append: <strong>(b1, b2, ..., bm)</strong>.</param>
        /// <returns>The sequence <strong>(a1, a2, ..., an, b1, b2, ..., bm)</strong>.</returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, params T[] others)
        {
            return Chain(items, others);
        }

        /// <summary>
        ///     Swaps the specified items in the given list.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="oldIndex">The index of the first item in <paramref name="list" />.</param>
        /// <param name="newIndex">The index of the second item in <paramref name="list" />.</param>
        public static void Swap<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            var temp = list[oldIndex];
            list[oldIndex] = list[newIndex];
            list[newIndex] = temp;
        }

        /// <summary>
        ///     Obtains the index of an item in a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the sequence items.</typeparam>
        /// <param name="items">The sequence.</param>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>
        ///     The index of <paramref name="item" /> in <paramref name="items" />
        ///     or <strong>-1</strong> if it is not in the sequence.
        /// </returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var enumerator = items.GetEnumerator();
            var count = 0;
            while (enumerator.MoveNext())
            {
                if (comparer.Equals(enumerator.Current, item))
                    return count;
                count++;
            }
            return -1;
        }

        /// <summary>
        ///     Copies a sequence of items to an array.
        /// </summary>
        /// <typeparam name="T">Type of the sequence items.</typeparam>
        /// <param name="items">The sequence.</param>
        /// <param name="array">The array.</param>
        /// <param name="start">The index in <paramref name="array" /> where to start copying.</param>
        /// <param name="count">The maximum number of items to copy.</param>
        public static void CopyTo<T>(this IEnumerable<T> items, T[] array, int start, int count)
        {
            var enumerator = items.GetEnumerator();
            var last = Math.Min(start + count, array.Length);
            for (var i = start; i < last && enumerator.MoveNext(); i++)
                array[i] = enumerator.Current;
        }

        /// <summary>
        /// Tries to select the specified item in the selector.
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
    }
}