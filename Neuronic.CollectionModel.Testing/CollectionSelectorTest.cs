using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuronic.CollectionModel.Testing
{
    /// <summary>
    /// Unit test for CompositeReadOnlyObservableListSource class.
    /// </summary>
    [TestClass]
    public class CollectionSelectorTest
    {
        private List<object> deleteItems = new List<object>();
        private List<int> changeItems = new List<int>();
        private List<int> changingItems = new List<int>();

        /// <summary>
        /// Testing methods SelectedIndex and SelectedItem.
        /// </summary>
        [TestMethod]
        public void SelectedIndexAndItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);
            var cs = new CollectionSelector<int>(rool);
            cs.SelectedItemChanged += Load1;
            cs.SelectedItemChanging += Load2;
            cs.SelectedIndex = 1;
            Assert.AreEqual(cs.SelectedItem, 2);
            Assert.AreEqual(changingItems.Count, 1);
            Assert.AreEqual(changingItems[0], -1);
            Assert.AreEqual(changeItems.Count, 1);
            Assert.AreEqual(changeItems[0], 1);
        }

        /// <summary>
        /// Testing methods Remove and Move.
        /// </summary>
        [TestMethod]
        public void DeleteAndMoveSelectedItemTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);
            var cs = new CollectionSelector<int>(rool);

            cs.SelectedIndex = 1;
            Assert.AreEqual(cs.Items.Count, 6);

            //[1 2 3 4 5 6]
            //SelectedIndex = 1
            //SelectedItem = 2

            observableCollection.Remove(2);
            Assert.AreEqual(cs.SelectedIndex, 1);
            Assert.AreEqual(cs.SelectedItem, 3);
            Assert.AreEqual(cs.Items.Count, 5);

            //[1 3 4 5 6]
            //SelectedIndex = 1
            //SelectedItem = 3

            observableCollection.Move(1, 3);
            Assert.AreEqual(cs.SelectedIndex, 3);
            Assert.AreEqual(cs.SelectedItem, 3);
            Assert.AreEqual(cs.Items.Count, 5);

            //[1 4 5 3 6]
            //SelectedIndex = 3
            //SelectedItem = 3
        }

        /// <summary>
        /// Testing Add method.
        /// </summary>
        [TestMethod]
        public void AddElementTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);
            var cs = new CollectionSelector<int>(rool);

            cs.SelectedIndex = 1;
            Assert.AreEqual(cs.SelectedIndex, 1);
            Assert.AreEqual(cs.SelectedItem, 2);
            Assert.AreEqual(cs.Items.Count, 6);

            //[1 2 3 4 5 6]
            //SelectedIndex = 1
            //SelectedItem = 2

            observableCollection.Add(7);
            Assert.AreEqual(cs.Items.Count, 7);

            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 3);
            Assert.AreEqual(cs.Items[3], 4);
            Assert.AreEqual(cs.Items[4], 5);
            Assert.AreEqual(cs.Items[5], 6);
            Assert.AreEqual(cs.Items[6], 7);

            Assert.AreEqual(cs.SelectedIndex, 1);
            Assert.AreEqual(cs.SelectedItem, 2);

            observableCollection.Clear();
            Assert.AreEqual(cs.SelectedIndex, -1);
            Assert.AreEqual(cs.SelectedItem, 0);
        }

        /// <summary>
        /// Testing Insert method.
        /// </summary>
        [TestMethod]
        public void InsertElementTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);
            var cs = new CollectionSelector<int>(rool);

            cs.SelectedIndex = 2;

            //[1 2 3 4 5 6]
            //SelectedIndex = 2
            //SelectedItem = 3

            Assert.AreEqual(cs.SelectedIndex, 2);
            Assert.AreEqual(cs.SelectedItem, 3);
            Assert.AreEqual(cs.Items.Count, 6);

            observableCollection.Insert(2, 7);
            Assert.AreEqual(cs.Items.Count, 7);

            //[1 2 7 3 4 5 6]
            //SelectedIndex = 3
            //SelectedItem = 3

            Assert.AreEqual(cs.Items[0], 1);
            Assert.AreEqual(cs.Items[1], 2);
            Assert.AreEqual(cs.Items[2], 7);
            Assert.AreEqual(cs.Items[3], 3);
            Assert.AreEqual(cs.Items[4], 4);
            Assert.AreEqual(cs.Items[5], 5);
            Assert.AreEqual(cs.Items[6], 6);

            Assert.AreEqual(cs.SelectedIndex, 3);
            Assert.AreEqual(cs.SelectedItem, 3);
        }

        private CollectionSelector<int> Copy(CollectionSelector<int> cs1)
        {
            var observableCollection = new ObservableCollection<int>();

            for (int i = 0; i < cs1.Items.Count; i++)
            {
                observableCollection.Add(cs1.Items[i]);
            }

            var rool = new ReadOnlyObservableList<int>(observableCollection);
            CollectionSelector<int> cs = new CollectionSelector<int>(rool);

            var ss = cs1.SelectedItem;

            for (int i = 0; i < cs.Items.Count; i++)
            {
                if (cs.Items[i] == ss)
                {
                    cs.SelectedIndex = i;
                }
            }

            return cs;
        }

        private void Load1(object sender, EventArgs e)
        {
            var cs = (CollectionSelector<int>)sender;
            var cs1 = Copy(cs);
            changeItems.Add(cs1.SelectedIndex);
        }

        private void Load2(object sender, EventArgs e)
        {
            var cs = (CollectionSelector<int>)sender;
            var cs1 = Copy(cs);
            changingItems.Add(cs1.SelectedIndex);
        }
    }
}

