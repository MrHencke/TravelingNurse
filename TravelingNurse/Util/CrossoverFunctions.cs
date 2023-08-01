using TravelingNurse.Extensions;
using TravelingNurse.Models;
namespace TravelingNurse.Util
{
    public delegate (List<IndividualTuple>, List<IndividualTuple>) CrossoverFunctionType(List<IndividualTuple> input);
    public delegate (IEnumerable<IndividualTuple>, IEnumerable<IndividualTuple>) InternalCrossoverFunctionType(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, List<IndividualTuple> input);

    public static class CrossoverFunctions
    {
        /// <summary>
        /// Combines genotype of two parents by slicing between routes at a given index.
        /// </summary>
        public static (IEnumerable<IndividualTuple>, IEnumerable<IndividualTuple>) SimpleCrossover(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, List<IndividualTuple> input)
        {
            IndividualTuple[] offspring = new IndividualTuple[input.Count];
            IndividualTuple[] parents = new IndividualTuple[input.Count];

            Random random = new();
            var sync = new object();

            Parallel.For(0, input.Count, x =>
            {
                Individual p1 = input[x].Item1;
                Individual p2 = input[x].Item2;

                if (random.NextDouble() < crossoverRate)
                {
                    var lengthGenotype = p1.Genotype.Count;

                    // Get genotype slices
                    var slicePoint = random.Next(lengthGenotype);
                    var (p1g1, p1g2) = p1.GetGenotypeSlice(slicePoint);
                    var (p2g1, p2g2) = p2.GetGenotypeSlice(slicePoint);

                    // Ensure values in latter slice does not exist in first.
                    p1g1 = p1g1.Select(x => x.Where(y => p2g2.SelectMany(x => x).All(z => z != y)).ToList()).ToList();
                    p2g1 = p2g1.Select(x => x.Where(y => p1g2.SelectMany(x => x).All(z => z != y)).ToList()).ToList();

                    // Concat, thus creating new genotype
                    Genotype c1g = p1g1.Concat(p2g2).ToList();
                    Genotype c2g = p2g1.Concat(p1g2).ToList();


                    // Check and fill missing
                    c1g = CheckAndFillMissingPatientsGreedy(c1g, fitnessFunction, penaltyFunction);
                    c2g = CheckAndFillMissingPatientsGreedy(c2g, fitnessFunction, penaltyFunction);


                    if (c1g.SelectMany(x => x).Count() != 100)
                        throw new Exception("Something went horribly wrong.");

                    // Mutate
                    c1g = mutationFunction(c1g.Clone(), mutationRate);
                    c2g = mutationFunction(c2g.Clone(), mutationRate);

                    Individual c1 = new(fitnessFunction, penaltyFunction, c1g.Clone());
                    Individual c2 = new(fitnessFunction, penaltyFunction, c2g.Clone());

                    lock (sync)
                    {
                        offspring[x] = new IndividualTuple(c1, c2);
                        parents[x] = input[x];
                    }
                }
                else
                {
                    lock (sync)
                    {
                        Genotype c1g = mutationFunction(p1.Genotype.Clone(), mutationRate);
                        Genotype c2g = mutationFunction(p2.Genotype.Clone(), mutationRate);
                        Individual c1 = new(fitnessFunction, penaltyFunction, c1g);
                        Individual c2 = new(fitnessFunction, penaltyFunction, c2g);
                        offspring[x] = new IndividualTuple(c1, c2);
                        parents[x] = input[x];
                    }
                }
            });
            return (parents, offspring);
        }

