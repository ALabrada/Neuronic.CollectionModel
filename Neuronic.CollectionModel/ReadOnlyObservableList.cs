using System.Collections.ObjectModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Utility class that allows to use an <see cref="ObservableCollection{T}" /> instance as a
    ///     <see cref="IReadOnlyObservableList{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    /// <seealso cref="System.Collections.ObjectModel.ReadOnlyObservableCollection{T}" />
    /// <seealso cref="Neuronic.CollectionModel.IReadOnlyObservableList{T}" />
    public class ReadOnlyObservableList<T> : ReadOnlyObservableCollection<T>, IReadOnlyObservableList<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlyObservableList{T}" /> class.
        /// </summary>
        /// <param name="list">
        ///     The <see cref="T:System.Collections.ObjectModel.ObservableCollection`1" /> with which to create this
        ///     instance of the <see cref="T:System.Collections.ObjectModel.ReadOnlyObservableCollection`1" /> class.
        /// </param>
        public ReadOnlyObservableList(ObservableCollection<T> list) : base(list)
        {
        }
    }
}