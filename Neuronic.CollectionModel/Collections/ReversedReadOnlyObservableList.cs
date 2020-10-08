using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Represents an <see cref="IReadOnlyObservableList{T}"/> that has it's elements in reversed order.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.EventSource" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    public class ReversedReadOnlyObservableList<T>: EventSource, IReadOnlyObservableList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReversedReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ReversedReadOnlyObservableList(IReadOnlyList<T> source) : base(source, "Item[]", nameof(Count))
        {
            Source = source;
        }

        /// <summary>
        /// Gets the source elements.
        /// </summary>
        protected IReadOnlyList<T> Source { get; }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = Count - 1; i >= 0; i--)
                yield return Source[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the element count.
        /// </summary>
        public int Count => Source.Count;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The element at <paramref name="index"/></returns>
        public T this[int index] => Source[Count - 1 - index];

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var newItems = new List<T>(e.NewItems?.Count ?? 0);
            if (e.NewItems != null && e.NewItems.Count > 0)
                newItems.AddRange(e.NewItems.OfType<T>());
            newItems.Reverse();

            var oldItems = new List<T>(e.OldItems?.Count ?? 0);
            if (e.OldItems != null && e.OldItems.Count > 0)
                oldItems.AddRange(e.OldItems.OfType<T>());
            oldItems.Reverse();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewStartingIndex >= 0:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, Count - newItems.Count - e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Add:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
                    break;

                case NotifyCollectionChangedAction.Remove when e.OldStartingIndex >= 0:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, Count - e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems));
                    break;

                case NotifyCollectionChangedAction.Replace when e.NewStartingIndex >= 0 && newItems.Count == oldItems.Count:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, Count - e.NewStartingIndex - newItems.Count));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));
                    break;

                case NotifyCollectionChangedAction.Move when e.NewStartingIndex >= 0 && e.OldStartingIndex >= 0:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, oldItems, Count - e.NewStartingIndex - oldItems.Count, Count - e.OldStartingIndex - oldItems.Count));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
            }
        }
    }
}