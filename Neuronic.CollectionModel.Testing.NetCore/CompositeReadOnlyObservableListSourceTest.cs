using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;
using Neuronic.CollectionModel.Collections.Containers;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    /// <summary>
    /// Unit test for CompositeReadOnlyObservableListSource class.
    /// </summary>
    [TestClass]
    public class CompositeReadOnlyObservableListSourceTest
    {
        List<object> deleteItems = new List<object>();

        /// <summary>
        /// Testing View property.
        /// </summary>
        [TestMethod]
        public void IteratorTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);

            int i = 1;
            for (int j = 0; j < croocs.Count; j++)
                foreach (var v in croocs[j].Collection)
                {
                    Assert.AreEqual(v, i);
                    i++;
                }
            Assert.AreEqual(i, 7);

            var iterator = croocs.View;
            i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
        }

        /// <summary>
        /// Testing Add method three consecutive times.  
        /// </summary>
        [TestMethod]
        public void AddItemsTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 7, 8, 9 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);

            observableCollection1.Add(4);
            observableCollection1.Add(5);
            observableCollection1.Add(6);

            var observableCollection3 = new ObservableCollection<int>(new List<int>() { 10, 11, 12 });
            var rool3 = new ReadOnlyObservableList<int>(observableCollection3);
            var cc3 = new CollectionContainer<int>(rool3);
            croocs.Add(cc3);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }

            Assert.AreEqual(i, 13);
        }

        /// <summary>
        /// Testing Insert method three consecutive times. 
        /// </summary>
        [TestMethod]
        public void InsertItemsTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 7, 8, 9 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);

            observableCollection2.Insert(0, 4);
            observableCollection2.Insert(1, 5);
            observableCollection2.Insert(2, 6);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
            Assert.AreEqual(iterator.Count, 9);
        }

        /// <summary>
        /// Testing Remove method two consecutive times.
        /// </summary>
        [TestMethod]
        public void RemoveItemsTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 7 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 8, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);

            observableCollection1.Remove(7);
            observableCollection2.RemoveAt(2);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
            Assert.AreEqual(iterator.Count, 6);
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void MoveTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 2, 1, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);
            observableCollection1.Move(1, 2);

            var iterator = croocs.View;
            int i = 0;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
            Assert.AreEqual(iterator.Count, 7);
        }

        /// <summary>
        /// Testing several operations Add, Insert and Remove.
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 5, 7, 8 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);

            //[1 2 3 5 7 8]
            observableCollection1.Add(4);
            //[1 2 3 4 5 7 8]
            observableCollection1.Add(8);
            //[1 2 3 8 4 5 7 8]
            observableCollection2.Insert(1, 6);
            //[1 2 3 8 4 5 6 7 8]
            observableCollection1.Remove(8);
            //[1 2 3 4 5 6 7 8]
            observableCollection1.Move(1, 3);
            //[1 3 4 2 5 6 7 8]

            var iterator = croocs.View;

            Assert.AreEqual(iterator.Count, 8);
            Assert.AreEqual(iterator[0], 1);
            Assert.AreEqual(iterator[1], 3);
            Assert.AreEqual(iterator[2], 4);
            Assert.AreEqual(iterator[3], 2);
            Assert.AreEqual(iterator[4], 5);
            Assert.AreEqual(iterator[5], 6);
            Assert.AreEqual(iterator[6], 7);
            Assert.AreEqual(iterator[7], 8);
        }

        /// <summary>
        /// Testing several operations insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void OperationWhithTransformingTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            var croocs = new CompositeReadOnlyObservableListSource<int>(cc);
            var viewCroocs = croocs.View;

            TransformingReadOnlyObservableList<int, int> trool = new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector, Delete);

            Assert.AreEqual(trool[0], 0);
            Assert.AreEqual(trool[1], 1);
            Assert.AreEqual(trool[2], 2);
            Assert.AreEqual(trool[3], 3);
            Assert.AreEqual(trool[4], 4);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 6);

            viewCroocs.CollectionChanged += ViewCroocs_CollectionChanged;

            //[0 1 2 3 4 5 6]
            observableCollection1.Remove(2);
            //[0 1 3 4 5 6]
            observableCollection2.Add(7);
            //[0 1 3 4 5 6 7]
            observableCollection1.Insert(2, 2);
            //[0 1 2 3 4 5 6 7]
            observableCollection1.Move(1, 2);
            //[0 2 1 3 4 5 6]
            observableCollection1.Swap(1, 2);
            //[0 1 2 3 4 5 6 7]

            Assert.AreEqual(trool.Count, 8);
            Assert.AreEqual(viewCroocs.Count, 8);
            Assert.AreEqual(trool[0], 0);
            Assert.AreEqual(trool[1], 1);
            Assert.AreEqual(trool[2], 2);
            Assert.AreEqual(trool[3], 3);
            Assert.AreEqual(trool[4], 4);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 6);
            Assert.AreEqual(trool[7], 7);

            Assert.AreEqual(deleteItems[0], 2);
        }

        /// <summary>
        /// Testing add CollectionContainer into TransformingReadOnlyObservableList class. 
        /// </summary>
        [TestMethod]
        public void AddListWithTransformingTest()
        {
            var croocs = new CompositeReadOnlyObservableListSource<int>();
            var viewCroocs = croocs.View;
            viewCroocs.CollectionChanged += ViewCroocs_CollectionChanged1;
            TransformingReadOnlyObservableList<int, int> trool = new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector);

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            croocs.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            croocs.Add(cc2);

            Assert.AreEqual(viewCroocs.Count, 7);
            Assert.AreEqual(viewCroocs[0], 0);
            Assert.AreEqual(viewCroocs[1], 1);
            Assert.AreEqual(viewCroocs[2], 2);
            Assert.AreEqual(viewCroocs[3], 3);
            Assert.AreEqual(viewCroocs[4], 4);
            Assert.AreEqual(viewCroocs[5], 5);
            Assert.AreEqual(viewCroocs[6], 6);

            Assert.AreEqual(trool.Count, 7);
            Assert.AreEqual(trool[0], 0);
            Assert.AreEqual(trool[1], 1);
            Assert.AreEqual(trool[2], 2);
            Assert.AreEqual(trool[3], 3);
            Assert.AreEqual(trool[4], 4);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 6);
        }

        /// <summary>
        /// Testing several operations insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void OperationListWithTransformingTest()
        {
            var croocs = new CompositeReadOnlyObservableListSource<int>();
            var viewCroocs = croocs.View;
            viewCroocs.CollectionChanged += ViewCroocs_CollectionChanged1;
            TransformingReadOnlyObservableList<int, int> trool =
                new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector);

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            croocs.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            croocs.Add(cc2);

            var observableCollection3 = new ObservableCollection<int>(new List<int>() { 7, 8, 9 });
            var rool3 = new ReadOnlyObservableList<int>(observableCollection3);
            var cc3 = new CollectionContainer<int>(rool3);

            var observableCollection4 = new ObservableCollection<int>(new List<int>() { 10, 11, 12 });
            var rool4 = new ReadOnlyObservableList<int>(observableCollection4);
            var cc4 = new CollectionContainer<int>(rool4);

            croocs.RemoveAt(1);
            croocs.Insert(0, cc3);
            croocs.Add(cc4);
            croocs.Move(0, 2);

            Assert.AreEqual(trool.Count, 10);
            Assert.AreEqual(trool[0], 0);
            Assert.AreEqual(trool[1], 1);
            Assert.AreEqual(trool[2], 2);
            Assert.AreEqual(trool[3], 3);
            Assert.AreEqual(trool[4], 10);
            Assert.AreEqual(trool[5], 11);
            Assert.AreEqual(trool[6], 12);
            Assert.AreEqual(trool[7], 7);
            Assert.AreEqual(trool[8], 8);
            Assert.AreEqual(trool[9], 9);
        }

        /// <summary>
        /// Testing Move method insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void MoveListWithTransformingTest()
        {
            var croocs = new CompositeReadOnlyObservableListSource<int>();
            var viewCroocs = croocs.View;
            viewCroocs.CollectionChanged += ViewCroocs_CollectionChanged1;
            TransformingReadOnlyObservableList<int, int> trool =
                new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector);

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 1, 2, 3, 4 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            croocs.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            croocs.Add(cc2);

            var observableCollection3 = new ObservableCollection<int>(new List<int>() { 7, 8, 9 });
            var rool3 = new ReadOnlyObservableList<int>(observableCollection3);
            var cc3 = new CollectionContainer<int>(rool3);
            croocs.Add(cc3);

            var observableCollection4 = new ObservableCollection<int>(new List<int>() { 10 });
            var rool4 = new ReadOnlyObservableList<int>(observableCollection4);
            var cc4 = new CollectionContainer<int>(rool4);
            croocs.Add(cc4);

            var observableCollection5 = new ObservableCollection<int>(new List<int>() { 11, 12, 13, 14 });
            var rool5 = new ReadOnlyObservableList<int>(observableCollection5);
            var cc5 = new CollectionContainer<int>(rool5);
            croocs.Add(cc5);

            //[0 1 2 3 4 5 6 7 8 9 10 11 12 13 14]
            croocs.Move(0, 4);
            //[5 6 7 8 9 10 11 12 13 14 0 1 2 3 4]
            croocs.Move(1, 3);
            //[5 6 10 11 12 13 14 7 8 9 0 1 2 3 4]
            croocs.Move(1, 4);
            //[5 6 11 12 13 14 7 8 9 0 1 2 3 4 10]

            var view = croocs.View;
            Assert.AreEqual(trool.Count, 15);
            Assert.AreEqual(trool[0], 5);
            Assert.AreEqual(trool[1], 6);
            Assert.AreEqual(trool[2], 11);
            Assert.AreEqual(trool[3], 12);
            Assert.AreEqual(trool[4], 13);
            Assert.AreEqual(trool[5], 14);
            Assert.AreEqual(trool[6], 7);
            Assert.AreEqual(trool[7], 8);
            Assert.AreEqual(trool[8], 9);
            Assert.AreEqual(trool[9], 0);
            Assert.AreEqual(trool[10], 1);
            Assert.AreEqual(trool[11], 2);
            Assert.AreEqual(trool[12], 3);
            Assert.AreEqual(trool[13], 4);
            Assert.AreEqual(trool[14], 10);
        }

        [TestMethod]
        public void TestRemoveSourceCollection()
        {
            var coll1 = Enumerable.Range(0, 5).ToList();
            var coll2 = Enumerable.Range(5, 5).ToList();
            var coll3 = Enumerable.Range(10, 5).ToList();

            var union = new ObservableCollection<IEnumerable<int>> {coll1, coll2, coll3};
            var collection = new ReadOnlyObservableList<IEnumerable<int>>(union).ListSelectMany(coll => coll);
            var copy = collection.ListSelect(i => i);

            Assert.AreEqual(15, collection.Count);
            Assert.IsTrue(Enumerable.Range(0, 15).SequenceEqual(collection));
            Assert.AreEqual(15, copy.Count);
            Assert.IsTrue(collection.SequenceEqual(copy));

            union.RemoveAt(1);

            Assert.AreEqual(10, collection.Count);
            Assert.IsTrue(Enumerable.Range(0, 5).Concat(Enumerable.Range(10, 5)).SequenceEqual(collection));
            Assert.AreEqual(10, copy.Count);
            Assert.IsTrue(collection.SequenceEqual(copy));
        }

        private void ViewCroocs_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private void ViewCroocs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        private int Selector(int item)
        {
            return (int)item;
        }

        private void Delete(int item)
        {
            deleteItems.Add(item);
        }
    }
}

