using System;
using System.Collections.Generic;

namespace UniRx.Completables.Operators
{
    internal class ConcatObservable : OperatorCompletableBase
    {
        private readonly IEnumerable<ICompletable> sources;

        public ConcatObservable(IEnumerable<ICompletable> sources)
            : base(true)
        {
            this.sources = sources;
        }

        public ICompletable Combine(IEnumerable<ICompletable> combineSources)
        {
            return new ConcatObservable(CombineSources(sources, combineSources));
        }

        private static IEnumerable<ICompletable> CombineSources(IEnumerable<ICompletable> first, IEnumerable<ICompletable> second)
        {
            foreach (var item in first)
            {
                yield return item;
            }
            foreach (var item in second)
            {
                yield return item;
            }
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new Concat(this, observer, cancel).Run();
        }

        private class Concat : OperatorCompletableObserverBase
        {
            private readonly ConcatObservable parent;
            private readonly object gate = new object();

            private bool isDisposed;
            private IEnumerator<ICompletable> e;
            private SerialDisposable subscription;
            private Action nextSelf;

            public Concat(ConcatObservable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                isDisposed = false;
                e = parent.sources.GetEnumerator();
                subscription = new SerialDisposable();

                var schedule = Scheduler.DefaultSchedulers.TailRecursion.Schedule(RecursiveRun);

                return StableCompositeDisposable.Create(schedule, subscription, Disposable.Create(() =>
               {
                   lock (gate)
                   {
                       isDisposed = true;
                       e.Dispose();
                   }
               }));
            }

            private void RecursiveRun(Action self)
            {
                lock (gate)
                {
                    nextSelf = self;
                    if (isDisposed) return;

                    var current = default(ICompletable);
                    var hasNext = false;
                    var ex = default(Exception);

                    try
                    {
                        hasNext = e.MoveNext();
                        if (hasNext)
                        {
                            current = e.Current;
                            if (current == null) throw new InvalidOperationException("sequence is null.");
                        }
                        else
                        {
                            e.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                        e.Dispose();
                    }

                    if (ex != null)
                    {
                        try { observer.OnError(ex); }
                        finally { Dispose(); }
                        return;
                    }

                    if (!hasNext)
                    {
                        try { observer.OnCompleted(); }
                        finally { Dispose(); }
                        return;
                    }

                    var source = current;
                    var d = new SingleAssignmentDisposable();
                    subscription.Disposable = d;
                    d.Disposable = source.Subscribe(this);
                }
            }

            public override void OnCompleted()
            {
                nextSelf();
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }
        }
    }
}
