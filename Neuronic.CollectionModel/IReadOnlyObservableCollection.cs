using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Represents a strongly typed, read-only collection of elements that can be monitored for changes.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements. This type parameter is covariant.</typeparam>
    /// <seealso cref="System.Collections.Specialized.INotifyCollectionChanged" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
    }

#if NET40
    /// <summary>
    ///     Represents a strongly-typed, read-only collection of elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements. This type parameter is covariant.</typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    public interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        /// <summary>
        ///     Gets the number of elements in the collection.
        /// </summary>
        /// <value>
        ///     The number of elements in the collection.
        /// </value>
        int Count { get; }
    }
#endif
}