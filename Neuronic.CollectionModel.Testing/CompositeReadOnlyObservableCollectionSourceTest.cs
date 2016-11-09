using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for CompositeReadOnlyObservableCollectionSource class.
    /// </summary>
    [TestClass]
    public class CompositeReadOnlyObservableCollectionSourceTest
    {
        List<object> deleteItems = new List<object>();

        /// <summary>
        /// Testing View property.
        /// </summary>
        [TestMethod]
        public void IndexerTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>(6);

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 1, 2, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            cc.Add(new CollectionContainer<int>(rool1));

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 5, 6 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            cc.Add(new CollectionContainer<int>(rool2));

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }

            i = 1;

            for (int j = 0; j < croocs.Count; j++)
                foreach (var v in croocs[j].Collection)
                {
                    Assert.AreEqual(v, i);
                    i++;
                }
            Assert.AreEqual(i, 7);
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

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);

            observableCollection1.Add(4);
            observableCollection1.Add(5);
            observableCollection1.Add(6);

            var observableCollection3 = new ObservableCollection<int>(new List<int>() { 10, 11, 12 });
            var rool3 = new ReadOnlyObservableList<int>(observableCollection3);
            var cc3 = new CollectionContainer<int>(rool3);
            croocs.Add(cc3);

            var observableCollection4 = new ObservableCollection<int>(new List<int>() { 13, 14, 15 });
            var rool4 = new ReadOnlyObservableList<int>(observableCollection4);
            var cc4 = new CollectionContainer<int>(rool4);
            croocs.Add(cc4);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }

            Assert.AreEqual(i, 16);
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

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);

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

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);

            observableCollection1.Remove(7);
            observableCollection2.RemoveAt(2);

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
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

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);

            //[1 2 3 5 7 8]
            observableCollection1.Add(4);
            //[1 2 3 4 5 7 8]
            observableCollection1.Add(8);
            //[1 2 3 4 8 5 7 8]
            observableCollection2.Insert(1, 6);
            //[1 2 3 4 8 5 6 7 8]
            observableCollection1.Remove(8);
            //[1 2 3 4 5 6 7 8]

            var iterator = croocs.View;
            int i = 1;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }

        }

        /// <summary>
        /// Testing Move method two consecutive times.
        /// </summary>
        [TestMethod]
        public void MoveTest()
        {
            List<CollectionContainer<int>> cc = new List<CollectionContainer<int>>();

            var observableCollection1 = new ObservableCollection<int>(new List<int>() { 0, 2, 1, 3 });
            var rool1 = new ReadOnlyObservableList<int>(observableCollection1);
            var cc1 = new CollectionContainer<int>(rool1);
            cc.Add(cc1);

            var observableCollection2 = new ObservableCollection<int>(new List<int>() { 4, 6, 5 });
            var rool2 = new ReadOnlyObservableList<int>(observableCollection2);
            var cc2 = new CollectionContainer<int>(rool2);
            cc.Add(cc2);

            CompositeReadOnlyObservableCollectionSource<int> croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);
            observableCollection1.Move(1, 2);
            observableCollection2.Move(1, 2);

            var iterator = croocs.View;
            int i = 0;
            foreach (var v in iterator)
            {
                Assert.AreEqual(v, i);
                i++;
            }
        }

        /// <summary>
        /// Testing move method insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void MoveListWithTransformingTest()
        {
            var croocs = new CompositeReadOnlyObservableCollectionSource<int>();
            var viewCroocs = croocs.View;
            TransformingReadOnlyObservableList<int, int> trool = new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector);

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

        /// <summary>
        /// Testing several operations insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void OperationListWithTransformingTest()
        {
            var croocs = new CompositeReadOnlyObservableCollectionSource<int>();
            var viewCroocs = croocs.View;
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

            //[0 1 2 3 4 5 6]
            croocs.RemoveAt(1);
            //[0 1 2 3]
            croocs.Insert(0, cc3);
            //[7 8 9 0 1 2 3]
            croocs.Add(cc4);
            //[7 8 9 0 1 2 3 10 11 12]
            croocs.Move(0, 2);
            //[0 1 2 3 10 11 12 7 8 9]

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
        /// Testing several operations insert into TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void OperationWithTransformingTest()
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

            var croocs = new CompositeReadOnlyObservableCollectionSource<int>(cc);
            var viewCroocs = croocs.View;

            TransformingReadOnlyObservableList<int, int> trool = new TransformingReadOnlyObservableList<int, int>(viewCroocs, Selector, Delete);

            Assert.AreEqual(trool[0], 0);
            Assert.AreEqual(trool[1], 1);
            Assert.AreEqual(trool[2], 2);
            Assert.AreEqual(trool[3], 3);
            Assert.AreEqual(trool[4], 4);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 6);

            deleteItems.Clear();
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

