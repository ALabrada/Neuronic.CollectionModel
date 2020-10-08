using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections.Containers
{

    /// <summary>
    /// Abstraction of an item container that monitors it's content for changes.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="ItemContainer{TItem}" />
    public abstract class TriggeredItemContainer<TItem> : ItemContainer<TItem>, IWeakEventListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriggeredItemContainer{TItem}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        protected TriggeredItemContainer(TItem item) : base(item)
        {
        }

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

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(Item, sender) || managerType != typeof(PropertyChangedEventManager))
                return false;
            OnTriggerPropertyChanged(sender, (PropertyChangedEventArgs) e);
            return true;
        }
    }
}