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
    public class CustomReadOnlyObservableCollection<T> : IReadOnlyObservableCollection<T>, IWeakEventListener
    {
        private readonly IReadOnlyCollection<T> _source;
        private int _lastCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CustomReadOnlyObservableCollection(IReadOnlyCollection<T> source)
        {
            _source = source;
            var propertyNotify = _source as INotifyPropertyChanged;
            if (propertyNotify != null)
                PropertyChangedEventManager.AddListener(propertyNotify, this, nameof(Count));
            var collectionNotify = _source as INotifyCollectionChanged;
            if (collectionNotify != null)
                CollectionChangedEventManager.AddListener(collectionNotify, this);
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
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(sender, _source))
                return false;
            if (managerType == typeof(PropertyChangedEventManager))
                OnPropertyChanged(e as PropertyChangedEventArgs);
            else if (managerType == typeof(CollectionChangedEventManager))
                OnCollectionChanged(e as NotifyCollectionChangedEventArgs);
            else return false;
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