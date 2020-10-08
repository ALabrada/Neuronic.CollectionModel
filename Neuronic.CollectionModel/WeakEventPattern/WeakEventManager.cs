#if NETSTD
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
    /// <summary>
    /// Provides a base class for the event manager that is used in the weak event pattern.
    /// The manager adds and removes listeners for events (or callbacks) that also use the pattern.
    /// </summary>
    public abstract class WeakEventManager
    {
        private static readonly Dictionary<Type, WeakEventManager> Managers = new Dictionary<Type, WeakEventManager>();
        private readonly ConditionalWeakTable<object, SourceInfo> _table = new ConditionalWeakTable<object, SourceInfo>();

        /// <summary>
        /// Adds the specified listener.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        protected void ProtectedAddListener(object source, IWeakEventListener listener)
        {
            ProtectedAddListener(source, new ListenerInfo(listener));
        }

        /// <summary>
        /// Adds the specified listener.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listenerInfo">The listener information.</param>
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

        /// <summary>
        /// Removes the specified listener.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        protected void ProtectedRemoveListener(object source, IWeakEventListener listener)
        {
            var listenerInfo = new ListenerInfo(listener);
            ProtectedRemoveListener(source, listenerInfo);
        }

        /// <summary>
        /// Removes the specified listener.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listenerInfo">The listener information.</param>
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

        /// <summary>
        /// Sets the current weak event manager for the specified type.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <param name="manager">The manager.</param>
        protected static void SetCurrentManager(Type managerType, WeakEventManager manager)
        {
            lock (Managers)
            {
                Managers[managerType] = manager;
            }
        }

        /// <summary>
        /// Gets the current weak event manager of the specified type.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Delivers the specified event to the compatible listeners.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="ArgumentNullException">source</exception>
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

        /// <summary>
        /// Checks if the specified listener is compatible with the event.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if the event is compatible with <paramref name="listener"/>; otherwise, <c>false</c>.</returns>
        protected virtual bool OnChooseListener(ListenerInfo listener, EventArgs eventArgs)
        {
            return true;
        }

        /// <summary>
        /// Contains information of an event source.
        /// </summary>
        protected class SourceInfo
        {
            private readonly ListenerCollection _listeners;

            /// <summary>
            /// Initializes a new instance of the <see cref="SourceInfo"/> class.
            /// </summary>
            public SourceInfo()
            {
                _listeners = new ListenerCollection();
            }

            /// <summary>
            /// Gets the listeners associated with the source.
            /// </summary>
            public ICollection<ListenerInfo> Listeners => _listeners;
        }

        /// <summary>
        /// Contains information of an event listener.
        /// </summary>
        protected class ListenerInfo : IEquatable<ListenerInfo>
        {
            private readonly WeakReference _listenerWeakRef;

            /// <summary>
            /// Initializes a new instance of the <see cref="ListenerInfo"/> class.
            /// </summary>
            /// <param name="listener">The listener.</param>
            /// <exception cref="ArgumentNullException">listener</exception>
            public ListenerInfo(IWeakEventListener listener)
            {
                if (listener == null) throw new ArgumentNullException(nameof(listener));
                _listenerWeakRef = new WeakReference(listener);
            }

            /// <summary>
            /// Gets the listener.
            /// </summary>
            /// <value>
            /// The listener.
            /// </value>
            public IWeakEventListener Listener => _listenerWeakRef.Target as IWeakEventListener;

            /// <summary>
            /// Gets a value indicating whether this instance is alive.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is alive; otherwise, <c>false</c>.
            /// </value>
            public bool IsAlive => _listenerWeakRef.IsAlive;

            /// <summary>
            /// Tries the get the listener instance.
            /// </summary>
            /// <param name="listener">The listener.</param>
            /// <returns><c>true</c> if the listener is alive; otherwise, <c>false</c>.</returns>
            public bool TryGetListener(out IWeakEventListener listener)
            {
                listener = _listenerWeakRef.Target as IWeakEventListener;
                return _listenerWeakRef.IsAlive;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
            /// </returns>
            public virtual bool Equals(ListenerInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ReferenceEquals(Listener, other.Listener) && IsAlive;
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                var other = obj as ListenerInfo;
                return other != null && Equals(other);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
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

#endif