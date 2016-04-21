using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public interface IProblemInstance
    {
    }
    public class DVRPProblemInstance : IProblemInstance
    {
        [Description("Magazyny - w założeniu będzie 1")]
        public List<Depot> Depots { get; set; }
        public List<Location> Locations { get; set; }
        [Description("Miejsca do odwiedzenia - klienci")]
        public List<Visit> Visits { get; set; }
        //zahardcodowane
        public int VehicleSpeed { get; set; } = 1;
        public int VehicleCapacity { get; set; }
        public int VehicleNumber { get; set; }


        public DVRPProblemInstance()
        {
            Depots = new List<Depot>();
            Locations = new List<Location>();
            Visits = new List<Visit>();
        }
    }
}
