

using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Threading;
using System.Threading.Tasks;

using RestSharp;

namespace WarManager.Special
{

    public static class RandomWordFetcher
    {
        /// <summary>
        /// Get a random word
        /// </summary>
        /// <returns></returns>
        public static string GetRandomWord()
        {
            try
            {
                var client = new RestClient("https://random-word-api.herokuapp.com/word");
                var request = new RestRequest(Method.GET);

                var response = client.Execute(request);

                string str = "";


                if (response.IsSuccessful)
                {
                    str = response.Content.Remove(response.Content.Length - 2, 2).Remove(0, 2);
                }
                else
                {
                    byte[] b = new byte[5];

                    for (int i = 0; i < 5; i++)
                    {
                        Random r = new Random();
                        b[i] = (byte)r.Next(97, 122);

                        str = Encoding.ASCII.GetString(b);

                        return str;
                    }
                }

                return str;

            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }

            return "";
        }
    }
}
