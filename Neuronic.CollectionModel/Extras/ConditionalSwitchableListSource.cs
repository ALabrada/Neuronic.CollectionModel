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
    public class ConditionalSwitchableListSource<T> : SwitchableListSource<T>, IObserver<bool>
    {
        private readonly IObservable<bool> _condition;
        private readonly IReadOnlyObservableList<T> _positiveSource;
        private readonly IReadOnlyObservableList<T> _negativeSource;
        private readonly IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalSwitchableListSource{T}"/> class.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="positiveSource">The positive source.</param>
        /// <param name="negativeSource">The negative source.</param>
        public ConditionalSwitchableListSource(IObservable<bool> condition,
            IReadOnlyObservableList<T> positiveSource, IReadOnlyObservableList<T> negativeSource)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _positiveSource = positiveSource;
            _negativeSource = negativeSource;

            _subscription = condition.Subscribe(this);
        }

        void IObserver<bool>.OnCompleted()
        {
        }

        void IObserver<bool>.OnError(Exception error)
        {
        }

        void IObserver<bool>.OnNext(bool value)
        {
            Source = value ? _positiveSource : _negativeSource;
        }
    }
}