using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Neuronic.CollectionModel.Testing
{
    class Notify: INotifyPropertyChanged
    {
        private int _prop;
        public int Prop {
            get { return _prop; }
            set
            {
                _prop = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}