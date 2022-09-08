

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Sharing
{
    [Serializable]
    public class ScheduleDescription
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        public string test { get; set; } = "heyooo";

        public override string ToString()
        {
            return Name + " " + Value;
        }
    }
}
