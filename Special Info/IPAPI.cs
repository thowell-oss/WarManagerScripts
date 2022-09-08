
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using RestSharp;
using System.Threading.Tasks;

namespace WarManager.Special
{
    public static class IPAPI
    {
        public static bool DataRefreshed = false;

        public static string PublicIPAddress { get; private set; }
        public static string City { get; private set; }
        public static string Region { get; private set; }
        public static string RegionCode { get; private set; }
        public static string Country { get; private set; }
        public static string PostalCode { get; private set; }
        public static double Latitude { get; private set; }
        public static double Longitude { get; private set; }

        public static void Refresh()
        {
            var client = new RestClient($"https://ipapi.co/json");
            var request = new RestRequest(Method.GET);

            var response = client.Execute(request);

            if (!response.IsSuccessful)
                throw new Exception("public ip ping not successful " + response.ResponseStatus + " " + response.Content);

            var content = response.Content;

            Debug.Log(content);

            using (JsonDocument doc = JsonDocument.Parse(content))
            {
                var root = doc.RootElement;

                PublicIPAddress = root.GetProperty("ip").GetString();
                City = root.GetProperty("city").GetString();
                Region = root.GetProperty("region").GetString();
                RegionCode = root.GetProperty("region_code").GetString();
                Country = root.GetProperty("country").GetString();
                PostalCode = root.GetProperty("postal").GetString();
                Latitude = root.GetProperty("latitude").GetDouble();
                Longitude = root.GetProperty("longitude").GetDouble();
            }

            DataRefreshed = true;
        }
    }
}
