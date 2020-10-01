using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class DynamicTransformingReadOnlyObservableListTest
    {
        List<int> deleteItems = new List<int>();

        /// <summary>
        /// Testing indexer of class.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector);

            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 2);
            Assert.AreEqual(trool[2], 3);
            Assert.AreEqual(trool[3], 4);
            Assert.AreEqual(trool[4], 5);
        }

        /// <summary>
        /// Testing Add method two consecutive times.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            observableCollection.Add(8.8);
            Assert.AreEqual(trool[trool.Count - 1], 8);

            observableCollection.Add(9.9);
            Assert.AreEqual(trool[trool.Count - 1], 9);

            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 2);
            Assert.AreEqual(trool[2], 3);
            Assert.AreEqual(trool[3], 4);
            Assert.AreEqual(trool[4], 5);
            Assert.AreEqual(trool[5], 6);
            Assert.AreEqual(trool[6], 7);
            Assert.AreEqual(trool[7], 8);
            Assert.AreEqual(trool[8], 9);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Insert method.
        /// </summary>
        [TestMethod]
        public void InsertElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            int pos = 6;
            observableCollection.Insert(pos, 8.8);
            Assert.AreEqual(trool[pos], 8);

            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 2);
            Assert.AreEqual(trool[2], 3);
            Assert.AreEqual(trool[3], 4);
            Assert.AreEqual(trool[4], 5);
            Assert.AreEqual(trool[5], 6);
            Assert.AreEqual(trool[6], 8);
            Assert.AreEqual(trool[7], 7);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            Assert.IsTrue(trool.Count() == 7);
            int posOld = 0;
            int posNew = 4;
            observableCollection.Move(posOld, posNew);
            Assert.AreEqual(trool[posNew], 1);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Clear method.
        /// </summary>
        [TestMethod]
        public void ClearTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            observableCollection.Clear();
            Assert.AreEqual(trool.Count, 0);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing replace of one element using indexer class.
        /// </summary>
        [TestMethod]
        public void ReplaceElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector, Delete);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            deleteItems.Clear();

            Assert.IsTrue(trool.Count() == 7);
            int pos = 0;
            observableCollection[pos] = 9.9;
            Assert.AreEqual(trool[pos], 9);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 1);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Remove method two consecutive times.
        /// </summary>
        [TestMethod]
        public void OnRemoveElementTest1()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector, Delete);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            Assert.IsTrue(observableCollection.Remove(2.2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            Assert.IsTrue(observableCollection.Remove(5.5));
            Assert.AreEqual(trool.Count, 5);
            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[1], 5);
            Assert.AreEqual(trool[1], 3);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Remove method two consecutive times.
        /// </summary>
        [TestMethod]
        public void OnRemoveElementTest2()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            DynamicTransformingReadOnlyObservableList<double, int> trool = new DynamicTransformingReadOnlyObservableList<double, int>(source, Selector, Delete);
            var copy = new TransformingReadOnlyObservableList<int, int>(trool, x => x);

            Assert.IsTrue(observableCollection.Remove(2.2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            observableCollection.RemoveAt(3);
            Assert.AreEqual(trool.Count, 5);
            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[1], 5);
            Assert.AreEqual(trool[1], 3);

            Assert.IsTrue(trool.SequenceEqual(copy));
        }

        [TestMethod]
        public void TriggerTest()
        {
            var values = new[] { 15, 14, 89, 56, 8, 68, 17, 39, 31, 93, 78, 80, 87, 85, 57, 20, 6, 1 };
            var indexes = new[] { 8, 13, 0, 2 };

            var people = values.Take(values.Length - indexes.Length).Select(x => new Observable(x)).ToList();
            var transfList = new DynamicTransformingReadOnlyObservableList<Observable, int>(people.ListAsObservable(),
                x => x.Subject);
            var copy = new TransformingReadOnlyObservableList<int, int>(transfList, x => x);

            Assert.AreEqual(people.Count, transfList.Count);
            Assert.IsTrue(people.Select(x => x.Prop).SequenceEqual(transfList));
            Assert.IsTrue(people.Select(x => x.Prop).SequenceEqual(copy));

            for (int i = 0; i < indexes.Length; i++)
            {
                var person = people[indexes[i]];
                person.Prop = values[values.Length - i - 1];
            }

            Assert.AreEqual(people.Count, transfList.Count);
            Assert.IsTrue(people.Select(x => x.Prop).SequenceEqual(transfList));
            Assert.IsTrue(people.Select(x => x.Prop).SequenceEqual(copy));
        }

        [TestMethod]
        public void SetOperationsTest()
        {
            var values = new[] { 15, 14, 89, 56, 8, 68, 17, 39, 31, 93, 78, 80, 87, 85, 57, 20, 6, 1 };

            var people = new ObservableSet<Observable>(values.Select(x => new Observable(x)).ToList());
            var transfList = new DynamicTransformingReadOnlyObservableList<Observable, int>(people,
                x => x.Subject);
            var copy = new TransformingReadOnlyObservableList<int, int>(transfList, x => x);

            Assert.AreEqual(people.Count, transfList.Count);
            Assert.IsTrue(new HashSet<int>(people.Select(x => x.Prop)).SetEquals(transfList));
            Assert.IsTrue(new HashSet<int>(people.Select(x => x.Prop)).SetEquals(copy));

            people.Add(new Observable(19));
            people.First().Prop = 82;
            people.Remove(people.Last());

            Assert.AreEqual(people.Count, transfList.Count);
            Assert.IsTrue(new HashSet<int>(people.Select(x => x.Prop)).SetEquals(transfList));
            Assert.IsTrue(new HashSet<int>(people.Select(x => x.Prop)).SetEquals(copy));
        }
        
        private IObservable<int> Selector(double item)
        {
            return ((int)item).AsObservable();
        }

        private void Delete(int item)
        {
            deleteItems.Add(item);
        }
    }
}
