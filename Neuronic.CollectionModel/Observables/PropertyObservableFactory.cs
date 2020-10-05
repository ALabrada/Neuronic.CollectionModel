using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neuronic.CollectionModel.Observables
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

        public IObservable<TResult> Observe(TItem item) => new NotifyObservable<TItem>(item, Triggers).Select(Function);
    }

    public class PropertyObservableFactory<TFirst, TSecond, TResult>
    {
        internal Func<TFirst, TSecond, TResult> Function { get; }
        internal string[] FirstTriggers { get; }
        internal string[] SecondTriggers { get; }

        public PropertyObservableFactory(Func<TFirst, TSecond, TResult> function, string[] firstTriggers, string[] secondTriggers)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            FirstTriggers = firstTriggers ?? new string[0];
            SecondTriggers = secondTriggers ?? new string[0];
            if (!CanNotifyFirst && FirstTriggers.Length > 0)
                throw new ArgumentException("TFirst should implement INotifyPropertyChanged in order to be observed.", nameof(firstTriggers));
            if (!CanNotifySecond && SecondTriggers.Length > 0)
                throw new ArgumentException("TSecond should implement INotifyPropertyChanged in order to be observed.", nameof(secondTriggers));
        }

        public static PropertyObservableFactory<TFirst, TSecond, TResult> FindIn(Expression<Func<TFirst, TSecond, TResult>> expression)
        {
            var firstTriggers = CanNotifyFirst ? FindFirstTriggersIn(expression) : new string[0];
            var secondTrigger = CanNotifySecond ? FindSecondTriggersIn(expression) : new string[0];
            var func = expression.Compile();
            return new PropertyObservableFactory<TFirst, TSecond, TResult>(func, firstTriggers, secondTrigger);
        }

        private static bool CanNotifyFirst =>
#if NETSTANDARD1_1
            typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeof(TFirst).GetTypeInfo());
#else
            typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TFirst));
#endif

        private static bool CanNotifySecond =>
#if NETSTANDARD1_1
            typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(typeof(TSecond).GetTypeInfo());
#else
            typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(TSecond));
#endif

        internal static string[] FindFirstTriggersIn(Expression<Func<TFirst, TSecond, TResult>> expression)
        {
            var detector = new TriggerDetector { Parameters = { expression.Parameters[0] } };
            detector.Visit(expression);
            return detector.Triggers.ToArray();
        }

        internal static string[] FindSecondTriggersIn(Expression<Func<TFirst, TSecond, TResult>> expression)
        {
            var detector = new TriggerDetector { Parameters = { expression.Parameters[1] } };
            detector.Visit(expression);
            return detector.Triggers.ToArray();
        }

        public IObservable<TResult> Observe(TFirst item, TSecond second)
        {
            return new NotifyObservable<TFirst>(item, FirstTriggers).Zip(new NotifyObservable<TSecond>(second, SecondTriggers), Function);
        } 
    }
}