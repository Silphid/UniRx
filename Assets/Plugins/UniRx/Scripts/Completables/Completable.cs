using System;
using System.Collections.Generic;
using UniRx.Completables.Operators;

namespace UniRx.Completables
{
    public static class Completable
    {
        #region Internal helpers

        internal static IEnumerable<ICompletable> Combine(ICompletable first, ICompletable second)
        {
            yield return first;
            yield return second;
        }

        internal static IEnumerable<ICompletable> Combine(ICompletable first, IEnumerable<ICompletable> seconds)
        {
            yield return first;
            
            foreach (var second in seconds)
                yield return second;
        }

        internal static IEnumerable<ICompletable> Combine(IEnumerable<ICompletable> firsts, ICompletable second)
        {
            foreach (var first in firsts)
                yield return first;
            
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
        
        #region Conversion (AsObservable, AsEmptyUnitObservable, AsCompletable) 

        public static IObservable<T> AsObservable<T>(this ICompletable source)
        {
            return new AsObservableObservable<T>(source);
        }

        public static IObservable<Unit> AsEmptyUnitObservable(this ICompletable source)
        {
            return new AsObservableObservable<Unit>(source);
        }

        public static IObservable<Unit> AsSingleUnitObservable(this ICompletable source)
        {
            return new AsSingleUnitObservableCompletable(source);
        }

        public static ICompletable AsCompletable<T>(this IObservable<T> source)
        {
            return new AsCompletableCompletable<T>(source);
        }

        #endregion

        #region Combination (Concat, Merge, Then, WhenAll)

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
        
        public static ICompletable Then(this ICompletable first, params ICompletable[] seconds)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (seconds == null) throw new ArgumentNullException("seconds");

            var concat = first as ConcatCompletable;
            if (concat != null)
                return concat.Combine(seconds);

            return Concat(Combine(first, seconds));
        }

        public static ICompletable Then(this ICompletable first, Func<ICompletable> secondFactory)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (secondFactory == null) throw new ArgumentNullException("secondFactory");

            var second = Defer(secondFactory);
            var concat = first as ConcatCompletable;
            if (concat != null)
                return concat.Combine(second);

            return Concat(Combine(first, second));
        }

        public static IObservable<T> Then<T>(this ICompletable first, IObservable<T> second)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");

