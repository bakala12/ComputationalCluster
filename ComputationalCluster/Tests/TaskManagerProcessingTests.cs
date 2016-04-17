using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.ClientComponentCommon;
using TaskManager.Core;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class TaskManagerProcessingTests
    {
        [TestMethod]
        public void DivideProblemWrongProblemTypeTest()
        {
            //DivideProblem dp = new DivideProblem() { ProblemType = "ASDF" };
            //var tmp = new TaskManagerMessageProcessor(new List<string>() { "DEF" }, 
            //    new TaskManagerStorage());
            //var msg = tmp.DivideProblem(dp);
            //Assert.IsInstanceOfType(msg, typeof(Error));
        }

        [TestMethod]
        public void DivideProblemCorrectResponseTest ()
        {
            //DivideProblem dp = new DivideProblem() { ProblemType = "DEF", Id = 123 };
            //var tmp = new TaskManagerMessageProcessor(new List<string>() { "DEF" },
            //    new TaskManagerStorage());
            //var msg = tmp.DivideProblem(dp);
            //Assert.IsInstanceOfType(msg, typeof(SolvePartialProblems));
            //var msgc = msg.Cast<SolvePartialProblems>();
            //Assert.AreEqual((uint)123, msgc.Id);
        }

        [TestMethod]
        public void NotExistingTaskExceptionTest()
        {
            //Solutions sol = new Solutions()
            //{
            //    Id = 123,
            //    SolutionsList = new SolutionsSolution [] {
            //    new SolutionsSolution () { TaskId=777 } }
            //};

            //var tmp = new TaskManagerMessageProcessor(null, new TaskManagerStorage());
            //tmp.HandleSolutions(sol);
        }
    }
}
