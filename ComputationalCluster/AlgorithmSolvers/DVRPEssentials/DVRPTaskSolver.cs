using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace AlgorithmSolvers.DVRPEssentials
{
    /// <summary>
    /// Task solver implementation for DVRP
    /// </summary>
    public class DvrpTaskSolver : TaskSolver
    {
        public DvrpTaskSolver(byte[] problemData) : base(problemData)
        {
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            //przerób bajty na PartialProblemInstance
            //w pętli (dla każdego samochodu odpal jakąś funkcję, która posortuje
            //klientów tak by koszt był najniższy
            //jeżeli nie można (demands są za duże lub czasy są złe)
            //to wyjdź
            //timeoutu nie rozpatruj, szkoda zdrowia
            throw new NotImplementedException();
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var converter = new ProblemToBytesConverter();
            var instance = (DVRPProblemInstance)converter.FromBytesArray(_problemData);
            return divideProblem(instance, converter);
        }

        private byte[][] divideProblem(DVRPProblemInstance instance, ProblemToBytesConverter converter)
        {
            var partialProblemInstances = new List<DVRPPartialProblemInstance>();
            var currProblem = new DVRPPartialProblemInstance()
            {
                VisitIds = new List<int>[instance.VehicleNumber]
            };
           
            divideProblemRec(ref partialProblemInstances, currProblem, instance.Visits, 0,
                instance.VehicleNumber);

            return partialProblemInstances.Select(converter.ToByteArray).ToArray();
        }

        /// <summary>
        /// rekursja do dzielenia
        /// </summary>
        /// <param name="problems">referencja by dodawać problemy</param>
        /// <param name="currProblem">obecnie konstruowany problem (zmienna automatyczna)</param>
        /// <param name="visits">id'ki do powpisywania gdzieś</param>
        /// <param name="i">obecny klient</param>
        /// <param name="vehicleNumber">niezbędna informacja</param>
        private void divideProblemRec(ref List<DVRPPartialProblemInstance> problems, 
            DVRPPartialProblemInstance currProblem,
            IReadOnlyList<Visit> visits, int i, int vehicleNumber)
        {
            //można dodać nowy problem
            if (i == visits.Count)
            {
                //nie wiem czy to działa - nie znam się na C#:
                problems.Add(currProblem);
                return;
            }
            else
            {
                //klienta i przypisujemy do j samochodu, wchodzimy w rekursję dla i+1 klienta,
                //cofamy się - i-tego klienta przypisujemy do j+1 samochodu, itd...
                //algorytm z nawrotami
                for (var j = 0; j < vehicleNumber; j++)
                {
                    currProblem.VisitIds[j].Add(visits[i].Id);
                    divideProblemRec(ref problems, currProblem, visits, i+1, vehicleNumber);
                    currProblem.VisitIds[j].Remove(visits[i].Id);
                }
            }
        }

        public override byte[] MergeSolution(byte[][] solutions)
        {
            var converter = new ProblemToBytesConverter();
            var partialSolutions = solutions.Select
                (solution => (DVRPPartialProblemInstance) converter.FromBytesArray(solution)).ToList();
            var imin = 0;
            var minCost = int.MaxValue;
            for (var i = 0; i < solutions.GetLength(0); i++)
            {
                if (partialSolutions[i].PartialResult >= minCost) continue;
                imin = i;
                minCost = partialSolutions[i].PartialResult;
            }
            return solutions[imin];
        }

        public override string Name { get; }
    }
}
