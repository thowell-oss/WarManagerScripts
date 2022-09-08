
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// handles individual pie chart data
    /// </summary>
    [Notes.Author("Pie chart data")]
    [Serializable]
    public class PieChartData
    {
        /// <summary>
        /// The data column of the selected value
        /// </summary>
        /// <value></value>
        [JsonPropertyName("column")]
        [SerializeField]
        public int Column { get; set; } = 1;

        /// <summary>
        /// Should the min and max values be set in json? or found in the data?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("clamp value")]
        public string ClampValue { get; set; } = "json"; //data or json

        /// <summary>
        /// The column to find the min value (if 'ClampValue' is set to data)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("min value column")]
        public int MinValueColumn { get; set; } = 2;

        /// <summary>
        /// The column to find the max value (if 'ClampValue' is set to data)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("max value column")]
        public int MaxValueColumn { get; set; } = 2;

        /// <summary>
        /// The minimum the number will be (if 'ClampValue' is set to json)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("minimum value")]
        public float Min { get; set; } = 0;

        /// <summary>
        /// The maximum the number will be (if 'ClampValue' is set to json)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("maximum value")]
        public float Max { get; set; } = 1;

        /// <summary>
        /// the hex color code
        /// </summary>
        /// <value></value>
        [JsonPropertyName("color")]
        public string HexColor { get; set; } = "#aa5555"; //red ish

    }
}
