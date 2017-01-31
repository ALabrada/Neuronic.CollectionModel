namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Abstraction of a class that allows to momentarily withhold change notification in the underlying lists.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.ITransactionalReadOnlyObservableCollection{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IManualReadOnlyObservableList{T}" />
    public interface ITransactionalReadOnlyObservableList<out T> : ITransactionalReadOnlyObservableCollection<T>,
        IManualReadOnlyObservableList<T>
    {
    }
}