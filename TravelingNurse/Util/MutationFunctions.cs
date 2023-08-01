namespace TravelingNurse.Util
{
    public delegate Genotype MutationFunctionType(Genotype input, double mutationRate);

    public static class MutationFunctions
    {

        /// <summary>
        /// Iterates all elements in genotype and moves to a random route.
        /// </summary>
        public static Genotype InsertionMutationElementWise(Genotype input, double mutationRate)
        {
            Random random = new();
            for (int fromRoute = 0; fromRoute < input.Count; fromRoute++)
            {
                for (int patient = 0; patient < input[fromRoute].Count; patient++)
                {
                    if (random.NextDouble() < mutationRate)
                    {
                        int toRoute = random.Next(input.Count);

                        int newPatientIndex = random.Next(input[toRoute].Count);
                        int tempValue = input[fromRoute][patient];
                        input[fromRoute].RemoveAt(patient);
                        input[toRoute].Insert(newPatientIndex, tempValue);
                    }
                }
            }
            return input;
        }

        /// <summary>
        /// Iterates all elements in genotype and swaps patients between routes.
        /// </summary>
        public static Genotype SwapMutationElementWise(Genotype input, double mutationRate)
        {
            Random random = new();
            for (int fromRoute = 0; fromRoute < input.Count; fromRoute++)
            {
                for (int patient = 0; patient < input[fromRoute].Count; patient++)
                {
                    if (random.NextDouble() < mutationRate)
                    {
                        int toRoute = random.Next(input.Count);
                        int newPatientIndex = random.Next(input[toRoute].Count);
                        (input[fromRoute][patient], input[toRoute][newPatientIndex]) = (input[toRoute][newPatientIndex], input[fromRoute][patient]);
                    }
                }
            }
            return input;
        }
    }
}
