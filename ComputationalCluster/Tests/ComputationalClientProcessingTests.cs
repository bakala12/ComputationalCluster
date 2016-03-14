using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Client.Core;

namespace Tests
{
    [TestClass]
    public class ComputationalClientProcessingTests
    {
       //this class will be extended in the second stage of project


        [TestMethod]
        public void GetRequestRelationsTest ()
        {
            var core = new ClientNodeProcessingModule(null, "abc");
            var request = core.GetRequest();
            Assert.AreEqual(core.Type, request.ProblemType);
            Assert.AreEqual(core.Type, "abc");
            Assert.IsTrue(request.SolvingTimeoutSpecified);
            Assert.IsTrue(request.SolvingTimeout > 0);
            //Assert.IsNotNull(request.Data);
            Assert.IsFalse(request.IdSpecified);
        }

        //GetRequestWithPreviouslyConfiguredProblemTest


    }
}
