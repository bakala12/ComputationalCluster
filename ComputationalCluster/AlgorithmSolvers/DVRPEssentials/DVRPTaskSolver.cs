using System;
using System.Collections.Generic;
using System.Data;
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

        ///
        ///
        ///SOLVE PROBLEM
        /// 
        /// 
        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            //przerób bajty na PartialProblemInstance
            var converter = new ProblemToBytesConverter();
            var partialInstance = (DVRPPartialProblemInstance) converter.FromBytesArray(partialData);
            var instance = (DVRPProblemInstance) converter.FromBytesArray(_problemData);
            //dla każdego samochodu (listy klientów mu przypisanej) sprawdz poprawność kombinacji
            foreach (var carVisits in partialInstance.VisitIds)
            {
                //sprawdź czy demands < capacity
                if (!checkDemands(instance, carVisits, instance.VehicleCapacity))
                {
                    return solutionImpossible();
                }

            }
            //po wstepnej walidacji, czas na takie ustawienie id'ków dla każdego samochodu,
            //aby koszt był najmniejszy (jak się nie da, to zwróć -1)
            int totalCost = 0;

            for (var i = 0; i<partialInstance.VisitIds.Length; i++)
            {
                var currCost = minimizePermutation(instance, ref partialInstance.VisitIds[i]);
                if (currCost == -1)
                {
                    return solutionImpossible();
                }

                totalCost += currCost;
            }

            partialInstance.SolutionResult = SolutionResult.Successful;
            partialInstance.PartialResult = totalCost;
            return converter.ToByteArray(partialInstance);
            //timeoutu nie rozpatruj, szkoda zdrowia
        }

        /// <summary>
        /// minimalizacja kosztu
        /// </summary>
        /// <param name="instance">dane całego problemu</param>
        /// <param name="carVisits">permutacje dla samochodu - do minimalizacji kosztu</param>
        /// <returns>koszt minimalnej permutacji, -1 w przypadku permutacji niemozliwej</returns>
        private int minimizePermutation(DVRPProblemInstance instance, ref List<int> carVisits)
        {
            //generacja wszystkich permutacji i sprawdzanie kosztu (zlozonosc n!)
            throw new NotImplementedException();
        }

        private byte[] solutionImpossible()
        {
            var converter = new ProblemToBytesConverter();
            return converter.ToByteArray(new DVRPPartialProblemInstance()
            {
                SolutionResult = SolutionResult.Impossible
            });
        }

        private bool checkDemands(DVRPProblemInstance instance, List<int> carVisits, int vehicleCapacity)
        {
            var demands = carVisits.Sum(v => instance.Visits.Single(x => x.Id == v).Demand);
            return Math.Abs(demands) < vehicleCapacity;
        }

        ///
        ///
        ///DIVIDE PROBLEM
        /// 
        /// 
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
        ///
        ///
        ///MERGE SOLUTION
        /// 
        /// 
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
