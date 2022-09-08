
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager.Unity3D.Windows;
using WarManager.Sharing;

using WarManager.Backend;


namespace WarManager.Unity3D
{

    public class NotificationsUIController : MonoBehaviour
    {

        [SerializeField] Sprite NewMessageSprite;
        [SerializeField] Sprite MessageSprite;
        [SerializeField] Sprite TaskNotCompletedSprite;
        [SerializeField] Sprite TaskCompletedSprite;
        [SerializeField] Sprite MessageContainsAttachments;
        [SerializeField] Sprite Reply;
        [SerializeField] Sprite Inbox;
        [SerializeField] Sprite Tasks;
        [SerializeField] Sprite AttachSprite;
        [SerializeField] Sprite SendMessageSprite;
        [SerializeField] Sprite DeleteSprite;
        [SerializeField] Sprite BackSprite;

        private List<DataEntry> _attachedEntries = new List<DataEntry>();
        private bool SortByTasks = true;

        public void Start()
        {
            LeanTween.delayedCall(0, () => ShowAllNotifications(0, false));
        }

        public void ShowAllNotifications()
        {
            ShowAllNotifications(0);
        }

        public void ShowAllNotifications(float callTime, bool forceOpen = true)
        {
            if (WarSystem.UserMessageNotificationsHandler == null)
                return;


            LeanTween.delayedCall(callTime, () =>
            {

                if (WarSystem.UserMessageNotificationsHandler.Deserialize())
                {

                    // WarSystem.UserMessageNotificationsHandler.Deserialize();
                    // WarSystem.UserMessageNotificationsHandler.CreateNewNotification("taylor.howell@jrcousa.com", "taylor.howell@jrcousa.com", "test", "test");
                    // WarSystem.UserMessageNotificationsHandler.Serialize();

                    List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();


                    content.Add(new SlideWindow_Element_ContentInfo(50));

                    UserNotification newNotification = new UserNotification()
                    {
                        From = WarSystem.CurrentActiveAccount.UserName,
                        Title = "New Message",
                    };

                    content.Add(new SlideWindow_Element_ContentInfo("Create New Task or Message", (x) => { CreateMessage(ShowAllNotifications, newNotification); }, NewMessageSprite));

                    if (!SortByTasks)
                    {
                        content.Add(new SlideWindow_Element_ContentInfo("Sort By Tasks", (x) => { SortByTasks = true; ShowAllNotifications(); }, Tasks));


                        content.Add(new SlideWindow_Element_ContentInfo("Recieved", 20));

                        var recievedNotifications = WarSystem.UserMessageNotificationsHandler.GetNotificationsRecieved(WarSystem.CurrentActiveAccount.UserName);

                        if (recievedNotifications == null || recievedNotifications.Count < 1)
                        {
                            content.Add(new SlideWindow_Element_ContentInfo("", "No Received Tasks or Messages"));
                        }

                        foreach (var x in recievedNotifications)
                        {
                            Sprite messge = MessageSprite;

                            if (x.IsTaskMessage)
                            {
                                if (x.TaskCompleted)
                                {
                                    messge = TaskCompletedSprite;
                                }
                                else
                                {
                                    messge = TaskNotCompletedSprite;
                                }
                            }
                            else
                            {
                                if ((x.AttachedSheetPoints != null && x.AttachedSheetPoints.Length > 0) || (x.AttachedEntryIDs != null && x.AttachedEntryIDs.Length > 0))
                                {
                                    messge = MessageContainsAttachments;
                                }
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(x.Title, (z) => { ViewMessage(ShowAllNotifications, x); }, messge));
                        }

                        content.Add(new SlideWindow_Element_ContentInfo("Sent", 20));

                        var sentNotifications = WarSystem.UserMessageNotificationsHandler.GetNotificationsSent(WarSystem.CurrentActiveAccount.UserName);

                        if (sentNotifications == null || sentNotifications.Count < 1)
                        {
                            content.Add(new SlideWindow_Element_ContentInfo("", "No Sent Tasks or Messages"));
                        }

                        foreach (var y in sentNotifications)
                        {
                            Sprite messge = MessageSprite;

                            if (y.IsTaskMessage)
                            {
                                if (y.TaskCompleted)
                                {
                                    messge = TaskCompletedSprite;
                                }
                                else
                                {
                                    messge = TaskNotCompletedSprite;
                                }
                            }
                            else
                            {
                                if ((y.AttachedSheetPoints != null && y.AttachedSheetPoints.Length > 0) || (y.AttachedEntryIDs != null && y.AttachedEntryIDs.Length > 0))
                                {
                                    messge = MessageContainsAttachments;
                                }
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(y.Title, (x) => { ViewMessage(ShowAllNotifications, y); }, messge));
                        }
                    }
                    else
                    {
                        content.Add(new SlideWindow_Element_ContentInfo("Sort By Sent and Recieved", (x) => { SortByTasks = false; ShowAllNotifications(); }, Inbox));

                        content.Add(new SlideWindow_Element_ContentInfo("Not Completed", 20));

                        var recievedNotifications = WarSystem.UserMessageNotificationsHandler.GetIncompletedTasks(WarSystem.CurrentActiveAccount.UserName);

                        if (recievedNotifications == null || recievedNotifications.Count < 1)
                        {
                            content.Add(new SlideWindow_Element_ContentInfo("", "No Incomplete Tasks"));
                        }

                        foreach (var x in recievedNotifications)
                        {
                            Sprite messge = MessageSprite;

                            if (x.IsTaskMessage)
                            {
                                if (x.TaskCompleted)
                                {
                                    messge = TaskCompletedSprite;
                                }
                                else
                                {
                                    messge = TaskNotCompletedSprite;
                                }
                            }
                            else
                            {
                                if ((x.AttachedSheetPoints != null && x.AttachedSheetPoints.Length > 0) || (x.AttachedEntryIDs != null && x.AttachedEntryIDs.Length > 0))
                                {
                                    messge = MessageContainsAttachments;
                                }
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(x.Title, (z) => { ViewMessage(ShowAllNotifications, x); }, messge));
                        }

                        content.Add(new SlideWindow_Element_ContentInfo("Completed", 20));

                        var sentNotifications = WarSystem.UserMessageNotificationsHandler.GetCompletedTasks(WarSystem.CurrentActiveAccount.UserName);

                        if (sentNotifications == null || sentNotifications.Count < 1)
                        {
                            content.Add(new SlideWindow_Element_ContentInfo("", "No Completed Tasks"));
                        }

                        foreach (var y in sentNotifications)
                        {
                            Sprite messge = MessageSprite;

                            if (y.IsTaskMessage)
                            {
                                if (y.TaskCompleted)
                                {
                                    messge = TaskCompletedSprite;
                                }
                                else
                                {
                                    messge = TaskNotCompletedSprite;
                                }
                            }
                            else
                            {
                                if ((y.AttachedSheetPoints != null && y.AttachedSheetPoints.Length > 0) || (y.AttachedEntryIDs != null && y.AttachedEntryIDs.Length > 0))
                                {
                                    messge = MessageContainsAttachments;
                                }
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(y.Title, (x) => { ViewMessage(ShowAllNotifications, y); }, messge));
                        }

                        content.Add(new SlideWindow_Element_ContentInfo(20));
                        content.Add(new SlideWindow_Element_ContentInfo("Non-Task Messages", 20));

                        var nonTaskMessages = WarSystem.UserMessageNotificationsHandler.GetNonTaskMessages(WarSystem.CurrentActiveAccount.UserName);

                        if (nonTaskMessages == null || nonTaskMessages.Count < 1)
                        {
                            content.Add(new SlideWindow_Element_ContentInfo("", "No Messages"));
                        }

                        foreach (var y in nonTaskMessages)
                        {
                            Sprite messge = MessageSprite;

                            if (y.IsTaskMessage)
                            {
                                if (y.TaskCompleted)
                                {
                                    messge = TaskCompletedSprite;
                                }
                                else
                                {
                                    messge = TaskNotCompletedSprite;
                                }
                            }
                            else
                            {
                                if ((y.AttachedSheetPoints != null && y.AttachedSheetPoints.Length > 0) || (y.AttachedEntryIDs != null && y.AttachedEntryIDs.Length > 0))
                                {
                                    messge = MessageContainsAttachments;
                                }
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(y.Title, (x) => { ViewMessage(ShowAllNotifications, y); }, messge));
                        }
                    }

                    SlideWindowsManager.main.AddTasksAndMessagesContent(content, forceOpen);
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Could not get Messages.", "Oops!");
                }
            });
        }

