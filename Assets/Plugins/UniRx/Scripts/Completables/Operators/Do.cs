using System;

namespace UniRx.Completables.Operators
{
    // DoOnError, DoOnCompleted, DoOnTerminate, DoOnSubscribe, DoOnCancel

    internal class DoOnErrorCompletable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action<Exception> onError;

        public DoOnErrorCompletable(ICompletable source, Action<Exception> onError)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onError = onError;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnErrorObserver(this, observer, cancel).Run();
        }

        private class DoOnErrorObserver : OperatorCompletableObserverBase
        {
            private readonly DoOnErrorCompletable<T> parent;

            public DoOnErrorObserver(DoOnErrorCompletable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.onError(error);
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }


                try { observer.OnError(error); }
                finally { Dispose(); }
            }
        }
    }

    internal class DoOnCompletedCompletable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCompleted;

        public DoOnCompletedCompletable(ICompletable source, Action onCompleted)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onCompleted = onCompleted;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnCompleted(this, observer, cancel).Run();
        }

        private class DoOnCompleted : OperatorCompletableObserverBase
        {
            private readonly DoOnCompletedCompletable<T> parent;

            public DoOnCompleted(DoOnCompletedCompletable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnCompleted()
            {
                try
                {
                    parent.onCompleted();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    Dispose();
                    return;
                }
                try { observer.OnCompleted(); } finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }
        }
    }

    internal class DoOnTerminateCompletable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onTerminate;

        public DoOnTerminateCompletable(ICompletable source, Action onTerminate)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onTerminate = onTerminate;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnTerminate(this, observer, cancel).Run();
        }

        class DoOnTerminate : OperatorCompletableObserverBase
        {
            private readonly DoOnTerminateCompletable<T> parent;

            public DoOnTerminate(DoOnTerminateCompletable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnCompleted()
            {
                try
                {
                    parent.onTerminate();
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    Dispose();
                    return;
                }
                try { observer.OnCompleted(); } finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.onTerminate();
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }
                try { observer.OnError(error); } finally { Dispose(); }
            }
        }
    }

    internal class DoOnSubscribeCompletable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action onSubscribe;

        public DoOnSubscribeCompletable(ICompletable source, Action onSubscribe)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onSubscribe = onSubscribe;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnSubscribe(this, observer, cancel).Run();
        }

        private class DoOnSubscribe : OperatorCompletableObserverBase
        {
            private readonly DoOnSubscribeCompletable<T> parent;

            public DoOnSubscribe(DoOnSubscribeCompletable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                try
                {
                    parent.onSubscribe();
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); }
                    finally { Dispose(); }
                    return Disposable.Empty;
                }

                return parent.source.Subscribe(this);
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); } finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); } finally { Dispose(); }
            }
        }
    }

    internal class DoOnCancelCompletable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCancel;

        public DoOnCancelCompletable(ICompletable source, Action onCancel)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onCancel = onCancel;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnCancel(this, observer, cancel).Run();
        }

        private class DoOnCancel : OperatorCompletableObserverBase
        {
            private readonly DoOnCancelCompletable<T> parent;
            private bool isCompletedCall;

            public DoOnCancel(DoOnCancelCompletable<T> parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return StableCompositeDisposable.Create(parent.source.Subscribe(this), Disposable.Create(() =>
                {
                    if (!isCompletedCall)
                    {
                        parent.onCancel();
                    }
                }));
            }

            public override void OnCompleted()
            {
                isCompletedCall = true;
                try { observer.OnCompleted(); } finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                isCompletedCall = true;
                try { observer.OnError(error); } finally { Dispose(); }
            }
        }
    }
}