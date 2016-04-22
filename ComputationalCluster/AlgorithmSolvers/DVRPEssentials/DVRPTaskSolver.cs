using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
            if (partialData == null || partialData.Length == 0)
                return null;
            //przerób bajty na PartialProblemInstance
            var converter = new ProblemToBytesConverter();
            var partialInstance = (DVRPPartialProblemInstance) converter.FromBytesArray(partialData);
            var instance = (DVRPProblemInstance) converter.FromBytesArray(_problemData);

            //dla każdego samochodu (listy klientów mu przypisanej) sprawdz poprawność kombinacji 
            //(w sensie żądań)
            if (!demandsValid(instance))
                return converter.ToByteArray(solutionImpossible());

            var n = instance.Visits.Count;
            var s = instance.VehicleNumber;
            var i = partialInstance.MinimalSetCount;

            if (s*i>n)
                return converter.ToByteArray(solutionImpossible());

            var solution = solvePossibleMinCountSets(instance, i);
				
				//	3.1.1.2.1 uruchom minimalizePermutation dla wszystkich samochodów tego zbioru
    //                3.1.1.2.2 jeżeli uzyskałeś permutację tańszą niż obecna w ref, to ją zaktualizuj

    //    3.2 uzyskasz minimalny koszt i minimalną wariację przydziałów - zwróć ją, lub błąd (np.koszt double.MinValue) -zwróć impossible

            return converter.ToByteArray(partialInstance);
            //timeoutu nie rozpatruj, szkoda zdrowia
        }

        /// <summary>
        /// wyznacz wszystkie możliwe przydziały i klientów do wszystkich s samochodów(rekurencja)
        /// </summary>
        /// <param name="instance">problem instance</param>
        /// <param name="i">minimal set count</param>
        /// <returns></returns>
        private DVRPPartialProblemInstance solvePossibleMinCountSets(DVRPProblemInstance instance, int i)
        {
            var solution = new DVRPPartialProblemInstance();
            solution.PartialResult = double.MinValue;
            solution.SolutionResult = SolutionResult.Impossible;
            var visits = new int[instance.VehicleNumber][];
            var currVisits = new List<int>[instance.VehicleNumber];
            for (var j = 0; j < instance.VehicleNumber; j++)
            {
                currVisits[j] = new List<int>();
            }

            solvePossibleMinCountSetsRec(instance, i, 0, 0, ref solution, currVisits);
            return solution;
        }

        private void solvePossibleMinCountSetsRec(DVRPProblemInstance instance, int i, int set, int j,
            ref DVRPPartialProblemInstance solution, List<int>[] currVisits)
        {
            if (j > i)
                throw new Exception("Something went wrong");
            if (j == i)
            {
                if (set < instance.VehicleNumber - 1)
                    solvePossibleMinCountSetsRec(instance, i, set + 1, 0, ref solution, currVisits);
                else
                {
                    //uzyskaliśmy podział, możemy rozróżniać zbiory y~
                    distinguishSubsets(instance, i, ref solution, currVisits);
                    return;
                }
            }

            for (var k = 0; k < instance.Visits.Count; k++)
            {
                if (!currVisits.Any(x=> x.Contains(instance.Visits[k].Id)))
                {
                    if (!(set == 0 && j == 0) && (instance.Visits[k].Id < currVisits[0][0] 
                        || (currVisits[set].Count>0 && currVisits[set].Last() > instance.Visits[k].Id)))
                        continue;
                    currVisits[set].Add(instance.Visits[k].Id);
                    solvePossibleMinCountSetsRec(instance, i, set, j+1, ref solution, currVisits);
                    currVisits[set].Remove(instance.Visits[k].Id);
                }
            }
        }

        /// <summary>
        /// dla każdego z s zbiorów wyróżnij zbiory y1,...,yk takie, że 
        /// ignoruje się je w przydziale pozostałych klientów (uwaga na warunki)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="i"></param>
        /// <param name="solution"></param>
        /// <param name="totalCost"></param>
        /// <param name="currVisits"></param>
        private void distinguishSubsets(DVRPProblemInstance instance, int i, ref DVRPPartialProblemInstance solution, 
            List<int>[] currVisits)
        {
            var ignoredSubsets = new List<int>();
            //klienci pozostali (liczba)
            var t = instance.Visits.Count - i*instance.VehicleNumber;
            var s = instance.VehicleNumber;
            var minInd = 1;
            //ile zbiorów można zignorować - od minInd do s-1
            if (t < s)
                minInd = s - t;
            else if (t == s)
                minInd = 1;
            else
                minInd = 1;

            //rozpatrujemy zignorowanie j zbiorów
            for (var j = minInd; j < s; j++)
            {
                distinguishSubsetsRec (instance, i, j, 0, ref solution, currVisits, ignoredSubsets);
            }
        }

        private void distinguishSubsetsRec(DVRPProblemInstance instance, int i, int j, int cur, 
            ref DVRPPartialProblemInstance solution, 
            List<int>[] currVisits, List<int> y)
        {
            if (cur == j)
            {
                //wygenerowales takie podzbiory y, mozna wchodzic dalej wglab
                generateSets(instance, i, ref solution, currVisits, y);
                return;
            }
            for (var k = 0; k < instance.VehicleNumber; k++)
            {
                if (y.Count != 0 && (y.Contains(k) || y.Last() > k))
                    continue;
                y.Add(k);
                distinguishSubsetsRec(instance, i, j, cur+1, ref solution, currVisits, y);
                y.Remove(k);
            }
        }

        private void generateSets(DVRPProblemInstance instance, int i, ref DVRPPartialProblemInstance solution, 
            List<int>[] currVisits, List<int> y)
        {
            //dodawaj klientów z nawrotami do zbiorów currVisits takich, że nie należą do y-greków
            //uważaj na powtórzenia
            int clast = instance.Visits.Count - i*instance.VehicleNumber;

            generateSetsRec(instance, i, ref solution, currVisits, y, clast);

        }

        private void generateSetsRec(DVRPProblemInstance instance, int i, ref DVRPPartialProblemInstance solution, 
            List<int>[] currVisits, List<int> y, int clast)
        {
            if (clast == 0)
            {
                double newCost = 0f;
                var newSolution = new List<int>[instance.VehicleNumber];
                for (var j = 0; j < instance.VehicleNumber; j++)
                {
                    newSolution[j] = new List<int>();
                    for (int e=0;e<currVisits[j].Count;e++)
                        newSolution[j].Add(currVisits[j][e]);

                    var cvref = newSolution[j].ToArray();
                    var currCost = minimizePermutation(instance, ref cvref);
                    if (currCost < 0f)
                    {
                        return;
                    }
                    newCost += currCost;
                }
                if (solution.PartialResult < 0f || (solution.PartialResult > 0f && newCost < solution.PartialResult))
                {
                    solution.VisitIds = new int[instance.VehicleNumber][];
                    for (int u = 0; i < instance.VehicleNumber; u++)
                        solution.VisitIds[u] = newSolution[u].ToArray();
                    solution.PartialResult = newCost;
                    solution.SolutionResult = SolutionResult.Successful;
                    return;
                }
            }
            for (var k = 0; k < instance.VehicleNumber; k++)
            {
                if (y.Contains(k))
                    continue;

                foreach (Visit v in instance.Visits)
                {
                    var id = v.Id;
                    if (currVisits.Any(x => x.Contains(id)))
                        continue;
                    if (currVisits[k].Count > i && currVisits[k].Last() > id)
                        continue;
                    currVisits[k].Add(id);
                    generateSetsRec(instance, i, ref solution, currVisits, y, clast-1);
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
            var cost = double.MinValue;
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
                if (routeImpossible (instance, newVisits))
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
                minimizePermutationRec(instance, ref carVisits, currCost+lengthCost,
                    currCapacity - Math.Abs(visit.Demand), ref minCost, newVisits);
                //uwzględnianie powrotu do depota
                if (from != depot.Location && currCapacity < instance.VehicleCapacity)
                {
                    minimizePermutationRec(instance, ref carVisits, currCost +
                        getDistanceCost(from, depot.Location)+getDistanceCost(depot.Location, to),
                        instance.VehicleCapacity-Math.Abs(visit.Demand), ref minCost, newVisits);
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

            var currCapacity = instance.VehicleCapacity;

            //sprawdzenie w pętli czy da się dojechać z i-1 wizyty do i-tej wizyty w dobrym czasie
            for (var i = 0; i < newVisits.Count-1; i++)
            {
                var visit = instance.Visits.Single(x => x.Id == newVisits[i]);
                currTime += visit.Duration;
                var nextVisit = instance.Visits.Single(x => x.Id == newVisits[i + 1]);

                //autko nie ma już towaru, trzeba wrócić do depot:
                if (currCapacity + nextVisit?.Demand < 0)
                {
                    currTime+= getTimeCost(instance, depot.Location, visit.Location);
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
            return (getDistanceCost(from,to)/(double)instance.VehicleSpeed);
        }

        /// <summary>
        /// wylicza koszt odległościowy z from do to (euklidesowo)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private double getDistanceCost (Location from, Location to)
        {
            return Math.Sqrt(Math.Pow(from.X - to.X,2) + Math.Pow(from.Y - to.Y,2));
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
            return divideProblem(instance, converter);
        }

        private byte[][] divideProblem(DVRPProblemInstance instance, ProblemToBytesConverter converter)
        {
            var partialProblemInstances = new List<DVRPPartialProblemInstance>();

            for (var i = 0; i < instance.Visits.Count/instance.VehicleNumber; i++)
            {
                partialProblemInstances.Add(new DVRPPartialProblemInstance()
                {
                    MinimalSetCount = i,
                    PartialResult = double.MinValue,
                    SolutionResult = SolutionResult.NotSolved
                });  
            }

            return partialProblemInstances.Select(converter.ToByteArray).ToArray();
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
                (solution => (DVRPPartialProblemInstance) converter.FromBytesArray(solution)).ToList();
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
