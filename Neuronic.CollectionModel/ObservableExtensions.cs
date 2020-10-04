using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Neuronic.CollectionModel.Observables;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Provides a set of extension methods for observables.
    /// </summary>
    public static class ObservableExtensions
    {
        internal static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source,
            Func<TSource, TResult> selector)
        {
            return new MapOperator<TSource,TResult>(source, selector);
        }

        internal static IObservable<TResult> Zip<TSource1, TSource2, TResult>(this IObservable<TSource1> first, 
            IObservable<TSource2> second, Func<TSource1, TSource2, TResult> selector)
        {
            return new ZipOperator<TSource1, TSource2, TResult>(first, second, selector);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObserver{T}"/> that emits <paramref name="item"/> once.
        /// </summary>
        /// <typeparam name="T">The type of the observed item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>The observable.</returns>
        public static IObservable<T> AsObservable<T>(this T item)
        {
            return new NotifyObservable<T>(item);
        }

        public static PropertyObservableFactory<TItem, TResult> FindProperties<TItem, TResult>(
            this Expression<Func<TItem, TResult>> function) where TItem : INotifyPropertyChanged
        {
            return PropertyObservableFactory<TItem, TResult>.FindIn(function);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObserver{T}"/> that generates an event every time one of the specified properties changes.
        /// </summary>
        /// <typeparam name="T">The type of the observed item.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>The observable.</returns>
        public static IObservable<T> Observe<T>(this T item, params string[] properties) where T : INotifyPropertyChanged
        {
            return new NotifyObservable<T>(item, properties);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObserver{T}"/> that generates an event every time any of it's properties changes.
        /// </summary>
        /// <typeparam name="T">The type of the observed item.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns>The observable.</returns>
        public static IObservable<T> ObserveAll<T>(this T item) where T : INotifyPropertyChanged
        {
            return new NotifyObservable<T>(item, string.Empty);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObservable{T}"/> of a value that can be calculated based on the instance's properties.
        /// </summary>
        /// <typeparam name="TItem">The type of the instance.</typeparam>
        /// <typeparam name="TResult">The type of the observed value.</typeparam>
        /// <param name="item">The instance.</param>
        /// <param name="function">The function that calculates the value.</param>
        /// <param name="properties">The properties that can affect the observed value.</param>
        /// <returns>The observable</returns>
        public static IObservable<TResult> ObserveValue<TItem, TResult>(this TItem item, Func<TItem, TResult> function, params string[] properties) where TItem: INotifyPropertyChanged
        {
            return new FunctionObservable<TItem, TResult>(item, function, properties);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObservable{T}"/> of a value that can be calculated based on the instance's properties.
        /// </summary>
        /// <typeparam name="TItem">The type of the instance.</typeparam>
        /// <typeparam name="TResult">The type of the observed value.</typeparam>
        /// <param name="item">The instance.</param>
        /// <param name="expression">The expression that calculates the value.</param>
        /// <returns>The observable</returns>
        /// <remarks>
        ///     The properties that can affect the observable value will be automatically obtained from
        ///     <paramref name="expression"/> using reflection. This may result in observing unnecessary
        ///     read-only properties, as the <see cref="INotifyPropertyChanged"/> does not allow to know
        ///     in advance which properties can generate change notifications.
        /// </remarks>
        public static IObservable<TResult> Observe<TItem, TResult>(this TItem item,
            System.Linq.Expressions.Expression<Func<TItem, TResult>> expression) where TItem : INotifyPropertyChanged
        {
            return new FunctionObservable<TItem, TResult>(item, expression);
        }

        /// <summary>
        ///     Creates an <see cref="System.IObservable{T}"/> of a value that can be calculated based on the instance's properties.
        /// </summary>
        /// <typeparam name="TItem">The type of the instance.</typeparam>
        /// <typeparam name="TResult">The type of the observed value.</typeparam>
        /// <param name="item">The instance.</param>
        /// <param name="function">The function that calculates the value.</param>
        /// <returns>The observable</returns>
        /// <remarks>
        ///     The observable will generate events every time any of the properties of <paramref name="item"/> changes.
        /// </remarks>
        public static IObservable<TResult> ObserveAll<TItem, TResult>(this TItem item,
            Func<TItem, TResult> function) where TItem : INotifyPropertyChanged
        {
            return new FunctionObservable<TItem, TResult>(item, function, string.Empty);
        }
    }
}