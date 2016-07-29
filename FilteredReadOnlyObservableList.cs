using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public class FilteredReadOnlyObservableList<T> :
        FilteredReadOnlyObservableCollectionBase<T, IndexedItemContainer<T>>, IReadOnlyObservableList<T>, IList<T>
    {
        public FilteredReadOnlyObservableList(IReadOnlyObservableCollection<T> source, Predicate<T> filter,
            params string[] triggers) : base(source, filter, triggers)
        {
            UpdateIndexes(0, Items.Count);
            LocalItems =
                new ObservableCollection<T>(from container in Items where container.IsIncluded select container.Item);


            Items.CollectionChanged += ItemsOnCollectionChanged;
            LocalItems.CollectionChanged += (sender, args) => OnCollectionChanged(args);
            ((INotifyPropertyChanged) LocalItems).PropertyChanged += (sender, args) => OnPropertyChanged(args);
        }

        protected ObservableCollection<T> LocalItems { get; }

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

        public override IEnumerator<T> GetEnumerator()
        {
            return LocalItems.GetEnumerator();
        }

        public override int Count => LocalItems.Count;
        public T this[int index] => LocalItems[index];

        T IList<T>.this[int index]
        {
            get { return this[index]; }

            set { throw new InvalidOperationException(); }
        }

        protected override IndexedItemContainer<T> CreateContainer(T item)
        {
            var container = new IndexedItemContainer<T>(item, Filter);
            container.IsIncludedChanged += ContainerOnIsIncludedChanged;
            container.AttachTriggers(Triggers);
            return container;
        }

        protected override void DestroyContainer(IndexedItemContainer<T> container)
        {
            container.DetachTriggers(Triggers);
            container.IsIncludedChanged -= ContainerOnIsIncludedChanged;
        }

        private void ContainerOnIsIncludedChanged(object sender, EventArgs eventArgs)
        {
            var container = (IndexedItemContainer<T>)sender;
            if (container.IsIncluded)
            {
                UpdateIndexes(container.GlobalIndex, Items.Count);
                LocalItems.Insert(container.LocalIndex, container.Item);
            }
            else
            {
                var localIndex = container.LocalIndex;
                UpdateIndexes(container.GlobalIndex, Items.Count);
                LocalItems.RemoveAt(localIndex);
            }
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IndexedItemContainer<T> newContainer, oldContainer;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    OnContainerAddedToItems(newContainer, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems.Count == 1);
                    oldContainer = (IndexedItemContainer<T>) e.OldItems[0];
                    OnContainerRemovedFromItems(oldContainer, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug.Assert(e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    OnContainerMovedInItems(newContainer, e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.OldItems.Count == 1 && e.NewItems.Count == 1);
                    newContainer = (IndexedItemContainer<T>) e.NewItems[0];
                    oldContainer = (IndexedItemContainer<T>) e.OldItems[0];
                    Debug.Assert(e.OldStartingIndex == e.NewStartingIndex);
                    OnContainerReplacedInItems(newContainer, oldContainer, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    LocalItems.Clear();
                    Debug.Assert(Items.Count == 0);
                    break;
            }
        }

        private void OnContainerReplacedInItems(IndexedItemContainer<T> newItem, IndexedItemContainer<T> oldItem, int index)
        {
            if (newItem.IsIncluded && !oldItem.IsIncluded)
            {
                UpdateIndexes(index, Items.Count);
                LocalItems.Insert(newItem.LocalIndex, newItem.Item);
            }
            else if (oldItem.IsIncluded && !newItem.IsIncluded)
            {
                UpdateIndexes(index, Items.Count);
                LocalItems.RemoveAt(oldItem.LocalIndex);
            }
            else
            {
                newItem.GlobalIndex = oldItem.GlobalIndex;
                newItem.LocalIndex = oldItem.LocalIndex;
                LocalItems[newItem.LocalIndex] = newItem.Item;
            }
            oldItem.GlobalIndex = int.MinValue;
            oldItem.LocalIndex = int.MinValue;
        }

        private void OnContainerMovedInItems(IndexedItemContainer<T> item, int oldIndex, int newIndex)
        {
            var oldLocalIndex = item.LocalIndex;
            if (oldIndex > newIndex)
                UpdateIndexes(newIndex, oldIndex);
            else
                UpdateIndexes(oldIndex, newIndex + 1);
            var newLocalIndex = item.LocalIndex;

            if (item.IsIncluded)
                LocalItems.Move(oldLocalIndex, newLocalIndex);
        }

        private void OnContainerRemovedFromItems(IndexedItemContainer<T> item, int index)
        {
            UpdateIndexes(index, Items.Count);
            if (item.IsIncluded)
                LocalItems.RemoveAt(item.LocalIndex);
            item.LocalIndex = item.GlobalIndex = int.MinValue;
        }

        private void OnContainerAddedToItems(IndexedItemContainer<T> item, int index)
        {
            UpdateIndexes(index, Items.Count);
            if (item.IsIncluded)
                LocalItems.Insert(item.LocalIndex, item.Item);
        }

        private void UpdateIndexes(int start, int end)
        {
            var lastLocal = start == 0 ? -1 : Items[start - 1].LocalIndex;
            for (var i = start; i < end; i++)
            {
                Items[i].GlobalIndex = i;
                lastLocal = Items[i].LocalIndex = Items[i].IsIncluded ? lastLocal + 1 : lastLocal; // The index it occupies or the one it should.
            }
        }
    }
}