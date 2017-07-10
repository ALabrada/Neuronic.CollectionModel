using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Utility class that allows to raise <see cref="NotifyCollectionChangedAction.Reset">Reset</see>
    ///     events at will from external code, thus refreshing other collections that depend on this.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public class ManualReadOnlyObservableCollection<T> : IWeakEventListener, IManualReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ManualReadOnlyObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ManualReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source)
        {
            Source = source;
            CollectionChangedEventManager.AddListener(Source, this);
            PropertyChangedEventManager.AddListener(Source, this, string.Empty);
        }

        /// <summary>
        ///     Gets the source collection.
        /// </summary>
        /// <value>
        ///     The source collection.
        /// </value>
        protected IReadOnlyObservableCollection<T> Source { get; }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() => Source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Gets the number of items in the collection.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        public int Count => Source.Count;

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     Occurs when the value of an instance's property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises the <see cref="CollectionChanged" /> event with the <see cref="NotifyCollectionChangedAction.Reset" />
        ///     action.
        /// </summary>
        public virtual void Reset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (sender != Source)
                return false;
            if (managerType == typeof (CollectionChangedEventManager))
                HandleCollectionEvents((NotifyCollectionChangedEventArgs) e);
            else if (managerType == typeof (PropertyChangedEventManager))
                HandlePropertyEvents((PropertyChangedEventArgs) e);
            else return false;
            return true;
        }

        /// <summary>
        ///     Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Handles <see cref="PropertyChanged" /> events from <see cref="Source" />.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void HandlePropertyEvents(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Count):
                    OnPropertyChanged(e);
                    break;
            }
        }

        /// <summary>
        ///     Handles <see cref="CollectionChanged" /> events from <see cref="Source" />.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void HandleCollectionEvents(NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }
    }
}