using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmSolvers.DVRPEssentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ProblemParserTests
    {
        [TestMethod]
        public void ProblemFromGeneratorParsingTest1()
        {
            var parser = new DVRPProblemParser("io2_4_plain_a_D.vrp");
            DVRPProblemInstance problem = parser.Parse();
            Assert.AreEqual(problem.Visits.Count, 4);
            Assert.AreEqual(problem.Visits[3].Location.Y, 24);
            Assert.AreEqual(problem.Visits.First().Duration, 20);
            Assert.AreEqual(problem.VehicleNumber, 4);
            Assert.AreEqual(problem.Depots.First().Location.X, 0);
        }
        [TestMethod]
        public void ProblemFromGeneratorParsingTest2()
        {
            var parser = new DVRPProblemParser("io2_5_plain_a_D.vrp");
            DVRPProblemInstance problem = parser.Parse();
            Assert.AreEqual(problem.Visits.Count, 5);
            Assert.AreEqual(problem.Visits[3].Location.Y, -10);
            Assert.AreEqual(problem.Visits.First().Duration, 20);
            Assert.AreEqual(problem.VehicleNumber, 5);
            Assert.AreEqual(problem.Depots.First().Location.X, 0);
        }
    }
}
