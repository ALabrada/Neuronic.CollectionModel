using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// An item container that has an observable index.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.Containers.ObservableItemContainer{TSource, TTarget}" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class IndexedItemContainer<TSource, TTarget> : ObservableItemContainer<TSource, TTarget>, 
        INotifyPropertyChanged
    {
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedItemContainer{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="observable">The observable.</param>
        /// <param name="valueComparer">The value comparer.</param>
        internal IndexedItemContainer(TSource item, IObservable<TTarget> observable, IEqualityComparer<TTarget> valueComparer = null) : base(item, observable, valueComparer)
        {
        }

        /// <summary>
        /// Gets or sets the index of the item in the collection.
        /// </summary>
        public int Index
        {
            get => _index;
            set
            {
                if (_index == value)
                    return;
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when the specified property changes.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnValueChanged(ValueChangedEventArgs<TTarget> eventArgs)
        {
            base.OnValueChanged(eventArgs);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}