        /// <summary>
        /// Combines genotype of two parents by slicing inside a route at a given index.
        /// </summary>
        public static (IEnumerable<IndividualTuple>, IEnumerable<IndividualTuple>) DeepCrossover(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, List<IndividualTuple> input)
        {
            IndividualTuple[] offspring = new IndividualTuple[input.Count];
            IndividualTuple[] parents = new IndividualTuple[input.Count];

            Random random = new();
            var sync = new object();

            Parallel.For(0, input.Count, x =>
            {

                Individual p1 = input[x].Item1;
                Individual p2 = input[x].Item2;

                if (random.NextDouble() < crossoverRate)
                {
                    Genotype g1 = input[x].Item1.Genotype;
                    Genotype g2 = input[x].Item2.Genotype;

                    var genotypeLength = g1.Count;

                    Random rand = new();
                    int slicePoint = rand.Next(genotypeLength);

                    Genotype p1g1 = new(), p2g1 = new(), p1g2 = new(), p2g2 = new();

                    // Slice the and join respective slices of genotypes 1 and 2.
                    for (int i = 0; i < genotypeLength; i++)
                    {
                        if (i < slicePoint)
                        {
                            p1g1.Add(g1[i]);
                            p2g1.Add(g2[i]);
                        }
                        else if (i == slicePoint)
                        {
                            var slicePointSublist1 = rand.Next(g1[i].Count);
                            var slicePointSublist2 = rand.Next(g2[i].Count);

                            Route r1s1 = g1[i].Take(slicePointSublist1).ToList();
                            Route r1s2 = g1[i].Skip(slicePointSublist1).ToList();
                            p1g1.Add(r1s1);

                            Route r2s1 = g2[i].Take(slicePointSublist2).ToList();
                            Route r2s2 = g2[i].Skip(slicePointSublist2).ToList();
                            p2g1.Add(r2s1);

                            p2g1 = p2g1.Select(x => x.Where(y => r1s2.All(z => z != y)).ToList()).ToList();
                            p1g1 = p1g1.Select(x => x.Where(y => r2s2.All(z => z != y)).ToList()).ToList();

                            p1g1[^1] = p1g1[^1].Concat(r2s2).ToList();
                            p2g1[^1] = p2g1[^1].Concat(r1s2).ToList();
                        }
                        else
                        {
                            p1g2.Add(g1[i]);
                            p2g2.Add(g2[i]);
                        }
                    }

                    // Ensure values in latter slice does not exist in first.
                    p1g1 = p1g1.Select(x => x.Where(y => p2g2.SelectMany(x => x).All(z => z != y)).ToList()).ToList();
                    p2g1 = p2g1.Select(x => x.Where(y => p1g2.SelectMany(x => x).All(z => z != y)).ToList()).ToList();

                    // Concat, thus creating new genotype
                    Genotype c1g = p1g1.Concat(p2g2).ToList();
                    Genotype c2g = p2g1.Concat(p1g2).ToList();

                    // Check and fill missing
                    c1g = CheckAndFillMissingPatientsGreedy(c1g, fitnessFunction, penaltyFunction);
                    c2g = CheckAndFillMissingPatientsGreedy(c2g, fitnessFunction, penaltyFunction);

                    if (c1g.SelectMany(x => x).Count() != 100)
                        throw new Exception("Something went horribly wrong.");

                    // Mutate
                    c1g = mutationFunction(c1g.Clone(), mutationRate);
                    c2g = mutationFunction(c2g.Clone(), mutationRate);

                    Individual c1 = new(fitnessFunction, penaltyFunction, c1g.Clone());
                    Individual c2 = new(fitnessFunction, penaltyFunction, c2g.Clone());

                    lock (sync)
                    {
                        offspring[x] = new IndividualTuple(c1, c2);
                        parents[x] = input[x];
                    }
                }
                else
                {
                    lock (sync)
                    {
                        Genotype c1g = mutationFunction(p1.Genotype.Clone(), mutationRate);
                        Genotype c2g = mutationFunction(p2.Genotype.Clone(), mutationRate);
                        Individual c1 = new(fitnessFunction, penaltyFunction, c1g);
                        Individual c2 = new(fitnessFunction, penaltyFunction, c2g);
                        offspring[x] = new IndividualTuple(c1, c2);
                        parents[x] = input[x];
                    }
                }
            });
            return (parents, offspring);
        }

