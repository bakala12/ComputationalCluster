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

            Assert.AreEqual(1, divideProblem.Length);
            //solving
            var solveProblem1 = taskSolver.Solve(divideProblem[0], TimeSpan.Zero);

            var finalSol = taskSolver.MergeSolution(new[] { solveProblem1 });

            //asercje necessary, jutro moze to zrobie
        }
        // najmniejsza z proponowanych przez O.
        [TestMethod]
        public void DvrpAlgorithmTest()
        {
            var parser = new DVRPProblemParser("io2_8_plain_a_D.vrp");
            var problem = parser.Parse();
            var converter = new ProblemToBytesConverter();
            var data = converter.ToByteArray(problem);
            var solver = new DvrpTaskSolver(data);

            var divides = solver.DivideProblem(0);
            var solutions = divides.Select(x => solver.Solve(x, TimeSpan.Zero)).ToArray();
            var final = solver.MergeSolution(solutions);
            var finalObj = (DVRPPartialProblemInstance) converter.FromBytesArray(final);
            Assert.AreEqual(723.39, Round(finalObj.PartialResult, 2), 2f );
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
                VehicleSpeed = 3,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(2, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(Round(finalSolution.PartialResult, 2), 422.03, 1f);
            var expected = new[]
            {
                new [] {1,2,3,4},
                new int[] {},
                new int[] {},
                new int[] {},
            };
            for (var j = 0; j < finalSolution.VisitIds.GetLength(0); j++)
            {
                Assert.IsTrue(
                    expected.Any(x =>
                    {
                        if (x.Length != finalSolution.VisitIds[j].Length)
                            return false;

                        for (var i = 0; i < x.Length; i++)
                        {
                            if (x[i] != finalSolution.VisitIds[j][i])
                                return false;
                        }
                        return true;
                    })
                    );
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

            Assert.AreEqual(2, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(567.62, Round(finalSolution.PartialResult, 2), 3.5f);
            var expected = new[]
            {
                new [] {1,2},
                new int[] {3,4},
                new int[] {},
                new int[] {}
            };
            for (int j = 0; j < finalSolution.VisitIds.GetLength(0); j++)
            {
                Assert.IsTrue(
                    expected.Any(x =>
                    {
                        if (x.Length != finalSolution.VisitIds[j].Length)
                            return false;

                        for (var i = 0; i < x.Length; i++)
                        {
                            if (x[i] != finalSolution.VisitIds[j][i])
                                return false;
                        }
                        return true;
                    })
                    );
            }

        }
        //http://pastebin.com/Xmjiz20Y
        [TestMethod]
        public void DvrpAlgorithmTest_ForAnotherProblem()
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
                    X = -67,
                    Y = 14
                },
                new Location
                {
                    Id = 2,
                    X = 94,
                    Y = 14
                },
                new Location
                {
                    Id = 3,
                    X = -29,
                    Y = -19
                },
                new Location
                {
                    Id = 4,
                    X = 43,
                    Y = 24
                }
            };
            // 4 visits
            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 196,
                    Demand = -48,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 443,
                    Demand = -16,
                    Duration = 20,
                    Id = 2,
                    Location = locationsArray[2]
                },
                new Visit
                {
                    AvailabilityTime = 180,
                    Demand = -35,
                    Duration = 20,
                    Id = 3,
                    Location = locationsArray[3]
                },
                new Visit
                {
                    AvailabilityTime = 15,
                    Demand = -37,
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
                VehicleSpeed = 7,
                Visits = visits
            };
            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(2, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(Round(finalSolution.PartialResult, 2), 349.70, 2.5f);
            

        }

        [TestMethod]
        public void DvrpDistantGroupsOfVisits()
        {
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
                    X = 115,
                    Y = 115
                },
                new Location
                {
                    Id = 2,
                    X = 120,
                    Y = 120
                },
                new Location
                {
                    Id = 3,
                    X = -100,
                    Y = -100
                },
                new Location
                {
                    Id = 4,
                    X = -101,
                    Y = -101
                }
            };
            var visitsArray = new[]
           {
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -10,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -10,
                    Duration = 20,
                    Id = 2,
                    Location = locationsArray[2]
                },
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -10,
                    Duration = 20,
                    Id = 3,
                    Location = locationsArray[3]
                },
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -10,
                    Duration = 20,
                    Id = 4,
                    Location = locationsArray[4]
                }
            };

            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 390
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 2;
            const int vehicleSpeed = 1;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                VehicleSpeed = vehicleSpeed,
                Visits = visits
            };

            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(3, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(2, finalSolution.VisitIds.Count(x => x.Length > 0));
            Assert.AreEqual(625.08, Round(finalSolution.PartialResult,2));
            Assert.IsTrue(finalSolution.VisitIds.Any(x=> x.Contains(3) && x.Contains(4)));
        }

        [TestMethod]
        public void DvrpTooBigDemandsTest()
        {
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
                    X = 3,
                    Y = 3
                }
            };

            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 9999
            };

            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -101,
                    Duration = 20,
                    Id = 1,
                    Location = locationsArray[1]
                }
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 1;


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

            Assert.AreEqual(2, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Impossible);

        }

        [TestMethod]
        public void DvrpTooFarAwayTest()
        {
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
                    X = 8,
                    Y = 6
                }
            };

            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 20
            };

            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -10,
                    Duration = 10,
                    Id = 1,
                    Location = locationsArray[1]
                }
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 1;
            const int vehicleSpeed = 1;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits,
                VehicleSpeed = vehicleSpeed
            };

            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(2, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Impossible);
        }

        [TestMethod]
        public void DvrpReturnToDepotTest()
        {
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
                    X = 8,
                    Y = 6
                },
                new Location
                {
                    Id = 2,
                    X = -8,
                    Y = -6
                }
            };

            var depot = new Depot
            {
                Id = 0,
                Location = locationsArray[0],
                EarliestDepartureTime = 0,
                LatestReturnTime = 9999
            };

            var visitsArray = new[]
            {
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -99,
                    Duration = 10,
                    Id = 1,
                    Location = locationsArray[1]
                },
                new Visit
                {
                    AvailabilityTime = 0,
                    Demand = -99,
                    Duration = 10,
                    Id = 2,
                    Location = locationsArray[2]
                }
            };

            var depots = new List<Depot> { depot };
            var locations = new List<Location>(locationsArray);
            var visits = new List<Visit>(visitsArray);
            const int vehicleCap = 100;
            const int vehicleNumber = 1;
            const int vehicleSpeed = 100;


            var problem = new DVRPProblemInstance
            {
                Depots = depots,
                Locations = locations,
                VehicleCapacity = vehicleCap,
                VehicleNumber = vehicleNumber,
                Visits = visits,
                VehicleSpeed = vehicleSpeed
            };

            var converter = new ProblemToBytesConverter();
            var bytes = converter.ToByteArray(problem);
            var taskSolver = new DvrpTaskSolver(bytes);
            var divideProblem = taskSolver.DivideProblem(0);
            var partialProblems = divideProblem.Select(partialProblem => (DVRPPartialProblemInstance)converter.FromBytesArray(partialProblem)).ToList();

            Assert.AreEqual(3, partialProblems.Count);
            var solvePartialProblem = new ConcurrentQueue<byte[]>();
            Parallel.ForEach(divideProblem, element =>
            {
                solvePartialProblem.Enqueue(taskSolver.Solve(element, TimeSpan.Zero));
            });

            var finalSolutionBytes = taskSolver.MergeSolution(solvePartialProblem.ToArray());

            var finalSolution = (DVRPPartialProblemInstance)converter.FromBytesArray(finalSolutionBytes);
            Assert.AreEqual(finalSolution.SolutionResult, SolutionResult.Successful);
            Assert.AreEqual(40, finalSolution.PartialResult, 0.2f);
        }
    }
}
