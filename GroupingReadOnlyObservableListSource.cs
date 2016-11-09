using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Groups the items from a specified sequence according to some criteria.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class GroupingReadOnlyObservableListSource<TSource, TKey> :
        ReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>>
    {
        private readonly ContainerList _containers;
        private readonly Func<TSource, TKey> _keySelector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupingReadOnlyObservableListSource{TSource, TKey}" /> class.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="keySelector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableListSource(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, params string[] triggers)
            : this(
                source, keySelector, comparer, triggers,
                new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupingReadOnlyObservableListSource{TSource, TKey}" /> class.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="explicitGroups">The explicit groups.</param>
        /// <param name="keySelector">The function used to obtain keys that represent the items.</param>
        /// <param name="comparer">
        ///     The comparer used for key comparison. Specify <c>null</c> to use the default comparer for
        ///     <typeparamref name="TKey" />.
        /// </param>
        /// <param name="triggers">
        ///     The names of the source item's properties that can alter the value of
        ///     <paramref name="keySelector" />.
        /// </param>
        public GroupingReadOnlyObservableListSource(IReadOnlyObservableCollection<TSource> source,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, params string[] triggers)
            : this(
                source, keySelector, comparer, triggers,
                new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>(explicitGroups))
        {
        }

        private GroupingReadOnlyObservableListSource(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, string[] triggers,
            ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups) : base(groups)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _keySelector = keySelector;
            triggers = triggers ?? new string[0];
            var equals = comparer == null
                ? new Func<TKey, TKey, bool>((a, b) => Equals(a, b))
                : ((a, b) => comparer.Equals(a, b));
            var initialContainers = source.Select(i => new GroupedItemContainer<TSource, TKey>(i, _keySelector));
            _containers = new ContainerList(initialContainers, equals, triggers, groups);
            CollectionChangedEventManager.AddHandler(source, OnSourceChanged);
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

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _containers.UpdateCollection((IEnumerable<TSource>) sender, e,
                o => new GroupedItemContainer<TSource, TKey>((TSource) o, _keySelector));
        }

        /// <summary>
        /// Internal list of item containers.
        /// </summary>
        protected class ContainerList : ObservableCollection<GroupedItemContainer<TSource, TKey>>
        {
            private readonly Func<TKey, TKey, bool> _equals;
            private readonly ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> _groups;
            private readonly string[] _triggers;
            private bool _includeImplicitGroups;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerList"/> class.
            /// </summary>
            /// <param name="collection">The initial containers.</param>
            /// <param name="equals">The key comparison function.</param>
            /// <param name="triggers">The triggers.</param>
            /// <param name="groups">The initial groups.</param>
            public ContainerList(IEnumerable<GroupedItemContainer<TSource, TKey>> collection,
                Func<TKey, TKey, bool> equals,
                string[] triggers, ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups)
                : base(collection)
            {
                _equals = equals;
                _triggers = triggers;
                _groups = groups;

                UpdateAllIndexes();
                if ((_groups.Count > 0) || IncludeImplicitGroups)
                    AddMissingGroups();
            }

            /// <summary>
            /// Gets or sets a value indicating whether to include implicit groups.
            /// </summary>
            /// <value>
            /// <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups"/>
            /// </value>
            public bool IncludeImplicitGroups
            {
                get { return _includeImplicitGroups; }
                set
                {
                    if (_includeImplicitGroups == value)
                        return;
                    _includeImplicitGroups = value;
                    if (_includeImplicitGroups)
                        AddMissingGroups();
                    else
                        RemoveImplicitGroups();
                }
            }

            private void ContainerOnKeyChanged(object sender, EventArgs eventArgs)
            {
                ContainerOnKeyChanged((GroupedItemContainer<TSource, TKey>)sender);
            }

            /// <summary>
            /// Occurs when the key of a container changes.
            /// </summary>
            /// <param name="container">The container.</param>
            protected virtual void ContainerOnKeyChanged(GroupedItemContainer<TSource, TKey> container)
            {
                var oldGroup = container.Group;
                if ((oldGroup != null) && _equals(oldGroup.Key, container.Key))
                    return;

                var group = _groups.SingleOrDefault(g => _equals(g.Key, container.Key));
                if ((@group == null) && IncludeImplicitGroups)
                {
                    @group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _groups.Add(@group);
                }

                if (oldGroup != null)
                {
                    oldGroup.InternalItems.RemoveAt(container.GroupIndex);
                    container.GroupIndex--;
                    UpdateIndexesFrom(container); // Update the indexes of the previous group. 
                }
                container.Group = @group;
                container.GroupIndex = -1;
                if (@group != null)
                {
                    FindGroupIndex(container); // Find the index of the item in the new group.
                    UpdateIndexesFrom(container); // Update the indexes of the new group. 
                    @group.InternalItems.Insert(container.GroupIndex, container.Item);
                }
            }

            /// <summary>
            /// Updates the source and group indexes, starting from the specified container.
            /// </summary>
            /// <param name="firstContainer">The first container.</param>
            protected void UpdateIndexesFrom(GroupedItemContainer<TSource, TKey> firstContainer)
            {
                UpdateIndexes(firstContainer.SourceIndex, firstContainer.GroupIndex, firstContainer.Group);
            }

            /// <summary>
            /// Updates all source indexes, optionally including the group indexes of the items that belong to <paramref name="group"/>.
            /// </summary>
            /// <param name="group">The group.</param>
            protected void UpdateAllIndexes(ReadOnlyObservableGroup<TSource, TKey> group = null)
            {
                UpdateIndexes(0, 0, group);
            }

            private void UpdateIndexes(int start, int groupIndex, ReadOnlyObservableGroup<TSource, TKey> group)
            {
                for (var i = start; i < Count; i++)
                {
                    var container = Items[i];
                    container.SourceIndex = i;
                    if (group == container.Group)
                        container.GroupIndex = groupIndex++;
                }
            }

            /// <summary>
            /// Finds and updates the group index of the specified container.
            /// </summary>
            /// <param name="container">The container.</param>
            protected void FindGroupIndex(GroupedItemContainer<TSource, TKey> container)
            {
                var i = container.SourceIndex - 1;
                    // Find the source index of the previous item that belongs to the same group
                while ((i >= 0) && (Items[i].Group != container.Group))
                    i--;
                container.GroupIndex = i < 0 ? 0 : Items[i].GroupIndex + 1;
            }

            /// <summary>
            /// Removes the implicit groups.
            /// </summary>
            protected void RemoveImplicitGroups()
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

            /// <summary>
            /// Adds the missing groups.
            /// </summary>
            protected void AddMissingGroups()
            {
                var groups = _groups;
                var groupCounts = groups.Select(g => g.Count).ToList();
                foreach (var container in Items)
                {
                    var group = container.Group;
                    if (group == null)
                    {
                        var groupIndex = groups.TakeWhile(g => !_equals(g.Key, container.Key)).Count();
                        if (groupIndex == groups.Count)
                        {
                            if (!IncludeImplicitGroups)
                                continue;
                            group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                            groups.Add(group);
                            groupCounts.Add(0);
                        }
                        container.Group = group;
                        container.GroupIndex = groupCounts[groupIndex];
                        groupCounts[groupIndex] = container.GroupIndex + 1;
                    }
                }
            }

            /// <summary>
            /// Removes all items from the collection.
            /// </summary>
            protected override void ClearItems()
            {
                foreach (var container in Items)
                {
                    container.DetachTriggers(_triggers);
                    container.KeyChanged -= ContainerOnKeyChanged;
                    container.Group = null;
                    container.GroupIndex = -1;
                    container.SourceIndex = -1;
                }

                base.ClearItems();

                var groups = _groups;
                for (var i = groups.Count - 1; i >= 0; i--)
                {
                    groups[i].InternalItems.Clear();
                    if (!groups[i].IsExplicit)
                        groups.RemoveAt(i);
                }
            }

            /// <summary>
            /// Inserts the item.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <param name="container">The container.</param>
            protected override void InsertItem(int index, GroupedItemContainer<TSource, TKey> container)
            {
                base.InsertItem(index, container);

                var group = _groups.SingleOrDefault(g => _equals(g.Key, container.Key));
                if ((group == null) && IncludeImplicitGroups)
                {
                    group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _groups.Add(group);
                }

                container.SourceIndex = index;
                container.Group = group;
                container.GroupIndex = -1;
                if (group != null)
                {
                    FindGroupIndex(container);
                    group.InternalItems.Insert(container.GroupIndex, container.Item);
                }
                UpdateIndexesFrom(container);
                container.AttachTriggers(_triggers);
                container.KeyChanged += ContainerOnKeyChanged;
            }

            /// <summary>
            /// Moves the item at the specified index to a new location in the collection.
            /// </summary>
            /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
            /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
            protected override void MoveItem(int oldIndex, int newIndex)
            {
                base.MoveItem(oldIndex, newIndex);
                var container = Items[newIndex];
                var oldGroupIndex = container.GroupIndex;
                UpdateAllIndexes(container.Group); // Update all source & group indexes at once.
                container.Group.InternalItems.Move(oldGroupIndex, container.GroupIndex);
            }

            /// <summary>
            /// Removes the item at the specified index of the collection.
            /// </summary>
            /// <param name="index">The zero-based index of the element to remove.</param>
            protected override void RemoveItem(int index)
            {
                var container = Items[index];
                var group = container.Group;
                var groupIndex = container.GroupIndex;

                container.DetachTriggers(_triggers);
                container.KeyChanged -= ContainerOnKeyChanged;

                base.RemoveItem(index);

                UpdateIndexesFrom(container);
                container.Group = null;
                container.GroupIndex = -1;
                container.SourceIndex = -1;

                if (group != null)
                {
                    group.InternalItems.RemoveAt(groupIndex);
                    if ((group.Count == 0) && !group.IsExplicit)
                        _groups.Remove(group);
                }
            }

            /// <summary>
            /// Sets the item.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <param name="container">The container.</param>
            protected override void SetItem(int index, GroupedItemContainer<TSource, TKey> container)
            {
                var oldContainer = Items[index];
                var oldGroup = oldContainer.Group;
                var oldGroupIndex = oldContainer.GroupIndex;

                oldContainer.DetachTriggers(_triggers);
                oldContainer.KeyChanged -= ContainerOnKeyChanged;

                base.SetItem(index, container);

                UpdateIndexesFrom(oldContainer);
                oldContainer.Group = null;
                oldContainer.GroupIndex = -1;
                oldContainer.SourceIndex = -1;

                var group = _groups.SingleOrDefault(g => _equals(g.Key, container.Key));
                if ((group == null) && IncludeImplicitGroups)
                {
                    group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _groups.Add(group);
                }
                container.SourceIndex = index;
                container.Group = group;
                container.GroupIndex = -1;

                if ((oldGroup == group) && (group != null))
                {
                    FindGroupIndex(container);
                    group.InternalItems[container.GroupIndex] = container.Item;
                }
                else
                {
                    if (oldGroup != null)
                    {
                        oldGroup.InternalItems.RemoveAt(oldGroupIndex);
                        if ((oldGroup.Count == 0) && !oldGroup.IsExplicit)
                            _groups.Remove(oldGroup);
                    }
                    if (group != null)
                    {
                        FindGroupIndex(container);
                        group.InternalItems.Insert(container.GroupIndex, container.Item);
                    }
                }
                UpdateIndexesFrom(container);
                container.AttachTriggers(_triggers);
                container.KeyChanged += ContainerOnKeyChanged;
            }
        }
    }
}