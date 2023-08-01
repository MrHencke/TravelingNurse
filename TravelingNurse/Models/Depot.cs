using System.Text.Json.Serialization;

namespace TravelingNurse.Models
{
    public class Depot
    {
        [JsonPropertyName("return_time")]
        public int ReturnTime { get; init; }
        [JsonPropertyName("x_coord")]
        public int XCoord { get; init; }
        [JsonPropertyName("y_coord")]
        public int YCoord { get; init; }
    }
}
