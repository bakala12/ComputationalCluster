using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public enum SolutionResult
    {
        NotSolved,
        Successful,
        Impossible
    }

    public class DVRPPartialProblemInstance : IProblemInstance
    {
        [Description("Miejsca do odwiedzenia - klienci")]
        public List<List<int>> VisitIds { get; set; }
        [Description("Wynik problemu - suma minimalnych odległości ścieżek odwiedzających wszystkich klientów")]
        public int PartialResult { get; set; }
        public SolutionResult SolutionResult { get; set; } = SolutionResult.NotSolved;

        public DVRPPartialProblemInstance()
        {
            VisitIds = new List<List<int>>();
        }
    }
}
