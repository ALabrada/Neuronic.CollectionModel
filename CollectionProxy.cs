using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public class CollectionProxy<T> : IReadOnlyObservableCollection<T>, ICollection<T>
    {
        private const string CountPropertyName = "Count";
        private IReadOnlyObservableCollection<object> _source;

        public IReadOnlyObservableCollection<object> Source
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
                    CollectionChangedEventManager.RemoveHandler(oldSource, SourceOnCollectionChanged);
                }
                // Update source
                _source = value;
                // Engage event handlers to new source (if it is not NULL).
                var newSource = _source;
                if (newSource != null)
                {
                    PropertyChangedEventManager.AddHandler(newSource, SourceOnPropertyChanged, CountPropertyName);
                    CollectionChangedEventManager.AddHandler(newSource, SourceOnCollectionChanged);
                }
                // Signal to update instance properties.
                OnPropertyChanged(new PropertyChangedEventArgs(CountPropertyName));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (Source?.Cast<T>() ?? Enumerable.Empty<T>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Source?.Count ?? 0;

        bool ICollection<T>.IsReadOnly => true;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void SourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(T item)
        {
            return Source?.Contains(item) ?? false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex, array.Length);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }
    }
}