        public void ViewMessage(Action previousCall, UserNotification selectedNotification)
        {
            var newNotification = WarSystem.UserMessageNotificationsHandler.GetUserNotification(selectedNotification.ID);
            selectedNotification = newNotification;

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(50));
            content.Add(new SlideWindow_Element_ContentInfo("Back", (x) => { ShowAllNotifications(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));


            if (WarSystem.CurrentActiveAccount.Permissions.ContainsKeywordPermission("Developer"))
            {
                content.Add(new SlideWindow_Element_ContentInfo("ID", selectedNotification.ID));
            }

            content.Add(new SlideWindow_Element_ContentInfo("From", selectedNotification.From));

            content.Add(new SlideWindow_Element_ContentInfo("To", string.Join(",", selectedNotification.To)));

            content.Add(new SlideWindow_Element_ContentInfo("Message Back", (x) =>
            {
                UserNotification notification = new UserNotification()
                {
                    From = WarSystem.CurrentActiveAccount.UserName,
                    To = selectedNotification.To
                };

                CreateMessage(() => { ViewMessage(previousCall, selectedNotification); }, notification);
            }, Reply));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo(selectedNotification.Title, selectedNotification.Message, 400));
            content.Add(new SlideWindow_Element_ContentInfo(20));


            if (selectedNotification.IsTaskMessage)
            {
                string completedTaskMessage = "Not Completed";

                if (selectedNotification.TaskCompleted)
                {
                    completedTaskMessage = "Completed";
                }

                content.Add(new SlideWindow_Element_ContentInfo("Task Completed ", completedTaskMessage));
                content.Add(new SlideWindow_Element_ContentInfo("Toggle Task Completed", (x) =>
                {
                    if (!WarSystem.UserMessageNotificationsHandler.HandleAction(() => WarSystem.UserMessageNotificationsHandler.ToggleNotificationTask(selectedNotification)))
                    {
                        MessageBoxHandler.Print_Immediate("Something went wrong", "Error");
                    }
                    ViewMessage(previousCall, selectedNotification);
                }));

                content.Add(new SlideWindow_Element_ContentInfo(20));
            }
            else
            {
                //content.Add(new SlideWindow_Element_ContentInfo("Task Completed", "(Not a task message)"));
            }


            if (selectedNotification.AttachedEntryIDs != null && selectedNotification.AttachedEntryIDs.Length > 0)
            {

                content.Add(new SlideWindow_Element_ContentInfo("Attached Entries", 30));
                var entries = selectedNotification.GetAttachedDataEntries();

                foreach (var entry in entries)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(entry, (x) =>
                    {
                        DataSetViewer.main.ShowDataEntryInfo(entry, () => { ViewMessage(previousCall, selectedNotification); }, selectedNotification.Title, true);
                    }));
                }
            }