        /// <summary>
        /// Combines genotype of two parents by exchanging a single route between them.
        /// </summary>
        public static (IEnumerable<IndividualTuple>, IEnumerable<IndividualTuple>) SingleRouteSwap(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, List<IndividualTuple> input)
        {
            IndividualTuple[] offspring = new IndividualTuple[input.Count];
            IndividualTuple[] parents = new IndividualTuple[input.Count];

            Random random = new();
            var sync = new object();

            Parallel.For(0, input.Count, i =>
            {
                Individual p1 = input[i].Item1;
                Individual p2 = input[i].Item2;

                if (random.NextDouble() < crossoverRate)
                {

                    Genotype c1g = p1.Genotype.Clone();
                    Genotype c2g = p2.Genotype.Clone();

                    var routeIndex = random.Next(0, c1g.Count);
                    Route p1Route = c1g[routeIndex];
                    Route p2Route = c2g[routeIndex];

                    c1g.RemoveAt(routeIndex);
                    c2g.RemoveAt(routeIndex);

                    c1g = c1g.Select(x => x.Where(y => p2Route.All(z => z != y)).ToList()).ToList();
                    c2g = c2g.Select(x => x.Where(y => p1Route.All(z => z != y)).ToList()).ToList();

                    c1g.Insert(routeIndex, p2Route);
                    c2g.Insert(routeIndex, p1Route);

                    c1g = CheckAndFillMissingPatientsGreedy(c1g, fitnessFunction, penaltyFunction);
                    c2g = CheckAndFillMissingPatientsGreedy(c2g, fitnessFunction, penaltyFunction);

                    if (c1g.SelectMany(x => x).Count() != 100)
                        throw new Exception("Something went horribly wrong.");

                    // Mutate
                    c1g = mutationFunction(c1g.Clone(), mutationRate);
                    c2g = mutationFunction(c2g.Clone(), mutationRate);

                    Individual c1 = new(fitnessFunction, penaltyFunction, c1g.Clone());
                    Individual c2 = new(fitnessFunction, penaltyFunction, c2g.Clone());

                    lock (sync)
                    {
                        offspring[i] = new IndividualTuple(c1, c2);
                        parents[i] = input[i];
                    }
                }
                else
                {
                    lock (sync)
                    {
                        Genotype c1g = mutationFunction(p1.Genotype.Clone(), mutationRate);
                        Genotype c2g = mutationFunction(p2.Genotype.Clone(), mutationRate);
                        Individual c1 = new(fitnessFunction, penaltyFunction, c1g);
                        Individual c2 = new(fitnessFunction, penaltyFunction, c2g);
                        offspring[i] = new IndividualTuple(c1, c2);
                        parents[i] = input[i];
                    }
                }
            });
            return (parents, offspring);
        }

        /// <summary>
        /// Combines genotype of two parents by exchanging a single route between them and removing all elements contained within them, greedily reassigns them to routes.
        /// </summary>
        public static (IEnumerable<IndividualTuple>, IEnumerable<IndividualTuple>) SingleRoutePatientsSwap(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, List<IndividualTuple> input)
        {
            IndividualTuple[] offspring = new IndividualTuple[input.Count];
            IndividualTuple[] parents = new IndividualTuple[input.Count];

            Random random = new();
            var sync = new object();

            Parallel.For(0, input.Count, i =>
            {
                Individual p1 = input[i].Item1;
                Individual p2 = input[i].Item2;

                if (random.NextDouble() < crossoverRate)
                {
                    Genotype c1g = p1.Genotype.Clone();
                    Genotype c2g = p2.Genotype.Clone();

                    var routeIndex = random.Next(0, c1g.Count);
                    Route p1Route = c1g[routeIndex];
                    Route p2Route = c2g[routeIndex];

                    c1g = c1g.Select(x => x.Where(y => p2Route.All(z => z != y)).ToList()).ToList();
                    c2g = c2g.Select(x => x.Where(y => p1Route.All(z => z != y)).ToList()).ToList();

                    c1g = CheckAndFillMissingPatientsGreedy(c1g, fitnessFunction, penaltyFunction);
                    c2g = CheckAndFillMissingPatientsGreedy(c2g, fitnessFunction, penaltyFunction);

                    if (c1g.SelectMany(x => x).Count() != 100)
                        throw new Exception("Something went horribly wrong.");

                    // Mutate
                    c1g = mutationFunction(c1g.Clone(), mutationRate);
                    c2g = mutationFunction(c2g.Clone(), mutationRate);

                    Individual c1 = new(fitnessFunction, penaltyFunction, c1g.Clone());
                    Individual c2 = new(fitnessFunction, penaltyFunction, c2g.Clone());

                    lock (sync)
                    {
                        offspring[i] = new IndividualTuple(c1, c2);
                        parents[i] = input[i];
                    }
                }
                else
                {
                    lock (sync)
                    {
                        Genotype c1g = mutationFunction(p1.Genotype.Clone(), mutationRate);
                        Genotype c2g = mutationFunction(p2.Genotype.Clone(), mutationRate);
                        Individual c1 = new(fitnessFunction, penaltyFunction, c1g);
                        Individual c2 = new(fitnessFunction, penaltyFunction, c2g);
                        offspring[i] = new IndividualTuple(c1, c2);
                        parents[i] = input[i];
                    }
                }
            });
            return (parents, offspring);
        }

