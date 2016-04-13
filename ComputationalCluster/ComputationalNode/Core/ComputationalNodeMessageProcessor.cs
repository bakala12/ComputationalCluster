﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlgorithmSolvers;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using log4net;
using UCCTaskSolver;

namespace ComputationalNode.Core
{
    /// <summary>
    /// provides CN's message handling utilities
    /// </summary>
    public class ComputationalNodeMessageProcessor: ClientMessageProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ComputationalNodeMessageProcessor(List<string> problems)
            : base (problems)
        {
            //enough for this stage:
            threads.Add(new StatusThread()
            {
                HowLongSpecified = false,
                ProblemInstanceIdSpecified = false,
                State = StatusThreadState.Idle,
                ProblemType = "",
                TaskIdSpecified = true,
                TaskId = ++threadCount
            });
        }

        public Message ComputeSubtask(SolvePartialProblems solvePartialProblems)
        {
            log.DebugFormat("Computation started. ({0})", solvePartialProblems.Id);

            if (!SolvableProblems.Contains(solvePartialProblems.ProblemType))
                return new Error()
                {
                    ErrorMessage = "Not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };
            //task solver stuff:
            var taskSolver = new DvrpTaskSolver(solvePartialProblems.CommonData);
            var solutionsList = new List<SolutionsSolution>();

            foreach (var partialProblem in solvePartialProblems.PartialProblems)
            {
                var newSolution = new SolutionsSolution()
                {
                    TaskId = partialProblem.TaskId,
                    TaskIdSpecified = true,
                    //TimeoutOccured = false,
                    Type = SolutionsSolutionType.Partial,
                    //ComputationsTime = 0
                };
                newSolution.Data = taskSolver.Solve(partialProblem.Data, TimeSpan.Zero);
            }

            log.DebugFormat("Computation finished. ({0})", solvePartialProblems.Id);

            return new Solutions()
            {
                CommonData = solvePartialProblems.CommonData,
                Id = solvePartialProblems.Id,
                ProblemType = solvePartialProblems.ProblemType,
                SolutionsList = solutionsList.ToArray()
            };
        }

        public Status GetStatus()
        {
            Status statusMsg = new Status()
            {
                Threads = threads.ToArray()
            };
            return statusMsg;
        }
    }
}
