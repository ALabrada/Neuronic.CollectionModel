using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for FilteredReadOnlyObservableCollection class.
    /// </summary>
    [TestClass]
    public class FilteredReadOnlyObservableCollectionTest
    {
        /// <summary>
        /// Testing Add method two consecutive times and Insert method.
        /// </summary>
        [TestMethod]
        public void AddItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            FilteredReadOnlyObservableCollection<int> frooc = new FilteredReadOnlyObservableCollection<int>(rool, i => i % 2 == 0);
            Assert.AreEqual(frooc.Count, 5);
            
            observableCollection.Add(11);
            observableCollection.Add(12);
            observableCollection.Add(14);
            observableCollection.Insert(12,13);

            int item = 2;
            var iterator = frooc.GetEnumerator();
            while (iterator.MoveNext())
            {
                Assert.AreEqual(iterator.Current, item);
                item += 2;
            }
            Assert.AreEqual(item, 16);
            Assert.AreEqual(frooc.Count, 7);
        }

        /// <summary>
        /// Testing Remove and RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            FilteredReadOnlyObservableCollection<int> frooc = new FilteredReadOnlyObservableCollection<int>(rool, i => i % 2 == 0);
            Assert.AreEqual(frooc.Count, 5);

            observableCollection.Remove(3);
            observableCollection.RemoveAt(1);
            observableCollection.Remove(9);

            int item = 4;
            var iterator = frooc.GetEnumerator();
            while (iterator.MoveNext())
            {
                Assert.AreEqual(iterator.Current, item);
                item += 2;
            }
            Assert.AreEqual(item, 12);
            Assert.AreEqual(frooc.Count, 4);
        }

        /// <summary>
        /// Testing Move method three consecutive times.
        /// </summary>
        [TestMethod]
        public void MoveItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            FilteredReadOnlyObservableCollection<int> frooc = new FilteredReadOnlyObservableCollection<int>(rool, i => i % 2 == 0);
            Assert.AreEqual(frooc.Count, 5);

            //[2 4 6 8 10]
            observableCollection.Move(1, 7);
            //[4 6 8 2 10]
            observableCollection.Move(3, 5);
            //[4 6 8 2 10]
            observableCollection.Move(3, 2);
            //[6 4 8 2 10]

            int[] a = new[] {6, 4, 8, 2, 10};

            var iterator = frooc.GetEnumerator();
            int j = 0;
            while (iterator.MoveNext())
            {
                Assert.AreEqual(iterator.Current, a[j]);
                j++;
            }
            Assert.AreEqual(frooc.Count, 5);
        }

        /// <summary>
        /// Testing several operations: Remove, Move, Add, Insert, RemoveAt method and indexer of class.
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20});
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            FilteredReadOnlyObservableCollection<int> frooc = new FilteredReadOnlyObservableCollection<int>(rool, i => i % 2 == 0);
            Assert.AreEqual(frooc.Count, 10);

            //[2 4 6 8 10 12 14 16 18 20]
            observableCollection.Remove(4);
            //[2 6 8 10 12 14 16 18 20]
            observableCollection.Move(6, 13);
            //[2 6 10 12 14 8 16 18 20]
            observableCollection.Add(22);
            //[2 6 10 12 14 8 16 18 20 22]
            observableCollection.Insert(3, 4);
            //[2 4 6 10 12 14 8 16 18 20 22]
            observableCollection.RemoveAt(20);
            //[2 4 6 10 12 14 8 16 18 20]
            observableCollection[0] = 24;
            //[24 2 4 6 10 12 14 8 16 18 20]

            int[] a = new[] { 24, 2, 4, 6, 10, 12, 14, 8, 16, 18, 20 };

            var iterator = frooc.GetEnumerator();
            int j = 0;
            while (iterator.MoveNext())
            {
                Assert.AreEqual(iterator.Current, a[j]);
                j++;
            }
            Assert.AreEqual(frooc.Count, a.Length);
        }

        /// <summary>
        /// Testing PropertyChanged event.
        /// </summary>
        [TestMethod]
        public void NotifyFilterTest()
        {
            var collection = new List<Notify>() {new Notify(), new Notify(), new Notify(), new Notify() };
            var observableCollection = new ObservableCollection<Notify>(collection);
            observableCollection[0].Prop = 1;
            observableCollection[1].Prop = 2;
            observableCollection[2].Prop = 3;
            observableCollection[3].Prop = 4;
            var rool = new ReadOnlyObservableList<Notify>(observableCollection);

            FilteredReadOnlyObservableCollection<Notify> frooc = new FilteredReadOnlyObservableCollection<Notify>(rool, notify => notify.Prop % 2 == 0, nameof(Notify.Prop));
            Assert.AreEqual(frooc.Count, 2);

            int i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new {actual, expected}))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 2);
            observableCollection[1].Prop = 5;

            Assert.AreEqual(frooc.Count, 1);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 1);
        }
    }
}
