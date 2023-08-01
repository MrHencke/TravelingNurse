using System.Text.Json.Serialization;

namespace TravelingNurse.Models
{
    public class Patient
    {
        [JsonPropertyName("x_coord")]
        public int XCoord { get; init; }
        [JsonPropertyName("y_coord")]
        public int YCoord { get; init; }
        [JsonPropertyName("demand")]
        public int Demand { get; init; }
        [JsonPropertyName("start_time")]
        public int StartTime { get; init; }
        [JsonPropertyName("end_time")]
        public int EndTime { get; init; }
        [JsonPropertyName("care_time")]
        public int CareTime { get; init; }
    }
}
