using System;
using UniRx.Completables.Operators;

namespace UniRx.Completables
{
    public static class Completable
    {
        public static ICompletable Then(this ICompletable source, ICompletable other)
        {
            return Then(source, () => other);
        }

        public static ICompletable Then(this ICompletable source, Func<ICompletable> selector)
        {
            return new ThenCompletable(source, selector);
        }
    }
}