            if (selectedNotification.AttachedSheetPoints != null)
            {
                if (selectedNotification.AttachedSheetPoints.Length > 0)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(20));

                    content.Add(new SlideWindow_Element_ContentInfo("Attached Pins", 30));
                }
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo("Delete Message", (x) =>
           {
               MessageBoxHandler.Print_Immediate("Would you like to delete \'" + selectedNotification.Title + "\'?", "Question", (x) =>
               {
                   if (x)
                   {
                       if (WarSystem.UserMessageNotificationsHandler.RemoveNotification(selectedNotification))
                       {
                           LeanTween.delayedCall(1, () =>
                           {
                               MessageBoxHandler.Print_Immediate("Message deleted", "Note");
                               previousCall();
                           });
                       }
                       else
                       {
                           LeanTween.delayedCall(1, () =>
                           {
                               MessageBoxHandler.Print_Immediate("Something went wrong", "Error");
                           });
                       }
                   }
               });
           }, DeleteSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            SlideWindowsManager.main.AddTasksAndMessagesContent(content, true);
        }

        public void CreateMessage(Action callback, UserNotification newNotification)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo("New Message", null));
            content.Add(new SlideWindow_Element_ContentInfo("Back", (x) => { callback(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));


            content.Add(new SlideWindow_Element_ContentInfo("From", newNotification.From));
            content.Add(new SlideWindow_Element_ContentInfo("To", string.Join(", ", newNotification.To)));
            content.Add(new SlideWindow_Element_ContentInfo("Change who receives the message", (x) => { SelectToAccounts(() => CreateMessage(callback, newNotification), newNotification); }));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo(newNotification.Title, newNotification.Message, 200));

            content.Add(new SlideWindow_Element_ContentInfo("Edit", 20));

            content.Add(new SlideWindow_Element_ContentInfo("Subject", newNotification.Title, (x) =>
            {
                newNotification.Title = x;
                CreateMessage(callback, newNotification);
            }));
            content.Add(new SlideWindow_Element_ContentInfo("Message", newNotification.Message, (x) =>
            {
                newNotification.Message = x;
                CreateMessage(callback, newNotification);
            }));

            content.Add(new SlideWindow_Element_ContentInfo(20));



            string str = "NOT a Task Message";

            if (newNotification.IsTaskMessage)
            {
                str = "Message is a Task Message";
                content.Add(new SlideWindow_Element_ContentInfo("", "Task Not Completed"));
            }

            content.Add(new SlideWindow_Element_ContentInfo(str, (x) =>
           {
               newNotification.ToggleMessageAsTask();
               CreateMessage(callback, newNotification);
           }));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            if (newNotification.AttachedEntryIDs.Length > 0)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Attached Entries (click to remove)", 20));
                DataEntry[] entries = newNotification.GetAttachedDataEntries();

                foreach (var x in entries)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(x, (y) =>
                    {
                        newNotification.RemoveEntry(x.DataSet.ID, x.RowID);
                        CreateMessage(callback, newNotification);
                    }));
                }
            }
            else
            {
                content.Add(new SlideWindow_Element_ContentInfo("Attached Entries", 20));
                content.Add(new SlideWindow_Element_ContentInfo("", "No attached entries"));
            }

            content.Add(new SlideWindow_Element_ContentInfo("Attach entries...", (x) => { AttachEntry(() => CreateMessage(callback, newNotification), newNotification); }, AttachSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Attached Sheets", 20));
            content.Add(new SlideWindow_Element_ContentInfo("", "No attached sheets"));
            content.Add(new SlideWindow_Element_ContentInfo("Attach sheets...", (x) => { MessageBoxHandler.Print_Immediate("This feature is coming soon. Contact support for more information.", "Note"); }, AttachSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo("Send Message", (x) =>
            {
                WarSystem.UserMessageNotificationsHandler.SendMessage(newNotification, true, UserNotificationEmailOptions.Ask, callback);
            }, SendMessageSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            SlideWindowsManager.main.AddTasksAndMessagesContent(content, true);
        }


        /// <summary>
        /// Attach a Data Entry to the list
        /// </summary>
        /// <param name="previousCall"></param>
        /// <param name="notification"></param>
        private void AttachEntry(Action previousCall, UserNotification notification)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(50));
            content.Add(new SlideWindow_Element_ContentInfo("Back", (x) => { previousCall(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));


            foreach (var set in WarSystem.DataSetManager.Datasets)
            {
                try
                {
                    content.Add(new SlideWindow_Element_ContentInfo(set.DatasetName, (x) =>
                    {
                        SelectDataEntry(() => { AttachEntry(previousCall, notification); }, previousCall, set, notification);
                    }));
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print(ex.Message);
                }
            }

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Select a Data Entry
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="next"></param>
        /// <param name="set"></param>
        /// <param name="notification"></param>
        private void SelectDataEntry(Action cancel, Action next, DataSet set, UserNotification notification)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(50));
            content.Add(new SlideWindow_Element_ContentInfo("Back", (x) => { cancel(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            foreach (var entry in set.Entries)
            {
                try
                {
                    content.Add(new SlideWindow_Element_ContentInfo(entry, (x) =>
                    {
                        notification.AddEntry(entry.DataSet.ID, entry.RowID);
                        ActiveSheetsDisplayer.main.ViewReferences();
                        next();
                    }));
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print(ex.Message);
                }
            }

            SlideWindowsManager.main.AddReferenceContent(content);
        }

        /// <summary>
        /// Select the 'to' accounts
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="selectedNotification"></param>
        private void SelectToAccounts(Action callback, UserNotification selectedNotification)
        {
            List<Account> accounts = Account.GetAccounts();


            List<string> selectedUserNames = new List<string>();
            List<string> unSelectedUserNames = new List<string>();

            for (int i = 0; i < selectedNotification.To.Length; i++)
            {
                for (int j = accounts.Count - 1; j >= 0; j--)
                {
                    if (selectedNotification.To[i] == accounts[j].UserName)
                    {
                        accounts.RemoveAt(j);
                    }
                }
            }

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo("To", null));
            content.Add(new SlideWindow_Element_ContentInfo("Back", (x) => { callback(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            if (accounts.Count < 1 && selectedNotification.To.Length < 1)
            {
                content.Add(new SlideWindow_Element_ContentInfo("No accounts to add or remove", 10));
            }

            if (accounts.Count > 0)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Accounts to Add", 20));
                foreach (var acct in accounts)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Select " + acct.UserName, (x) =>
                    {
                        selectedNotification.AddToUsersToSend(acct.UserName);
                        SelectToAccounts(callback, selectedNotification);
                    }));
                }
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));
            if (selectedNotification.To.Length > 0)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Accounts Added", 20));

                selectedUserNames.AddRange(selectedNotification.To);

                foreach (var usrName in selectedUserNames)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Remove " + usrName, (x) =>
                    {
                        selectedNotification.RemoveFromUsersToSend(usrName);
                        SelectToAccounts(callback, selectedNotification);
                    }));
                }
            }

            SlideWindowsManager.main.AddTasksAndMessagesContent(content, true);
        }

        /// <summary>
        /// refresh upon change in connection
        /// </summary>
        /// <param name="connected"></param>
        private void Refresh(bool connected)
        {
            if (connected)
            {
                ShowAllNotifications(0, false);
            }
        }

        private void OnInit()
        {
            ShowAllNotifications(0, false);
        }

        public void OnEnable()
        {
            //WarSystem.OnInit += OnInit;
            WarSystem.OnConnectionToServerChanged += Refresh;
        }

        public void OnDisable()
        {
            //WarSystem.OnInit -= OnInit;
            WarSystem.OnConnectionToServerChanged -= Refresh;
        }

    }


}
