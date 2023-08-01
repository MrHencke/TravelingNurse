using FF = TravelingNurse.Util.FitnessFunctions;
using SF = TravelingNurse.Util.ParentSelectionFunctions;
using CF = TravelingNurse.Util.CrossoverFunctions;
using SSF = TravelingNurse.Util.SurvivorSelectionFunctions;
using MF = TravelingNurse.Util.MutationFunctions;
using IL = TravelingNurse.Util.InstanceLoader;
using GA = TravelingNurse.Models.GeneticAlgorithm;

using static System.Console;
using TravelingNurse.Models;
using TravelingNurse.Extensions;
using TravelingNurse.Util;

namespace TravelingNurse
{
    public class Program
    {
        private static async Task Main()
        {
            await GA();
        }

        /// <summary>
        /// Gets an instance, and starts running the GA on the instance with the supplied parameters.
        /// </summary>
        private static async Task GA(bool writeToFile = false)
        {
            Clear();
            WriteLine("Type the instance number to run on:");
            string? num = ReadLine();
            num ??= "0";
            int parsedNum = int.Parse(num);
            Instance instance = await IL.GetTrainInstanceByNumberAsync(parsedNum);
            int populationSize = 500;
            double crossoverRate = 0.9;
            double mutationRate = 0.01;
            int repetitions = 5;
            MutationFunctionType mutationFunction = MF.InsertionMutationElementWise;
            int curriedFitnessFunction(Genotype x) => FF.FitnessFunc(instance.TravelTimes, x);
            int curriedPenaltyFunction(Genotype x) => FF.PenaltyFunction(instance.Patients, instance.TravelTimes, instance.Depot.ReturnTime, instance.CapacityNurse, x);
            (List<IndividualTuple>, List<IndividualTuple>) curriedCrossoverFunction(List<IndividualTuple> x) => CF.RepeatCrossover(CF.SingleRouteSwap, curriedFitnessFunction, curriedPenaltyFunction, crossoverRate, mutationFunction, mutationRate, repetitions, x);

            GA geneticAlgorithm = new
            (
            instance,
            curriedFitnessFunction,
            curriedPenaltyFunction,
            SF.RouletteSelection,
            curriedCrossoverFunction,
            SSF.SafeDistinctGreedySurvivorFunc,
            mutationFunction,
            populationSize,
            mutationRate,
            crossoverRate
            )
            {
                Debug = true
            };

            geneticAlgorithm.Run();

            Individual bestIndividual = geneticAlgorithm.BestValidIndividual;
            WriteScore(bestIndividual);

            if (writeToFile)
                await WriteToFile(bestIndividual, instance);
        }


        private static void WriteScore(Individual bestIndividual)
        {
            WriteLine();
            WriteLine($"Fitness: {bestIndividual.Fitness}");
            WriteLine($"Is valid: {bestIndividual.IsValid}");
            WriteLine($"Non-penalized Fitness: {bestIndividual.PureFitness}");
            WriteLine($"Penalty: {bestIndividual.Penalty}");
        }
        private static async Task WriteToFile(Individual bestIndividual, Instance instance)
        {
            await bestIndividual.Genotype.WriteToFileAsync(instance.InstanceName);
            bestIndividual.Genotype.Plot(instance.InstanceName, instance.Patients, instance.Depot);
            bestIndividual.Genotype.SaveAndPrintOutputSolution(instance, bestIndividual.Fitness);
        }
    }
}