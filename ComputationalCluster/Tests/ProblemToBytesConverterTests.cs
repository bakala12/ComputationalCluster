using System;
using System.Collections.Generic;
using System.Configuration;
using AlgorithmSolvers.DVRPEssentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ProblemToBytesConverterTests
    {
        [TestMethod]
        public void SimpleParseTest()
        {
            var loc1 = new Location() {Id = 1, X = 1, Y = 2};
            var loc2 = new Location() { Id = 2, X = 3, Y = 4};
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
            var vehicleNumber = 10;


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
            var ret = (DVRPProblemInstance)converter.FromBytesArray(bytes);

            Assert.AreEqual(loc1.Id, ret.Locations[0].Id);
            Assert.AreEqual(loc1.X, ret.Locations[0].X);
            Assert.AreEqual(loc1.Y, ret.Locations[0].Y);
            Assert.AreEqual(loc2.Id, ret.Locations[1].Id);
            Assert.AreEqual(loc2.X, ret.Locations[1].X);
            Assert.AreEqual(loc2.Y, ret.Locations[1].Y);

            Assert.AreEqual(depot1.Id, ret.Depots[0].Id);
            Assert.AreEqual(depot1.EarliestDepartureTime, ret.Depots[0].EarliestDepartureTime);
            Assert.AreEqual(depot1.LatestReturnTime, ret.Depots[0].LatestReturnTime);
            Assert.AreEqual(depot1.Location.Id, ret.Depots[0].Location.Id);
            Assert.AreEqual(depot1.Location.X, ret.Depots[0].Location.X);
            Assert.AreEqual(depot1.Location.Y, ret.Depots[0].Location.Y);

            Assert.AreEqual(visit1.Id, ret.Visits[0].Id);
            Assert.AreEqual(visit1.AvailabilityTime, ret.Visits[0].AvailabilityTime);
            Assert.AreEqual(visit1.Demand, ret.Visits[0].Demand);
            Assert.AreEqual(visit1.Duration, ret.Visits[0].Duration);
            Assert.AreEqual(visit1.Location.Id, ret.Visits[0].Location.Id);
            Assert.AreEqual(visit1.Location.X, ret.Visits[0].Location.X);
            Assert.AreEqual(visit1.Location.Y, ret.Visits[0].Location.Y);

            Assert.AreEqual(ret.VehicleCapacity, vehicleCap);
            Assert.AreEqual(ret.VehicleNumber, vehicleNumber);

            Assert.AreEqual(1, ret.Visits.Count);
            Assert.AreEqual(2, ret.Locations.Count);
            Assert.AreEqual(1, ret.Depots.Count);
        }

        [TestMethod]
        public void PartialProblemParsingTest()
        {
            var visits = new int [3][];
            visits[0] = new int[1];
            visits[0][0] = 1;
            visits[1] = new int[1];
            visits[1][0] = 2;
            visits[2] = new int[1];
            visits[2][0] = 3;
            var instance = new DVRPPartialProblemInstance()
            {
                PartialResult = 0,
                SolutionResult = SolutionResult.NotSolved,
                VisitIds = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(instance);
            var ret = (DVRPPartialProblemInstance)converter.FromBytesArray(bytes);
            //assertions:
            Assert.AreEqual(visits.Length, ret.VisitIds.Length);
            Assert.AreEqual(visits[0].Length, ret.VisitIds[0].Length);
            Assert.AreEqual(visits[1].Length, ret.VisitIds[1].Length);
            Assert.AreEqual(visits[2].Length, ret.VisitIds[2].Length);
            Assert.AreEqual(visits[0][0], ret.VisitIds[0][0]);
            Assert.AreEqual(visits[1][0], ret.VisitIds[1][0]);
            Assert.AreEqual(visits[2][0], ret.VisitIds[2][0]);
            Assert.AreEqual(ret.PartialResult, 0);
            Assert.AreEqual(ret.SolutionResult, SolutionResult.NotSolved);
        }
    }
}
