using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Neuronic.CollectionModel.Observables;

namespace Neuronic.CollectionModel.Collections.Linq
{
    internal interface IDefinition<TElement>
    {
        Func<TElement, IObservable<object>> KeySelector { get; }
        IComparer Comparer { get; }

        IReadOnlyObservableList<TElement> Sort(IEnumerable<TElement> source);
    }

    internal class Definition<TElement, TKey> : IDefinition<TElement>
    {
        public Definition(Func<TElement, IObservable<TKey>> keySelector, IComparer<TKey> comparer = null)
        {
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            Comparer = comparer as Comparer<TKey> ?? new WrapComparer<TKey>(comparer);
        }

        public Definition(Expression<Func<TElement, TKey>> keySelector, IComparer<TKey> comparer = null)
            : this(item => PropertyObservableFactory<TElement, TKey>.CreateFrom(keySelector).Observe(item), comparer)
        {
        }

        public Func<TElement, IObservable<TKey>> KeySelector { get; }

        public Comparer<TKey> Comparer { get; }

        Func<TElement, IObservable<object>> IDefinition<TElement>.KeySelector => 
            item =>
            {
                var key = KeySelector(item);
                return key as IObservable<object> ?? key.Select(x => (object) x);
            };

        IComparer IDefinition<TElement>.Comparer => Comparer;

        public IReadOnlyObservableList<TElement> Sort(IEnumerable<TElement> source)
        {
            return new KeySortedReadOnlyObservableList<TElement, TKey>(source, KeySelector, Comparer.Compare, null);
        }
    }

    class KeySortingQueryableCollection<TElement>: QueryableCollection<TElement>, IOrderedQueryable<TElement>
    {
        private readonly List<IDefinition<TElement>> _definitions = new List<IDefinition<TElement>>();

        public KeySortingQueryableCollection(IEnumerable<TElement> source, IDefinition<TElement> definition) 
            : base((source as KeySortingQueryableCollection<TElement>)?.OriginalSource ?? source)
        {
            if (source is KeySortingQueryableCollection<TElement> previous)
                _definitions.AddRange(previous.Definitions);
            _definitions.Add(definition);
        }

        protected IEnumerable<TElement> OriginalSource => base.Source;

        public override IEnumerable<TElement> Source
        {
            get
            {
                if (Definitions.Count == 0)
                    return base.Source;
                if (Definitions.Count == 1)
                    return Definitions[0].Sort(base.Source);

                var comparer = new CompositeComparer(Definitions.Select(d => d.Comparer).ToArray());
                var keySelector = new Func<TElement, IObservable<IList>>(item =>
                {
                    var observables = new List<IObservable<object>>(Definitions.Count);
                    observables.AddRange(from def in Definitions select def.KeySelector(item));
                    return new CompositeObservable(observables);
                });
                return new KeySortedReadOnlyObservableList<TElement,IList>(base.Source, keySelector, comparer.Compare, null);
            }
        }

        protected IList<IDefinition<TElement>> Definitions
        {
            get
            {
                return _definitions;
            }
        }
    }

    class WrapComparer<T> : Comparer<T>
    {
        private readonly IComparer<T> _inner;

        public WrapComparer(IComparer<T> inner)
        {
            _inner = inner ?? Default;
        }

        public override int Compare(T x, T y)
        {
            return _inner.Compare(x, y);
        }
    }

    class CompositeComparer : Comparer<IList>
    {
        private readonly IComparer[] _comparers;

        public CompositeComparer(params IComparer[] comparers)
        {
            _comparers = comparers;
        }

        public override int Compare(IList x, IList y)
        {
            if (x.Count != _comparers.Length)
                throw new ArgumentException("Invalid item", nameof(x));
            if (y.Count != _comparers.Length)
                throw new ArgumentException("Invalid item", nameof(y));
            for (int i = 0; i < _comparers.Length; i++)
            {
                var result = _comparers[i].Compare(x[i], y[i]);
                if (result != 0)
                    return result;
            }

            return 0;
        }
    }
}