using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Container that contains meta-data for the collection elements.
    /// </summary>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
    public class RefCountItemContainer<T> : ItemContainer<T>, INotifyPropertyChanged
    {
        private int _originalCount;
        private int _substractedCount;

        private RefCountItemContainer(T item, int originalCount, int substractedCount) : base(item)
        {
            _originalCount = originalCount;
            _substractedCount = substractedCount;
        }

        /// <summary>
        /// Creates a container for an item from the original source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The container of <paramref name="item"/>.</returns>
        public static RefCountItemContainer<T> CreateOriginal(T item) => new RefCountItemContainer<T>(item, 1, 0);

        /// <summary>
        /// Creates a container for an item from the substracted source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The container of <paramref name="item"/>.</returns>
        public static RefCountItemContainer<T> CreateSubstracted(T item) => new RefCountItemContainer<T>(item, 0, 1);

        /// <summary>
        /// Gets or sets the number of times the item appears in the original source.
        /// </summary>
        /// <value>
        /// The number of time the item appears in the original source.
        /// </value>
        public int OriginalCount
        {
            get { return _originalCount; }
            set { Set(nameof(OriginalCount), ref _originalCount, value); }
        }

        /// <summary>
        /// Gets or sets the number of times the item appears in the substracted source.
        /// </summary>
        /// <value>
        /// The number of times the item appears in the substracted source.
        /// </value>
        public int SubstractedCount
        {
            get { return _substractedCount; }
            set { Set(nameof(SubstractedCount), ref _substractedCount, value); }
        }

        /// <summary>
        /// Sets the value of a property and sends a notification if needed.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="field">The backing field of the property.</param>
        /// <param name="value">The value to set.</param>
        /// <returns><c>true</c> if the property was updated; otherwise, <c>false</c>.</returns>
        protected bool Set<TProperty>(string propertyName, ref TProperty field, TProperty value)
        {
            if (Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Increments the counters using information from another container.
        /// </summary>
        /// <param name="other">The other.</param>
        public void Increment(RefCountItemContainer<T> other)
        {
            OriginalCount += other.OriginalCount;
            SubstractedCount += other.SubstractedCount;
        }

        /// <summary>
        /// Decrements the counters using information from another container.
        /// </summary>
        /// <param name="other">The other.</param>
        public void Decrement(RefCountItemContainer<T> other)
        {
            OriginalCount -= other.OriginalCount;
            SubstractedCount -= other.SubstractedCount;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}