using System.Diagnostics;
using System.Reactive.Subjects;

namespace Neuronic.CollectionModel.Testing
{
    [DebuggerDisplay("{Prop}")]
    public class Observable
    {
        public Observable() : this (0)
        {
        }

        public Observable(int value)
        {
            Subject = new BehaviorSubject<int>(value);
        }

        public int Prop
        {
            get => Subject.Value;
            set => Subject.OnNext(value);
        }

        public BehaviorSubject<int> Subject { get; }
    }
}