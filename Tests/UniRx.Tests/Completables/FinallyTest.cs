using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace UniRx.Completables.Tests
{
    [TestClass]
    [TestFixture]
    public class FinallyTest
    {
        private bool finallyCalled;
        
        [TestInitialize]
        [SetUp]
        public void SetUp()
        {
            finallyCalled = false;
        }

        [TestMethod]
        [Test]
        public void OnComplete_CallsFinallyAndOnCompleted()
        {
            bool onCompletedCalled = false;
            var subject = new CompletableSubject();
            subject
                .Finally(() => finallyCalled = true)
                .Subscribe(() => onCompletedCalled = true);

            subject.OnCompleted();
            onCompletedCalled.IsTrue();
            finallyCalled.IsTrue();
        }
        
        [TestMethod]
        [Test]
        public void OnError_CallsFinallyAndOnError()
        {
            Exception receivedException = null;
            var emittedException = new Exception();
            var subject = new CompletableSubject();
            subject
                .Finally(() => finallyCalled = true)
                .Subscribe(ex => receivedException = ex);

            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
            finallyCalled.IsTrue();
        }
        
        [TestMethod]
        [Test]
        public void ExceptionDuringSubscribe_CallsFinallyAndRethrows()
        {
            Exception catchedException = null;
            var thrownException = new Exception();
            
            try
            {
                Completable
                    .Create(observer =>
                    {
                        throw thrownException;
                    })
                    .Finally(() => finallyCalled = true)
                    .Subscribe();
            }
            catch (Exception ex)
            {
                catchedException = ex;
            }
            
            catchedException.IsSameReferenceAs(thrownException);
            finallyCalled.IsTrue();
        }
    }
}