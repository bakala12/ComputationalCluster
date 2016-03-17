using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;

namespace ComputationalNode.Core
{
    /// <summary>
    /// provides non-communication CN's functionalities
    /// </summary>
    public class ComputationalNodeProcessingModule : ProcessingModule
    {
        private List<StatusThread> threads;
        private ulong threadCount = 0;

        public ComputationalNodeProcessingModule(List<string> problems) : base(problems)
        {
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

        public Message ComputeSubtask(SolvePartialProblems solvePartialProblems)
        {
            //implementation in second stage, now mocked:
            if (!SolvableProblems.Contains(solvePartialProblems.ProblemType))
                return new Error()
                {
                    ErrorMessage = "not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };


            return new Solutions()
            {
                CommonData = new byte[] {0},
                Id = solvePartialProblems.Id,
                ProblemType = solvePartialProblems.ProblemType,
                Solutions1 = new[]
                {
                    new SolutionsSolution()
                    {
                        Data = null,
                        ComputationsTime = 1,
                        TaskIdSpecified = false,
                        Type = SolutionsSolutionType.Partial
                    }
                }
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
