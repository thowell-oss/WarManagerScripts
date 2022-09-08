
using System;
using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

using WarManager;
using WarManager.Sharing;

namespace WarManager.Logging
{
    [Notes.Author("Record type for the logger to use an cache the latest actions")]
    public class LogAction : IComparable<LogAction>, IEquatable<LogAction>
    {
        /// <summary>
        /// The message that War Manager wants to say
        /// </summary>
        /// <value></value>
        public string Message { get; private set; } = "";

        /// <summary>
        /// The type of message being written
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public MessageType MessageType { get; private set; } = MessageType.none;

        /// <summary>
        /// The string formatted version of the message type
        /// </summary>
        /// <returns></returns>
        public string LogType => MessageType.ToString();

        /// <summary>
        /// The time that the message was created
        /// </summary>
        /// <value></value>
        public DateTime Time { get; private set; } = DateTime.Now;

        /// <summary>
        /// The device ID of the computer using War Manager
        /// </summary>
        /// <value></value>
        public string DeviceID { get; private set; }

        /// <summary>
        /// The ip address that is logged when the message is created
        /// </summary>
        /// <value></value>
        public string IPAddress { get; private set; }

        /// <summary>
        /// The account that was using War Manager when the message was created
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Account UserAccount { get; private set; }

        /// <summary>
        /// The username of the account
        /// </summary>
        public string AccountUserName
        {
            get
            {
                if (UserAccount != null && UserAccount.UserName != null) return UserAccount.UserName;
                else return "User not logged in";
            }

        }

        /// <summary>
        /// Creates a log stamp (no message)
        /// </summary>
        public LogAction()
        {
            Message = "";

            IPAddress = WarSystem.IPAddress;
            Time = DateTime.Now;
            UserAccount = WarSystem.CurrentActiveAccount;
            MessageType = MessageType.none;
        }

        /// <summary>
        /// Create a message
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="type">the type of message being submitted</param>
        public LogAction(string message, MessageType type)
        {
            if (message == null)
                throw new NullReferenceException("The message cannot be null");

            IPAddress = WarSystem.IPAddress;
            Time = DateTime.Now;
            UserAccount = WarSystem.CurrentActiveAccount;
            Message = message;
            MessageType = type;
            DeviceID = WarSystem.DeviceID;
        }

        /// <summary>
        /// Create a message
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="IP">the IP address</param>
        /// <param name="time">the dateTime</param>
        /// <param name="user">the user account</param>
        /// /// <param name="type">the type of message being submitted</param>
        public LogAction(string message, MessageType type, string IP, string deviceID, DateTime time, Account user)
        {

            if (message == null)
                throw new NullReferenceException("The message cannot be null");

            if (time == null)
                throw new NullReferenceException("The time cannot be null");

            if (IP == null)
                throw new NullReferenceException("the ip address cannot be null");

            Regex regex = new Regex(@"(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}", RegexOptions.Singleline);

            if (!regex.IsMatch(IP))
                throw new ArgumentException("The string given as an IP address does not have the correct format");

            if (user == null)
                throw new NullReferenceException("The user account cannot be null");

            if (deviceID == null)
                throw new NullReferenceException("The device ID is null");

            Message = message;
            Time = time;
            DeviceID = deviceID;
            UserAccount = user;
            MessageType = type;
        }

        public int CompareTo(LogAction other)
        {
            if (other == null || Time == null || other.Time == null) return 0;

            return Time.CompareTo(other.Time);
        }

        public bool Equals(LogAction other)
        {
            if (other == null || (Time == null || other.Time == null) && Time != other.Time) return false;

            return Time == other.Time;
        }
    }

    /// <summary>
    /// The type of message logged
    /// </summary>
    [Notes.Author("the type of message being submitted to the logger")]
    public enum MessageType
    {
        ///<summary> For testing or insignificant messages</summary>
        none,
        ///<summary>General information</summary>
        info,
        ///<summary>The user did something that caused an error - not software breaking</summary>
        error,
        ///<summary>Software breaking bugs. An email gets sent to the developer</summary>
        critical,
        ///<summary>Logs for any debugging scenarios</summary>
        debug,
        ///<summary>A significant event happened. Usually something important to an admin user for reporting purposes</summary>
        logEvent,
    }
}