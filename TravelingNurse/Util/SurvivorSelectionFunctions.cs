using TravelingNurse.Extensions;
using TravelingNurse.Models;

namespace TravelingNurse.Util
{
    public delegate List<Individual> SurvivorSelectionType(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize);
    public static class SurvivorSelectionFunctions
    {
        /// <summary>
        /// Greedily select fittest offspring.
        /// </summary>
        public static List<Individual> SimpleGreedySurvivorFunc(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            return offspring
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .OrderBy(x => x.Fitness)
                .Take(populationSize).ToList();
        }

        /// <summary>
        /// Greedily select fittest offspring, removes duplicates before selection.
        /// </summary>
        public static List<Individual> DistinctGreedySurvivorFunc(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            return offspring
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .DistinctBy(x => x.Genotype.Flatten())
                .OrderBy(x => x.Fitness)
                .Take(populationSize).ToList();
        }

        /// <summary>
        /// Greedily select fittest offspring, removes duplicates before selection, also ensures a steady mix between valid and invalid individuals.
        /// </summary>
        public static List<Individual> SafeDistinctGreedySurvivorFunc(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            int validCount = populationSize * 6 / 10;
            var output = offspring.SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .Where(x => x.IsValid)
                .DistinctBy(x => x.Genotype.Flatten())
                .OrderBy(x => x.Fitness)
                .Take(validCount).ToList();

            var rest = offspring
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .DistinctBy(x => x.Genotype.Flatten())
                .OrderBy(x => x.Fitness)
                .Except(output)
                .Take(populationSize - validCount);

            output.AddRange(rest);
            if (output.Count != populationSize)
            {
                var missingCount = populationSize - output.Count;
                output.AddRange(offspring
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .DistinctBy(x => x.Genotype.Flatten())
                .OrderBy(x => x.Fitness)
                .Except(output)
                .Take(missingCount));
            }

            return output;
        }

        /// <summary>
        /// Greedily select fittest parents and offspring, removes duplicates before selection.
        /// </summary>
        public static List<Individual> ElitistDistinctSurvivorFunc(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            int elitistCount = Convert.ToInt32(populationSize / 50);

            List<Individual> elites = parents
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .DistinctBy(x => x.Genotype.Flatten())
                .OrderBy(x => x.Fitness).Take(elitistCount).ToList();

            List<Individual> survivors = offspring
                .SelectMany(x => new List<Individual>() { x.Item1, x.Item2 }).ToList()
                .OrderBy(x => x.Fitness)
                .DistinctBy(x => x.Genotype.Flatten())
                .Take(populationSize - elitistCount).ToList();

            survivors.AddRange(elites);
            return survivors;
        }

        /// <summary>
        /// Crowding that supports repeated crossovers by sorting by distinct fitness.
        /// </summary>
        public static List<Individual> GreedyCrowding(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            return Crowding(parents, offspring).OrderBy(x => x.Fitness).DistinctBy(x => x.Genotype.Flatten()).Take(populationSize).ToList();
        }

        /// <summary>
        /// Simple crowding for non-repeated crossover.
        /// </summary>
        public static List<Individual> SimpleCrowding(List<IndividualTuple> parents, List<IndividualTuple> offspring, int populationSize)
        {
            if (parents.Count != offspring.Count)
            {
                throw new Exception("Simple crowding does not support used crossover function.");
            }

            return Crowding(parents, offspring).ToList();
        }

        /// <summary>
        /// Crowding algorithm.
        /// </summary>
        public static IEnumerable<Individual> Crowding(List<IndividualTuple> parents, List<IndividualTuple> offspring)
        {
            List<Individual> survivors = new();
            var sync = new object();

            Parallel.For(0, parents.Count, i =>
            {
                var (p1, p2) = parents[i];
                var (c1, c2) = offspring[i];

                if (HammingDistance(p1.Genotype, c1.Genotype) + HammingDistance(p2.Genotype, c2.Genotype) < HammingDistance(p1.Genotype, c2.Genotype) + HammingDistance(p2.Genotype, c1.Genotype))
                {
                    if (p1.Fitness < c1.Fitness)
                    {
                        lock (sync)
                        {
                            survivors.Add(p1);
                        }
                    }
                    else
                    {
                        lock (sync)
                        {
                            survivors.Add(c1);
                        }
                    }

                    if (p2.Fitness < c2.Fitness)
                    {
                        lock (sync)
                        {
                            survivors.Add(p2);
                        }
                    }
                    else
                    {
                        lock (sync)
                        {
                            survivors.Add(c2);
                        }
                    }
                }
                else
                {
                    if (p1.Fitness < c2.Fitness)
                    {
                        lock (sync)
                        {
                            survivors.Add(p1);
                        }
                    }
                    else
                    {
                        lock (sync)
                        {
                            survivors.Add(c2);
                        }
                    }

                    if (p2.Fitness < c1.Fitness)
                    {
                        lock (sync)
                        {
                            survivors.Add(p2);
                        }
                    }
                    else
                    {
                        lock (sync)
                        {
                            survivors.Add(c1);
                        }
                    }
                }
            });
            return survivors;
        }

        /// <summary>
        /// Calculates hamming distance between two genotypes.
        /// </summary>
        private static int HammingDistance(Genotype g1, Genotype g2)
        {
            string g1_str = string.Join("", g1.Flatten());
            string g2_str = string.Join("", g2.Flatten());
            return HammingDistance(g1_str, g2_str);
        }

        /// <summary>
        /// Calculates hamming distance between two strings.
        /// </summary>
        private static int HammingDistance(string s, string t)
        {
            if (s.Length != t.Length)
            {
                throw new Exception("Strings must be equal length");
            }

            return s.Zip(t, (c, b) => c != b).Count(f => f);
        }
    }
}
