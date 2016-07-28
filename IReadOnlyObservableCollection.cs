using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        
    }
}