using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Base class for the container lists used by the grouping collections.
    /// </summary>
    /// <typeparam name="TSource">The type of the source items.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{Neuronic.CollectionModel.Collections.Containers.GroupedItemContainer{TSource, TKey}}" />
    internal abstract class
        GroupingContainerList<TSource, TKey> : ObservableCollection<GroupedItemContainer<TSource, TKey>>
    {
        private bool _includeImplicitGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingContainerList{TSource, TKey}"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="collection">The initial container collection.</param>
        /// <param name="keyComparer">The key comparer.</param>
        /// <param name="groups">The initial groups.</param>
        /// <exception cref="System.ArgumentException">At least one of the explicit groups is already in use.</exception>
        protected GroupingContainerList(IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> owner,
            IEnumerable<GroupedItemContainer<TSource, TKey>> collection, IEqualityComparer<TKey> keyComparer,
            IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups) : base(collection)
        {
            KeyComparer = keyComparer;
            Groups = groups;
            Owner = owner;

            foreach (var group in Groups)
            {
                if (group.Owner != null)
                    throw new ArgumentException("At least one of the explicit groups is already in use.");
                group.Owner = Owner;
            }

            for (var i = 0; i < Items.Count; i++)
            {
                var container = Items[i];
                container.SourceIndex = i;
                container.GroupIndex = -1;
                container.Group = null;
                container.KeyChanged += ContainerOnKeyChanged;
            }
        }

        /// <summary>
        /// Gets the key comparer.
        /// </summary>
        /// <value>
        /// The key comparer.
        /// </value>
        protected IEqualityComparer<TKey> KeyComparer { get; }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        protected IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> Owner { get; }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public IReadOnlyObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> Groups { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether to include implicit groups.
        /// </summary>
        /// <value>
        ///     <see cref="GroupingReadOnlyObservableListSource{TSource,TKey}.IncludeImplicitGroups" />
        /// </value>
        public bool IncludeImplicitGroups
        {
            get => _includeImplicitGroups;
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

        /// <summary>
        /// Removes the group.
        /// </summary>
        /// <param name="group">The group.</param>
        protected abstract void RemoveGroup(ReadOnlyObservableGroup<TSource, TKey> group);

        /// <summary>
        /// Adds the group.
        /// </summary>
        /// <param name="group">The group.</param>
        protected abstract void AddGroup(ReadOnlyObservableGroup<TSource, TKey> group);

        /// <summary>
        /// Clears the groups.
        /// </summary>
        protected virtual void ClearGroups()
        {
            foreach (var group in Groups.ToList())
            {
                group.InternalItems.Clear();
                if (!group.IsExplicit)
                    RemoveGroup(group);
            }
        }

        /// <summary>
        /// Finds the group with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The group with key <paramref name="key"/>, or <c>null</c> if there is none.</returns>
        protected abstract ReadOnlyObservableGroup<TSource, TKey> FindGroup(TKey key);

        private void ContainerOnKeyChanged(object sender, EventArgs eventArgs)
        {
            ContainerOnKeyChanged((GroupedItemContainer<TSource, TKey>) sender);
        }

        /// <summary>
        ///     Occurs when the key of a container changes.
        /// </summary>
        /// <param name="container">The container.</param>
        protected virtual void ContainerOnKeyChanged(GroupedItemContainer<TSource, TKey> container)
        {
            var oldGroup = container.Group;
            if (oldGroup != null && KeyComparer.Equals(oldGroup.Key, container.Key))
                return;

            var group = FindGroup(container.Key);
            if (group == null && IncludeImplicitGroups)
            {
                group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false) {Owner = Owner};
                AddGroup(group);
            }

            if (oldGroup != null)
            {
                oldGroup.InternalItems.RemoveAt(container.GroupIndex);
                if (oldGroup.Count == 0 && !oldGroup.IsExplicit)
                    RemoveGroup(oldGroup);
                container.GroupIndex--;
                UpdateIndexesFrom(container); // Update the indexes of the previous group. 
            }
            container.Group = group;
            container.GroupIndex = -1;
            if (group != null)
            {
                FindGroupIndex(container); // Find the index of the item in the new group.
                UpdateIndexesFrom(container); // Update the indexes of the new group. 
                group.InternalItems.Insert(container.GroupIndex, container.Item);
            }
        }

        /// <summary>
        ///     Updates the source and group indexes, starting from the specified container.
        /// </summary>
        /// <param name="firstContainer">The first container.</param>
        protected void UpdateIndexesFrom(GroupedItemContainer<TSource, TKey> firstContainer)
        {
            UpdateIndexes(firstContainer.SourceIndex, firstContainer.GroupIndex, firstContainer.Group);
        }

        /// <summary>
        ///     Updates all source indexes, optionally including the group indexes of the items that belong to
        ///     <paramref name="group" />.
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
                if (group != null && group == container.Group)
                    container.GroupIndex = groupIndex++;
            }
        }

        /// <summary>
        ///     Finds and updates the group index of the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        protected void FindGroupIndex(GroupedItemContainer<TSource, TKey> container)
        {
            var i = container.SourceIndex - 1;
            // Find the source index of the previous item that belongs to the same group
            while (i >= 0 && Items[i].Group != container.Group)
                i--;
            container.GroupIndex = i < 0 ? 0 : Items[i].GroupIndex + 1;
        }

        /// <summary>
        ///     Removes the implicit groups.
        /// </summary>
        protected virtual void RemoveImplicitGroups()
        {
            foreach (var container in Items.Where(c => !c.Group.IsExplicit))
            {
                container.Group = null;
                container.GroupIndex = -1;
            }

            var groupsToRemove = new List<ReadOnlyObservableGroup<TSource, TKey>>(Groups.Count);
            groupsToRemove.AddRange(Groups.Where(g => !g.IsExplicit));

            foreach (var group in groupsToRemove)
            {
                group.InternalItems.Clear();
                RemoveGroup(group);
            }
        }

        /// <summary>
        ///     Adds the missing groups.
        /// </summary>
        protected void AddMissingGroups()
        {
            foreach (var container in Items)
            {
                var group = container.Group;
                if (group == null)
                {
                    group = FindGroup(container.Key);
                    if (group == null)
                    {
                        if (!IncludeImplicitGroups)
                            continue;
                        AddGroup(group =
                            new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false) {Owner = Owner});
                    }
                    container.Group = group;
                    container.GroupIndex = group.Count;
                    group.InternalItems.Add(container.Item);
                }
            }
        }

        /// <summary>
        ///     Inserts the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="container">The container.</param>
        protected override void InsertItem(int index, GroupedItemContainer<TSource, TKey> container)
        {
            base.InsertItem(index, container);

            var group = FindGroup(container.Key);
            if (group == null && IncludeImplicitGroups)
            {
                group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false) {Owner = Owner};
                AddGroup(group);
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
            container.KeyChanged += ContainerOnKeyChanged;
        }

        /// <summary>
        ///     Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
        /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            var container = Items[newIndex];
            var oldGroupIndex = container.GroupIndex;
            UpdateAllIndexes(container.Group); // Update all source & group indexes at once.
            container.Group?.InternalItems.Move(oldGroupIndex, container.GroupIndex);
        }

        /// <summary>
        ///     Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            var container = Items[index];
            var group = container.Group;
            var groupIndex = container.GroupIndex;

            container.Dispose();
            container.KeyChanged -= ContainerOnKeyChanged;

            base.RemoveItem(index);

            UpdateIndexesFrom(container);
            container.Group = null;
            container.GroupIndex = -1;
            container.SourceIndex = -1;

            if (group != null)
            {
                group.InternalItems.RemoveAt(groupIndex);
                if (group.Count == 0 && !group.IsExplicit)
                    RemoveGroup(group);
            }
        }

        /// <summary>
        ///     Sets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="container">The container.</param>
        protected override void SetItem(int index, GroupedItemContainer<TSource, TKey> container)
        {
            var oldContainer = Items[index];
            var oldGroup = oldContainer.Group;
            var oldGroupIndex = oldContainer.GroupIndex;

            oldContainer.Dispose();
            oldContainer.KeyChanged -= ContainerOnKeyChanged;

            base.SetItem(index, container);

            UpdateIndexesFrom(oldContainer);
            oldContainer.Group = null;
            oldContainer.GroupIndex = -1;
            oldContainer.SourceIndex = -1;

            var group = FindGroup(container.Key);
            if (group == null && IncludeImplicitGroups)
            {
                group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false) {Owner = Owner};
                AddGroup(group);
            }
            container.SourceIndex = index;
            container.Group = group;
            container.GroupIndex = -1;

            if (oldGroup == group && group != null)
            {
                FindGroupIndex(container);
                group.InternalItems[container.GroupIndex] = container.Item;
            }
            else
            {
                if (oldGroup != null)
                {
                    oldGroup.InternalItems.RemoveAt(oldGroupIndex);
                    if (oldGroup.Count == 0 && !oldGroup.IsExplicit)
                        RemoveGroup(oldGroup);
                }
                if (group != null)
                {
                    FindGroupIndex(container);
                    group.InternalItems.Insert(container.GroupIndex, container.Item);
                }
            }
            UpdateIndexesFrom(container);
            container.KeyChanged += ContainerOnKeyChanged;
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var container in Items)
            {
                container.Dispose();
                container.KeyChanged -= ContainerOnKeyChanged;
                container.Group = null;
                container.GroupIndex = -1;
                container.SourceIndex = -1;
            }

            base.ClearItems();

            ClearGroups();
        }
    }
}