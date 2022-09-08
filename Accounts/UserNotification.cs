
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using WarManager.Backend;

namespace WarManager.Sharing
{
    /// <summary>
    /// Handles the storage and sharing data of a specific message
    /// </summary>
    [Serializable]
    [Notes.Author("Handles the storage and sharing of a specific message")]
    public class UserNotification : IComparable<UserNotification>
    {
        /// <summary>
        /// The title of the notification
        /// </summary>
        /// <value></value>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// The message of the notification
        /// </summary>
        /// <value></value>
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        /// <summary>
        /// The email of the from user
        /// </summary>
        /// <value></value>
        [JsonPropertyName("from")]
        public string From { get; set; } = "";

        /// <summary>
        /// The email of the to user
        /// </summary>
        /// <value></value>
        [JsonPropertyName("to")]
        public string[] To { get; set; } = new string[0];

        /// <summary>
        /// Is the message a task?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("task")]
        public bool IsTaskMessage { get; set; } = false;

        /// <summary>
        /// Has the task been completed?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("complete")]
        public bool TaskCompleted { get; set; } = false;

        /// <summary>
        /// Mark the message as read
        /// </summary>
        /// <value></value>
        [JsonPropertyName("mark as read")]
        public bool MessageRead { get; set; } = false;

        /// <summary>
        /// Has the message been flagged?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("flagged")]
        public bool MarkAsFlagged { get; set; } = false;

        /// <summary>
        /// Entries that are attached to the message
        /// </summary>
        /// <value></value>
        [JsonPropertyName("attached entries")]
        public string[] AttachedEntryIDs { get; set; } = new string[0];

        /// <summary>
        /// Sheets that are attached to the message
        /// </summary>
        /// <value></value>
        [JsonPropertyName("attached sheet locations")]
        public string[] AttachedSheetPoints { get; set; } = new string[0];

        /// <summary>
        /// War Manager instructions
        /// </summary>
        /// <value></value>
        [JsonPropertyName("instructions")]
        public string WarManagerInstructions { get; set; } = "";

        /// <summary>
        /// The id of the message
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string ID { get; set; } = "";

        public int CompareTo(UserNotification other)
        {
            return Title.CompareTo(other.Title);
        }

        /// <summary>
        /// Get the json file of the user notification
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// To string standard - prints out the user notification details in a json format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetJson();
        }


        /// <summary>
        /// Add a user name to the list of users to send the message to
        /// </summary>
        /// <param name="userName"></param>
        public void AddToUsersToSend(string userName)
        {
            var users = new List<string>();

            users.AddRange(To);
            users.Add(userName);

            To = users.ToArray();
        }

        /// <summary>
        /// Remove a user name from the list of users to send the message to
        /// </summary>
        /// <param name="userName"></param>
        public void RemoveFromUsersToSend(string userName)
        {
            var users = new List<string>();

            users.AddRange(To);
            users.Remove(userName);

            To = users.ToArray();
        }

        /// <summary>
        /// Get all user accounts from the sender list
        /// </summary>
        /// <returns></returns>
        public List<Account> GetUserAccountsFromToList()
        {
            var userNames = new List<Account>();

            List<Account> accounts = Account.GetAccounts();

            foreach (var userName in To)
            {
                foreach (var acct in accounts)
                {
                    if (acct.UserName == userName)
                    {
                        userNames.Add(acct);
                    }
                }
            }

            return userNames;
        }

        /// <summary>
        /// Get the user account from the sender (From:) list
        /// </summary>
        /// <returns>returns the account if the operation was successful, if not returns null</returns>
        public Account GetSenderUserAccount()
        {
            List<Account> accounts = Account.GetAccounts();

            foreach (var acct in accounts)
            {
                if (acct.UserName == From)
                {
                    return acct;
                }
            }

            return null;
        }

        /// <summary>
        /// Add user entry to the user notification
        /// </summary>
        /// <param name="entry">The entry</param>
        public void AddEntry(string setID, string entryID)
        {
            var entries = new List<string>();
            entries.AddRange(AttachedEntryIDs);
            entries.Add(setID + "," + entryID);

            AttachedEntryIDs = entries.ToArray();
        }

        /// <summary>
        /// Get Attached data entries
        /// </summary>
        /// <returns></returns>
        public DataEntry[] GetAttachedDataEntries()
        {
            List<DataEntry> entries = new List<DataEntry>();

            foreach (var x in WarSystem.DataSetManager.Datasets)
            {
                foreach (var y in AttachedEntryIDs)
                {
                    string[] str = y.Split(',');

                    // if (Int32.TryParse(str[1], out var row))
                    // {
                    if (str[0] == x.ID)
                    {
                        entries.Add(x.GetEntry(str[1]));
                    }
                    // }
                }
            }

            return entries.ToArray();
        }

        /// <summary>
        /// Remove the user entry from the user notification
        /// </summary>
        /// <param name="entryId">the entry id to remove</param>
        public void RemoveEntry(string setID, string entryID)
        {
            var entries = new List<string>();
            entries.AddRange(AttachedEntryIDs);

            entries.Remove(setID + "," + entryID);

            AttachedEntryIDs = entries.ToArray();
        }


        /// <summary>
        /// Add a sheet point to the user notification
        /// </summary>
        /// <param name="sheetid">the sheet id</param>
        /// <param name="location">the point location of the id</param>
        public void AddSheetPoint(string sheetid, Point location)
        {
            string str = sheetid + ":" + location.x + "," + location.y;

            var sheets = new List<string>();
            sheets.AddRange(AttachedSheetPoints);
            sheets.Add(str);
            AttachedSheetPoints = sheets.ToArray();
        }


        /// <summary>
        /// Remove a sheet point to the user notification
        /// </summary>
        /// <param name="sheetid">the sheet id</param>
        /// <param name="location">the point location of the id</param>
        public void RemoveSheetPoint(string sheetid, Point location)
        {
            string str = sheetid + ":" + location.x + "," + location.y;

            var sheets = new List<string>();
            sheets.AddRange(AttachedSheetPoints);

            sheets.Remove(str);
            AttachedSheetPoints = sheets.ToArray();
        }

        /// <summary>
        /// Toggle the message as a task
        /// </summary>
        public void ToggleMessageAsTask()
        {
            IsTaskMessage = !IsTaskMessage;

            if (!IsTaskMessage)
            {
                TaskCompleted = false;
            }
        }

        /// <summary>
        /// Toggle the task message as completed (or not)
        /// </summary>
        [Obsolete("use UserPersistanceNotificationsHandler.ToggleNotificationTask() instead")]
        public void ToggleTaskCompleted()
        {
            if (!IsTaskMessage)
            {
                UnityEngine.Debug.Log("not a task message");
                return;
            }

            UnityEngine.Debug.Log("Task message before " + TaskCompleted);

            TaskCompleted = !TaskCompleted;

            UnityEngine.Debug.Log("Task message after " + TaskCompleted);
        }
    }
}
