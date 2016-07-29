using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public abstract class FilteredReadOnlyObservableCollectionBase<TItem, TContainer> :
        IReadOnlyObservableCollection<TItem>, ICollection<TItem> where TContainer : ItemContainer<TItem>
    {
        protected readonly Predicate<TItem> Filter;
        protected readonly IReadOnlyObservableCollection<TItem> Source;
        protected readonly string[] Triggers;

        protected FilteredReadOnlyObservableCollectionBase(ObservableCollection<TContainer> items,
            IReadOnlyObservableCollection<TItem> source, Predicate<TItem> filter, params string[] triggers)
        {
            Source = source;
            Filter = filter;
            Items = items;
            Triggers = triggers;

            CollectionChangedEventManager.AddHandler(Source, SourceOnCollectionChanged);
            foreach (var item in source)
                Items.Add(CreateContainer(item));
        }

        protected FilteredReadOnlyObservableCollectionBase(IReadOnlyObservableCollection<TItem> source,
            Predicate<TItem> filter, params string[] triggers)
            : this(new ObservableCollection<TContainer>(), source, filter, triggers)
        {
        }

        protected ObservableCollection<TContainer> Items { get; }
        bool ICollection<TItem>.IsReadOnly => true;

        void ICollection<TItem>.Add(TItem item)
        {
            throw new InvalidOperationException();
        }

        void ICollection<TItem>.Clear()
        {
            throw new InvalidOperationException();
        }

        bool ICollection<TItem>.Remove(TItem item)
        {
            throw new InvalidOperationException();
        }

        public virtual bool Contains(TItem item)
        {
            return this.AsEnumerable().Contains(item);
        }

        public virtual void CopyTo(TItem[] array, int arrayIndex)
        {
            this.AsEnumerable().CopyTo(array, arrayIndex, array.Length);
        }

        public virtual int Count => this.AsEnumerable().Count();
        public abstract IEnumerator<TItem> GetEnumerator();
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Items.UpdateCollection(Source, e, o => CreateContainer((TItem) o), DestroyContainer);
        }

        protected abstract TContainer CreateContainer(TItem item);
        protected abstract void DestroyContainer(TContainer container);

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}