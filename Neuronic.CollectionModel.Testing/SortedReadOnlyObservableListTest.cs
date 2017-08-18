using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for SortedReadOnlyObservableList class.
    /// </summary>
    [TestClass]
    public class SortedReadOnlyObservableListTest
    {
        /// <summary>
        /// Testing indexer of class.
        /// </summary>
        [TestMethod]
        public void IndexerTest()
        {
            var observableCollection =
                new ObservableCollection<Person>(new List<Person>()
                {
                    new Person(3),
                    new Person(1),
                    new Person(7),
                    new Person(5),
                    new Person(4),
                    new Person(6),
                    new Person(2),
                    new Person(7)
                });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            Assert.AreEqual(srool[0].Age, 1);
            Assert.AreEqual(srool[1].Age, 2);
            Assert.AreEqual(srool[2].Age, 3);
            Assert.AreEqual(srool[3].Age, 4);
            Assert.AreEqual(srool[4].Age, 5);
            Assert.AreEqual(srool[5].Age, 6);
            Assert.AreEqual(srool[6].Age, 7);

            
        }
        
        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(2), new Person(1) });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            var s = srool.GetEnumerator();
            int j = 1;
            while (s.MoveNext())
            {
                var current = s.Current;
                Assert.AreEqual(current.Age, j);
                j++;
            }

            Assert.AreEqual(srool.Count, 3);
            for (int i = 0; i < observableCollection.Count; i++)
            {
                Assert.AreEqual(srool[i].Age, i + 1);
            }
        }

        /// <summary>
        /// Testing Add and Insert method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(2), new Person(1) });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Person(6));
            observableCollection.Insert(2, new Person(4));
            observableCollection.Add(new Person(5));
            observableCollection.Insert(1,new Person(7));

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
            {
                Assert.AreEqual(srool[i].Age, i + 1);
            }
        }

        /// <summary>
        /// Testing RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteElementTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(2), new Person(1), new Person(7), new Person(5), new Person(4), new Person(6), new Person(2), new Person(7)});
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            int[] ages = new[] {1, 2, 2, 3, 4, 5, 6, 7, 7};

            Assert.AreEqual(srool.Count, 9);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, ages[i]);

            observableCollection.RemoveAt(1);
            observableCollection.RemoveAt(2);

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i+1);
        }

        /// <summary>
        /// Testing Move method several times.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var observableCollection =
                new ObservableCollection<Person>(new List<Person>()
                {
                    new Person(3),
                    new Person(2),
                    new Person(1),
                    new Person(7),
                    new Person(5),
                    new Person(4),
                    new Person(6),
                });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            observableCollection.Move(1,3);
            observableCollection.Move(4,2);
            observableCollection.Move(3,5);

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i + 1);
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class. 
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(1), new Person(7), new Person(5), new Person(4), new Person(6), new Person(2), new Person(7) });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, ages[i]);

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Person(10));
            observableCollection.Insert(3, new Person(9));
            observableCollection.Move(5,2);
            observableCollection[8] = new Person(8);

            Assert.AreEqual(srool.Count, 10);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i + 1);
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest1()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(1), new Person(7), new Person(5), new Person(4), new Person(6), new Person(2), new Person(7) });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));
            var trool = new TransformingReadOnlyObservableList<Person, int>(srool, Selector);

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, ages[i]);

            observableCollection.Add(new Person(10));
            observableCollection.Insert(3, new Person(9));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Person(8);

            Assert.AreEqual(srool.Count, 10);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i + 1);

            for (int i = 0; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i], i+1);
            }
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest2()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>() { new Person(3), new Person(1), new Person(7), new Person(5), new Person(4), new Person(6), new Person(2), new Person(7) });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = new SortedReadOnlyObservableList<Person>(rool, Person.CompareByAge, nameof(Person.Age));
            var trool = new TransformingReadOnlyObservableList<Person, Person>(srool, p => p);

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, ages[i]);

            observableCollection.Add(new Person(10));
            observableCollection.Insert(3, new Person(9));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Person(-1);
            observableCollection.Insert(4, new Person(8));

            Assert.AreEqual(srool.Count, 11);
            Assert.AreEqual(srool[0].Age, -1);
            for (int i = 1; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i);

            Assert.AreEqual(trool.Count, 11);
            Assert.AreEqual(trool[0].Age, -1);
            for (int i = 1; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i].Age, i);
            }

            observableCollection.Single(p => p.Age == observableCollection.Count - 1).Age = 0;

            Assert.AreEqual(srool.Count, 11);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, i - 1);

            Assert.AreEqual(trool.Count, 11);
            for (int i = 0; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i].Age, i - 1);
            }
        }

        [TestMethod]
        public void SetOperationsTest()
        {
            const int initialCount = 15;
            const int removeItems = 4;
            const int insertItems = 2;
            var items = new[] { 40, 63, 98, 20, 24, 76, 96, 53, 5, 11, 29, 12, 46, 59, 7, 45, 86, 91, 57, 95 };
            var indexes = new[] { 6, 2, 5, 11, 0, 3, 13, 1 };

            var originalSet = new HashSet<Person>(items.Take(initialCount).Select(x => new Person(x)), new PersonEqualityComparer());
            var set = new ObservableSet<Person>(originalSet);
            var sorted = new SortedReadOnlyObservableList<Person>(set, (x, y) => x.Age.CompareTo(y.Age),
                new PersonEqualityComparer(), nameof(Person.Age));
            
            Assert.AreEqual(initialCount, set.Count);
            Assert.AreEqual(initialCount, sorted.Count);
            Assert.IsTrue(items.Take(initialCount).Select(x => new Person(x)).OrderBy(p => p.Age).SequenceEqual(sorted, new PersonEqualityComparer()));

            foreach (var person in indexes.Take(removeItems).Select(i => new Person(items[i])))
                set.Remove(person);

            Assert.AreEqual(initialCount - removeItems, set.Count);
            Assert.AreEqual(initialCount - removeItems, sorted.Count);
            Assert.IsTrue(originalSet.OrderBy(p => p.Age).SequenceEqual(sorted, new PersonEqualityComparer()));

            foreach (var person in items.Skip(initialCount).Take(insertItems).Select(x => new Person(x)))
                set.Add(person);

            Assert.AreEqual(initialCount - removeItems + insertItems, set.Count);
            Assert.AreEqual(initialCount - removeItems + insertItems, sorted.Count);
            Assert.IsTrue(originalSet.OrderBy(p => p.Age).SequenceEqual(sorted, new PersonEqualityComparer()));

            foreach (var result in indexes.Skip(removeItems)
                .Zip(items.Skip(initialCount + insertItems),
                    (i, x) => new {OldItem = new Person(items[i]), NewItem = new Person(x)}))
                set.SymmetricExceptWith(new[] {result.OldItem, result.NewItem});

            Assert.AreEqual(initialCount - removeItems + insertItems, set.Count);
            Assert.AreEqual(initialCount - removeItems + insertItems, sorted.Count);
            Assert.IsTrue(originalSet.OrderBy(p => p.Age).SequenceEqual(sorted, new PersonEqualityComparer()));

            set.Clear();

            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(0, sorted.Count);
        }

        private int Selector(Person item)
        {
            return item.Age;
        }

        private void SroolOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Count")
            {
            }

            if (propertyChangedEventArgs.PropertyName == "Item[]")
            {
            }
        }

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
}
