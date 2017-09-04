using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Neuronic.CollectionModel.Results
{
    /// <summary>
    /// An observable result that determines if an element is present in a collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Results.QueryObservableResult{T, System.Boolean}" />
    public class ContainsObservableResult<T> : QueryObservableResult<T, bool>
    {
        private readonly IEqualityComparer<T> _comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsObservableResult{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value to find.</param>
        /// <param name="comparer">The equality comparer. If it is <c>null</c> the default comparer is used.</param>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public ContainsObservableResult(IReadOnlyObservableCollection<T> source, T value, IEqualityComparer<T> comparer = null) : base(source)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Value = value;
            _comparer = comparer ?? EqualityComparer<T>.Default;

            CurrentValue = source.Contains(value, _comparer);
        }

        /// <summary>
        /// Gets the value to find in the collection.
        /// </summary>
        /// <value>
        /// The value to find in the collection.
        /// </value>
        public T Value { get; }

        /// <summary>
        /// Called when the source collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        protected override void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // It was already in the collection or is one of the new items
                    CurrentValue = CurrentValue || e.NewItems.OfType<T>().Contains(Value, _comparer);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // It was already in the collection and was not one of the removed elements. 
                    CurrentValue = CurrentValue && (!e.OldItems.OfType<T>().Contains(Value, _comparer) ||
                                                    Source.Contains(Value, _comparer)); // Even if it was removed, maybe there is another one equal.
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // It is one of the new items or it was already in the collection and was not one of the removed elements. 
                    CurrentValue = e.NewItems.OfType<T>().Contains(Value, _comparer) || CurrentValue &&
                                   (!e.OldItems.OfType<T>().Contains(Value, _comparer) ||
                                    Source.Contains(Value, _comparer)); // Even if it was removed, maybe there is another one equal.
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Moving does not affect this operation.
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Recalculate
                    CurrentValue = Source.Contains(Value, _comparer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}