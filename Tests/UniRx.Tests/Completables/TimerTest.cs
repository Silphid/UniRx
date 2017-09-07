using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace UniRx.Completables.Tests
{
    [TestClass] [TestFixture]
    public class TimerTest
    {
        [TestMethod] [Test]
        public void Timer_TimeSpan()
        {
            var scheduler = new TestScheduler();
            var timer = Completable.Timer(TimeSpan.FromTicks(10), scheduler);

            AssertCompletion(timer, scheduler);
        }

        [TestMethod] [Test]
        public void Timer_DateTimeOffset()
        {
            var scheduler = new TestScheduler();
            var dueTime = scheduler.Now + TimeSpan.FromTicks(10);
            var timer = Completable.Timer(dueTime, scheduler);

            AssertCompletion(timer, scheduler);
        }

        private static void AssertCompletion(ICompletable timer, TestScheduler scheduler)
        {
            var observer = new StubCompletableObserver();
            timer.Subscribe(observer);

            scheduler.AdvanceBy(9);
            observer.IsCompleted.IsFalse();
            observer.HasError.IsFalse();

            scheduler.AdvanceBy(1);
            observer.IsCompleted.IsTrue();
            observer.HasError.IsFalse();
        }
    }
}