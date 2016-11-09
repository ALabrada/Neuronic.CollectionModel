using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for SwitchableListSource class.
    /// </summary>
    [TestClass]
    public class SwitchableListSourceTest
    {
        List<object> deleteItems = new List<object>();

        /// <summary>
        /// Testing change source of the SwitchableListSource.
        /// </summary>
        [TestMethod]
        public void ChangeSourceTest()
        {
            var observableCollection1 = new ObservableCollection<string>(new List<string>() { "a", "b", "c", "d", "e" });
            var rool1 = new ReadOnlyObservableList<string>(observableCollection1);
            var sls = new SwitchableListSource<object>();
            sls.Source = rool1;

            Assert.AreEqual(sls.Count, 5);
            Assert.AreEqual(sls[0], "a");
            Assert.AreEqual(sls[1], "b");
            Assert.AreEqual(sls[2], "c");
            Assert.AreEqual(sls[3], "d");
            Assert.AreEqual(sls[4], "e");

            var observableCollection2 = new ObservableCollection<object>(new List<object>() { 8, 9, 10, 11 });
            IReadOnlyObservableList<object> rool2 = new ReadOnlyObservableList<object>(observableCollection2);
            sls.Source = rool2;

            Assert.AreEqual(sls.Count, 4);
            Assert.AreEqual(sls[0], 8);
            Assert.AreEqual(sls[1], 9);
            Assert.AreEqual(sls[2], 10);
            Assert.AreEqual(sls[3], 11);

            sls.Source = null;
            Assert.AreEqual(sls.Count, 0);
        }

        /// <summary>
        /// Testing Add method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool = new TransformingReadOnlyObservableList<object, object>(sls, Selector);

            observableCollection.Add(8.8);
            Assert.AreEqual(trool[trool.Count - 1], 8.8);
        }

        /// <summary>
        /// Testing Insert method.
        /// </summary>
        [TestMethod]
        public void InsertElementTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool = new TransformingReadOnlyObservableList<object, object>(sls, Selector);

            observableCollection.Insert(3, 8);

            Assert.AreEqual(trool.Count, 8);
            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 2);
            Assert.AreEqual(trool[2], 3);
            Assert.AreEqual(trool[3], 8);
            Assert.AreEqual(trool[4], 4);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 6);
            Assert.AreEqual(trool[7], 7);
        }

        /// <summary>
        /// Testing Move method two consecutive times.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool = new TransformingReadOnlyObservableList<object, object>(sls, Selector);

            observableCollection.Move(1, 5);
            observableCollection.Move(3, 6);

            Assert.AreEqual(trool.Count, 7);
            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 3);
            Assert.AreEqual(trool[2], 4);
            Assert.AreEqual(trool[3], 6);
            Assert.AreEqual(trool[4], 2);
            Assert.AreEqual(trool[5], 7);
            Assert.AreEqual(trool[6], 5);
        }

        /// <summary>
        /// Testing Clear method.
        /// </summary>
        [TestMethod]
        public void ClearTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool =
                new TransformingReadOnlyObservableList<object, object>(sls, Selector);

            observableCollection.Clear();
            Assert.AreEqual(trool.Count, 0);
        }

        /// <summary>
        /// Testing replace of two elements using indexer class and whith TransformingReadOnlyObservableList class.
        /// </summary>
        [TestMethod]
        public void ReplaceElementTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool =
                new TransformingReadOnlyObservableList<object, object>(sls, Selector);

            observableCollection[2] = 8;
            observableCollection[5] = 9;

            Assert.AreEqual(trool.Count, 7);
            Assert.AreEqual(trool[0], 1);
            Assert.AreEqual(trool[1], 2);
            Assert.AreEqual(trool[2], 8);
            Assert.AreEqual(trool[3], 4);
            Assert.AreEqual(trool[4], 5);
            Assert.AreEqual(trool[5], 9);
            Assert.AreEqual(trool[6], 7);
        }

        /// <summary>
        /// Testing Remove method two consecutive times whith TransformingReadOnlyObservableList.
        /// </summary>
        [TestMethod]
        public void OnRemoveElementTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool =
                new TransformingReadOnlyObservableList<object, object>(sls, Selector, Delete);

            Assert.IsTrue(observableCollection.Remove(2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            Assert.IsTrue(observableCollection.Remove(5));
            Assert.AreEqual(trool.Count, 5);
            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[1], 5);
            Assert.AreEqual(trool[1], 3);
        }

        /// <summary>
        /// Testing Remove method two consecutive times.
        /// </summary>
        [TestMethod]
        public void OnRemoveElementTest2()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool =
                new TransformingReadOnlyObservableList<object, object>(sls, Selector, Delete);

            Assert.IsTrue(observableCollection.Remove(2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            observableCollection.RemoveAt(3);
            Assert.AreEqual(trool.Count, 5);
            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[1], 5);
            Assert.AreEqual(trool[1], 3);
        }

        /// <summary>
        /// Testing several operations with TransformingReadOnlyObservableList class: Insert, Add, RemoveAt and Move method.
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<object>(new List<object>() { 1, 2, 3, 4, 5, 6, 7 });
            IReadOnlyObservableList<object> rool = new ReadOnlyObservableList<object>(observableCollection);
            SwitchableListSource<object> sls = new SwitchableListSource<object>();
            sls.Source = rool;
            TransformingReadOnlyObservableList<object, object> trool = new TransformingReadOnlyObservableList<object, object>(sls, Selector, Delete);

            deleteItems.Clear();

            //[1 2 3 4 5 6 7]
            observableCollection.Insert(3, 8);
            //[1 2 3 8 4 5 6 7]
            observableCollection.Add(9);
            //[1 2 3 8 4 5 6 7 9]
            observableCollection.RemoveAt(0);
            //[2 3 8 4 5 6 7 9]
            observableCollection[2] = 10;
            //[2 3 10 4 5 6 7 9]
            observableCollection.Move(4, 5);
            //[2 3 10 4 6 5 7 9]

            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[0], 1);
            Assert.AreEqual(deleteItems[1], 8);

            Assert.AreEqual(trool.Count, 8);
            Assert.AreEqual(trool[0], 2);
            Assert.AreEqual(trool[1], 3);
            Assert.AreEqual(trool[2], 10);
            Assert.AreEqual(trool[3], 4);
            Assert.AreEqual(trool[4], 6);
            Assert.AreEqual(trool[5], 5);
            Assert.AreEqual(trool[6], 7);
            Assert.AreEqual(trool[7], 9);
        }

        private object Selector(object item)
        {
            return item;
        }

        private void Delete(object item)
        {
            deleteItems.Add(item);
        }
    }

}

