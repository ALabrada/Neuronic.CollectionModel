using System;
using System.ComponentModel;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel
{
    class NotifyObservable<T>: IObservable<T> where T: INotifyPropertyChanged
    {
        private readonly T _item;
        private readonly string[] _triggers;

        public NotifyObservable(T item, params string[] triggers)
        {
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

                foreach (var trigger in _triggers)
                    PropertyChangedEventManager.AddListener(_item, this, trigger);
            }

            public void Dispose()
            {
                foreach (var trigger in _triggers)
                    PropertyChangedEventManager.RemoveListener(_item, this, trigger);
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                _observer.OnNext(_item);
                return true;
            }
        }
    }
}