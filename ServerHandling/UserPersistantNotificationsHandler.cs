
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace WarManager.Sharing
{
    /// <summary>
    /// Handles bulk notifications
    /// </summary>
    [Serializable]
    [Notes.Author("Handles bulk notifications")]
    public class UserPersistantNotificationsHandler
    {
        /// <summary>
        /// The server path for the notifications
        /// </summary>
        [JsonIgnore]
        public static readonly string ServerPath = GeneralSettings.Save_Location_Server + @"\Data\ServerNotifications.json";

        /// <summary>
        /// The server path for the notifications
        /// </summary>
        [JsonIgnore]
        private static readonly string _serverFilePath = GeneralSettings.Save_Location_Server + @"\Data";

        /// <summary>
        /// The server path for the notifications
        /// </summary>
        [JsonIgnore]
        private static readonly string _serverBackUpPath = GeneralSettings.Save_Location_Server + @"\Data\War System\Backup Server Notifications\Server Notifications-Backup";

        /// <summary>
        /// is there a deserialization error?
        /// </summary>
        private bool _deserializationError = false;

        /// <summary>
        /// The list of notifications gathered from the server
        /// </summary>
        /// <typeparam name="UserNotification"></typeparam>
        /// <returns></returns>
        [JsonPropertyName("notifications")]
        public UserNotification[] Notifications { get; set; } = new UserNotification[0];

        public UserPersistantNotificationsHandler()
        {
            WarSystem.WriteToLog("User persistant notifications handler created", Logging.MessageType.logEvent);
        }

        /// <summary>
        /// Initialize the file watcher
        /// </summary>
        /// <remarks>Does not work with unity runtime</remarks>
        public void InitFileWatcher()
        {

            // Debug.Log("Init file watcher " + ServerPath);

            if (Directory.Exists(_serverFilePath))
            {

                try
                {
                    FileSystemWatcher systemWatcher = new FileSystemWatcher(_serverFilePath, "*.*");
                    systemWatcher.Path = _serverFilePath;

                    systemWatcher.EnableRaisingEvents = true;
                    systemWatcher.NotifyFilter =
                    NotifyFilters.CreationTime |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size;

                    systemWatcher.Filter = "*.*";

                    systemWatcher.Changed += FileWatcher_DeserializeOnChange;

                    // Debug.Log("COMPLETED");
                }
                catch (IOException ex)
                {
#if UNITY_EDITOR
                    Debug.LogError(ex.Message);
#endif
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError(ex.Message + " " + ex.HelpLink);
#endif
                }
            }
            else
            {
                NotificationHandler.Print("There was an error watching the server messages. ");
            }
        }

        /// <summary>
        /// Update upon the filewatcher seeing a change in the file system
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="e">file system event</param>
        /// <remarks>Does not work with unity runtime</remarks>
        void FileWatcher_DeserializeOnChange(object source, FileSystemEventArgs e)
        {

            Debug.Log("files changed");

            var previousNotifications = GetNotificationsRecieved(WarSystem.CurrentActiveAccount.UserName);

            Debug.Log(previousNotifications.Count);

            try
            {

                bool x = Deserialize();

                if (x)
                {
                    var newNotifications = GetNotificationsRecieved(WarSystem.CurrentActiveAccount.UserName);

                    Debug.Log("Previous " + previousNotifications.Count);
                    Debug.Log("New Notifications " + previousNotifications.Count);

                    if (previousNotifications.Count < newNotifications.Count)
                    {
                        var amt = newNotifications.Count - previousNotifications.Count;
                        NotificationHandler.Print("*You have " + amt + " messages");
                    }
                }
                else
                {
                    Debug.LogError("Could not deserialize the file");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Close the notifications handler - usually called when the 'Quit' is selected by the user
        /// </summary>
        public void Close()
        {
            CreateBackUpFile();
            Serialize();
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        /// <param name="fromUser">from the user</param>
        /// <param name="toUsers">the to users </param>
        /// <param name="title">the title of the message</param>
        /// <param name="message">the message itself</param>
        /// <param name="isTask">does the message contain a task?</param>
        /// <param name="taskComplete">is the task complete?</param>
        /// <param name="messageRead">has the message been marked as read?</param>
        /// <param name="flagged">has the message been flagged?</param>
        /// <param name="wmMessage">war manager instructions if needed</param>
        public void CreateNewNotification(string id, string fromUser, string[] toUsers, string title, string message,
         bool isTask = false, bool taskComplete = false, bool messageRead = false, bool flagged = false, string[] attachedEntries = null, string[] attachedSheets = null, string wmMessage = "")
        {
            Deserialize(ServerPath);

            List<UserNotification> notifications = new List<UserNotification>();

            notifications.AddRange(Notifications);

            UserNotification newNotification = new UserNotification()
            {
                ID = id,
                From = fromUser,
                To = toUsers,
                Title = title,
                Message = message,
                IsTaskMessage = isTask,
                TaskCompleted = taskComplete,
                AttachedEntryIDs = attachedEntries,
                AttachedSheetPoints = attachedSheets,
                WarManagerInstructions = wmMessage
            };

            notifications.Add(newNotification);

            notifications.Sort();

            Notifications = notifications.ToArray();

            WarSystem.WriteToLog("User Notification Created:\n" + newNotification.GetJson(), Logging.MessageType.logEvent);

            Serialize(ServerPath);
        }


        /// <summary>
        /// Create a new notification based of an existing notification
        /// </summary>
        /// <param name="notification"></param>
        private void CreateNewNotification(UserNotification notification)
        {
            CreateNewNotification(notification.ID, notification.From, notification.To, notification.Title,
             notification.Message, notification.IsTaskMessage, notification.TaskCompleted,
             notification.MessageRead, notification.MarkAsFlagged, notification.AttachedEntryIDs, notification.AttachedSheetPoints, notification.WarManagerInstructions);
        }

        /// <summary>
        /// Get notifications that was recieved
        /// </summary>
        /// <param name="toUser"></param>
        /// <returns></returns>
        public List<UserNotification> GetNotificationsRecieved(string toUser)
        {
            List<UserNotification> lst = new List<UserNotification>();

            foreach (var note in Notifications)
            {
                for (int i = 0; i < note.To.Length; i++)
                {
                    if (note.To[i] == toUser)
                        lst.Add(note);
                }
            }

            return lst;
        }

        /// <summary>
        /// Deserialize, do the action and then serialize again
        /// </summary>
        /// <param name="a">the action to do</param>
        /// <returns>returns true if the deserialization and serialization was successful, false if not</returns>
        public bool HandleAction(Action a)
        {

            try
            {
                if (Deserialize())
                {
                    a();

                    return Serialize();
                }
            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Error handling Tasks and Messages " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Handles the process of sending a message or task
        /// </summary>
        /// <param name="newNotification">the notification being sent</param>
        /// <param name="includeMessageBoxes">should message boxes be included or will it be a silent send?</param>
        /// <param name="emailOptions">the options for sending an email to each of the accounts on the sender's list</param>
        /// <param name="callback">call back</param>
        public void SendMessage(UserNotification newNotification, bool includeMessageBoxes, UserNotificationEmailOptions emailOptions = UserNotificationEmailOptions.DoNotSend, Action callback = null)
        {

            if (newNotification == null)
            {
                throw new NullReferenceException("The user task or message cannot be null");
            }

            string boxHandlerMessage = CreateSendMessage(newNotification);

            if (includeMessageBoxes)
            {
                if (newNotification.To == null || newNotification.To.Length < 1)
                {
                    MessageBoxHandler.Print_Immediate("You have not set who you are sending it to.", "Error");
                    return;
                }

                MessageBoxHandler.Print_Immediate(boxHandlerMessage, "Question", (x) =>
                {
                    if (x)
                    {
                        FinalizeSendTaskOrMessage(newNotification, emailOptions, callback);
                    }
                });
            }
            else
            {
                if (newNotification.To == null || newNotification.To.Length < 1)
                {
                    newNotification.To = new string[1] { newNotification.From };
                }

                FinalizeSendTaskOrMessage(newNotification, emailOptions, callback);
            }
        }

        /// <summary>
        /// Create the send message
        /// </summary>
        /// <param name="newNotification">the new user notification</param>
        /// <returns>returns the box handler message</returns>
        private string CreateSendMessage(UserNotification newNotification)
        {

            string boxHandlerMessage = "Are you sure you want to send \"" + newNotification.Title + "\"?";

            if (newNotification.Title.Trim() == "New Message" || newNotification.Title.Trim() == "")
            {
                boxHandlerMessage = "\nThe task or message does not have a proper subject. Do you still want to send?";
            }

            if (newNotification.Message.ToLower().Contains("attached") && newNotification.AttachedEntryIDs.Length < 1 && newNotification.AttachedSheetPoints.Length < 1)
            {
                boxHandlerMessage = "There are no attachments with your task or message. Do you still want to send?";
            }

            return boxHandlerMessage;
        }

        /// <summary>
        /// handle the email
        /// </summary>
        /// <param name="newNotification">the new user notification</param>
        /// <param name="emailOptions">the email options for the user new user notification</param>
        private void HandleEmail(UserNotification newNotification, UserNotificationEmailOptions emailOptions)
        {
            if (emailOptions == UserNotificationEmailOptions.DoNotSend)
                return;

            if (emailOptions == UserNotificationEmailOptions.Ask)
            {
                MessageBoxHandler.Print_Immediate("Would you like to also email the task or message?", "Question", (x) =>
                {
                    if (x)
                    {
                        SendEmailToUsers(newNotification);
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Email Sent", "Note");
                        });
                    }
                });
            }
            else if (emailOptions == UserNotificationEmailOptions.Send)
            {
                SendEmailToUsers(newNotification);
            }
        }

        /// <summary>
        /// Send the email to users
        /// </summary>
        /// <param name="newNotification">the user notification</param>
        private void SendEmailToUsers(UserNotification newNotification)
        {
            for (int i = 0; i < newNotification.To.Length; i++)
            {
                EmailClient.SendQuickEmail(newNotification.To[i], newNotification.From + " - " + newNotification.Title, "From:\n" + newNotification.From + "\n\nMessage:\n" + newNotification.Message + "\n\n" + newNotification.GetSenderUserAccount().PermissionsName);
            }
        }

        /// <summary>
        /// Wrap up any last issues and send the message
        /// </summary>
        /// <param name="newNotification">the new user notification</param>
        /// <param name="emailOptions">the email options</param>
        /// <param name="callback">the UI call back</param>
        private void FinalizeSendTaskOrMessage(UserNotification newNotification, UserNotificationEmailOptions emailOptions, Action callback)
        {
            newNotification.ID = Guid.NewGuid().ToString();

            WarSystem.UserMessageNotificationsHandler.CreateNewNotification(newNotification);
            WarSystem.UserMessageNotificationsHandler.CreateBackUpFile();

            LeanTween.delayedCall(1, () =>
            {
                callback();

                LeanTween.delayedCall(1, () =>
                {
                    HandleEmail(newNotification, emailOptions);
                });
            });
        }

        /// <summary>
        /// Toggle the notification as a task
        /// </summary>
        /// <param name="notification">the user notification to toggle as a task</param>
        public void ToggleNotificationTask(UserNotification notification)
        {
            if (Deserialize())
            {

                foreach (var x in Notifications)
                {

                    if (x.ID == notification.ID)
                    {
                        x.TaskCompleted = !x.TaskCompleted;

                        Debug.Log("task toggled " + x.TaskCompleted);

                        return;
                    }
                }

                Serialize();
            }
        }

        /// <summary>
        /// Get notifications that was sent
        /// </summary>
        /// <param name="fromUser">the from user</param>
        /// <returns>returns a list of user notifications</returns>
        public List<UserNotification> GetNotificationsSent(string fromUser)
        {
            List<UserNotification> lst = new List<UserNotification>();

            foreach (var note in Notifications)
            {
                if (note.From == fromUser)
                {
                    lst.Add(note);
                }
            }

            return lst;
        }


        /// <summary>
        /// Get Tasks that have not been completed yet
        /// </summary>
        /// <param name="user">the user to search from</param>
        /// <returns>returns a list of incompleted tasks</returns>
        public List<UserNotification> GetIncompletedTasks(string user)
        {
            List<UserNotification> lst = new List<UserNotification>();

            foreach (var note in Notifications)
            {
                if (note.IsTaskMessage && !note.TaskCompleted)
                {
                    if (note.From == user)
                    {
                        lst.Add(note);
                    }
                    else
                    {
                        for (int i = 0; i < note.To.Length; i++)
                        {
                            if (note.To[i] == user)
                                lst.Add(note);
                        }
                    }
                }
            }

            return lst;
        }


        /// <summary>
        /// Get tasks that have been completed
        /// </summary>
        /// <param name="user">the user to search from</param>
        /// <returns>returns the list of of user notifications</returns>
        public List<UserNotification> GetCompletedTasks(string user)
        {
            List<UserNotification> lst = new List<UserNotification>();

            foreach (var note in Notifications)
            {
                if (note.IsTaskMessage && note.TaskCompleted)
                {
                    if (note.From == user)
                    {
                        lst.Add(note);
                    }
                    else
                    {
                        for (int i = 0; i < note.To.Length; i++)
                        {
                            if (note.To[i] == user)
                                lst.Add(note);
                        }
                    }
                }
            }

            return lst;
        }

        /// <summary>
        /// Get messages that are not tasks
        /// </summary>
        /// <param name="user">the user name to search for</param>
        /// <returns>returns a list of user notifications</returns>
        public List<UserNotification> GetNonTaskMessages(string user)
        {
            List<UserNotification> lst = new List<UserNotification>();

            foreach (var note in Notifications)
            {
                if (!note.IsTaskMessage)
                {
                    if (note.From == user)
                    {
                        lst.Add(note);
                    }
                    else
                    {
                        for (int i = 0; i < note.To.Length; i++)
                        {
                            if (note.To[i] == user)
                                lst.Add(note);
                        }
                    }
                }
            }

            return lst;
        }


        /// <summary>
        /// Remove the notification from the list
        /// </summary>
        /// <param name="id">the id of the notification</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool RemoveNotification(UserNotification notification)
        {

            if (Deserialize())
            {
                List<UserNotification> notifications = new List<UserNotification>();
                notifications.AddRange(Notifications);


                int location = -1;
                int i = 0;

                while (i < notifications.Count && location < 0)
                {

                    if (notifications[i] != null && notifications[i].ID == notification.ID)
                    {
                        location = i;
                        // Debug.Log("Found notification: " + location);
                    }

                    // Debug.Log(notifications[i].ID);

                    i++;
                }

                if (location >= 0)
                {
                    string json = notifications[location].GetJson();

                    notifications.RemoveAt(location);

                    Notifications = notifications.ToArray();

                    WarSystem.WriteToLog("Removed notification\n" + json, Logging.MessageType.logEvent);

                    bool removed = Serialize();

                    // Debug.Log("removed: " + removed);
                    return removed;
                }
                else
                {
                    NotificationHandler.Print("Could not find message with the selected guid " + notification.GetJson());
                    return false;
                }
            }

            NotificationHandler.Print("Could not refresh messages list" + notification.GetJson());
            return false;
        }

        /// <summary>
        /// Get a copy of all the messages in the server in a JSON format
        /// </summary>
        /// <returns>returns a string</returns>
        public string GetAllMessagesJSON()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            try
            {
                string str = JsonSerializer.Serialize<UserPersistantNotificationsHandler>(this, options);
                return str;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            return "";
        }

        /// <summary>
        /// Serialze the user notifications to the server
        /// </summary>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool Serialize()
        {
            return Serialize(ServerPath);
        }


        /// <summary>
        /// Save the user notifications to a specific location
        /// </summary>
        /// <param name="location">the file location of the notifications</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool Serialize(string location)
        {
            try
            {
                if (_deserializationError)
                    return false;

                if (Notifications == null)
                    Notifications = new UserNotification[0];

                File.WriteAllText(location, GetAllMessagesJSON());
                return true;
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// Deserialize the server notifications
        /// </summary>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool Deserialize()
        {
            return Deserialize(ServerPath);
        }

        /// <summary>
        /// Saves the messages file to a backup location incase of an internal error
        /// </summary>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool CreateBackUpFile()
        {
            if (_deserializationError)
                return false;

            if (Deserialize())
            {
                return Serialize(_serverBackUpPath + " " + Guid.NewGuid().ToString().Substring(0, 5) + ".json");
            }

            return false;
        }


        /// <summary>
        /// Deserialize notifications from a certian location
        /// </summary>
        /// <param name="path">the path that the json file is located</param>
        /// <returns>returns true if the deserialization was successful, false if not</returns>
        public bool Deserialize(string path)
        {
            if (!File.Exists(path))
            {
                if (!Directory.Exists(path))
                {
                    NotificationHandler.Print("Cannot find location to get user notifications " + path);
                    _deserializationError = true;
                    return false;
                }
                else
                {
                    File.Create(path);
                    Notifications = new UserNotification[0];
                    return true;
                }
            }

            try
            {
                string str = File.ReadAllText(path);

                if (string.IsNullOrEmpty(str)) // empty file
                {
                    Notifications = new UserNotification[0];
                    return true;
                }

                var newInstance = JsonSerializer.Deserialize<UserPersistantNotificationsHandler>(str);

                Notifications = newInstance.GetAllUserNotifications();

                _deserializationError = false;
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
                _deserializationError = true;
                Notifications = new UserNotification[0];
                WarSystem.WriteToLog("Error attempting to get user notifications:\n" + ex.Message, Logging.MessageType.error);
            }

            return true;
        }

        /// <summary>
        /// Get a specific user notification by id
        /// </summary>
        /// <param name="id">the id of the user notification</param>
        /// <returns>returns the user notification</returns>
        public UserNotification GetUserNotification(string id)
        {
            foreach (var x in Notifications)
            {
                if (x.ID == id)
                {
                    return x;
                }
            }

            return null;
        }

        /// <summary>
        /// Get an array of all the current user notifications
        /// </summary>
        /// <returns>returns the array of user notifications</returns>
        public UserNotification[] GetAllUserNotifications()
        {
            if (Notifications == null || Notifications == new UserNotification[0])
                return new UserNotification[0];

            UserNotification[] notification = new UserNotification[Notifications.Length];
            Array.Copy(Notifications, notification, Notifications.Length);

            return notification;
        }

    }

    /// <summary>
    /// handles email proceedures when sending a message
    /// </summary>
    public enum UserNotificationEmailOptions
    {
        DoNotSend,
        Send,
        Ask,
    }
}
