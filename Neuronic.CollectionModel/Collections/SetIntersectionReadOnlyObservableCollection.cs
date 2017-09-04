using System.Collections.Generic;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that is the set intersection of two other observable collections.
    /// </summary>
    /// <typeparam name="T">The type of the collection's elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.SetOperationReadOnlyObservableCollection{T}" />
    public class SetIntersectionReadOnlyObservableCollection<T> : SetOperationReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="firstSource">The source collection.</param>
        /// <param name="secondSource">The items to exclude from <paramref name="firstSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetIntersectionReadOnlyObservableCollection(IEnumerable<T> firstSource,
            IEnumerable<T> secondSource, IEqualityComparer<T> comparer = null) : base(
            firstSource,
            secondSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableSet<T>(new HashSet<T>(comparer)))
        {
        }

        /// <summary>
        /// Determines if the specified container's item should be included in the result.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="container" /> belongs in the collection; otherwise, false.
        /// </returns>
        protected override bool ShouldIncludeContainer(RefCountItemContainer<T> container)
        {
            return container.CountOnFirst > 0 && container.CountOnSecond > 0;
        }
    }
}