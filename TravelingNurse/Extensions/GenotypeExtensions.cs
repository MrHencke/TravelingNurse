using System.Text.Json;
using TravelingNurse.Models;
using FF = TravelingNurse.Util.FitnessFunctions;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Models;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using System.Drawing;
using System.Numerics;

namespace TravelingNurse.Extensions
{
    public static class GenotypeExtensions
    {
        /// <summary>
        /// Makes a shallow copy of a genotype
        /// </summary>
        public static Genotype Clone(this Genotype input)
        {
            Genotype output = new();
            input.ForEach(x => output.Add(new Route(x)));
            return output;
        }

        /// <summary>
        /// Flattens a genotype from List<List<int>> to List<int>"
        /// </summary>
        public static Route Flatten(this Genotype input)
        {
            return input.SelectMany(x => x).ToList();
        }

        /// <summary>
        /// Asynchronously writes a genotype to file.
        /// </summary>
        public static async Task WriteToFileAsync(this Genotype input, string name)
        {
            string input_str = JsonSerializer.Serialize(input);
            await File.WriteAllTextAsync($"{name}_Solution.json", input_str);
        }

        /// <summary>
        /// Synchronously writes a genotype to file.
        /// </summary>
        public static void WriteToFile(this Genotype input, string name)
        {
            string input_str = JsonSerializer.Serialize(input);
            File.WriteAllText($"{name}_Solution.json", input_str);
        }

        /// <summary>
        /// Plots a genotype and saves it in project folder.
        /// </summary>
        /// <param name="genotype"></param>
        /// <param name="patients"></param>
        /// <param name="depot"></param>
        public static void Plot(this Genotype genotype, string instanceName, Dictionary<int, Patient> patients, Depot depot)
        {
            var plt = new ScottPlot.Plot(1920, 1080);
            for (int i = 0; i < genotype.Count; i++)
            {
                List<double> dataX = new() { depot.XCoord };
                List<double> dataY = new() { depot.YCoord };

                foreach (int patient in genotype[i])
                {
                    dataX.Add(patients[patient].XCoord);
                    dataY.Add(patients[patient].YCoord);
                }

                dataX.Add(depot.XCoord);
                dataY.Add(depot.YCoord);
                ScottPlot.MarkerShape markerShape = ScottPlot.MarkerShape.filledSquare;
                plt.AddScatter(dataX.ToArray(), dataY.ToArray(), GetColor(i), 3, 10, markerShape);
            }
            plt.SaveFig($"{instanceName}_Routes.png");
        }

        private static Color GetColor(int i) => ColorTranslator.FromHtml(Colours[i]);

        private readonly static string[] Colours = new string[] {
        "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#000000",
        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080", "#808080",
        "#C00000", "#00C000", "#0000C0", "#C0C000", "#C000C0", "#00C0C0", "#C0C0C0",
        "#400000", "#004000", "#000040", "#404000", "#400040", "#004040", "#404040",
        "#200000", "#002000", "#000020", "#202000", "#200020", "#002020", "#202020",
        "#600000", "#006000", "#000060", "#606000", "#600060", "#006060", "#606060",
        "#A00000", "#00A000", "#0000A0", "#A0A000", "#A000A0", "#00A0A0", "#A0A0A0",
        "#E00000", "#00E000", "#0000E0", "#E0E000", "#E000E0", "#00E0E0", "#E0E0E0",
        };

        /// <summary>
        /// Prints and saves solution output.
        /// </summary>
        public static string GetOutputSolution(this Genotype genotype, Instance instance, int fitness)
        {
            string preMeta = $"Nurse capacity: {instance.CapacityNurse}\nDepot return time: {instance.Depot.ReturnTime}";
            List<SolutionOutput> rows = new();
            var (coveredDemands, routeDurations, patientMetas) = FF.PerRouteMeta(instance.Patients, instance.TravelTimes, genotype);
            for (int i = 0; i < genotype.Count; i++)
            {
                double routeDuration = Math.Round(routeDurations[i], 2);

                List<string> sequences = genotype[i].Select(patient =>
                    patientMetas[patient - 1].ToString()
                ).ToList();
                if (sequences.Count != 0)
                {
                    sequences.Insert(0, "D (0)");
                    sequences.Add($"D ({routeDuration})");
                }

                rows.Add(new SolutionOutput()
                {
                    Name = $"Nurse {i + 1} (N{i + 1})",
                    RouteDuration = routeDuration,
                    CoveredDemand = coveredDemands[i],
                    _patientSequence = sequences
                });
            }

            CellFormat headerFormat = new()
            {
                Alignment = Alignment.Center,
            };

            Table table = new TableBuilder(headerFormat)
                .AddColumn("Name")
                    .RowsFormat()
                .AddColumn("Route Duration")
                    .RowsFormat()
                .AddColumn("Covered Demand")
                    .RowsFormat()
                .AddColumn("Patient Sequence")
                    .RowsFormat()
                .Build();
            table.Config = TableConfig.Unicode();

            foreach (var row in rows)
            {
                table.AddRow(row.Name, row.RouteDuration, row.CoveredDemand, row.PatientSequence);
            }

            string tableString = table.ToString();
            string postMeta = $"Objective value: {fitness}";
            return "\n" + preMeta + "\n" + tableString + "\n" + postMeta;
        }

        public static void SaveOutputSolution(this Genotype genotype, string name, Instance instance, int fitness)
        {
            string solutionTable = genotype.GetOutputSolution(instance, fitness);
            File.WriteAllText($"{name}_SolutionOutput.txt", solutionTable);
        }
        public static void PrintOutputSolution(this Genotype genotype, Instance instance, int fitness)
        {
            Console.WriteLine(genotype.GetOutputSolution(instance, fitness));
        }

        public static void SaveAndPrintOutputSolution(this Genotype genotype, Instance instance, int fitness)
        {
            string solutionTable = genotype.GetOutputSolution(instance, fitness);
            File.WriteAllText($"{instance.InstanceName}_SolutionOutput.txt", solutionTable);
            Console.WriteLine(solutionTable);
        }

        public static void SaveGenotypeData(this Genotype genotype, Instance instance, int fitness, string extraName)
        {
            genotype.WriteToFile(instance.InstanceName + extraName);
            genotype.Plot(instance.InstanceName + extraName, instance.Patients, instance.Depot);
            genotype.SaveOutputSolution(instance.InstanceName + extraName, instance, fitness);
        }
    }
}
