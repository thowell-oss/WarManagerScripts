/* WeatherData.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Threading;
using System.Threading.Tasks;

using RestSharp;


namespace WarManager.Special
{
    /// <summary>
    /// Handles Getting weather data from a specific lat and long
    /// </summary>
    public class WeatherData
    {
        /// <summary>
        /// The Json file location of the weather data
        /// </summary>
        public static string weatherJSONFilePath => GeneralSettings.GetWarSystemPath + @"\Weather\WeatherData";
        public static string solarJSONFilePath => GeneralSettings.GetWarSystemPath + @"\Weather\SolarData";

        private static List<HourlyWeather> HourlyWeather = new List<HourlyWeather>();

        public static IEnumerable<HourlyWeather> Weather
        {
            get => HourlyWeather;
        }

        public static int WeatherLength => HourlyWeather.Count;

        public static HourlyWeather[] GetWeather()
        {
            return HourlyWeather.ToArray();
        }

        public static void Refresh()
        {
            Refresh(IPAPI.Latitude, IPAPI.Longitude);
        }

        public static void Refresh(double lat, double lng)
        {
            if (File.Exists(weatherJSONFilePath + $"{lat} {lng}.json"))
            {
                FileInfo info = new FileInfo(weatherJSONFilePath + $"{lat} {lng}.json");
            }

            DateTime now = DateTime.UtcNow;

            // if (info.LastWriteTimeUtc < now.AddHours(-24))
            // {
            //     await RefreshWeatherAndSolar(lat, lng, ct);
            // }

            string weatherDataJSON = "";

            using (StreamReader reader = new StreamReader(weatherJSONFilePath + $"{lat} {lng}.json"))
            {
                weatherDataJSON = reader.ReadToEnd();
            }

            RefreshWeatherList(weatherDataJSON);
        }

        private static async Task RefreshWeatherAndSolar(CancellationToken ct)
        {
            try
            {
                if (!Directory.Exists(GeneralSettings.GetWarSystemPath + @"\Weather"))
                    Directory.CreateDirectory(GeneralSettings.GetWarSystemPath + @"\Weather");

                await GetNewWeatherReport(IPAPI.Latitude, IPAPI.Longitude, ct);
                await GetNewSolarReport(IPAPI.Latitude, IPAPI.Longitude, ct);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the weather report
        /// </summary>
        /// <param name="filePath">the place to store the file path</param>
        /// <param name="lat">the latitude of the location</param>
        /// <param name="lng">the longitude of the location</param>
        private static async Task<bool> GetNewWeatherReport(double lat, double lng, CancellationToken ct)
        {
            var client = new RestClient("https://api.stormglass.io/v2/weather/point");
            var request = new RestRequest(Method.GET);
            request.AddParameter("lat", lat);
            request.AddParameter("lng", lng);
            request.AddParameter("params", "precipitation,cloudCover,gust");
            request.AddParameter("source", "noaa");
            request.AddHeader("Authorization",
            "a8eba242-4ac2-11eb-9138-0242ac130002-a8eba2c4-4ac2-11eb-9138-0242ac130002");

            var response = await client.ExecuteAsync(request, ct);

            if (!response.IsSuccessful)
            {
                return false;
            }

            var content = response.Content;

            using (StreamWriter writer = new StreamWriter(weatherJSONFilePath + $"{lat} {lng}.json"))
            {
                await writer.WriteLineAsync(content);
            }
            return true;
        }

        private static async Task<bool> GetNewSolarReport(double lat, double lng, CancellationToken ct)
        {
            var client = new RestClient("https://api.stormglass.io/v2/solar/point");
            var request = new RestRequest(Method.GET);
            request.AddParameter("lat", lat);
            request.AddParameter("lng", lng);
            request.AddParameter("params", "uvIndex");
            request.AddParameter("source", "noaa");
            request.AddHeader("Authorization",
            "a8eba242-4ac2-11eb-9138-0242ac130002-a8eba2c4-4ac2-11eb-9138-0242ac130002");

            var response = await client.ExecuteAsync(request, ct);

            if (!response.IsSuccessful)
            {
                return false;
            }

            var content = response.Content;

            using (StreamWriter writer = new StreamWriter(solarJSONFilePath + $"{lat}, {lng}.json"))
            {
                await writer.WriteLineAsync(content);
            }

            return true;
        }

        private static void RefreshWeatherList(string JSON)
        {
            using (JsonDocument document = JsonDocument.Parse(JSON))
            {
                var root = document.RootElement;

                JsonElement HoursElement = root.GetProperty("hours");

                List<HourlyWeather> hoursList = new List<HourlyWeather>();

                foreach (var x in HoursElement.EnumerateArray())
                {
                    JsonElement cloudCoverElement = x.GetProperty("cloudCover");
                    JsonElement gustElement = x.GetProperty("gust");
                    JsonElement precipitationElement = x.GetProperty("precipitation");
                    JsonElement timeElement = x.GetProperty("time");

                    hoursList.Add(new HourlyWeather()
                    {
                        CloudCover = cloudCoverElement.GetProperty("noaa").GetDouble(),
                        Gust = gustElement.GetProperty("noaa").GetDouble() * 2.2369383, //multiplying by 2.2 in order to convert from meters/second to MPH
                        Precipitation = precipitationElement.GetProperty("noaa").GetDouble(),
                        Time = timeElement.GetDateTime()
                    });
                }

                HourlyWeather.Clear();
                HourlyWeather = hoursList;
            }
        }
    }

    /// <summary>
    /// Represents the weather data sample from a given hour
    /// </summary>
    [Notes.Author("the weather parsed into a class")]
    public class HourlyWeather : IComparable<HourlyWeather>, IEquatable<HourlyWeather>
    {
        /// <summary>
        /// The time of this weather
        /// </summary>
        /// <value></value>
        public DateTime Time { get; set; }

        /// <summary>
        /// The cloud cover (percent)
        /// </summary>
        /// <value></value>
        public double CloudCover { get; set; }

        /// <summary>
        /// The gusts
        /// </summary>
        /// <value></value>
        public double Gust { get; set; }

        /// <summary>
        /// The Precipitation
        /// </summary>
        /// <value></value>
        public double Precipitation { get; set; }

        public int CompareTo(HourlyWeather other)
        {
            if (other == null || (other.Time == null && Time != null) || (other.Time != null && Time == null))
                return 1;

            if (Time == null && other.Time == null)
                return 0;

            return Time.CompareTo(other.Time);
        }

        public bool Equals(HourlyWeather other)
        {
            if (other == null || (other.Time == null && Time != null) || (other.Time != null && Time == null))
                return false;

            if (Time == null && other.Time == null)
                return true;

            return Time == other.Time;
        }

        /// <summary>
        /// Set the date time from a string
        /// </summary>
        /// <param name="time"></param>
        public void SetDateTime(string time)
        {
            if (DateTime.TryParse(time, out var dateTime))
            {
                Time = dateTime;
            }
        }

        public sealed override string ToString()
        {
            return Time.DayOfWeek + " at " + Time.ToShortTimeString() + " Cloud Cover: " + CloudCover + "% Gust: " + string.Format("{0:0.##}", Gust) + " MPH Precipitation: " + Precipitation + " %";
        }
    }
}
