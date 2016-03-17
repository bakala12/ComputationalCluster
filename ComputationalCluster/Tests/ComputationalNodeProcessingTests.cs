using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;
using ComputationalNode.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ComputationalNodeProcessingTests
    {
        [TestMethod]
        public void ComputeSubtaskWrongProblemTypeTest()
        {
            SolvePartialProblems spp = new SolvePartialProblems() {ProblemType = "ABC"};
            var cnp = new ComputationalNodeProcessingModule(new List<string>() {"DVRP"});
            var msg = cnp.ComputeSubtask(spp);
            Assert.IsInstanceOfType(msg, typeof(Error));
        }

        [TestMethod]
        public void ComputationalNodeCorrectResponseTest()
        {
            SolvePartialProblems spp = new SolvePartialProblems() { ProblemType = "DEF", Id = 123 };
            var cnp = new ComputationalNodeProcessingModule(new List<string>() {"DEF"});
            var msg = cnp.ComputeSubtask(spp);
            Assert.IsInstanceOfType(msg, typeof(Solutions));
            var msgc = msg.Cast<Solutions>();
            Assert.AreEqual((uint)123, msgc.Id);
        }
    }
}
