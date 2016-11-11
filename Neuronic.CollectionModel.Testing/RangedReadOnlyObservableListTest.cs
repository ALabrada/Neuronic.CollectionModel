using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class RangedReadOnlyObservableListTest
    {
        [TestMethod]
        public void TestCountAndOffset()
        {
            var values = Enumerable.Range(0, 30).ToList();

            var range = new RangedReadOnlyObservableList<int>(values, 5, 7);
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
            var range = new RangedReadOnlyObservableList<int>(source, 5, 10);
            foreach (var i in values.Skip(15))
                source.Add(i);
            for (int i = 0; i < 5; i++)
                source.Insert(5 + i, values[10 + i]);
            for (int i = 0; i < 5; i++)
                source.Insert(i, values[i]);
            Assert.AreEqual(10, range.Count);
            Assert.IsTrue(range.SequenceEqual(values.Skip(5).Take(10)));
        }
    }
}