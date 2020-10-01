using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Results;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [TestClass]
    public class ContainsObservableResultTest
    {
        [TestMethod]
        public void TestCreation()
        {
            var items = new[] {18, 16, 15, 5, 9, 9, 8, 14, 19, 5, 6, 3, 13, 6, 5, 19, 5, 12, 7, 15};

            Assert.AreEqual(true, new ContainsObservableResult<int>(items.ListAsObservable(), 5).CurrentValue);
            Assert.AreEqual(false, new ContainsObservableResult<int>(items.ListAsObservable(), 20).CurrentValue);
        }

        [TestMethod]
        public void TestCreationCustomComparer()
        {
            var items = new[] { 18, 16, 15, 5, 9, 9, 8, 14, 19, 5, 6, 3, 13, 6, 5, 19, 5, 12, 7, 15 };

            var comparer = new PersonEqualityComparer();

            Assert.AreEqual(true,
                new ContainsObservableResult<Person>(items.Select(i => new Person(i)).ListAsObservable(), new Person(5),
                    comparer).CurrentValue);
            Assert.AreEqual(false,
                new ContainsObservableResult<Person>(items.Select(i => new Person(i)).ListAsObservable(),
                    new Person(20), comparer).CurrentValue);
        }

        [TestMethod]
        public void TestAdding()
        {
            var items = new[] { 7, 8, 17, 4, 19, 10, 5, 1, 4, 1, 14, 3, 9, 19, 9, 19, 18, 2, 19, 2 };

            var list = new ObservableCollection<int>();
            var contained = new ContainsObservableResult<int>(list.ListAsObservable(), 14);
            var notContained = new ContainsObservableResult<int>(list.ListAsObservable(), 20);

            Assert.AreEqual(false, contained.CurrentValue);
            Assert.AreEqual(false, notContained.CurrentValue);

            foreach (var item in items)
                list.Add(item);

            Assert.AreEqual(true, contained.CurrentValue);
            Assert.AreEqual(false, notContained.CurrentValue);
        }

        [TestMethod]
        public void TestRemoving()
        {
            var items = new[] {4, 13, 2, 6, 0, 0, 2, 15, 18, 7, 14, 19, 18, 13, 16, 9, 19, 11, 5, 19};
            var remove = new[] {3, 7, 9, 17, 11, 14, 13, 2};

            var list = new ObservableCollection<int>(items);
            var results = Enumerable.Range(0, 21).Select(i => new ContainsObservableResult<int>(list.ListAsObservable(), i)).ToList();

            foreach (var index in remove)
                list.Remove(items[index]);

            foreach (var result in results)
                Assert.AreEqual(list.Contains(result.Value), result.CurrentValue);
        }

        [TestMethod]
        public void TestReplacing()
        {
            var items = new[] { 1, 3, 16, 5, 14, 18, 7, 13, 4, 24, 10, 13, 3, 22, 8, 19, 6, 19, 4, 24, 0, 8, 21, 24, 23 };
            var replace = new[] { 1, 3, 11, 15, 10, 17, 6 };

            var list = new ObservableCollection<int>(items.Take(items.Length - replace.Length));
            var results = Enumerable.Range(0, 26).Select(i => new ContainsObservableResult<int>(list.ListAsObservable(), i)).ToList();

            for (int i = 0; i < replace.Length; i++)
                list[replace[i]] = items[items.Length - replace.Length - 1 + i];

            foreach (var result in results)
                Assert.AreEqual(list.Contains(result.Value), result.CurrentValue);
        }
    }
}

