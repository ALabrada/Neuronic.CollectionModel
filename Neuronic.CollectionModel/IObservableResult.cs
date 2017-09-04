using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    /// Represents an operation's result that can change over time.
    /// </summary>
    /// <typeparam name="T">The type of the operation's result</typeparam>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public interface IObservableResult<out T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>
        /// The current result of the operation.
        /// </value>
        T CurrentValue { get; }
        /// <summary>
        /// Occurs when the value of <see cref="CurrentValue"/> is changing.
        /// </summary>
        event EventHandler CurrentValueChanging;
        /// <summary>
        /// Occurs when the value of <see cref="CurrentValue"/> has changed.
        /// </summary>
        event EventHandler CurrentValueChanged;
    }
}