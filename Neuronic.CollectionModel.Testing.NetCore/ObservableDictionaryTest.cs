using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    /// <summary>
    /// Summary description for ObservableDictionaryTest
    /// </summary>
    [TestClass]
    public class ObservableDictionaryTest
    {
        public ObservableDictionaryTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestAddingItems()
        {
            var items = new[] {79, 88, 8, 61, 40, 87, 27, 18, 59, 12};
            var dict = new ObservableDictionary<int, double>();
            var keys = dict.Keys;
            var values = keys.ListSelect(i => 1d / i);

            Assert.AreEqual(0, dict.Count);
            Assert.AreEqual(0, keys.Count);
            Assert.AreEqual(0, values.Count);

            foreach (var value in items)
                dict.Add(value, 1d / value);

            Assert.AreEqual(items.Length, dict.Count);
            Assert.AreEqual(items.Length, keys.Count);
            Assert.AreEqual(items.Length, values.Count);

            var keySet = new HashSet<int>(items);
            var valueSet = new HashSet<double>(items.Select(i => 1d / i));
            var pairSet =
                new HashSet<KeyValuePair<int, double>>(items.Select(i => new KeyValuePair<int, double>(i, 1d / i)), new PairComparer<int, double>());

            Assert.IsTrue(pairSet.SetEquals(dict));
            Assert.IsTrue(keySet.SetEquals(keys));
            Assert.IsTrue(valueSet.SetEquals(dict.Values));
            Assert.IsTrue(valueSet.SetEquals(values));
        }

        [TestMethod]
        public void TestAddingItemsThroughIndexer()
        {
            var items = new[] { 87, 17, 5, 26, 96, 76, 54, 23, 38, 35 };
            var dict = new ObservableDictionary<int, double>();
            var keys = dict.Keys;
            var values = keys.ListSelect(i => 1d / i);

            Assert.AreEqual(0, dict.Count);
            Assert.AreEqual(0, keys.Count);
            Assert.AreEqual(0, values.Count);

            foreach (var value in items)
                dict[value] = 1d / value;

            Assert.AreEqual(items.Length, dict.Count);
            Assert.AreEqual(items.Length, keys.Count);
            Assert.AreEqual(items.Length, values.Count);

            var keySet = new HashSet<int>(items);
            var valueSet = new HashSet<double>(items.Select(i => 1d / i));
            var pairSet =
                new HashSet<KeyValuePair<int, double>>(items.Select(i => new KeyValuePair<int, double>(i, 1d / i)), new PairComparer<int, double>());

            Assert.IsTrue(pairSet.SetEquals(dict));
            Assert.IsTrue(keySet.SetEquals(keys));
            Assert.IsTrue(valueSet.SetEquals(dict.Values));
            Assert.IsTrue(valueSet.SetEquals(values));
        }

        [TestMethod]
        public void TestRemovingItems()
        {
            var items = new[] { 11, 96, 10, 16, 39, 5, 47, 88, 25, 58, 93, 43, 28, 30 };
            var indexes = new[] {7, 9, 4, 12, 8, 2, 1, 11};

            var dict = new ObservableDictionary<int, double>(items.ToDictionary(i => i, i => 1d / i));
            var keys = dict.Keys;
            var values = keys.ListSelect(i => 1d / i);

            Assert.AreEqual(items.Length, dict.Count);
            Assert.AreEqual(items.Length, keys.Count);
            Assert.AreEqual(items.Length, values.Count);

            foreach (var i in indexes)
                Assert.IsTrue(dict.Remove(items[i]));

            Assert.AreEqual(items.Length - indexes.Length, dict.Count);
            Assert.AreEqual(items.Length - indexes.Length, keys.Count);
            Assert.AreEqual(items.Length - indexes.Length, values.Count);

            var keySet = new HashSet<int>(items);
            keySet.ExceptWith(indexes.Select(i => items[i]));
            var valueSet = new HashSet<double>(keySet.Select(i => 1d / i));
            var pairSet =
                new HashSet<KeyValuePair<int, double>>(keySet.Select(i => new KeyValuePair<int, double>(i, 1d / i)), new PairComparer<int, double>());

            Assert.IsTrue(pairSet.SetEquals(dict));
            Assert.IsTrue(keySet.SetEquals(keys));
            Assert.IsTrue(valueSet.SetEquals(dict.Values));
            Assert.IsTrue(valueSet.SetEquals(values));
        }

        [TestMethod]
        public void TestReplaceItems()
        {
            var items = new[] { 42, 91, 80, 83, 29, 40, 88, 26, 38, 31, 85, 25, 98, 77, 62, 28 };
            var indexes = new[] { 1, 2, 8, 4 };

            var initialCount = items.Length - indexes.Length;
            var dict = new ObservableDictionary<int, double>(items.Take(initialCount).ToDictionary(i => i, i => 1d / i));
            var keys = dict.Keys;
            var values = keys.ListSelect(i => 1d / i);

            Assert.AreEqual(initialCount, dict.Count);
            Assert.AreEqual(initialCount, keys.Count);
            Assert.AreEqual(initialCount, values.Count);

            var count = initialCount;
            foreach (var i in indexes)
                dict[items[i]] = 1d / items[count++];

            Assert.AreEqual(initialCount, dict.Count);
            Assert.AreEqual(initialCount, keys.Count);
            Assert.AreEqual(initialCount, values.Count);

            var keySet = new HashSet<int>(items.Take(initialCount));
            var valueSet = new HashSet<double>(keySet.Select(i => 1d / i));
            var pairSet =
                new HashSet<KeyValuePair<int, double>>(keySet.Select(i => new KeyValuePair<int, double>(i, 1d / i)),
                    new PairComparer<int, double>());
            pairSet.ExceptWith(indexes.Select(i => items[i]).Select(i => new KeyValuePair<int, double>(i, 1d / i)));
            pairSet.UnionWith(indexes.Zip(items.Skip(initialCount),
                (index, item) => new KeyValuePair<int, double>(items[index], 1d / item)));

            Assert.IsTrue(pairSet.SetEquals(dict));
            Assert.IsTrue(keySet.SetEquals(keys));
            Assert.IsTrue(valueSet.SetEquals(values));

            valueSet.SymmetricExceptWith(indexes.Select(i => items[i]).Concat(items.Skip(initialCount)).Select(i => 1d / i));
            Assert.IsTrue(valueSet.SetEquals(dict.Values));
        }

        [TestMethod]
        public void TestClearItems()
        {
            var items = new[] { 98, 15, 51, 87, 66, 20, 27, 86 };

            var dict = new ObservableDictionary<int, double>(items.ToDictionary(i => i, i => 1d / i));
            var keys = dict.Keys;
            var values = keys.ListSelect(i => 1d / i);

            Assert.AreEqual(items.Length, dict.Count);
            Assert.AreEqual(items.Length, keys.Count);
            Assert.AreEqual(items.Length, values.Count);

            dict.Clear();

            Assert.AreEqual(0, dict.Count);
            Assert.AreEqual(0, keys.Count);
            Assert.AreEqual(0, values.Count);
        }

        class PairComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return Equals(x.Key, y.Key) && Equals(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
    }
}
