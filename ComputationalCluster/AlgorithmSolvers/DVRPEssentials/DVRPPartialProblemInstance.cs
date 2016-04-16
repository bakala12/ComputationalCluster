using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class DVRPPartialProblemInstance : IProblemInstance
    {

        [Description("Magazyny - w założeniu będzie 1")]
        public List<Depot> Depots { get; set; }
        public List<Location> Locations { get; set; }
        [Description("Miejsca do odwiedzenia - klienci")]
        public List<Visit> Visits { get; set; }
        public int VehicleCapacity { get; set; }
        [Description("Wynik podproblemu - minimalna odległość ścieżki odwiedzającej wszystkich klientów")]
        public int PartialResult { get; set; }
    }
}
