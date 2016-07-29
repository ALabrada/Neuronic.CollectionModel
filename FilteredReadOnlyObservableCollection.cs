using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public class FilteredReadOnlyObservableCollection<T> : FilteredReadOnlyObservableCollectionBase<T, ItemContainer<T>>
    {
        private int _count;

        public FilteredReadOnlyObservableCollection(IReadOnlyObservableCollection<T> source, Predicate<T> filter,
            params string[] triggers) : base(source, filter, triggers)
        {
            Items.CollectionChanged += ItemsOnCollectionChanged;
            _count = Items.Count(c => c.IsIncluded);
        }

        public override int Count => _count;

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs newArgs = null;
            ItemContainer<T> newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (ItemContainer<T>) e.NewItems[0];
                    if (newContainer.IsIncluded)
                    {
                        SetCount(Count + 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            newContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (ItemContainer<T>) e.OldItems[0];
                    if (oldContainer.IsIncluded)
                    {
                        SetCount(Count - 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (ItemContainer<T>) e.NewItems[0];
                    oldContainer = (ItemContainer<T>) e.OldItems[0];
                    if (newContainer.IsIncluded && oldContainer.IsIncluded)
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                            newContainer.Item, oldContainer.Item);
                    else if (newContainer.IsIncluded)
                    {
                        SetCount(Count + 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                            newContainer.Item);
                    }
                    else if (oldContainer.IsIncluded)
                    {
                        SetCount(Count - 1);
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                            oldContainer.Item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SetCount(Items.Count(c => c.IsIncluded));
                    newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
            }
            if (newArgs != null)
                OnCollectionChanged(newArgs);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return (from container in Items where container.IsIncluded select container.Item).GetEnumerator();
        }

        private void SetCount(int value)
        {
            if (_count == value) return;
            _count = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        protected override ItemContainer<T> CreateContainer(T item)
        {
            var container = new ItemContainer<T>(item, Filter);
            container.IsIncludedChanged += ContainerOnIsIncludedChanged;
            container.AttachTriggers(Triggers);
            return container;
        }

        protected override void DestroyContainer(ItemContainer<T> container)
        {
            container.DetachTriggers(Triggers);
            container.IsIncludedChanged -= ContainerOnIsIncludedChanged;
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs e)
        {
            var container = (ItemContainer<T>) sender;
            NotifyCollectionChangedAction action;
            if (container.IsIncluded)
            {
                action = NotifyCollectionChangedAction.Add;
                SetCount(Count + 1);
            }
            else
            {
                action = NotifyCollectionChangedAction.Remove;
                SetCount(Count - 1);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, container.Item));
        }
    }
}