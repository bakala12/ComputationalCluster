// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Server.Data
{
    /// <summary>
    /// Stores information about the problem.
    /// Will be used in future development.
    /// </summary>
    public class ProblemDataSet
    {
        //no need for problemId - it's the key of a dict

        /// <summary>
        /// type of this problem ("DVRP" is enough for this stage)
        /// </summary>
        public string ProblemType { get; set; }
        /// <summary>
        /// commonData, used in construction of SolvePartialProblems msg
        /// and DivideProblem msg, acquired from SolveRequest
        /// </summary>
        public byte[] CommonData { get; set; }
        /// <summary>
        /// array of subproblems (and corresponding solutions)
        /// acquired from TM after sending DivideProbllem msg
        /// </summary>
        public PartialSet[] PartialSets { get; set; }

    }
}