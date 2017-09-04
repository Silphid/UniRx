using System;
using System.Collections.Generic;
using UniRx.Completables.Operators;

namespace UniRx.Completables
{
    public static class Completable
    {
        #region Internal helpers

        internal static IEnumerable<ICompletable> Combine(ICompletable first, IEnumerable<ICompletable> seconds)
        {
            yield return first;
            
            foreach (var second in seconds)
                yield return second;
        }

        internal static IEnumerable<ICompletable> Combine(IEnumerable<ICompletable> firsts, IEnumerable<ICompletable> seconds)
        {
            foreach (var first in firsts)
                yield return first;

            foreach (var second in seconds)
                yield return second;
        }

        #endregion
        
        #region Concat

        public static ICompletable Concat(params ICompletable[] sources)
        {
            if (sources == null) throw new ArgumentNullException("sources");

            return new ConcatCompletable(sources);
        }

        public static ICompletable Concat(this IEnumerable<ICompletable> sources)
        {
            if (sources == null) throw new ArgumentNullException("sources");

            return new ConcatCompletable(sources);
        }

        public static ICompletable Concat(this IObservable<ICompletable> sources)
        {
            return sources.Merge(maxConcurrent: 1);
        }

        public static ICompletable Concat(this ICompletable first, params ICompletable[] seconds)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (seconds == null) throw new ArgumentNullException("seconds");

            var concat = first as ConcatCompletable;
            if (concat != null)
                return concat.Combine(seconds);

            return Concat(Combine(first, seconds));
        }

        #endregion
        
        #region Do

        public static ICompletable DoOnError<T>(this ICompletable source, Action<Exception> onError)
        {
            return new DoOnErrorObservable<T>(source, onError);
        }

        public static ICompletable DoOnCompleted<T>(this ICompletable source, Action onCompleted)
        {
            return new DoOnCompletedObservable<T>(source, onCompleted);
        }

        public static ICompletable DoOnTerminate<T>(this ICompletable source, Action onTerminate)
        {
            return new DoOnTerminateObservable<T>(source, onTerminate);
        }

        public static ICompletable DoOnSubscribe<T>(this ICompletable source, Action onSubscribe)
        {
            return new DoOnSubscribeObservable<T>(source, onSubscribe);
        }

        public static ICompletable DoOnCancel<T>(this ICompletable source, Action onCancel)
        {
            return new DoOnCancelObservable<T>(source, onCancel);
        }
        
        #endregion

        #region Merge

        public static ICompletable Merge(this IEnumerable<ICompletable> sources)
        {
            return Merge(sources, Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        public static ICompletable Merge(this IEnumerable<ICompletable> sources, IScheduler scheduler)
        {
            return new MergeCompletable(sources.ToObservable(scheduler), scheduler == Scheduler.CurrentThread);
        }

        public static ICompletable Merge(this IEnumerable<ICompletable> sources, IScheduler scheduler, int maxConcurrent)
        {
            return new MergeCompletable(sources.ToObservable(scheduler), maxConcurrent, scheduler == Scheduler.CurrentThread);
        }

        public static ICompletable Merge(params ICompletable[] sources)
        {
            return Merge(Scheduler.DefaultSchedulers.ConstantTimeOperations, sources);
        }

        public static ICompletable Merge(IScheduler scheduler, params ICompletable[] sources)
        {
            return new MergeCompletable(sources.ToObservable(scheduler), scheduler == Scheduler.CurrentThread);
        }

        public static ICompletable Merge(this ICompletable first, params ICompletable[] others)
        {
            return Merge(Combine(first, others));
        }

        public static ICompletable Merge(this ICompletable first, ICompletable second, IScheduler scheduler)
        {
            return Merge(scheduler, first, second);
        }

        public static ICompletable Merge(this IObservable<ICompletable> sources)
        {
            return new MergeCompletable(sources, false);
        }

        public static ICompletable Merge(this IObservable<ICompletable> sources, int maxConcurrent)
        {
            return new MergeCompletable(sources, maxConcurrent, false);
        }

        #endregion
        
        #region Then

        public static ICompletable Then(this ICompletable source, ICompletable other)
        {
            return Then(source, () => other);
        }

        public static ICompletable Then(this ICompletable source, Func<ICompletable> selector)
        {
            return new ThenCompletable(source, selector);
        }
        
        #endregion
    }
}