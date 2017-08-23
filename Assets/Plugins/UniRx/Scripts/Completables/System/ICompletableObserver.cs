using System;

namespace UniRx.Completables
{
    public interface ICompletableObserver
    {
        void OnCompleted();
        void OnError(Exception error);
    }
}