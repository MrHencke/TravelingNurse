using TravelingNurse.Extensions;
using TravelingNurse.Util;
using static System.Console;
using CU = TravelingNurse.Util.ConsoleUtils;

namespace TravelingNurse.Models
{
    public class GeneticAlgorithm
    {
        public Instance Instance { get; init; }
        public FitnessFunctionType FitnessFunction { get; init; }
        public FitnessFunctionType PenaltyFunction { get; init; }
        public ParentSelectionType ParentSelectionFunction { get; init; }
        public CrossoverFunctionType CrossoverFunction { get; init; }
        public SurvivorSelectionType SurvivorSelectionFunction { get; init; }
        public MutationFunctionType MutationFunction { get; init; }
        public double CrossoverRate { get; init; } = 0.9;
        public double MutationRate { get; init; } = 0.01;
        public int MaxIterations { get; init; } = 10000;
        public int PopulationSize { get; init; } = 500;
        public List<Individual> Population { get; private set; }
        public Individual BestIndividual { get; private set; } = new Individual();
        public Individual BestValidIndividual { get; private set; } = new Individual();
        public Individual BestIndividualLastRound { get; private set; } = new Individual();
        public int RoundBestIndividualAquired { get; private set; } = 0;
        public int MaxRoundsWithoutImprovement { get; private set; } = 300;
        public Func<int, double> BenchmarkThreshold { get; init; }
        public List<int> BenchmarkTargets = new() { 30, 20, 10, 5 };
        public bool Debug { get; init; } = false;

        /// <summary>
        /// Quick constructor
        /// </summary>
        public GeneticAlgorithm(Instance instance)
        {
            Instance = instance;
            FitnessFunction = (Genotype x) => FitnessFunctions.FitnessFunc(instance.TravelTimes, x);
            PenaltyFunction = (Genotype x) => FitnessFunctions.PenaltyFunction(instance.Patients, instance.TravelTimes, instance.Depot.ReturnTime, instance.CapacityNurse, x);
            ParentSelectionFunction = ParentSelectionFunctions.RouletteSelection;
            SurvivorSelectionFunction = SurvivorSelectionFunctions.SafeDistinctGreedySurvivorFunc;
            MutationFunction = MutationFunctions.InsertionMutationElementWise;
            CrossoverFunction = (List<IndividualTuple> x) => CrossoverFunctions.RepeatCrossover(CrossoverFunctions.SingleRoutePatientsSwap, FitnessFunction, PenaltyFunction, CrossoverRate, MutationFunction, MutationRate, 4, x);
            Population = GenerateInitialPopulation(PopulationSize);
            BenchmarkThreshold = (int percentage) => Instance.Benchmark + (Instance.Benchmark / 100 * percentage);
        }

        /// <summary>
        /// Full constructor.
        /// </summary>
        public GeneticAlgorithm(
            Instance instance,
            FitnessFunctionType fitnessFunction,
            FitnessFunctionType penaltyFunction,
            ParentSelectionType parentSelectionFunction,
            CrossoverFunctionType crossoverFunction,
            SurvivorSelectionType survivorSelectionFunction,
            MutationFunctionType mutationFunction,
            int populationSize,
            double mutationRate,
            double crossoverRate
        )
        {
            Instance = instance;
            FitnessFunction = fitnessFunction;
            PenaltyFunction = penaltyFunction;
            ParentSelectionFunction = parentSelectionFunction;
            CrossoverFunction = crossoverFunction;
            SurvivorSelectionFunction = survivorSelectionFunction;
            MutationFunction = mutationFunction;
            Population = GenerateInitialPopulation(populationSize);
            PopulationSize = populationSize;
            MutationRate = mutationRate;
            CrossoverRate = crossoverRate;
            BenchmarkThreshold = (int percentage) => Instance.Benchmark + (Instance.Benchmark / 100 * percentage);
        }

