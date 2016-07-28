using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    public interface ICollectionSelector<out T> : INotifyPropertyChanged
    {
        int SelectedIndex { get; set; }
        T SelectedItem { get; }
        IReadOnlyObservableList<T> Items { get; }
        event EventHandler SelectedItemChanged;
    }
}