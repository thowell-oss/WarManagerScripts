
using System;
using System.Security;
using System.IO;
using System.Net;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Sharing;
using WarManager.Sharing.Security;

using System.Text.Json;
using System.Text.Json.Serialization;


namespace WarManager.Logging
{

    [Notes.Author("Logs information into a text file to create a paper trial of what has happened.")]
    public class LogHandler
    {
        /// <summary>
        /// The loction where all the logs are stored
        /// </summary>
        private string _logLocation
        {
            get
            {
                if (WarSystem.IsConnectedToServer)
                {
                    return _onlineLogPath;
                }
                else
                {
                    return _offlinePath;
                }
            }
        }

        private List<LogAction> _logs = new List<LogAction>();

        #region old log system

        /// <summary>
        /// The full details of the session
        /// </summary>
        private string _fullDetails;
        public bool Offline = false;
        private string _pathExtension = @"\Data\War System\Logs\";
        private string _onlineLogPath;
        private string _offlinePath;

        [SerializeField]
        private string _sessionString;
        private string fileName;

        private string lastDate;
        private string lastTime;
        private string lastID;
        private string lastIP;
        private string lastUserName;

        private Account currentUser;

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public LogHandler()
        {
            StartSession();
        }

        /// <summary>
        /// Start a new session with a new user
        /// </summary>
        private void StartSession()
        {
            if (WarSystem.CurrentActiveAccount == null)
            {
                fileName = DateTime.UtcNow.ToFileTime() + ".json";
            }
            else
            {
                fileName = WarSystem.CurrentActiveAccount.UserName + " " + DateTime.UtcNow.ToFileTime() + ".json";
            }

            //WriteToLog("New session started");
            NewMessage("New Session Started", Logging.MessageType.logEvent);

            currentUser = WarSystem.CurrentActiveAccount;

        }

        private void Refresh()
        {
            CheckFullDetails();

            _onlineLogPath = GeneralSettings.Save_Location_Server + _pathExtension + fileName;

            _offlinePath = GeneralSettings.Save_Location_Offline + _pathExtension + fileName;

            if (!Directory.Exists(GeneralSettings.Save_Location_Server + _pathExtension) && WarSystem.IsConnectedToServer)
            {
                Directory.CreateDirectory(GeneralSettings.Save_Location_Server + _pathExtension);
            }


            if (WarSystem.IsConnectedToServer && !Directory.Exists(GeneralSettings.Save_Location_Offline + _pathExtension))
            {
                Directory.CreateDirectory(GeneralSettings.Save_Location_Offline + _pathExtension);
            }
        }

        /// <summary>
        /// Check the details of the session to ensure accuracy
        /// </summary>
        private void CheckFullDetails()
        {
            string nextStamp = "";

            string date = WarSystem.CurrentDate;
            string time = WarSystem.CurrentTime;
            string id = WarSystem.DeviceID;
            string ip = WarSystem.IPAddress;

            if (date != lastDate || lastDate == null)
            {
                lastDate = date;
                nextStamp += "\n\tDate: " + lastDate;
            }

            if (time != lastTime || lastTime == null)
            {
                lastTime = time;
                nextStamp += "\n\tTime: " + lastTime;
            }

            if (id != lastID || lastID == null)
            {
                lastID = id;
                nextStamp += "\n\tDevice ID:" + lastID;
            }

            if (ip != lastIP || lastID == null)
            {
                lastIP = ip;
                nextStamp += "\n\tIP Address: " + lastIP;
            }

            if (WarSystem.CurrentActiveAccount != null && WarSystem.CurrentActiveAccount.UserName != lastUserName)
            {
                lastUserName = WarSystem.CurrentActiveAccount.UserName;
                _fullDetails = "User: " + lastUserName + " (" + WarSystem.CurrentActiveAccount.FirstName + " " + WarSystem.CurrentActiveAccount.LastName + ").";
            }
            else if (WarSystem.CurrentActiveAccount == null)
            {
                _fullDetails = "Error: Not logged in! " + Guid.NewGuid().ToString().Substring(10);
            }
            else
            {
                _fullDetails = "";
            }

            _fullDetails += nextStamp;
        }

        /// <summary>
        /// open the file and write a new line of log information
        /// </summary>
        /// <param name="newLog">the new string of info</param>
        [Obsolete("Use NewMessage() instead", true)]
        public void WriteToLog(string newLog)
        {
            Refresh();
            string final = "\n" + newLog + "\n\t" + _fullDetails;

            _sessionString += final; //TODO: Store and send this to me


            // if (!WarSystem.IsConnectedToServer || _onlineLogPath == null)
            // {
            //     using (Stream str = File.OpenWrite(_logLocation))
            //     {
            //         using (StreamWriter writer = new StreamWriter(str))
            //         {
            //             foreach (char ch in _sessionString)
            //             {
            //                 writer.Write(ch);
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     using (Stream str = File.OpenWrite(_logLocation))
            //     {
            //         using (StreamWriter writer = new StreamWriter(str))
            //         {
            //             foreach (char ch in _sessionString)
            //             {
            //                 writer.Write(ch);
            //             }
            //         }
            //     }
            // }
        }

        /// <summary>
        /// Log a new message
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="logType">the type of message logged</param>
        public void NewMessage(string message, MessageType logType)
        {
            Refresh();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.AllowTrailingCommas = true;

            _logs.Add(new LogAction(message, logType));

            if (currentUser == null && WarSystem.CurrentActiveAccount != null)
            {
                FileInfo info = new FileInfo(_logLocation);
                string newName = "\\" + WarSystem.CurrentActiveAccount.UserName + info.Name;

                string fullDirectory = info.Directory + newName;
                // Debug.Log(fullDirectory);

                fileName = newName;
                Refresh();
                currentUser = WarSystem.CurrentActiveAccount;

                // if (File.Exists(_logLocation))
                // {
                //     File.Move(_logLocation, fullDirectory);

                //     if (File.Exists(_logLocation))
                //         File.Delete(_logLocation);

                //     _onlineLogPath = fullDirectory;

                //     Debug.Log("moved");

                //     currentUser = WarSystem.CurrentActiveAccount;
                // }
                // else
                // {
                //     UnityEngine.Debug.Log("could not find " + _logLocation);
                // }
            }


            using (FileStream stream = File.OpenWrite(_logLocation))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string data = JsonSerializer.Serialize(_logs, options);
                    writer.Write(data);
                }
            }

            if (logType == MessageType.critical)
            {

#if UNITY_EDITOR
                Debug.LogError(message);
#endif

                NotificationHandler.Print("Error! " + message);
                EmailClient.SendNotificationSMTPEmailToDev($"{WarSystem.CurrentActiveAccount.UserName} - Critical Error", $"{message}\n\n{WarSystem.ConnectedDeviceStamp}");
            }
        }

        /// <summary>
        /// End the logging session
        /// </summary>
        /// <param name="termination">was the session forcefully terminated?</param>
        public void EndSession(bool termination, string reason = "")
        {
            SendLogIfPossible();

            if (termination)
            {
                //WriteToLog("End - something caused termination");
                NewMessage("Session Ended - Something caused Termination\n" + reason, Logging.MessageType.critical);
            }
            else
            {
                // WriteToLog("End");
                NewMessage("Session Ended", Logging.MessageType.logEvent);
            }
        }

        /// <summary>
        /// Sends the offline log to me if necessary
        /// </summary>
        public void SendLogIfPossible()
        {

        }
    }
}
