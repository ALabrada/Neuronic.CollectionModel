using System;
using Neuronic.CollectionModel.Results;

namespace Neuronic.CollectionModel.Observables
{
    class Observer<TInput> : IObserver<TInput>
    {
        public bool IsInitialized { get; private set; }

        public bool IsComplete { get; private set; }

        public TInput Value { get; private set; }

        public event EventHandler ValueChanged;

        public event EventHandler Completed;

        public event EventHandler<ErrorEventArgs> Error; 

        void IObserver<TInput>.OnCompleted()
        {
            IsComplete = true;
            OnCompleted();
        }

        void IObserver<TInput>.OnError(Exception error)
        {
            IsComplete = true;
            OnError(new ErrorEventArgs(error));
        }

        void IObserver<TInput>.OnNext(TInput value)
        {
            IsInitialized = true;
            Value = value;
            OnValueChanged();
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}