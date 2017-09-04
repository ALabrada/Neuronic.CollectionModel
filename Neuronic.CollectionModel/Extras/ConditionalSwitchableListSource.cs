using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// List that can switch between two sources based on a boolean condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Extras.SwitchableListSource{T}" />
    /// <seealso cref="System.Windows.IWeakEventListener" />
    public class ConditionalSwitchableListSource<T> : SwitchableListSource<T>, IWeakEventListener
    {
        private readonly IObservableResult<bool> _condition;
        private readonly IReadOnlyObservableList<T> _positiveSource;
        private readonly IReadOnlyObservableList<T> _negativeSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalSwitchableListSource{T}"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        public ConditionalSwitchableListSource(IObservableResult<bool> condition,
            IReadOnlyObservableList<T> positiveSource, IReadOnlyObservableList<T> negativeSource)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _positiveSource = positiveSource;
            _negativeSource = negativeSource;

            PropertyChangedEventManager.AddListener(_condition, this, nameof(IObservableResult<bool>.CurrentValue));

            Source = _condition.CurrentValue ? _positiveSource : _negativeSource;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_condition, sender))
                return false;
            Source = _condition.CurrentValue ? _positiveSource : _negativeSource;
            return true;
        }
    }
}