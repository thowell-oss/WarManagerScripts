
using System;
using System.Collections.Generic;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Sharing
{
    public class ScheduleModel
    {
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("creationDate")]
        public string CreationDate { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public List<CardModel> ContentList { get; set; } = new List<CardModel>();


        // [JsonPropertyName("content")]
        // public string Content => GetContent();

        // public override string ToString()
        // {
        //     return id + " " + Title + " " + CreationDate + "\n" + string.Join(", ", Content);
        // }

        private string GetContent()
        {
            return JsonSerializer.Serialize<CardModel[]>(ContentList.ToArray());
        }

        public string GetJSON()
        {

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };

            return JsonSerializer.Serialize(this, options);
        }
    }
}
