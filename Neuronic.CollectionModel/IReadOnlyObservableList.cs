using System.Collections.Generic;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Represents a strongly typed, read-only collection of elements that can be accessed by index and monitored for
    ///     changes.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements. This type parameter is covariant.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    /// <seealso cref="System.Collections.Generic.IReadOnlyList{T}" />
    public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>
    {
    }
}