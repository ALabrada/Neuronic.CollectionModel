using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Base class for the collections that use another collection as the source of it's change notifications.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    public class EventSource : IWeakEventListener, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private readonly object _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSource"/> class.
        /// </summary>
        /// <param name="source">The source of change notifications.</param>
        /// <param name="propertyNames">
        /// The names of the properties that should generate change notifications.
        /// Use <see cref="string.Empty"/> to use the same set of properties as the source.
        /// </param>
        public EventSource(object source, params string[] propertyNames)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            var propSource = source as INotifyPropertyChanged;
            if (propSource != null)
            {
                foreach (var propertyName in propertyNames)
                    PropertyChangedEventManager.AddListener(propSource, this, propertyName);
            }
            var colSource = source as INotifyCollectionChanged;
            if (colSource != null)
            {
                CollectionChangedEventManager.AddListener(colSource, this);
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return OnReceiveWeakEvent(managerType, sender, e);
        }

        /// <summary>
        /// Called when a change notification is received from the source.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if the event was handled; otherwise, <c>false</c>.</returns>
        protected virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender))
                return false;
            if (managerType == typeof(CollectionChangedEventManager))
                OnCollectionChanged((NotifyCollectionChangedEventArgs) e);
            else if (managerType == typeof(PropertyChangedEventManager))
                OnPropertyChanged((PropertyChangedEventArgs) e);
            else
                return false;
            return true;
        }

        /// <summary>
        /// Occurs when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}