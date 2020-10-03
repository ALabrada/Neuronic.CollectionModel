using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel.Collections
{
    internal interface IDefinition<TElement>
    {
        Func<TElement, IObservable<object>> KeySelector { get; }
        IComparer Comparer { get; }

        IReadOnlyObservableList<TElement> Sort(IEnumerable<TElement> source);
    }

    internal class Definition<TElement, TKey> : IDefinition<TElement>
    {
        public Definition(Func<TElement, IObservable<TKey>> keySelector, Comparer<TKey> comparer)
        {
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            Comparer = comparer ?? Comparer<TKey>.Default;
        }

        public Definition(Expression<Func<TElement, TKey>> keySelector, Comparer<TKey> comparer)
            : this(item => new FunctionObservable<TElement, TKey>(item, keySelector), comparer)
        {
        }

        public Func<TElement, IObservable<TKey>> KeySelector { get; }

        public Comparer<TKey> Comparer { get; }

        Func<TElement, IObservable<object>> IDefinition<TElement>.KeySelector => 
            item =>
            {
                var key = KeySelector(item);
                return key as IObservable<object> ?? new ContravariantObservable(key);
            };

        IComparer IDefinition<TElement>.Comparer => Comparer;

        public IReadOnlyObservableList<TElement> Sort(IEnumerable<TElement> source)
        {
            return new KeySortedReadOnlyObservableList<TElement, TKey>(source, KeySelector, Comparer.Compare, null);
        }

        class ContravariantObservable : IObservable<object>, IObserver<TKey>
        {
            private readonly IObservable<TKey> _other;
            private IObserver<object> _observer;

            public ContravariantObservable(IObservable<TKey> other)
            {
                _other = other;
            }

            public IDisposable Subscribe(IObserver<object> observer)
            {
                _observer = observer;
                return _other.Subscribe(this);
            }

            public void OnCompleted()
            {
                _observer.OnCompleted();
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
            }

            public void OnNext(TKey value)
            {
                _observer.OnNext(value);
            }
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

    struct CompositeObservable : IObservable<IList>
    {
        private readonly IList<IObservable<object>> _values;

        public CompositeObservable(IList<IObservable<object>> values)
        {
            _values = values;
        }

        public IDisposable Subscribe(IObserver<IList> observer)
        {
            return new CompositeSubscription(_values, observer);
        }
    }

    class CompositeSubscription : IDisposable
    {
        private readonly bool[] _isInitialized;
        private readonly IObserver<IList> _observer;
        private readonly List<IDisposable> _subscriptions;

        public CompositeSubscription(IList<IObservable<object>> inputs, IObserver<IList> output)
        {
            var length = inputs.Count;
            _observer = output;
            Key = new object[length];
            _isInitialized = new bool[length];
            _subscriptions = new List<IDisposable>(length);
            _subscriptions.AddRange(inputs.Select((v, i) => v.Subscribe(new CompositeObserver(this, i))));
        }

        public IList Key { get; }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
            Dispose();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
            Dispose();
        }

        public void UpdateValueAt(object value, int index)
        {
            Key[index] = value;
            _isInitialized[index] = true;
            if (Array.TrueForAll(_isInitialized, x => x))
                _observer.OnNext(Key);
        }
    }

    struct CompositeObserver : IObserver<object>
    {
        private readonly CompositeSubscription _subscription;
        private readonly int _index;

        public CompositeObserver(CompositeSubscription subscription, int index)
        {
            _subscription = subscription;
            _index = index;
        }

        public void OnCompleted()
        {
            _subscription.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subscription.OnError(error);
        }

        public void OnNext(object value)
        {
            _subscription.UpdateValueAt(value, _index);
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