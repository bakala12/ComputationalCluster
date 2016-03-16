using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Core
{
    /// <summary>
    /// provides non-communication TM's functionalities
    /// </summary>
    public class TaskManagerProcessingModule: ProcessingModule
    {
        /// <summary>
        /// current problems in TM indexed by problem id in cluster (given by CS)
        /// </summary>
        private TaskManagerStorage storage;
        private List<StatusThread> threads;
        private ulong threadCount = 0;

        public TaskManagerProcessingModule(List<string> problems, TaskManagerStorage _storage)
            : base(problems)
        {
            storage = _storage;
            threads = new List<StatusThread>();
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

        public Message DivideProblem(DivideProblem divideProblem)
        {
            //implementation in second stage, this is mocked
            if (!SolvableProblems.Contains(divideProblem.ProblemType))
                return new Error()
                {
                    ErrorMessage = "not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };

            var partialProblem = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 0,
                Data = new byte[] { 0 },
                NodeID = componentId
            };

            //adding info about partial problems, their task ids, and partialProblem
            //some things can be temporary (partialProblems?)
            storage.AddIssue(divideProblem.Id, new ProblemInfo()
            {
                ProblemsCount = 1,
                ProblemType = divideProblem.ProblemType,
                SolutionsCount = 0
            });

            storage.AddTaskToIssue(divideProblem.Id, partialProblem);
            //end of implementation

            //creating msg
            SolvePartialProblems partialProblems = new SolvePartialProblems()
            {
                ProblemType = divideProblem.ProblemType,
                Id = divideProblem.Id,
                CommonData = divideProblem.Data,
                PartialProblems = new SolvePartialProblemsPartialProblem[]
                {
                    partialProblem
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
            if (solutions.Solutions1 == null)
                return null;
            
            foreach (var solution in solutions.Solutions1)
            {
                if (!storage.ContainsIssue(solutions.Id) || storage.ExistsTask(solutions.Id,solution.TaskId))
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
                Solutions finalSolution = LinkSolutions(solutions.Id);
                return finalSolution;
            }

            return null;
        }

        //task solver stuff
        public Solutions LinkSolutions(ulong problemId)
        {
            //for issue in storage (by problemId) - get all tasks
            //get SolutionsSolution from them and do something amazing

            //return final solution (this one is mocked)
            return new Solutions()
            {
                CommonData = new byte[] { 0 },
                Id = problemId,
                ProblemType = storage.GetIssueType(problemId),
                Solutions1
            = new[] { new SolutionsSolution() { Data = null, ComputationsTime = 1, TaskIdSpecified = false,
            Type = SolutionsSolutionType.Final} }
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
