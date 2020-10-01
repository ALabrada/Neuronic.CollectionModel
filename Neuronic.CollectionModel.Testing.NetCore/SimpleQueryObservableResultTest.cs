using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neuronic.CollectionModel.WeakEventPattern;

namespace Neuronic.CollectionModel.Testing.NetCore
{
    [TestClass]
    public class SimpleQueryObservableResultTest : IWeakEventListener
    {
        private bool? _currentValue;

        [TestInitialize]
        public void Initialize()
        {
            _currentValue = null;
        }

        [TestMethod]
        public void TestAny()
        {
            var source = new ObservableCollection<int>();
            var result = source.ListAsObservable().ObservableAny();
            PropertyChangedEventManager.AddListener(result, this, nameof(IObservableResult<bool>.CurrentValue));
            Assert.AreEqual(false, result.CurrentValue);
            Assert.IsNull(_currentValue);
            source.Add(0);
            Assert.AreEqual(true, _currentValue);
            source.RemoveAt(0);
            Assert.AreEqual(false, _currentValue);
            for (int i = 1; i < 10; i++)
                source.Add(i);
            Assert.AreEqual(true, _currentValue);
            for (int i = 1; i < 10; i += 2)
                source.Remove(i);
            Assert.AreEqual(true, _currentValue);
            for (int i = 0; i < source.Count; i += 2)
                source[i] *= 2;
            for (int i = 1; i < 5; i++)
                source.Add(i);
            Assert.AreEqual(true, _currentValue);
            source.Clear();
            Assert.AreEqual(false, _currentValue);
        }

        [TestMethod]
        public void TestFilteredAny()
        {
            var source = new ObservableCollection<Notify>();
            var result = source.ListAsObservable().ObservableAny(p => p.Prop % 2 == 1, nameof(Notify.Prop));
            PropertyChangedEventManager.AddListener(result, this, nameof(IObservableResult<bool>.CurrentValue));
            Assert.AreEqual(false, result.CurrentValue);
            Assert.IsNull(_currentValue);
            source.Add(new Notify {Prop = 0});
            Assert.AreEqual(false, result.CurrentValue);
            Assert.IsNull(_currentValue);
            for (int i = 1; i < 10; i++)
                source.Add(new Notify { Prop = i });
            Assert.AreEqual(true, _currentValue);
            foreach (var t in source)
                t.Prop *= 2;
            Assert.AreEqual(false, _currentValue);
            source[0].Prop = 1;
            Assert.AreEqual(true, _currentValue);
            source.Clear();
            Assert.AreEqual(false, _currentValue);
        }

        [TestMethod]
        public void TestFilteredAll()
        {
            var source = new ObservableCollection<Notify>();
            var result = source.ListAsObservable().ObservableAll(p => p.Prop % 2 == 0, nameof(Notify.Prop));
            PropertyChangedEventManager.AddListener(result, this, nameof(IObservableResult<bool>.CurrentValue));
            Assert.AreEqual(true, result.CurrentValue);
            Assert.IsNull(_currentValue);
            source.Add(new Notify { Prop = 0 });
            Assert.AreEqual(true, result.CurrentValue);
            Assert.IsNull(_currentValue);
            for (int i = 1; i < 10; i++)
                source.Add(new Notify { Prop = i });
            Assert.AreEqual(false, _currentValue);
            foreach (var t in source)
                t.Prop *= 2;
            Assert.AreEqual(true, _currentValue);
            source[0].Prop = 1;
            Assert.AreEqual(false, _currentValue);
            source.Clear();
            Assert.AreEqual(true, _currentValue);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var query = sender as IObservableResult<bool>;
            if (query == null)
                return false;
            _currentValue = query.CurrentValue;
            return true;
        }
    }
}
