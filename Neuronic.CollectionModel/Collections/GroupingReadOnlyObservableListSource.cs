using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Neuronic.CollectionModel.Collections.Containers;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    ///     Groups the items from a specified sequence according to some criteria.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    /// <para>
    ///     You should use <see cref="GroupingReadOnlyObservableCollectionSource{TSource,TKey}"/> instead of this class.
    ///     It works faster and, anyway, the order of the groups does not necessarily depends on the order of the source
    ///     elements, so there is no real need for indexing the groups. 
    /// </para>
    /// </remarks>
    public class GroupingReadOnlyObservableListSource<TSource, TKey> :
        ReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>>, IWeakEventListener
    {
        private readonly ContainerList _containers;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly IEqualityComparer<GroupedItemContainer<TSource, TKey>> _sourceComparer;
        private readonly INotifyCollectionChanged _source;

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
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.    
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableListSource(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TSource> sourceComparer, params string[] triggers)
            : this(
                source, keySelector, keyComparer, sourceComparer, triggers,
                new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>())
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
        ///     and does not provide index information in its <see cref="NotifyCollectionChangedEventArgs"/> events.  
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableListSource(IEnumerable<TSource> source,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TSource> sourceComparer, params string[] triggers)
            : this(
                source, keySelector, keyComparer, sourceComparer, triggers,
                new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>(explicitGroups))
        {
        }

        private GroupingReadOnlyObservableListSource(IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TSource> sourceComparer, string[] triggers,
            ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups) : base(groups)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _keySelector = keySelector;
            _sourceComparer = new ContainerEqualityComparer<TSource, GroupedItemContainer<TSource, TKey>>(sourceComparer);
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
            get { return _containers.IncludeImplicitGroups; }
            set { _containers.IncludeImplicitGroups = value; }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            OnSourceChanged(sender, (NotifyCollectionChangedEventArgs) e);
            return true;
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _containers.UpdateCollection((IEnumerable<TSource>) sender, e,
                o => new GroupedItemContainer<TSource, TKey>((TSource) o, _keySelector), comparer: _sourceComparer);
        }

        /// <summary>
        /// Internal list of item containers.
        /// </summary>
        private class ContainerList : GroupingContainerList<TSource, TKey>
        {
            private readonly ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> _groups;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerList"/> class.
            /// </summary>
            /// <param name="owner">The owning grouping source.</param>
            /// <param name="collection">The initial containers.</param>
            /// <param name="keyComparer">The key equality comparer</param>
            /// <param name="triggers">The triggers.</param>
            /// <param name="groups">The initial groups.</param>
            public ContainerList(GroupingReadOnlyObservableListSource<TSource, TKey> owner,
                IEnumerable<GroupedItemContainer<TSource, TKey>> collection, IEqualityComparer<TKey> keyComparer,
                string[] triggers, ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups)
                : base(owner, collection, keyComparer, triggers, groups.CollectionAsObservable())
            {
                _groups = groups;

                if (Groups.Count > 0 || IncludeImplicitGroups)
                    AddMissingGroups();
            }

            protected override void RemoveGroup(ReadOnlyObservableGroup<TSource, TKey> group)
            {
                _groups.Remove(group);
            }

            protected override void AddGroup(ReadOnlyObservableGroup<TSource, TKey> group)
            {
                _groups.Add(group);
            }

            protected override void ClearGroups()
            {
                var groups = _groups;
                for (var i = groups.Count - 1; i >= 0; i--)
                {
                    groups[i].InternalItems.Clear();
                    if (!groups[i].IsExplicit)
                        groups.RemoveAt(i);
                }
            }

            protected override ReadOnlyObservableGroup<TSource, TKey> FindGroup(TKey key)
            {
                return _groups.SingleOrDefault(g => KeyComparer.Equals(g.Key, key));
            }

            /// <summary>
            /// Removes the implicit groups.
            /// </summary>
            protected override void RemoveImplicitGroups()
            {
                foreach (var container in Items.Where(c => !c.Group.IsExplicit))
                {
                    container.Group = null;
                    container.GroupIndex = -1;
                }

                var groups = _groups;
                for (var i = groups.Count - 1; i >= 0; i--)
                    if (!groups[i].IsExplicit)
                    {
                        groups[i].InternalItems.Clear();
                        groups.RemoveAt(i);
                    }
            }
        }
    }
}