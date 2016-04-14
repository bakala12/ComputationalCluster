using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class DVRPProblemParser
    {
        public string ProblemPath { get; set; }

        public DVRPProblemParser(string path)
        {
            ProblemPath = path;
        }

        // should return DVRPProblem object
        public DVRPProblemInstance Parse()
        {
            var dvrp = new DVRPProblemInstance();
            string line;
            int numDepots=0, numVisits=0, numLocations=0, numVehicles=0;
            StreamReader file = new StreamReader(ProblemPath);
            while ((line = file.ReadLine()) != "DATA_SECTION")
            {
                if (line.Contains("NUM_DEPOTS"))
                    numDepots = int.Parse(line.Split(':')[1]);
                if (line.Contains("NUM_VISITS"))
                    numVisits = int.Parse(line.Split(':')[1]);
                if (line.Contains("NUM_LOCATIONS"))
                    numLocations = int.Parse(line.Split(':')[1]);
                if (line.Contains("NUM_VEHICLES"))
                    numVehicles = int.Parse(line.Split(':')[1]);
            }
            while ((line = file.ReadLine()) != "EOF" && line != null)
            {
                if (line == "DEPOTS")
                    line = setDepots(file, numDepots, dvrp);
                if (line == "DEMAND_SECTION")
                    line = setDemands(file, numVisits, dvrp);
                if (line == "LOCATION_COORD_SECTION")
                    line = setLocations(file, numLocations, dvrp);
                if (line == "DEPOT_LOCATION_SECTION")
                    line = setDepotLocations(file, numDepots, dvrp);
                if (line == "VISIT_LOCATION_SECTION")
                    line = setVisitLocations(file, numVisits, dvrp);
                if (line == "DURATION_SECTION")
                    line = setVisitDuration(file, numVisits, dvrp);
                if (line == "DEPOT_TIME_WINDOW_SECTION")
                    line = setDepotTimeAvailability(file, numDepots, dvrp);
                if (line == "TIME_AVAIL_SECTION")
                    line = setVisitTimeAvailability(file, numVisits, dvrp);
            }
            file.Close();
            return dvrp;
        }

        private string setDepots(StreamReader file, int numDepots, DVRPProblemInstance dvrp)
        {
            string line = string.Empty;
            while (numDepots-- > 0 && (line = file.ReadLine()) != null)
            {
                dvrp.Depots.Add(new Depot() { Id = int.Parse(line) });
            }
            return line;
        }

        private string setDemands(StreamReader file, int numVisits, DVRPProblemInstance dvrp)
        {
            string line = string.Empty;
            while (numVisits-- > 0 && (line = file.ReadLine()) != null)
            {
                dvrp.Visits.Add(new Visit() { Id = int.Parse(line.Split(' ')[2]), Demand = int.Parse(line.Split(' ')[3]) });
            }
            return line;
        }

        private string setLocations(StreamReader file, int numLocations, DVRPProblemInstance dvrp)
        {
            string line = string.Empty;
            while (numLocations-- > 0 && (line = file.ReadLine()) != null)
            {
                dvrp.Locations.Add(new Location() { Id = int.Parse(line.Split(' ')[2]), X = int.Parse(line.Split(' ')[3]), Y = int.Parse(line.Split(' ')[4]) });
            }
            return line;
        }

        private string setDepotLocations(StreamReader file, int numDepots, DVRPProblemInstance dvrp)
        {
            string line = string.Empty;
            while (numDepots-- > 0 && (line = file.ReadLine()) != null)
            {
                int ind = int.Parse(line.Split(' ')[2]);
                dvrp.Depots.First(d => d.Id == ind).Location = dvrp.Locations[int.Parse(line.Split(' ')[3])];
            }
            return line;
        }

        private string setVisitLocations(StreamReader file, int numVisits, DVRPProblemInstance dvrp)
        {
            string line = string.Empty;
            while (numVisits-- > 0 && (line = file.ReadLine()) != null)
            {
                int ind = int.Parse(line.Split(' ')[2]);
                int indL = int.Parse(line.Split(' ')[3]);
                dvrp.Visits.First(v => v.Id == ind).Location = dvrp.Locations.First(l => l.Id == indL);
            }
            return line;
        }

        private string setVisitDuration(StreamReader file, int numVisits, DVRPProblemInstance dvrp)
        {
            string line;
            while ((line = file.ReadLine()) != "DEPOT_TIME_WINDOW_SECTION")
            {
                int ind = int.Parse(line.Split(' ')[2]);
                dvrp.Visits.First(v => v.Id == ind).Duration = int.Parse(line.Split(' ')[3]);
            }
            return line;
        }

        private string setDepotTimeAvailability(StreamReader file, int numDepots, DVRPProblemInstance dvrp)
        {
            string line;
            while ((line = file.ReadLine()) != "COMMENT: TIMESTEP: 7")
            {
                int ind = int.Parse(line.Split(' ')[2]);
                dvrp.Depots.First(v => v.Id == ind).EarliestDepartureTime = int.Parse(line.Split(' ')[3]);
                dvrp.Depots.First(v => v.Id == ind).EarliestDepartureTime = int.Parse(line.Split(' ')[4]);

            }
            return line;
        }

        private string setVisitTimeAvailability(StreamReader file, int numVisits, DVRPProblemInstance dvrp)
        {
            string line;
            while ((line = file.ReadLine()) != "EOF")
            {
                int ind = int.Parse(line.Split(' ')[2]);
                dvrp.Visits.First(v => v.Id == ind).AvailabilityTime = int.Parse(line.Split(' ')[3]);

            }
            return line;
        }
    }
}
