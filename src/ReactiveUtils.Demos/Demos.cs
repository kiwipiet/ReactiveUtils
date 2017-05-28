using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CLAP;
using JetBrains.Annotations;

namespace ReactiveUtils.Demos
{
    class Demos
    {
        [Verb]
        [UsedImplicitly]
        static void AlternatingSubject()
        {
            new AlternatingSubjectDemo().Run();
        }

        [Verb(IsDefault = true)]
        [UsedImplicitly]
        static void PauseResume()
        {
            var values = Observable.Interval(TimeSpan.FromSeconds(1));
            var pauser = new Subject<bool>();

            var subscription = values.Pausable(pauser).Subscribe(v => Console.WriteLine($"{DateTime.Now} {v} {Thread.CurrentThread.ManagedThreadId}"));

            while (true)
            {
                var key = Console.ReadKey();
                switch (key.KeyChar)
                {
                    case 'p':
                        pauser.OnNext(true);
                        break;
                    case 'r':
                        pauser.OnNext(false);
                        break;
                    case 'q':
                        subscription.Dispose();
                        return;
                }
            }
        }
    }
}