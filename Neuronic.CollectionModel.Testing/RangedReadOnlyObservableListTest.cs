using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class RangedReadOnlyObservableListTest
    {
        [TestMethod]
        public void TestCountAndOffset()
        {
            var values = Enumerable.Range(0, 30).ToList();

            var range = new RangedReadOnlyObservableList<int>(values.ListAsObservable(), 5, 7);
            Assert.AreEqual(7, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(5).Take(7)));

            range.MaxCount = 13;
            Assert.AreEqual(13, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(5).Take(13)));

            range.Offset = 22;
            Assert.AreEqual(8, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(22).Take(13)));

            range.MaxCount = 6;
            Assert.AreEqual(6, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(22).Take(6)));

            range.Offset = 17;
            Assert.AreEqual(6, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(17).Take(6)));

            range.MaxCount = 15;
            Assert.AreEqual(13, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(17).Take(15)));
        }

        [TestMethod]
        public void TestInsertItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values.Skip(5).Take(5));
            var range = new RangedReadOnlyObservableList<int>(source.ListAsObservable(), 5, 10);
            foreach (var i in values.Skip(15))
                source.Add(i);
            for (int i = 0; i < 5; i++)
                source.Insert(5 + i, values[10 + i]);
            for (int i = 0; i < 5; i++)
                source.Insert(i, values[i]);
            Assert.AreEqual(10, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(5).Take(10)));
        }

        [TestMethod]
        public void TestRemoveItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var range = new RangedReadOnlyObservableList<int>(source.ListAsObservable(), 3, 5);

            // Remove pair values
            for (int i = source.Count - 1; i >= 0; i--)
                if (source[i]%2 == 0)
                    source.RemoveAt(i);

            Assert.AreEqual(5, range.Count);
            Assert.IsTrue(range.SequenceEqual(source.Skip(3).Take(5)));

            // Remove multiple of 3
            for (int i = source.Count - 1; i >= 0; i--)
                if (source[i] % 3 == 0)
                    source.RemoveAt(i);

            Assert.AreEqual(4, range.Count);
            Assert.IsTrue(range.SequenceEqual(source.Skip(3).Take(5)));
        }

        [TestMethod]
        public void TestMoveItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var range = new RangedReadOnlyObservableList<int>(source.ListAsObservable(), 7, 5);

            var m = source.Count/2;
            for (int i = m + 1; i < source.Count; i++)
                source.Move(i, m);
            for (int i = m - 1; i >= 0; i--)
                source.Move(i, source.Count - 1);
            Assert.AreEqual(5, range.Count);
            values.Reverse();
            Assert.IsTrue(range.SequenceEqual(values.Skip(7).Take(5)));
        }

        [TestMethod]
        public void TestSetItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var range = new RangedReadOnlyObservableList<int>(source.ListAsObservable(), 9);

            for (int i = 0; i < source.Count; i++)
                source[i] *= 2;

            Assert.IsTrue(range.SequenceEqual(values.Skip(9).Select(i => 2*i)));
        }

        [TestMethod]
        public void TestClearItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var range = new RangedReadOnlyObservableList<int>(source.ListAsObservable(), 11);

            source.Clear();
            Assert.AreEqual(0, range.Count);
        }
    }
}