using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    public abstract class ProcessingModule
    {
        protected ulong componentId;
        public ulong ComponentId { get; set; }
        public List<string> SolvableProblems;
        public ProcessingModule(List<string> problems)
        {
            SolvableProblems = problems;
        }
    }
}
