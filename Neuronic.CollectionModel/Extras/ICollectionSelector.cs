using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel.Extras
{
    /// <summary>
    ///     Represents a single-item selection mechanism for lists.
    /// </summary>
    /// <typeparam name="T">The type of the list items. This is a covariant type parameter.</typeparam>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public interface ICollectionSelector<out T> : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the index of the selected item.
        ///     Changes to this value should rise the <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <value>
        ///     The index of the selected item.
        /// </value>
        int SelectedIndex { get; set; }

        /// <summary>
        ///     Gets the selected item.
        ///     Changes to this value should rise the <see cref="INotifyPropertyChanged.PropertyChanged" /> and
        ///     <see cref="SelectedItemChanged" /> events.
        /// </summary>
        /// <value>
        ///     The selected item.
        /// </value>
        T SelectedItem { get; }

        /// <summary>
        ///     Gets the items.
        /// </summary>
        /// <value>
        ///     The item list.
        /// </value>
        IReadOnlyObservableList<T> Items { get; }

        /// <summary>
        ///     Occurs before the selected item changes.
        /// </summary>
        event EventHandler SelectedItemChanging;

        /// <summary>
        ///     Occurs when the selected item changes.
        /// </summary>
        event EventHandler SelectedItemChanged;
    }
}