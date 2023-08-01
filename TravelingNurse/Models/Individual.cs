using TravelingNurse.Extensions;
using TravelingNurse.Util;

namespace TravelingNurse.Models
{
    public class Individual
    {
        public Genotype Genotype { get; private set; }
        public int PureFitness { get; private set; }
        public int Penalty { get; private set; }
        public int Fitness { get => PureFitness + Penalty; }
        public bool IsValid { get => Penalty == 0; }

        /// <summary>
        /// Empty constructor, only used for placeholder individuals.
        /// </summary>
        public Individual()
        {
            Genotype = new Genotype();
            PureFitness = 0;
            Penalty = 100000;
        }

        /// <summary>
        /// Full constructor for a new individual
        /// </summary>
        public Individual(Instance instance, FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction)
        {
            Genotype genotype = GenerateInitialGenotype(instance);
            PureFitness = fitnessFunction(genotype);
            Penalty = penaltyFunction(genotype);
            Genotype = genotype;
        }

        /// <summary>
        /// Full constructor when genotype is provided.
        /// </summary>
        public Individual(FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction, Genotype genotype)
        {
            Genotype = genotype;
            PureFitness = fitnessFunction(genotype);
            Penalty = penaltyFunction(genotype);
        }

        /// <summary>
        /// Stochastically generates and returns a new genotype.
        /// </summary>
        private static Genotype GenerateInitialGenotype(Instance instance)
        {
            List<int> sortedPatients = instance.Patients.OrderBy(x => x.Value.EndTime).Select(x => x.Key).ToList();
            Genotype genotype = new();
            Random random = new();

            for (int i = 0; i < instance.NumNurses; i++)
            {
                genotype.Add(new Route());
            }

            for (int i = 0; i < instance.Patients.Count; i++)
            {
                int patientIndex = sortedPatients[i];
                int nurse = random.Next(instance.NumNurses);
                genotype[nurse].Add(patientIndex);
            }
            return genotype;
        }

        /// <summary>
        /// Creates genotype slices and returns them, used for simple crossover.
        /// </summary>
        public (EnumerableGenotype, EnumerableGenotype) GetGenotypeSlice(int slicePoint)
        {
            EnumerableGenotype g1 = Genotype.Take(slicePoint);
            EnumerableGenotype g2 = Genotype.Skip(slicePoint);

            return (g1, g2);
        }

        /// <summary>
        /// Sets genotype and regenerates fitness and penalty.
        /// </summary>
        public void SetGenotype(Genotype genotype, FitnessFunctionType fitnessFunction, FitnessFunctionType penaltyFunction)
        {
            Genotype = genotype;
            PureFitness = fitnessFunction(genotype);
            Penalty = penaltyFunction(genotype);
        }
    }
}
