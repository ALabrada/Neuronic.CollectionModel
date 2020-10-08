#if NETSTD
using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel.WeakEventPattern
{
    /// <summary>
    /// Provides a <see cref="WeakEventManager"/> implementation so that you can use the "weak event listener"
    /// pattern to attach listeners for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    public class PropertyChangedEventManager : WeakEventManager
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

        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        /// <param name="source">The source.</param>
        protected override void StartListening(object source)
        {
            var nSource = (INotifyPropertyChanged)source;
            nSource.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        /// <param name="source">The source.</param>
        protected override void StopListening(object source)
        {
            var nSource = (INotifyPropertyChanged)source;
            nSource.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeliverEvent(sender, e);
        }

        /// <summary>
        /// Checks if the specified listener is compatible with the event.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="eventArgs">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <returns>
        ///   <c>true</c> if the event is compatible with <paramref name="listener" />; otherwise, <c>false</c>.
        /// </returns>
        protected override bool OnChooseListener(ListenerInfo listener, EventArgs eventArgs)
        {
            var sourcePropName = (eventArgs as PropertyChangedEventArgs)?.PropertyName;
            return base.OnChooseListener(listener, eventArgs) && ((PropertyListenerInfo)listener).PropertyName.Length == 0 ||
                   string.Equals(sourcePropName, ((PropertyListenerInfo)listener).PropertyName,
                       StringComparison.Ordinal);
        }

        class PropertyListenerInfo : ListenerInfo
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