        /// <summary>
        /// Repeats the crossover functions n times. 
        /// </summary>
        public static (List<IndividualTuple>, List<IndividualTuple>) RepeatCrossover(InternalCrossoverFunctionType crossoverFunction, FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, double crossoverRate, MutationFunctionType mutationFunction, double mutationRate, int repetitions, List<IndividualTuple> input)
        {
            List<IndividualTuple> parents = new();
            List<IndividualTuple> offspring = new();

            var sync = new object();

            Parallel.For(0, repetitions, i =>
            {
                lock (sync)
                {
                    List<IndividualTuple> inputClone = new(input);
                    var (new_parents, new_offspring) = crossoverFunction(fitnessFunction, penaltyFunction, crossoverRate, mutationFunction, mutationRate, inputClone);
                    parents.AddRange(new_parents);
                    offspring.AddRange(new_offspring);
                }
            });

            return (parents, offspring);
        }

        /// <summary>
        /// Greedily fills missing patients at fitting positions, based on fitness.
        /// </summary>
        private static Genotype CheckAndFillMissingPatientsGreedy(Genotype genotype, FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, int patientCount = 100)
        {
            Genotype output = genotype.Clone();
            IEnumerable<int> flattenedGenotype = output.SelectMany(x => x);
            if (flattenedGenotype.Count() == patientCount) return output;

            IEnumerable<int> missingPatients = Enumerable.Range(1, 100).Except(flattenedGenotype);
            foreach (int patient in missingPatients)
            {
                List<Genotype> candidateGenotypes = new();

                for (int routeIndex = 0; routeIndex < output.Count; routeIndex++)
                {
                    for (int patientIndex = 0; patientIndex < output[routeIndex].Count; patientIndex++)
                    {
                        Genotype local = output.Clone();
                        local[routeIndex].Insert(patientIndex, patient);
                        candidateGenotypes.Add(local);
                    }
                    Genotype last = output.Clone();
                    last[routeIndex].Insert(output[routeIndex].Count, patient);
                    candidateGenotypes.Add(last);
                }
                output = candidateGenotypes.OrderBy(x => fitnessFunction(x) + penaltyFunction(x)).First();
            }
            return output;
        }

        /// <summary>
        /// Stochastically fills missing patients at fitting positions, based on fitness.
        /// </summary>
        private static Genotype CheckAndFillMissingPatientsStochastic(Genotype genotype, FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, int patientCount = 100)
        {
            Genotype output = genotype.Clone();
            Route flattenedGenotype = output.SelectMany(x => x).ToList();
            if (flattenedGenotype.Count == patientCount) return output;

            IEnumerable<int> missingPatients = Enumerable.Range(1, 100).Except(flattenedGenotype);
            var routeCount = genotype.Count;
            Random random = new();

            foreach (int patient in missingPatients)
            {
                var route = random.Next(routeCount);
                output[route].Insert(random.Next(output[route].Count + 1), patient);
            }
            return output;
        }
    }
}
