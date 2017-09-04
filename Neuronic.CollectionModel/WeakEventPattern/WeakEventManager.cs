using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Neuronic.CollectionModel.Extras;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    internal abstract class WeakEventManager
    {
        private static readonly Dictionary<Type, WeakEventManager> _managers = new Dictionary<Type, WeakEventManager>();
        private readonly LinkedList<ListenerInfo> _listeners = new LinkedList<ListenerInfo>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        protected void ProtectedAddListener(object source, IWeakEventListener listener)
        {
            AddListener(new ListenerInfo(source, listener));
            StartListening(source);
        }

        protected void AddListener(ListenerInfo info)
        {
            _lock.EnterWriteLock();
            try
            {
                var node = _listeners.First;
                while (node != null)
                {
                    var next = node.Next;
                    if (!node.Value.IsAlive)
                        _listeners.Remove(node);
                    node = next;
                    if (node == _listeners.First)
                        break;
                }
                _listeners.AddLast(info);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        protected void ProtectedRemoveListener(object source, IWeakEventListener listener)
        {
            StopListening(source);
            RemoveListener(new ListenerInfo(source, listener));
        }

        protected void RemoveListener(ListenerInfo info)
        {
            _lock.EnterWriteLock();
            try
            {
                var node = _listeners.First;
                while (node != null)
                {
                    var next = node.Next;
                    if (node.Value.Equals(info) || !node.Value.IsAlive)
                        _listeners.Remove(node);
                    node = next;
                    if (node == _listeners.First)
                        break;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        protected static void SetCurrentManager(Type managerType, WeakEventManager manager)
        {
            lock (_managers)
            {
                _managers[managerType] = manager;
            }
        }

        protected static WeakEventManager GetCurrentManager(Type managerType)
        {
            lock (_managers)
            {
                WeakEventManager result;
                return _managers.TryGetValue(managerType, out result) ? result : null;
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

        protected void DeliverEvent(object sender, EventArgs eventArgs)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            _lock.EnterReadLock();
            List<IWeakEventListener> listeners;
            try
            {
                listeners = new List<IWeakEventListener>(_listeners.Count);
                listeners.AddRange(from info in _listeners
                    let source = info.Source
                    let listener = info.Listener
                    where ReferenceEquals(sender, source) && listener != null && OnChooseListener(info, eventArgs)
                    select listener);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            foreach (var listener in listeners)
            {
                listener.ReceiveWeakEvent(GetType(), sender, eventArgs);
            }
        }

        protected virtual bool OnChooseListener(ListenerInfo listener, EventArgs eventArgs)
        {
            return true;
        }

        protected class ListenerInfo : IEquatable<ListenerInfo>
        {
            private readonly WeakReference _listener;
            private readonly WeakReference _source;
            private readonly int _hashCode;

            public ListenerInfo(object source, IWeakEventListener listener)
            {
                if (source == null) throw new ArgumentNullException(nameof(source));
                if (listener == null) throw new ArgumentNullException(nameof(listener));
                _listener = new WeakReference(listener);
                _source = new WeakReference(source);
                _hashCode = (listener.GetHashCode() * 397) ^ source.GetHashCode();
            }

            public object Source => _source.Target;

            public IWeakEventListener Listener => _listener.Target as IWeakEventListener;

            public bool IsAlive => _listener.IsAlive && _source.IsAlive;

            public virtual bool Equals(ListenerInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return IsAlive && Listener.Equals(other.Listener) && Source.Equals(other.Source);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                var other = obj as ListenerInfo;
                return other != null && Equals(other);
            }

            public override int GetHashCode() => _hashCode;
        }
    }
}