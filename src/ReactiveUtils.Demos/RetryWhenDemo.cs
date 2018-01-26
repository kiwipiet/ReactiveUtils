using System;
using System.Reactive.Linq;

namespace ReactiveUtils.Demos
{
    class RetryWhenDemo
    {
        public void Run()
        {
            int[] count = { 3 };

            Observable.Defer(() =>
                {
                    if (count[0]-- == 0)
                    {
                        return Observable.Return("Success");
                    }

                    throw new Exception("Error");
                    // or
                    //return Observable.Throw<String>(new Exception());
                })
                .RetryWhen(
                    f => f.SelectMany(e =>
                    {
                        Console.WriteLine("Retrying...");
                        return Observable.Timer(TimeSpan.FromSeconds(1));
                    })
                )
                .Subscribe(Console.WriteLine, Console.WriteLine, () => Console.WriteLine("Done"));

            Console.ReadKey();
        }
    }
}