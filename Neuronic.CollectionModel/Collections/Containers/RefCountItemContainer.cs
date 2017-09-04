using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections.Containers
{
    /// <summary>
    /// Container that contains meta-data for the collection elements.
    /// </summary>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
    public class RefCountItemContainer<T> : ItemContainer<T>, INotifyPropertyChanged
    {
        private int _countOnFirst;
        private int _countOnSecond;

        private RefCountItemContainer(T item, int countOnFirst, int countOnSecond) : base(item)
        {
            _countOnFirst = countOnFirst;
            _countOnSecond = countOnSecond;
        }

        /// <summary>
        /// Creates a container for an item from the first operand source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The container of <paramref name="item"/>.</returns>
        public static RefCountItemContainer<T> CreateFromFirst(T item) => new RefCountItemContainer<T>(item, 1, 0);

        /// <summary>
        /// Creates a container for an item from the second operand source.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The container of <paramref name="item"/>.</returns>
        public static RefCountItemContainer<T> CreateFromSecond(T item) => new RefCountItemContainer<T>(item, 0, 1);

        /// <summary>
        /// Gets or sets the number of times the item appears in the first operand source.
        /// </summary>
        /// <value>
        /// The number of time the item appears in the first operand source.
        /// </value>
        public int CountOnFirst
        {
            get { return _countOnFirst; }
            set { Set(nameof(CountOnFirst), ref _countOnFirst, value); }
        }

        /// <summary>
        /// Gets or sets the number of times the item appears in the second operand source.
        /// </summary>
        /// <value>
        /// The number of times the item appears in the second operand source.
        /// </value>
        public int CountOnSecond
        {
            get { return _countOnSecond; }
            set { Set(nameof(CountOnSecond), ref _countOnSecond, value); }
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
            CountOnFirst += other.CountOnFirst;
            CountOnSecond += other.CountOnSecond;
        }

        /// <summary>
        /// Decrements the counters using information from another container.
        /// </summary>
        /// <param name="other">The other.</param>
        public void Decrement(RefCountItemContainer<T> other)
        {
            CountOnFirst -= other.CountOnFirst;
            CountOnSecond -= other.CountOnSecond;
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