using System;
using System.ComponentModel;

namespace Neuronic.CollectionModel
{
    public class ItemContainer<TItem>
    {
        private readonly Predicate<TItem> _filter;

        public ItemContainer(TItem item, Predicate<TItem> filter)
        {
            _filter = filter;
            Item = item;
            IsIncluded = _filter(Item);
        }

        public TItem Item { get; }
        public bool IsIncluded { get; private set; }

        public void AttachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.AddHandler(notify, ItemOnTriggerPropertyChanged, name);
        }

        public void DetachTriggers(string[] triggers)
        {
            var notify = Item as INotifyPropertyChanged;
            if (notify == null) return;
            foreach (var name in triggers)
                PropertyChangedEventManager.RemoveHandler(notify, ItemOnTriggerPropertyChanged, name);
        }

        private void ItemOnTriggerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var wasIncluded = IsIncluded;
            IsIncluded = _filter(Item);
            if (IsIncluded != wasIncluded)
                IsIncludedChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsIncludedChanged;
    }
}