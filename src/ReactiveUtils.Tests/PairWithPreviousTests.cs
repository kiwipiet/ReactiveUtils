using System;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Xunit;

namespace ReactiveUtils.Tests
{
    public class PairWithPreviousTests : ReactiveTest
    {
        [Fact]
        public void Works()
        {
            var testScheduler = new TestScheduler();

            var source = Observable.Range(1, 3);

            var results = testScheduler.Start(
                () => source.PairWithPrevious());

            results.Messages.AssertEqual(
                OnNext(Subscribed, Tuple.Create(0, 1)),
                OnNext(Subscribed, Tuple.Create(1, 2)),
                OnNext(Subscribed, Tuple.Create(2, 3)),
                OnCompleted<Tuple<int, int>>(Subscribed));
        }
    }
}