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
    public class PrefixReadOnlyObservableListTest
    {
        [TestMethod]
        public void IteratorTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var prool = new PrefixReadOnlyObservableList<int>(observableCollection, i => i <= 5);

            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 5).SequenceEqual(prool));
        }

        /// <summary>
        /// Testing Add method several times and Inset method.
        /// </summary>
        [TestMethod]
        public void AddItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection.Add(2);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Insert(3, 11);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Insert(3, 5);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Insert(0, 0);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Remove and RemoveAt method.
        /// </summary>
        [TestMethod]
        public void DeleteItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 11, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection.RemoveAt(9);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Remove(11);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.RemoveAt(8);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.RemoveAt(1);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void MoveItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection.Move(3, 0);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Move(10, 5);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Move(9, 6);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Move(5, 10);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing Move method.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection[5] = 11;
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection[8] = 12;
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection[5] = 12;
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection[1] = 4;
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing several operations: Remove, Move, Add, Inser and RemoveAt method and indexer of class.
        /// </summary>
        [TestMethod]
        public void OperationsTest()
        {
            var observableCollection = new ObservableCollection<int>(new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 });
            var rool = new ReadOnlyObservableList<int>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<int>(rool, i => i <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection.Remove(4);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Move(6, 13);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Add(22);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.Insert(3, 4);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection.RemoveAt(20);
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));

            observableCollection[0] = 24;
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(i => i <= 10).SequenceEqual(copy));
        }

        /// <summary>
        /// Testing PropertyChanged event.
        /// </summary>
        [TestMethod]
        public void TriggerTest()
        {
            var observableCollection = new ObservableCollection<Notify>(new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
                .Select(i => new Notify {Prop =  i}).ToList());
            var rool = new ReadOnlyObservableList<Notify>(observableCollection);

            var prool = new PrefixReadOnlyObservableList<Notify>(rool, n => n.Prop <= 10);
            var copy = prool.ListSelect(x => x);

            observableCollection[5].Prop = 11;
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(copy));

            observableCollection[8].Prop = 12;
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(copy));

            observableCollection[5].Prop = 12;
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(copy));

            observableCollection[1].Prop = 4;
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(prool));
            Assert.IsTrue(observableCollection.TakeWhile(n => n.Prop <= 10).SequenceEqual(copy));
        }
    }
}
