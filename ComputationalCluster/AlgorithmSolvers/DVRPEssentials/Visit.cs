using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class Visit
    {
        public int Id { get; set; }
        public Location Location { get; set; }
        [Description("Zapotrzebowanie na towar - ujemna wartość oznacza, że towar należy dostarczyć, dodatnia - ilość towaru do zabrania")]
        public int Demand { get; set; }
        [Description("Czas pobytu w miejscu - można rozważać jako czas rozładunku")]
        public int Duration { get; set; }
        [Description("Czas w jakim należy odwiedzić dane miejsce")]
        public int AvailabilityTime { get; set; }
    }
}
