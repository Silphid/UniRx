using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace UniRx.Completables.Tests
{
    [TestClass]
    [TestFixture]
    public class CatchTest
    {
        [TestMethod]
        [Test]
        public void CatchWithMoreSpecificException_ShouldNotCatchButCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new Exception();
            
            var subject = new CompletableSubject();
            subject
                .Catch<InvalidOperationException>(ex =>
                {
                    Assert.Fail("Should not be called");
                    return Completable.Empty();
                })
                .Subscribe(ex => receivedException = ex);
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [TestMethod]
        [Test]
        public void CatchWithMoreGenericException_ShouldCatchButNotCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new InvalidOperationException();
            
            var subject = new CompletableSubject();
            subject
                .Catch<Exception>(ex =>
                {
                    receivedException = ex;
                    return Completable.Empty();
                })
                .Subscribe(ex => Assert.Fail("Should not be called"));
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [TestMethod]
        [Test]
        public void CatchIgnoreWithHandler_ShouldCatchButNotCallOnError()
        {
            Exception receivedException = null;
            var emittedException = new InvalidOperationException();
            
            var subject = new CompletableSubject();
            subject
                .CatchIgnore<Exception>(ex =>
                {
                    receivedException = ex;
                })
                .Subscribe(ex => Assert.Fail("Should not be called"));
            
            subject.OnError(emittedException);
            receivedException.IsSameReferenceAs(emittedException);
        }

        [TestMethod]
        [Test]
        public void CatchIgnoreWithoutHandler_ShouldNotCallOnError()
        {
            bool onCompletedCalled = false;
            
            var subject = new CompletableSubject();
            subject
                .CatchIgnore<Exception>()
                .Subscribe(() => onCompletedCalled = true, ex => Assert.Fail("Should not be called"));
            
            subject.OnError(new Exception());
            onCompletedCalled.IsTrue();
        }
    }
}