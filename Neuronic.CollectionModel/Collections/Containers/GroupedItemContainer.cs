using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Represents a container used to represent internally source items in <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="ItemContainer{TItem}" />
    public class GroupedItemContainer<TSource, TKey> : ObservableItemContainer<TSource, TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedItemContainer{TSource, TKey}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="keyComparer">The key comparer.</param>
        public GroupedItemContainer(TSource item, Func<TSource, IObservable<TKey>> selector, IEqualityComparer<TKey> keyComparer) : this (item, selector(item), keyComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedItemContainer{TSource, TKey}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="observable">The observable.</param>
        /// <param name="keyComparer">The key comparer.</param>
        public GroupedItemContainer(TSource item, IObservable<TKey> observable, IEqualityComparer<TKey> keyComparer) : base(item, observable, keyComparer)
        {
        }

        /// <summary>
        /// Gets or sets the group that currently contains the item.
        /// </summary>
        /// <value>
        /// The group that currently contains the item.
        /// </value>
        public ReadOnlyObservableGroup<TSource, TKey> Group { get; set; }

        /// <summary>
        /// Gets or sets the item's index in the source sequence.
        /// </summary>
        /// <value>
        /// The item's index in the source sequence.
        /// </value>
        public int SourceIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the item's index in its group.
        /// </summary>
        /// <value>
        /// The item's index in its group.
        /// </value>
        public int GroupIndex { get; set; } = -1;
    }
}