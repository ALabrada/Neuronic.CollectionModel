using System;
using System.Windows;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// Manager used to implement the Weak Event Pattern for the <see cref="ISelectableItem.SelectionChanged"/> event.
    /// </summary>
    /// <seealso cref="System.Windows.WeakEventManager" />
    public class SelectionChangedEventManager : WeakEventManager
    {
        private SelectionChangedEventManager()
        {

        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(ISelectableItem source,
            EventHandler<EventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(ISelectableItem source,
            EventHandler<EventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static SelectionChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(SelectionChangedEventManager);
                var manager = (SelectionChangedEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new SelectionChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        /// <summary>
        /// Return a new list to hold listeners to the event.
        /// </summary>
        protected override ListenerList NewListenerList()
        {
            return new ListenerList<EventArgs>();
        }

        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected override void StartListening(object source)
        {
            ISelectableItem typedSource = (ISelectableItem)source;
            typedSource.SelectionChanged += OnSelectionChanged;
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected override void StopListening(object source)
        {
            ISelectableItem typedSource = (ISelectableItem)source;
            typedSource.SelectionChanged -= OnSelectionChanged;
        }

        /// <summary>
        /// Event handler for the SelectionChanged event.
        /// </summary>
        private void OnSelectionChanged(object sender, EventArgs eventArgs)
        {
            DeliverEvent(sender, eventArgs);
        }
    }
}