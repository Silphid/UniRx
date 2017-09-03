using System;

namespace UniRx.Completables.Operators
{
    // DoOnError, DoOnCompleted, DoOnTerminate, DoOnSubscribe, DoOnCancel

    internal class DoCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action onCompleted;
        private readonly Action<Exception> onError;

        public DoCompletable(ICompletable source, Action onCompleted, Action<Exception> onError)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onCompleted = onCompleted;
            this.onError = onError;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new Do(this, observer, cancel).Run();
        }

        private class Do : OperatorCompletableObserverBase
        {
            private readonly DoCompletable parent;

            public Do(DoCompletable parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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
                
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.onError(error);
                }
                catch (Exception ex)
                {
                    try
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }
                    return;
                }

                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }

    internal class DoObserverCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly ICompletableObserver observer;

        public DoObserverCompletable(ICompletable source, ICompletableObserver observer)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.observer = observer;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer1, IDisposable cancel)
        {
            return new Do(this, observer1, cancel).Run();
        }

        private class Do : OperatorCompletableObserverBase
        {
            private readonly DoObserverCompletable parent;

            public Do(DoObserverCompletable parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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
                    parent.observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }

                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.observer.OnError(error);
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

    internal class DoOnErrorObservable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action<Exception> onError;

        public DoOnErrorObservable(ICompletable source, Action<Exception> onError)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.onError = onError;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new DoOnError(this, observer, cancel).Run();
        }

        private class DoOnError : OperatorCompletableObserverBase
        {
            readonly DoOnErrorObservable<T> parent;

            public DoOnError(DoOnErrorObservable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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

    internal class DoOnCompletedObservable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCompleted;

        public DoOnCompletedObservable(ICompletable source, Action onCompleted)
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
            private readonly DoOnCompletedObservable<T> parent;

            public DoOnCompleted(DoOnCompletedObservable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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

    internal class DoOnTerminateObservable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onTerminate;

        public DoOnTerminateObservable(ICompletable source, Action onTerminate)
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
            private readonly DoOnTerminateObservable<T> parent;

            public DoOnTerminate(DoOnTerminateObservable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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

    internal class DoOnSubscribeObservable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action onSubscribe;

        public DoOnSubscribeObservable(ICompletable source, Action onSubscribe)
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
            private readonly DoOnSubscribeObservable<T> parent;

            public DoOnSubscribe(DoOnSubscribeObservable<T> parent, ICompletableObserver observer, IDisposable cancel) : base(observer, cancel)
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

    internal class DoOnCancelObservable<T> : OperatorCompletableBase
    {
        private readonly ICompletable source;
        public readonly Action onCancel;

        public DoOnCancelObservable(ICompletable source, Action onCancel)
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
            private readonly DoOnCancelObservable<T> parent;
            private bool isCompletedCall;

            public DoOnCancel(DoOnCancelObservable<T> parent, ICompletableObserver observer, IDisposable cancel)
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