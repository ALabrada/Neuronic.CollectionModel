using System.ComponentModel;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// Represents the negation of a boolean query.
    /// </summary>
    /// <seealso cref="BooleanObservableResult" />
    public class InvertedBooleanObservableResult : BooleanObservableResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvertedBooleanObservableResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public InvertedBooleanObservableResult(IObservableResult<bool> result) : base(result)
        {
            CurrentValue = !result.CurrentValue;
        }

        /// <summary>
        /// Called when the result of the underlying operation changes.
        /// </summary>
        /// <param name="sender">The underlying operation.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnResultChanged(object sender, PropertyChangedEventArgs args)
        {
            CurrentValue = !((IObservableResult<bool>)sender).CurrentValue;
        }
    }
}