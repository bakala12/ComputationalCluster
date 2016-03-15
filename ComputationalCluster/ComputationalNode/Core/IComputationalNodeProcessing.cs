using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace ComputationalNode.Core
{
    /// <summary>
    /// methods essential to CN work
    /// </summary>
    public interface IComputationalNodeProcessing
    {
        Solutions ComputeSubtask(SolvePartialProblems solvePartialProblems);
        Status GetStatus();
        ulong ComponentId { get; set; }
    }
}
