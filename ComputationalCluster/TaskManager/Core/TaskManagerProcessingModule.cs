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
    public class TaskManagerProcessingModule: ITaskManagerProcessing
    {
        /// <summary>
        /// current problems in TM indexed by problem id in cluster (given by CS)
        /// </summary>
        private Dictionary<ulong, ProblemInfo> currentProblems;
        private ulong componentId;
        private List<StatusThread> threads;
        private ulong threadCount = 0;

        //necessary for now, but could/should be removed:
        public ulong ComponentId
        {
            get
            {
                return componentId;
            }
            set
            {
                componentId = value;
            }
        }

        public TaskManagerProcessingModule()
        {
            currentProblems = new Dictionary<ulong, ProblemInfo>();
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

        public SolvePartialProblems DivideProblem(DivideProblem divideProblem)
        {
            //implementation in second stage, this is mocked:

            var partialProblem = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 0,
                Data = new byte[] { 0 },
                NodeID = componentId
            };

            //adding info about partial problems, their task ids, and partialProblem
            //some things can be temporary (partialProblems?)
            currentProblems.Add(divideProblem.Id, new ProblemInfo()
            {
                ProblemsCount = 1,
                ProblemType = divideProblem.ProblemType,
                SolutionsCount = 0
            });

            currentProblems[divideProblem.Id].PartialProblems.Add(0, new PartialInfo()
            { Solution = null, Problem = partialProblem });
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
            foreach (var solution in solutions.Solutions1)
            {
                if (currentProblems[solutions.Id].PartialProblems[solution.TaskId].Solution != null)
                {
                    throw new Exception("Problem with multiple sending of partialProblem by CS");
                }
                currentProblems[solutions.Id].PartialProblems[solution.TaskId].Solution = solution;
                currentProblems[solutions.Id].SolutionsCount++;
            }
            //this is not possible:
            //if (currentProblems[solutions.Id].SolutionsCount > currentProblems[solutions.Id].ProblemsCount)

            //can be linked, because all of partial problems were solved & delivered
            if (currentProblems[solutions.Id].SolutionsCount == currentProblems[solutions.Id].ProblemsCount)
            {
                Solutions finalSolution = LinkSolutions(solutions.Id);
                return finalSolution;
            }

            return null;
        }

        //task solver stuff
        public Solutions LinkSolutions(ulong problemId)
        {
            //foreach (var pInfo in currentProblems[problemId].PartialProblems)
            //get SolutionsSolution from pInfo and do something amazing

            //return final solution (this one is mocked)
            return new Solutions()
            {
                CommonData = new byte[] { 0 },
                Id = problemId,
                ProblemType = currentProblems[problemId].ProblemType,
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
