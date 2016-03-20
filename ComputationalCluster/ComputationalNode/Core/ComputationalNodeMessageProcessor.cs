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
    /// provides CN's message handling utilities
    /// </summary>
    public class ComputationalNodeMessageProcessor: ClientMessageProcessor
    {
        public ComputationalNodeMessageProcessor(List<string> problems ): base (problems)
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
            //some error handling:
            if (!this.SolvableProblems.Contains(solvePartialProblems.ProblemType))
            {
                return new Error()
                {
                    ErrorMessage = "Invalid type of problem delivered",
                    ErrorType = ErrorErrorType.InvalidOperation
                };
            }
            Console.WriteLine("Computation started & finished.");
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
                        TaskIdSpecified = true,
                        TaskId = solvePartialProblems.PartialProblems[0].TaskId,
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
