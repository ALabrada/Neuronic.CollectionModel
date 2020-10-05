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
    public class InnerJoinReadOnlyObservableCollectionTest
    {  
        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var outerCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(2), new Observable(1) });
            var innerCollection = Enumerable.Range(2, 10).Select(i => new Notify { Prop = i });
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection, 
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x,y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            Assert.AreEqual(ijrooc.Count, 2);
            Assert.IsTrue(new HashSet<int>(new[] { 4, 6 }).SetEquals(ijrooc));
        }

        /// <summary>
        /// Testing Add and Insert method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection,
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            outerCollection.Add(new Observable(99));
            outerCollection.Insert(2, new Observable(8));
            outerCollection.Add(new Observable(4));

            innerCollection.Insert(0, new Notify { Prop = 0 });
            innerCollection.Add(new Notify { Prop = 17 });
            innerCollection.Insert(0, new Notify { Prop = 13 });

            var expectedValues = new HashSet<int>(
                from x in outerCollection join y in innerCollection on x.Prop equals y.Prop select x.Prop + y.Prop);
            Assert.IsTrue(expectedValues.SetEquals(ijrooc));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }

        /// <summary>
        /// Testing RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteElementTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection,
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            outerCollection.RemoveAt(8);
            outerCollection.RemoveAt(2);

            innerCollection.RemoveAt(9);
            innerCollection.RemoveAt(6);
            innerCollection.RemoveAt(4);


            var expectedValues = new HashSet<int>(
                from x in outerCollection join y in innerCollection on x.Prop equals y.Prop select x.Prop + y.Prop);
            Assert.IsTrue(expectedValues.SetEquals(ijrooc));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }

        /// <summary>
        /// Testing Move method several times.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection,
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            outerCollection.Move(1, 3);
            outerCollection.Move(4, 2);
            innerCollection.Move(3, 5);

            var expectedValues = new HashSet<int>(
                from x in outerCollection join y in innerCollection on x.Prop equals y.Prop select x.Prop + y.Prop);
            Assert.IsTrue(expectedValues.SetEquals(ijrooc));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }

        /// <summary>
        /// Testing several operations: Add, Insert and move method and indexer of class. 
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection,
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            outerCollection.Add(new Observable(4));
            innerCollection.Insert(0, new Notify { Prop = 13 });
            outerCollection.RemoveAt(8);
            innerCollection[4] = new Notify { Prop = 99 };

            var expectedValues = new HashSet<int>(
                from x in outerCollection join y in innerCollection on x.Prop equals y.Prop select x.Prop + y.Prop);
            Assert.IsTrue(expectedValues.SetEquals(ijrooc));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }

        [TestMethod]
        public void TriggerTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var ijrooc = new InnerJoinReadOnlyObservableCollection<Observable, Notify, int, int>(outerCollection, innerCollection,
                x => x.Subject, x => x.ObserveAll().Select(i => i.Prop), (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = ijrooc.ListSelect(x => x);

            outerCollection[8].Prop = 4;
            innerCollection[8].Prop = 0;

            var expectedValues = new HashSet<int>(
                from x in outerCollection join y in innerCollection on x.Prop equals y.Prop select x.Prop + y.Prop);
            Assert.IsTrue(expectedValues.SetEquals(ijrooc));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }
    }
}
