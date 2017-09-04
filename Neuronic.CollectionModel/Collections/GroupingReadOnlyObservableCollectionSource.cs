using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Groups the items from a specified sequence according to some criteria.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{Neuronic.CollectionModel.ReadOnlyObservableGroup{TSource, TKey}}" />
    public class
        GroupingReadOnlyObservableCollectionSource<TSource, TKey> : CustomReadOnlyObservableCollection<
            ReadOnlyObservableGroup<TSource, TKey>>
    {
        private readonly ContainerList _containers;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly INotifyCollectionChanged _source;
        private readonly IEqualityComparer<GroupedItemContainer<TSource, TKey>> _sourceComparer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupingReadOnlyObservableListSource{TSource, TKey}" /> class.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="keySelector">The function used to obtain keys that represent the items.</param>
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs" /> events.
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableCollectionSource(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TSource> sourceComparer, params string[] triggers)
            : this(
                source, keySelector, keyComparer, sourceComparer, triggers,
                new ObservableDictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>>(
                    new Dictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>>(keyComparer)))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupingReadOnlyObservableListSource{TSource, TKey}" /> class.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="keySelector">The function used to obtain keys that represent the items.</param>
        /// <param name="keyComparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="sourceComparer">
        ///     A comparer for the source items. This is only used if the source collection is not a list
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs" /> events.
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableCollectionSource(IEnumerable<TSource> source,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TSource> sourceComparer, params string[] triggers)
            : this(
                source, keySelector, keyComparer, sourceComparer, triggers,
                new ObservableDictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>>(
                    explicitGroups.ToDictionary(g => g.Key, g => g, keyComparer)))
        {
        }

        private GroupingReadOnlyObservableCollectionSource(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TSource> sourceComparer, string[] triggers,
            ObservableDictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>> groups) : base (groups.Values)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _keySelector = keySelector;
            _sourceComparer =
                new ContainerEqualityComparer<TSource, GroupedItemContainer<TSource, TKey>>(sourceComparer);
            keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            triggers = triggers ?? new string[0];
            var initialContainers = source.Select(i => new GroupedItemContainer<TSource, TKey>(i, _keySelector));
            _containers = new ContainerList(this, initialContainers, keyComparer, triggers, groups);
            _source = source as INotifyCollectionChanged;
            if (_source != null)
                CollectionChangedEventManager.AddListener(_source, this);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to include implicit groups.
        /// </summary>
        /// <value>
        ///     if <c>true</c>, new groups (implicit) will be created for items that do not belong to the explicit groups;
        ///     otherwise, any source item that do not belong to any of the explicit groups will be discarded.
        /// </value>
        public bool IncludeImplicitGroups
        {
            get => _containers.IncludeImplicitGroups;
            set => _containers.IncludeImplicitGroups = value;
        }

        /// <summary>
        /// Called when a change notification is received from the source or the groups collection.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <returns>
        /// <c>true</c> if the event was handled; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return base.OnReceiveWeakEvent(managerType, sender, e);
            OnSourceChanged(sender, (NotifyCollectionChangedEventArgs) e);
            return true;
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _containers.UpdateCollection((IEnumerable<TSource>) sender, e,
                o => new GroupedItemContainer<TSource, TKey>((TSource) o, _keySelector), comparer: _sourceComparer);
        }

        /// <summary>
        ///     Internal list of item containers.
        /// </summary>
        private class ContainerList : GroupingContainerList<TSource, TKey>
        {
            private readonly ObservableDictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>> _groups;

            /// <summary>
            ///     Initializes a new instance of the <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.ContainerList" />
            ///     class.
            /// </summary>
            /// <param name="owner">The owning grouping source.</param>
            /// <param name="collection">The initial containers.</param>
            /// <param name="keyComparer">The key equality comparer</param>
            /// <param name="triggers">The triggers.</param>
            /// <param name="groups">The initial groups.</param>
            public ContainerList(GroupingReadOnlyObservableCollectionSource<TSource, TKey> owner,
                IEnumerable<GroupedItemContainer<TSource, TKey>> collection, IEqualityComparer<TKey> keyComparer,
                string[] triggers, ObservableDictionary<TKey, ReadOnlyObservableGroup<TSource, TKey>> groups)
                : base(owner, collection, keyComparer, triggers, groups.Values)
            {
                _groups = groups;

                if (Groups.Count > 0 || IncludeImplicitGroups)
                    AddMissingGroups();
            }

            protected override void RemoveGroup(ReadOnlyObservableGroup<TSource, TKey> group)
            {
                _groups.Remove(group.Key);
            }

            protected override void AddGroup(ReadOnlyObservableGroup<TSource, TKey> group)
            {
                _groups.Add(group.Key, group);
            }

            protected override ReadOnlyObservableGroup<TSource, TKey> FindGroup(TKey key)
            {
                ReadOnlyObservableGroup<TSource, TKey> group;
                return _groups.TryGetValue(key, out group) ? group : null;
            }
        }
    }
}