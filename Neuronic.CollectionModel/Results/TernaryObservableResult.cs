using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// Represents an observable ternary operator (_ ? _ : _).
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Results.ObservableResult{TResult}" />
    public class TernaryObservableResult<TResult> : ObservableResult<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TernaryObservableResult{TResult}"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveResult">The positive result.</param>
        /// <param name="negativeResult">The negative result.</param>
        public TernaryObservableResult(
            IObservableResult<bool> condition, 
            IObservableResult<TResult> positiveResult,
            IObservableResult<TResult> negativeResult)
        {
            Condition = condition;
            PositiveResult = positiveResult;
            NegativeResult = negativeResult;

            PropertyChangedEventManager.AddHandler(Condition, OnObservableResultChanged, nameof(IObservableResult<bool>.CurrentValue));
            PropertyChangedEventManager.AddHandler(PositiveResult, OnObservableResultChanged, nameof(IObservableResult<TResult>.CurrentValue));
            PropertyChangedEventManager.AddHandler(NegativeResult, OnObservableResultChanged, nameof(IObservableResult<TResult>.CurrentValue));

            UpdateCurrentValue();
        }

        /// <summary>
        /// Gets the condition.
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        protected IObservableResult<bool> Condition { get; }

        /// <summary>
        /// Gets the operation's result when <see cref="Condition"/> is <c>true</c>.
        /// </summary>
        /// <value>
        /// The operation's result when <see cref="Condition"/> is <c>true</c>.
        /// </value>
        protected IObservableResult<TResult> PositiveResult { get; }

        /// <summary>
        /// Gets operation's result when <see cref="Condition"/> is <c>false</c>.
        /// </summary>
        /// <value>
        /// The operation's result when <see cref="Condition"/> is <c>false</c>.
        /// </value>
        protected IObservableResult<TResult> NegativeResult { get; }

        private void UpdateCurrentValue()
        {
            CurrentValue = Condition.CurrentValue ? PositiveResult.CurrentValue : NegativeResult.CurrentValue;
        }

        private void OnObservableResultChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateCurrentValue();
        }
    }
}
