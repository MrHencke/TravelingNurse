
namespace TravelingNurse.Models
{
    public class SolutionOutput
    {
        public string Name { get; set; }
        public double RouteDuration { get; set; }
        public double CoveredDemand { get; set; }
        public List<string> _patientSequence { private get; init; }
        public string PatientSequence { get => string.Join(" -> ", _patientSequence); }
    }

    public class PatientMeta
    {
        public int Id { get; set; }
        public double VisitedStart { get; set; }
        public double VisitedEnd { get; set; }
        public int TimeWindowStart { get; set; }
        public int TimeWindowEnd { get; set; }

        public override string ToString()
        {
            return $"P{Id} ({Math.Round(VisitedStart, 2)}-{Math.Round(VisitedEnd, 2)}) [{TimeWindowStart}-{TimeWindowEnd}]";
        }
    }
}
