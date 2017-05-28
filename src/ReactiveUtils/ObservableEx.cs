using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ReactiveUtils
{
    public static class ObservableEx
    {
        /// <summary>
        ///     Comparing previous and current items
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=213" />
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(this IObservable<TSource> source)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current));
        }

        /// <summary>
        ///     ObserveLatestOn – A coalescing ObserveOn
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=192" />
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IObservable<T> ObserveLatestOn<T>(
            this IObservable<T> source, IScheduler scheduler)
        {
            return Observable.Create<T>(observer =>
            {
                Notification<T> outsideNotification;
                var gate = new object();
                var active = false;
                var cancelable = new MultipleAssignmentDisposable();
                var disposable = source.Materialize().Subscribe(thisNotification =>
                {
                    bool wasNotAlreadyActive;
                    lock (gate)
                    {
                        wasNotAlreadyActive = !active;
                        active = true;
                        outsideNotification = thisNotification;
                    }

                    if (wasNotAlreadyActive)
                        cancelable.Disposable = scheduler.Schedule(self =>
                        {
                            Notification<T> localNotification;
                            lock (gate)
                            {
                                localNotification = outsideNotification;
                                outsideNotification = null;
                            }
                            localNotification.Accept(observer);
                            bool hasPendingNotification;
                            lock (gate)
                            {
                                hasPendingNotification = active = outsideNotification != null;
                            }
                            if (hasPendingNotification)
                                self();
                        });
                });
                return new CompositeDisposable(disposable, cancelable);
            });
        }

        /// <summary>
        ///     A reactive join example
        ///     <para>
        ///         Note: One thing to point out when using a stream as it’s own duration function, as I do in this example, is
        ///         that this is going to cause the stream to be subscribed to multiple times – you will want to either
        ///         Publish()/RefCount() the source stream or ensure that it is hot.
        ///         <see cref="http://www.zerobugbuild.com/?p=144" />
        ///     </para>
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=160" />
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TSample"></typeparam>
        /// <param name="source"></param>
        /// <param name="sampler"></param>
        /// <returns></returns>
        public static IObservable<TSource> SampleOn<TSource, TSample>(
            this IObservable<TSource> source,
            IObservable<TSample> sampler)
        {
            return from s in source
                join t in sampler
                on source equals Observable.Empty<Unit>()
                select s;
        }

        /// <summary>
        ///     Helper to log to the Console
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=47" />
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IObservable<T> LogToConsole<T>(
            this IObservable<T> source, string key = null)
        {
            return Observable.Create<T>(observer =>
                source.Materialize().Subscribe(notification =>
                {
                    Console.WriteLine(NotifyToString(notification, key));
                    notification.Accept(observer);
                }));
        }

        /// <summary>
        ///     Helper to log to the Console on subscribe
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=47" />
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IDisposable SubscribeConsoleNotifier<T>(
            this IObservable<T> source, string key = null)
        {
            return source.Materialize().Subscribe(
                n => Console.WriteLine(NotifyToString(n, key)));
        }

        private static string NotifyToString<T>(Notification<T> notification, string key = null)
        {
            if (key != null)
                return key + "\t" + notification;

            return notification.ToString();
        }
    }
}