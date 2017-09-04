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

        public abstract void OnCompleted();
        
        public abstract void OnError(Exception error);

        public void Dispose()
        {
            observer = EmptyCompletableObserver.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            if (target != null)
                target.Dispose();
        }
    }

    public abstract class OperatorObservableToCompletableObserverBase<T> : IDisposable, IObserver<T>
    {
        protected internal volatile ICompletableObserver observer;
        private IDisposable cancel;

        protected OperatorObservableToCompletableObserverBase(ICompletableObserver observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnNext(T value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            observer = EmptyCompletableObserver.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            if (target != null)
                target.Dispose();
        }
    }
}