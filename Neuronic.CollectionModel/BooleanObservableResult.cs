using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// An utility class that enables the use of boolean operators.
    /// </summary>
    public class BooleanObservableResult : ObservableResult<bool>
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
            PropertyChangedEventManager.AddHandler(result,
                (sender, args) => CurrentValue = ((IObservableResult<bool>) sender).CurrentValue, nameof(CurrentValue));
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
    }
}