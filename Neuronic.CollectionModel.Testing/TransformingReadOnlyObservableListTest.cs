using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for TransformingReadOnlyObservableList class.
    /// </summary>
    [TestClass]
    public class TransformingReadOnlyObservableListTest
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
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector);

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
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector);

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
        }

        /// <summary>
        /// Testing Insert method.
        /// </summary>
        [TestMethod]
        public void InsertElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector);

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
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void MoveElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector);

            Assert.IsTrue(trool.Count() == 7);
            int posOld = 0;
            int posNew = 4;
            observableCollection.Move(posOld, posNew);
            Assert.AreEqual(trool[posNew], 1);
        }

        /// <summary>
        /// Testing Clear method.
        /// </summary>
        [TestMethod]
        public void ClearTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector);

            observableCollection.Clear();
            Assert.AreEqual(trool.Count, 0);
        }

        /// <summary>
        /// Testing replace of one element using indexer class.
        /// </summary>
        [TestMethod]
        public void ReplaceElementTest()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector, Delete);

            deleteItems.Clear();

            Assert.IsTrue(trool.Count() == 7);
            int pos = 0;
            observableCollection[pos] = 9.9;
            Assert.AreEqual(trool[pos], 9);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 1);
        }

        /// <summary>
        /// Testing Remove method two consecutive times.
        /// </summary>
        [TestMethod]
        public void OnRemoveElementTest1()
        {
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector, Delete);

            Assert.IsTrue(observableCollection.Remove(2.2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            Assert.IsTrue(observableCollection.Remove(5.5));
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
            var observableCollection = new ObservableCollection<double>(new List<double>() { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7 });
            IReadOnlyObservableCollection<double> source = new ReadOnlyObservableList<double>(observableCollection);
            TransformingReadOnlyObservableList<double, int> trool = new TransformingReadOnlyObservableList<double, int>(source, Selector, Delete);

            Assert.IsTrue(observableCollection.Remove(2.2));
            Assert.AreEqual(trool.Count, 6);
            Assert.AreEqual(deleteItems.Count, 1);
            Assert.AreEqual(deleteItems[0], 2);

            observableCollection.RemoveAt(3);
            Assert.AreEqual(trool.Count, 5);
            Assert.AreEqual(deleteItems.Count, 2);
            Assert.AreEqual(deleteItems[1], 5);
            Assert.AreEqual(trool[1], 3);
        }

        private int Selector(double item)
        {
            return (int)item;
        }

        private void Delete(int item)
        {
            deleteItems.Add(item);
        }
    }
}
