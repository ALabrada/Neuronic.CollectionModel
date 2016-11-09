using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents a container used to represent internally source items in <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.ItemContainer{TSource}" />
    public class GroupedItemContainer<TSource, TKey> : ItemContainer<TSource>
    {
        private readonly Func<TSource, TKey> _selector;
        private TKey _key;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedItemContainer{TSource, TKey}"/> class.
        /// </summary>
        /// <param name="item">The contained item.</param>
        /// <param name="selector">The function used to obtain a grouping key that represents the item.</param>
        public GroupedItemContainer(TSource item, Func<TSource, TKey> selector) : base (item)
        {
            _selector = selector;
            Key = _selector(item);
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
        public int SourceIndex { get; set; }

        /// <summary>
        /// Gets or sets the item's index in its group.
        /// </summary>
        /// <value>
        /// The item's index in its group.
        /// </value>
        public int GroupIndex { get; set; }

        /// <summary>
        /// Called when the value of any of the trigger property changes for this item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Key = _selector(Item);
        }

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
    }
}