﻿using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CLAP;

namespace ReactiveUtils.Demos
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Run<Demos>(args);
        }
    }

    class Demos
    {
        [Verb(IsDefault = true)]
        static void AlternatingSubject()
        {
            new AlternatingSubjectDemo().Run();
        }
    }

    class AlternatingSubjectDemo
    {
        private readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(2);

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
                from tick in Observable.Interval(TargetElapsedTime).TakeUntil(releases)
                select Unit.Default;
            //this TakeUnitl watches for either a grab or a gague full
            var increments = from r in releases
                from tick in Observable.Interval(TargetElapsedTime).TakeUntil(grabs.Merge(gagueFulls))
                select Unit.Default;

            //simulated values for testing, you may just have
            //these be properties on an INotifyPropertyChanged object
            //rather than having a PlayerScoreChanged observable.
            const int GagueMax = 20;
            const int GagueMin = 0;
            const int GagueStep = 1;
            var gagueValue = GagueMax;
            var playerScore = 0;

            var disp = new CompositeDisposable();
            //hook up IsGrabbed to the grabs and releases
            disp.Add(grabs.Subscribe(v => isGrabbed.OnNext(true)));
            disp.Add(releases.Subscribe(v => isGrabbed.OnNext(false)));
            //output grabbed state to the console for testing
            disp.Add(isGrabbed.Subscribe(v => Console.WriteLine("Grabbed: " + v)));
            disp.Add(gagueFulls.Subscribe(v => Console.WriteLine("Gague full")));


            disp.Add(decrements.Subscribe(v =>
            {
                //testing use only
                if (gagueValue <= GagueMin)
                {
                    Console.WriteLine("Should not get here, decrement below min!!!");
                }

                //do the decrement
                gagueValue -= GagueStep;
                Console.WriteLine("Gague value: " + gagueValue.ToString());
                if (gagueValue <= GagueMin)
                {
                    gagueValue = GagueMin;
                    Console.WriteLine("New gague value: " + gagueValue);
                    alt.TriggerSecond();
                    //trigger a release when the gague empties
                }
            }));
            disp.Add(decrements.Subscribe(v =>
            {
                //based on your example, it seems you score just for grabbing
                playerScore += 1;
                Console.WriteLine("Player Score: " + playerScore);
            }));
            disp.Add(increments.Subscribe(v =>
            {
                //testing use only
                if (gagueValue >= GagueMax)
                {
                    Console.WriteLine("Should not get here, increment above max!!!");
                }

                //do the increment
                gagueValue += GagueStep;
                Console.WriteLine("Gague value: " + gagueValue.ToString());
                if (gagueValue >= GagueMax)
                {
                    gagueValue = GagueMax;
                    Console.WriteLine("New gague value: " + gagueValue);
                    gagueFulls.OnNext(Unit.Default);
                    //trigger a full
                }
            }));
            //hook the "mouse" to the grab/release subject
            disp.Add(mouseDowns.Subscribe(v => alt.TriggerFirst()));
            disp.Add(mouseUps.Subscribe(v => alt.TriggerSecond()));

            //mouse simulator
            var done = false;
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