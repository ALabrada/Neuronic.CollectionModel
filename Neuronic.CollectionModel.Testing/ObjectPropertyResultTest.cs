using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.Results;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class ObjectPropertyResultTest
    {
        [TestMethod]
        public void TestCollectionCountResult()
        {
            const int skip = 2;
            const int count = 10;
            var collection = new ObservableCollection<int>();
            for (int i = 1; i <= skip; i++)
                collection.Add(i);
            var countResult = new ObjectPropertyResult<IReadOnlyObservableCollection<int>, int>(collection.ListAsObservable(),
                x => x.Count);
            Assert.AreEqual(skip, countResult.CurrentValue);
            var sum = 0;
            countResult.PropertyChanged += (sender, args) => sum += countResult.CurrentValue;
            for (int i = 0; i < count; i++)
                collection.Add(skip + 1 + i);
            Assert.AreEqual(skip + count, countResult.CurrentValue);
            Assert.AreEqual(collection.Skip(skip).Sum(), sum);
        }
    }
}