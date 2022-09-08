
using System;
using RestSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace WarManager.Sharing.Security
{
    /// <summary>
    /// Group Me push notifications to developer
    /// </summary>
    [Notes.Author("GroupMe push notifications that get sent to a registered group chat for the developer")]
    public class GroupMeDeveloperPushNotificationHandler
    {

        private static string bot_id = "";

        private bool error = false;

        private static string[] _greetings = new string[6] { "Taylor,", "Hey Taylor,", "Yo,", "Hey,", "Heyoo!", "" };
        private static string[] _errorGreetings = new string[3] { "Error!", "We got a problem -", "" };
        private static string[] _logGreetings = new string[3] { "FYI,", "Just a log -", "" };

        private string path = GeneralSettings.Save_Location_Server + @"\Data\War System\keys.txt";

        public bool MovedCards { get; set; } = false;
        public bool OpenedSheets { get; set; } = false;
        public bool ChangedAccountSettings { get; set; } = false;
        public bool CreatedSheets { get; set; } = false;
        public bool EditedData { get; set; } = false;

        public bool ReplacedData { get; set; } = false;

        private int Errors = 0;

        /// <summary>
        /// Create the logout message
        /// </summary>
        /// <returns></returns>
        public string CreateLogoutMessage()
        {
            string finalStr = "It looks like the user ";

            List<string> logoutMessage = new List<string>();

            if (MovedCards)
                logoutMessage.Add("moved some cards");

            if (OpenedSheets)
                logoutMessage.Add("opened a sheet or two");

            if (CreatedSheets)
                logoutMessage.Add("created sheets");

            if (EditedData)
                logoutMessage.Add("edited data");

            if (ChangedAccountSettings)
                logoutMessage.Add("changed account settings");

            if (ReplacedData)
                logoutMessage.Add("replaced data");

            if (logoutMessage.Count > 1)
            {
                string final = logoutMessage[logoutMessage.Count - 1];
                logoutMessage.RemoveAt(logoutMessage.Count - 1);

                finalStr += string.Join(", ", logoutMessage) + ", and " + final + ".";
            }
            else
            {
                finalStr += logoutMessage[0] + ".";
            }

            if (Errors > 1)
                finalStr += " There were " + Errors + " errors.";
            else if (Errors == 1)
                finalStr += " There was 1 error.";
            else
                finalStr += " There were no errors.";

            return finalStr;
        }

        public void IncrementErrors()
        {
            Errors += 1;
        }

        public GroupMeDeveloperPushNotificationHandler()
        {

            try
            {

                if (System.IO.File.Exists(path))
                {

                    string file = System.IO.File.ReadAllText(path);
                    string[] keys = file.Split(new char[1] { '\n' });
                    bot_id = keys[7].Remove(0, "botID: ".Length);

                    WarSystem.WriteToLog("Found GroupMe Bot ID", Logging.MessageType.info);
                }
                else
                {
                    WarSystem.WriteToLog("Could not Found GroupMe Bot ID", Logging.MessageType.info);
                    throw new System.IO.FileNotFoundException("Cannot find the Found GroupMe Bot ID");
                }
            }
            catch (Exception ex)
            {
                SendErrorEmail("Could not find Group Me bot id", ex.Message);
            }

        }

        /// <summary>
        /// Send a push message
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="type">the message type</param>
        public void SendPushMessage(string message, WarManager.Logging.MessageType type)
        {

            if (message == null || message.Trim() == string.Empty)
                return;

            if (error)
            {
                SendErrorEmail(message, "internal error");
                return;
            }


            if (bot_id == null || bot_id.Trim() == string.Empty)
            {
                SendErrorEmail(message, "bot id is null or empty");
                return;
            }


            int total = _greetings.Length - 1;

            if (type == WarManager.Logging.MessageType.critical || type == WarManager.Logging.MessageType.error)
                total = _errorGreetings.Length - 1;

            if (type == WarManager.Logging.MessageType.info || type == WarManager.Logging.MessageType.debug)
                total = _logGreetings.Length - 1;

            int rand = UnityEngine.Random.Range(0, total);
            string greeting = _greetings[rand];

            // string body = $"{{\"text\" : \"{greeting} {message}\", \"bot_id\" : \"{bot_id}\"}}";
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            GroupMeMessage payload = new GroupMeMessage(greeting + " " + message, bot_id);

            string body = JsonSerializer.Serialize<GroupMeMessage>(payload);

            var client = new RestClient("https://api.groupme.com/v3/bots/post");
            var request = new RestRequest(Method.POST);
            request.AddJsonBody(body);

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {

            }
            else
            {
                error = true;
                SendErrorEmail(message, response.Content);
            }
        }

        /// <summary>
        /// Send an email as fallback
        /// </summary>
        /// <param name="groupMeMessage">the message</param>
        /// <param name="ErrorMessage">the error content</param>
        private void SendErrorEmail(string groupMeMessage, string ErrorMessage)
        {
            //WarSystem.WriteToLog("could not send Group Me push notifications " + ErrorMessage, Logging.MessageType.error);
            EmailClient.SendNotificationSMTPEmailToDev("Fallback Message (groupMe failed)", groupMeMessage + "\n\nGroup Me Error:\n" + ErrorMessage, false);
#if UNITY_EDITOR
            UnityEngine.Debug.Log(ErrorMessage);
#endif
        }
    }

    [Serializable]
    public class GroupMeMessage
    {
        [JsonPropertyName("text")]
        public string Message { get; private set; }

        [JsonPropertyName("bot_id")]
        public string BotID { get; private set; }

        [JsonConstructor]
        public GroupMeMessage(string message, string botId)
        {
            Message = message;
            BotID = botId;
        }
    }
}