using System;
using System.Reactive.Concurrency;

namespace ReactiveUtils
{
    public static class SchedulerEx
    {
        /// <summary>
        /// Dealing with occasionally overrunning Timer initiated tasks using recursive scheduling
        /// </summary>
        /// <see cref="http://www.zerobugbuild.com/?p=259"/>
        /// <param name="scheduler"></param>
        /// <param name="interval"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IDisposable ScheduleRecurringAction(this IScheduler scheduler, TimeSpan interval, Action action)
        {
            return scheduler.Schedule(interval, scheduleNext =>
            {
                action();
                scheduleNext(interval);
            });
        }
    }
}
