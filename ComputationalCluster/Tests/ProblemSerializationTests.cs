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
    public class ProblemSerializationTests
    {
        private readonly ProblemSerializer _serializer = new ProblemSerializer();

        [TestMethod]
        public void DvrpProblemInstanceSerializationTest()
        {
            var problem = new DVRPProblemInstance();
            problem.Depots.Add(new Depot() { Id = 5, EarliestDepartureTime = 123 });
            problem.Visits.Add(new Visit() { Location = new Location() {X = 5, Y = 5} });
            string xml = _serializer.ToXmlString(problem);
            DVRPProblemInstance p = (DVRPProblemInstance) _serializer.FromXmlString(xml);
            Assert.AreEqual(problem.Depots.First().Id, p.Depots.First().Id);
            Assert.AreEqual(problem.Visits.First().Location.X, p.Visits.First().Location.X);
            Assert.AreEqual(problem.Visits.First().Location.Y, p.Visits.First().Location.Y);
        }

        [TestMethod]
        public void DvrpPartialProblemInstanceSerializationTest()
        {
            var problem = new DVRPPartialProblemInstance();
            problem.SolutionResult = SolutionResult.Successful;
            problem.PartialResult = 123;
            problem.VisitIds = new int[][]
            {
                new int[] {1,3,5,7,9},
                new int[] {0,2,4,6},
                new int[] {11,22}
            };
            string xml = _serializer.ToXmlString(problem);
            DVRPPartialProblemInstance p = (DVRPPartialProblemInstance)_serializer.FromXmlString(xml);
            Assert.AreEqual(problem.SolutionResult, p.SolutionResult);
            Assert.AreEqual(problem.PartialResult, p.PartialResult);
            Assert.AreEqual(problem.VisitIds[2][1], p.VisitIds[2][1]);
        }
    }
}
