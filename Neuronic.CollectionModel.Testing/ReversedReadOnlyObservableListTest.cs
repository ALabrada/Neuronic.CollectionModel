using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class ReversedReadOnlyObservableListTest
    {
        [TestMethod]
        public void TestIterator()
        {
            var values = Enumerable.Range(0, 30).ToList();

            var rrool = new ReversedReadOnlyObservableList<int>(values.ListAsObservable());
            Assert.AreEqual(30, rrool.Count);
            Assert.IsTrue(rrool.SequenceEqual(values.AsEnumerable().Reverse()));
        }

        [TestMethod]
        public void TestInsertItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values.Skip(5).Take(5));
            var rrool = new ReversedReadOnlyObservableList<int>(source.ListAsObservable());
            var copy = rrool.ListSelect(x => x);

            foreach (var i in values.Skip(15))
                source.Add(i);
            for (int i = 0; i < 5; i++)
                source.Insert(5 + i, values[10 + i]);
            for (int i = 0; i < 5; i++)
                source.Insert(i, values[i]);
            Assert.AreEqual(20, rrool.Count);
            Assert.IsTrue(rrool.SequenceEqual(values.AsEnumerable().Reverse()));
            Assert.IsTrue(copy.SequenceEqual(values.AsEnumerable().Reverse()));
        }

        [TestMethod]
        public void TestRemoveItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var rrool = new ReversedReadOnlyObservableList<int>(source.ListAsObservable());
            var copy = rrool.ListSelect(x => x);

            // Remove pair values
            for (int i = source.Count - 1; i >= 0; i--)
                if (source[i] % 2 == 0)
                    source.RemoveAt(i);

            Assert.AreEqual(10, rrool.Count);
            Assert.IsTrue(rrool.SequenceEqual(source.AsEnumerable().Reverse()));
            Assert.IsTrue(copy.SequenceEqual(source.AsEnumerable().Reverse()));

            // Remove multiple of 3
            for (int i = source.Count - 1; i >= 0; i--)
                if (source[i] % 3 == 0)
                    source.RemoveAt(i);

            Assert.AreEqual(7, rrool.Count);
            Assert.IsTrue(rrool.SequenceEqual(source.AsEnumerable().Reverse()));
            Assert.IsTrue(copy.SequenceEqual(source.AsEnumerable().Reverse()));
        }

        [TestMethod]
        public void TestMoveItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var rrool = new ReversedReadOnlyObservableList<int>(source.ListAsObservable());
            var copy = rrool.ListSelect(x => x);

            var m = source.Count / 2;
            for (int i = m + 1; i < source.Count; i++)
                source.Move(i, m);
            for (int i = m - 1; i >= 0; i--)
                source.Move(i, source.Count - 1);
            Assert.AreEqual(20, rrool.Count);
            values.Reverse();
            Assert.IsTrue(rrool.SequenceEqual(values.AsReadOnly().Reverse()));
            Assert.IsTrue(copy.SequenceEqual(values.AsEnumerable().Reverse()));
        }

        [TestMethod]
        public void TestSetItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var rrool = new ReversedReadOnlyObservableList<int>(source.ListAsObservable());
            var copy = rrool.ListSelect(x => x);

            for (int i = 0; i < source.Count; i++)
                source[i] *= 2;

            Assert.IsTrue(rrool.SequenceEqual(values.Select(i => 2 * i).Reverse()));
            Assert.IsTrue(copy.SequenceEqual(values.Select(i => 2 * i).Reverse()));
        }

        [TestMethod]
        public void TestClearItems()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var source = new ObservableCollection<int>(values);
            var rrool = new ReversedReadOnlyObservableList<int>(source.ListAsObservable());
            var copy = rrool.ListSelect(x => x);

            source.Clear();
            Assert.AreEqual(0, rrool.Count);
            Assert.AreEqual(0, copy.Count);
        }
    }
}
