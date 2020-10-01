using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;
using System.Reactive;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class KeySortedReadOnlyObservableListTest
    {
        /// <summary>
        /// Testing indexer of class.
        /// </summary>
        [TestMethod]
        public void IndexerTest()
        {
            var observableCollection =
                new ObservableCollection<Observable>(new List<Observable>()
                {
                    new Observable(3),
                    new Observable(1),
                    new Observable(7),
                    new Observable(5),
                    new Observable(4),
                    new Observable(6),
                    new Observable(2),
                    new Observable(7)
                });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            Assert.AreEqual(srool[0].Prop, 1);
            Assert.AreEqual(srool[1].Prop, 2);
            Assert.AreEqual(srool[2].Prop, 3);
            Assert.AreEqual(srool[3].Prop, 4);
            Assert.AreEqual(srool[4].Prop, 5);
            Assert.AreEqual(srool[5].Prop, 6);
            Assert.AreEqual(srool[6].Prop, 7);
        }

        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(2), new Observable(1) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            var s = srool.GetEnumerator();
            int j = 1;
            while (s.MoveNext())
            {
                var current = s.Current;
                Assert.AreEqual(current.Prop, j);
                j++;
            }

            Assert.AreEqual(srool.Count, 3);
            for (int i = 0; i < observableCollection.Count; i++)
            {
                Assert.AreEqual(srool[i].Prop, i + 1);
            }
        }

        /// <summary>
        /// Testing Add and Insert method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(2), new Observable(1) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Observable(6));
            observableCollection.Insert(2, new Observable(4));
            observableCollection.Add(new Observable(5));
            observableCollection.Insert(1, new Observable(7));

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
            {
                Assert.AreEqual(srool[i].Prop, i + 1);
            }
        }

        /// <summary>
        /// Testing RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteElementTest()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(2), new Observable(1), new Observable(7), new Observable(5), new Observable(4), new Observable(6), new Observable(2), new Observable(7) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            int[] ages = new[] { 1, 2, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 9);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, ages[i]);

            observableCollection.RemoveAt(1);
            observableCollection.RemoveAt(2);

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i + 1);
        }

        /// <summary>
        /// Testing Move method several times.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var observableCollection =
                new ObservableCollection<Observable>(new List<Observable>()
                {
                    new Observable(3),
                    new Observable(2),
                    new Observable(1),
                    new Observable(7),
                    new Observable(5),
                    new Observable(4),
                    new Observable(6),
                });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            observableCollection.Move(1, 3);
            observableCollection.Move(4, 2);
            observableCollection.Move(3, 5);

            Assert.AreEqual(srool.Count, 7);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i + 1);
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class. 
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(1), new Observable(7), new Observable(5), new Observable(4), new Observable(6), new Observable(2), new Observable(7) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, ages[i]);

            srool.PropertyChanged += SroolOnPropertyChanged;

            observableCollection.Add(new Observable(10));
            observableCollection.Insert(3, new Observable(9));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Observable(8);

            Assert.AreEqual(srool.Count, 10);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i + 1);
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest1()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(1), new Observable(7), new Observable(5), new Observable(4), new Observable(6), new Observable(2), new Observable(7) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);
            var trool = new TransformingReadOnlyObservableList<Observable, int>(srool, x => x.Prop);

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, ages[i]);

            observableCollection.Add(new Observable(10));
            observableCollection.Insert(3, new Observable(9));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Observable(8);

            Assert.AreEqual(srool.Count, 10);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i + 1);

            for (int i = 0; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i], i + 1);
            }
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class whith TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void TransformingOperationsTest2()
        {
            var observableCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(1), new Observable(7), new Observable(5), new Observable(4), new Observable(6), new Observable(2), new Observable(7) });
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);
            var srool = new KeySortedReadOnlyObservableList<Observable, int>(rool, x => x.Subject, null, null);
            var trool = new TransformingReadOnlyObservableList<Observable, Observable>(srool, p => p);

            int[] ages = new[] { 1, 2, 3, 4, 5, 6, 7, 7 };

            Assert.AreEqual(srool.Count, 8);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, ages[i]);

            observableCollection.Add(new Observable(10));
            observableCollection.Insert(3, new Observable(9));
            observableCollection.Move(5, 2);
            observableCollection[8] = new Observable(-1);
            observableCollection.Insert(4, new Observable(8));

            Assert.AreEqual(srool.Count, 11);
            Assert.AreEqual(srool[0].Prop, -1);
            for (int i = 1; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i);

            Assert.AreEqual(trool.Count, 11);
            Assert.AreEqual(trool[0].Prop, -1);
            for (int i = 1; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i].Prop, i);
            }

            observableCollection.Single(p => p.Prop == observableCollection.Count - 1).Prop = 0;

            Assert.AreEqual(srool.Count, 11);
            for (int i = 0; i < observableCollection.Count; i++)
                Assert.AreEqual(srool[i].Prop, i - 1);

            Assert.AreEqual(trool.Count, 11);
            for (int i = 0; i < trool.Count; i++)
            {
                Assert.AreEqual(trool[i].Prop, i - 1);
            }
        }

        [TestMethod]
        public void TriggerTest()
        {
            var values = new[] { 15, 14, 89, 56, 8, 68, 17, 39, 31, 93, 78, 80, 87, 85, 57, 20, 6, 1 };
            var indexes = new[] { 8, 13, 0, 2 };

            var people = values.Take(values.Length - indexes.Length).Select(x => new Observable(x)).ToList();
            var sortedList = new KeySortedReadOnlyObservableList<Observable, int>(people.ListAsObservable(),
                x => x.Subject, null, null);

            Assert.AreEqual(people.Count, sortedList.Count);
            Assert.IsTrue(people.OrderBy(x => x.Prop).SequenceEqual(sortedList));

            for (int i = 0; i < indexes.Length; i++)
            {
                var person = people[indexes[i]];
                person.Prop = values[values.Length - i - 1];
            }

            Assert.AreEqual(people.Count, sortedList.Count);
            Assert.IsTrue(people.OrderBy(x => x.Prop).SequenceEqual(sortedList));
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