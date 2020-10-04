using System;

namespace Neuronic.CollectionModel.Observables
{
    class MapOperator<TSource, TTarger> : IObservable<TTarger>, IObserver<TSource>
    {
        private readonly Func<TSource, TTarger> _func;
        private readonly IObservable<TSource> _source;
        private IObserver<TTarger> _observer;

        public MapOperator(IObservable<TSource> source, Func<TSource, TTarger> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
            if (source != null) _source = source;
        }

        public IDisposable Subscribe(IObserver<TTarger> observer)
        {
            _observer = observer;
            return _source.Subscribe(this);
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnNext(TSource value)
        {
            _observer.OnNext(_func(value));
        }
    }
}