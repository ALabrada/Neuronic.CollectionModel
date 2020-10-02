using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An utility collection that allows to simulate contra-variance in read-only observable lists.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TTarget">The target subtype of <typeparamref name="TSource"/>.</typeparam>
    /// <seealso cref="CastingReadOnlyObservableCollection{TSource,TTarget}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{TTarget}" />
    /// <seealso cref="System.Collections.Generic.IList{TTarget}" />
    public class CastingReadOnlyObservableList<TSource, TTarget> : CastingReadOnlyObservableCollection<TSource, TTarget>, IReadOnlyObservableList<TTarget>, IList<TTarget>
    {
        private readonly IReadOnlyObservableList<TSource> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastingReadOnlyObservableList{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CastingReadOnlyObservableList(IReadOnlyObservableList<TSource> source) : base (source, nameof(Count), "Item[]")
        {
            _source = source;
        }

        /// <summary>
        /// Gets the <typeparamref name="TTarget"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TTarget"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TTarget this[int index] => (TTarget)(object) _source[index];

        /// <summary>
        /// Gets or sets the <typeparamref name="TTarget"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TTarget"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        TTarget IList<TTarget>.this[int index]
        {
            get { return this[index]; }

            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(TTarget item)
        {
            return _source.IndexOf((TSource)(object)item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        void IList<TTarget>.Insert(int index, TTarget item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        void IList<TTarget>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }
    }
}