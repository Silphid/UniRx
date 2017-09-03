using System;
using System.Threading;
using UniRx.Completables.InternalUtil;

namespace UniRx.Completables.Operators
{
    public abstract class OperatorCompletableObserverBase : IDisposable, ICompletableObserver
    {
        protected internal volatile ICompletableObserver observer;
        private IDisposable cancel;

        protected OperatorCompletableObserverBase(ICompletableObserver observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            observer = EmptyCompletableObserver.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            target?.Dispose();
        }
    }
}