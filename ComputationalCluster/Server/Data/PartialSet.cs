using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace Server.Data
{
    /// <summary>
    /// Fresh - computed by TM, not sent to any CN
    /// Ongoing - sent to CN or not sent to TM to link
    /// Sent - sent to TM to link
    /// </summary>
    public enum PartialSetStatus
    {
        Fresh,
        Ongoing,
        Sent
    }
    /// <summary>
    /// info about one of partial problems (and solutions)
    /// </summary>
    public class PartialSet
    {
        /// <summary>
        /// if Ongoing, this is the id of Comp. node computing it
        /// </summary>
        public int NodeId { get; set; }
        /// <summary>
        /// null if CN hasn't computed it
        /// </summary>
        public SolutionsSolution PartialSolution { get; set; }
        /// <summary>
        /// problem instance acquired from TM
        /// </summary>
        public SolvePartialProblemsPartialProblem PartialProblem { get; set; }
        /// <summary>
        /// current state of this subproblem
        /// </summary>
        public PartialSetStatus Status { get; set; } 

    }
}
