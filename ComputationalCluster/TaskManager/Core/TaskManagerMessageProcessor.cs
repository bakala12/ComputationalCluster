using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlgorithmSolvers.DVRPEssentials;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.Shared;
using log4net;
using UCCTaskSolver;

namespace TaskManager.Core
{
    /// <summary>
    /// TM's message handling utilities
    /// </summary>
    public class TaskManagerMessageProcessor : ClientMessageProcessor
    {
        private readonly IAssemblyResolver _resolver = new AssemblyResolver();
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ProblemToBytesConverter _problemConverter = new ProblemToBytesConverter();

        /// <summary>
        /// current problems in TM indexed by problem id in cluster (given by CS)
        /// </summary>
        private TaskManagerStorage storage;

        public TaskManagerMessageProcessor(List<string> problems, TaskManagerStorage _storage)
            : base(problems)
        {
            storage = _storage;
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

        public Status GetStatus()
        {
            Status statusMsg = new Status()
            {
                Threads = threads.ToArray()
            };
            return statusMsg;
        }

        public Message DivideProblem(DivideProblem divideProblem)
        {
            log.DebugFormat("Division of problem has started. ({0})", divideProblem.Id);

            if (!SolvableProblems.Contains(divideProblem.ProblemType))
            {
                log.Debug("Not supported problem type.");
                return new Error()
                {
                    ErrorMessage = "not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };
            }

            var commonData = divideProblem.Data;
            var taskSolver = _resolver.GetInstanceByBaseTypeName(divideProblem.ProblemType, commonData);
            var bytes = taskSolver.DivideProblem(0);

            log.DebugFormat("Length of divide problem message: {0}", bytes?.Sum(x => x.Length));
            //adding info about partial problems, their task ids, and partialProblem
            //some things can be temporary (partialProblems?)
            storage.AddIssue(divideProblem.Id, new ProblemInfo()
            {
                ProblemsCount = bytes?.GetLength(0) ?? 0,
                ProblemType = divideProblem.ProblemType,
                SolutionsCount = 0,
                CommonData = commonData
            });

            var problemsList = new List<SolvePartialProblemsPartialProblem>();
            //iterate through all partial problems and create proper messages
            for (var i = 0; i < (bytes?.GetLength(0) ?? 0); i++)
            {
                var partialProblem = new SolvePartialProblemsPartialProblem()
                {
                    TaskId = (ulong)i,
                    Data = bytes[i],
                    NodeID = componentId
                };

                problemsList.Add(partialProblem);
                //adding info about subtask to task manager memory
                storage.AddTaskToIssue(divideProblem.Id, partialProblem);
            }

            log.DebugFormat("Division finished. ({0})", divideProblem.Id);
            //creating msg
            var partialProblems = new SolvePartialProblems()
            {
                ProblemType = divideProblem.ProblemType,
                Id = divideProblem.Id,
                CommonData = divideProblem.Data,
                PartialProblems = problemsList.ToArray()
            };
            return partialProblems;
        }

        /// <summary>
        /// handles solutions msg. according to specifiaction, Solutions message
        /// concerns only one problem
        /// </summary>
        /// <param name="solutions"></param>
        public Solutions HandleSolutions(Solutions solutions)
        {
            if (solutions.SolutionsList == null)
                return null;
            log.DebugFormat("Adding partial solutions to TM's memory. ({0})", solutions.Id);
            foreach (var solution in solutions.SolutionsList)
            {
                if (!storage.ContainsIssue(solutions.Id) || !storage.ExistsTask(solutions.Id, solution.TaskId))
                {
                    throw new Exception("Invalid solutions message delivered to TM");
                }
                storage.AddSolutionToIssue(solutions.Id, solution.TaskId, solution);
            }
            //this is not possible:
            //if (currentProblems[solutions.Id].SolutionsCount > currentProblems[solutions.Id].ProblemsCount)

            //can be linked, because all of partial problems were solved & delivered
            if (storage.IssueCanBeLinked(solutions.Id))
            {
                log.DebugFormat("Linking solutions (id:{0})", solutions.Id);
                Solutions finalSolution = LinkSolutions(solutions.Id, solutions.ProblemType);
                storage.RemoveIssue(solutions.Id);
                return finalSolution;
            }

            return null;
        }

        //task solver stuff
        public Solutions LinkSolutions(ulong problemId, string problemType)
        {
            //link solutions (task solver stuff)
            var commonData = storage.GetCommonData(problemId);
            var taskSolver = _resolver.GetInstanceByBaseTypeName(problemType, commonData);
            var solutionsBytes = storage.GetIssueSolutionsBytes(problemId);
            var finalBytes = taskSolver.MergeSolution(solutionsBytes);

            log.DebugFormat("Solutions have been linked ({0})", problemId);
            //return final solution (this one is mocked)
            return new Solutions()
            {
                CommonData = commonData,
                Id = problemId,
                ProblemType = storage.GetIssueType(problemId),
                SolutionsList = new[]
                {
                    new SolutionsSolution()
                    {
                        Data = finalBytes,
                        ComputationsTime = 1,
                        TaskIdSpecified = false,
                        Type = SolutionsSolutionType.Final
                    }
                }
            };
        }
    }
}
