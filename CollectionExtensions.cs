using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuronic.CollectionModel
{
    public static class CollectionExtensions
    {
        public static void UpdateCollection<T>(this ObservableCollection<T> list, IEnumerable source, NotifyCollectionChangedEventArgs e,
            Func<object, T> select, Action<T> onRemove = null)
        {
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
                    if (e.NewItems.Count == e.OldItems.Count)
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var item = list[e.OldStartingIndex + i];
                            list[e.OldStartingIndex + i] = @select(e.NewItems[i]);
                            onRemove?.Invoke(item);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            var item = list[e.OldStartingIndex];
                            list.RemoveAt(e.OldStartingIndex);
                            onRemove?.Invoke(item);
                        }
                        for (var i = 0; i < e.NewItems.Count; i++)
                            list.Insert(e.OldStartingIndex + i, @select(e.NewItems[i]));
                    }
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

        public static IEnumerable<T> Chain<T>(this IEnumerable<T> items, IEnumerable<T> others)
        {
            foreach (var item in items)
                yield return item;
            foreach (var item in others)
                yield return item;
        }

        public static IEnumerable<T> Chain<T>(this T item, IEnumerable<T> others)
        {
            yield return item;
            foreach (var other in others)
                yield return other;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, params T[] others)
        {
            return Chain(items, others);
        }

        public static void Move<T>(this IList<T> items, int oldIndex, int newIndex)
        {
            var temp = items[oldIndex];
            items[oldIndex] = items[newIndex];
            items[newIndex] = temp;
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            var enumerator = items.GetEnumerator();
            var count = 0;
            while (enumerator.MoveNext())
            {
                if (Equals(enumerator.Current, item))
                    return count;
                count++;
            }
            return -1;
        }

        public static void CopyTo<T>(this IEnumerable<T> items, T[] array, int start, int count)
        {
            var enumerator = items.GetEnumerator();
            var last = Math.Min(start + count, array.Length);
            for (int i = start; i < last && enumerator.MoveNext(); i++)
                array[i] = enumerator.Current;
        }
    }
}
