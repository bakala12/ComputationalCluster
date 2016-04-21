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
        public int[][] VisitIds { get; set; }
        [Description("Wynik problemu - suma minimalnych odległości ścieżek odwiedzających wszystkich klientów")]
        public double PartialResult { get; set; }
        public SolutionResult SolutionResult { get; set; } = SolutionResult.NotSolved;

        public DVRPPartialProblemInstance()
        {
            VisitIds = null;
        }
    }
}
