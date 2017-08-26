namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Represents a strongly typed, read-only collection of elements that can be accessed by index and monitored for
    ///     changes.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements. This type parameter is covariant.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    /// <seealso cref="IReadOnlyList{T}" />
    public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>
    {
    }

    /// <summary>
    ///     Represents a read-only collection of elements that can be accessed by index;
    /// </summary>
    /// <typeparam name="T">>The type of the collection elements. This type parameter is covariant.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyCollection{T}" />
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        /// <summary>
        ///     Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <value>
        ///     The element at the specified index in the read-only list.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The element at <paramref name="index" />.</returns>
        T this[int index] { get; }
    }
}