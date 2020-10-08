using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    ///     An item container that monitors an observable associated to the item.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.Containers.ItemContainer{TItem}" />
    /// <seealso cref="System.IObserver{TResult}" />
    [DebuggerDisplay("{Item}: {Value}")]
    public class ObservableItemContainer<TItem, TResult> : ItemContainer<TItem>, IObserver<TResult>, IDisposable
    {
        private readonly IDisposable _subscription;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableItemContainer{TItem, TResult}"/> class.
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

        /// <summary>
        /// Gets the value comparer.
        /// </summary>
        /// <value>
        /// The value comparer.
        /// </value>
        public IEqualityComparer<TResult> ValueComparer { get; }

        /// <summary>
        /// Gets the current observed value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
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

        /// <summary>
        /// Occurs when the observed value changes.
        /// </summary>
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="E:ValueChanged" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ValueChangedEventArgs{TResult}"/> instance containing the event data.</param>
        protected virtual void OnValueChanged(ValueChangedEventArgs<TResult> eventArgs)
        {
            ValueChanged?.Invoke(this, eventArgs);
        }
    }

    /// <summary>
    ///     Contains parameters for value change events. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.EventArgs" />
    public class ValueChangedEventArgs<T> : EventArgs
    {
        internal ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the old value.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        public T OldValue { get; }

        /// <summary>
        /// Creates new value.
        /// </summary>
        /// <value>
        /// The new value.
        /// </value>
        public T NewValue { get; }
    }
}