using System.Collections.ObjectModel;
using System.Linq;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents a group of items, obtained by grouping a sequence of items according to some criteria.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="ReadOnlyObservableList{T}" />
    public class ReadOnlyObservableGroup<TSource, TKey> : ReadOnlyObservableList<TSource>, IGrouping<TKey, TSource>
    {
        private ReadOnlyObservableGroup(ObservableCollection<TSource> items, TKey key, bool isExplicit) : base(items)
        {
            InternalItems = items;
            Key = key;
            IsExplicit = isExplicit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyObservableGroup{TSource, TKey}"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isExplicit">if set to <c>true</c> the group is explicit, otherwise it is implicit.</param>
        internal ReadOnlyObservableGroup(TKey key, bool isExplicit) : this(new ObservableCollection<TSource>(), key, isExplicit)
        {
        }

        /// <summary>
        /// Initializes a new explicit group.
        /// </summary>
        /// <param name="key">The key.</param>
        public ReadOnlyObservableGroup(TKey key) : this (key, true) { }

        /// <summary>
        /// Gets or sets the grouping collections source that owns and fills this instance.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        internal IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> Owner { get; set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is explicit.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is explicit; otherwise, <c>false</c>.
        /// </value>
        /// <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups"/>
        public bool IsExplicit { get; }

        /// <summary>
        /// Gets the internal items.
        /// </summary>
        /// <value>
        /// The internal items.
        /// </value>
        protected internal ObservableCollection<TSource> InternalItems { get; }
    }
}