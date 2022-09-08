using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// Handles the pie chart data payload from the data set to the pie chart card element"
    /// </summary>
    [Notes.Author("Handles the pie chart data payload from the data set to the pie chart card element")]
    public class PieChartElementData : CardElementViewData
    {
        [JsonPropertyName("pie chart data")]
        public List<PieChartData> Data = new List<PieChartData>();

        [JsonPropertyName("title font size")]
        public int TitleFontSize { get; set; } = 14;

        [JsonPropertyName("label font size")]
        public int LabelFontSize { get; set; } = 12;

        [JsonPropertyName("title color")]
        public string TitleColorHex { get; set; } = "#eee";

        [JsonPropertyName("label color")]
        public string LabelColorHex { get; set; } = "#eee";

        [JsonPropertyName("label orbit distance")]
        public double LabelOrbitDistanceFromCenter { get; set; } = 2.5;

        [JsonPropertyName("label orbit offset")]
        public double[] LabelOffset { get; set; } = new double[2] { .5f, .5f };

        [JsonPropertyName("value format")]
        public string LabelValueFormat { get; set; } = "percent"; //percent or actual

        [JsonPropertyName("*layout")]
        public override string[] Layout { get => new string[0]; set { } }
    }
}
