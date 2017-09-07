using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace UniRx.Completables.Tests
{
    [TestClass] [TestFixture]
    public class TimeoutTest
    {
        private CompletableSubject _subject;
        private TestScheduler _scheduler;
        private ICompletable _completable;
        private StubCompletableObserver _observer;

        [TestInitialize]
        [SetUp]
        public void SetUp()
        {
            _subject = new CompletableSubject();
            _scheduler = new TestScheduler();
            _observer = new StubCompletableObserver();
        }
        
        [TestMethod]
        [Test]
        public void TimesOutAfterTimeSpan()
        {
            _completable = _subject.Timeout(TimeSpan.FromTicks(10), _scheduler);

            AssertTimesOutAfterTenTicks();
        }

        [TestMethod]
        [Test]
        public void TimesOutAfterDateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = _subject.Timeout(dueTime, _scheduler);

            AssertTimesOutAfterTenTicks();
        }
        
        [TestMethod]
        [Test]
        public void DoesNotTimeOutIfCompletesBeforeTimeSpan()
        {
            _completable = _subject.Timeout(TimeSpan.FromTicks(10), _scheduler);

            AssertDoesNotTimeOutIfCompletesBeforeTenTicks();
        }

        [TestMethod]
        [Test]
        public void DoesNotTimeOutIfCompletesBeforeDateTimeOffset()
        {
            var dueTime = _scheduler.Now + TimeSpan.FromTicks(10);
            _completable = _subject.Timeout(dueTime, _scheduler);

            AssertDoesNotTimeOutIfCompletesBeforeTenTicks();
        }

        private void AssertTimesOutAfterTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsNull();

            _scheduler.AdvanceBy(1);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsInstanceOf<TimeoutException>();

            // Letting more time elapse should not affect anything
            _scheduler.AdvanceBy(1000);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsInstanceOf<TimeoutException>();
        }

        private void AssertDoesNotTimeOutIfCompletesBeforeTenTicks()
        {
            _completable.Subscribe(_observer);

            _scheduler.AdvanceBy(9);
            _observer.IsCompleted.IsFalse();
            _observer.Error.IsNull();

            _subject.OnCompleted();
            _observer.IsCompleted.IsTrue();
            _observer.Error.IsNull();

            // Letting more time elapse should not affect anything
            _scheduler.AdvanceBy(1000);
            _observer.IsCompleted.IsTrue();
            _observer.Error.IsNull();
        }
    }
}