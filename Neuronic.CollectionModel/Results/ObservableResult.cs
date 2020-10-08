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
        /// Occurs when an error occurs while evaluating the query.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Raises the <see cref="E:Error" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ErrorEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IObservableResult<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(CurrentValue, other.CurrentValue);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is IObservableResult<T> other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
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