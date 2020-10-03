using System;
using System.Collections;
using System.Collections.Generic;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Abstraction of an item container that monitors an observable associated to the item.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.Containers.ItemContainer{TItem}" />
    /// <seealso cref="System.IObserver{TResult}" />
    public class ObservableItemContainer<TItem, TResult> : ItemContainer<TItem>, IObserver<TResult>, IDisposable
    {
        private readonly IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableItemContainer{TItem, TResult}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="observable">The observable.</param>
        /// <param name="valueComparer">The value comparer.</param>
        public ObservableItemContainer(TItem item, IObservable<TResult> observable, 
            IEqualityComparer<TResult> valueComparer = null) : base(item)
        {
            Observable = observable;
            ValueComparer = valueComparer ?? EqualityComparer<TResult>.Default;
            _subscription = Observable?.Subscribe(this);
        }

        /// <summary>
        /// Gets the observable.
        /// </summary>
        /// <value>
        /// The observable.
        /// </value>
        public IObservable<TResult> Observable { get; }

        public IEqualityComparer<TResult> ValueComparer { get; }

        public TResult Value { get; private set; }

        /// <summary>
        /// Called when the observed value changes.
        /// </summary>
        /// <param name="value">The value.</param>
        protected virtual void OnNewValue(TResult value)
        {
            var oldValue = Value;
            Value = value;
            OnValueChanged(new ValueChangedEventArgs<TResult>(oldValue, value));
        }

        public event EventHandler<ValueChangedEventArgs<TResult>> ValueChanged;

        void IObserver<TResult>.OnNext(TResult value)
        {
            OnNewValue(value);
        }

        void IObserver<TResult>.OnError(Exception error)
        {
        }

        void IObserver<TResult>.OnCompleted()
        {
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        protected virtual void OnValueChanged(ValueChangedEventArgs<TResult> eventArgs)
        {
            ValueChanged?.Invoke(this, eventArgs);
        }
    }

    public class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }

        public T NewValue { get; }
    }
}