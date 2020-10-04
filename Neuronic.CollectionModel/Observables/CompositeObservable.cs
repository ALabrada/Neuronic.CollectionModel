using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Neuronic.CollectionModel.Observables
{
    struct CompositeObservable : IObservable<IList>
    {
        private readonly IList<IObservable<object>> _values;

        public CompositeObservable(IList<IObservable<object>> values)
        {
            _values = values;
        }

        public IDisposable Subscribe(IObserver<IList> observer)
        {
            return new CompositeSubscription(_values, observer);
        }

        class CompositeSubscription : IDisposable
        {
            private readonly bool[] _isInitialized;
            private readonly IObserver<IList> _observer;
            private readonly List<IDisposable> _subscriptions;

            public CompositeSubscription(IList<IObservable<object>> inputs, IObserver<IList> output)
            {
                var length = inputs.Count;
                _observer = output;
                Key = new object[length];
                _isInitialized = new bool[length];
                _subscriptions = new List<IDisposable>(length);
                _subscriptions.AddRange(inputs.Select((v, i) => v.Subscribe(new CompositeObserver(this, i))));
            }

            public IList Key { get; }

            public void Dispose()
            {
                foreach (var subscription in _subscriptions)
                    subscription.Dispose();
            }

            public void OnCompleted()
            {
                _observer.OnCompleted();
                Dispose();
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
                Dispose();
            }

            public void UpdateValueAt(object value, int index)
            {
                Key[index] = value;
                _isInitialized[index] = true;
                if (Array.TrueForAll(_isInitialized, x => x))
                    _observer.OnNext(Key);
            }
        }

        struct CompositeObserver : IObserver<object>
        {
            private readonly CompositeSubscription _subscription;
            private readonly int _index;

            public CompositeObserver(CompositeSubscription subscription, int index)
            {
                _subscription = subscription;
                _index = index;
            }

            public void OnCompleted()
            {
                _subscription.OnCompleted();
            }

            public void OnError(Exception error)
            {
                _subscription.OnError(error);
            }

            public void OnNext(object value)
            {
                _subscription.UpdateValueAt(value, _index);
            }
        }
    }
}