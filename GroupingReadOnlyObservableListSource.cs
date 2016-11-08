using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Neuronic.CollectionModel
{
    public class GroupingReadOnlyObservableListSource<TSource, TKey> : ReadOnlyObservableList<ReadOnlyObservableGroup<TSource, TKey>>
    {
        private readonly IReadOnlyObservableCollection<TSource> _source;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly Func<TKey, TKey, bool> _equals;
        private readonly string[] _triggers;
        private readonly ItemList _containers;
        private readonly ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> _groups = new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>();
        private bool _includeImplicitGroups;

        public GroupingReadOnlyObservableListSource(IReadOnlyObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, params string[] triggers)
            : this(new ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>>())
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            _source = source;
            _keySelector = keySelector;
            _triggers = triggers ?? new string[0];
            _equals = comparer == null
                ? new Func<TKey, TKey, bool>((a, b) => Equals(a, b))
                : ((a, b) => comparer.Equals(a, b));
            _containers = new ItemList(this,
                source.Select(i => new GroupedItemContainer<TSource, TKey>(i, _keySelector)));
            CollectionChangedEventManager.AddHandler(source, OnSourceChanged);
        }

        public GroupingReadOnlyObservableListSource(IReadOnlyObservableCollection<TSource> source,
            IEnumerable<ReadOnlyObservableGroup<TSource, TKey>> explicitGroups,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, params string[] triggers) : this (source, keySelector, comparer, triggers)
        {
            foreach (var group in explicitGroups)
                _groups.Add(group);
            if (_groups.Count > 0)
                _containers.AddMissingGroups();
        }

        private GroupingReadOnlyObservableListSource(ObservableCollection<ReadOnlyObservableGroup<TSource, TKey>> groups) : base (groups)
        {
            
        }

        public bool IncludeImplicitGroups
        {
            get { return _includeImplicitGroups; }
            set
            {
                if (_includeImplicitGroups == value)
                    return;
                _includeImplicitGroups = value;
                if (_includeImplicitGroups)
                    _containers.AddMissingGroups();
                else
                    _containers.RemoveInplicitGroups();
            }
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _containers.UpdateCollection((IEnumerable<TSource>) sender, e, o => new GroupedItemContainer<TSource, TKey>((TSource)o, _keySelector));
        }

        class ItemList : ObservableCollection<GroupedItemContainer<TSource, TKey>>
        {
            private readonly GroupingReadOnlyObservableListSource<TSource, TKey> _owner;

            public ItemList(GroupingReadOnlyObservableListSource<TSource, TKey> owner, IEnumerable<GroupedItemContainer<TSource, TKey>> source) : base (source)
            {
                _owner = owner;
                UpdateAllIndexes();
                if (_owner._groups.Count > 0 || _owner.IncludeImplicitGroups)
                    AddMissingGroups();
            }

            private void ContainerOnKeyChanged(object sender, EventArgs eventArgs)
            {
                var container = (GroupedItemContainer<TSource, TKey>) sender;
                var oldGroup = container.Group;
                if (oldGroup != null && _owner._equals(oldGroup.Key, container.Key))
                    return;

                var group = _owner._groups.SingleOrDefault(g => _owner._equals(g.Key, container.Key));
                if (group == null && _owner.IncludeImplicitGroups)
                {
                    group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _owner._groups.Add(group);
                }

                if (oldGroup != null)
                {
                    oldGroup.InternalItems.RemoveAt(container.GroupIndex);
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

            private void UpdateIndexesFrom(GroupedItemContainer<TSource, TKey> firstContainer)
            {
                UpdateIndexes(firstContainer.SourceIndex, firstContainer.GroupIndex, firstContainer.Group);
            }

            private void UpdateAllIndexes(ReadOnlyObservableGroup<TSource, TKey> group = null)
            {
                UpdateIndexes(0, 0, group);
            }

            private void UpdateIndexes(int start, int groupIndex, ReadOnlyObservableGroup<TSource, TKey> group)
            {
                for (int i = start; i < Count; i++)
                {
                    var container = Items[i];
                    container.SourceIndex = i;
                    if (group == container.Group)
                        container.GroupIndex = groupIndex++;
                }
            }

            private void FindGroupIndex(GroupedItemContainer<TSource, TKey> container)
            {
                var i = container.SourceIndex - 1; // Find the source index of the previous item that belongs to the same group
                while (i >= 0 && Items[i].Group != container.Group)
                {
                    i--;
                }
                container.GroupIndex = i < 0 ? 0 : Items[i].GroupIndex + 1;
            }

            public void RemoveInplicitGroups()
            {
                foreach (var container in Items.Where(c => !c.Group.IsExplicit))
                {
                    container.Group = null;
                    container.GroupIndex = -1;
                }

                var groups = _owner._groups;
                for (int i = groups.Count - 1; i >= 0; i--)
                {
                    if (!groups[i].IsExplicit)
                    {
                        groups[i].InternalItems.Clear();
                        groups.RemoveAt(i);
                    }
                }
            }

            public void AddMissingGroups()
            {
                var groups = _owner._groups;
                var groupCounts = groups.Select(g => g.Count).ToList();
                foreach (var container in Items)
                {
                    var group = container.Group;
                    if (group == null)
                    {
                        var groupIndex = groups.TakeWhile(g => !_owner._equals(g.Key, container.Key)).Count();
                        if (groupIndex == groups.Count)
                        {
                            if (!_owner.IncludeImplicitGroups)
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

            protected override void ClearItems()
            {
                foreach (var container in Items)
                {
                    container.DetachTriggers(_owner._triggers);
                    container.KeyChanged -= ContainerOnKeyChanged;
                    container.Group = null;
                    container.GroupIndex = -1;
                    container.SourceIndex = -1;
                }

                base.ClearItems();

                var groups = _owner._groups;
                for (int i = groups.Count - 1; i >= 0; i--)
                {
                    groups[i].InternalItems.Clear();
                    if (!groups[i].IsExplicit)
                        groups.RemoveAt(i);
                }
            }

            protected override void InsertItem(int index, GroupedItemContainer<TSource, TKey> container)
            {
                base.InsertItem(index, container);

                var group = _owner._groups.SingleOrDefault(g => _owner._equals(g.Key, container.Key));
                if (group == null && _owner.IncludeImplicitGroups)
                {
                    group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _owner._groups.Add(group);
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
                container.AttachTriggers(_owner._triggers);
                container.KeyChanged += ContainerOnKeyChanged;
            }

            protected override void MoveItem(int oldIndex, int newIndex)
            {
                base.MoveItem(oldIndex, newIndex);
                var container = Items[newIndex];
                var oldGroupIndex = container.GroupIndex;
                UpdateAllIndexes(container.Group); // Update all source & group indexes at once.
                container.Group.InternalItems.Move(oldGroupIndex, container.GroupIndex);
            }

            protected override void RemoveItem(int index)
            {
                var container = Items[index];
                var group = container.Group;
                var groupIndex = container.GroupIndex;

                container.DetachTriggers(_owner._triggers);
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
                        _owner._groups.Remove(group); 
                }
            }

            protected override void SetItem(int index, GroupedItemContainer<TSource, TKey> container)
            {
                var oldContainer = Items[index];
                var oldGroup = oldContainer.Group;
                var oldGroupIndex = oldContainer.GroupIndex;

                oldContainer.DetachTriggers(_owner._triggers);
                oldContainer.KeyChanged -= ContainerOnKeyChanged;

                base.SetItem(index, container);

                UpdateIndexesFrom(oldContainer);
                oldContainer.Group = null;
                oldContainer.GroupIndex = -1;
                oldContainer.SourceIndex = -1;

                var group = _owner._groups.SingleOrDefault(g => _owner._equals(g.Key, container.Key));
                if (group == null && _owner.IncludeImplicitGroups)
                {
                    group = new ReadOnlyObservableGroup<TSource, TKey>(container.Key, false);
                    _owner._groups.Add(group);
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
                            _owner._groups.Remove(oldGroup);
                    }
                    if (group != null)
                    {
                        FindGroupIndex(container);
                        group.InternalItems.Insert(container.GroupIndex, container.Item);
                    }
                }
                UpdateIndexesFrom(container);
                container.AttachTriggers(_owner._triggers);
                container.KeyChanged += ContainerOnKeyChanged;
            }
        }
    }

    public class GroupedItemContainer<TSource, TKey> : ItemContainer<TSource>
    {
        private readonly Func<TSource, TKey> _selector;
        private TKey _key;

        public GroupedItemContainer(TSource item, Func<TSource, TKey> selector) : base (item)
        {
            _selector = selector;
            Key = _selector(item);
        }

        public TKey Key
        {
            get { return _key; }
            protected set
            {
                _key = value;
                OnKeyChanged();
            }
        }

        public ReadOnlyObservableGroup<TSource, TKey> Group { get; set; }

        public int SourceIndex { get; set; }

        public int GroupIndex { get; set; }

        protected override void OnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Key = _selector(Item);
        }

        public event EventHandler KeyChanged;

        protected virtual void OnKeyChanged()
        {
            KeyChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ReadOnlyObservableGroup<TSource, TKey> : ReadOnlyObservableList<TSource>
    {
        private ReadOnlyObservableGroup(ObservableCollection<TSource> items, TKey key, bool isExplicit) : base(items)
        {
            InternalItems = items;
            Key = key;
            IsExplicit = isExplicit;
        }

        internal ReadOnlyObservableGroup(TKey key, bool isFixed) : this(new ObservableCollection<TSource>(), key, isFixed)
        {
        }

        public ReadOnlyObservableGroup(ObservableCollection<TSource> items, TKey key) : this (items, key, true) { }

        public TKey Key { get; }

        protected internal bool IsExplicit { get; }

        protected internal ObservableCollection<TSource> InternalItems { get; }
    }
}