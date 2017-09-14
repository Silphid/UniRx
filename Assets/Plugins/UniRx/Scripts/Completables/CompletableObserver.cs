using System;
using System.Threading;
using UniRx.Completables.InternalUtil;
using UniRx.Completables.Operators;

namespace UniRx.Completables
{
    public static class CompletableObserver
    {
        internal static ICompletableObserver CreateSubscribeObserver(Action onCompleted, Action<Exception> onError)
        {
            return new Subscribe(onCompleted, onError);
        }

        internal static ICompletableObserver CreateSubscribeWithStateObserver<TState>(TState state, Action<Exception, TState> onError, Action<TState> onCompleted)
        {
            return new Subscribe<TState>(state, onCompleted, onError);
        }

        internal static ICompletableObserver CreateSubscribeWithState2Observer<TState1, TState2>(TState1 state1, TState2 state2, Action<Exception, TState1, TState2> onError, Action<TState1, TState2> onCompleted)
        {
            return new Subscribe<TState1, TState2>(state1, state2, onCompleted, onError);
        }

        internal static ICompletableObserver CreateSubscribeWithState3Observer<TState1, TState2, TState3>(TState1 state1, TState2 state2, TState3 state3, Action<Exception, TState1, TState2, TState3> onError, Action<TState1, TState2, TState3> onCompleted)
        {
            return new Subscribe<TState1, TState2, TState3>(state1, state2, state3, onCompleted, onError);
        }

        public static ICompletableObserver Create()
        {
            return Create(Stubs.Throw, Stubs.Nop);
        }

        public static ICompletableObserver Create(Action<Exception> onError)
        {
            return Create(onError, Stubs.Nop);
        }

        public static ICompletableObserver Create(Action onCompleted)
        {
            return Create(Stubs.Throw, onCompleted);
        }

        public static ICompletableObserver Create(Action<Exception> onError, Action onCompleted)
        {
            return new AnonymousCompletableObserver(onError, onCompleted);
        }

        public static ICompletableObserver CreateAutoDetachObserver(ICompletableObserver observer, IDisposable disposable)
        {
            return new AutoDetachCompletableObserver(observer, disposable);
        }

        private class AnonymousCompletableObserver : ICompletableObserver
        {
            private readonly Action onCompleted;
            private readonly Action<Exception> onError;

            private int isStopped;

            public AnonymousCompletableObserver(Action<Exception> onError, Action onCompleted)
            {
                this.onCompleted = onCompleted;
                this.onError = onError;
            }

