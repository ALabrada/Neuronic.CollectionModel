using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Collections;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class QueryableTest
    {
        [TestMethod]
        public void SelectTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify {Prop = x});
            var result = from item in source.ListAsObservable().AsQueryableCollection() select item.Prop;
            Assert.IsTrue(result.CollectionAsObservable() is DynamicTransformingReadOnlyObservableList<Notify, int>);
        }

        [TestMethod]
        public void SelectManyTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable()
                .ListSelectMany(item => Enumerable.Range(0, 5).Select(i => 100 * i + item.Prop));
            Assert.IsFalse(result.CollectionAsObservable() is EventSource);
        }

        [TestMethod]
        public void WhereTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Where(item => item.Prop % 2 == 0);
            Assert.IsTrue(result.CollectionAsObservable() is FilteredReadOnlyObservableList<Notify>);
        }

        [TestMethod]
        public void OfTypeTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().OfType<object>();
            Assert.IsTrue(result.CollectionAsObservable() is CastingReadOnlyObservableList<Notify, object>);
        }

        [TestMethod]
        public void CastTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Cast<object>();
            Assert.IsTrue(result.CollectionAsObservable() is CastingReadOnlyObservableList<Notify, object>);
        }

        [TestMethod]
        public void JoinTest()
        {
            var outerSource = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var innerSource = Enumerable.Range(1, 10).Select(x => new Notify { Prop = 2 * x - 1 });
            var result = outerSource.ListAsObservable().AsQueryableCollection().Join(innerSource, 
                x => x.Prop, x => x.Prop, (x, y) => x.Prop + y.Prop, EqualityComparer<int>.Default);
            Assert.IsTrue(result.CollectionAsObservable() is InnerJoinReadOnlyObservableCollection<Notify, Notify, int, int>);
        }

        [TestMethod]
        public void OrderByTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().OrderBy(x => x.Prop, Comparer<int>.Default);
            var result2 = result.ThenBy(x => x.Prop * 2);
            Assert.IsTrue(result.CollectionAsObservable() is KeySortedReadOnlyObservableList<Notify, int>);
            Assert.IsTrue(result2.CollectionAsObservable() is KeySortedReadOnlyObservableList<Notify, IList>);
        }

        [TestMethod]
        public void OrderByDescendingTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().OrderByDescending(x => x.Prop, Comparer<int>.Default);
            var result2 = result.ThenByDescending(x => x.Prop * 2);
            Assert.IsTrue(result.CollectionAsObservable() is KeySortedReadOnlyObservableList<Notify, int>);
            Assert.IsTrue(result2.CollectionAsObservable() is KeySortedReadOnlyObservableList<Notify, IList>);
        }

        [TestMethod]
        public void TakeTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Take(10);
            Assert.IsTrue(result.CollectionAsObservable() is RangedReadOnlyObservableList<Notify>);
        }

        [TestMethod]
        public void SkipTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Skip(10);
            Assert.IsTrue(result.CollectionAsObservable() is RangedReadOnlyObservableList<Notify>);
        }

        [TestMethod]
        public void GroupByTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().GroupBy(i => i.Prop % 2, EqualityComparer<int>.Default);
            Assert.IsTrue(result.CollectionAsObservable() is GroupingReadOnlyObservableCollectionSource<Notify, int>);
        }

        [TestMethod]
        public void DistinctTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Distinct();
            Assert.IsTrue(result.CollectionAsObservable() is DistinctReadOnlyObservableCollection<Notify>);
        }

        [TestMethod]
        public void ConcatTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection()
                .Concat(Enumerable.Range(30, 10).Select(x => new Notify { Prop = x }));
            Assert.IsTrue(result.CollectionAsObservable() is IReadOnlyObservableCollection<Notify>);
        }

        [TestMethod]
        public void UnionTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection()
                .Union(Enumerable.Range(30, 10).Select(x => new Notify { Prop = x }));
            Assert.IsTrue(result.CollectionAsObservable() is DistinctReadOnlyObservableCollection<Notify>);
        }

        [TestMethod]
        public void ExceptTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection()
                .Except(Enumerable.Range(30, 10).Select(x => new Notify { Prop = x }));
            Assert.IsTrue(result.CollectionAsObservable() is SetDifferenceReadOnlyObservableCollection<Notify>);
        }

        [TestMethod]
        public void IntersectTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection()
                .Intersect(Enumerable.Range(30, 10).Select(x => new Notify { Prop = x }));
            Assert.IsTrue(result.CollectionAsObservable() is SetIntersectionReadOnlyObservableCollection<Notify>);
        }

        [TestMethod]
        public void ZipTest()
        {
            var outerSource = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x }).ToList();
            var innerSource = Enumerable.Range(1, 10).Select(x => new Notify { Prop = 2 * x - 1 }).ToList();
            var result = outerSource.ListAsObservable().AsQueryableCollection().Zip(innerSource,
                (x, y) => x.Prop + y.Prop);
            Assert.IsTrue(result.CollectionAsObservable() is ZipReadOnlyObservableList<Notify, Notify, int>);
        }

        [TestMethod]
        public void CountTest()
        {
            var source = Enumerable.Range(0, 20).Select(x => new Notify { Prop = x });
            var result = source.ListAsObservable().AsQueryableCollection().Count();
            Assert.AreEqual(20, result);
        }
    }
}