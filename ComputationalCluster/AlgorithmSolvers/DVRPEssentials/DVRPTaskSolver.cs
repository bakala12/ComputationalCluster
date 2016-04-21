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
            foreach (var carVisits in partialInstance.VisitIds)
            {
                if (!demandsValid(instance, carVisits))
                    return solutionImpossible();
            }

            //po wstepnej walidacji, czas na takie ustawienie id'ków dla każdego samochodu,
            //aby koszt był najmniejszy
            double totalCost = 0;

            //permutowanie każdego zbioru dla samochodów
            for (var i = 0; i<partialInstance.VisitIds.Length; i++)
            { 
                //jeżeli permutacja jest niemożliwa, cały podproblem jest niemożliwy, więc zwróć Impossible
                var currCost = minimizePermutation(instance, ref partialInstance.VisitIds[i]);
                if (currCost < 0)
                {
                    return solutionImpossible();
                }
                totalCost += currCost;
            }
            //udało się:
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
        private bool routeImpossible(DVRPProblemInstance instance, List<int> newVisits)
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
        private byte[] solutionImpossible()
        {
            var converter = new ProblemToBytesConverter();
            return converter.ToByteArray(new DVRPPartialProblemInstance()
            {
                PartialResult = double.MaxValue,
                SolutionResult = SolutionResult.Impossible,
                VisitIds = null
            });
        }

        /// <summary>
        /// sprawdza czy klient nie ma zamówienia > vehicleCapacity (lepiej byłoby zrobić to w
        /// divide, ale to jest zbyt trudne)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="carVisits"></param>
        /// <returns>true jeżeli warunki są ok</returns>
        private bool demandsValid(DVRPProblemInstance instance, int[] carVisits)
        {
            return !carVisits.Any
                (x => Math.Abs(instance.Visits.Single(v => v.Id == x).Demand) > instance.VehicleCapacity);
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
            var currVisits = new List<int>[instance.VehicleNumber];
           
            divideProblemRec(ref partialProblemInstances, currVisits, instance.Visits, 0,
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
            List<int>[] currVisits,
            IReadOnlyList<Visit> visits, int i, int vehicleNumber)
        {
            //można dodać nowy problem
            if (i == visits.Count)
            {
                //deep copy
                var cpy = new DVRPPartialProblemInstance();
                var arrCpy = new List<int>[vehicleNumber];
                for (var k = 0; k < vehicleNumber; k++)
                {
                    if (currVisits[k] == null)
                    {
                        arrCpy[k] = new List<int>();
                        continue;
                    }

                    arrCpy[k] = new List<int>();
                    for (var s = 0; s < currVisits[k].Count; s++)
                    {
                        arrCpy[k].Add(currVisits[k][s]);
                    }
                }
                cpy.VisitIds = new int[vehicleNumber][];
                for (var j = 0; j < vehicleNumber; j++)
                    cpy.VisitIds[j] = arrCpy[j].ToArray();
                cpy.SolutionResult = SolutionResult.NotSolved; 
                problems.Add(cpy);

                return;
            }
            else
            {
                //klienta i przypisujemy do j samochodu, wchodzimy w rekursję dla i+1 klienta,
                //cofamy się - i-tego klienta przypisujemy do j+1 samochodu, itd...
                //algorytm z nawrotami
                for (var j = 0; j < vehicleNumber; j++)
                {
                    if (currVisits[j] == null)
                        currVisits[j] = new List<int>();

                    currVisits[j].Add(visits[i].Id);
                    divideProblemRec(ref problems, currVisits, visits, i+1, vehicleNumber);
                    currVisits[j].Remove(visits[i].Id);
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
