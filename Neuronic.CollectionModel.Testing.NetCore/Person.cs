using System.ComponentModel;
using System.Diagnostics;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [DebuggerDisplay("Age = {Age}, Sex = {Sex}")]
    class Person : INotifyPropertyChanged
    {
        private int _age;
        private string _sex;

        public Person(int age, string sex = "")
        {
            this.Age = age;
            this.Sex = sex;
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

        public string Sex
        {
            get => _sex;
            set
            {
                if (Equals(_sex, value))
                    return;
                _sex = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Sex)));
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