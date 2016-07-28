using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel
{
    public class ViewReadOnlyObservableCollection<T> : IReadOnlyObservableCollection<T>, ICollection<T>
    {
        private readonly ICollectionView _view;
        private int _count;

        public ViewReadOnlyObservableCollection(ICollectionView view)
        {
            _view = view;
            Count = view.Cast<T>().Count();
            CollectionChangedEventManager.AddHandler(_view, ViewOnCollectionChanged);
        }

        private void ViewOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    Count = _view.Cast<T>().Count();
                    break;
                default:
                    Count += e.NewItems.Count - e.OldItems.Count;
                    break;
            }
            OnCollectionChanged(e);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _view.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollectionView) _view).GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(T item) => _view.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex, array.Length);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        public int Count
        {
            get { return _count; }
            private set
            {
                if (_count == value) return;
                _count = value;
                OnPropertyChanged();
            }
        }

        bool ICollection<T>.IsReadOnly => true;
        
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}