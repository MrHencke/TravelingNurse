using System.Text.Json.Serialization;

namespace TravelingNurse.Models
{
    public class Instance
    {
        [JsonPropertyName("instance_name")]
        public string InstanceName { get; init; }
        [JsonPropertyName("nbr_nurses")]
        public int NumNurses { get; init; }
        [JsonPropertyName("capacity_nurse")]
        public int CapacityNurse { get; init; }
        [JsonPropertyName("benchmark")]
        public double Benchmark { get; init; }
        [JsonPropertyName("depot")]
        public Depot Depot { get; init; }
        [JsonPropertyName("patients")]
        public Dictionary<int, Patient> Patients { get; init; }
        [JsonPropertyName("travel_times")]
        public List<List<double>> TravelTimes { get; init; }
    }
}