using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// Collection that can switch between two sources based on a boolean condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Extras.SwitchableCollectionSource{T}" />
    public class ConditionalSwitchableCollectionSource<T> : SwitchableCollectionSource<T>, IObserver<bool>
    {
        private readonly IObservable<bool> _condition;
        private readonly IReadOnlyObservableCollection<T> _positiveSource;
        private readonly IReadOnlyObservableCollection<T> _negativeSource;
        private readonly IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalSwitchableCollectionSource{T}"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        public ConditionalSwitchableCollectionSource(IObservable<bool> condition,
            IReadOnlyObservableCollection<T> positiveSource, IReadOnlyObservableCollection<T> negativeSource)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _positiveSource = positiveSource;
            _negativeSource = negativeSource;

            _subscription = condition.Subscribe(this);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(bool value)
        {
            Source = value ? _positiveSource : _negativeSource;
        }
    }
}