using System;
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
        private TKey _key;

        public GroupedItemContainer(TSource item, Func<TSource, IObservable<TKey>> selector) : this (item, selector(item))
        {
        }

        public GroupedItemContainer(TSource item, IObservable<TKey> observable) : base(item, observable)
        {
        }

        /// <summary>
        /// Gets or sets the grouping key that represents the item.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TKey Key
        {
            get { return _key; }
            protected set
            {
                _key = value;
                OnKeyChanged();
            }
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

        /// <summary>
        /// Occurs when the item's key changes.
        /// </summary>
        public event EventHandler KeyChanged;

        /// <summary>
        /// Called when the item's key changes.
        /// </summary>
        protected virtual void OnKeyChanged()
        {
            KeyChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnValueChanged(TKey value)
        {
            Key = value;
        }
    }
}