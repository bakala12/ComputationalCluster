using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class DVRPProblemInstance
    {
        public List<Depot> Depots { get; set; }
        public List<Location> Locations { get; set; }
        public List<Visit> Visits { get; set; }
        public int VehicleCapacity { get; set; }
        public int VehicleNumber { get; set; }


        public DVRPProblemInstance()
        {
            Depots = new List<Depot>();
            Locations = new List<Location>();
            Visits = new List<Visit>();
        }
    }


    public class Location
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Visit
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        public int Demand { get; set; }
        public int Duration { get; set; }
        public int AvailabilityTime { get; set; }
    }

    public class Depot
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        public int EarliestDepartureTime { get; set; }
        public int LatestReturnTime { get; set; }
    }
}
