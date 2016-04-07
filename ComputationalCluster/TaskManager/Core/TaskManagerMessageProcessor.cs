using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace TaskManager.Core
{
    /// <summary>
    /// TM's message handling utilities
    /// </summary>
    public class TaskManagerMessageProcessor : ClientMessageProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            //implementation in second stage
            if (!SolvableProblems.Contains(divideProblem.ProblemType))
            {
                log.Debug("Not supported problem type.");
                return new Error()
                {
                    ErrorMessage = "not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };
            }
            var partialProblem = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 0,
                Data = new byte[] { 0 },
                NodeID = componentId
            };
            var partialProblem2 = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 1,
                Data = new byte[] { 0 },
                NodeID = componentId
            };
            //adding info about partial problems, their task ids, and partialProblem
            //some things can be temporary (partialProblems?)
            storage.AddIssue(divideProblem.Id, new ProblemInfo()
            {
                ProblemsCount = 2,
                ProblemType = divideProblem.ProblemType,
                SolutionsCount = 0
            });

            storage.AddTaskToIssue(divideProblem.Id, partialProblem);
            storage.AddTaskToIssue(divideProblem.Id, partialProblem2);
            //end of implementation
            //mock (thread sleep)
            Thread.Sleep(50000);
            log.DebugFormat("Division finished. ({0})", divideProblem.Id);
            //creating msg
            SolvePartialProblems partialProblems = new SolvePartialProblems()
            {
                ProblemType = divideProblem.ProblemType,
                Id = divideProblem.Id,
                CommonData = divideProblem.Data,
                PartialProblems = new SolvePartialProblemsPartialProblem[]
                {
                    partialProblem, partialProblem2
                }
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
                Solutions finalSolution = LinkSolutions(solutions.Id);
                storage.RemoveIssue(solutions.Id);
                return finalSolution;
            }

            return null;
        }

        //task solver stuff
        public Solutions LinkSolutions(ulong problemId)
        {
            //for issue in storage (by problemId) - get all tasks
            //get SolutionsSolution from them and do something amazing
            //mock (thread sleep)
            Thread.Sleep(20000);
            log.DebugFormat("Solutions have been linked ({0})", problemId);
            //return final solution (this one is mocked)
            return new Solutions()
            {
                CommonData = new byte[] { 0 },
                Id = problemId,
                ProblemType = storage.GetIssueType(problemId),
                SolutionsList = new[]
                {
                    new SolutionsSolution()
                    {
                        Data = null,
                        ComputationsTime = 1,
                        TaskIdSpecified = false,
                        Type = SolutionsSolutionType.Final
                    }
                }
            };
        }
    }
}
