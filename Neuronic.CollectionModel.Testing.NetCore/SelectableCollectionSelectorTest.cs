using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Extras;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    /// <summary>
    /// Unit test for SelectableCollectionSelector class.
    /// </summary>
    [TestClass]
    public class SelectableCollectionSelectorTest
    {
        /// <summary>
        /// Testing SelectedIndex method and IsSelected property.
        /// </summary>
        [TestMethod]
        public void ChageSelectedItemTest()
        {
            var observableCollection = new ObservableCollection<SelItem>(new List<SelItem>() { new SelItem(1), new SelItem(2), new SelItem(3), new SelItem(4), new SelItem(5) });
            var scs = new SelectableCollectionSelector<SelItem>(observableCollection);

            scs.SelectedIndex = 2;
            Assert.AreEqual(scs.Items[0].Value, 1);
            Assert.AreEqual(scs.Items[1].Value, 2);
            Assert.AreEqual(scs.Items[2].Value, 3);
            Assert.AreEqual(scs.Items[3].Value, 4);
            Assert.AreEqual(scs.Items[4].Value, 5);
            Assert.AreEqual(scs.SelectedIndex, 2);
            Assert.AreEqual(scs.SelectedItem.Value, 3);

            Assert.IsFalse(scs.Items[0].IsSelected);
            Assert.IsFalse(scs.Items[1].IsSelected);
            Assert.IsTrue(scs.Items[2].IsSelected);
            Assert.IsFalse(scs.Items[3].IsSelected);
            Assert.IsFalse(scs.Items[4].IsSelected);
        }

        /// <summary>
        /// Testing IsSelected property.
        /// </summary>
        [TestMethod]
        public void ChageIsSelectedTest()
        {
            //var observableCollection = new ObservableCollection<SelItem>(new List<SelItem>() { new SelItem(1), new SelItem(2), new SelItem(3), new SelItem(4), new SelItem(5) });
            //var scs = new SelectableCollectionSelector<SelItem>(observableCollection);

            //scs.Items[2].IsSelected = true;
            //Assert.AreEqual(scs.SelectedIndex, 2);
        }

        ///// <summary>
        ///// Testing
        ///// </summary>
        //[TestMethod]
        //public void ItemTest()
        //{
        //    var observableCollection =
        //        new ObservableCollection<SelItem>(new List<SelItem>()
        //        {
        //            new SelItem(1),
        //            new SelItem(2),
        //            new SelItem(3),
        //            new SelItem(4),
        //            new SelItem(5)
        //        });
        //    var scs = new SelectableCollectionSelector<SelItem>(observableCollection);

        //    scs.SelectedIndex = 2;
        //}
    }

    class SelItem : ISelectableItem
    {
        private bool isSelect;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get { return isSelect; }
            set
            {
                PropertyChanged += (sender, args) => OnPropertyChanged(args);
                isSelect = value;
            }
        }

        public event EventHandler SelectionChanged;

        public SelItem(int value)
        {
            Value = value;
        }

        public int Value { get; set; }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
