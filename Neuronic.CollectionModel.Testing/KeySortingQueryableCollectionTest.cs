using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class KeySortingQueryableCollectionTest
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
                    new Person(3, "M"),
                    new Person(1, "M"),
                    new Person(7, "M"),
                    new Person(5, "M"),
                    new Person(4, "F"),
                    new Person(6, "F"),
                    new Person(2, "F"),
                    new Person(7, "M")
                });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            Assert.AreEqual(srool[0].Age, 2);
            Assert.AreEqual(srool[1].Age, 4);
            Assert.AreEqual(srool[2].Age, 6);

            Assert.AreEqual(srool[3].Age, 1);
            Assert.AreEqual(srool[4].Age, 3);
            Assert.AreEqual(srool[5].Age, 5);
            Assert.AreEqual(srool[6].Age, 7);
        }

        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<Person>(Enumerable.Range(1, 10).Select(x => new Person(x, x % 2 == 0 ? "F" : "M")));
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            Assert.IsTrue(srool.Take(5).Select(p => p.Sex).SequenceEqual(Enumerable.Repeat("F", 5)));
            Assert.IsTrue(srool.Skip(5).Select(p => p.Sex).SequenceEqual(Enumerable.Repeat("M", 5)));
            Assert.IsTrue(srool.Take(5).Select(p => p.Age).SequenceEqual(Enumerable.Range(1, 5).Select(i => 2 * i)));
            Assert.IsTrue(srool.Skip(5).Select(p => p.Age).SequenceEqual(Enumerable.Range(0, 5).Select(i => 2 * i + 1)));
        }

        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void InvertedIteratorTest()
        {
            var observableCollection = new ObservableCollection<Person>(Enumerable.Range(1, 10).Select(x => new Person(x, x % 2 == 0 ? "F" : "M")));
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderByDescending(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            Assert.IsTrue(srool.Skip(5).Select(p => p.Sex).SequenceEqual(Enumerable.Repeat("F", 5)));
            Assert.IsTrue(srool.Take(5).Select(p => p.Sex).SequenceEqual(Enumerable.Repeat("M", 5)));
            Assert.IsTrue(srool.Skip(5).Select(p => p.Age).SequenceEqual(Enumerable.Range(1, 5).Select(i => 2 * i)));
            Assert.IsTrue(srool.Take(5).Select(p => p.Age).SequenceEqual(Enumerable.Range(0, 5).Select(i => 2 * i + 1)));
        }

        /// <summary>
        /// Testing Add and Insert method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>()
            {
                new Person(3, "M"), new Person(2, "F"), new Person(1, "M")
            });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Person(6, "F"));
            observableCollection.Insert(2, new Person(4, "F"));
            observableCollection.Add(new Person(5, "M"));
            observableCollection.Insert(1, new Person(7, "M"));

            Assert.AreEqual(srool.Count, 7);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
        }

        /// <summary>
        /// Testing RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteElementTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>()
            {
                new Person(3, "M"), new Person(2, "F"), new Person(1, "M"), new Person(7, "M"), new Person(5, "M"), new Person(4, "F"), new Person(6, "F"), new Person(2, "F"), new Person(7, "M")
            });

            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            int[] ages = new[] { 2, 2, 4, 6, 1, 3, 5, 7, 7 };

            Assert.AreEqual(srool.Count, 9);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Age, ages[i]);

            observableCollection.RemoveAt(1);
            observableCollection.RemoveAt(2);

            Assert.AreEqual(srool.Count, 7);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
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
                    new Person(3, "M"),
                    new Person(2, "F"),
                    new Person(1, "M"),
                    new Person(7, "M"),
                    new Person(5, "M"),
                    new Person(4, "F"),
                    new Person(6, "F"),
                });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            observableCollection.Move(1, 3);
            observableCollection.Move(4, 2);
            observableCollection.Move(3, 5);

            Assert.AreEqual(srool.Count, 7);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class. 
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>()
            {
                new Person(3, "M"), new Person(1, "M"), new Person(7, "M"), new Person(5, "M"), new Person(4, "F"), new Person(6, "F"), new Person(2, "F"), new Person(7, "M")
            });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            Assert.AreEqual(srool.Count, 8);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Person(10, "F"));
            observableCollection.Insert(3, new Person(9, "M"));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Person(8, "F");

            Assert.AreEqual(srool.Count, 10);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest1()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>()
            {
                new Person(3, "M"), new Person(1, "M"), new Person(7, "M"), new Person(5, "M"), new Person(4, "F"), new Person(6, "F"), new Person(2, "F"), new Person(7, "M")
            });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();
            var trool = new TransformingReadOnlyObservableList<Person, int>(srool, Selector);

            Assert.AreEqual(srool.Count, 8);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));

            observableCollection.Add(new Person(10, "F"));
            observableCollection.Insert(3, new Person(9, "M"));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Person(8, "F");

            Assert.AreEqual(srool.Count, 10);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).Select(x => x.Age).SequenceEqual(trool));
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest2()
        {
            var observableCollection = new ObservableCollection<Person>(new List<Person>()
            {
                new Person(3, "M"), new Person(1, "M"), new Person(7, "M"), new Person(5, "M"), new Person(4, "F"), new Person(6, "F"), new Person(2, "F"), new Person(7, "M")
            });
            var rool = new ReadOnlyObservableList<Person>(observableCollection);
            var srool = rool.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();
            var trool = new TransformingReadOnlyObservableList<Person, Person>(srool, p => p);

            Assert.AreEqual(srool.Count, 8);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));

            observableCollection.Add(new Person(10, "F"));
            observableCollection.Insert(3, new Person(9, "M"));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Person(-1, "X");
            observableCollection.Insert(4, new Person(8, "F"));

            Assert.AreEqual(srool.Count, 11);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));

            Assert.AreEqual(trool.Count, 11);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(trool));

            observableCollection.Single(p => p.Age == observableCollection.Count - 1).Age = 0;

            Assert.AreEqual(srool.Count, 11);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));

            Assert.AreEqual(trool.Count, 11);
            Assert.IsTrue(observableCollection.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(srool));
        }

        [TestMethod]
        public void TriggerTest()
        {
            var values = new[] { 15, 14, 89, 56, 8, 68, 17, 39, 31, 93, 78, 80, 87, 85, 57, 20, 6, 1 };
            var indexes = new[] { 8, 13, 0, 2 };

            var people = values.Take(values.Length - indexes.Length).Select(x => new Person(x, x % 2 == 0 ? "F" : "M")).ToList();
            var sortedList = people.AsQueryableCollection().OrderBy(x => x.Sex).ThenBy(x => x.Age).ListAsObservable();

            Assert.AreEqual(people.Count, sortedList.Count);
            Assert.IsTrue(people.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(sortedList));

            for (int i = 0; i < indexes.Length; i++)
            {
                var person = people[indexes[i]];
                person.Age = values[values.Length - i - 1];
            }

            people[6].Sex = "F";

            Assert.AreEqual(people.Count, sortedList.Count);
            Assert.IsTrue(people.OrderBy(x => x.Sex).ThenBy(x => x.Age).SequenceEqual(sortedList));
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
    }
}
