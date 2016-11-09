using System.Diagnostics;

namespace Neuronic.CollectionModel.Testing
{
    [DebuggerDisplay("Age = {Age}")]
    class Person
    {
        public Person(int age)
        {
            this.Age = age;
        }

        public int Age { get; set; }


        public static int CompareByAge(Person person1, Person person2)
        {
            return person1.Age.CompareTo(person2.Age);
        }
    }
}