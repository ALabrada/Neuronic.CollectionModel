using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    ///     Base class for <see cref="IObservableResult{T}" /> implementations.
    /// </summary>
    /// <typeparam name="T">The type of the operation's result</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IObservableResult{T}" />
    public class ObservableResult<T> : IObservableResult<T>, IEquatable<IObservableResult<T>>
    {
        private T _currentValue;

        /// <summary>
        ///     Gets the current value.
        /// </summary>
        /// <value>
        ///     The current result of the operation.
        /// </value>
        public T CurrentValue
        {
            get { return _currentValue; }
            protected set
            {
                if (Equals(_currentValue, value))
                    return;
                OnCurrentValueChanging();
                _currentValue = value;
                OnCurrentValueChanged();
            }
        }

        /// <summary>
        ///     Occurs when the value of <see cref="P:Neuronic.CollectionModel.IObservableResult`1.CurrentValue" /> is changing.
        /// </summary>
        public event EventHandler CurrentValueChanging;

        /// <summary>
        ///     Occurs when the value of <see cref="P:Neuronic.CollectionModel.IObservableResult`1.CurrentValue" /> has changed.
        /// </summary>
        public event EventHandler CurrentValueChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Occurs when a property value is changing.
        /// </summary>
        //public event PropertyChangingEventHandler PropertyChanging;

        public event EventHandler<ErrorEventArgs> Error;

        protected virtual void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        /// <summary>
        ///     Called when the value of <see cref="CurrentValue" /> is changing.
        /// </summary>
        protected virtual void OnCurrentValueChanging()
        {
            CurrentValueChanging?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when the value of <see cref="CurrentValue" /> has changed.
        /// </summary>
        protected virtual void OnCurrentValueChanged()
        {
            CurrentValueChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentValue)));
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
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator ==(
            ObservableResult<T> first, ObservableResult<T> second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => Equals(f, s));
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator !=(ObservableResult<T> first, ObservableResult<T> second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => !Equals(f, s));
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator ==(
            ObservableResult<T> first, T second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => Equals(f, s));
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator !=(ObservableResult<T> first, T second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => !Equals(f, s));
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator ==(
            T first, ObservableResult<T> second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => Equals(f, s));
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        [Obsolete("Use System.Reactive extensions instead.")]
        public static IObservableResult<bool> operator !=(T first, ObservableResult<T> second)
        {
            return new CompositeObservableResult<T, T, bool>(first, second, (f, s) => !Equals(f, s));
        }
        
        public bool Equals(IObservableResult<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(CurrentValue, other.CurrentValue);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IObservableResult<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(CurrentValue);
        }

        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            return new ObservableResultSubscription<T>(this, observer);
        }
    }
}