            return first.AsObservable<T>().Concat(second);
        }
        
        public static ICompletable Then<T>(this IObservable<T> first, params ICompletable[] seconds)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (seconds == null) throw new ArgumentNullException("seconds");

            return Concat(Combine(first.AsCompletable(), seconds));
        }
        
        /// <summary>
        /// <para>Specialized for single async operations like Task.WhenAll, Zip.Take(1).</para>
        /// <para>If sequence is empty, return T[0] array.</para>
        /// </summary>
        public static ICompletable WhenAll(params ICompletable[] sources)
        {
            return sources.Length != 0
                ? new WhenAllCompletable(sources)
                : Empty();
        }

        /// <summary>
        /// <para>Specialized for single async operations like Task.WhenAll, Zip.Take(1).</para>
        /// <para>If sequence is empty, return T[0] array.</para>
        /// </summary>
        public static ICompletable WhenAll(this IEnumerable<ICompletable> sources)
        {
            var array = sources as ICompletable[];
            return array != null
                ? WhenAll(array)
                : new WhenAllCompletable(sources);
        }

        #endregion

        #region Creation (Defer, Empty, Throw)

        /// <summary>
        /// Create anonymous completable. Observer has exception durability. This is recommended to make operator and event like generator. 
        /// </summary>
        public static ICompletable Create(Func<ICompletableObserver, IDisposable> subscribe)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateCompletable(subscribe);
        }

        /// <summary>
        /// Create anonymous completable. Observer has exception durability. This is recommended to make operator and event like generator (Hot Completable). 
        /// </summary>
        public static ICompletable Create(Func<ICompletableObserver, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateCompletable(subscribe, isRequiredSubscribeOnCurrentThread);
        }

        /// <summary>
        /// Create anonymous completable. Observer has exception durability. This is recommended to make operator and event like generator. 
        /// </summary>
        public static ICompletable CreateWithState<TState>(TState state, Func<TState, ICompletableObserver, IDisposable> subscribe)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateCompletable<TState>(state, subscribe);
        }

        /// <summary>
        /// Create anonymous completable. Observer has exception durability. This is recommended to make operator and event like generator (Hot Completable). 
        /// </summary>
        public static ICompletable CreateWithState<TState>(TState state, Func<TState, ICompletableObserver, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateCompletable<TState>(state, subscribe, isRequiredSubscribeOnCurrentThread);
        }

        public static ICompletable Defer(Func<ICompletable> observableFactory)
        {
            return new DeferCompletable(observableFactory);
        }

        /// <summary>
        /// Empty Completable. Returns only OnCompleted.
        /// </summary>
        public static ICompletable Empty()
        {
            return Empty(Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        /// <summary>
        /// Empty Completable. Returns only OnCompleted on specified scheduler.
        /// </summary>
        public static ICompletable Empty(IScheduler scheduler)
        {
            return scheduler == Scheduler.Immediate
                ? (ICompletable) ImmutableEmptyCompletable.Instance
                : new EmptyCompletable(scheduler);
        }

        /// <summary>
        /// Never-terminating Completable.
        /// </summary>
        public static ICompletable Never()
        {
            return ImmutableNeverCompletable.Instance;
        }
        
        /// <summary>
        /// Empty Completable. Returns only onError.
        /// </summary>
        public static ICompletable Throw(Exception error)
        {
            return Throw(error, Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        /// <summary>
        /// Empty Completable. Returns only onError on specified scheduler.
        /// </summary>
        public static ICompletable Throw(Exception error, IScheduler scheduler)
        {
            return new ThrowCompletable(error, scheduler);
        }

        #endregion
        
        #region DoOn... (Error, Completed, Terminate, Subscribe, Cancel)

        public static ICompletable DoOnError<T>(this ICompletable source, Action<Exception> onError)
        {
            return new DoOnErrorCompletable<T>(source, onError);
        }

        public static ICompletable DoOnCompleted<T>(this ICompletable source, Action onCompleted)
        {
            return new DoOnCompletedCompletable<T>(source, onCompleted);
        }

        public static ICompletable DoOnTerminate<T>(this ICompletable source, Action onTerminate)
        {
            return new DoOnTerminateCompletable<T>(source, onTerminate);
        }

        public static ICompletable DoOnSubscribe<T>(this ICompletable source, Action onSubscribe)
        {
            return new DoOnSubscribeCompletable<T>(source, onSubscribe);
        }

        public static ICompletable DoOnCancel<T>(this ICompletable source, Action onCancel)
        {
            return new DoOnCancelCompletable<T>(source, onCancel);
        }
        
        #endregion

        #region Error Handling (Catch, CatchIgnore)

        /// <summary>Catches given exception and returns Completable provided by errorHandler Func.</summary>
        public static ICompletable Catch<TException>(this ICompletable source, Func<TException, ICompletable> errorHandler)
            where TException : Exception
        {
            return new CatchCompletable<TException>(source, errorHandler);
        }

        /// <summary>Tries multiples Completable sources in order, catching any exception and falling back to next source.</summary>
        public static ICompletable Catch(this IEnumerable<ICompletable> sources)
        {
            return new CatchCompletable(sources);
        }

        /// <summary>Catches given exception and returns Observable.Empty.</summary>
        public static IObservable<TSource> CatchIgnore<TSource, TException>(this IObservable<TSource> source, Action<TException> errorAction)
            where TException : Exception
        {
            var result = source.Catch((TException ex) =>
            {
                errorAction(ex);
                return Observable.Empty<TSource>();
            });
            return result;
        }

        /// <summary>Catches any exception and returns Observable.Empty.</summary>
        public static IObservable<TSource> CatchIgnore<TSource>(this IObservable<TSource> source)
        {
            return source.Catch<TSource, Exception>(Stubs.CatchIgnore<TSource>);
        }

        #endregion

        #region Time (Timer, ThenTimer, Timeout, Wait)

        public static ICompletable Timer(TimeSpan dueTime, IScheduler scheduler = null)
        {
            return new TimerCompletable(dueTime, scheduler ?? Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static ICompletable Timer(DateTimeOffset dueTime, IScheduler scheduler = null)
        {
            return new TimerCompletable(dueTime, scheduler ?? Scheduler.DefaultSchedulers.TimeBasedOperations);
        }
        
        public static ICompletable ThenTimer(this ICompletable This, TimeSpan dueTime, IScheduler scheduler = null)
        {
            if (This == null) throw new ArgumentNullException("This");

            return This.Then(Timer(dueTime, scheduler));
        }
        
        public static ICompletable ThenTimer(this ICompletable This, DateTimeOffset dueTime, IScheduler scheduler = null)
        {
            if (This == null) throw new ArgumentNullException("This");

            return This.Then(Timer(dueTime, scheduler));
        }

        public static ICompletable Timeout(this ICompletable This, TimeSpan dueTime, IScheduler scheduler = null)
        {
            return new TimeoutCompletable(This, dueTime, scheduler ?? Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static ICompletable Timeout(this ICompletable This, DateTimeOffset dueTime, IScheduler scheduler = null)
        {
            return new TimeoutCompletable(This, dueTime, scheduler ?? Scheduler.DefaultSchedulers.TimeBasedOperations);
        }

        public static void Wait(this ICompletable This, TimeSpan? timeoutDuration = null)
        {
            new WaitCompletableObserver(This, timeoutDuration).Run();
        }

        #endregion
    }
}