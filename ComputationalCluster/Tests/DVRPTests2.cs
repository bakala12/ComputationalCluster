using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlgorithmSolvers.DVRPEssentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UCCTaskSolver;

namespace Tests
{
    [TestClass]
    public class DVRPTests2
    {
        ProblemToBytesConverter _converter = new ProblemToBytesConverter();

        [TestMethod]
        public void ProblemTest1()
        {
            DVRPProblemInstance instance = new DVRPProblemInstance()
            {
                Depots = new List<Depot>()
                {
                    new Depot()
                    {
                        Id = 1,
                        Location = new Location() {Id = 1, X=0, Y=0},
                        EarliestDepartureTime = 0,
                        LatestReturnTime = 440
                    } 
                },
                Locations = new List<Location>()
                {
                    new Location() {Id = 1, X=1, Y=-29},
                    new Location() {Id = 2, X = 2, Y=-7}
                },
                VehicleCapacity = 100,
                VehicleNumber = 1,
                Visits = new List<Visit>()
            };
            byte[] bytes = _converter.ToByteArray(instance);
            TaskSolver solver = new DvrpTaskSolver(bytes);
            var divideProblem = solver.DivideProblem(0);
            List<byte[]> results = divideProblem.Select(b => solver.Solve(b, TimeSpan.Zero)).ToList();
            var final = _converter.FromBytesArray(solver.MergeSolution(results.ToArray()));
            Assert.IsNotNull(final);
            Assert.IsInstanceOfType(final, typeof(DVRPPartialProblemInstance));
            Assert.AreEqual(SolutionResult.Successful, ((DVRPPartialProblemInstance)final).SolutionResult);
            Assert.AreEqual(263.78, ((DVRPPartialProblemInstance)final).PartialResult);
        }
    }
}
