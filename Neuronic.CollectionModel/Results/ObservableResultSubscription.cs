using System;

namespace Neuronic.CollectionModel.Results
{
    class ObservableResultSubscription<T> : IDisposable
    {
        private readonly ObservableResult<T> _observable;
        private readonly IObserver<T> _observer;

        public ObservableResultSubscription(ObservableResult<T> observable, IObserver<T> observer)
        {
            _observable = observable ?? throw new ArgumentNullException(nameof(observable));
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            _observable.CurrentValueChanged += ObservableOnCurrentValueChanged;
            _observable.Error += ObservableOnError;

            _observer.OnNext(_observable.CurrentValue);
        }

        private void ObservableOnCurrentValueChanged(object sender, EventArgs e)
        {
            _observer.OnNext(_observable.CurrentValue);
        }

        private void ObservableOnError(object sender, ErrorEventArgs e)
        {
            _observer.OnError(e.Error);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _observable.CurrentValueChanged -= ObservableOnCurrentValueChanged;
                _observable.Error -= ObservableOnError;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}