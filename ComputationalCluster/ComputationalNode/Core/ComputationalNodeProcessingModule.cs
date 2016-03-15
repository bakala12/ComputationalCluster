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
    public class ComputationalNodeProcessingModule : IComputationalNodeProcessing
    {
        public Solutions ComputeSubtask(SolvePartialProblems solvePartialProblems)
        {
            throw new NotImplementedException();
        }

        public Status GetStatus()
        {
            throw new NotImplementedException();
        }

        public ulong ComponentId { get; set; }
    }
}
