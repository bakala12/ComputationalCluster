using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;
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
            if (partialData == null || partialData.Length == 0)
                return null;
            //przerób bajty na PartialProblemInstance
            var converter = new ProblemToBytesConverter();
            var partialInstance = (DVRPPartialProblemInstance)converter.FromBytesArray(partialData);
            var instance = (DVRPProblemInstance)converter.FromBytesArray(_problemData);

            //dla każdego samochodu (listy klientów mu przypisanej) sprawdz poprawność kombinacji 
            //(w sensie żądań)
            if (!demandsValid(instance))
                return converter.ToByteArray(solutionImpossible());

            var n = instance.Visits.Count;
            var s = instance.VehicleNumber;
            var i = partialInstance.MinimalSetCount;

            if (s * i > n)
                return converter.ToByteArray(solutionImpossible());

            var minSets = partialInstance.MinimalIgnoredSets;
            var maxSets = partialInstance.MaximumIgnoredSets;
            partialInstance.PartialResult = double.MaxValue;

            var currVisits = new List<int>[instance.VehicleNumber];
            for (int j = 0; j < instance.VehicleNumber; j++)
            {
                currVisits[j] = new List<int>();
                for (int k=0;k<partialInstance.VisitIds[j].Length;k++)
                    currVisits[j].Add(partialInstance.VisitIds[j][k]);
            }

            solveInParallel(instance, ref partialInstance, currVisits, double.MaxValue, i, minSets, maxSets);

            return converter.ToByteArray(partialInstance);
            //timeoutu nie rozpatruj, szkoda zdrowia
        }

        private void solveInParallel(DVRPProblemInstance instance, ref DVRPPartialProblemInstance finalSolution,
            List<int>[] currVisits, double currCost, int i, int minSets, int maxSets)
        {
            var lastIds = new int[instance.Visits.Count];
            for (var j = 0; j < instance.Visits.Count; j++)
            {
                if (currVisits.Any(x => x.Contains(instance.Visits[j].Id)))
                    lastIds[j] = -1;
                else
                    lastIds[j] = instance.Visits[j].Id;
            }
            var currSolution = new DVRPPartialProblemInstance();

            for (var j = minSets; j <= maxSets; j++)
            {
                currSolution = new DVRPPartialProblemInstance();
                currSolution.MinimalIgnoredSets = j;
                currSolution.MaximumIgnoredSets = j;
                currSolution.PartialResult = double.MaxValue;
                currSolution.SolutionResult = SolutionResult.NotSolved;
                generateSets(instance, i, ref currSolution, currVisits, lastIds, j);
                if (currSolution.PartialResult < finalSolution.PartialResult)
                {
                    solutionCopyTo(currSolution, ref finalSolution);
                }
            }
        }

        private void solutionCopyTo(DVRPPartialProblemInstance a, ref DVRPPartialProblemInstance b)
        {
            b.MaximumIgnoredSets = a.MaximumIgnoredSets;
            b.MinimalIgnoredSets = a.MinimalIgnoredSets;
            b.MinimalSetCount = a.MinimalSetCount;
            b.SolutionResult = a.SolutionResult;
            b.PartialResult = a.PartialResult;
            b.VisitIds = new int[a.VisitIds.GetLength(0)][];
            for (int i = 0; i < a.VisitIds.GetLength(0); i++)
            {
                b.VisitIds[i] = new int[a.VisitIds[i].Length];
                a.VisitIds[i].CopyTo(b.VisitIds[i], 0);
            }
        }

        private void generateSets(DVRPProblemInstance instance, int i, ref DVRPPartialProblemInstance solution,
            List<int>[] currVisits, int[] lastIds, int ignoredCount)
        {
            //dodawaj klientów z nawrotami do zbiorów currVisits takich, że nie należą do y-greków
            //uważaj na powtórzenia
            var clast = instance.Visits.Count - i * instance.VehicleNumber;
            var cval = instance.VehicleNumber;

            generateSetsRec(instance, i, ref solution, currVisits, ignoredCount, clast, cval, lastIds, 0);

        }

        private void generateSetsRec(DVRPProblemInstance instance, int i, ref DVRPPartialProblemInstance solution,
            List<int>[] currVisits, int ignoredCount, int clast, int cval, int[] lastIds, int minSet)
        {
            if (clast == 0 && ignoredCount != 0 && cval > ignoredCount)
                return;
            if (clast == 0)
            {
                double newCost = 0f;
                var newSolution = new List<int>[instance.VehicleNumber];
                for (var j = 0; j < instance.VehicleNumber; j++)
                {
                    newSolution[j] = new List<int>();
                    for (int e = 0; e < currVisits[j].Count; e++)
                        newSolution[j].Add(currVisits[j][e]);

                    var cvref = newSolution[j].ToArray();
                    var currCost = minimizePermutation(instance, ref cvref);
                    if (currCost < 0f)
                    {
                        return;
                    }
                    newCost += currCost;
                }
                if (newCost < solution.PartialResult)
                {
                    solution.VisitIds = new int[instance.VehicleNumber][];
                    for (int u = 0; u < instance.VehicleNumber; u++)
                        solution.VisitIds[u] = newSolution[u].ToArray();
                    solution.PartialResult = newCost;
                    solution.SolutionResult = SolutionResult.Successful;
                }
                return;
            }

            for (var k = minSet; k < instance.VehicleNumber; k++)
            {
                if (currVisits[k].Count == i && cval == ignoredCount)
                    continue;
                if (k > 0 && currVisits[k - 1].Count == i)
                    return;

                for (var t = 0; t < lastIds.Length; t++)
                {
                    if (lastIds[t] == -1)
                        continue;
                    if (currVisits[k].Count > i && currVisits[k].Last() > lastIds[t])
                        continue;
                    if (k > 0 && currVisits[k - 1][0] > lastIds[t])
                        continue;
                    var add = currVisits[k].Count == i ? (byte)1 : (byte)0;
                    currVisits[k].Add(lastIds[t]);
                    var tmp = lastIds[t];
                    lastIds[t] = -1;
                    generateSetsRec(instance, i, ref solution, currVisits, ignoredCount, 
                        clast - 1, cval - add, lastIds,k);
                    lastIds[t] = tmp;
                    currVisits[k].Remove(lastIds[t]);
                }
            }
        }

        /// <summary>
        /// minimalizacja kosztu
        /// </summary>
        /// <param name="instance">dane całego problemu</param>
        /// <param name="carVisits">permutacje dla samochodu - do minimalizacji kosztu</param>
        /// <returns>koszt minimalnej permutacji, -1 w przypadku nieistniejacej permutacji (dla warunkow czasowych)
        /// </returns>
        private double minimizePermutation(DVRPProblemInstance instance, ref int[] carVisits)
        {
            if (carVisits.Length == 0)
                return 0f;
            //generacja wszystkich permutacji i sprawdzanie kosztu (zlozonosc n!)
            //permutacja generowana w rekursji
            var newVisits = new List<int>();
            var cost = double.MaxValue;
            minimizePermutationRec(instance, ref carVisits, 0,
                instance.VehicleCapacity, ref cost, newVisits);
            return cost;
        }

        /// <summary>
        /// algorytm z nawrotami
        /// </summary>
        /// <param name="instance">instancja problemu</param>
        /// <param name="carVisits">minimalna permutacja klientów dla samochodu</param>
        /// <param name="currCost">koszt obecnie budowanej permutacji (odległość)</param>
        /// <param name="currCapacity">ile autku zostało w bagażniku towaru</param>
        /// <param name="minCost">koszt carVisits (tzn. minimalny)</param>
        /// <param name="newVisits">aktualnie budowana permutacja</param>
        private void minimizePermutationRec(DVRPProblemInstance instance, ref int[] carVisits,
            double currCost, int currCapacity, ref double minCost, List<int> newVisits)
        {
            //zbudowalismy pewna permutacje, sprawdzenie czy jest dobra i ew. aktualizacja refów
            if (newVisits.Count == carVisits.Length)
            {
                if (routeImpossible(instance, newVisits))
                    return;
                //dodatkowo dodany koszt drogi powrotnej do depotu:
                double realCost = currCost + getDistanceCost(instance.Depots.Single().Location,
                    instance.Visits.Single(x => x.Id == newVisits.Last()).Location);
                //żeby nadpisać ujemną liczbę coś takiego musi być:
                if (realCost >= minCost && minCost > 0f)
                    return;
                minCost = realCost;
                //deep copy
                var cpy = new int[carVisits.Length];
                newVisits.CopyTo(cpy);
                carVisits = cpy;

                return;
            }

            //rekursywna generacja permutacji
            for (var i = 0; i < carVisits.Length; i++)
            {
                if (currCapacity < 0)
                    return;

                var visitId = carVisits[i];

                if (newVisits.Contains(visitId)) continue;
                //if (newVisits.Count == 0 && i > (carVisits.Length + 1)/2)
                //    return;
                //if (newVisits.Count == 1 && i > (carVisits.Length + 1)/2)
                //    return;

                //ogólnie to paskudnie wygląda (bo visits to tylko inty do visit.Id)
                var visit = instance.Visits.Single(x => x.Id == visitId);
                var depot = instance.Depots.Single();

                var from = newVisits.Count == 0
                    ? depot.Location
                    : instance.Visits.Single(x => x.Id == newVisits.Last()).Location;

                var to = visit.Location;
                //dodawanie kosztu (w sensie dystansu)
                //autko nie ma towaru dla tego klienta, trzeba nawrócić do depota

                var lengthCost = getDistanceCost(from, to);
                newVisits.Add(visitId);
                var nextvToDepot = getDistanceCost(depot.Location, to);
                var curvToDepot = getDistanceCost(from, depot.Location);
                //uwzględnianie powrotu do depota

                if (from != depot.Location && currCapacity + visit.Demand <= 0)
                {
                    if (currCost +
                        curvToDepot +
                        2*nextvToDepot > minCost)
                    {
                        newVisits.Remove(visitId);
                        return;
                    }

                    minimizePermutationRec(instance, ref carVisits, currCost +
                        curvToDepot +
                        nextvToDepot,
                        instance.VehicleCapacity - Math.Abs(visit.Demand), ref minCost, newVisits);
                }
                else
                {
                    if (currCost + lengthCost + nextvToDepot > minCost)
                    {
                        newVisits.Remove(visitId);
                        return;
                    }
                    minimizePermutationRec(instance, ref carVisits, currCost + lengthCost,
                    currCapacity - Math.Abs(visit.Demand), ref minCost, newVisits);
                }

                newVisits.Remove(visitId);
            }
        }

        /// <summary>
        /// sprawdza, czy droga ma sens pod względem wymagań czasowych
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="newVisits"></param>
        /// <returns>true jeżeli droga jest chujowa</returns>
        private bool routeImpossible(DVRPProblemInstance instance, IReadOnlyList<int> newVisits)
        {
            //sprawdzenie czy sie zdazy dojechac z depotu do pierwszej wizyty
            var depot = instance.Depots.Single();
            var firstVisit = instance.Visits.Single(x => x.Id == newVisits[0]);
            var currTime = (double)depot.EarliestDepartureTime +
                getTimeCost(instance, depot.Location, firstVisit.Location);

            //przyjechaliśmy przed otwarciem depota. trzeba zaczekać na otwarcie
            //interesuje nas nie minimalizacja czasu, ale drogi, więc może tak być:
            if (currTime < firstVisit.AvailabilityTime)
                currTime = (double)firstVisit.AvailabilityTime;
            //if (currTime + getTimeCost(instance, firstVisit.Location, depot.Location) >= depot.LatestReturnTime)
            //    return true;

            var currCapacity = instance.VehicleCapacity;

            //sprawdzenie w pętli czy da się dojechać z i-1 wizyty do i-tej wizyty w dobrym czasie
            for (var i = 0; i < newVisits.Count - 1; i++)
            {
                var visit = instance.Visits.Single(x => x.Id == newVisits[i]);
                currTime += visit.Duration;
                var nextVisit = instance.Visits.Single(x => x.Id == newVisits[i + 1]);
                //if (currTime + getTimeCost(instance, visit.Location, depot.Location) >= depot.LatestReturnTime)
                //    return true;

                //autko nie ma już towaru, trzeba wrócić do depot:
                if (currCapacity + nextVisit?.Demand < 0)
                {
                    currTime += getTimeCost(instance, depot.Location, visit.Location);
                    currTime += getTimeCost(instance, depot.Location, nextVisit.Location);
                    currCapacity = instance.VehicleCapacity;
                }
                else
                {
                    currTime += getTimeCost(instance, visit.Location, nextVisit.Location);
                }
                //podobnie jak wyżej, poczekanie na otwarcie klienta
                if (currTime < nextVisit.AvailabilityTime)
                    currTime = (double)nextVisit.AvailabilityTime;
                //if (currTime + getTimeCost(instance, nextVisit.Location, depot.Location) >= depot.LatestReturnTime)
                //    return true;
            }

            //sprawdzenie, czy sie zdazy dojechac z ostatniej wizyty do depotu
            var lastVisit = instance.Visits.Single(x => x.Id == newVisits.Last());
            currTime += lastVisit.Duration + getTimeCost(instance, lastVisit.Location, depot.Location);
            return currTime > depot.LatestReturnTime;
        }

        /// <summary>
        /// wylicza koszt czasowy (t = s/V)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private double getTimeCost(DVRPProblemInstance instance, Location from, Location to)
        {
            return (getDistanceCost(from, to) / (double)instance.VehicleSpeed);
        }

        /// <summary>
        /// wylicza koszt odległościowy z from do to (euklidesowo)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private double getDistanceCost(Location from, Location to)
        {
            return Math.Sqrt(Math.Pow(from.X - to.X, 2) + Math.Pow(from.Y - to.Y, 2));
        }

        /// <summary>
        /// templatka do zwracania enuma z Impossible
        /// </summary>
        /// <returns></returns>
        private DVRPPartialProblemInstance solutionImpossible()
        {
            var converter = new ProblemToBytesConverter();
            return new DVRPPartialProblemInstance()
            {
                PartialResult = double.MaxValue,
                SolutionResult = SolutionResult.Impossible,
                VisitIds = null
            };
        }

        /// <summary>
        /// sprawdza czy klient nie ma zamówienia > vehicleCapacity (lepiej byłoby zrobić to w
        /// divide, ale to jest zbyt trudne)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>true jeżeli warunki są ok</returns>
        private bool demandsValid(DVRPProblemInstance instance)
        {
            return !instance.Visits.Any
                (x => x.Demand + instance.VehicleCapacity < 0);
        }

        ///
        ///
        ///DIVIDE PROBLEM
        /// 
        /// 
        public override byte[][] DivideProblem(int threadCount)
        {
            if (_problemData == null || _problemData.Length == 0)
                return null;

            var converter = new ProblemToBytesConverter();
            var instance = (DVRPProblemInstance)converter.FromBytesArray(_problemData);
            var problems = divideProblem(instance);
            return problems.Select(x => converter.ToByteArray(x)).ToArray();
        }

        private DVRPPartialProblemInstance[] divideProblem(DVRPProblemInstance instance)
        {
            var partialProblemInstances = new List<DVRPPartialProblemInstance>();

            for (var i = 0; i <= instance.Visits.Count / instance.VehicleNumber; i++)
            {
                //generacja permutacji dla i
                //generacja partial problemów - minimalingored, maximumignored
                generatePartialProblems(instance, i, ref partialProblemInstances);
            }

            return partialProblemInstances.ToArray();
        }

        private void generatePartialProblems(DVRPProblemInstance instance, int i, 
            ref List<DVRPPartialProblemInstance> partialProblemInstances)
        {
            var schemas = solvePossibleMinCountSets(instance, i);
            
            foreach (var schema in schemas)
            {
                var j = instance.Visits.Count - i * instance.VehicleNumber == 0 ? 0 : 1;
                while (j < instance.VehicleNumber)
                {
                    var newPartial = new DVRPPartialProblemInstance();
                    solutionCopyTo(schema, ref newPartial);
                    if (i == 0 && j < 3)
                    {
                        newPartial.MinimalIgnoredSets = j;
                        newPartial.MaximumIgnoredSets = j;
                    }
                    else
                    {
                        newPartial.MinimalIgnoredSets = j;
                        newPartial.MaximumIgnoredSets = j + 1 < instance.VehicleNumber ? j + 1 : j;
                        j++;
                    }
                    partialProblemInstances.Add(newPartial);
                    j++;
                }
            }
        }

        /// <summary>
        /// wyznacz wszystkie możliwe przydziały i klientów do wszystkich s samochodów(rekurencja)
        /// </summary>
        /// <param name="instance">problem instance</param>
        /// <param name="i">minimal set count</param>
        /// <returns></returns>
        private List<DVRPPartialProblemInstance> solvePossibleMinCountSets(DVRPProblemInstance instance, int i)
        {
            var solution = new List<DVRPPartialProblemInstance>();

            var currVisits = new List<int>[instance.VehicleNumber];
            for (var j = 0; j < instance.VehicleNumber; j++)
            {
                currVisits[j] = new List<int>();
            }
            var lastIds = new int[instance.Visits.Count];
            int ind = 0;

            foreach (var v in instance.Visits)
                lastIds[ind++] = v.Id;
            solvePossibleMinCountSetsRec(instance, i, 0, 0, ref solution, currVisits, lastIds);
            return solution;
        }

        private void solvePossibleMinCountSetsRec(DVRPProblemInstance instance, int i, int set, int j,
            ref List<DVRPPartialProblemInstance> solutions, List<int>[] currVisits, int[] lastIds)
        {
            if (j > i)
                throw new Exception("Something went wrong");
            if (j == i)
            {
                if (set < instance.VehicleNumber - 1)
                {
                    solvePossibleMinCountSetsRec(instance, i, set + 1, 0, ref solutions, currVisits, lastIds);
                    return;
                }
                else
                {
                    var newSol = new DVRPPartialProblemInstance();
                    newSol.PartialResult = double.MaxValue;
                    newSol.SolutionResult = SolutionResult.Impossible;
                    newSol.MinimalSetCount = i;
                    //uzyskaliśmy podział, przepisanie do szkieletu zadania
                    newSol.VisitIds = new int[currVisits.Length][];
                    for (var k = 0; k < currVisits.Length; k++)
                    {
                        newSol.VisitIds[k] = new int[currVisits[k].Count];
                        for (var s = 0; s < currVisits[k].Count; s++)
                            newSol.VisitIds[k][s] = currVisits[k][s];
                    }
                    solutions.Add(newSol);
                    return;
                }
            }

            for (var k = 0; k < lastIds.Length; k++)
            {
                if (lastIds[k] != -1)
                {
                    if ((set>0 && j==0 && currVisits[set-1].Last() > lastIds[k]) || 
                        (!(set == 0 && j == 0) && (instance.Visits[k].Id < currVisits[0][0]
                        || (currVisits[set].Count > 0 && currVisits[set].Last() > instance.Visits[k].Id)
                        || (currVisits[0].Count > 0 && currVisits[0][0] > instance.Visits[k].Id))))
                        continue;
                    var id = lastIds[k];
                    currVisits[set].Add(id);
                    lastIds[k] = -1;
                    solvePossibleMinCountSetsRec(instance, i, set, j + 1, ref solutions, currVisits, lastIds);
                    lastIds[k] = id;
                    currVisits[set].Remove(id);
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
            if (solutions == null || solutions.GetLength(0) == 0)
                return null;
            var converter = new ProblemToBytesConverter();
            var partialSolutions = solutions.Select
                (solution => (DVRPPartialProblemInstance)converter.FromBytesArray(solution)).ToList();
            var imin = 0;
            var minCost = double.MaxValue;
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
