using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace ReactiveUtils
{
    /// <summary>
    /// http://stackoverflow.com/questions/9418385/toggling-between-values-while-producing-events-on-timer/9445579#9445579
    /// </summary>
    public class AlternatingSubject : IDisposable
    {
        private readonly object _lockObj = new object();

        private int _firstTriggered;

        public ISubject<Unit> First { get; } = new Subject<Unit>();

        public ISubject<Unit> Second { get; } = new Subject<Unit>();

        public void TriggerFirst()
        {
            if (System.Threading.Interlocked.Exchange(ref _firstTriggered, 1) == 1)
                return;

            First.OnNext(Unit.Default);
        }

        public void TriggerSecond()
        {
            if (System.Threading.Interlocked.Exchange(ref _firstTriggered, 0) == 0)
                return;

            Second.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            lock (_lockObj)
            {
                First.OnCompleted();
                Second.OnCompleted();
            }
        }
    }
}
