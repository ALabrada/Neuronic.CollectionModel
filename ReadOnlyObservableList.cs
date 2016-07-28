using System.Collections.ObjectModel;

namespace Neuronic.CollectionModel
{
    public class ReadOnlyObservableList<T> : ReadOnlyObservableCollection<T>, IReadOnlyObservableList<T>
    {
        public ReadOnlyObservableList(ObservableCollection<T> list) : base(list)
        {
        }
    }
}