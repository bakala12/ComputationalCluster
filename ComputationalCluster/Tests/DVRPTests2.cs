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
        readonly ProblemToBytesConverter _converter = new ProblemToBytesConverter();

        [TestMethod]
        public void ProblemTest1()
        {
            #region Init
            DVRPProblemInstance instance = new DVRPProblemInstance()
            {
                Depots = new List<Depot>()
                {
                    new Depot()
                    {
                        Id = 0,
                        Location = new Location() {Id = 0, X=0, Y=0},
                        EarliestDepartureTime = 0,
                        LatestReturnTime = 500
                    }
                },
                Locations = new List<Location>()
                {
                    new Location() {Id=0, X = 0, Y=0},
                    new Location() {Id = 1, X=48, Y=-53},
                    new Location() {Id = 2, X = 63, Y=35},
                    new Location() {Id =3, X=-25, Y=22},
                    new Location() {Id=4, X=-33, Y=74 },
                    new Location() {Id=5, X = -23, Y=-17}
                },
                VehicleCapacity = 100,
                VehicleNumber = 5,
                Visits = new List<Visit>()
                {
                    new Visit()
                    {
                        AvailabilityTime = 343,
                        Demand = -11,
                        Duration = 20,
                        Id=1,
                        Location = new Location(){Id = 1, X=48, Y=-53}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 230,
                        Demand = -26,
                        Duration = 20,
                        Id=2,
                        Location = new Location(){Id = 2, X=63, Y=35}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 178,
                        Demand = -45,
                        Duration = 20,
                        Id=3,
                        Location = new Location(){Id = 3, X=-25, Y=22}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 488,
                        Demand = -28,
                        Duration = 20,
                        Id=4,
                        Location = new Location(){Id = 4, X=-33, Y=74}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 283,
                        Demand = -15,
                        Duration = 20,
                        Id=5,
                        Location = new Location(){Id = 5, X=-23, Y=-17}
                    },
                }
            };
            #endregion
            byte[] bytes = _converter.ToByteArray(instance);
            TaskSolver solver = new DVRPTaskSolver(bytes);
            var divideProblem = solver.DivideProblem(1);
            List<byte[]> results = divideProblem.Select(b => solver.Solve(b, TimeSpan.Zero)).ToList();
            var final = _converter.FromBytesArray(solver.MergeSolution(results.ToArray()));
            Assert.IsNotNull(final);
            Assert.IsInstanceOfType(final, typeof(DVRPPartialProblemInstance));
            //Assert.AreEqual(SolutionResult.Successful, ((DVRPPartialProblemInstance)final).SolutionResult);
            //Assert.AreEqual(434.13, ((DVRPPartialProblemInstance)final).PartialResult);
        }

        /// <summary>
        /// http://pastebin.com/AVsN2xAh
        /// Difference 2.5
        /// </summary>
        [TestMethod]
        public void ProblemTest2()
        {
            #region Init
            DVRPProblemInstance instance = new DVRPProblemInstance()
            {
                Depots = new List<Depot>()
                {
                    new Depot()
                    {
                        Id = 0,
                        Location = new Location() {Id = 0, X=0, Y=0},
                        EarliestDepartureTime = 0,
                        LatestReturnTime = 500
                    }
                },
                Locations = new List<Location>()
                {
                    new Location() {Id=0, X=0, Y=0 },
                    new Location() {Id = 1, X=14, Y=84},
                    new Location() {Id = 2, X = 60, Y=-76},
                    new Location() {Id =3, X=-69, Y=-32},
                    new Location() {Id=4, X=50, Y=-10 },
                    new Location() {Id=5, X = -30, Y=-57}
                },
                VehicleCapacity = 100,
                VehicleNumber = 5,
                VehicleSpeed = 1,
                Visits = new List<Visit>()
                {
                    new Visit()
                    {
                        AvailabilityTime = 100,
                        Demand = -15,
                        Duration = 20,
                        Id=1,
                        Location = new Location(){Id = 1, X=14, Y=85}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 55,
                        Demand = -32,
                        Duration = 20,
                        Id=2,
                        Location = new Location(){Id = 2, X=60, Y=-76}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 223,
                        Demand = -20,
                        Duration = 20,
                        Id=3,
                        Location = new Location(){Id = 3, X=-69, Y=-32}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 61,
                        Demand = -29,
                        Duration = 20,
                        Id=4,
                        Location = new Location(){Id = 4, X=50, Y=-10}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 409,
                        Demand = -48,
                        Duration = 20,
                        Id=5,
                        Location = new Location(){Id = 5, X=-30, Y=-57}
                    }
                }
            };
            #endregion
            byte[] bytes = _converter.ToByteArray(instance);
            TaskSolver solver = new DVRPTaskSolver(bytes);
            var divideProblem = solver.DivideProblem(1);
            List<byte[]> results = divideProblem.Select(b => solver.Solve(b, TimeSpan.Zero)).ToList();
            var final = _converter.FromBytesArray(solver.MergeSolution(results.ToArray()));
            Assert.IsNotNull(final);
            Assert.IsInstanceOfType(final, typeof(DVRPPartialProblemInstance));
            Assert.AreEqual(SolutionResult.Successful, ((DVRPPartialProblemInstance)final).SolutionResult);
            Assert.AreEqual(536.20, ((DVRPPartialProblemInstance)final).PartialResult,2.5);
        }

        /// <summary>
        /// http://pastebin.com/t1PPzV92
        /// </summary>
        [TestMethod]
        public void ProblemTest3()
        {
            #region Init
            DVRPProblemInstance instance = new DVRPProblemInstance()
            {
                Depots = new List<Depot>()
                {
                    new Depot()
                    {
                        Id = 0,
                        Location = new Location() {Id = 0, X=0, Y=0},
                        EarliestDepartureTime = 0,
                        LatestReturnTime = 520
                    }
                },
                Locations = new List<Location>()
                {
                    new Location() {Id=0, X=0, Y=0 },
                    new Location() {Id = 1, X=-78, Y=-81},
                    new Location() {Id = 2, X = 35, Y=-4},
                    new Location() {Id =3, X=-23, Y=-64},
                    new Location() {Id=4, X=76, Y=31 },
                    new Location() {Id=5, X = 49, Y=83},
                    new Location() {Id=6, X=-9, Y=48 }
                },
                VehicleCapacity = 100,
                VehicleNumber = 6,
                VehicleSpeed = 3,
                Visits = new List<Visit>()
                {
                    new Visit()
                    {
                        AvailabilityTime = 372,
                        Demand = -18,
                        Duration = 20,
                        Id=1,
                        Location = new Location(){Id = 1, X=-78, Y=-81}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 483,
                        Demand = -44,
                        Duration = 20,
                        Id=2,
                        Location = new Location(){Id = 2, X=35, Y=-4}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 354,
                        Demand = -25,
                        Duration = 20,
                        Id=3,
                        Location = new Location(){Id = 3, X=-23, Y=-64}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 86,
                        Demand = -25,
                        Duration = 20,
                        Id=4,
                        Location = new Location(){Id = 4, X=76, Y=31}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 19,
                        Demand = -28,
                        Duration = 20,
                        Id=5,
                        Location = new Location(){Id = 5, X=49, Y=83}
                    },
                    new Visit()
                    {
                        AvailabilityTime = 24,
                        Demand = -22,
                        Duration = 20,
                        Id = 6,
                        Location = new Location() {Id=6, X=-9, Y=48 }
                    }
                }
            };
            #endregion
            byte[] bytes = _converter.ToByteArray(instance);
            TaskSolver solver = new DVRPTaskSolver(bytes);
            var divideProblem = solver.DivideProblem(1);
            List<byte[]> results = divideProblem.Select(b => solver.Solve(b, TimeSpan.Zero)).ToList();
            var final = _converter.FromBytesArray(solver.MergeSolution(results.ToArray()));
            Assert.IsNotNull(final);
            Assert.IsInstanceOfType(final, typeof(DVRPPartialProblemInstance));
            Assert.AreEqual(SolutionResult.Successful, ((DVRPPartialProblemInstance)final).SolutionResult);
            Assert.AreEqual(545f, ((DVRPPartialProblemInstance)final).PartialResult,1);
        }
    }
}
