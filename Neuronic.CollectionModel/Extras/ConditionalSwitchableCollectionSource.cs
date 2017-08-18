using System;
using System.ComponentModel;
using System.Windows;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// Collection that can switch between two sources based on a boolean condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Extras.SwitchableCollectionSource{T}" />
    /// <seealso cref="System.Windows.IWeakEventListener" />
    public class ConditionalSwitchableCollectionSource<T> : SwitchableCollectionSource<T>, IWeakEventListener
    {
        private readonly IObservableResult<bool> _condition;
        private readonly IReadOnlyObservableCollection<T> _positiveSource;
        private readonly IReadOnlyObservableCollection<T> _negativeSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalSwitchableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        public ConditionalSwitchableCollectionSource(IObservableResult<bool> condition,
            IReadOnlyObservableCollection<T> positiveSource, IReadOnlyObservableCollection<T> negativeSource)
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