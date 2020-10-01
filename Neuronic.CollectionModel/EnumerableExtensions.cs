using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public static class EnumerableExtensions
    {
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
            return items.Concat(others);
        }

        /// <summary>
        ///     Returns the minimum value of a sequence or a default value if it is empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="items">The sequence.</param>
        /// <param name="comparer">The comparison function.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        ///     <paramref name="defaultValue" /> if <paramref name="items" /> is empty; otherwise,
        ///     the minimum value in <paramref name="items" /> according to <paramref name="comparer" />.
        /// </returns>
        public static T MinOrDefault<T>(this IEnumerable<T> items, Comparison<T> comparer = null,
            T defaultValue = default(T))
        {
            comparer = comparer ?? Comparer<T>.Default.Compare;
            using (var enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return defaultValue;
                var result = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    var cmp = comparer(current, result);
                    if (cmp < 0)
                        result = current;
                }
                return result;
            }
        }

        /// <summary>
        ///     Returns the maximum value of a sequence or a default value if it is empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="items">The sequence.</param>
        /// <param name="comparer">The comparison function.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        ///     <paramref name="defaultValue" /> if <paramref name="items" /> is empty; otherwise,
        ///     the maximum value in <paramref name="items" /> according to <paramref name="comparer" />.
        /// </returns>
        public static T MaxOrDefault<T>(this IEnumerable<T> items, Comparison<T> comparer = null,
            T defaultValue = default(T))
        {
            comparer = comparer ?? Comparer<T>.Default.Compare;
            using (var enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return defaultValue;
                var result = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    var cmp = comparer(current, result);
                    if (cmp > 0)
                        result = current;
                }
                return result;
            }
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
            var list = items as IList;
            if (list != null)
                return list.IndexOf(item);

            comparer = comparer ?? EqualityComparer<T>.Default;
            using (var enumerator = items.GetEnumerator())
            {
                var count = 0;
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                        return count;
                    count++;
                }
                return -1;
            }
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
            using (var enumerator = items.GetEnumerator())
            {
                var last = Math.Min(start + count, array.Length);
                for (var i = start; (i < last) && enumerator.MoveNext(); i++)
                    array[i] = enumerator.Current;
            }
        }
    }
}