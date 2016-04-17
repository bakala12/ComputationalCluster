using System;
using System.Collections.Generic;
using AlgorithmSolvers.DVRPEssentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DVRPTests
    {
        [TestMethod]
        public void DvrpSimpleProblemTest()
        {
            var loc1 = new Location() {Id = 1, X = 1, Y = 2};
            var loc2 = new Location() {Id = 2, X = 3, Y = 4};
            var visit1 = new Visit()
            {
                AvailabilityTime = 2,
                Demand = 3,
                Duration = 1,
                Id = 1,
                Location = loc2
            };
            var depot1 = new Depot()
            {
                EarliestDepartureTime = 0,
                Id = 1,
                LatestReturnTime = 10,
                Location = loc1
            };
            var depots = new List<Depot>();
            depots.Add(depot1);
            var locations = new List<Location>();
            locations.Add(loc1);
            locations.Add(loc2);
            var visits = new List<Visit>();
            visits.Add(visit1);
            var vehicleCap = 100;
            var vehicleNumber = 2;


            var instance = new DVRPProblemInstance()
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(instance);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = new List<DVRPPartialProblemInstance>();
            foreach (var partialProblem in divideProblem)
            {
                partialProblems.Add((DVRPPartialProblemInstance) converter.FromBytesArray(partialProblem));
            }

            Assert.AreEqual(2, partialProblems.Count);
            //coś może więcej...
        }
    }
}
