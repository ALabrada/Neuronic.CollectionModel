using System.ComponentModel;
using System.Diagnostics;

namespace Neuronic.CollectionModel.Testing
{
    [DebuggerDisplay("Age = {Age}")]
    class Person : INotifyPropertyChanged
    {
        private int _age;

        public Person(int age)
        {
            this.Age = age;
        }

        public int Age
        {
            get { return _age; }
            set
            {
                if (Equals(_age, value))
                    return;
                _age = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Age)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static int CompareByAge(Person person1, Person person2)
        {
            return person1.Age.CompareTo(person2.Age);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}