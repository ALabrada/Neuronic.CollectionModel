﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// List that contains a single item. This item can be an <see cref="IObservableResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    public class SingleItemObservableList<T> : IReadOnlyObservableList<T>, IObserver<T>
    {
        private readonly IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleItemObservableList{T}"/> class.
        /// </summary>
        /// <param name="observableItem">The observable item.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observableItem"/> is <c>null</c>.</exception>
        public SingleItemObservableList(IObservable<T> observableItem)
        {
            if (observableItem == null) throw new ArgumentNullException(nameof(observableItem));
            ObservableItem = observableItem;

            _subscription = ObservableItem.Subscribe(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleItemObservableList{T}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public SingleItemObservableList(T item)
        {
            CurrentItem = item;
        }

        /// <summary>
        /// Gets the observable item.
        /// </summary>
        /// <value>
        /// The observable item.
        /// </value>
        protected IObservable<T> ObservableItem { get; }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <value>
        /// The current item.
        /// </value>
        public T CurrentItem { get; private set; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int IReadOnlyCollection<T>.Count => Count;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        protected int Count => CurrentItem != null ? 1 : 0;

        /// <inheritdoc />
        T IReadOnlyList<T>.this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                return CurrentItem;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (CurrentItem != null)
                yield return CurrentItem;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// Occurs when a property's value changes.
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

        void IObserver<T>.OnCompleted()
        {
        }

        void IObserver<T>.OnError(Exception error)
        {
        }

        void IObserver<T>.OnNext(T value)
        {
            var newItem = value;
            var oldItem = CurrentItem;
            CurrentItem = newItem;
            if (oldItem != null && newItem != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, 0));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentItem)));
            }
            else if (oldItem != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, 0));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentItem)));
            }
            else if (newItem != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, 0));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentItem)));
            }
        }
    }
}