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
        public string ProblemType = "";
        /// <summary>
        /// partial problems indexed by task id (given by TM)
        /// </summary>
        public Dictionary<ulong, PartialInfo> PartialProblems;
        public ProblemInfo()
        {
            PartialProblems = new Dictionary<ulong, PartialInfo>();
        }
    }
}
