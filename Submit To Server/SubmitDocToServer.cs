

using System;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;
using WarManager.Sharing;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StringUtility;

namespace WarManager.Backend
{
    public class SubmitDocToServer
    {

        private static readonly string SubmitURL = "https://wmschedulehandler.azurewebsites.net/api/postschedule";
        private static readonly string BaseViewURL = "https://wmscheduleviewer.azurewebsites.net/api/view?id=";

        //https://wmscheduleviewer.azurewebsites.net/api/view?id=738c4991-ea0a-4505-a9bb-372bcb434026


        /// <summary>
        /// Submit the cards to be put in a document and sent to azure
        /// </summary>
        /// <param name="title">the title of the document</param>
        /// <param name="cards">the cards in the document</param>
        public void SubmitCards(string title, List<Card> cards, bool openBrowser, bool textUser, List<KeyValuePair<string, DataSet>> tags)
        {

            if (cards == null || cards.Count == 0)
            {
                MessageBoxHandler.Print_Immediate("No cards selected.", "Error");
                return;
            }

            CardUtility.SortByGridAndVector(cards, new Point(-1, 1));

            ScheduleModel model = new ScheduleModel();
            model.CreationDate = DateTime.Now.ToShortDateString();
            model.Description = WarSystem.CurrentActiveAccount.UserName;
            model.Title = title;

            Dictionary<DataSet, List<string>> tagDict = new Dictionary<DataSet, List<string>>();

            foreach (var x in tags)
            {
                if (!tagDict.ContainsKey(x.Value))
                {
                    tagDict.Add(x.Value, new List<string>());
                }

                tagDict[x.Value].Add(x.Key);
            }

            List<CardModel> cardModels = new List<CardModel>();

            foreach (var x in cards)
            {

                // Debug.Log(x.Entry.GetAllowedDataValues()[0].ParseToParagraph());

                string name = "";

                // if (x.Entry.TryGetValueWithHeader(tags[0], out var values))
                // {
                //     name = values.Value.ToString();
                // }

                List<ScheduleDescription> descriptions = new List<ScheduleDescription>();

                List<string[]> someTags = new List<string[]>();

                if (tagDict.ContainsKey(x.DataSet))
                {
                    foreach (var y in tagDict[x.DataSet])
                    {
                        string[] tag = new string[1] { y };
                        someTags.Add(tag);
                    }

                }

                foreach (var y in x.Entry.DataSet.GetPrintableInfo(x.Entry, someTags))
                {
                    descriptions.Add(new ScheduleDescription()
                    {
                        Name = y.name,
                        Value = y.value,
                    });
                }

                cardModels.Add(new CardModel()
                {
                    Name = name,
                    Type = x.DataSet.PrintFormat,
                    Descriptions = descriptions
                });
            }

            model.ContentList = cardModels;

            string str = model.GetJSON();
            MessageBoxHandler.Print_Immediate("Are you sure you want to publish?", "Question", (x) =>
                {
                    if (x)
                    {

                        string parameterId = "";

                        try
                        {
                            parameterId = Publish(str);

                        }
                        catch (Exception ex)
                        {
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Error submitting document: " + ex.Message, "Error");
                            });

                            return;
                        }

                        string link = BaseViewURL + parameterId;

                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate(link, "Your Link to View the Schedule:");
                        });


                        LeanTween.delayedCall(2, () =>
                        {
                            if (openBrowser)
                                Application.OpenURL(link);

                            if (textUser)
                            {
                                Debug.Log("texting user");
                                TwilioSMSHandler handler = new TwilioSMSHandler();
                                handler.SendMessage(title.SetStringQuotes() + " link: " + link, WarSystem.AccountPreferences.PhoneNumber, false, true);
                            }

                            Debug.Log("could not text user");
                        });
                    }
                });
        }

        /// <summary>
        /// Publish the document
        /// </summary>
        /// <param name="modelJson">the json document</param>
        /// <returns>returns the id so the document can be looked up</returns>
        private string Publish(string modelJson)
        {

            RestClient client = new RestClient();
            RestRequest request = new RestRequest();
            request.AddParameter("application/json", modelJson, ParameterType.RequestBody);

            request.Method = Method.POST;

            client.BaseUrl = new Uri(SubmitURL);
            var response = client.Execute(request, Method.POST);

            if (response.IsSuccessful)
            {
                return response.Content.Remove(0, 1).Remove(response.Content.Length - 2, 1);
            }
            else
            {
                throw new Exception(response.ErrorMessage + " " + response.StatusCode + " " + response.StatusDescription);
            }
        }
    }
}
