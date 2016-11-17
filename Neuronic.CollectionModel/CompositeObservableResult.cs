using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents an observable result that is a binary operation that involves at least one observable result.
    /// </summary>
    /// <typeparam name="TFirst">The type of the first operand.</typeparam>
    /// <typeparam name="TSecond">The type of the second operand.</typeparam>
    /// <typeparam name="TResult">The type of the operation's result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.ObservableResult{TResult}" />
    public class CompositeObservableResult<TFirst, TSecond, TResult> : ObservableResult<TResult>
    {
        private readonly IObservableResult<TFirst> _first;
        private readonly IObservableResult<TSecond> _second;
        private readonly Func<TFirst, TSecond, TResult> _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeObservableResult{TFirst, TSecond, TResult}"/> class.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown any of the operands or the operation ar <c>null</c>.
        /// </exception>
        public CompositeObservableResult(IObservableResult<TFirst> first, IObservableResult<TSecond> second,
            Func<TFirst, TSecond, TResult> operation)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            _first = first;
            _second = second;
            _operation = operation;
            CurrentValue = _operation(_first.CurrentValue, _second.CurrentValue);
            PropertyChangedEventManager.AddHandler(_first, OnOperandChanged, nameof(CurrentValue));
            PropertyChangedEventManager.AddHandler(_second, OnOperandChanged, nameof(CurrentValue));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeObservableResult{TFirst, TSecond, TResult}"/> class.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown any of the operands or the operation ar <c>null</c>.
        /// </exception>
        public CompositeObservableResult(IObservableResult<TFirst> first, TSecond second,
            Func<TFirst, TSecond, TResult> operation)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            _first = first;
            _second = new FixedValue<TSecond>(second);
            _operation = operation;
            CurrentValue = _operation(_first.CurrentValue, _second.CurrentValue);
            PropertyChangedEventManager.AddHandler(_first, OnOperandChanged, nameof(CurrentValue));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeObservableResult{TFirst, TSecond, TResult}"/> class.
        /// </summary>
        /// <param name="first">The first operand.</param>
        /// <param name="second">The second operand.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown any of the operands or the operation ar <c>null</c>.
        /// </exception>
        public CompositeObservableResult(TFirst first, IObservableResult<TSecond> second,
            Func<TFirst, TSecond, TResult> operation)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            _first = new FixedValue<TFirst>(first);
            _second = second;
            _operation = operation;
            CurrentValue = _operation(_first.CurrentValue, _second.CurrentValue);
            PropertyChangedEventManager.AddHandler(_second, OnOperandChanged, nameof(CurrentValue));
        }

        private void OnOperandChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            CurrentValue = _operation(_first.CurrentValue, _second.CurrentValue);
        }

        class FixedValue<T> : ObservableResult<T>
        {
            public FixedValue(T value)
            {
                CurrentValue = value;
            }
        }
    } 
}