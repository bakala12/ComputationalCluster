using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class Depot
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        public int EarliestDepartureTime { get; set; }
        public int LatestReturnTime { get; set; }
    }
}
