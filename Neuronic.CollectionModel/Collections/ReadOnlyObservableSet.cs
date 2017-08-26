using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Read-only interface of a set.
    /// </summary>
    /// <typeparam name="T">The type of the set elements.</typeparam>
    /// <seealso cref="System.Collections.Generic.ISet{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public class ReadOnlyObservableSet<T> : EventSource, ISet<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        protected ISet<T> Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObservableSet{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ReadOnlyObservableSet(ISet<T> source) : base (source, nameof(Count))
        {
            Source = source;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => Source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a subset of <paramref name="other" />; otherwise, false.
        /// </returns>
        public bool IsSubsetOf(IEnumerable<T> other) => Source.IsSubsetOf(other);

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a superset of <paramref name="other" />; otherwise, false.
        /// </returns>
        public bool IsSupersetOf(IEnumerable<T> other) => Source.IsSupersetOf(other);

        /// <summary>
        /// Determines whether the current set is a proper (strict) superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a proper superset of <paramref name="other" />; otherwise, false.
        /// </returns>
        public bool IsProperSupersetOf(IEnumerable<T> other) => Source.IsProperSupersetOf(other);

        /// <summary>
        /// Determines whether the current set is a proper (strict) subset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is a proper subset of <paramref name="other" />; otherwise, false.
        /// </returns>
        public bool IsProperSubsetOf(IEnumerable<T> other) => Source.IsProperSubsetOf(other);

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set and <paramref name="other" /> share at least one common element; otherwise, false.
        /// </returns>
        public bool Overlaps(IEnumerable<T> other) => Source.Overlaps(other);

        /// <summary>
        /// Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns>
        /// true if the current set is equal to <paramref name="other" />; otherwise, false.
        /// </returns>
        public bool SetEquals(IEnumerable<T> other) => Source.SetEquals(other);

        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(T item) => Source.Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) => Source.CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => Source.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => true;
    }
}