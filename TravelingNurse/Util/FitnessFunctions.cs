using TravelingNurse.Models;

namespace TravelingNurse.Util
{
    public delegate int FitnessFunctionType(Genotype input);
    public static class FitnessFunctions
    {

        /// <summary>
        /// Calculates the fitness of a genotype
        /// </summary>
        public static int FitnessFunc(List<List<double>> travelTimes, Genotype input)
        {
            double totalTravelTime = 0;

            for (int i = 0; i < input.Count; i++)
            {

                int patientIndex = 0;

                for (int j = 0; j < input[i].Count; j++)
                {
                    if (j == 0)
                    {
                        patientIndex = input[i][0];
                        totalTravelTime += travelTimes[0][patientIndex];
                    }
                    else
                    {
                        patientIndex = input[i][j];
                        int lastPatient = input[i][j - 1];
                        totalTravelTime += travelTimes[lastPatient][patientIndex];
                    }
                }
                // Trip back to depot
                totalTravelTime += travelTimes[patientIndex][0];

            }
            return Convert.ToInt32(totalTravelTime);
        }

        /// <summary>
        /// Calculates the penalty of a genotype
        /// </summary>
        public static int PenaltyFunction(Dictionary<int, Patient> patients, List<List<double>> travelTimes, int returnTime, int capacityNurse, Genotype input)
        {
            if (input.Count == 0) return 10000;

            double scalingFactor = 26;

            var visited = new HashSet<int>();

            int revisitedCount = 0;
            int endTimeViolations = 0;
            int overCapacityViolations = 0;
            int afterDepotReturnTimeViolations = 0;

            for (int i = 0; i < input.Count; i++)
            {
                double routeTravelTimes = 0.0;
                double routeWaitTimes = 0.0;
                double routeCareTimes = 0.0;
                int routeDemand = 0;
                double TotalTime() => routeTravelTimes + routeWaitTimes + routeCareTimes;
                var hasEndTimeViolationYet = false;

                for (int j = 0; j < input[i].Count; j++)
                {
                    int patientIndex = input[i][j];

                    if (visited.Contains(patientIndex)) revisitedCount += 1;

                    visited.Add(patientIndex);

                    Patient patient = patients[patientIndex];

                    routeDemand += patient.Demand;

                    if (j == 0)
                    {
                        routeTravelTimes += travelTimes[0][patientIndex];
                    }
                    else
                    {
                        int lastPatientIndex = input[i][j - 1];
                        routeTravelTimes += travelTimes[lastPatientIndex][patientIndex];
                    }

                    if (TotalTime() < patient.StartTime)
                    {
                        routeWaitTimes += (patient.StartTime - TotalTime());
                    }

                    routeCareTimes += patient.CareTime;

                    if (TotalTime() > patient.EndTime && !hasEndTimeViolationYet)
                    {
                        // Resolve impl only considers a single end time violation, breaks inner loop and carries on to next route. I wont be replicating behaviour here.
                        endTimeViolations += 1;
                        hasEndTimeViolationYet = true;
                    }

                    if (j == input[i].Count - 1)
                    {
                        routeTravelTimes += travelTimes[patientIndex][0];
                    }

                }

                if (routeDemand > capacityNurse)
                {
                    overCapacityViolations += 25;
                }

                if (TotalTime() > returnTime)
                {
                    afterDepotReturnTimeViolations += 25;
                }
            }
            return Convert.ToInt32((revisitedCount + endTimeViolations + afterDepotReturnTimeViolations + overCapacityViolations) * scalingFactor);
        }

        public static (double[], double[], PatientMeta[]) PerRouteMeta(Dictionary<int, Patient> patients, List<List<double>> travelTimes, Genotype input)
        {
            double[] routeDurations = new double[input.Count];
            double[] coveredDemands = new double[input.Count];
            PatientMeta[] patientMetas = new PatientMeta[patients.Count];

            for (int i = 0; i < input.Count; i++)
            {
                double routeTravelTimes = 0.0;
                double routeWaitTimes = 0.0;
                double routeCareTimes = 0.0;
                int routeDemand = 0;
                double TotalTime() => routeTravelTimes + routeWaitTimes + routeCareTimes;

                for (int j = 0; j < input[i].Count; j++)
                {

                    int patientIndex = input[i][j];
                    PatientMeta patientMeta = new()
                    {
                        Id = patientIndex,
                        TimeWindowStart = patients[patientIndex].StartTime,
                        TimeWindowEnd = patients[patientIndex].EndTime,
                    };
                    Patient patient = patients[patientIndex];

                    routeDemand += patient.Demand;

                    if (j == 0)
                    {
                        routeTravelTimes += travelTimes[0][patientIndex];
                    }
                    else
                    {
                        int lastPatientIndex = input[i][j - 1];
                        routeTravelTimes += travelTimes[lastPatientIndex][patientIndex];
                    }

                    if (TotalTime() < patient.StartTime)
                    {
                        routeWaitTimes += (patient.StartTime - TotalTime());
                    }

                    patientMeta.VisitedStart = TotalTime();
                    routeCareTimes += patient.CareTime;
                    patientMeta.VisitedEnd = TotalTime();

                    if (j == input[i].Count - 1)
                    {
                        routeTravelTimes += travelTimes[patientIndex][0];
                    }

                    patientMetas[patientIndex - 1] = patientMeta;
                }

                coveredDemands[i] = routeDemand;
                routeDurations[i] = routeTravelTimes;

            }
            return (coveredDemands, routeDurations, patientMetas);
        }
    }
}
