using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Neuronic.CollectionModel;
using Neuronic.CollectionModel.Extras;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    internal abstract class WeakEventManager
    {
        private static readonly Dictionary<Type, WeakEventManager> Managers = new Dictionary<Type, WeakEventManager>();
        private readonly ConditionalWeakTable<object, SourceInfo> _table = new ConditionalWeakTable<object, SourceInfo>();

        protected void ProtectedAddListener(object source, IWeakEventListener listener)
        {
            ProtectedAddListener(source, new ListenerInfo(listener));
        }

        protected void ProtectedAddListener(object source, ListenerInfo listenerInfo)
        {
            SourceInfo sourceInfo;
            lock (_table)
            {
                if (!_table.TryGetValue(source, out sourceInfo))
                    _table.Add(source, sourceInfo = new SourceInfo());
            }
            lock (sourceInfo.Listeners)
            {
                if (!sourceInfo.Listeners.Any())
                    StartListening(source);

                sourceInfo.Listeners.Add(listenerInfo);
            }
        }

        protected void ProtectedRemoveListener(object source, IWeakEventListener listener)
        {
            var listenerInfo = new ListenerInfo(listener);
            ProtectedRemoveListener(source, listenerInfo);
        }

        protected void ProtectedRemoveListener(object source, ListenerInfo listenerInfo)
        {
            SourceInfo sourceInfo;
            lock (_table)
            {
                if (!_table.TryGetValue(source, out sourceInfo)) return;
            }
            lock (sourceInfo.Listeners)
            {
                sourceInfo.Listeners.Remove(listenerInfo);
                if (!sourceInfo.Listeners.Any())
                {
                    StopListening(source);
                    lock (_table)
                    {
                        _table.Remove(source);
                    }
                }
            }
        }

        protected static void SetCurrentManager(Type managerType, WeakEventManager manager)
        {
            lock (Managers)
            {
                Managers[managerType] = manager;
            }
        }

        protected static WeakEventManager GetCurrentManager(Type managerType)
        {
            lock (Managers)
            {
                WeakEventManager result;
                return Managers.TryGetValue(managerType, out result) ? result : null;
            }
        }

        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected abstract void StartListening(object source);

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected abstract void StopListening(object source);

        protected void DeliverEvent(object source, EventArgs eventArgs)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            SourceInfo sourceInfo;
            lock (_table)
            {
                if (!_table.TryGetValue(source, out sourceInfo)) return;
            }

            List<IWeakEventListener> listeners;
            lock (sourceInfo.Listeners)
            {
                listeners = new List<IWeakEventListener>(sourceInfo.Listeners.Count);
                listeners.AddRange(from info in sourceInfo.Listeners
                    where OnChooseListener(info, eventArgs)
                    select info.Listener);

                if (!sourceInfo.Listeners.Any())
                {
                    StopListening(source);
                    lock (_table)
                    {
                        _table.Remove(source);
                    }
                }
            }
            foreach (var listener in listeners)
            {
                listener.ReceiveWeakEvent(GetType(), source, eventArgs);
            }
        }

        protected virtual bool OnChooseListener(ListenerInfo listener, EventArgs eventArgs)
        {
            return true;
        }

        protected class SourceInfo
        {
            private readonly ListenerCollection _listeners;

            public SourceInfo()
            {
                _listeners = new ListenerCollection();
            }

            public ICollection<ListenerInfo> Listeners => _listeners;
        }

        protected class ListenerInfo : IEquatable<ListenerInfo>
        {
            private readonly WeakReference _listenerWeakRef;

            public ListenerInfo(IWeakEventListener listener)
            {
                if (listener == null) throw new ArgumentNullException(nameof(listener));
                _listenerWeakRef = new WeakReference(listener);
            }

            public IWeakEventListener Listener => _listenerWeakRef.Target as IWeakEventListener;

            public bool IsAlive => _listenerWeakRef.IsAlive;

            public bool TryGetListener(out IWeakEventListener listener)
            {
                listener = _listenerWeakRef.Target as IWeakEventListener;
                return _listenerWeakRef.IsAlive;
            }

            public virtual bool Equals(ListenerInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ReferenceEquals(Listener, other.Listener) && IsAlive;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                var other = obj as ListenerInfo;
                return other != null && Equals(other);
            }

            public override int GetHashCode()
            {
                return _listenerWeakRef.GetHashCode();
            }
        }

        class ListenerCollection : ICollection<ListenerInfo>
        {
            private readonly LinkedList<ListenerInfo> _items = new LinkedList<ListenerInfo>();
            public int Count => _items.Count;
            public bool IsReadOnly => false;

            public IEnumerator<ListenerInfo> GetEnumerator()
            {
                var node = _items.First;
                while (node != null)
                {
                    var next = node.Next;
                    if (node.Value.IsAlive)
                        yield return node.Value;
                    else
                        _items.Remove(node);
                    node = next;
                    if (node == _items.First)
                        break;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(ListenerInfo item)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));
                _items.AddLast(item);
            }

            public void Clear()
            {
                _items.Clear();
            }

            public bool Contains(ListenerInfo item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(ListenerInfo[] array, int arrayIndex)
            {
                this.CopyTo(array, arrayIndex, array.Length);
            }

            public bool Remove(ListenerInfo item)
            {
                var node = _items.First;
                while (node != null)
                {
                    var next = node.Next;
                    var oldItem = node.Value;
                    if (item.Equals(oldItem))
                    {
                        _items.Remove(node);
                        return true;
                    }
                    else if (!oldItem.IsAlive)
                        _items.Remove(node);
                    node = next;
                    if (node == _items.First)
                        break;
                }
                return false;
            }
        }
    }
}
