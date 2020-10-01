using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [TestClass]
    public class SetDifferenceReadOnlyObservableCollectionTest
    {
        [TestMethod]
        public void TestCreation()
        {
            var items = new[] { 13, 5, 13, 6, 10, 7, 4, 19, 5, 13, 2, 6, 12, 1, 6, 14, 6, 1, 7, 15 };
            var sItems = new[] {16, 0, 1, 16, 11, 18, 2, 16, 17, 11};

            var dif = new SetDifferenceReadOnlyObservableCollection<int>(items, sItems);

            var set = new HashSet<int>(items);
            set.ExceptWith(sItems);
            Assert.IsTrue(set.SetEquals(dif));
        }

        [TestMethod]
        public void TestCreationWithCustomComparer()
        {
            var items = new[] { 2, 7, 4, 17, 1, 4, 4, 4, 18, 14, 7, 11, 0, 10, 11, 3, 15, 8, 19, 9 };
            var sItems = new[] {14, 12, 5, 5, 13, 13, 3, 17, 14, 11};

            var list = items.Select(i => new Person(i)).ToList();
            var sList = sItems.Select(i => new Person(i)).ToList();
            var dif = new SetDifferenceReadOnlyObservableCollection<Person>(list, sList, new PersonEqualityComparer());

            var set = new HashSet<Person>(list, new PersonEqualityComparer());
            set.ExceptWith(sList);
            Assert.IsTrue(set.SetEquals(dif));
        }

        [TestMethod]
        public void TestAddingItems()
        {
            var items = new[] { 24, 9, 18, 8, 5, 2, 12, 23, 11, 1, 23, 2, 16, 1, 8, 28, 16, 3, 1, 29, 14, 7, 8, 1, 14, 3, 5, 15, 20, 8 };
            var sItems = new[] {2, 10, 14, 9, 13, 29, 14, 11, 29, 24, 1, 0, 14, 26, 28};

            var list = new ObservableCollection<int>(items.Take(10));
            var sList = new ObservableCollection<int>(sItems.Take(5));
            var dif = new SetDifferenceReadOnlyObservableCollection<int>(list.ListAsObservable(),
                sList.ListAsObservable());

            foreach (var item in items.Skip(10))
                list.Add(item);

            foreach (var item in sItems.Skip(5))
                sList.Add(item);

            var set = new HashSet<int>(items);
            set.ExceptWith(sItems);
            Assert.IsTrue(set.SetEquals(dif));
        }

        [TestMethod]
        public void TestRemovingItems()
        {
            var items = new[] { 12, 8, 12, 17, 7, 13, 12, 12, 6, 8, 11, 10, 13, 16, 1, 14, 3, 11, 10, 19 };
            var sItems = new[] {3, 2, 22, 10, 20, 24, 29, 4, 18, 18, 27, 21, 20, 3, 13};
            var remove = new[] { 8, 14, 3, 18, 17 };
            var sRemove = new[] {9, 16, 11, 18, 1};

            var list = new ObservableCollection<int>(items);
            var sList = new ObservableCollection<int>(sItems);
            var dif = new SetDifferenceReadOnlyObservableCollection<int>(list.ListAsObservable(),
                sList.ListAsObservable());

            foreach (var item in remove)
                list.Remove(item);

            foreach (var item in sRemove)
                sList.Remove(item);

            var set = new HashSet<int>(list);
            set.ExceptWith(sList);
            Assert.IsTrue(set.SetEquals(dif));
        }

        [TestMethod]
        public void TestReplacingItems()
        {
            var items = new[] { 27, 12, 28, 27, 23, 29, 17, 12, 13, 17, 2, 2, 5, 28, 7, 10, 15, 5, 17, 29, 7, 2, 23, 23, 16, 12, 13, 27, 9, 28 };
            var sItems = new[] { 5, 13, 17, 3, 5, 26, 7, 7, 23, 0, 10, 1, 22, 14, 18 };
            var replace = new[] { 24, 13, 21, 28, 12 };
            var sReplace = new[] {15, 5, 18, 11, 20};

            var list = new ObservableCollection<int>(items.Skip(replace.Length));
            var sList = new ObservableCollection<int>(sItems.Skip(sReplace.Length));
            var dif = new SetDifferenceReadOnlyObservableCollection<int>(list.ListAsObservable(), sList.ListAsObservable());

            for (int i = 0; i < replace.Length; i++)
            {
                var index = list.IndexOf(replace[i]);
                if (index >= 0)
                    list[index] = items[i];
            }

            for (int i = 0; i < sReplace.Length; i++)
            {
                var index = sList.IndexOf(sReplace[i]);
                if (index >= 0)
                    sList[index] = sItems[i];
            }

            var set = new HashSet<int>(list);
            set.ExceptWith(sList);
            Assert.IsTrue(set.SetEquals(dif));
        }

        [TestMethod]
        public void TestCombiningOperations()
        {
            const int repsPerOp = 5;

            var items = new[]
            {
                33, 30, 17, 36, 26, 5, 20, 26, 11, 6, 38, 21, 27, 23, 27, 6, 27, 29, 10, 1, 37, 31, 5, 12, 18, 28, 31,
                27, 16, 38, 18, 18, 23, 35, 17, 38, 33, 2, 12, 37
            };
            var sItems = new[]
            {
                32, 1, 37, 27, 20, 3, 3, 20, 35, 11, 29, 10, 39, 38, 3, 22, 12, 22, 26, 15, 38, 4, 9, 10, 11, 21, 24,
                25, 36, 0
            };
            var remove = new[] { 2, 26, 11, 19, 16 };
            var sRemove = new[] { 16, 17, 25, 23, 4 };
            var replace = new[] { 34, 35, 14, 35, 3 };
            var sReplace = new[] { 2, 25, 4, 17, 38 };

            var list = new ObservableCollection<int>(items.Skip(2 * repsPerOp));
            var sList = new ObservableCollection<int>(sItems.Skip(2 * repsPerOp));
            var dif = new SetDifferenceReadOnlyObservableCollection<int>(list.ListAsObservable(), sList.ListAsObservable());

            for (int i = 0; i < repsPerOp; i++)
            {
                list.Remove(remove[i]);
                list.Add(items[repsPerOp + i]);
                var index = list.IndexOf(replace[i]);
                if (index >= 0)
                    list[index] = items[i];
            }

            for (int i = 0; i < repsPerOp; i++)
            {
                sList.Remove(sRemove[i]);
                sList.Add(sItems[repsPerOp + i]);
                var index = sList.IndexOf(sReplace[i]);
                if (index >= 0)
                    sList[index] = sItems[i];
            }

            var set = new HashSet<int>(list);
            set.ExceptWith(sList);
            Assert.IsTrue(set.SetEquals(dif));

            sList.Clear();

            Assert.AreEqual(dif.Count, list.Distinct().Count());

            list.Clear();

            Assert.AreEqual(0, dif.Count);
        }
    }
}
