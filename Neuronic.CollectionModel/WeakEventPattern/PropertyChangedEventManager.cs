#if NETSTD
using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    internal class PropertyChangedEventManager : WeakEventManager
    {
        private PropertyChangedEventManager()
        {

        }

        /// <summary>
        /// Adds a listener for the given source's event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        /// <param name="propertyName">The name of the property or <see cref="string.Empty"/> for all.</param>
        public static void AddListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            CurrentManager.ProtectedAddListener(source, new PropertyListenerInfo(listener, propertyName));
        }

        /// <summary>
        /// Removes a listener for the given source's event.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="listener">The listener.</param>
        /// <param name="propertyName">The name of the property or <see cref="string.Empty"/> for all.</param>
        public static void RemoveListener(INotifyPropertyChanged source, IWeakEventListener listener, string propertyName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            CurrentManager.ProtectedRemoveListener(source, new PropertyListenerInfo(listener, propertyName));
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static PropertyChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(PropertyChangedEventManager);
                var manager = (PropertyChangedEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new PropertyChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected override void StartListening(object source)
        {
            var nSource = (INotifyPropertyChanged)source;
            nSource.PropertyChanged += OnPropertyChanged;
        }

        protected override void StopListening(object source)
        {
            var nSource = (INotifyPropertyChanged)source;
            nSource.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeliverEvent(sender, e);
        }

        protected override bool OnChooseListener(ListenerInfo listener, EventArgs eventArgs)
        {
            var sourcePropName = (eventArgs as PropertyChangedEventArgs)?.PropertyName;
            return base.OnChooseListener(listener, eventArgs) && sourcePropName == string.Empty ||
                   string.Equals(sourcePropName, ((PropertyListenerInfo)listener).PropertyName,
                       StringComparison.Ordinal);
        }

        protected class PropertyListenerInfo : ListenerInfo
        {
            public PropertyListenerInfo(IWeakEventListener listener, string propertyName) : base(listener)
            {
                PropertyName = propertyName;
            }

            public string PropertyName { get; }

            public override bool Equals(ListenerInfo obj)
            {
                var other = obj as PropertyListenerInfo;
                return other != null && base.Equals(obj) && string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal);
            }
        }
    }
} 
#endif