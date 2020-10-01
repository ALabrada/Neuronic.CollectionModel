using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// An utility class that enables the use of boolean operators.
    /// </summary>
    [Obsolete("Use System.Reactive extensions instead.")]
    public class BooleanObservableResult : ObservableResult<bool>, IWeakEventListener
    {
        private readonly IObservableResult<bool> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanObservableResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public BooleanObservableResult(IObservableResult<bool> result)
        {
            _result = result;
            CurrentValue = _result.CurrentValue;
            PropertyChangedEventManager.AddListener(result,
                this, nameof(CurrentValue));
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_result, sender) || managerType != typeof(PropertyChangedEventManager))
                return false;
            OnResultChanged(sender, (PropertyChangedEventArgs) e);
            return true;
        }

        /// <summary>
        /// Called when the result of the underlying operation changes.
        /// </summary>
        /// <param name="sender">The underlying operation.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnResultChanged(object sender, PropertyChangedEventArgs args)
        {
            CurrentValue = ((IObservableResult<bool>) sender).CurrentValue;
        }

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator &(BooleanObservableResult first, IObservableResult<bool> second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f & s);
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator |(BooleanObservableResult first, IObservableResult<bool> second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f | s);
        }

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator &(IObservableResult<bool> first, BooleanObservableResult second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f & s);
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator |(IObservableResult<bool> first, BooleanObservableResult second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f | s);
        }

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator &(BooleanObservableResult first, bool second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f & s);
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator |(BooleanObservableResult first, bool second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f | s);
        }

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator &(bool first, BooleanObservableResult second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f & s);
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="first">The first operator.</param>
        /// <param name="second">The second operator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator |(bool first, BooleanObservableResult second)
        {
            return new CompositeObservableResult<bool, bool, bool>(first, second, (f, s) => f | s);
        }

        /// <summary>
        /// Implements the operator !.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static IObservableResult<bool> operator !(BooleanObservableResult operand)
        {
            return new InvertedBooleanObservableResult(operand._result);
        }
    }
}