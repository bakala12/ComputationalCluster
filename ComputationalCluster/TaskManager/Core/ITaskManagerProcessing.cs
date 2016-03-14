using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    /// <summary>
    /// methods essential to TM work
    /// </summary>
    public interface ITaskManagerProcessing
    {
        SolvePartialProblems DivideProblem(DivideProblem divideProblem);
        Solutions HandleSolutions(Solutions solutions);
        Solutions LinkSolutions(ulong problemId);
        Status GetStatus();
    }
}
