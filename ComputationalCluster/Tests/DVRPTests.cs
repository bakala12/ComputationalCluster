using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlgorithmSolvers.DVRPEssentials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Math;
namespace Tests
{
    [TestClass]
    public class DvrpTests
    {
        [TestMethod]
        public void DvrpSimpleProblemTest()
        {
            var loc1 = new Location { Id = 1, X = 1, Y = 2 };
            var loc2 = new Location { Id = 2, X = 3, Y = 4 };
            var visit1 = new Visit
            {
                AvailabilityTime = 0,
                Demand = 3,
                Duration = 1,
                Id = 1,
                Location = loc2
            };
            var depot1 = new Depot
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


            var instance = new DVRPProblemInstance
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
                partialProblems.Add((DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem));
            }

            Assert.AreEqual(2, partialProblems.Count);
            //solving
            var solveProblem1 = taskSolver.Solve(divideProblem[0], TimeSpan.Zero);
            var solveProblem2 = taskSolver.Solve(divideProblem[1], TimeSpan.Zero);

            var finalSol = taskSolver.MergeSolution(new[] { solveProblem1, solveProblem2 });

            //asercje necessary, jutro moze to zrobie
        }
        // najmniejsza z proponowanych przez O.
        [ExpectedException(typeof(OutOfMemoryException))] // so far
        [TestMethod]
        public void DvrpAlgorithmTest()
        {
            // 13 locations
            var locationsArray = new[]
            {
                new Location
                {
                    Id = 0,
                    X = 0,
                    Y = 0
                },
                new Location
                {
                    Id = 1,
                    X = -55,
                    Y = -26
                },
                new Location
                {
                    Id = 2,
                    X = -24,
                    Y = 38
                },
                new Location
                {
                    Id = 3,
                    X = -99,
                    Y = -29
                },
                new Location
                {
                    Id = 4,
                    X = -42,
                    Y = 30
                },
                new Location
                {
                    Id = 5,
                    X = 59,
                    Y = 66
                },
                new Location
                {
                    Id = 6,
                    X = 55,
                    Y = -35
                },
                new Location
                {
                    Id = 7,
                    X = -42,
                    Y = 3
                },
                new Location
                {
                    Id = 8,
                    X = 95,
                    Y = 13
                },
                new Location
                {
                    Id = 9,
                    X = 71,
                    Y = -90
                },
                new Location
                {
                    Id = 10,
                    X = 38,
                    Y = 32
                },
                new Location
                {
                    Id = 11,
                    X = 67,
                    Y = -22
                },
                new Location
                {
                    Id = 12,
                    X = 58,
                    Y = -97
                },
            };

            // 12 visits
            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 616,
                    Demand = -48,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 91,
                    Demand = -20,
                    Duration = 20,
                    Id = 2,
                    Location = locationsArray[2]
                },
                new Visit
                {
                    AvailabilityTime = 240,
                    Demand = -45,
                    Duration = 20,
                    Id = 3,
                    Location = locationsArray[3]
                },
                new Visit
                {
                    AvailabilityTime = 356,
                    Demand = -19,
                    Duration = 20,
                    Id = 4,
                    Location = locationsArray[4]
                },
                new Visit
                {
                    AvailabilityTime = 528,
                    Demand = -32,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[5]
                },
                new Visit
                {
                    AvailabilityTime = 459,
                    Demand = -42,
                    Duration = 20,
                    Id = 6,
                    Location = locationsArray[6]
                },
                new Visit
                {
                    AvailabilityTime = 433,
                    Demand = -19,
                    Duration = 20,
                    Id = 7,
                    Location = locationsArray[7]
                },
                new Visit
                {
                    AvailabilityTime = 513,
                    Demand = -35,
                    Duration = 20,
                    Id = 8,
                    Location = locationsArray[8]
                },
                new Visit
                {
                    AvailabilityTime = 444,
                    Demand = -30,
                    Duration = 20,
                    Id = 9,
                    Location = locationsArray[9]
                },
                new Visit
                {
                    AvailabilityTime = 44,
                    Demand = -26,
                    Duration = 20,
                    Id = 10,
                    Location = locationsArray[10]
                },
                new Visit
                {
                    AvailabilityTime = 318,
                    Demand = -41,
                    Duration = 20,
                    Id = 11,
                    Location = locationsArray[11]
                },
                new Visit
                {
                    AvailabilityTime = 20,
                    Demand = -27,
                    Duration = 20,
                    Id = 12,
                    Location = locationsArray[12]
                },
            };

            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 640
            };

            var depots = new List<Depot> {depot};
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 12;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            // ReSharper disable once UnusedVariable
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance) converter.FromBytesArray(partialProblem)).ToList();

            // TODO
        }

        // http://pastebin.com/LfnBUJVi
        // executes for about 15 sec
        [TestMethod]
        public void DvrpAlgorithmTest_ForVeryTinyProblemSample()
        {
            // 5 locations
            var locationsArray = new[]
            {
                new Location
                {
                    Id = 0,
                    X = 0,
                    Y = 0
                },
                new Location
                {
                    Id = 1,
                    X = -83,
                    Y = 99
                },
                new Location
                {
                    Id = 2,
                    X = -64,
                    Y = 94
                },
                new Location
                {
                    Id = 3,
                    X = 32,
                    Y = -49
                },
                new Location
                {
                    Id = 4,
                    X = -99,
                    Y = 34
                }
            };
            // 4 visits
            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 265,
                    Demand = -31,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 314,
                    Demand = -24,
                    Duration = 20,
                    Id = 2,
                    Location = locationsArray[2]
                },
                new Visit
                {
                    AvailabilityTime = 435,
                    Demand = -17,
                    Duration = 20,
                    Id = 3,
                    Location = locationsArray[3]
                },
                new Visit
                {
                    AvailabilityTime = 238,
                    Demand = -24,
                    Duration = 20,
                    Id = 4,
                    Location = locationsArray[4]
                }
            };
            // 1 depot
            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 480
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 4;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(256, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(Round(finalSolution.PartialResult, 5), 422.0288);
            var expected = new[]
            {
                new [] {3},
                new int[] {},
                new int[] {2,1,4},
                new int[] {},
            };
            for(var j=0; j< finalSolution.VisitIds.GetLength(0); j++)
            {
                for (var i = 0; i < finalSolution.VisitIds[j].Length; i++)
                {
                    Assert.AreEqual(finalSolution.VisitIds[j][i], expected[j][i]);
                }
            }

        }

        // http://pastebin.com/LqVe84pE
        // executes for about 15 sec
        [TestMethod]
        public void DvrpAlgorithmTest_ForAnotherProblemSample()
        {
            // 5 locations
            var locationsArray = new[]
            {
                new Location
                {
                    Id = 0,
                    X = 0,
                    Y = 0
                },
                new Location
                {
                    Id = 1,
                    X = 57,
                    Y = 82
                },
                new Location
                {
                    Id = 2,
                    X = -95,
                    Y = 36
                },
                new Location
                {
                    Id = 3,
                    X = -20,
                    Y = -78
                },
                new Location
                {
                    Id = 4,
                    X = 40,
                    Y = -32
                }
            };
            // 4 visits
            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 37,
                    Demand = -39,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 192,
                    Demand = -17,
                    Duration = 20,
                    Id = 2,
                    Location = locationsArray[2]
                },
                new Visit
                {
                    AvailabilityTime = 243,
                    Demand = -18,
                    Duration = 20,
                    Id = 3,
                    Location = locationsArray[3]
                },
                new Visit
                {
                    AvailabilityTime = 151,
                    Demand = -20,
                    Duration = 20,
                    Id = 4,
                    Location = locationsArray[4]
                }
            };
            // 1 depot
            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 480
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 4;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(256, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(Round(finalSolution.PartialResult, 5), 531.78848);
            var expected = new[]
            {
                new [] {2,1,4,3},
                new int[] {},
                new int[] {},
                new int[] {}
            };
            for (var j = 0; j < finalSolution.VisitIds.GetLength(0); j++)
            {
                for (var i = 0; i < finalSolution.VisitIds[j].Length; i++)
                {
                    Assert.AreEqual(finalSolution.VisitIds[j][i], expected[j][i]);
                }
            }

        }
    }
}
