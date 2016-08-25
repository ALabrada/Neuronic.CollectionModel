using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     An utility collection that can be used to switch sources transparently.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    /// <seealso cref="System.Collections.Generic.ICollection{T}" />
    public class SwitchableCollectionSource<T> : SwitchableCollectionSourceBase<T>
    {
        private IReadOnlyCollection<T> _source;

        /// <summary>
        ///     Gets source collection.
        /// </summary>
        /// <value>
        ///     The source collection.
        /// </value>
        protected override IReadOnlyCollection<T> SourceOverride => _source;

        /// <summary>
        ///     Gets or sets the source collection.
        /// </summary>
        /// <value>
        ///     The source collection.
        /// </value>
        public IReadOnlyCollection<T> Source
        {
            get { return _source; }
            set
            {
                // Check if the source is actually changing.
                if (Equals(_source, value))
                    return;
                // Disengage event handlers from old source (if it is not NULL).
                var oldSource = _source;
                if (oldSource != null)
                {
                    var notifyProperties = oldSource as INotifyPropertyChanged;
                    if (notifyProperties != null)
                        PropertyChangedEventManager.RemoveHandler(notifyProperties, SourceOnPropertyChanged, CountPropertyName);
                    var notifyCollection = oldSource as INotifyCollectionChanged;
                    if (notifyCollection != null)
                        CollectionChangedEventManager.RemoveHandler(notifyCollection, SourceOnCollectionChanged);
                }
                // Update source
                _source = value;
                // Engage event handlers to new source (if it is not NULL).
                var newSource = _source;
                if (newSource != null)
                {
                    var notifyProperties = newSource as INotifyPropertyChanged;
                    if (notifyProperties != null)
                        PropertyChangedEventManager.AddHandler(notifyProperties, SourceOnPropertyChanged, CountPropertyName);
                    var notifyCollection = newSource as INotifyCollectionChanged;
                    if (notifyCollection != null)
                        CollectionChangedEventManager.AddHandler(notifyCollection, SourceOnCollectionChanged);
                }
                // Signal to update instance properties.
                OnPropertyChanged(new PropertyChangedEventArgs(CountPropertyName));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        private void SourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e);
        }
    }
}