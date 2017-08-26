using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Observable list that represents a contiguous sub-sequence of another list.
    /// </summary>
    /// <typeparam name="T">The type of the list elements.</typeparam>
    /// <seealso cref="ReadOnlyObservableList{T}" />
    public class RangedReadOnlyObservableList<T> : ReadOnlyObservableList<T>, IWeakEventListener
    {
        private readonly ObservableCollection<T> _list;
        private readonly IReadOnlyList<T> _source;
        private int _offset;
        private int _maxCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangedReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        /// <param name="offset">The offset of the sub-sequence.</param>
        /// <param name="maxCount">The maximum number of elements in the sub-sequence (Default to unlimited).</param>
        public RangedReadOnlyObservableList(IReadOnlyList<T> source, int offset = 0, int maxCount = -1)
            : this (new ObservableCollection<T>(), source, offset, maxCount)
        {
        }

        private RangedReadOnlyObservableList(ObservableCollection<T> list, IReadOnlyList<T> source, int offset,
            int maxCount) : base(list)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            _list = list;
            _source = source;
            _offset = offset;
            _maxCount = maxCount;

            ResetItems();

            var notify = _source as INotifyCollectionChanged;
            if (notify != null)
                CollectionChangedEventManager.AddListener(notify, this);
        }

        /// <summary>
        /// Gets or sets the offset.
        /// </summary>
        /// <value>
        /// The offset of the sub-sequence.
        /// </value>
        public int Offset
        {
            get { return _offset; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (_offset == value)
                    return;
                var diff = value - _offset;
                _offset = value;
                if (Math.Abs(diff) >= Count)
                    ResetItems();
                else if (diff > 0)
                    RemoveItems(diff, value);
                else
                    InsertItems(-diff, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        /// <value>
        /// The maximum count of the sub-sequence.
        /// </value>
        public int MaxCount
        {
            get { return _maxCount; }
            set
            {
                if (_offset == value)
                    return;
                _maxCount = value;
                CleanItems();
                FillItems();
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int count;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    count = e.NewItems.Count;
                    if (MaxCount >= 0)
                        count = Math.Min(count, Offset + MaxCount - e.NewStartingIndex);
                    InsertItems(count, Math.Max(e.NewStartingIndex, Offset));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    count = e.OldItems.Count;
                    if (MaxCount >= 0)
                        count = Math.Min(count, Offset + MaxCount - e.OldStartingIndex);
                    RemoveItems(count, Math.Max(e.OldStartingIndex, Offset));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetItems();
                    break;
                case NotifyCollectionChangedAction.Move:
                    count = e.OldItems.Count;
                    if (MaxCount >= 0)
                        count = Math.Min(count, Offset + MaxCount - e.OldStartingIndex);
                    RemoveItems(count, Math.Max(e.OldStartingIndex, Offset));
                    count = e.NewItems.Count;
                    if (MaxCount >= 0)
                        count = Math.Min(count, Offset + MaxCount - e.NewStartingIndex);
                    InsertItems(count, Math.Max(e.NewStartingIndex, Offset));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var newCount = e.OldItems.Count;
                    var oldCount = e.NewItems.Count;
                    if (MaxCount >= 0)
                    {
                        oldCount = Math.Min(oldCount, Offset + MaxCount - e.OldStartingIndex);
                        newCount = Math.Min(newCount, Offset + MaxCount - e.NewStartingIndex);
                    }
                    ReplaceItems(oldCount, newCount, Math.Max(e.NewStartingIndex, Offset));
                    break;
            }
        }

        private void ReplaceItems(int oldCount, int newCount, int sourceIndex)
        {
            // First, remove extra items
            RemoveItems(oldCount - newCount, sourceIndex + newCount);
            // Second, replace common items
            var count = Math.Min(newCount, oldCount);
            for (int i = 0; i < count; i++)
                _list[sourceIndex - Offset + i] = _source[sourceIndex + i];
            // Third, add missing items
            InsertItems(newCount - oldCount, sourceIndex + oldCount);
        }

        private void RemoveItems(int count, int sourceIndex)
        {
            for (int i = count - 1; i >= 0; i--)
                _list.RemoveAt(sourceIndex - Offset + i);
            if (MaxCount >= 0)
                while (_list.Count < MaxCount && _source.Count > Offset + _list.Count)
                    _list.Add(_source[Offset + _list.Count]);
        }

        private void InsertItems(int count, int sourceIndex)
        {
            for (int i = 0; i < count; i++)
            {
                if (_list.Count == MaxCount)
                    _list.RemoveAt(_list.Count - 1);
                _list.Insert(sourceIndex - Offset + i, _source[sourceIndex + i]);
            }
        }

        private void ResetItems()
        {
            _list.Clear();
            var count = _source.Count - Offset;
            if (MaxCount >= 0)
                count = Math.Min(count, MaxCount);
            for (int i = 0; i < count; i++)
                _list.Add(_source[Offset + _list.Count]);
        }

        private void FillItems()
        {
            while (_source.Count > Offset + _list.Count && (MaxCount < 0 || _list.Count < MaxCount))
                _list.Add(_source[Offset + _list.Count]);
        }

        private void CleanItems()
        {
            if (MaxCount < 0) return;
            while (_list.Count > MaxCount)
                _list.RemoveAt(_list.Count - 1);
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!ReferenceEquals(_source, sender) || managerType != typeof(CollectionChangedEventManager))
                return false;
            SourceOnCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
            return true;
        }
    }
}