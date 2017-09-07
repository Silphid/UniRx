using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace UniRx.Completables.Tests
{
    [TestClass] [TestFixture]
    public class TimerTest
    {
        private TestScheduler _scheduler;
        private ICompletable _completable;
        private StubCompletableObserver _observer;

        [TestInitialize]
        [SetUp]
        public void SetUp()
        {
            _scheduler = new TestScheduler();
            _observer = new StubCompletableObserver();
        }
        
        [TestMethod]
        [Test]
        public void Timer_TimeSpan()
        {
            _completable = Completable.Timer(TimeSpan.FromTicks(10), _scheduler);

            AssertCompletionAfterTenTicks();
        }

        [TestMethod]
        [Test]
        public void Timer_DateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = Completable.Timer(dueTime, _scheduler);

            AssertCompletionAfterTenTicks();
        }

        private void AssertCompletionAfterTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.HasError.IsFalse();

            _scheduler.AdvanceBy(1);
            _observer.IsCompleted.IsTrue();
            _observer.HasError.IsFalse();
        }

        [TestMethod]
        [Test]
        public void ThenTimer_IntegrationInASequence()
        {
            var subject1 = new CompletableSubject();
            var subject2 = new CompletableSubject();

            _completable = subject1.ThenTimer(TimeSpan.FromTicks(10), _scheduler).Then(subject2);
            _scheduler.AdvanceBy(1000);

            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsFalse();

            _completable.Subscribe(_observer);
            _scheduler.AdvanceBy(1000);
            subject1.HasObservers.IsTrue();
            subject2.HasObservers.IsFalse();
            _observer.IsCompleted.IsFalse();
            
            // Complete first subject and elapse 9 ticks of timer (should start the timer but not complete it).
            subject1.OnCompleted();
            _scheduler.AdvanceBy(9);
            subject2.HasObservers.IsFalse();
            _observer.IsCompleted.IsFalse();

            // Advancing by one more tick should complete the timer and allow second subject to be subscribed to.
            _scheduler.AdvanceBy(1);
            subject2.HasObservers.IsTrue();
            _observer.IsCompleted.IsFalse();
            
            // Completing second subject should complete the whole sequence.
            subject2.OnCompleted();
            _observer.IsCompleted.IsTrue();
        }
    }
}