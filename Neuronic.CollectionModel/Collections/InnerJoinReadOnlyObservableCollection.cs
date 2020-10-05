using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.WeakEventPattern;
using System.Windows;

namespace Neuronic.CollectionModel.Collections
{
    public class InnerJoinReadOnlyObservableCollection<TOuter, TInner, TKey, TResult> : IReadOnlyObservableCollection<TResult>, IWeakEventListener
    {
        private readonly IEnumerable<TOuter> _outerSource;
        private readonly IEnumerable<TInner> _innerSource;
        private readonly Func<TOuter, IObservable<TKey>> _outerKeySelector;
        private readonly Func<TInner, IObservable<TKey>> _innerKeySelector;
        private readonly Func<TOuter, TInner, IObservable<TResult>> _resultSelector;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly KeyValuePairEqualityComparer<TOuter> _outerComparer;
        private readonly KeyValuePairEqualityComparer<TInner> _innerComparer;

        private readonly ObservableDictionary<TKey, ObservableItemContainer<TOuter, TKey>> _outerItems;
        private readonly ObservableDictionary<TKey, ObservableItemContainer<TInner, TKey>> _innerItems;
        private readonly ObservableDictionary<TKey, ObservableItemContainer<Tuple<TOuter, TInner>, TResult>> _mergedItems;

        public InnerJoinReadOnlyObservableCollection(
            IEnumerable<TOuter> outerSource, IEnumerable<TInner> innerSource,
            Func<TOuter, IObservable<TKey>> outerKeySelector, Func<TInner, IObservable<TKey>> innerKeySelector,
            Func<TOuter, TInner, IObservable<TResult>> resultSelector, IEqualityComparer<TKey> keyComparer = null, 
            IEqualityComparer<TOuter> outerComparer = null, IEqualityComparer<TInner> innerComparer = null)
        {
            _outerSource = outerSource ?? throw new ArgumentNullException(nameof(outerSource));
            _innerSource = innerSource ?? throw new ArgumentNullException(nameof(innerSource));
            _outerKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
            _innerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
            _resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            _innerComparer = new KeyValuePairEqualityComparer<TInner>(innerComparer);
            _outerComparer = new KeyValuePairEqualityComparer<TOuter>(outerComparer);

            _outerItems = new ObservableDictionary<TKey, ObservableItemContainer<TOuter, TKey>>(
                new Dictionary<TKey, ObservableItemContainer<TOuter, TKey>>(_keyComparer));
            _innerItems = new ObservableDictionary<TKey, ObservableItemContainer<TInner, TKey>>(
                new Dictionary<TKey, ObservableItemContainer<TInner, TKey>>(_keyComparer));
            _mergedItems = new ObservableDictionary<TKey, ObservableItemContainer<Tuple<TOuter, TInner>, TResult>>(
                new Dictionary<TKey, ObservableItemContainer<Tuple<TOuter, TInner>, TResult>>(_keyComparer));

            foreach (var item in _outerSource)
                _outerItems.Add(CreateContainer(item, _outerKeySelector));
            foreach (var item in _innerSource)
                _innerItems.Add(CreateContainer(item, _innerKeySelector));
            var merge = _outerItems.Values.Join(_innerItems.Values, 
                outer => outer.Value, inner => inner.Value, Tuple.Create);
            foreach (var pair in merge)
                _mergedItems.Add(pair.Item1.Value, Merge(pair.Item1.Item, pair.Item2.Item));

            _outerItems.Values.CollectionChanged += OuterItemsOnCollectionChanged;
            _innerItems.Values.CollectionChanged += InnerItemsOnCollectionChanged;
            _mergedItems.Values.CollectionChanged += (sender, args) => OnCollectionChanged(args);
            _mergedItems.Values.PropertyChanged += (sender, args) => OnPropertyChanged(args);

            if (_innerSource is INotifyCollectionChanged innerNotifier)
                CollectionChangedEventManager.AddListener(innerNotifier, this);
            if (_outerSource is INotifyCollectionChanged outerNotifier)
                CollectionChangedEventManager.AddListener(outerNotifier, this);
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return _mergedItems.Values.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _mergedItems.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(CollectionChangedEventManager))
                return false;
            if (ReferenceEquals(sender, _innerSource))
            {
                _innerItems.UpdateCollection(_innerItems, (NotifyCollectionChangedEventArgs) e, 
                    o => CreateContainer((TInner) o, _innerKeySelector), DestroyContainer, _innerComparer);
            }
            else if (ReferenceEquals(sender, _outerItems))
            {
                _outerItems.UpdateCollection(_outerItems, (NotifyCollectionChangedEventArgs)e,
                    o => CreateContainer((TOuter)o, _outerKeySelector), DestroyContainer, _outerComparer);
            }

            return true;
        }

        private KeyValuePair<TKey, ObservableItemContainer<T, TKey>> CreateContainer<T>(T item, Func<T, IObservable<TKey>> selector)
        {
            var container = new ObservableItemContainer<T, TKey>(item, selector(item), _keyComparer); 
            container.ValueChanged += ContainerOnValueChanged;
            return new KeyValuePair<TKey, ObservableItemContainer<T, TKey>>(container.Value, container);
        }

