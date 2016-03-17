using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace ComputationalNode.Core
{
    /// <summary>
    /// provides non-communication CN's functionalities
    /// </summary>
    public class ComputationalNodeProcessingModule
    {
        private List<StatusThread> threads;
        private ulong threadCount = 0;

        public ulong ComponentId { get; set; }

        public ComputationalNodeProcessingModule()
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

        public Solutions ComputeSubtask(SolvePartialProblems solvePartialProblems)
        {
            //implementation in second stage, now mocked:
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
