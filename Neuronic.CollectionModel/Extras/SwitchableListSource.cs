using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// An utility list that can be used to switch sources transparently.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="SwitchableCollectionSourceBase{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    public class SwitchableListSource<T> : SwitchableCollectionSourceBase<T>, IReadOnlyObservableList<T>, IList<T>
    {
        private const string IndexerName = "Item[]";
        private IReadOnlyList<T> _source;

        /// <summary>
        /// Gets source collection.
        /// </summary>
        /// <value>
        /// The source collection.
        /// </value>
        protected override IReadOnlyCollection<T> SourceOverride => _source;

        /// <summary>
        /// Gets or sets the source collection.
        /// </summary>
        /// <value>
        /// The source collection.
        /// </value>
        public IReadOnlyList<T> Source
        {
            get { return _source; }
            set
            {
                // Check if the source is actually changing.
                if (Equals(_source, value))
                    return;
                // Disengage event handlers from old source (if it is not NULL).
                var oldSource = _source;
                if (oldSource != null)
                {
                    var notifyProperties = oldSource as INotifyPropertyChanged;
                    if (notifyProperties != null)
                    {
                        PropertyChangedEventManager.RemoveListener(notifyProperties, this, CountPropertyName);
                        PropertyChangedEventManager.RemoveListener(notifyProperties, this, IndexerName);
                    }
                    var notifyCollection = oldSource as INotifyCollectionChanged;
                    if (notifyCollection != null)
                        CollectionChangedEventManager.RemoveListener(notifyCollection, this);
                }
                // Update source
                _source = value;
                // Engage event handlers to new source (if it is not NULL).
                var newSource = _source;
                if (newSource != null)
                {
                    var notifyProperties = newSource as INotifyPropertyChanged;
                    if (notifyProperties != null)
                    {
                        PropertyChangedEventManager.AddListener(notifyProperties, this, CountPropertyName);
                        PropertyChangedEventManager.AddListener(notifyProperties, this, IndexerName);
                    }
                    var notifyCollection = newSource as INotifyCollectionChanged;
                    if (notifyCollection != null)
                        CollectionChangedEventManager.AddListener(notifyCollection, this);
                }
                // Signal to update instance properties.
                OnPropertyChanged(new PropertyChangedEventArgs(CountPropertyName));
                OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        T IList<T>.this[int index]
        {
            get { return this[index]; }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                if (Source == null)
                    throw new IndexOutOfRangeException("The collection is empty.");
                return (T) Source[index];
            }
        }

        int IList<T>.IndexOf(T item)
        {
            return Source?.IndexOf(item) ?? -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }
    }
}