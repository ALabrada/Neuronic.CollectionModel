using System;
using System.ComponentModel;
using System.Windows;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Abstraction of an item and it's meta-data in some collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public abstract class ItemContainer<TItem> : IWeakEventListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemContainer{TItem}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        protected ItemContainer(TItem item)
        {
            Item = item;
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <value>
        ///     The item.
        /// </value>
        public TItem Item { get; }

        /// <summary>
        ///     Attaches the event handlers that listen to changes in the trigger properties.
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        public void AttachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.AddListener(notify, this, name);
        }

        /// <summary>
        ///     Detaches the event handlers that listen to changes in the trigger properties.
        /// </summary>
        /// <param name="triggers">The triggers.</param>
        public void DetachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.RemoveListener(notify, this, name);
        }

        /// <summary>
        /// Called when the value of any of the trigger property changes for this item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected abstract void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Item?.Equals(obj) ?? false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Item?.GetHashCode() ?? 0;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(Item, sender) || managerType != typeof(PropertyChangedEventManager))
                return false;
            OnTriggerPropertyChanged(sender, (PropertyChangedEventArgs) e);
            return true;
        }
    }
}