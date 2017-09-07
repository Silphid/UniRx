﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;

namespace UniRx.Completables.Tests
{
    [TestClass]
    [TestFixture]
    public class ThenTest
    {
        [TestMethod]
        [Test]
        public void Then_OnCompleted()
        {
            var subject1 = new CompletableSubject();
            var subject2 = new CompletableSubject();

            var completable = subject1.Then(subject2);

            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsFalse();

            var observer = new StubCompletableObserver();
            completable.Subscribe(observer);
                
            subject1.HasObservers.IsTrue();
            subject2.HasObservers.IsFalse();
            observer.IsCompleted.IsFalse();
            
            subject1.OnCompleted();
            
            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsTrue();
            observer.IsCompleted.IsFalse();
            
            subject2.OnCompleted();
           
            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsFalse();
            observer.IsCompleted.IsTrue();
        }
        
        [TestMethod]
        [Test]
        public void Then_OnError()
        {
            var subject1 = new CompletableSubject();
            var subject2 = new CompletableSubject();

            var completable = subject1.Then(subject2);

            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsFalse();

            var observer = new StubCompletableObserver();
            completable.Subscribe(observer);
                
            subject1.HasObservers.IsTrue();
            subject2.HasObservers.IsFalse();
            observer.IsCompleted.IsFalse();
            
            var exception = new Exception();
            subject1.OnError(exception);

            subject1.HasObservers.IsFalse();
            subject2.HasObservers.IsFalse();
            observer.IsCompleted.IsFalse();
            observer.Error.IsSameReferenceAs(exception);
        }
    }
}
