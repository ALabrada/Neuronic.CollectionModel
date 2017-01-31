using System.Collections.Specialized;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Abstraction of a class that allows to raise <see cref="NotifyCollectionChangedAction.Reset">Reset</see>
    ///     events at will from external code, thus refreshing other collections that depend on this.
    /// </summary>
    /// <typeparam name="T">The type of the collection's items.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableCollection{T}" />
    public interface IManualReadOnlyObservableCollection<out T> : IReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///     Raises the <see cref="ManualReadOnlyObservableCollection{T}.CollectionChanged" /> event with the
        ///     <see cref="NotifyCollectionChangedAction.Reset" />
        ///     action.
        /// </summary>
        void Reset();
    }
}