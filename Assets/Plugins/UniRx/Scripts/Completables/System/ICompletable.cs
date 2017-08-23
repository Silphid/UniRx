using System;

namespace UniRx.Completables
{
    public interface ICompletable
    {
        IDisposable Subscribe(ICompletableObserver observer);
    }
}