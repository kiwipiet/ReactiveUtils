using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveUtils.Demos
{
    class AlternatingSubjectDemo
    {
        private readonly TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(2);

        public void Run()
        {
            var alt = new AlternatingSubject();
            var grabs = alt.First;
            var releases = alt.Second;
            var isGrabbed = new Subject<bool>();

            //I assume you have these in your real app, 
            //simulate them with key presses here
            var mouseDowns = new Subject<Unit>();
            var mouseUps = new Subject<Unit>();

            var gagueFulls = new Subject<Unit>();

            //the TakeUntils ensure that the timers stop ticking appropriately
            var decrements = from g in grabs
                from tick in Observable.Interval(_targetElapsedTime).TakeUntil(releases)
                select Unit.Default;
            //this TakeUnitl watches for either a grab or a gague full
            var increments = from r in releases
                from tick in Observable.Interval(_targetElapsedTime).TakeUntil(grabs.Merge(gagueFulls))
                select Unit.Default;

            //simulated values for testing, you may just have
            //these be properties on an INotifyPropertyChanged object
            //rather than having a PlayerScoreChanged observable.
            const int gagueMax = 20;
            const int gagueMin = 0;
            const int gagueStep = 1;
            var gagueValue = gagueMax;
            var playerScore = 0;

            var disp = new CompositeDisposable
            {
                grabs.Subscribe(v => isGrabbed.OnNext(true)),
                releases.Subscribe(v => isGrabbed.OnNext(false)),
                isGrabbed.Subscribe(v => Console.WriteLine("Grabbed: " + v)),
                gagueFulls.Subscribe(v => Console.WriteLine("Gague full")),
                decrements.Subscribe(v =>
                {
                    //testing use only
                    if (gagueValue <= gagueMin)
                    {
                        Console.WriteLine("Should not get here, decrement below min!!!");
                    }

                    //do the decrement
                    gagueValue -= gagueStep;
                    Console.WriteLine("Gague value: " + gagueValue.ToString());
                    if (gagueValue <= gagueMin)
                    {
                        gagueValue = gagueMin;
                        Console.WriteLine("New gague value: " + gagueValue);
                        alt.TriggerSecond();
                        //trigger a release when the gague empties
                    }
                }),
                decrements.Subscribe(v =>
                {
                    //based on your example, it seems you score just for grabbing
                    playerScore += 1;
                    Console.WriteLine("Player Score: " + playerScore);
                }),
                increments.Subscribe(v =>
                {
                    //testing use only
                    if (gagueValue >= gagueMax)
                    {
                        Console.WriteLine("Should not get here, increment above max!!!");
                    }

                    //do the increment
                    gagueValue += gagueStep;
                    Console.WriteLine("Gague value: " + gagueValue.ToString());
                    if (gagueValue >= gagueMax)
                    {
                        gagueValue = gagueMax;
                        Console.WriteLine("New gague value: " + gagueValue);
                        gagueFulls.OnNext(Unit.Default);
                        //trigger a full
                    }
                }),
                mouseDowns.Subscribe(v => alt.TriggerFirst()),
                mouseUps.Subscribe(v => alt.TriggerSecond())
            };
            //hook up IsGrabbed to the grabs and releases
            //output grabbed state to the console for testing


            //hook the "mouse" to the grab/release subject

            //mouse simulator
            bool done;
            do
            {
                done = false;
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.G)
                {
                    mouseDowns.OnNext(Unit.Default);
                }
                else if (key.Key == ConsoleKey.R)
                {
                    mouseUps.OnNext(Unit.Default);
                }
                else
                {
                    done = true;
                }
            } while (!done);
            //shutdown
            disp.Dispose();
            Console.ReadKey();
        }
    }
}