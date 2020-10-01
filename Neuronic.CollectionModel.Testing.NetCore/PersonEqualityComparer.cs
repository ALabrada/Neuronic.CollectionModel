using System.Collections.Generic;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    class PersonEqualityComparer : IEqualityComparer<Person>
    {
        public bool Equals(Person x, Person y)
        {
            return x.Age == y.Age;
        }

        public int GetHashCode(Person obj)
        {
            return obj.Age.GetHashCode();
        }
    }
}