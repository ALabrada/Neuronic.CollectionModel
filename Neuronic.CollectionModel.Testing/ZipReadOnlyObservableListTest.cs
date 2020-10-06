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
    public class ZipReadOnlyObservableListTest
    {
        /// <summary>
        /// Testing GetEnumerator method.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var outerCollection = new ObservableCollection<Observable>(new List<Observable>() { new Observable(3), new Observable(2), new Observable(1) });
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(2, 10).Select(i => new Notify { Prop = i }));
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            
            Assert.AreEqual(zrool.Count, 3);
            Assert.IsTrue(new HashSet<int>(new[] { 5, 5, 5 }).SetEquals(zrool));
        }

        /// <summary>
        /// Testing Add and Insert method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 8).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 10).Select(i => new Notify { Prop = i }));
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = zrool.ListSelect(x => x);

            outerCollection.Add(new Observable(99));
            outerCollection.Insert(2, new Observable(8));
            outerCollection.Add(new Observable(4));

            innerCollection.Insert(0, new Notify { Prop = 0 });
            innerCollection.Add(new Notify { Prop = 17 });
            innerCollection.Insert(0, new Notify { Prop = 13 });

            var expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x,y) => x.Prop + y.Prop));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
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
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = zrool.ListSelect(x => x);

            outerCollection.RemoveAt(8);
            outerCollection.RemoveAt(2);

            innerCollection.RemoveAt(9);
            innerCollection.RemoveAt(6);
            innerCollection.RemoveAt(4);


            var expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x, y) => x.Prop + y.Prop));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
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
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = zrool.ListSelect(x => x);

            outerCollection.Move(1, 3);
            outerCollection.Move(4, 2);
            innerCollection.Move(3, 5);

            var expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x, y) => x.Prop + y.Prop));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
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
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => System.Reactive.Linq.Observable.Return(x.Prop + y.Prop));
            var copy = zrool.ListSelect(x => x);

            outerCollection.Add(new Observable(4));
            innerCollection.Insert(0, new Notify { Prop = 13 });
            outerCollection.RemoveAt(8);
            innerCollection[4] = new Notify { Prop = 99 };

            var expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x, y) => x.Prop + y.Prop));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }

        [TestMethod]
        public void TriggerTest()
        {
            var baseline = new System.Reactive.Subjects.BehaviorSubject<int>(0);
            var outerCollection = new ObservableCollection<Observable>(Enumerable.Range(1, 10).Select(i => new Observable(2 * i + 1)));
            var innerCollection = new ObservableCollection<Notify>(Enumerable.Range(1, 5).Select(i => new Notify { Prop = i }));
            var zrool = new ZipReadOnlyObservableList<Observable, Notify, int>(outerCollection, innerCollection,
                (x, y) => x.Subject.CombineLatest(y.ObserveAll().Select(b => b.Prop), (a, b) => a + b).CombineLatest(baseline, (a, b) => a + b));
            var copy = zrool.ListSelect(x => x);

            outerCollection[8].Prop = 4;
            innerCollection[2].Prop = 0;
            outerCollection[3].Prop = 100;

            var expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x, y) => x.Prop + y.Prop + baseline.Value));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
            Assert.IsTrue(expectedValues.SetEquals(copy));

            baseline.OnNext(2);

            expectedValues = new HashSet<int>(outerCollection.Zip(innerCollection, (x, y) => x.Prop + y.Prop + baseline.Value));
            Assert.IsTrue(expectedValues.SetEquals(zrool));
            Assert.IsTrue(expectedValues.SetEquals(copy));
        }
    }
}
