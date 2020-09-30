using System;

namespace Neuronic.CollectionModel.Results
{
    class NotifyRelay<T>: ObservableResult<T>, IObserver<T>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            CurrentValue = value;
        }
    }
}