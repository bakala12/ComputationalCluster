using CommunicationsUtils.Messages;

namespace Server.Data
{
    /// <summary>
    /// next enum
    /// </summary>
    public enum PartialSetStatus
    {
        /// <summary>
        /// partial problem is fresh - not sent to any CN
        /// </summary>
        Fresh,
        /// <summary>
        /// partial solution is in computation stage, or is computed but not sent to TM
        /// </summary>
        Ongoing,
        /// <summary>
        /// partial solution is sent to TM. nothing to do (except TM's failure)
        /// </summary>
        Sent
    }
    /// <summary>
    /// info about one of partial problems (and solutions)
    /// </summary>
    public class PartialSet
    {
        /// <summary>
        /// if Ongoing, this is the id of Comp. node computing it
        /// could be useful in CN's malfunction issue
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
