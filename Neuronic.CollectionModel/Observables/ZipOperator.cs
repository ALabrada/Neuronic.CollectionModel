using System;
using System.Collections.Generic;
using Neuronic.CollectionModel.Results;

namespace Neuronic.CollectionModel.Observables
{
    struct ZipOperator<TInput1, TInput2, TOutput>: IObservable<TOutput>
    {
        private readonly Func<TInput1, TInput2, TOutput> _selector;

        public ZipOperator(IObservable<TInput1> firstInput, IObservable<TInput2> secondInput,
            Func<TInput1, TInput2, TOutput> selector)
        {
            FirstInput = firstInput ?? throw new ArgumentNullException(nameof(firstInput));
            SecondInput = secondInput ?? throw new ArgumentNullException(nameof(secondInput));
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public IObservable<TInput1> FirstInput { get; }

        public IObservable<TInput2> SecondInput { get; }

        public IDisposable Subscribe(IObserver<TOutput> observer)
        {
            return new Subscription(FirstInput, SecondInput, observer, _selector);
        }

        class Subscription : IDisposable
        {
            private readonly IObserver<TOutput> _observer;
            private readonly Func<TInput1, TInput2, TOutput> _selector;
            private readonly Observer<TInput1> _firstObserver;
            private readonly Observer<TInput2> _secondObserver;
            private readonly List<IDisposable> _subscriptions;

            public Subscription(IObservable<TInput1> first, IObservable<TInput2> second, IObserver<TOutput> output, Func<TInput1, TInput2, TOutput> selector)
            {
                _observer = output;
                _selector = selector;
                _firstObserver = new Observer<TInput1>();
                _secondObserver = new Observer<TInput2>();

                _firstObserver.ValueChanged += ObserverOnValueChanged;
                _secondObserver.ValueChanged += ObserverOnValueChanged;
                _firstObserver.Error += ObserverOnError;
                _secondObserver.Error += ObserverOnError;
                _firstObserver.Completed += ObserverOnCompleted;
                _secondObserver.Completed += ObserverOnCompleted;

                _subscriptions = new List<IDisposable>(2)
                {
                    first.Subscribe(_firstObserver),
                    second.Subscribe(_secondObserver)
                };
            }

            public void Dispose()
            {
                _firstObserver.ValueChanged -= ObserverOnValueChanged;
                _secondObserver.ValueChanged -= ObserverOnValueChanged;
                _firstObserver.Error -= ObserverOnError;
                _secondObserver.Error -= ObserverOnError;
                _firstObserver.Completed -= ObserverOnCompleted;
                _secondObserver.Completed -= ObserverOnCompleted;

                foreach (var subscription in _subscriptions)
                    subscription.Dispose();
                _subscriptions.Clear();
            }

            private void ObserverOnValueChanged(object sender, EventArgs e)
            {
                if (_firstObserver.IsInitialized && _secondObserver.IsInitialized)
                {
                    var value = _selector(_firstObserver.Value, _secondObserver.Value);
                    _observer.OnNext(value);
                }
            }

            private void ObserverOnError(object sender, ErrorEventArgs e)
            {
                Dispose();
                _observer.OnError(e.Error);
            }

            private void ObserverOnCompleted(object sender, EventArgs e)
            {
                if (_firstObserver.IsComplete && _secondObserver.IsComplete)
                {
                    Dispose();
                    _observer.OnCompleted();
                }
            }
        }
    }
}