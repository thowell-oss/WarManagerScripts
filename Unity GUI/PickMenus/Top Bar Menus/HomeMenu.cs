

using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager;
using WarManager.Cards;
using WarManager.Unity3D.Windows;
using WarManager.Backend;
using WarManager.Sharing;
using StringUtility;

namespace WarManager.Unity3D.PickMenu
{
    [Notes.Author("Handles commands form menus on the top bar")]
    public class HomeMenu : MonoBehaviour
    {
        [SerializeField]
        private Vector2 spawnOffsetLeft = new Vector2(-15, -20);

        [SerializeField]
        private Vector2 spawnOffsetRight = new Vector2(15, -20);

        [SerializeField]
        ActiveSheetsDisplayer _sheetsDisplayer;

        [SerializeField]
        SaveButtonManager _saveCommands;

        [SerializeField]
        SheetsCommands _sheetCommands;

        [SerializeField]
        CopyPasteDuplicate _cardCommands;

        [SerializeField]
        SimpleUndoRedoManager _undoRedoManager;

        [SerializeField]
        SheetsCardSelectionManager _sheetsCardSelectionManager;

        [SerializeField]
        WindowController _settingsWindow;

        [SerializeField]
        NotificationsUIController _notificationsUIController;

        [SerializeField]
        DropMenu _searchMenu;


        private bool IsNotHomeSheetCurrent
        {
            get
            {
                return WarManager.Backend.SheetsManager.CurrentSheetID != WarManager.Backend.SheetsManager.HomeSheetID;
            }
        }

        /// <summary>
        /// Home Menu
        /// </summary>
        /// <param name="obj"></param>
        public void ActivateHomeMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> homeButtons = new List<(string, Action actions, bool active)>();

            homeButtons.Add(("New Sheet", _sheetsDisplayer.NewSheet, true));
            homeButtons.Add(("Home Sheet", _sheetsDisplayer.OpenHomeSheet, IsNotHomeSheetCurrent));
            homeButtons.Add(("References...", () =>
            {
                _sheetsDisplayer.ViewReferences();
                SlideWindowsManager.main.OpenReference(false);

            }, true));

            homeButtons.Add(("", () => { }, false));

            homeButtons.Add(("My Account Settings", () => ActiveSheetsDisplayer.main.ShowAccountInfo(true), true));
            // currentSheetButtons.Add(("Preferences", () => _settingsWindow.ActivateWindow(true), true));

            AccountManagementUI accountManagementUI = new AccountManagementUI();

