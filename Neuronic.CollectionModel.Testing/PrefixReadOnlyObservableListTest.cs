using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class PrefixReadOnlyObservableListTest
    {
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var prool = new PrefixReadOnlyObservableList<int>(observableCollection, i => i <= 5);

            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
        }

        /// <summary>
        /// Testing Add method several times and Inset method.
        /// </summary>
        [TestMethod]
        public void AddItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 5);
            var copy = prool.ListSelect(x => x);

            observableCollection.Add(2);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));

            observableCollection.Insert(3, 11);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));

            observableCollection.Insert(0, 0);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Remove and RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 5, 4, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 5);
            var copy = prool.ListSelect(x => x);

            observableCollection.RemoveAt(9);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));

            observableCollection.Remove(6);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));

            observableCollection.RemoveAt(1);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void MoveItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var frooc = new FilteredReadOnlyObservableList<int>(rool, i => i % 2 == 0);
            Assert.AreEqual(frooc.Count, 5);

            //[2 4 6 8 10]
            observableCollection.Move(1, 7);
            //[4 6 8 2 10]
            observableCollection.Move(3, 5);
            //[4 6 8 2 10]
            observableCollection.Move(3, 2);
            //[6 4 8 2 10]

            int[] a = new[] { 6, 4, 8, 2, 10 };

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
        /// Testing several operations: Remove, Move, Add, Inser and RemoveAt method and indexer of class.
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var frooc = new FilteredReadOnlyObservableList<int>(rool, i => i % 2 == 0);
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
            var collection = new List<Notify>() { new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify() };
            var observableCollection = new ObservableCollection<Notify>(collection);
            observableCollection[0].Prop = 1;
            observableCollection[1].Prop = 2;
            observableCollection[2].Prop = 3;
            observableCollection[3].Prop = 4;
            observableCollection[4].Prop = 5;
            observableCollection[5].Prop = 6;
            observableCollection[6].Prop = 7;
            var rool = new ReadOnlyObservableList<Notify>(observableCollection);

            FilteredReadOnlyObservableList<Notify> frooc = new FilteredReadOnlyObservableList<Notify>(rool, notify => notify.Prop % 2 == 0, nameof(Notify.Prop));
            Assert.AreEqual(frooc.Count, 3);

            int i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);

            observableCollection[1].Prop = 5;

            Assert.AreEqual(frooc.Count, 2);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 2);

            observableCollection[4].Prop = 8;

            Assert.AreEqual(frooc.Count, 3);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);
        }

        /// <summary>
        /// Testing PropertyChanged event.
        /// </summary>
        [TestMethod]
        public void NotifyFilterAutoPropertyTest()
        {
            var collection = new List<Notify>() { new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify() };
            var observableCollection = new ObservableCollection<Notify>(collection);
            observableCollection[0].Prop = 1;
            observableCollection[1].Prop = 2;
            observableCollection[2].Prop = 3;
            observableCollection[3].Prop = 4;
            observableCollection[4].Prop = 5;
            observableCollection[5].Prop = 6;
            observableCollection[6].Prop = 7;
            var rool = new ReadOnlyObservableList<Notify>(observableCollection);

            FilteredReadOnlyObservableList<Notify> frooc = new FilteredReadOnlyObservableList<Notify>(rool, notify => notify.Observe(x => x.Prop % 2 == 0));
            Assert.AreEqual(frooc.Count, 3);

            int i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);

            observableCollection[1].Prop = 5;

            Assert.AreEqual(frooc.Count, 2);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 2);

            observableCollection[4].Prop = 8;

            Assert.AreEqual(frooc.Count, 3);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);
        }

        /// <summary>
        /// Testing PropertyChanged event.
        /// </summary>
        [TestMethod]
        public void NotifyFilterObserveAllTest()
        {
            var collection = new List<Notify>() { new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify(), new Notify() };
            var observableCollection = new ObservableCollection<Notify>(collection);
            observableCollection[0].Prop = 1;
            observableCollection[1].Prop = 2;
            observableCollection[2].Prop = 3;
            observableCollection[3].Prop = 4;
            observableCollection[4].Prop = 5;
            observableCollection[5].Prop = 6;
            observableCollection[6].Prop = 7;
            var rool = new ReadOnlyObservableList<Notify>(observableCollection);

            FilteredReadOnlyObservableList<Notify> frooc = new FilteredReadOnlyObservableList<Notify>(rool, notify => notify.ObserveAll(x => x.Prop % 2 == 0));
            Assert.AreEqual(frooc.Count, 3);

            int i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);

            observableCollection[1].Prop = 5;

            Assert.AreEqual(frooc.Count, 2);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 2);

            observableCollection[4].Prop = 8;

            Assert.AreEqual(frooc.Count, 3);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);
        }

        /// <summary>
        /// Testing PropertyChanged event.
        /// </summary>
        [TestMethod]
        public void ObservableFilterTest()
        {
            var collection = new List<Observable>() { new Observable(), new Observable(), new Observable(), new Observable(), new Observable(), new Observable(), new Observable() };
            var observableCollection = new ObservableCollection<Observable>(collection);
            observableCollection[0].Prop = 1;
            observableCollection[1].Prop = 2;
            observableCollection[2].Prop = 3;
            observableCollection[3].Prop = 4;
            observableCollection[4].Prop = 5;
            observableCollection[5].Prop = 6;
            observableCollection[6].Prop = 7;
            var rool = new ReadOnlyObservableList<Observable>(observableCollection);

            FilteredReadOnlyObservableList<Observable> frooc = new FilteredReadOnlyObservableList<Observable>(rool, notify => notify.Subject.Select(x => x % 2 == 0));
            Assert.AreEqual(frooc.Count, 3);

            int i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);

            observableCollection[1].Prop = 5;

            Assert.AreEqual(frooc.Count, 2);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 2);

            observableCollection[4].Prop = 8;

            Assert.AreEqual(frooc.Count, 3);
            i = 0;
            foreach (var result in frooc.Zip(collection.Where(c => c.Prop % 2 == 0), (actual, expected) => new { actual, expected }))
            {
                Assert.AreEqual(result.expected, result.actual);
                i++;
            }
            Assert.AreEqual(i, 3);
        }
    }
}
