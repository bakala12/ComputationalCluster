using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using log4net;

namespace ComputationalNode.Core
{
    /// <summary>
    /// provides CN's message handling utilities
    /// </summary>
    public class ComputationalNodeMessageProcessor: ClientMessageProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            log.DebugFormat("Computation started. ({0})", solvePartialProblems.Id);
            Thread.Sleep(30000);
            //implementation in second stage, now mocked (thread sleep)
            if (!SolvableProblems.Contains(solvePartialProblems.ProblemType))
                return new Error()
                {
                    ErrorMessage = "Not supported problem type",
                    ErrorType = ErrorErrorType.InvalidOperation
                };

            log.DebugFormat("Computation finished. ({0})", solvePartialProblems.Id);

            return new Solutions()
            {
                CommonData = new byte[] {0},
                Id = solvePartialProblems.Id,
                ProblemType = solvePartialProblems.ProblemType,
                SolutionsList = new[]
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
