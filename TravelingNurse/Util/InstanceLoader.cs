using System.Text.Json;
using TravelingNurse.Models;

namespace TravelingNurse.Util
{
    public static class InstanceLoader
    {

        /// <summary>
        /// Gets a train instance by number. Throws exception if number is not found.
        /// </summary>
        public static async Task<Instance> GetTrainInstanceByNumberAsync(int num)
        {
            var fileName = $"TravelingNurse/Instances/train_{num}.json";
            using FileStream stream = File.OpenRead(fileName);
            Instance? instance = await JsonSerializer.DeserializeAsync<Instance>(stream) ?? throw new InvalidDataException();
            return instance;
        }

        /// <summary>
        /// Gets a train instance by name. Throws exception if name is not found.
        /// </summary>
        public static async Task<Instance> GetInstanceByNameAsync(string name)
        {
            var fileName = $"TravelingNurse/Instances/{name}.json";
            using FileStream stream = File.OpenRead(fileName);
            Instance? instance = await JsonSerializer.DeserializeAsync<Instance>(stream) ?? throw new InvalidDataException();
            return instance;
        }
    }
}