        /// <summary>
        /// Runs the genetic algorithm for MaxIterations iterations, also provides debug messages and reassigns best individuals.
        /// </summary>
        public void Run()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            double nextThreshold = BenchmarkThreshold(BenchmarkTargets.First());
            for (int i = 1; i < MaxIterations; i++)
            {
                int validCount = Population.Where(x => x.IsValid).Count();
                int invalidCount = Population.Count - validCount;
                if (Debug)
                {
                    Clear();
                    WriteLine($"Running on {Instance.InstanceName}.");
                    WriteLine($"Benchmark: {CU.Blue(Instance.Benchmark.ToString())}. Target Threshold: {CU.Blue(BenchmarkThreshold(5).ToString())}. Next Threshold: {CU.Blue(nextThreshold.ToString())}");
                    WriteLine($"Elapsed time: {CU.Yellow(Convert.ToInt32(watch.Elapsed.TotalSeconds).ToString())}s: @ Iteration {i}/{MaxIterations}. {CU.Green("Valid")}/{CU.Red("Invalid")} individuals: {CU.Green(validCount.ToString())}/{CU.Red(invalidCount.ToString())}.");
                    WriteLine($"Best overall fitness: {(BestIndividual.IsValid ? CU.Green($"{BestIndividual.Fitness} (valid)") : CU.Red($"{BestIndividual.Fitness} (invalid)"))}{(!BestIndividual.IsValid ? ", " + CU.Green($"{BestValidIndividual.Fitness} (valid)") : "")}");
                    WriteLine($"Best fitness last round: {(BestIndividualLastRound.IsValid ? CU.Green($"{BestIndividualLastRound.Fitness} (valid)") : CU.Red($"{BestIndividualLastRound.Fitness} (invalid)"))}");
                    if (i > RoundBestIndividualAquired + MaxRoundsWithoutImprovement / 3)
                    {
                        int roundsUntilEarlyTermination = RoundBestIndividualAquired + MaxRoundsWithoutImprovement - i;
                        WriteLine($"\nWill terminate in: {CU.Yellow(roundsUntilEarlyTermination.ToString())} rounds if no more improvements.");
                    }
                }
                else
                {
                    WriteLine("Best fitness: " + CU.Green($"{BestValidIndividual.Fitness}"));
                }

                // Get parents
                var parents = ParentSelectionFunction(Population);

                // Generate offspring, mutate and return as tuple
                var (matched_parents, offspring) = CrossoverFunction(parents);

                // Decide survivors and squash back into a list.
                Population = SurvivorSelectionFunction(matched_parents, offspring, PopulationSize);

                GetBestIndividual(i);

                if (i - RoundBestIndividualAquired == MaxRoundsWithoutImprovement)
                {
                    WriteLine($"\nTerminating after {watch.Elapsed.TotalSeconds} secs at round {i} due to no improvement for {MaxRoundsWithoutImprovement} rounds.");
                    break;
                }

                if (BestValidIndividual.Fitness <= BenchmarkThreshold(BenchmarkTargets.First()))
                {
                    int targetValue = BenchmarkTargets.First();
                    BestValidIndividual.Genotype.SaveGenotypeData(Instance, BestValidIndividual.Fitness, $"_{targetValue}");
                    WriteLine($"Hit benchmark target {targetValue}, saving data.");
                    BenchmarkTargets.Remove(targetValue);
                    if (BenchmarkTargets.Count == 0)
                    {
                        WriteLine($"\nTerminating early at round {i} due to being within 5% of benchmark");
                        break;
                    }
                    nextThreshold = BenchmarkThreshold(BenchmarkTargets.First());
                }
            }
            watch.Stop();
        }

        /// <summary>
        /// Reassigns the best individuals when needed.
        /// </summary>
        private void GetBestIndividual(int i)
        {
            Individual bestContender = Population.OrderBy(x => x.Fitness).First();
            BestIndividualLastRound.SetGenotype(bestContender.Genotype.Clone(), FitnessFunction, PenaltyFunction);

            if (bestContender.Fitness < BestIndividual.Fitness)
            {
                BestIndividual.SetGenotype(bestContender.Genotype.Clone(), FitnessFunction, PenaltyFunction);
                RoundBestIndividualAquired = i;
            }

            var validIndividuals = Population.Where(x => x.IsValid);
            if (validIndividuals.Any())
            {
                Individual validContender = validIndividuals.OrderBy(x => x.Fitness).First();
                if (validContender.Fitness < BestValidIndividual.Fitness)
                {
                    BestValidIndividual.SetGenotype(validContender.Genotype.Clone(), FitnessFunction, PenaltyFunction);
                    RoundBestIndividualAquired = i;
                }
            }
        }

        /// <summary>
        /// Generates the initial population for the GA, defers the actual initialization to the Individual class.
        /// </summary>
        private List<Individual> GenerateInitialPopulation(int populationSize)
        {
            //Ensure even population size (For later crossover)
            int popSize = populationSize % 2 == 0 ? populationSize : populationSize + 1;

            var population = new List<Individual>();
            for (int i = 0; i < popSize; i++)
            {
                population.Add(new Individual(Instance, FitnessFunction, PenaltyFunction));
            }

            return population;
        }
    }
}