            if (WarSystem.CurrentActiveAccount.IsAdmin)
                homeButtons.Add(("Edit Accounts <size=75%>(Admin)", () => { accountManagementUI.ShowAllAccounts(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));
            else
                homeButtons.Add(("View Accounts", () => { accountManagementUI.ShowAllAccounts(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));


            if (WarSystem.CurrentActiveAccount.IsAdmin)
                homeButtons.Add(("Edit Permissions <size=75%>(Admin)", () => { ActiveSheetsDisplayer.main.ViewPermissions(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));
            else
                homeButtons.Add(("View Permissions", () => { ActiveSheetsDisplayer.main.ViewPermissions(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));

            homeButtons.Add(("View Logs <size=75%>(Admin)", () =>
            {
                if (WarSystem.CurrentActiveAccount.IsAdmin)
                {
                    string filePath = GeneralSettings.Save_Location_Server + @"\Data\War System\Logs";
                    MessageBoxHandler.Print_Immediate("Taking you to " + filePath, "Note", (x) =>
                    {
                        if (x)
                        {
                            Application.OpenURL(filePath);
                        }
                    });
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Cannot access the logs because you are not an administrator", "Nope");
                }

            }, WarSystem.CurrentActiveAccount.IsAdmin));

            homeButtons.Add(("", () => { }, false));

            homeButtons.Add(("Save <size=75%>(ctrl + s)", _saveCommands.SaveSheets, true));
            homeButtons.Add(("Quit <size=75%>(ctrl + q)", _sheetCommands.QuitSession, true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetLeft;
            PickMenu.PickMenuManger.main.OpenPickMenu(homeButtons, finalLocation);
        }

        /// <summary>
        /// Activate the view menu
        /// </summary>
        /// <param name="obj">the location that the pick menu should appear</param>
        public void ActivateViewMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> viewButtons = new List<(string, Action actions, bool active)>();

            Action a = () =>
            {
                if (!_searchMenu.ActiveMenu)
                    _searchMenu.ToggleActive();
            };



            viewButtons.Add(("Search", a, true));


            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {

                var cards = new List<Card>();
                cards.AddRange(_sheetsCardSelectionManager.Cards);


                viewButtons.Add(("See More...", () =>
                                {
                                    try
                                    {
                                        DataSetViewer.main.ShowDataEntryInfo(cards[0].Entry, () =>
                                        {
                                            if (cards[0] != null)
                                                DataSetViewer.main.ShowDataSet(cards[0].Entry.DataSet.ID, ActiveSheetsDisplayer.main.ViewReferences);
                                        }, cards[0].Entry.DataSet.DatasetName.SetStringQuotes());
                                    }
                                    catch (System.Exception ex)
                                    {
                                        MessageBoxHandler.Print_Immediate("Could not show card details " + ex.Message, "Error");
                                    }

                                }, _sheetsCardSelectionManager.CardTotal == 1));

                viewButtons.Add(("Views...", () =>
                    {

                        try
                        {
                            DataSetViewer.main.ShowViews(() =>
                            {
                                DataSetViewer.main.ShowDataEntryInfo(cards[0].Entry, () =>
                                {
                                    if (cards[0] != null)
                                        DataSetViewer.main.ShowDataSet(cards[0].Entry.DataSet.ID, ActiveSheetsDisplayer.main.ViewReferences);
                                }, cards[0].Entry.DataSet.DatasetName.SetStringQuotes());
                            }, cards[0].DataSet.DatasetName.SetStringQuotes() + " properties", cards[0].DataSet);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBoxHandler.Print_Immediate("Could not show card details " + ex.Message, "Error");
                        }


                    }, _sheetsCardSelectionManager.CardTotal == 1));


                viewButtons.Add(("Guides", () => { SheetCardClusterContextHandler.Main.ToggleActivateCluster(); }, sheet.Persistent));
                viewButtons.Add(("Data Set Colors", () =>
                {
                    SheetsManager.ShowDataSetColorBars = !SheetsManager.ShowDataSetColorBars;
                }, true));

                // viewButtons.Add(("Toggle On-Screen Shortcuts", () =>
                // {
                //     OnScreenInputGUI.main.ToggleGUI();
                // }, true));

            }

            viewButtons.Add(("Refresh Sheet", () =>
            {
                SheetsManager.ReloadCurrentSheet();
            }, true));

            viewButtons.Add(("Reload Data", () =>
            {
                WarSystem.RefreshLoadedDataSets();
            }, true));

            viewButtons.Add(("Toggle Full Screen <size=75%>(F11)", () =>
            {
                Screen.fullScreen = !Screen.fullScreen;

                if (Screen.fullScreen)
                {
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                else
                {
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }

#if UNITY_EDITOR
                Debug.Log("Full Screen toggled to " + Screen.fullScreen);
#endif
            }, true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetLeft;
            PickMenu.PickMenuManger.main.OpenPickMenu(viewButtons, finalLocation);
        }

        /// <summary>
        /// Edit Menu
        /// </summary>
        /// <param name="obj"></param>
        public void ActivateEditMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> editButtons = new List<(string, Action actions, bool active)>();

            editButtons.Add(("Undo <size=75%>(ctrl + z)", _undoRedoManager.Undo, true));
            editButtons.Add(("Redo <size=75%>(ctrl + y)", _undoRedoManager.Redo, true));
            editButtons.Add(("Copy <size=75%>(ctrl + c)", _cardCommands.CopySelectedCards, true));
            editButtons.Add(("Paste <size=75%>(ctrl + v)", _cardCommands.Paste, true));
            editButtons.Add(("Duplicate <size=75%>(ctrl + d)", _cardCommands.Duplicate, true));
            editButtons.Add(("Remove <size=75%>(ctrl + del)", _cardCommands.Remove, true));
            editButtons.Add(("Lock <size=75%>(ctrl + l)", () => { _cardCommands.Lock(true); }, true));
            editButtons.Add(("Unlock", () => { _cardCommands.Lock(false); }, true));
            editButtons.Add(("Select All <size=75%>(ctrl + a)", () => { _cardCommands.SelectAll(true); }, true));
            editButtons.Add(("Deselect All <size=75%>(ctrl + a)", () => { _cardCommands.SelectAll(false); }, true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetLeft;
            PickMenu.PickMenuManger.main.OpenPickMenu(editButtons, finalLocation);
        }

        public void ActivateShareMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> shareButtons = new List<(string, Action actions, bool active)>();




            shareButtons.Add(("Create Schedule", () =>
            {

                SubmitDocToServer doc = new SubmitDocToServer();

                var selected = CardUtility.GetSelectedCardsOnCurrentSheet();

                if (selected == null)
                {
                    selected = CardUtility.SelectAllCards();
                }

                if (selected == null)
                {
                    MessageBoxHandler.Print_Immediate("No cards to select. Cannot create a schedule with no cards.", "Error Creating Schedule");
                    return;
                }

                EditTextMessageBoxController.OpenModalWindow("New Schedule", "Create Title", (title) =>
                    {
                        if (title != null && title != string.Empty)
                        {

                            var canText = UnitedStatesPhoneNumber.TryParse(WarSystem.AccountPreferences.PhoneNumber, out var phone);

                            if (!canText)
                            {
                                MessageBoxHandler.Print_Immediate("Cannot Text You the link. Send anyway?", "Question", (answer) =>
                                {
                                    if (answer)
                                    {
                                        doc.SubmitCards(title, selected, false, false, new List<KeyValuePair<string, DataSet>>());
                                    }
                                });
                            }
                            else
                            {
                                Debug.Log("submitting cards");
                                doc.SubmitCards(title, selected, false, canText, new List<KeyValuePair<string, DataSet>>());
                            }
                        }
                        else
                        {
                            MessageBoxHandler.Print_Immediate("Cannot create a schedule with an empty title.", "Error");
                        }
                    });
            }, true));


            ExportOptionsManager exportOptionsManager = new ExportOptionsManager();

            shareButtons.Add(("Export", () => { exportOptionsManager.Export(() => { ActiveSheetsDisplayer.main.ViewReferences(); }, "References"); }, true));

            string title = "Disable Action Cards";

            if (!GeneralSettings.EnableActors)
            {
                title = "Enable Action Cards";
            }

            shareButtons.Add((title, () => { GeneralSettings.EnableActors = !GeneralSettings.EnableActors; }, true));

            shareButtons.Add(("Message Me <size=75%>SMS", () =>
            {
                EditTextMessageBoxController.OpenModalWindow("", "Message Me " + WarSystem.AccountPreferences.PhoneNumber, (x) =>
                {
                    if (!string.IsNullOrEmpty(x))
                    {
                        var sms = new WarManager.Sharing.TwilioSMSHandler();
                        sms.SendMessage(x, WarSystem.AccountPreferences.PhoneNumber);
                    }
                }, false);
            }, WarSystem.AccountPreferences != null && !string.IsNullOrEmpty(WarSystem.AccountPreferences.PhoneNumber)));

            var selectedCards = CardUtility.GetSelectedCardsOnCurrentSheet();
            var phoneColumnData = CardUtility.GetCommonColumnInfoFromCards(selectedCards, SearchType.valueType, ColumnInfo.GetValueTypeOfPhone);
            var EmailColumnData = CardUtility.GetCommonColumnInfoFromCards(selectedCards, SearchType.valueType, ColumnInfo.GetValueTypeOfEmail);


            shareButtons.Add(("Send Announcement", () =>
            {

                EditTextMessageBoxController.OpenModalWindow("", "Edit Announcement Message", (x) =>
                {
                    if (x != null && x.Trim().Length > 0)
                    {



                        bool phoneError = false;
                        bool emailError = false;

                        var sms = new WarManager.Sharing.TwilioSMSHandler();

                        foreach (var y in phoneColumnData)
                        {
                            if (y.Entry.TryGetValueWithHeader(y.HeaderName, out var value))
                            {
                                try
                                {
                                    if (UnitedStatesPhoneNumber.TryParse(value.Value.ToString(), out var result))
                                    {
                                        sms.SendMessage(x, result.FullNumberUS, false, false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    WarSystem.WriteToLog(ex.Message + " " + value.Value.ToString(), Logging.MessageType.error);
                                    phoneError = true;
                                }
                            }
                        }

                        List<string> emails = new List<string>();

                        foreach (var y in EmailColumnData)
                        {
                            if (y.Entry.TryGetValueWithHeader(y.HeaderName, out var value))
                            {

                                string email = value.ParseToParagraph();
                                emails.Add(email);
                            }
                        }

                        try
                        {
                            EmailClient.SendSMTPEmail(emails.ToArray(), "Announcement from " + WarSystem.CurrentActiveAccount.FirstName + " " + WarSystem.CurrentActiveAccount.LastName, x, true, false);
                        }
                        catch (Exception ex)
                        {
                            WarSystem.WriteToLog("Error sending emails in announcement " + ex.Message, Logging.MessageType.error);
                            emailError = true;
                        }

                        if (!phoneError && !emailError)
                        {
                            WarSystem.WriteToDev("Announcement - " + x, Logging.MessageType.logEvent);
                        }

                        if (emailError)
                        {
                            WarSystem.WriteToDev("Some email errors writing an announcement: " + x, Logging.MessageType.error);
                        }

                        if (phoneError)
                        {
                            WarSystem.WriteToDev("Some phone errors writing an announcement: " + x, Logging.MessageType.error);
                        }

                    }
                    else
                    {
                        LeanTween.delayedCall(1, () => { MessageBoxHandler.Print_Immediate("Nothing to send", "Note"); });
                    }
                });

            }, selectedCards.Count > 0));

            if (WarSystem.AccountPreferences == null)
            {
                Debug.LogError("account prefs null");
            }

            // if (string.IsNullOrEmpty(WarSystem.AccountPreferences.PhoneNumber))
            // {
            //     Debug.LogError("account phone number null");
            // }

            // shareButtons.Add(("Import", () => {}, false));
            // shareButtons.Add(("Export", () => {}, false));

            string messagesName = "Reload Messages";
            WarSystem.UserMessageNotificationsHandler.Deserialize();
            var amt = WarSystem.UserMessageNotificationsHandler.GetNotificationsRecieved(WarSystem.CurrentActiveAccount.UserName).Count;

            if (amt > 0)
                messagesName = messagesName + "(" + amt + ")";
            shareButtons.Add((messagesName, () => { _notificationsUIController.ShowAllNotifications(.5f); }, true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetLeft;
            PickMenu.PickMenuManger.main.OpenPickMenu(shareButtons, finalLocation);
        }

        public void ActivateCurrentSheetMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> currentSheetButtons = new List<(string, Action actions, bool active)>();

            bool active = IsNotHomeSheetCurrent;


            currentSheetButtons.Add(("Save <size=75%>(ctrl + s)", _saveCommands.SaveSheets, true));
            currentSheetButtons.Add(("Rename", _sheetCommands.RenameSheet, active));
            currentSheetButtons.Add(("Close", _sheetCommands.CloseSheet_buttonListener, active));
            currentSheetButtons.Add(("Delete", _sheetCommands.DeleteSheet_buttonListener, active));
            currentSheetButtons.Add(("Properties", () => { ActiveSheetsDisplayer.main.SheetProperties(SheetsManager.CurrentSheetID); }, active));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetLeft;
            PickMenu.PickMenuManger.main.OpenPickMenu(currentSheetButtons, finalLocation);
        }

        public void ActivateAccountMenu(Transform obj)
        {
            List<(string, Action actions, bool active)> currentSheetButtons = new List<(string, Action actions, bool active)>();

            bool active = IsNotHomeSheetCurrent;

            currentSheetButtons.Add(("Account Settings", () => ActiveSheetsDisplayer.main.ShowAccountInfo(true), true));
            // currentSheetButtons.Add(("Preferences", () => _settingsWindow.ActivateWindow(true), true));
            if (WarSystem.CurrentActiveAccount.IsAdmin)
                currentSheetButtons.Add(("Edit Permissions (Admin)", () => { ActiveSheetsDisplayer.main.ViewPermissions(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));
            else
                currentSheetButtons.Add(("View Permissions", () => { ActiveSheetsDisplayer.main.ViewPermissions(ActiveSheetsDisplayer.main.ViewReferences, "References"); }, true));
            currentSheetButtons.Add(("Quit", _sheetCommands.QuitSession, true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetRight;
            PickMenu.PickMenuManger.main.OpenPickMenu(currentSheetButtons, finalLocation);
        }

        public void ActivateWorldOptions(Transform obj)
        {
            List<(string, Action actions, bool active)> currentSheetButtons = new List<(string, Action actions, bool active)>();

            currentSheetButtons.Add(("Web Site - Support", () => MessageBoxHandler.Print_Immediate("Taking you to " + GeneralSettings.WebSiteSupportURL, "Note", (x) =>
            {
                if (x)
                {
                    Application.OpenURL(GeneralSettings.WebSiteSupportURL);
                }
            }), true));

            currentSheetButtons.Add(("File Bug Report", () => MessageBoxHandler.Print_Immediate("Taking you to " + GeneralSettings.BugReportURL, "Note", (x) =>
            {
                if (x)
                {
                    Application.OpenURL(GeneralSettings.BugReportURL);
                }
            }), true));

            currentSheetButtons.Add(("Message Developer (SMS)", () => EditTextMessageBoxController.OpenModalWindow("", "Message to Taylor Howell", (x) =>
            {
                if (!string.IsNullOrEmpty(x))
                {
                    var handler = new WarManager.Sharing.TwilioSMSHandler();
                    Task.Run(() => handler.SendMessage(x, "+19137497477"));
                }
            }), true));

            Vector3 finalLocation = obj.position + (Vector3)spawnOffsetRight;
            PickMenu.PickMenuManger.main.OpenPickMenu(currentSheetButtons, finalLocation);
        }


        /// <summary>
        /// stop using the forms
        /// </summary>
        public void StopForms() => WarSystem.FormsController.CancelForms();
    }
}
