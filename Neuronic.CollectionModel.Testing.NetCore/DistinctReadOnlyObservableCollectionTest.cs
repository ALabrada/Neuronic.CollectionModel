using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [TestClass]
    public class DistinctReadOnlyObservableCollectionTest
    {
        [TestMethod]
        public void TestCreation()
        {
            var items = new[] {8, 2, 10, 14, 15, 4, 10, 18, 12, 6, 6, 1, 10, 13, 14, 16, 14, 17, 18, 4};

            var list = items.ListAsObservable();
            var distinct = new DistinctReadOnlyObservableCollection<int>(list);

            var set = new HashSet<int>(items);
            Assert.IsTrue(set.SetEquals(distinct));
        }

        [TestMethod]
        public void TestCreationWithCustomComparer()
        {
            var items = new[] { 10, 8, 13, 2, 3, 17, 8, 3, 6, 6, 2, 5, 13, 14, 6, 13, 1, 12, 6, 17 };

            var list = items.Select(i => new Person(i)).ToList();
            var distinct = new DistinctReadOnlyObservableCollection<Person>(list.ListAsObservable(), new PersonEqualityComparer());

            var set = new HashSet<Person>(list, new PersonEqualityComparer());
            Assert.IsTrue(set.SetEquals(distinct));
        }

        [TestMethod]
        public void TestAddingItems()
        {
            var items = new[] { 24, 9, 18, 8, 5, 2, 12, 23, 11, 1, 23, 2, 16, 1, 8, 28, 16, 3, 1, 29, 14, 7, 8, 1, 14, 3, 5, 15, 20, 8 };

            var list = new ObservableCollection<int>(items.Take(10));
            var distinct = new DistinctReadOnlyObservableCollection<int>(list.ListAsObservable());

            foreach (var item in items.Skip(10))
                list.Add(item);

            var set = new HashSet<int>(items);
            Assert.IsTrue(set.SetEquals(distinct));
        }

        [TestMethod]
        public void TestRemovingItems()
        {
            var items = new[] { 3, 13, 1, 7, 1, 13, 19, 11, 7, 0, 14, 11, 15, 16, 10, 15, 9, 0, 17, 1 };
            var remove = new[] {9, 7, 6, 11, 9};

            var list = new ObservableCollection<int>(items);
            var distinct = new DistinctReadOnlyObservableCollection<int>(list.ListAsObservable());

            foreach (var item in remove)
                list.Remove(item);

            var set = new HashSet<int>(list);
            Assert.IsTrue(set.SetEquals(distinct));
        }

        [TestMethod]
        public void TestReplacingItems()
        {
            var items = new [] { 16, 0, 16, 23, 2, 26, 27, 3, 2, 26, 2, 0, 4, 28, 18, 28, 28, 22, 1, 7, 27, 14, 13, 16, 20, 20, 5, 18, 17, 24 };
            var replace = new [] { 24, 13, 21, 28, 12 };

            var list = new ObservableCollection<int>(items.Skip(replace.Length));
            var distinct = new DistinctReadOnlyObservableCollection<int>(list.ListAsObservable());

            for (int i = 0; i < replace.Length; i++)
            {
                var index = list.IndexOf(replace[i]);
                if (index >= 0)
                    list[index] = items[i];
            }

            var set = new HashSet<int>(list);
            Assert.IsTrue(set.SetEquals(distinct));
        }

        [TestMethod]
        public void TestCombiningOperations()
        {
            const int repsPerOp = 5;

            var items = new [] { 13, 23, 14, 4, 28, 11, 3, 10, 12, 9,
                5, 1, 5, 10, 13, 2, 14, 20, 26, 13, 15, 28, 13, 26, 19, 6, 2, 18, 9, 20 };
            var remove = new[] {10, 0, 15, 13, 14 };
            var replace = new[] {9, 1, 14, 26, 11};

            var list = new ObservableCollection<int>(items.Skip(2 * repsPerOp));
            var distinct = new DistinctReadOnlyObservableCollection<int>(list.ListAsObservable());

            for (int i = 0; i < repsPerOp; i++)
            {
                list.Remove(remove[i]);
                list.Add(items[repsPerOp + i]);
                var index = list.IndexOf(replace[i]);
                if (index >= 0)
                    list[index] = items[i];
            }

            var set = new HashSet<int>(list);
            Assert.IsTrue(set.SetEquals(distinct));

            list.Clear();

            Assert.AreEqual(0, distinct.Count);
        }
    }
}
