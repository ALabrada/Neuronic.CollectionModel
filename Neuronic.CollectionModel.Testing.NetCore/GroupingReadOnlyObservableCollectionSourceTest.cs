using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [TestClass]
    public class GroupingReadOnlyObservableCollectionSourceTest
    {
        [TestMethod]
        public void TestGroupElements()
        {
            var values = new [] { 30, 74, 86, 15, 3, 43, 54, 41, 24, 2, 81, 0, 47, 32, 6, 94, 93, 28, 9, 20 };
            var oddValues = values.Where(i => i % 2 == 1).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(values,
                    new[] { oddGroup }, v => v % 2, null, null);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            groups.IncludeImplicitGroups = true;
            Assert.AreEqual(2, groups.Count);
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            var evenValues = values.Where(i => i % 2 == 0).ToList();
            var evenGroup = groups.Except(new [] {oddGroup}).First();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestExplicitAddElements()
        {
            var oddValues = Enumerable.Range(0, 10).Select(i => 2 * i + 1).ToList();
            var evenValues = Enumerable.Range(0, 10).Select(i => 2 * i).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>();
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(new ReadOnlyObservableList<int>(source),
                    new[] { oddGroup }, v => v % 2, null, null);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);

            foreach (var i in oddValues.Take(5))
                source.Add(i);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(source.Count, oddGroup.Count);
            foreach (var result in source.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            foreach (var i in oddValues.Skip(5).Concat(evenValues).OrderBy(i => i))
                source.Add(i);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestImplicitAddElements()
        {
            var oddValues = Enumerable.Range(0, 10).Select(i => 2 * i + 1).ToList();
            var evenValues = Enumerable.Range(0, 10).Select(i => 2 * i).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>();
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(new ReadOnlyObservableList<int>(source),
                        new[] { oddGroup }, v => v % 2, null, null)
                    { IncludeImplicitGroups = true };

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);

            foreach (var i in oddValues.Take(5))
                source.Add(i);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(source.Count, oddGroup.Count);
            foreach (var result in source.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            foreach (var i in oddValues.Skip(5).Concat(evenValues).OrderBy(i => i))
                source.Add(i);

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            var evenGroup = groups.Except(new[] { oddGroup }).First();
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestMoveElements()
        {
            var values = Enumerable.Range(0, 20).ToList();
            var oddValues = values.Where(i => i % 2 == 1).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                    new[] { oddGroup }, v => v % 2, null, null);

            for (int i = 1; i < source.Count; i++)
                source.Move(i, 0);
            oddValues.Reverse();

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestExplicitRemoveElements()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                    new[] { oddGroup }, v => v % 2, null, null);

            while (source.Count > 10)
                source.RemoveAt(10);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = values.Take(10).Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            while (source.Count > 0)
                source.RemoveAt(source.Count - 1);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);
        }

        [TestMethod]
        public void TestImplicitRemoveElements()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                        new[] { oddGroup }, v => v % 2, null, null)
                    { IncludeImplicitGroups = true };

            while (source.Count > 10)
                source.RemoveAt(10);

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = values.Take(10).Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            var evenValues = values.Take(10).Where(i => i % 2 == 0).ToList();
            var evenGroup = groups.Except(new[] { oddGroup }).First();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            while (source.Count > 0)
                source.RemoveAt(source.Count - 1);

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);
        }

        [TestMethod]
        public void TestExplicitClear()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                    new[] { oddGroup }, v => v % 2, null, null);
            source.Clear();

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);
        }

        [TestMethod]
        public void TestImplicitClear()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                        new[] { oddGroup }, v => v % 2, null, null)
                    { IncludeImplicitGroups = true };
            source.Clear();

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            Assert.AreEqual(0, oddGroup.Count);
        }

        [TestMethod]
        public void TestExplicitSetElements()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                    new[] { oddGroup }, v => v % 2, null, null);

            for (int i = 0; i < source.Count; i++)
                source[i] = values[i] / 2;

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = source.Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < source.Count; i++)
                source[i] = values[i];

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = source.Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestImplicitSetElements()
        {
            var values = Enumerable.Range(0, 20).ToList();

            var oddGroup = new ReadOnlyObservableGroup<int, int>(1);
            var source = new ObservableCollection<int>(values);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<int, int>(source,
                        new[] { oddGroup }, v => v % 2, null, null)
                    { IncludeImplicitGroups = true };

            for (int i = 0; i < source.Count; i++)
                source[i] = values[i] / 2;

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = source.Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            var evenGroup = groups.Except(new[] { oddGroup }).First();
            var evenValues = source.Where(i => i % 2 == 0).ToList();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < source.Count; i++)
                source[i] = values[i];

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = source.Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            Assert.IsTrue(groups.Contains(evenGroup));
            evenValues = source.Where(i => i % 2 == 0).ToList();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < source.Count; i++)
                source[i] = 2 * values[i] + 1;

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = source.Where(i => i % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestExplicitMutableElements()
        {
            var values = Enumerable.Range(0, 20).Select(i => new Notify { Prop = i }).ToList();

            var oddGroup = new ReadOnlyObservableGroup<Notify, int>(1);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<Notify, int>(values,
                    new[] { oddGroup }, n => n.Prop % 2, null, null, nameof(Notify.Prop));

            foreach (var value in values)
                value.Prop /= 2;

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = values.Where(n => n.Prop % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < values.Count; i++)
                values[i].Prop = i;

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = values.Where(n => n.Prop % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }

        [TestMethod]
        public void TestImplicitMutableElements()
        {
            var values = Enumerable.Range(0, 20).Select(i => new Notify { Prop = i }).ToList();

            var oddGroup = new ReadOnlyObservableGroup<Notify, int>(1);
            var groups =
                new GroupingReadOnlyObservableCollectionSource<Notify, int>(values,
                        new[] { oddGroup }, n => n.Prop % 2, null, null, nameof(Notify.Prop))
                    { IncludeImplicitGroups = true };

            foreach (var value in values)
                value.Prop /= 2;

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            var oddValues = values.Where(n => n.Prop % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            var evenGroup = groups.Except(new[] { oddGroup }).First();
            var evenValues = values.Where(n => n.Prop % 2 == 0).ToList();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < values.Count; i++)
                values[i].Prop = i;

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = values.Where(n => n.Prop % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
            Assert.IsTrue(groups.Contains(evenGroup));
            evenValues = values.Where(n => n.Prop % 2 == 0).ToList();
            Assert.AreEqual(evenValues.Count, evenGroup.Count);
            foreach (var result in evenValues.Zip(evenGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);

            for (int i = 0; i < values.Count; i++)
                values[i].Prop = 2 * i + 1;

            Assert.AreEqual(1, groups.Count);
            Assert.IsTrue(groups.Contains(oddGroup));
            oddValues = values.Where(n => n.Prop % 2 == 1).ToList();
            Assert.AreEqual(oddValues.Count, oddGroup.Count);
            foreach (var result in oddValues.Zip(oddGroup, (expected, actual) => new { expected, actual }))
                Assert.AreEqual(result.expected, result.actual);
        }
    }
}
