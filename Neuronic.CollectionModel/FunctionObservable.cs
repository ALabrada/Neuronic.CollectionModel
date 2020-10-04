using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using Neuronic.CollectionModel.WeakEventPattern;
using System.Linq.Expressions;
using System.Reflection;

namespace Neuronic.CollectionModel
{
    public class PropertyObservableFactory<TItem, TResult>
    {
        internal Func<TItem, TResult> Function { get; }
        internal string[] Triggers { get; }

        public PropertyObservableFactory(Func<TItem, TResult> function, params string[] triggers)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            Triggers = triggers ?? new string[0];
            if (!CanNotify && Triggers.Length > 0)
                throw new ArgumentException("The instance should implement INotifyPropertyChanged in order to be observed.", nameof(triggers));
        }

        public static PropertyObservableFactory<TItem, TResult> FindIn(Expression<Func<TItem, TResult>> expression)
        {
            var triggers = CanNotify ? FindTriggersIn(expression) : new string[0];
            var func = expression.Compile();
            return new PropertyObservableFactory<TItem, TResult>(func, triggers);
        }

        public static PropertyObservableFactory<Tuple<TItem, TAux>, TResult> FindIn<TAux>(Expression<Func<TItem, TAux, TResult>> expression)
        {
            var triggers = CanNotify ? FindTriggersIn(expression) : new string[0];
            var func = expression.Compile();
            return new PropertyObservableFactory<Tuple<TItem, TAux>, TResult>(
                t => func(t.Item1, t.Item2), triggers);
        }

        private static bool CanNotify =>
#if NETSTANDARD1_1
            typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeof(TItem).GetTypeInfo());
#else
            typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TItem));
#endif

        internal static string[] FindTriggersIn(Expression<Func<TItem, TResult>> expression)
        {
            var detector = new TriggerDetector { Parameters = { expression.Parameters[0] } };
            detector.Visit(expression);
            return detector.Triggers.ToArray();
        }

        internal static string[] FindTriggersIn<TAux>(Expression<Func<TItem, TAux, TResult>> expression)
        {
            var detector = new TriggerDetector { Parameters = { expression.Parameters[0] } };
            detector.Visit(expression);
            return detector.Triggers.ToArray();
        }

        public IObservable<TResult> Observe(TItem item) => new FunctionObservable<TItem, TResult>(item, this);
    }

    struct FunctionObservable<TItem, TResult> : IObservable<TResult>
    {
        private readonly TItem _item;
        private readonly PropertyObservableFactory<TItem, TResult> _factory;

        public FunctionObservable(TItem item, PropertyObservableFactory<TItem, TResult> factory)
        {
            _item = item;
            _factory = factory;
        }

        public FunctionObservable(TItem item, Func<TItem, TResult> function, params string[] triggers)
            : this(item, new PropertyObservableFactory<TItem, TResult>(function, triggers))
        {
        }

        public FunctionObservable(TItem item, Expression<Func<TItem, TResult>> expression)
            : this(item, PropertyObservableFactory<TItem, TResult> .FindIn(expression))
        {
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return new Subscription(_item, _factory.Function, _factory.Triggers, observer);
        }

        class Subscription : IDisposable, IWeakEventListener
        {
            private readonly TItem _item;
            private readonly Func<TItem, TResult> _function;
            private readonly string[] _triggers;
            private readonly IObserver<TResult> _observer;

            public Subscription(TItem item, Func<TItem, TResult> function, string[] triggers,
                IObserver<TResult> observer)
            {
                _item = item;
                _observer = observer ?? throw new ArgumentNullException(nameof(observer));
                _function = function ?? throw new ArgumentNullException(nameof(function));

                _observer.OnNext(_function(_item));
                if (item is INotifyPropertyChanged notifier)
                {
                    _triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.AddListener(notifier, this, trigger);
                }
            }

            public void Dispose()
            {
                if (_item is INotifyPropertyChanged notifier)
                {
                    foreach (var trigger in _triggers)
                        PropertyChangedEventManager.RemoveListener(notifier, this, trigger);
                }
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                var result = _function(_item);
                _observer.OnNext(result);
                return true;
            }
        }
    }
}