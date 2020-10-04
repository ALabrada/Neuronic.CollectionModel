using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.Observables;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Base class for filtered collection.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TContainer">The type of the container.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{TItem}" />
    /// <seealso cref="System.Collections.Generic.ICollection{TItem}" />
    public abstract class FilteredReadOnlyObservableCollectionBase<TItem, TContainer> :
        IReadOnlyObservableCollection<TItem>, ICollection<TItem>, IWeakEventListener where TContainer : FilterItemContainer<TItem>
    {
        /// <summary>
        /// The container comparer
        /// </summary>
        protected readonly IEqualityComparer<TContainer> ContainerComparer;

        /// <summary>
        ///     The filter.
        /// </summary>
        protected readonly Func<TItem, IObservable<bool>> Filter;

        /// <summary>
        ///     The source collection.
        /// </summary>
        protected readonly IReadOnlyObservableCollection<TItem> Source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableCollectionBase{TItem, TContainer}"/> class.
        /// </summary>
        /// <param name="items">The collection of item containers.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="itemComparer">The equality comparer for the items, in case <paramref name="source"/> is an index-less collection.</param>
        protected FilteredReadOnlyObservableCollectionBase(ObservableCollection<TContainer> items,
            IReadOnlyObservableCollection<TItem> source, Func<TItem, IObservable<bool>> filter, IEqualityComparer<TItem> itemComparer)
        {
            Source = source;
            Filter = filter;
            Items = items;
            ContainerComparer = new ContainerEqualityComparer<TItem, TContainer>(itemComparer);

            CollectionChangedEventManager.AddListener(Source, this);
            foreach (var item in source)
                Items.Add(CreateContainer(item));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableCollectionBase{TItem, TContainer}" /> class.
        /// </summary>
        /// <param name="items">The collection of item containers.</param>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="itemComparer">The equality comparer for the items, in case <paramref name="source"/> is an index-less collection.</param>
        /// <param name="triggers">
        ///     The names of the item's properties that can cause <paramref name="filter" /> to change its
        ///     value.
        /// </param>
        protected FilteredReadOnlyObservableCollectionBase(ObservableCollection<TContainer> items,
            IReadOnlyObservableCollection<TItem> source, Predicate<TItem> filter, IEqualityComparer<TItem> itemComparer, params string[] triggers)
            : this (items, source, x => new FunctionObservable<TItem,bool>(x, b => filter(b), triggers), itemComparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableCollectionBase{TItem, TContainer}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="itemComparer">The equality comparer for the items, in case <paramref name="source"/> is an index-less collection.</param>
        /// <param name="triggers">
        ///     The names of the item's properties that can cause <paramref name="filter" /> to change its
        ///     value.
        /// </param>
        protected FilteredReadOnlyObservableCollectionBase(IReadOnlyObservableCollection<TItem> source,
            Predicate<TItem> filter, IEqualityComparer<TItem> itemComparer, params string[] triggers)
            : this(new ObservableCollection<TContainer>(), source, filter, itemComparer, triggers)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FilteredReadOnlyObservableCollectionBase{TItem, TContainer}" /> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="filter">The filter predicate.</param>
        /// <param name="itemComparer">The equality comparer for the items, in case <paramref name="source"/> is an index-less collection.</param> 
        protected FilteredReadOnlyObservableCollectionBase(IReadOnlyObservableCollection<TItem> source, Func<TItem, IObservable<bool>> filter, IEqualityComparer<TItem> itemComparer)
            : this(new ObservableCollection<TContainer>(), source, filter, itemComparer)
        {
        }

        /// <summary>
        ///     Gets the item containers.
        /// </summary>
        /// <value>
        ///     The collection of item containers.
        /// </value>
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

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        public virtual bool Contains(TItem item)
        {
            return this.AsEnumerable().Contains(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public virtual void CopyTo(TItem[] array, int arrayIndex)
        {
            this.AsEnumerable().CopyTo(array, arrayIndex, array.Length);
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public virtual int Count => this.AsEnumerable().Count();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public abstract IEnumerator<TItem> GetEnumerator();

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Items.UpdateCollection(Source, e, o => CreateContainer((TItem) o), DestroyContainer, comparer: ContainerComparer);
        }

        /// <summary>
        ///     When implemented in a derived class, creates a container for an item that is included in the source collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Container for <paramref name="item" />.</returns>
        protected abstract TContainer CreateContainer(TItem item);

        /// <summary>
        ///     When implemented in a derived class, destroys a container when it's item is removed from the source collection.
        /// </summary>
        /// <param name="container">The container.</param>
        protected abstract void DestroyContainer(TContainer container);

        /// <summary>
        ///     Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(Source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs) e);
            return true;
        }
    }
}