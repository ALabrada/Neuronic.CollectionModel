using System;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Abstraction of an item container that monitors an observable associated to the item.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.Containers.ItemContainer{TItem}" />
    /// <seealso cref="System.IObserver{TResult}" />
    public abstract class ObservableItemContainer<TItem, TResult> : ItemContainer<TItem>, IObserver<TResult>, IDisposable
    {
        private readonly IDisposable _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableItemContainer{TItem, TResult}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="observable">The observable.</param>
        protected ObservableItemContainer(TItem item, IObservable<TResult> observable) : base(item)
        {
            Observable = observable;
            _subscription = Observable?.Subscribe(this);
        }

        /// <summary>
        /// Gets the observable.
        /// </summary>
        /// <value>
        /// The observable.
        /// </value>
        public IObservable<TResult> Observable { get; }

        /// <summary>
        /// Called when the observed value changes.
        /// </summary>
        /// <param name="value">The value.</param>
        protected abstract void OnValueChanged(TResult value);

        void IObserver<TResult>.OnNext(TResult value)
        {
            OnValueChanged(value);
        }

        void IObserver<TResult>.OnError(Exception error)
        {
        }

        void IObserver<TResult>.OnCompleted()
        {
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}