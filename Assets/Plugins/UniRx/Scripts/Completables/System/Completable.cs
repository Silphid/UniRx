using System;
using System.Collections.Generic;
using UniRx.Completables.Operators;

namespace UniRx.Completables
{
    public static class Completable
    {
        #region Concat
        
        public static ICompletable Concat(params ICompletable[] sources)
        {
            if (sources == null) throw new ArgumentNullException("sources");

            return new ConcatObservable(sources);
        }

        public static ICompletable Concat(this IEnumerable<ICompletable> sources)
        {
            if (sources == null) throw new ArgumentNullException("sources");

            return new ConcatObservable(sources);
        }

// TODO: Implement Merge
//        public static ICompletable Concat(this IObservable<ICompletable> sources)
//        {
//            return sources.Merge(maxConcurrent: 1);
//        }

// TODO: Implement CombineSources
//        public static ICompletable Concat(this ICompletable first, params ICompletable[] seconds)
//        {
//            if (first == null) throw new ArgumentNullException("first");
//            if (seconds == null) throw new ArgumentNullException("seconds");
//
//            var concat = first as ConcatObservable;
//            if (concat != null)
//            {
//                return concat.Combine(seconds);
//            }
//
//            return Concat(CombineSources(first, seconds));
//        }

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