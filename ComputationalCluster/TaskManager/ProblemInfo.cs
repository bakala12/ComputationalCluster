using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    public class ProblemInfo
    {
        public int ProblemsCount = 0;
        public int SolutionsCount = 0;
        public byte[] CommonData = null;
        public string ProblemType;
        /// <summary>
        /// partial problems indexed by task id (given by TM)
        /// </summary>
        public Dictionary<ulong, SolutionsSolution> PartialSolutions;
        public ProblemInfo()
        {
            PartialSolutions = new Dictionary<ulong, SolutionsSolution>();
        }
    }
}
