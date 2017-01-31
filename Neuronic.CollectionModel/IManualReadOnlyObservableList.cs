using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Abstraction of a class that allows to raise <see cref="NotifyCollectionChangedAction.Reset">Reset</see>
    ///     events at will from external code, thus refreshing other lists that depend on this.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IManualReadOnlyObservableCollection{T}" />
    public interface IManualReadOnlyObservableList<out T> : IReadOnlyObservableList<T>,
        IManualReadOnlyObservableCollection<T>
    {
    }
}