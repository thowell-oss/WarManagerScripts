using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Special;

namespace WarManager.Backend.CardsElementData
{

    /// <summary>
    /// Provide weather data
    /// </summary>
    [Notes.Author("Provide weather data")]
    public sealed class WeatherDataElementDriver : CardElementViewData
    {
        private List<HourlyWeather> Weather = new List<HourlyWeather>();

        public override string[] Layout { get; set; }

        public WeatherDataElementDriver(HourlyWeather[] weather)
        {
            Weather.AddRange(weather);
        }
    }
}
