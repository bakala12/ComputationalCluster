using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace Client.Core
{
    /// <summary>
    /// interface for processing module to make it look neat
    /// </summary>
    public interface IClientNodeProcessing
    {
        void GetProblem();
        SolveRequest GetRequest();
        void DoSomethingWithSolution(SolutionsSolution solution);
    }
}
