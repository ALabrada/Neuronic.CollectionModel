using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Observables
{
    struct NotifyObservable<T>: IObservable<T>
    {
        private readonly T _item;
        private readonly string[] _triggers;

        public NotifyObservable(T item, params string[] triggers)
        {
            if (!(item is INotifyPropertyChanged) && triggers.Length > 0)
                throw new ArgumentException("The instance should implement INotifyPropertyChanged in order to be observed.", nameof(item));
            _item = item;
            _triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return new Subscription(_item, _triggers, observer);
        }

        class Subscription : IDisposable, IWeakEventListener
        {
            private readonly T _item;
            private readonly string[] _triggers;
            private readonly IObserver<T> _observer;

            public Subscription(T item, string[] triggers, IObserver<T> observer)
            {
                _item = item;
                _triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));

                _observer.OnNext(_item);
                if (_item is INotifyPropertyChanged notifier)
                {
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.AddListener(notifier, this, trigger);
                }
            }

            public void Dispose()
            {
                if (_item is INotifyPropertyChanged notifier)
                {
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.RemoveListener(notifier, this, trigger);
                }
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                _observer.OnNext(_item);
                return true;
            }
        }
    }
}