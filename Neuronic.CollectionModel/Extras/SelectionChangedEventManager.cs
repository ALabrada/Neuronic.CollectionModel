using System;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    /// Manager used to implement the Weak Event Pattern for the <see cref="ISelectableItem.SelectionChanged"/> event.
    /// </summary>
    internal class SelectionChangedEventManager : WeakEventManager
    {
        private SelectionChangedEventManager()
        {

        }

        /// <summary>
        /// Adds a listener for the given source's event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        public static void AddListener(ISelectableItem source, IWeakEventListener listener)
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
        public static void RemoveListener(ISelectableItem source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedRemoveListener(source, listener);
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