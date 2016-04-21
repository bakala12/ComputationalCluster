using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;
using UCCTaskSolver;

namespace CommunicationsUtils.ClientComponentCommon
{
    /// <summary>
    /// common class for TM's processing and CN's processing
    /// </summary>
    public abstract class ClientMessageProcessor
    {
        protected ulong componentId;
        public ulong ComponentId { get; set; }

        protected List<StatusThread> threads;
        protected ulong threadCount = 0;

        public List<string> SolvableProblems;

        protected ClientMessageProcessor(List<string> problems)
        {
            SolvableProblems = problems;
            threads = new List<StatusThread>();
        }
    }
}
