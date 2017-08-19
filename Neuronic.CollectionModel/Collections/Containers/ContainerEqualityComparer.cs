using System.Collections.Generic;

namespace Neuronic.CollectionModel.Collections.Containers
{
    internal class ContainerEqualityComparer<T, TContainer> : IEqualityComparer<TContainer>
        where TContainer : ItemContainer<T>
    {
        private readonly IEqualityComparer<T> _sourceComparer;

        public ContainerEqualityComparer(IEqualityComparer<T> sourceComparer)
        {
            _sourceComparer = sourceComparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(TContainer x, TContainer y)
        {
            return _sourceComparer.Equals(x.Item, y.Item);
        }

        public int GetHashCode(TContainer obj)
        {
            return _sourceComparer.GetHashCode(obj.Item);
        }
    }
}