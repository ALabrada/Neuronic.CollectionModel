using System;
using System.Collections.Specialized;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    internal class CollectionChangedEventManager : WeakEventManager
    {
        private CollectionChangedEventManager()
        {
            
        }

        /// <summary>
        /// Adds a listener for the given source's event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void AddListener(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedAddListener(source, listener);
        }

        /// <summary>
        /// Removes a listener for the given source's event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void RemoveListener(INotifyCollectionChanged source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static CollectionChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(CollectionChangedEventManager);
                var manager = (CollectionChangedEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new CollectionChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected override void StartListening(object source)
        {
            var nSource = (INotifyCollectionChanged) source;
            nSource.CollectionChanged += OnCollectionChanged;
        }

        protected override void StopListening(object source)
        {
            var nSource = (INotifyCollectionChanged)source;
            nSource.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeliverEvent(sender, e);
        }
    }
}