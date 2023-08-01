using TravelingNurse.Models;
using Weighted_Randomizer;

namespace TravelingNurse.Util
{
    public delegate List<IndividualTuple> ParentSelectionType(List<Individual> input);

    public static class ParentSelectionFunctions
    {
        /// <summary>
        /// Sorts parents by fitness and pairs the fittest.
        /// </summary>
        public static List<IndividualTuple> ElitistParentSelection(List<Individual> input)
        {
            var orderedInput = input.OrderBy(x => x.Fitness);
            var even = orderedInput.Where((elem, ind) => ind % 2 == 0);
            var odd = orderedInput.Where((elem, ind) => ind % 2 == 1);
            return even.Zip(odd, (a, b) => new IndividualTuple(a, b)).ToList();
        }

        /// <summary>
        /// Randomly select parents based on a probability from fitness.
        /// </summary>
        public static List<IndividualTuple> RouletteSelection(List<Individual> input)
        {
            int maxFitness = input.OrderByDescending(x => x.Fitness).First().Fitness;

            List<IndividualTuple> parents = new();
            DynamicWeightedRandomizer<int> randomizer = new();
            for (int i = 0; i < input.Count; i++)
            {
                int transformedFitness = maxFitness - input[i].Fitness + 1;
                randomizer.Add(i, transformedFitness);
            }
            ;

            while (randomizer.Count > 0)
            {
                Individual p1 = input[randomizer.NextWithRemoval()];
                Individual p2 = input[randomizer.NextWithRemoval()];
                parents.Add(new IndividualTuple(p1, p2));
            }
            return parents;
        }
    }
}

