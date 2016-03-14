using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace Client.Core
{
    /// <summary>
    /// handles a problem in console - reading, preparing message,
    /// and maybe something more
    /// </summary>
    public class ClientNodeProcessingModule : IClientNodeProcessing
    {
        private byte[] data;
        private string type;

        public string Type
        {
            get
            {
                return type;
            }
        }

        public ClientNodeProcessingModule()
        {

        }

        public ClientNodeProcessingModule(byte[] _data, string _type)
        {
            data = _data;
            type = _type;
        }

        /// <summary>
        /// this will do something with final solution, e.g. printing it
        /// </summary>
        /// <param name="solution"></param>
        public void DoSomethingWithSolution(SolutionsSolution solution)
        {
            return;
        }

        /// <summary>
        /// not-implemented yet subroutine of getting a problem to solve
        /// and transforming it into the byte data
        /// </summary>
        /// <returns></returns>
        public void GetProblem()
        {
            data = new byte[] { 123 };
            type = "EXAMPLE";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>solverequest based on current state of processing module</returns>
        public SolveRequest GetRequest()
        {
            return new SolveRequest()
            {
                Data = data,
                ProblemType = type,
                SolvingTimeoutSpecified = true,
                SolvingTimeout = Properties.Settings.Default.SolveTimeout
            };
        }
    }
}
