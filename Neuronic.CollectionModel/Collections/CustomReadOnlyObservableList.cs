using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// Utility class to represent any read-only list as observable, with the ability to manually notify changes.
    /// </summary>
    /// <typeparam name="T">The type of the list's elements.</typeparam>
    /// <seealso cref="CustomReadOnlyObservableCollection{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    public class CustomReadOnlyObservableList<T> : CustomReadOnlyObservableCollection<T>, IReadOnlyObservableList<T>
    {
        private const string IndexerPropertyName = "Item[]";
        private readonly IReadOnlyList<T> _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomReadOnlyObservableList{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public CustomReadOnlyObservableList(IReadOnlyList<T> source) : base(source)
        {
            _source = source;
            var propertyNotify = _source as INotifyPropertyChanged;
            if (propertyNotify != null)
                PropertyChangedEventManager.AddListener(propertyNotify, this, IndexerPropertyName);
        }

        /// <summary>
        /// Gets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index] => _source[index];

        /// <summary>
        /// Resets the collection.
        /// </summary>
        /// <remarks>
        /// This is useful if the source collection does not implement <see cref="T:System.Collections.Specialized.INotifyCollectionChanged" />
        /// and you want to notify changes in it.
        /// </remarks>
        public override void Reset()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(IndexerPropertyName));
            base.Reset();
        }
    }
}