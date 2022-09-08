using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// loading bar to be used like the radial dial
    /// </summary>
    [Notes.Author("loading bar to be used like the radial dial")]
    public sealed class LoadingBarElementData : CardElementViewData
    {
        [JsonPropertyName("bar color")]
        public string BarColorHex = "#aaeeaa";

        [JsonPropertyName("use gradient")]
        public bool UseGradientColor = true;

        [JsonPropertyName("bar color gradient top")]
        public string BarColorGradientTop = "#aaeeaa";

        [JsonPropertyName("background color")]
        public string BackgroundColorHex = "#111";

        [JsonPropertyName("minimum value")]
        public double Min { get; set; }

        [JsonPropertyName("maximum value")]
        public double Max { get; set; }

        [JsonPropertyName("title text font size")]
        public int TitleTextFontSize = 14;

        [JsonPropertyName("title text color")]
        public string TitleTextColor = "#eee";

        [JsonPropertyName("percent text?")]
        public string UsePercentText = "left"; //left, right, or none
        [JsonPropertyName("percent text font size")]
        public int PercentTextFontSize = 14;

        [JsonPropertyName("percent text color")]
        public string PercentTextColor = "#eee";

        [JsonPropertyName("use line")]
        public bool UseLine = true;

        [JsonPropertyName("line color")]
        public string LineColor = "#eee";

        [JsonPropertyName("*layout")]
        public override string[] Layout { get; set; }

        public LoadingBarElementData()
        {
            ColumnType = "bar";
        }
    }
}