using System;
using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Data;

namespace Tests
{
    [TestClass]
    public class DataSetTests
    {
        [TestMethod]
        public void DataSetDivideForProperTaskManager()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent() {ComponentType = ComponentType.TaskManager,
                SolvableProblems = new [] {"abc"}});
            var dict  = new Dictionary<int, ProblemDataSet>();
            var bytes  = new byte[1] { 1 };
            dict.Add(1, 
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 0,
                    ProblemType = "abc"
                });
            var ret = DataSetOps.GetMessageForTaskManager(comps, 1, dict);
            Assert.AreEqual(typeof(DivideProblem), ret.GetType());
            var msg = ret.Cast<DivideProblem>();
            Assert.AreEqual(1, msg.Data.Length);
            Assert.AreEqual(1, msg.Data[0]);
            Assert.AreEqual(1, dict[1].TaskManagerId);
        }

        [TestMethod]
        public void DataSetNothingForTaskManager()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.TaskManager,
                SolvableProblems = new[] { "def" }
            });
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 0,
                    ProblemType = "abc"
                });
            var ret = DataSetOps.GetMessageForTaskManager(comps, 1, dict);
            Assert.IsNull(ret);
        }

        [TestMethod]
        public void DataSetSolutionsForTaskManager()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.TaskManager,
                SolvableProblems = new[] { "abc" }
            });
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new []
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = new SolutionsSolution(),
                            Status = PartialSetStatus.Ongoing
                        }
                    }
                });
            var ret = DataSetOps.GetMessageForTaskManager(comps, 1, dict);
            Assert.AreEqual(typeof(Solutions), ret.GetType());
            var msg = ret.Cast<Solutions>();
            Assert.AreEqual(1, msg.SolutionsList.Length);
            Assert.AreEqual(1, msg.CommonData.Length);
            Assert.AreEqual(1, msg.CommonData[0]);
        }

        [TestMethod]
        public void DataSetSolveForCompNode()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new[]
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = null,
                            Status = PartialSetStatus.Fresh
                        }
                    }
                });
            var ret = DataSetOps.GetMessageForCompNode(comps, 1, dict);
            Assert.AreEqual(typeof(SolvePartialProblems), ret.GetType());
            var msg = ret.Cast<SolvePartialProblems>();
            Assert.AreEqual(1, msg.PartialProblems.Length);
            Assert.AreEqual(1, msg.CommonData.Length);
            Assert.AreEqual(1, msg.CommonData[0]);
            Assert.AreEqual(1, dict[1].PartialSets[0].NodeId);
        }

        [TestMethod]
        public void DataSetNothingForCompNode()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = null
                });
            var ret = DataSetOps.GetMessageForCompNode(comps, 1, dict);
            Assert.IsNull(ret);
        }

        [TestMethod]
        public void DataSetFinalSolutionState()
        {
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new[]
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = new SolutionsSolution()
                            {
                                Type = SolutionsSolutionType.Final
                            },
                            Status = PartialSetStatus.Sent
                        }
                    }
                });
            var ret = DataSetOps.GetSolutionState(new SolutionRequest() { Id =1 }, dict);
            Assert.AreEqual(typeof(Solutions), ret.GetType());
            var msg = ret.Cast<Solutions>();
            Assert.AreEqual(1, msg.SolutionsList.Length);
            Assert.AreEqual(SolutionsSolutionType.Final, msg.SolutionsList[0].Type);
        }

        [TestMethod]
        public void DataSetTaskManagerFailureNextTaskManagerWork()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.TaskManager,
                SolvableProblems = new[] { "abc" }
            });
            comps.Add(2, new ActiveComponent()
            {
                ComponentType = ComponentType.TaskManager,
                SolvableProblems = new [] {"abc"}
            });
            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 0,
                    ProblemType = "abc"
                });
            var ret = DataSetOps.GetMessageForTaskManager(comps, 1, dict);
            Assert.AreEqual(typeof(DivideProblem), ret.GetType());
            
            DataSetOps.HandleClientMalfunction(comps, 1, dict);

            var ret2 = DataSetOps.GetMessageForTaskManager(comps, 2, dict);
            Assert.AreEqual(typeof(DivideProblem), ret2.GetType());
            var msg2 = ret.Cast<DivideProblem>();
            Assert.AreEqual(1, msg2.Data.Length);
            Assert.AreEqual(1, msg2.Data[0]);
            Assert.AreEqual(2, dict[1].TaskManagerId);
        }

        [TestMethod]
        public void DataSetCompNodeFailureNextCompNodeWork()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });
            comps.Add(2, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });

            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new[]
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = null,
                            Status = PartialSetStatus.Fresh
                        }
                    }
                });
            var ret = DataSetOps.GetMessageForCompNode(comps, 1, dict);
            Assert.AreEqual(typeof(SolvePartialProblems), ret.GetType());

            DataSetOps.HandleClientMalfunction(comps, 1, dict);

            var ret2 = DataSetOps.GetMessageForCompNode(comps, 2, dict);
            Assert.AreEqual(typeof(SolvePartialProblems), ret2.GetType());
            var msg = ret2.Cast<SolvePartialProblems>();
            Assert.AreEqual(1, msg.PartialProblems.Length);
            Assert.AreEqual(1, msg.CommonData.Length);
            Assert.AreEqual(1, msg.CommonData[0]);
            Assert.AreEqual(2, dict[1].PartialSets[0].NodeId);
        }
        [TestMethod]
        public void DataSetMultipleProblemsTest()
        {
            var comps = new Dictionary<int, ActiveComponent>();
            comps.Add(1, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });
            comps.Add(2, new ActiveComponent()
            {
                ComponentType = ComponentType.ComputationalNode,
                SolvableProblems = new[] { "abc" }
            });

            var dict = new Dictionary<int, ProblemDataSet>();
            var bytes = new byte[1] { 1 };
            dict.Add(1,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new[]
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = null,
                            Status = PartialSetStatus.Fresh
                        }
                    }
                });
            dict.Add(2,
                new ProblemDataSet()
                {
                    CommonData = bytes,
                    TaskManagerId = 1,
                    ProblemType = "abc",
                    PartialSets = new[]
                    {
                        new PartialSet()
                        {
                            PartialProblem = new SolvePartialProblemsPartialProblem(),
                            PartialSolution = null,
                            Status = PartialSetStatus.Fresh
                        }
                    }
                });
            var ret = DataSetOps.GetMessageForCompNode(comps, 1, dict);
            Assert.AreEqual(typeof(SolvePartialProblems), ret.GetType());
            Assert.AreEqual(1, dict[1].PartialSets[0].NodeId);

            var ret2 = DataSetOps.GetMessageForCompNode(comps, 2, dict);
            Assert.AreEqual(typeof(SolvePartialProblems), ret2.GetType());
            Assert.AreEqual(2, dict[2].PartialSets[0].NodeId);
        }
    }
}
