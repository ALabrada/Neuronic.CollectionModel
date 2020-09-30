using System;

namespace Neuronic.CollectionModel.Results
{
    class ObservableResultSubscription<T> : IDisposable
    {
        private readonly IObservableResult<T> _observable;
        private readonly IObserver<T> _observer;

        public ObservableResultSubscription(IObservableResult<T> observable, IObserver<T> observer)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            _observable.CurrentValueChanged += ObservableOnCurrentValueChanged;

            _observer.OnNext(_observable.CurrentValue);
        }

        private void ObservableOnCurrentValueChanged(object sender, EventArgs e)
        {
            _observer.OnNext(_observable.CurrentValue);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _observable.CurrentValueChanged -= ObservableOnCurrentValueChanged;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}