            public void OnCompleted()
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onCompleted();
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onError(error);
                }
            }
        }

            // same as AnonymousObserver...
        private class Subscribe : ICompletableObserver
        {
            private readonly Action onCompleted;
            private readonly Action<Exception> onError;

            private int isStopped;

            public Subscribe(Action onCompleted, Action<Exception> onError)
            {
                this.onCompleted = onCompleted;
                this.onError = onError;
            }

            public void OnCompleted()
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onCompleted();
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onError(error);
                }
            }
        }

        // with state
        private class Subscribe<TState> : ICompletableObserver
        {
            private readonly TState state;
            private readonly Action<TState> onCompleted;
            private readonly Action<Exception, TState> onError;

            private int isStopped;

            public Subscribe(TState state, Action<TState> onCompleted, Action<Exception, TState> onError)
            {
                this.state = state;
                this.onCompleted = onCompleted;
                this.onError = onError;
            }

            public void OnCompleted()
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onCompleted(state);
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onError(error, state);
                }
            }
        }

        private class Subscribe<TState1, TState2> : ICompletableObserver
        {
            private readonly TState1 state1;
            private readonly TState2 state2;
            private readonly Action<TState1, TState2> onCompleted;
            private readonly Action<Exception, TState1, TState2> onError;

            private int isStopped;

            public Subscribe(TState1 state1, TState2 state2, Action<TState1, TState2> onCompleted, Action<Exception, TState1, TState2> onError)
            {
                this.state1 = state1;
                this.state2 = state2;
                this.onCompleted = onCompleted;
                this.onError = onError;
            }

            public void OnCompleted()
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onCompleted(state1, state2);
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onError(error, state1, state2);
                }
            }
        }

        private class Subscribe<TState1, TState2, TState3> : ICompletableObserver
        {
            private readonly TState1 state1;
            private readonly TState2 state2;
            private readonly TState3 state3;
            private readonly Action<TState1, TState2, TState3> onCompleted;
            private readonly Action<Exception, TState1, TState2, TState3> onError;

            private int isStopped;

            public Subscribe(TState1 state1, TState2 state2, TState3 state3, Action<TState1, TState2, TState3> onCompleted, Action<Exception, TState1, TState2, TState3> onError)
            {
                this.state1 = state1;
                this.state2 = state2;
                this.state3 = state3;
                this.onCompleted = onCompleted;
                this.onError = onError;
            }

            public void OnCompleted()
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onCompleted(state1, state2, state3);
                }
            }

            public void OnError(Exception error)
            {
                if (Interlocked.Increment(ref isStopped) == 1)
                {
                    onError(error, state1, state2, state3);
                }
            }
        }

        private class AutoDetachCompletableObserver : OperatorCompletableObserverBase
        {
            public AutoDetachCompletableObserver(ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }
        }

        public static ICompletableObserver Synchronize(this ICompletableObserver observer)
        {
            return new SynchronizedCompletableObserver(observer, new object());
        }

        public static ICompletableObserver Synchronize(this ICompletableObserver observer, object gate)
        {
            return new SynchronizedCompletableObserver(observer, gate);
        }
    }

    public static class CompletableExtensions
    {
        public static IDisposable Subscribe(this ICompletable source)
        {
            return source.Subscribe(ThrowCompletableObserver.Instance);
        }

        public static IDisposable Subscribe(this ICompletable source, Action onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(onCompleted, Stubs.Throw));
        }

        public static IDisposable Subscribe(this ICompletable source, Action<Exception> onError, Action onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(onCompleted, onError));
        }

        public static IDisposable Subscribe(this ICompletable source, Action<Exception> onError)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeObserver(Stubs.Nop, onError));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source, TState state, Action<TState> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithStateObserver(state, Stubs<TState>.Throw, onCompleted));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source, TState state, Action<Exception, TState> onError)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithStateObserver(state, onError, Stubs<TState>.Ignore));
        }

        public static IDisposable SubscribeWithState<TState>(this ICompletable source, TState state, Action<Exception, TState> onError, Action<TState> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithStateObserver(state, onError, onCompleted));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source, TState1 state1, TState2 state2, Action<TState1, TState2> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithState2Observer(state1, state2, Stubs<TState1, TState2>.Throw, onCompleted));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source, TState1 state1, TState2 state2, Action<Exception, TState1, TState2> onError)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithState2Observer(state1, state2, onError, Stubs<TState1, TState2>.Ignore));
        }

        public static IDisposable SubscribeWithState2<TState1, TState2>(this ICompletable source, TState1 state1, TState2 state2, Action<Exception, TState1, TState2> onError, Action<TState1, TState2> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithState2Observer(state1, state2, onError, onCompleted));
        }

        public static IDisposable SubscribeWithState3<TState1, TState2, TState3>(this ICompletable source, TState1 state1, TState2 state2, TState3 state3, Action<TState1, TState2, TState3> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithState3Observer(state1, state2, state3, Stubs<TState1, TState2, TState3>.Throw, onCompleted));
        }

        public static IDisposable SubscribeWithState3<TState1, TState2, TState3>(this ICompletable source, TState1 state1, TState2 state2, TState3 state3, Action<Exception, TState1, TState2, TState3> onError, Action<TState1, TState2, TState3> onCompleted)
        {
            return source.Subscribe(CompletableObserver.CreateSubscribeWithState3Observer(state1, state2, state3, onError, onCompleted));
        }
    }

    internal static class Stubs
    {
        public static readonly Action Nop = () => { };
        public static readonly Action<Exception> Throw = ex => { throw ex; };
            
        // Marker for CatchIgnore and Catch to avoid iOS AOT problem.
        public static ICompletable CatchIgnore(Exception ex)
        {
            return Completable.Empty();
        }
            
        public static void CatchIgnoreVoid(Exception ex)
        {
        }
    }
}