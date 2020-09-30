using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Neuronic.CollectionModel.Testing
{
    [TestClass]
    public class ObserverTest
    {
        [TestMethod]
        public void TestAny()
        {
            var source = new ObservableCollection<int>();
            var subject = new BehaviorSubject<bool>(false);
            using (source.ListAsObservable().ObservableAny().Subscribe(subject))
            {
                Assert.AreEqual(false, subject.Value);
                source.Add(0);
                Assert.AreEqual(true, subject.Value);
                source.RemoveAt(0);
                Assert.AreEqual(false, subject.Value);
                for (int i = 1; i < 10; i++)
                    source.Add(i);
                Assert.AreEqual(true, subject.Value);
                for (int i = 1; i < 10; i += 2)
                    source.Remove(i);
                Assert.AreEqual(true, subject.Value);
                for (int i = 0; i < source.Count; i += 2)
                    source[i] *= 2;
                for (int i = 1; i < 5; i++)
                    source.Add(i);
                Assert.AreEqual(true, subject.Value);
                source.Clear();
                Assert.AreEqual(false, subject.Value);
            }
        }

        [TestMethod]
        public void TestFilteredAny()
        {
            var source = new ObservableCollection<Notify>();
            var subject = new BehaviorSubject<bool>(false);
            using (source.ListAsObservable().ObservableAny(p => p.Prop % 2 == 1, nameof(Notify.Prop)).Subscribe(subject))
            {
                source.Add(new Notify { Prop = 0 });
                Assert.AreEqual(false, subject.Value);
                for (int i = 1; i < 10; i++)
                    source.Add(new Notify { Prop = i });
                Assert.AreEqual(true, subject.Value);
                foreach (var t in source)
                    t.Prop *= 2;
                Assert.AreEqual(false, subject.Value);
                source[0].Prop = 1;
                Assert.AreEqual(true, subject.Value);
                source.Clear();
                Assert.AreEqual(false, subject.Value); 
            }
        }

        [TestMethod]
        public void TestFilteredAll()
        {
            var source = new ObservableCollection<Notify>();
            var subject = new BehaviorSubject<bool>(false);
            using (source.ListAsObservable().ObservableAll(p => p.Prop % 2 == 0, nameof(Notify.Prop)).Subscribe(subject))
            {
                source.Add(new Notify { Prop = 0 });
                Assert.AreEqual(true, subject.Value);
                for (int i = 1; i < 10; i++)
                    source.Add(new Notify { Prop = i });
                Assert.AreEqual(false, subject.Value);
                foreach (var t in source)
                    t.Prop *= 2;
                Assert.AreEqual(true, subject.Value);
                source[0].Prop = 1;
                Assert.AreEqual(false, subject.Value);
                source.Clear();
                Assert.AreEqual(true, subject.Value); 
            }
        }

    }
}