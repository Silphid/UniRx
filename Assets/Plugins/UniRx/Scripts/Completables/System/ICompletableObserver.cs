using System;

namespace UniRx.Completables
{
    public interface ICompletableObserver
    {
        void OnError(Exception error);
        void OnCompleted();
    }
}