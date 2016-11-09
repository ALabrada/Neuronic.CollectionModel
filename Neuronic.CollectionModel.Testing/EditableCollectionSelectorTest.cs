using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for EditableCollectionSelector class.
    /// </summary>
    [TestClass]
    public class EditableCollectionSelectorTest
    {
        List<object> deleteItems = new List<object>();

        /// <summary>
        /// Testing Add and AddAndSelect method.
        /// </summary>
        [TestMethod]
        public void AddItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var cs = new EditableCollectionSelector<int>(observableCollection);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            cs.Add(7);
            Assert.AreEqual(cs.Items[6], 7);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            //[1 2 3 4 5 6 7]
            //SelectedIndex = 2
            //SelectedItem = 3

            cs.AddAndSelect(8);
            Assert.AreEqual(cs.Items[7], 8);
            Assert.AreEqual(cs.SelectedIndex, 7);
            Assert.AreEqual(cs.SelectedItem, 8);

            //[1 2 3 4 5 6 7 8]
            //SelectedIndex = 7
            //SelectedItem = 8
        }

        /// <summary>
        /// Testing Remove and RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var cs = new EditableCollectionSelector<int>(observableCollection);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            cs.Remove(4);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 3);
            Assert.AreEqual(cs.Items[3], 5);
            Assert.AreEqual(cs.Items[4], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            //[1 2 3 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            cs.RemoveAt(2);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 5);
            Assert.AreEqual(cs.Items[3], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 5);

            //[1 2 5 6]
            //SelectedIndex = 2
            //SelectedItem = 5

            cs.RemoveSelected();
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 6);

            //[1 2 6]
            //SelectedIndex = 2
            //SelectedItem = 6

            cs.RemoveSelected();
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.SelectedIndex, 1);
            Assert.AreEqual(cs.SelectedItem, 2);

            //[1 2]
            //SelectedIndex = 1
            //SelectedItem = 2
        }

        /// <summary>
        /// Testing Replace and ReplaceAndSelect method.
        /// </summary>
        [TestMethod]
        public void ReplaceItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var cs = new EditableCollectionSelector<int>(observableCollection);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            cs.Replace(3, 7);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 7);
            Assert.AreEqual(cs.Items[3], 4);
            Assert.AreEqual(cs.Items[4], 5);
            Assert.AreEqual(cs.Items[5], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 7);

            //[1 2 7 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 7

            bool rep = cs.ReplaceAndSelect(7, 3);
            Assert.IsTrue(rep);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 3);
            Assert.AreEqual(cs.Items[3], 4);
            Assert.AreEqual(cs.Items[4], 5);
            Assert.AreEqual(cs.Items[5], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3
        }

        /// <summary>
        /// Testing Insert and InsertAndSelect method.
        /// </summary>
        [TestMethod]
        public void InsertItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var cs = new EditableCollectionSelector<int>(observableCollection);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            cs.Insert(3, 7);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 3);
            Assert.AreEqual(cs.Items[3], 7);
            Assert.AreEqual(cs.Items[4], 4);
            Assert.AreEqual(cs.Items[5], 5);
            Assert.AreEqual(cs.Items[6], 6);
            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            //[1 2 3 7 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            cs.InsertAndSelect(5, 8);
            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 3);
            Assert.AreEqual(cs.Items[3], 7);
            Assert.AreEqual(cs.Items[4], 4);
            Assert.AreEqual(cs.Items[5], 8);
            Assert.AreEqual(cs.Items[6], 5);
            Assert.AreEqual(cs.Items[7], 6);
            Assert.AreEqual(cs.SelectedIndex, 5);
            Assert.AreEqual(cs.SelectedItem, 8);

            //[1 2 3 7 4 8 5 6]
            //SelectedIndex = 5
            //SelectedItem = 8
        }

        /// <summary>
        /// Testing SelectedIndex and SelectedItem method.
        /// </summary>
        [TestMethod]
        public void SelectedMethodTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var cs = new EditableCollectionSelector<int>(observableCollection);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);

            cs.Select(4);
            Assert.AreEqual(cs.SelectedIndex, 3);
            Assert.AreEqual(cs.SelectedItem, 4);

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3
        }
    }
}

