using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Neuronic.CollectionModel
{
    public class FilteredReadOnlyObservableList<T> : IReadOnlyObservableList<T>, IList<T>
    {
        private readonly Predicate<T> _filter;
        private readonly IReadOnlyObservableCollection<T> _source;
        private readonly string[] _triggers;

        public FilteredReadOnlyObservableList(IReadOnlyObservableCollection<T> source, Predicate<T> filter,
            params string[] triggers)
        {
            _source = source;
            _filter = filter;
            _triggers = triggers;
            GlobalItems = new GlobalItemCollection();
            LocalItems = new ObservableCollection<T>();

            CollectionChangedEventManager.AddHandler(_source, SourceOnCollectionChanged);
            GlobalItems.CollectionChanged += GlobalItemsOnCollectionChanged;
            LocalItems.CollectionChanged += (sender, args) => OnCollectionChanged(args);
            ((INotifyPropertyChanged) LocalItems).PropertyChanged += (sender, args) => OnPropertyChanged(args);
        }

        protected ObservableCollection<ItemContainer> GlobalItems { get; }
        protected ObservableCollection<T> LocalItems { get; }
        bool ICollection<T>.IsReadOnly => true;

        T IList<T>.this[int index]
        {
            get { return this[index]; }

            set { throw new InvalidOperationException(); }
        }

        public int IndexOf(T item)
        {
            return LocalItems.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
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
            return LocalItems.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            LocalItems.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return LocalItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => LocalItems.Count;
        public T this[int index] => LocalItems[index];

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GlobalItems.UpdateCollection(_source, e, o =>
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

        private void GlobalItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemContainer newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (ItemContainer) e.NewItems[0];
                    if (newContainer.IsIncluded)
                        LocalItems.Insert(newContainer.LocalIndex, newContainer.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (ItemContainer) e.OldItems[0];
                    if (oldContainer.IsIncluded)
                        LocalItems.RemoveAt(oldContainer.LocalIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    //Debug.Assert(e.NewItems.Count == 1);
                    //newContainer = (ItemContainer)e.NewItems[0];
                    //if (newContainer.IsIncluded)
                    //    LocalItems.Move();
                    //break;
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (ItemContainer) e.NewItems[0];
                    oldContainer = (ItemContainer) e.OldItems[0];
                    Debug.Assert(newContainer.LocalIndex == oldContainer.LocalIndex);
                    if (newContainer.IsIncluded && oldContainer.IsIncluded)
                        LocalItems[newContainer.LocalIndex] = newContainer.Item;
                    else if (newContainer.IsIncluded)
                        LocalItems.Insert(newContainer.LocalIndex, newContainer.Item);
                    else if (oldContainer.IsIncluded)
                        LocalItems.RemoveAt(oldContainer.LocalIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LocalItems.Clear();
                    Debug.Assert(GlobalItems.Count == 0);
                    break;
            }
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs eventArgs)
        {
            var container = (ItemContainer) sender;
            if (container.IsIncluded)
            {
                for (var i = container.GlobalIndex; i < GlobalItems.Count; i++)
                    GlobalItems[i].LocalIndex++;
                LocalItems.Insert(container.LocalIndex, container.Item);
            }
            else
            {
                for (var i = container.GlobalIndex; i < GlobalItems.Count; i++)
                    GlobalItems[i].LocalIndex--;
                LocalItems.RemoveAt(container.LocalIndex);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

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
            public int LocalIndex { get; set; }
            public int GlobalIndex { get; set; }

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

        protected class GlobalItemCollection : ObservableCollection<ItemContainer>
        {
            protected override void InsertItem(int index, ItemContainer item)
            {
                item.GlobalIndex = index;
                item.LocalIndex = index == 0 ? -1 : Items[index - 1].LocalIndex;
                if (item.IsIncluded)
                    item.LocalIndex++;

                UpdateIndexes(index, Count, 1, item.IsIncluded ? 1 : 0);

                base.InsertItem(index, item);
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                //if (oldIndex > newIndex)
                //    UpdateIndexes(newIndex, oldIndex, 1, Items[oldIndex].IsIncluded ? 1 : 0);
                //else if (oldIndex < newIndex)
                //    UpdateIndexes(oldIndex + 1, newIndex, -1, Items[oldIndex].IsIncluded ? -1 : 0);

                //Items[oldIndex].GlobalIndex = newIndex;
                //Items[oldIndex].LocalIndex = newIndex == 0 ? -1 : Items[newIndex - 1].LocalIndex;
                //if (Items[oldIndex].IsIncluded)
                //    Items[oldIndex].LocalIndex++;

                //base.MoveItem(oldIndex, newIndex);
                var item = Items[oldIndex];
                RemoveItem(oldIndex);
                InsertItem(newIndex, item);
            }

            protected override void RemoveItem(int index)
            {
                UpdateIndexes(index + 1, Count, -1, Items[index].IsIncluded ? -1 : 0);

                Items[index].GlobalIndex = int.MinValue;

                base.RemoveItem(index);
            }

            protected override void SetItem(int index, ItemContainer item)
            {
                item.GlobalIndex = index;
                item.LocalIndex = index == 0 ? -1 : Items[index - 1].LocalIndex;
                if (item.IsIncluded)
                    item.LocalIndex++;

                if (item.LocalIndex != Items[index].LocalIndex)
                    UpdateIndexes(index + 1, Count, 0, item.LocalIndex - Items[index].LocalIndex);

                Items[index].GlobalIndex = int.MinValue;

                base.SetItem(index, item);
            }

            private void UpdateIndexes(int start, int end, int globalDelta, int localDelta)
            {
                for (var i = start; i < end; i++)
                {
                    Items[i].GlobalIndex += globalDelta;
                    Items[i].LocalIndex += localDelta;
                }
            }
        }
    }
}