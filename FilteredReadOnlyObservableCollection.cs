using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel
{
    public class FilteredReadOnlyObservableCollection<T> : IReadOnlyObservableCollection<T>, ICollection<T> 
    {
        protected class ItemContainer
        {
            private readonly Predicate<T> _filter;

            public ItemContainer(T item, Predicate<T> filter)
            {
                _filter = filter;
                Item = item;
                IsIncluded = _filter(Item);
            }

            public T Item { get; }

            public bool IsIncluded { get; private set; }

            public void AttachTriggers(string[] triggers)
            {
                var notify = Item as INotifyPropertyChanged;
                if (notify == null) return;
                foreach (var name in triggers)
                    PropertyChangedEventManager.AddHandler(notify, ItemOnTriggerPropertyChanged, name);
            }

            public void DetachTriggers(string[] triggers)
            {
                var notify = Item as INotifyPropertyChanged;
                if (notify == null) return;
                foreach (var name in triggers)
                    PropertyChangedEventManager.RemoveHandler(notify, ItemOnTriggerPropertyChanged, name);
            }

            private void ItemOnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
            {
                var wasIncluded = IsIncluded;
                IsIncluded = _filter(Item);
                if (IsIncluded != wasIncluded)
                    IsIncludedChanged?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler IsIncludedChanged;
        }

        private readonly IReadOnlyObservableCollection<T> _source;
        private readonly Predicate<T> _filter;
        private readonly string[] _triggers;
        private int _count;

        public FilteredReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, Predicate<T> filter, params string[] triggers) 
        {
            _source = source;
            _filter = filter;
            _triggers = triggers;
            Items = new ObservableCollection<ItemContainer>();

            CollectionChangedEventManager.AddHandler(_source, SourceOnCollectionChanged);
            Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        protected ObservableCollection<ItemContainer> Items { get; }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Items.UpdateCollection(_source, e, o =>
            {
                var container = new ItemContainer((T) o, _filter);
                container.IsIncludedChanged += ContainerOnIsIncludedChanged;
                container.AttachTriggers(_triggers);
                return container;
            }, container =>
            {
                container.DetachTriggers(_triggers);
                container.IsIncludedChanged -= ContainerOnIsIncludedChanged;
            });
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs newArgs = null;
            ItemContainer newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (ItemContainer) e.NewItems[0];
                    if (newContainer.IsIncluded)
                    {
                        Count++;
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (ItemContainer) e.OldItems[0];
                    if (oldContainer.IsIncluded)
                    {
                        Count--;
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (ItemContainer)e.NewItems[0];
                    oldContainer = (ItemContainer)e.OldItems[0];
                    if (newContainer.IsIncluded && oldContainer.IsIncluded)
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            newContainer.Item, oldContainer.Item);
                    else if (newContainer.IsIncluded)
                    {
                        Count++;
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newContainer.Item);
                    }
                    else if (oldContainer.IsIncluded)
                    {
                        Count--;
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Count = Items.Count(c => c.IsIncluded);
                    newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
            }
            if (newArgs != null)
                OnCollectionChanged(newArgs);
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs eventArgs)
        {
            var container = (ItemContainer) sender;
            NotifyCollectionChangedAction action;
            if (container.IsIncluded)
            {
                action = NotifyCollectionChangedAction.Add;
                Count++;
            }
            else
            {
                action = NotifyCollectionChangedAction.Remove;
                Count--;
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, container.Item));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (from container in Items where container.IsIncluded select container.Item).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<T>.Clear()
        {
            throw new InvalidOperationException();
        }

        bool ICollection<T>.Contains(T item)
        {
            return this.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
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

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}