using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Observable query that stores the last element of an observable collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection's elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.QueryObservableResult{T, T}" />
    public class LastElementQueryResult<T> : QueryObservableResult<T,T>
    {
        private readonly T _defaultValue;
        private int _currentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="LastElementQueryResult{T}"/> class.
        /// </summary>
        /// <param name="source">The source collection.</param>
        /// <param name="defaultValue">The default value.</param>
        public LastElementQueryResult(IReadOnlyObservableCollection<T> source, T defaultValue = default(T)) : base(source)
        {
            _defaultValue = defaultValue;
            CurrentValue = source.Count == 0 ? _defaultValue : source.Last();
            _currentIndex = source.Count - 1;
        }

        /// <summary>
        /// Called when the source collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var source = (IReadOnlyObservableCollection<T>) sender;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex > _currentIndex)
                        CurrentValue = (T) e.NewItems[e.NewItems.Count - 1];
                    _currentIndex += e.NewItems.Count;
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex + e.OldItems.Count > _currentIndex)
                        CurrentValue = source.Count == 0 ? _defaultValue : source.Last();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex + e.OldItems.Count > _currentIndex)
                        CurrentValue = (T)e.NewItems[e.NewItems.Count - 1];
                    break;
                default:
                    CurrentValue = source.Count == 0 ? _defaultValue : source.Last();
                    break;
            }
            _currentIndex = source.Count - 1;
        }
    }
}