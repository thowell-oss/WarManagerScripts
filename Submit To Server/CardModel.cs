using System;
using System.Collections.Generic;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Sharing
{
    [Serializable]
    public class CardModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = "body";

        [JsonPropertyName("descriptions")]
        public List<ScheduleDescription> Descriptions { get; set; } = new List<ScheduleDescription>();

        public override string ToString()
        {
            return Name + " " + Type + " " + String.Join(", ", Descriptions);
        }
    }
}
