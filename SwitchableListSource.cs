using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// An utility list that can be used to switch sources transparently.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.SwitchableCollectionSourceBase{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    /// <seealso cref="System.Collections.Generic.IList{T}" />
    public class SwitchableListSource<T> : SwitchableCollectionSourceBase<T>, IReadOnlyObservableList<T>, IList<T>
    {
        private const string IndexerName = "Item[]";
        private IReadOnlyObservableList<object> _source;

        /// <summary>
        /// Gets source collection.
        /// </summary>
        /// <value>
        /// The source collection.
        /// </value>
        protected override IReadOnlyObservableCollection<object> SourceOverride => _source;

        /// <summary>
        /// Gets or sets the source collection.
        /// </summary>
        /// <value>
        /// The source collection.
        /// </value>
        public IReadOnlyObservableList<object> Source
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
                    PropertyChangedEventManager.RemoveHandler(oldSource, SourceOnPropertyChanged, CountPropertyName);
                    PropertyChangedEventManager.RemoveHandler(oldSource, SourceOnPropertyChanged, IndexerName);
                    CollectionChangedEventManager.RemoveHandler(oldSource, SourceOnCollectionChanged);
                }
                // Update source
                _source = value;
                // Engage event handlers to new source (if it is not NULL).
                var newSource = _source;
                if (newSource != null)
                {
                    PropertyChangedEventManager.AddHandler(newSource, SourceOnPropertyChanged, CountPropertyName);
                    PropertyChangedEventManager.AddHandler(newSource, SourceOnPropertyChanged, IndexerName);
                    CollectionChangedEventManager.AddHandler(newSource, SourceOnCollectionChanged);
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

        /// <summary>
        /// Gets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The item at <paramref name="index"/>.</returns>
        public T this[int index]
        {
            get
            {
                if (Source == null)
                    throw new IndexOutOfRangeException("The collection is empty.");
                return (T) Source[index];
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void SourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
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