        private void DestroyContainer<T>(KeyValuePair<TKey, ObservableItemContainer<T, TKey>> item)
        {
            item.Value.ValueChanged -= ContainerOnValueChanged;
            item.Value.Dispose();
        }

        private void ContainerOnValueChanged(object sender, ValueChangedEventArgs<TKey> e)
        {
            if (sender is ObservableItemContainer<TOuter, TKey> outerContainer)
            {
                _outerItems.Remove(e.OldValue);
                _outerItems[e.NewValue] = outerContainer;
            }
            else if (sender is ObservableItemContainer<TInner, TKey> innerContainer)
            {
                _innerItems.Remove(e.OldValue);
                _innerItems[e.NewValue] = innerContainer;
            }
        }

        private void InnerItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ObservableItemContainer<TInner, TKey> item in e.NewItems)
                        if (_outerItems.TryGetValue(item.Value, out var otherItem))
                        {
                            var container = Merge(otherItem.Item, item.Item);
                            _mergedItems.Add(item.Value, container);
                        }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ObservableItemContainer<TInner, TKey> item in e.OldItems)
                        if (_mergedItems.TryGetValue(item.Value, out var container))
                        {
                            DestroyContainer(container);
                            _mergedItems.Remove(item.Value);
                        }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (ObservableItemContainer<TInner, TKey> item in e.OldItems)
                        if (_mergedItems.TryGetValue(item.Value, out var container))
                        {
                            DestroyContainer(container);
                            _mergedItems.Remove(item.Value);
                        }

                    foreach (ObservableItemContainer<TInner, TKey> item in e.NewItems)
                        if (_outerItems.TryGetValue(item.Value, out var otherItem))
                        {
                            var container = Merge(otherItem.Item, item.Item);
                            _mergedItems.Add(item.Value, container);
                        }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var container in _mergedItems.Values)
                        DestroyContainer(container);
                    _mergedItems.Clear();
                    break;

                default:
                    throw new NotSupportedException($"Cannot handle {e.Action} in a dictionary.");
            }
        }

        private void OuterItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ObservableItemContainer<TOuter, TKey> item in e.NewItems)
                        if (_innerItems.TryGetValue(item.Value, out var otherItem))
                        {
                            var container = Merge(item.Item, otherItem.Item);
                            _mergedItems.Add(item.Value, container);
                        }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ObservableItemContainer<TOuter, TKey> item in e.OldItems)
                        if (_mergedItems.TryGetValue(item.Value, out var container))
                        {
                            DestroyContainer(container);
                            _mergedItems.Remove(item.Value);
                        }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (ObservableItemContainer<TOuter, TKey> item in e.OldItems)
                        if (_mergedItems.TryGetValue(item.Value, out var container))
                        {
                            DestroyContainer(container);
                            _mergedItems.Remove(item.Value);
                        }

                    foreach (ObservableItemContainer<TOuter, TKey> item in e.NewItems)
                        if (_innerItems.TryGetValue(item.Value, out var otherItem))
                        {
                            var container = Merge(item.Item, otherItem.Item);
                            _mergedItems.Add(item.Value, container);
                        }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var container in _mergedItems.Values)
                        DestroyContainer(container);
                    _mergedItems.Clear();
                    break;

                default:
                    throw new NotSupportedException($"Cannot handle {e.Action} in a dictionary.");
            }
        }

        private ObservableItemContainer<Tuple<TOuter, TInner>, TResult> Merge(
            TOuter outerItem, TInner innerItem)
        {
            var tuple = Tuple.Create(outerItem, innerItem);
            var observable = _resultSelector(tuple.Item1, tuple.Item2);
            var container = new ObservableItemContainer<Tuple<TOuter, TInner>, TResult>(tuple, observable);
            container.ValueChanged += MergedContainerOnValueChanged;
            return container;
        }

        private void DestroyContainer(ObservableItemContainer<Tuple<TOuter, TInner>, TResult> item)
        {
            item.ValueChanged -= MergedContainerOnValueChanged;
            item.Dispose();
        }

        private void MergedContainerOnValueChanged(object sender, ValueChangedEventArgs<TResult> e)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, 
                e.NewValue, e.OldValue));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        class KeyValuePairEqualityComparer<T> : EqualityComparer<KeyValuePair<TKey, ObservableItemContainer<T, TKey>>>
        {
            private readonly ContainerEqualityComparer<T, ObservableItemContainer<T, TKey>> _inner;

            public KeyValuePairEqualityComparer(ContainerEqualityComparer<T, ObservableItemContainer<T, TKey>> inner)
            {
                _inner = inner;
            }

            public KeyValuePairEqualityComparer(IEqualityComparer<T> inner) 
                : this (new ContainerEqualityComparer<T, ObservableItemContainer<T, TKey>>(inner))
            {
                
            }

            public override bool Equals(KeyValuePair<TKey, ObservableItemContainer<T, TKey>> x, KeyValuePair<TKey, ObservableItemContainer<T, TKey>> y)
            {
                return _inner.Equals(x.Value, y.Value);
            }

            public override int GetHashCode(KeyValuePair<TKey, ObservableItemContainer<T, TKey>> obj)
            {
                return _inner.GetHashCode(obj.Value);
            }
        }
    }
}