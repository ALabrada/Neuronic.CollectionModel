using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Collections
{
    /// <summary>
    /// An observable collection that is the set difference of two other observable collections.
    /// </summary>
    /// <typeparam name="T">The type of the collection elements.</typeparam>
    /// <seealso cref="Neuronic.CollectionModel.Collections.CustomReadOnlyObservableCollection{T}" />
    public class SetDifferenceReadOnlyObservableCollection<T> : SetOperationReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableCollection(IEnumerable<T> originalSource,
            IEnumerable<T> substractedSource, IEqualityComparer<T> comparer = null) : base(
            originalSource,
            substractedSource,
            comparer ?? EqualityComparer<T>.Default,
            new ObservableSet<T>(new HashSet<T>(comparer)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDifferenceReadOnlyObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="originalSource">The source collection.</param>
        /// <param name="substractedSource">The items to exclude from <paramref name="originalSource"/>.</param>
        /// <param name="comparer">The equality comparer.</param>
        public SetDifferenceReadOnlyObservableCollection(IReadOnlyObservableCollection<T> originalSource,
            IReadOnlyObservableCollection<T> substractedSource, IEqualityComparer<T> comparer = null) : base(
            originalSource,
            substractedSource,
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
            return container.CountOnFirst > 0 && container.CountOnSecond == 0;
        }
    }
}