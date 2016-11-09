using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    /// <summary>
    ///     Represents an object that can be selected.
    /// </summary>
    /// <seealso>
    ///     <cref>System.ComponentModel.INotifyPropertyChanged</cref>
    /// </seealso>
    public interface ISelectableItem : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        bool IsSelected { get; set; }

        /// <summary>
        ///     Occurs when <see cref="IsSelected" /> changes.
        /// </summary>
        event EventHandler SelectionChanged;
    }
}