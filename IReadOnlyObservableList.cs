using System.Collections.Generic;

namespace Neuronic.CollectionModel
{
    public interface IReadOnlyObservableList<out T> : IReadOnlyObservableCollection<T>, IReadOnlyList<T>
    {
    }
}