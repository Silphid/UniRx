using System;

namespace UniRx.Completables.Operators
{
    internal class ThenCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Func<ICompletable> selector;

        public ThenCompletable(ICompletable source, Func<ICompletable> selector)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.selector = selector;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new Then(this, observer, cancel).Run();
        }

        private class Then : OperatorCompletableObserverBase
        {
            private readonly ThenCompletable parent;
            private readonly SerialDisposable serialDisposable = new SerialDisposable();

            public Then(ThenCompletable parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                var sourceDisposable = new SingleAssignmentDisposable();
                serialDisposable.Disposable = sourceDisposable;

                sourceDisposable.Disposable = parent.source.Subscribe(this);
                return serialDisposable;
            }

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                var v = parent.selector();
                // dispose source subscription
                serialDisposable.Disposable = v.Subscribe(observer);
            }
        }
    }
}