using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neuronic.CollectionModel.Observables
{
    /// <summary>
    ///     Facilitates the automatic generation of <see cref="IObservable{T}"/> instances
    ///     that depend on and respond to changes in <see cref="INotifyPropertyChanged"/> instances. 
    /// </summary>
    /// <typeparam name="TItem">The type of the observed item.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class PropertyObservableFactory<TItem, TResult>
    {
        internal Func<TItem, TResult> Function { get; }
        internal string[] Triggers { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyObservableFactory{TItem, TResult}"/> class.
        /// </summary>
        /// <param name="function">The function that depends on <typeparamref name="TItem"/>.</param>
        /// <param name="triggers">
        ///     The names of the properties of <typeparamref name="TItem"/> that may affect the
        ///     result of <paramref name="function"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">function</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if <paramref name="triggers"/> has any items, but <typeparamref name="TItem"/>
        ///     does not implement <see cref="INotifyPropertyChanged"/>.
        /// </exception>
        public PropertyObservableFactory(Func<TItem, TResult> function, params string[] triggers)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            Triggers = triggers ?? new string[0];
            if (!CanNotify && Triggers.Length > 0)
                throw new ArgumentException("The instance should implement INotifyPropertyChanged in order to be observed.", nameof(triggers));
        }

        /// <summary>
        ///     Generates an <see cref="PropertyObservableFactory{TItem,TResult}"/> automatically
        ///     from the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The generated <see cref="PropertyObservableFactory{TItem,TResult}"/>.</returns>
        public static PropertyObservableFactory<TItem, TResult> CreateFrom(Expression<Func<TItem, TResult>> expression)
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

        /// <summary>
        ///     Creates an <see cref="IObservable{T}"/> that applies the function to the specified item
        ///     and updates the result when necessary.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="IObservable{T}"/>.</returns>
        public IObservable<TResult> Observe(TItem item) => new NotifyObservable<TItem>(item, Triggers).Select(Function);
    }

    /// <summary>
    ///     Facilitates the automatic generation of <see cref="IObservable{T}"/> instances
    ///     that depend on and respond to changes in pairs of <see cref="INotifyPropertyChanged"/> instances. 
    /// </summary>
    /// <typeparam name="TFirst">The type of the first observed item.</typeparam>
    /// <typeparam name="TSecond">The type of the second observed item.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class PropertyObservableFactory<TFirst, TSecond, TResult>
    {
        internal Func<TFirst, TSecond, TResult> Function { get; }
        internal string[] FirstTriggers { get; }
        internal string[] SecondTriggers { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyObservableFactory{TFirst, TSecond, TResult}"/> class.
        /// </summary>
        /// <param name="function">The function that depends on <typeparamref name="TFirst"/> and <typeparamref name="TSecond"/>.</param>
        /// <param name="firstTriggers">
        ///     The names of the properties of <typeparamref name="TFirst"/> that may affect the
        ///     result of <paramref name="function"/>.
        /// </param>
        /// <param name="secondTriggers">
        ///     The names of the properties of <typeparamref name="TSecond"/> that may affect the
        ///     result of <paramref name="function"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">function</exception>
        /// <exception cref="ArgumentException">     
        ///     Thrown if <paramref name="firstTriggers"/> or <paramref name="secondTriggers"/> have any items,
        ///     but <typeparamref name="TFirst"/> or <typeparamref name="TSecond"/> do not implement
        ///     <see cref="INotifyPropertyChanged"/>, respectivelly.
        /// </exception>
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

        /// <summary>
        ///     Generates an <see cref="PropertyObservableFactory{TFirst,TSecond,TResult}"/> automatically
        ///     from the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The generated <see cref="PropertyObservableFactory{TFirst,TSecond,TResult}"/>.</returns>
        public static PropertyObservableFactory<TFirst, TSecond, TResult> CreateFrom(Expression<Func<TFirst, TSecond, TResult>> expression)
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

        /// <summary>
        ///     Creates an <see cref="IObservable{T}"/> that applies the function to the specified
        ///     items and updates the result when necessary.
        /// </summary>
        /// <param name="first">The first item.</param>
        /// <param name="second">The second item.</param>
        /// <returns>The result.</returns>
        public IObservable<TResult> Observe(TFirst first, TSecond second)
        {
            return new NotifyObservable<TFirst>(first, FirstTriggers).Zip(new NotifyObservable<TSecond>(second, SecondTriggers), Function);
        }

        /// <summary>
        ///     Creates an <see cref="IObservable{T}"/> that applies the function to the specified
        ///     items and updates the result when <paramref name="first"/> changes.
        /// </summary>
        /// <param name="first">The first item.</param>
        /// <param name="second">The second item.</param>
        /// <returns>The result.</returns>
        /// <remarks>
        ///     This method assumes that <paramref name="second"/> is immutable or
        ///     that it's changes cannot affect the result of the function. />
        /// </remarks>
        public IObservable<TResult> ObserveFirst(TFirst first, TSecond second)
        {
            return new NotifyObservable<TFirst>(first, FirstTriggers).Select(x => Function(x, second));
        }

        /// <summary>
        ///     Creates an <see cref="IObservable{T}"/> that applies the function to the specified
        ///     items and updates the result when <paramref name="second"/> changes.
        /// </summary>
        /// <param name="first">The first item.</param>
        /// <param name="second">The second item.</param>
        /// <returns>The result.</returns>
        /// <remarks>
        ///     This method assumes that <paramref name="first"/> is immutable or
        ///     that it's changes cannot affect the result of the function. />
        /// </remarks>
        public IObservable<TResult> ObserveSecond(TFirst first, TSecond second)
        {
            return new NotifyObservable<TSecond>(second, SecondTriggers).Select(x => Function(first, x));
        }
    }
}