using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Utility class to represent any read-only collection as observable, with the ability to manually notify changes.
    /// </summary>
    /// <typeparam name="T">The type of the collection's elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public class CustomReadOnlyObservableCollection<T> : EventSource, IReadOnlyObservableCollection<T>
    {
        private readonly IReadOnlyCollection<T> _source;
        private int _lastCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CustomReadOnlyObservableCollection(IReadOnlyCollection<T> source) : this(source, nameof(Count))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="propertyNames">
        /// The names of the properties that should generate change notifications.
        /// Use <see cref="string.Empty"/> to use the same set of properties as the source.
        /// </param>
        protected CustomReadOnlyObservableCollection(IReadOnlyCollection<T> source, params string[] propertyNames) : base(source, propertyNames)
        {
            _source = source;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => _source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _source.Count;

        /// <summary>
        /// Called when a change notification is received from the source.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <returns>
        ///   <c>true</c> if the event was handled; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!base.OnReceiveWeakEvent(managerType, sender, e))
                return false;
            _lastCount = Count;
            return true;
        }

        /// <summary>
        /// Resets the collection.
        /// </summary>
        /// <remarks>
        /// This is useful if the source collection does not implement <see cref="INotifyCollectionChanged"/>
        /// and you want to notify changes in it.
        /// </remarks>
        public virtual void Reset()
        {
            if (_lastCount != Count)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                _lastCount = Count;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}