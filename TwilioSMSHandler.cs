
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RestSharp;
using WarManager;
using WarManager.Cards;
using WarManager.Backend;
using System.Threading.Tasks;
using RestSharp.Authenticators;

namespace WarManager.Sharing
{
    /// <summary>
    /// Handles API calls in order to send sms messages
    /// </summary>
    [Notes.Author("Handles API calls in order to send sms messages")]
    public class TwilioSMSHandler
    {
        /// <summary>
        /// The API User Key
        /// </summary>
        private string _APIUserKey = "";

        /// <summary>
        /// The API User Password
        /// </summary>
        private string _APIUserPassword = "";

        /// <summary>
        /// The Twilio Phone Number
        /// </summary>
        private string _fromPhoneNumber = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public TwilioSMSHandler()
        {
            string path = GeneralSettings.Save_Location_Server + @"\Data\War System\keys.txt";
            try
            {

                if (System.IO.File.Exists(path))
                {

                    string file = System.IO.File.ReadAllText(path);

                    string[] keys = file.Split(new char[1] { '\n' });

                    _APIUserKey = keys[0].Trim();
                    _APIUserPassword = keys[1].Trim();
                    _fromPhoneNumber = keys[2].Trim();

                    WarSystem.WriteToLog("Found keys to use Twilio", Logging.MessageType.info);
                }
                else
                {
                    WarSystem.WriteToLog("Could not find keys to use Twilio", Logging.MessageType.info);
                    throw new System.IO.FileNotFoundException("Cannot find the keys file for twilio");
                }
            }
            catch (Exception ex)
            {
                WarSystem.WriteToLog("Could not find keys to use Twilio " + ex.Message, Logging.MessageType.info);
            }
        }


        /// <summary>
        /// Send SMS message
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="toPhoneNumber">the phone number to send the message</param>
        /// <returns></returns>
        public bool SendMessage(string message, string toPhoneNumber, bool notify = true, bool automated = false)
        {



            if (UnitedStatesPhoneNumber.IsDefaultPhoneNumber(toPhoneNumber))
            {
                MessageBoxHandler.Print_Immediate("It seems that the phone number you're trying to reach does not exist (000) 000 - 0000", "Error");
                WarSystem.WriteToLog("Message " + message + " attempted to send to " + toPhoneNumber, Logging.MessageType.error);
                return false;
            }

            // #if UNITY_EDITOR
            //             if (toPhoneNumber != "+19137497477")
            //             {
            //                 message = "to " + toPhoneNumber + ": " + message;
            //                 toPhoneNumber = "+19137497477";
            //             }
            // #endif

            Debug.Log(toPhoneNumber);


            if (string.IsNullOrEmpty(message)) throw new ArgumentException("The message cannot be null or empty");

            if (string.IsNullOrEmpty(toPhoneNumber)) throw new ArgumentException("The phone number cannot be null or empty");

            if (automated)
            {
                message += "\n\n- War Manager Assistant";
            }
            else
            {
                message += "\n\n- " + WarSystem.CurrentActiveAccount.UserName + "\n(War Manager User)";
            }

            var client = new RestClient($"https://api.twilio.com/2010-04-01/Accounts/{_APIUserKey}/Messages.json");
            client.Authenticator = new HttpBasicAuthenticator(_APIUserKey, _APIUserPassword);

            var request = new RestRequest(Method.POST);
            request.AddParameter("Body", message);
            request.AddParameter("From", _fromPhoneNumber);
            request.AddParameter("To", toPhoneNumber);

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                if (notify)
                {
                    MessageBoxHandler.Print_Immediate("Message Sent Successfully", "Success");
                }

                WarSystem.WriteToLog("SMS Message \'" + message + "\'" + " sent to " + toPhoneNumber, Logging.MessageType.logEvent);
                WarSystem.WriteToLog(response.Content, Logging.MessageType.debug);

                return true;
            }
            else
            {
                if (notify)
                {
                    MessageBoxHandler.Print_Immediate("Error! Message Was Not Sent Successfully", "Error");
                }
                else
                {
                    Debug.LogError("Error!" + response.Content);
                }

                WarSystem.WriteToLog("SMS Message \'" + message + "\'" + " was not sent to " + toPhoneNumber + " . Twilio server response: " + response.Content, Logging.MessageType.error);

                return false;
            }

            Debug.Log(response.Content);
        }
    }
}
