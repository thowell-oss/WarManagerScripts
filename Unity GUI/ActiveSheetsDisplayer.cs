/* ActiveSheetDisplayer.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Unity3D.Windows;
using WarManager.Backend;

using Sirenix.OdinInspector;

using StringUtility;

using WarManager.Sharing.Security;
using WarManager.Sharing;

using StringUtility;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles managing the selection of the multiple sheets in the war manager
    /// </summary>
    public class ActiveSheetsDisplayer : MonoBehaviour
    {
        [TabGroup("Runtime")]
        [SerializeField]
        private List<string> _closedSheetNames = new List<string>();
        [TabGroup("Runtime")]
        [SerializeField]
        private List<string> _dataSets = new List<string>();
        [TabGroup("Setup")]
        public Sprite cardSheetSprite;
        [TabGroup("Setup")]
        public Sprite editCardSheetSprite;
        [TabGroup("Setup")]
        public Sprite DataSetSprite;
        [TabGroup("Setup")]
        public Sprite BackSprite;
        [TabGroup("Setup")]
        public Sprite RefreshSprite;
        [TabGroup("Setup")]
        public Sprite DeleteSprite;
        [TabGroup("Setup")]
        public Sprite DeleteForeverSprite;
        [TabGroup("Setup")]
        public Sprite AddSprite;
        [TabGroup("Setup")]
        public Sprite AddSheetSprite;
        [TabGroup("Setup")]
        public Sprite MergeSprite;
        [TabGroup("Setup")]
        public Sprite ViewSprite;
        [TabGroup("Setup")]
        public Sprite PermissionsSprite;
        [TabGroup("Setup")]
        public Sprite CodeSprite;
        [TabGroup("Setup")]
        public Sprite ShareSprite;
        [TabGroup("Setup")]
        public Sprite FolderSprite;
        [TabGroup("Setup")]
        public Sprite FileSprite;
        [TabGroup("Setup")]
        public Sprite UpSprite;
        [TabGroup("Setup")]
        private string selectedSheetName;

        [TabGroup("Runtime")]
        private List<DataSet> selectedDataSets = new List<DataSet>();

        #region singleton
        public static ActiveSheetsDisplayer main;

        /// <summary>
        /// The File Picker
        /// </summary>
        /// <value></value>
        public FilePicker FilePicker { get; private set; }

        /// <summary>
        /// the ui manager for handling new sheets
        /// </summary>
        private NewSheetUIManager _newSheetUIManager;

        void Awake()
        {
            if (main == null)
            {
                main = this;
            }
            else
            {
                throw new System.NotSupportedException("only on active sheets displayer can be used at a time");
            }
        }

        #endregion

        void Start()
        {
            // ShowReference();
            ShowAccountInfo(false);

            _newSheetUIManager = new NewSheetUIManager();
            FilePicker = new FilePicker();
        }

        /// <summary>
        /// Set the properties window to show sheet properties
        /// </summary>
        public void ShowSheetProperties()
        {
            if (SheetsManager.SheetCount < 1)
                return;

            RefreshSheetProperties(SheetsManager.CurrentSheetID, true);
        }

        public void OnOpenCardSheet(string id)
        {
            // RefreshReferences();
            RefreshSheetProperties(id, false);
        }

        public void OnSetSheetCurrent(string id)
        {
            RefreshSheetProperties(id, false);
            //ViewReferences();
        }

        public void OnCloseCardSheet(string id)
        {
            ViewReferences();
            ClearProperties();
        }

        private void ClearProperties()
        {
            SlideWindowsManager.main.ClearProperties();
        }

        private void RefreshSheetProperties(string id, bool ForceWindowPaneOpen)
        {
            // if (id == null || id == string.Empty)
            // {
            ClearProperties();
            SlideWindowsManager.main.CloseProperties(false);
            return;
            // }

            //Debug.Log(id);

            var sheet = SheetsManager.GetActiveSheet(id);

            if (!WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(id, out var sheetFileControl))
            {
                Debug.LogError("sheet file control is null");
                return;
            }

            if (sheet == null)
            {
                Debug.LogError("sheet is null");
                ClearProperties();
                return;
            }

            var contentList = new List<SlideWindow_Element_ContentInfo>();

            var sheetInfo = sheetFileControl.Data;

            SlideWindow_Element_ContentInfo header = new SlideWindow_Element_ContentInfo(sheetInfo.SheetName, null);
            contentList.Add(header);

            SlideWindow_Element_ContentInfo owner = new SlideWindow_Element_ContentInfo("Owner", sheetInfo.Owner);
            SlideWindow_Element_ContentInfo displayId = new SlideWindow_Element_ContentInfo("ID", sheetInfo.ID);
            contentList.Add(owner);
            contentList.Add(displayId);

            contentList.Add(new SlideWindow_Element_ContentInfo(100));
            contentList.Add(new SlideWindow_Element_ContentInfo("Data Sets", null));

            var datasetIds = sheet.GetDatasetIDs();

            foreach (var set in datasetIds)
            {

                if (WarSystem.DataSetManager.TryGetDataset(set, out var dataset))
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("Data Set name", dataset.DatasetName));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Owner", dataset.Owner));

                    List<string> categoriesList = new List<string>();

                    foreach (var cat in dataset.Categories)
                    {
                        categoriesList.Add(cat);
                    }

                    contentList.Add(new SlideWindow_Element_ContentInfo("Permission Categories", string.Join(", ", categoriesList)));

                }
                else
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("Not Accessible " + dataset.DatasetName, null));
                }
                contentList.Add(new SlideWindow_Element_ContentInfo(20));
            }

            contentList.RemoveAt(contentList.Count - 1);


            SlideWindowsManager.main.AddPropertiesContent(contentList, ForceWindowPaneOpen);
        }

        public void ShowAccountInfo(bool ForceWindowPaneOpen)
        {

            AccountManagementUI ui = new AccountManagementUI();
            ui.ShowAccountInfo(WarSystem.CurrentActiveAccount, ForceWindowPaneOpen, WarSystem.CurrentActiveAccount);
        }

        #region old

        // public void ShowAccountInfo(bool ForceWindowPaneOpen)
        // {

        // var account = WarSystem.CurrentActiveAccount;
        // var contentList = new List<SlideWindow_Element_ContentInfo>();

        // contentList.Add(new SlideWindow_Element_ContentInfo("References", ViewReferences, null));
        // contentList.Add(new SlideWindow_Element_ContentInfo(20));

        // bool foundPhone = false;

        // if (account != null)
        // {
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Account Info", 20));
        //     contentList.Add(new SlideWindow_Element_ContentInfo(20));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("User Name", account.UserName));

        //     if (WarSystem.AccountPreferences.PhoneNumber != null && WarSystem.AccountPreferences.PhoneNumber.Trim().Length > 0)
        //     {
        //         contentList.Add(new SlideWindow_Element_ContentInfo("Phone", WarSystem.AccountPreferences.PhoneNumber));
        //         foundPhone = true;
        //     }

        //     contentList.Add(new SlideWindow_Element_ContentInfo("Real Name", account.FirstName + " " + account.LastName));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Permissions", account.PermissionsName));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("View Permissions", 0, (x) =>
        //     {
        //         ViewPermissions(() =>
        //         {
        //             ShowAccountInfo(true);
        //         }, $"Account Settings");
        //     }, PermissionsSprite));

        //     contentList.Add(new SlideWindow_Element_ContentInfo(20));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Details", 20));
        //     var str = string.Join(", ", account.FullAccessCategories);
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Categories", str));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Admin?", account.IsAdmin.ToString()));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Language", account.UserSelectedLanguage.ToString()));
        //     contentList.Add(new SlideWindow_Element_ContentInfo(50));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Change Password", 0, (x) => { PasswordChangeHandler.HandlePasswordChange(); },
        //         null));

        //     UserPreferencesHandler.CheckAutomaticLogin();

        //     if (UserPreferencesHandler.AutomaticLoginEnabled)
        //     {
        //         contentList.Add(new SlideWindow_Element_ContentInfo("Turn Off Automatic Log in", 0, (x) =>
        //         {
        //             UserPreferencesHandler.DisableAutomaticLogin();
        //             ShowAccountInfo(ForceWindowPaneOpen);
        //         }, null));
        //     }
        //     else
        //     {
        //         contentList.Add(new SlideWindow_Element_ContentInfo("Turn On Automatic Log in", 0, (x) =>
        //         {
        //             UserPreferencesHandler.EnableAutomaticLogin();
        //             ShowAccountInfo(ForceWindowPaneOpen);
        //         }, null));
        //     }
        //     contentList.Add(new SlideWindow_Element_ContentInfo(20));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Verify Phone", 20));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Why does War Manager need my Phone Number?", "In order to use some messaging capabilities, " +
        //     "(like texting yourself an employee phone number), War Manager needs to know who to text."));
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Note", "War Manager will not spam your phone"));

        //     string phone = "";

        //     contentList.Add(new SlideWindow_Element_ContentInfo("Phone Number", "", (x) =>
        //     {
        //         phone = x;
        //     }));

        //     var phoneNumberInput = new SlideWindow_Element_ContentInfo("Verify Phone Number", 0, (x) => { VerifyPhone(phone); }, null);
        //     phoneNumberInput.DescriptionHeader = "Verify Phone";
        //     phoneNumberInput.DescriptionInfo = "Type in your phone number and select \'Verify Phone\' below. Answer the security question and you are good to go.";
        //     contentList.Add(phoneNumberInput);

        //     contentList.Add(new SlideWindow_Element_ContentInfo(20));

        //     string text = "Turn Off Context Buttons";
        //     if (!SheetCardClusterContextHandler.Main.ActivateCluster)
        //     {
        //         text = "Turn On Context Buttons";
        //     }

        //     contentList.Add(new SlideWindow_Element_ContentInfo(text, 0, (x) =>
        //     {
        //         SheetCardClusterContextHandler.Main.ToggleActivateCluster();
        //         ShowAccountInfo(ForceWindowPaneOpen);

        //     }, null));
        // }
        // else
        // {
        //     contentList.Add(new SlideWindow_Element_ContentInfo("Not logged in", null));
        // }

        // SlideWindowsManager.main.AddReferenceContent(contentList, ForceWindowPaneOpen);
        //}

        // private void VerifyPhone(string phone)
        // {
        //     if (UnitedStatesPhoneNumber.TryParse(phone, out var result))
        //     {
        //         var y = UnityEngine.Random.Range(0, 100);
        //         var z = UnityEngine.Random.Range(0, 100);
        //         var j = UnityEngine.Random.Range(0, 1);

        //         while (z == y)
        //             z = UnityEngine.Random.Range(0, 100);

        //         string message = "War Manager Phone Verification Code: " + y;

        //         Debug.Log(result.FullNumberUS);

        //         TwilioSMSHandler handler = new TwilioSMSHandler();
        //         handler.SendMessage(message, result.FullNumberUS, false, true);

        //         if (j == 0)
        //         {
        //             MessageBoxHandler.Print_Immediate("Did you get " + y + "?", "Question", (x) =>
        //             {
        //                 if (x)
        //                 {
        //                     WarSystem.AccountPreferences.PhoneNumber = result.FullNumberUS;
        //                     LeanTween.delayedCall(1, () =>
        //                     {
        //                         MessageBoxHandler.Print_Immediate("Phone Number Verified", "Note");
        //                         WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
        //                     });

        //                     UserPreferencesHandler.SavePreferences();

        //                     ShowAccountInfo(false);
        //                 }
        //             });
        //         }
        //         else
        //         {
        //             MessageBoxHandler.Print_Immediate("Did you get " + z + "?", "Question", (x) =>
        //             {
        //                 if (!x)
        //                 {
        //                     WarSystem.AccountPreferences.PhoneNumber = result.FullNumberUS;
        //                     LeanTween.delayedCall(1, () =>
        //                     {
        //                         MessageBoxHandler.Print_Immediate("Phone Number Verified", "Note");
        //                     });

        //                     UserPreferencesHandler.SavePreferences();

        //                     ShowAccountInfo(false);
        //                 }
        //             });
        //         }
        //     }
        //     else
        //     {
        //         MessageBoxHandler.Print_Immediate("Incorrect format - your phone number must be 10 digits long", "Error");
        //     }
        // }

        #endregion

        /// <summary>
        /// Finds, sorts and displays the sheets (both active and closed)
        /// </summary>
        public void ViewReferences()
        {

            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();

            if (WarSystem.CurrentActiveAccount != null)
            {
                List<Sheet<Card>> sheets = new List<Sheet<Card>>();

                sheets.Clear();
                sheets.AddRange(SheetsManager.GetActiveCardSheets());

                sheets.Sort(delegate (Sheet<Card> a, Sheet<Card> b)
                {
                    return a.Name.ToLower().CompareTo(b.Name.ToLower());
                });

                contentList.Add(new SlideWindow_Element_ContentInfo("", 0, (x) =>
                {
                    SlideWindowsManager.main.CloseReference(true);
                }, BackSprite));
                contentList.Add(new SlideWindow_Element_ContentInfo("Refresh", 0, (x) =>
                {
                    WarSystem.RefreshLoadedDataSets();
                    SheetsManager.ReloadCurrentSheet();
                }, RefreshSprite));



                contentList.Add(new SlideWindow_Element_ContentInfo(20));
                contentList.Add(new SlideWindow_Element_ContentInfo("Sheets", 20));
                contentList.Add(new SlideWindow_Element_ContentInfo("Create New Sheet", 0, (x) =>
                {
                    if (_newSheetUIManager != null)
                        _newSheetUIManager.SetNewSheet(ViewReferences, "References");
                }, AddSheetSprite));

                if (!SheetsManager.HomeSheetActive || SheetsManager.SheetCount != 1)
                    contentList.Add(new SlideWindow_Element_ContentInfo("Home Sheet", 0, (x) =>
                        {
                            SheetsManager.SetSheetCurrent(SheetsManager.HomeSheetID);
                        }, editCardSheetSprite));

                contentList.Add(new SlideWindow_Element_ContentInfo(20));

                if (SheetsManager.SheetCount > 0)
                {

                    contentList.Add(new SlideWindow_Element_ContentInfo(10));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Downloaded Sheets", 20));


                    if ((SheetsManager.SheetCount == 1 && SheetsManager.IsSheetActive(SheetsManager.HomeSheetID)) || SheetsManager.SheetCount < 1)
                    {
                        contentList.Add(new SlideWindow_Element_ContentInfo(10));
                        contentList.Add(new SlideWindow_Element_ContentInfo("No Downloaded Sheets", "Click on a sheet (below) or create a new one."));
                    }

                    Sheet<Card> selectedSheet = null;

                    if (!string.IsNullOrEmpty(SheetsManager.CurrentSheetID) && SheetsManager.CurrentSheetID != SheetsManager.HomeSheetID)
                    {

                        selectedSheet = SheetsManager.GetActiveSheet(SheetsManager.CurrentSheetID);

                        if (selectedSheet != null && WarSystem.GetSheetMetaData(selectedSheet.ID, out var sheetMetaData))
                        {


                            if (sheetMetaData != null && sheetMetaData.SheetName.Trim() != GeneralSettings.HomeSheetName)
                            {
                                try
                                {
                                    // Debug.Log("Adding a slide window element");
                                    var selected = new SlideWindow_Element_ContentInfo(sheetMetaData, selectedSheet);
                                    if (WarSystem.AccountPreferences != null)
                                        if (WarSystem.AccountPreferences.PinnedSheets.Contains(sheetMetaData.ID))
                                        {
                                            selected.isPinned = true;
                                        }


                                    contentList.Add(selected);
                                    contentList.Add(new SlideWindow_Element_ContentInfo(10));
                                }
                                catch (System.Exception ex)
                                {
                                    NotificationButtonManager.print(ex.Message);
                                }
                            }
                        }
                    }
                    else if (SheetsManager.HomeSheetActive)
                    {

                    }

                    var pinnedSheets = WarSystem.AccountPreferences.GetActivePinnedSheets();

                    if (SheetsManager.TryGetCurrentSheet(out var currentSheet))
                    {
                        pinnedSheets.Remove(currentSheet);
                    }

                    for (int m = sheets.Count - 1; m >= 0; m--)
                    {
                        if (pinnedSheets.Contains(sheets[m]))
                        {
                            sheets.RemoveAt(m);
                        }
                    }

                    foreach (var x in pinnedSheets)
                    {
                        if (WarSystem.GetSheetMetaData(x.ID, out var sheetMetaData))
                        {
                            var sheet = new SlideWindow_Element_ContentInfo(sheetMetaData, x);
                            if (WarSystem.AccountPreferences != null)
                                if (WarSystem.AccountPreferences.PinnedSheets.Contains(sheetMetaData.ID))
                                {
                                    sheet.isPinned = true;
                                }
                                else
                                {
                                    sheet.isPinned = false;
                                }

                            contentList.Add(sheet);
                        }
                    }

                    for (int i = 0; i < sheets.Count; i++)
                    {
                        int loc = i;
                        if (sheets[i] != null && sheets[i].ID != string.Empty)
                        {
                            var nextSprite = cardSheetSprite;

                            if (sheets[i].ID != SheetsManager.CurrentSheetID && sheets[i].Name.Trim() != GeneralSettings.HomeSheetName)
                            {

                                if (WarSystem.GetSheetMetaData(sheets[i].ID, out var sheetMetaData))
                                {
                                    var sheet = new SlideWindow_Element_ContentInfo(sheetMetaData, sheets[i]);
                                    if (WarSystem.AccountPreferences != null)
                                        if (WarSystem.AccountPreferences.PinnedSheets.Contains(sheetMetaData.ID))
                                        {
                                            sheet.isPinned = true;
                                        }
                                        else
                                        {
                                            sheet.isPinned = false;
                                        }

                                    contentList.Add(sheet);
                                }
                            }
                        }
                    }
                }
                else
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("", "No Open Sheets"));
                }

                List<SheetMetaData> closedSheets = new List<SheetMetaData>();

                if (WarSystem.CurrentSheetsManifest != null)
                    closedSheets = WarSystem.CurrentSheetsManifest.GetClosedSheets(WarSystem.CurrentActiveAccount);

                Queue<SheetMetaData> pinnedSheetData = new Queue<SheetMetaData>();

                if (WarSystem.AccountPreferences != null)
                {
                    var pinnedClosedSheets = WarSystem.AccountPreferences.GetClosedPinnedSheets();

                    for (int i = closedSheets.Count - 1; i >= 0; i--)
                    {
                        for (int l = 0; l < pinnedClosedSheets.Count; l++)
                        {
                            if (closedSheets[i].ID == pinnedClosedSheets[l].ID)
                            {
                                pinnedSheetData.Enqueue(closedSheets[i]);
                                closedSheets.RemoveAt(i);
                            }
                        }
                    }
                }

                _closedSheetNames.Clear();

                if (closedSheets.Count > 0)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Sheets in the Cloud", 20));

                    while (pinnedSheetData.Count > 0)
                    {
                        var contentInfo = new SlideWindow_Element_ContentInfo(pinnedSheetData.Dequeue());
                        contentInfo.isPinned = true;
                        contentList.Add(contentInfo);
                    }

                    for (int i = 0; i < closedSheets.Count; i++)
                    {
                        if (closedSheets[i] != null)
                            contentList.Add(new SlideWindow_Element_ContentInfo(closedSheets[i]));
                    }

                    contentList.Add(new SlideWindow_Element_ContentInfo(50));
                }

                contentList.Add(new SlideWindow_Element_ContentInfo("Data Sets", 20));
                contentList.Add(new SlideWindow_Element_ContentInfo("Create New Data Set", 0, (x) => { MessageBoxHandler.Print_Immediate("Coming Soon", "Note"); }, AddSprite));
                contentList.Add(new SlideWindow_Element_ContentInfo(10));

                _dataSets.Clear();
                contentList.Add(new SlideWindow_Element_ContentInfo("Action", -1, (x) =>
                {
                    DataSetViewer.main.ShowActions(ViewReferences);
                }, DataSetSprite));

                int k = 0;

                if (WarSystem.DataSetManager == null)
                    return;

                foreach (var set in WarSystem.DataSetManager.Datasets)
                {

                    var d = new SlideWindow_Element_ContentInfo(set.DatasetName.SetStringQuotes(), k, (x) => { ShowDataSetEntries.Main.SetSearch(set, (x) => { DataSetViewer.main.ShowDataEntryInfo(x, () => { ViewReferences(); }, "Main Menu"); }); }, DataSetSprite);
                    d.Button_PickMenuMore.Add(("Properties", () =>
                    {
                        DataSetViewer.main.ShowDataSetProperties(() => { ViewReferences(); SlideWindowsManager.main.OpenReference(true); }, "References", set);
                    }, true));


                    if (WarSystem.CurrentActiveAccount.IsAdmin)
                    {
                        d.Button_PickMenuMore.Add(("Merge Data (Admin)", () => { MessageBoxHandler.Print_Immediate("Coming soon.", "Note"); }, false));
                        d.Button_PickMenuMore.Add(("Quick Replace Data (Admin)", () => { ReplaceData(set, ViewReferences, "References"); }, true));
                        d.Button_PickMenuMore.Add(("View Raw Data (Admin)", () => { MessageBoxHandler.Print_Immediate("Taking you to " + set.DataListLocation, "Note", (x) => { if (x) Application.OpenURL(set.DataFile.FilePath); }); }, true));
                        d.Button_PickMenuMore.Add(("Settings (JSON) (Admin)", () => { MessageBoxHandler.Print_Immediate("Taking you to " + set.DataListLocation, "Note", (x) => { if (x) Application.OpenURL(set.DataSetLocation); }); }, true));
                    }

                    contentList.Add(d);
                    _dataSets.Add(set.ID);

                    k++;
                }

                if (_dataSets.Count < 1)
                    contentList.Add(new SlideWindow_Element_ContentInfo("", "No Data Sets to View"));


                // if (SheetsManager.SheetCount >= 1)
                // {
                //     if (SheetsManager.TryGetCurrentSheet(out var sheet))
                //     {
                //         contentList.Add(new SlideWindow_Element_ContentInfo("Cards in " + sheet.Name, 20));

                //         List<Card> cardsArray = sheet.GetAllObj();

                //         foreach (var card in cardsArray)
                //         {
                //             if (WarSystem.TryGetDataset(card.DatasetID, out var control))
                //             {
                //                 contentList.Add(new SlideWindow_Element_ContentInfo(card.DatasetID, (int)card.DataRepID, (x) =>
                //                 {
                //                     DataSetViewer.main.ShowDataPieceInfo(control.Data, (int)card.DataRepID);
                //                 }));
                //             }
                //         }
                //     }
                // }

                contentList.Add(new SlideWindow_Element_ContentInfo(50));
                contentList.Add(new SlideWindow_Element_ContentInfo("Permissions:", System.String.Join(", ", WarSystem.CurrentActiveAccount.Permissions.GetFullAccessCategories(null))));
                contentList.Add(new SlideWindow_Element_ContentInfo("View Permissions", 0, (x) => { ViewPermissions(ViewReferences, "References"); }, PermissionsSprite));
            }
            else
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Not logged in", 18));
            }

            SlideWindowsManager.main.AddReferenceContent(contentList);
        }

        /// <summary>
        /// Merge Data
        /// </summary>
        /// <param name="set">the data set to merge data</param>
        private void MergeData(DataSet set)
        {
            DataMergeController dataMergeController = new DataMergeController();
            dataMergeController.MergeFiles(set);
        }


        /// <summary>
        /// Replace the data in the data set
        /// </summary>
        /// <param name="set">the data set</param>
        private void ReplaceData(DataSet set, Action back, string backTitle)
        {

            FilePicker.Init(new string[1] { ".csv" }, false, (x) =>
            {
                if (x)
                {
                    string newFilePath = FilePicker.SelectedPath;
                    DataFileInstance newInstance = CSVSerialization.Deserialize(newFilePath, false);

                    if (newInstance.DataCount == 0 || newInstance.HeaderLength == 0)
                    {
                        MessageBoxHandler.Print_Immediate($"Cannot replace {set.DatasetName} data with an empty file", "Note");
                        return;
                    }

                    DataSetReplaceHandler handler = new DataSetReplaceHandler(set.DataFile, newInstance);

                    try
                    {
                        handler.CompareHeaders();
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                        return;
                    }

                    MessageBoxHandler.Print_Immediate($"Are you sure you want to replace {set.DatasetName} data with {newInstance.FileName}? This might take a second...", "Question", (l) =>
                    {
                        if (l)
                        {

                            handler.CalculateMerge(.5f, .7f);

                            List<string[]> dataAdded = new List<string[]>();
                            List<string[]> dataRemoved = new List<string[]>();

                            var replacedInstance = handler.GetNewDataFileInstance(out dataAdded, out dataRemoved);

                            set.ReplaceData(replacedInstance);

                            // DataSetSerializer serializer = new DataSetSerializer();
                            // serializer.SerializeDataSet(set);

                            FilePicker = new FilePicker();

                            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

                            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, BackSprite));

                            content.Add(new SlideWindow_Element_ContentInfo(30));
                            content.Add(new SlideWindow_Element_ContentInfo("New Data Entries", 20));
                            for (int i = 0; i < dataAdded.Count; i++)
                            {
                                if (dataAdded[i].Length > 2 || (dataAdded[i].Length == 2 && dataAdded[i][1].Trim() != string.Empty))
                                    content.Add(new SlideWindow_Element_ContentInfo((i + 1).ToString(), string.Join(", ", dataAdded[i])));
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(30));
                            content.Add(new SlideWindow_Element_ContentInfo("Old Data Entries Removed", 20));
                            for (int i = 0; i < dataRemoved.Count; i++)
                            {
                                if (dataRemoved[i].Length > 2 || (dataRemoved[i].Length == 2 && dataRemoved[i][1].Trim() != string.Empty))
                                {
                                    string[] nextData = new string[dataRemoved.Count - 1];
                                    Array.Copy(dataRemoved[i], 1, nextData, 0, nextData.Length);
                                    content.Add(new SlideWindow_Element_ContentInfo((i + 1).ToString(), string.Join(", ", nextData)));
                                }
                            }

                            SlideWindowsManager.main.AddReferenceContent(content, true);


                            // foreach (var y in replacedInstance.Data)
                            // {
                            //     Debug.Log(string.Join(",", y));
                            // }
                        }
                    });
                }
            });
        }

        /// <summary>
        /// look at a specific sheet
        /// </summary>
        /// <param name="sheetID">the given if of the sheet</param>
        public void SheetProperties(string sheetID)
        {
            SheetProperties(ViewReferences, "References", sheetID);
        }

        /// <summary>
        /// look at a specific sheet
        /// </summary>
        /// <param name="sheetID">the given if of the sheet</param>
        public void SheetProperties(Action back, string backTitle, string sheetID)
        {
            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();
            contentList.Add(new SlideWindow_Element_ContentInfo(20));
            contentList.Add(new SlideWindow_Element_ContentInfo(backTitle, -1, (x) =>
            {
                back();
                SlideWindowsManager.main.OpenReference(true);
            }, BackSprite));
            contentList.Add(new SlideWindow_Element_ContentInfo(20));


            if (SheetsManager.TryGetActiveSheet(sheetID, out var sheet) && WarSystem.GetSheetMetaData(sheetID, out SheetMetaData sheetMetaData))
            {

                contentList.Add(new SlideWindow_Element_ContentInfo(sheet.Name, 22));
                contentList.Add(new SlideWindow_Element_ContentInfo("Description", sheetMetaData.SheetDescription, (x) =>
                {
                    var metaData = sheetMetaData.Clone();

                    if (x != null)
                        metaData.SheetDescription = x;
                    SheetProperties(back, backTitle, sheetID);
                    WarSystem.CurrentSheetsManifest.ReplaceMetaData(metaData);
                }));
                contentList.Add(new SlideWindow_Element_ContentInfo(20));
                if (sheetID != SheetsManager.CurrentSheetID)
                    contentList.Add(new SlideWindow_Element_ContentInfo("View \'" + sheet.Name + "\'", -1, x => { SheetsManager.SetSheetCurrent(sheetID); }, ViewSprite));
                contentList.Add(new SlideWindow_Element_ContentInfo(20));

                var grid = SheetsManager.GetWarGrid(sheetID);
                string gridScale = grid.GridScale.x + ", " + grid.GridScale.y;
                contentList.Add(new SlideWindow_Element_ContentInfo("Scale", gridScale));

                if (sheetID != SheetsManager.HomeSheetID)
                {

                    var categories = sheetMetaData.Categories;
                    var accounts = Account.GetAccountsListNonGreedy(categories);

                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Users that can see \'" + sheetMetaData.SheetName + "\'", 20));

                    for (int i = 0; i < accounts.Count; i++)
                    {
                        string extra = "";

                        if (accounts[i].IsAdmin)
                        {
                            extra = " - Admin";
                        }
                        else if (accounts[i].ContainsAllCategoriesAccessCharacter)
                        {
                            extra = " - All Categories";
                        }

                        contentList.Add(new SlideWindow_Element_ContentInfo(accounts[i].UserName + extra, accounts[i].PermissionsName));
                    }
                }
                else
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("", "Everyone can see this sheet."));
                }

                var datasetIds = sheet.GetDatasetIDs();

                if (datasetIds.Length > 0)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Data Sets in \'" + sheet.Name + "\'", 20));

                    foreach (var setId in sheet.GetDatasetIDs())
                    {
                        if (WarSystem.DataSetManager.TryGetDataset(setId, out var set))
                        {
                            var x = new SlideWindow_Element_ContentInfo(set.DatasetName, -1, x =>
                            {
                                DataSetViewer.main.ShowDataSet(setId, () =>
                                    {
                                        SlideWindowsManager.main.ClearReference();
                                        ActiveSheetsDisplayer.main.ViewReferences();
                                    });
                            }, DataSetSprite);

                            x.Button_PickMenuMore.Add(($"Remove {set.DatasetName.SetStringQuotes()}", () =>
                            {
                                SheetsManager.RemoveDataSet(sheet, set.ID);
                            }, sheetMetaData.CanEdit && set.CanEditData));

                            x.Button_PickMenuMore.Add(($" Properties", () => { DataSetViewer.main.ShowDataSetProperties(() => { SheetProperties(back, backTitle, sheetID); }, sheet.Name.SetStringQuotes(), set); }, WarSystem.CurrentActiveAccount.IsAdmin));

                            contentList.Add(x);
                        }
                    }

                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                }

                var notAddedDataSets = WarSystem.DataSetManager.GetDataSets(WarSystem.DataSetManager.GetDataSetsFromSheet(sheet.ID, false));

                string[] names = new string[notAddedDataSets.Count];

                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = notAddedDataSets[i].DatasetName;
                }

                if (sheetMetaData.CanEdit)
                {
                    var dropDown = new SlideWindow_Element_ContentInfo("Add Dataset", 1, x =>
                    {
                        SelectFromListOfDataSets(() => SheetProperties(sheetID), sheet.Name, sheet, WarSystem.DataSetManager.GetDataSetsFromSheet(sheet.ID, false));
                    }, AddSprite);
                }
                else
                {
                    var dropDown = new SlideWindow_Element_ContentInfo("No Edit Permissions", "You cannot add a Data Set to this sheet.");
                    contentList.Add(dropDown);
                }

                contentList.Add(new SlideWindow_Element_ContentInfo(50));
                contentList.Add(new SlideWindow_Element_ContentInfo("Cards in " + sheet.Name.SetStringQuotes(), 20));

                List<Card> cardsArray = CardUtility.GetCardsFromCurrentSheet();
                List<DataSet> sets = new List<DataSet>();
                foreach (var x in datasetIds)
                {
                    if (WarSystem.DataSetManager.TryGetDataset(x, out var dataset))
                    {
                        sets.Add(dataset);
                    }
                }

                if (sets.Count > 0)
                {
                    sets.Sort((x, y) =>
                    {
                        return x.DatasetName.CompareTo(y.DatasetName);
                    });

                    foreach (var x in sets)
                    {
                        var cardLen = CardUtility.GetCardsFromDataSet(cardsArray, x.ID).Count;
                        contentList.Add(new SlideWindow_Element_ContentInfo($"{x.DatasetName} count", $"{cardLen} cards on {sheet.Name}"));
                    }
                }

                contentList.Add(new SlideWindow_Element_ContentInfo(20));

                foreach (var card in cardsArray)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(card.Entry, (x) =>
                    {
                        DataSetViewer.main.ShowDataEntryInfo(card.Entry, () => { DataSetViewer.main.ShowDataSet(card.DatasetID, () => { SheetProperties(sheetID); }); }, card.DataSet.DatasetName);
                    }));
                }
            }
            else
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Error", "Could not fetch sheet data."));
            }

            SlideWindowsManager.main.AddPropertiesContent(contentList, true);
        }

        /// <summary>
        /// When adding datasets to the sheet, view the datasets options
        /// </summary>
        /// <param name="back">the back action</param>
        /// <param name="backTitle">the back title</param>
        /// <param name="sheet">the sheet</param>
        /// <param name="selectedDataSets">the selected datasets</param>
        private void SelectFromListOfDataSets(Action back, string backTitle, Sheet<Card> sheet, List<DataSet> selectedDataSets)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, 0, (x) =>
            {
                back();
            }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            var unselectedDatasets = WarSystem.DataSetManager.GetDataSets(selectedDataSets);

            for (int i = 0; i < unselectedDatasets.Count; i++)
            {
                content.Add(new SlideWindow_Element_ContentInfo(unselectedDatasets[i].DatasetName, 0, (x) =>
                {
                    SheetsManager.AddDataset(sheet, unselectedDatasets[i].ID);
                    back();
                }, AddSprite));
            }

            SlideWindowsManager.main.AddPropertiesContent(content);
        }

        private void LookAtDataset(int index)
        {
            if (index == -1)
                return;

            DataSetViewer.main.ShowDataSet(_dataSets[index], () =>
                {
                    SlideWindowsManager.main.ClearReference();
                    ActiveSheetsDisplayer.main.ViewReferences();
                });
            return;
        }

        public void NewSheet()
        {
            if (WarSystem.IsConnectedToServer)
                _newSheetUIManager.SetNewSheet(ViewReferences, "References");
            else
                MessageBoxHandler.Print_Immediate("Cannot create a new sheet because War Manager is not connected to the server. Please check the connection and try again. Contact support if you need help.", "Error");
        }

        /// <summary>
        /// View Permissions
        /// </summary>
        /// <param name="back">the back action</param>
        /// <param name="backTitle">back title</param>
        public void ViewPermissions(Action back, string backTitle)
        {
            PermissionsUI ui = new PermissionsUI();
            SlideWindowsManager.main.OpenReference(true);
            ui.ShowPermissions(back, backTitle);
        }

        /// <summary>
        /// Open the sheet using the button designated index location
        /// </summary>
        /// <param name="index">the index of the path location name</param>
        public void OpenSheet(int index)
        {
            MessageBoxHandler.Print_Immediate("Would you like to open \'" + _closedSheetNames[index] + "\' ?", "Question", x =>
            {
                if (x)
                {
                    // LoadingScreenManager.main.SetLoadingScreen(true, "Fetching " + "\'" + _closedSheetNames[index] + "\' from " + "\'" + WarSystem.ConnectedServerName + "\'");
                    // LoadingScreenManager.main.LoadingSliderNormalized = 0;

                    LeanTween.delayedCall(1, () =>
                    {
                        string path = GeneralSettings.Save_Location_Server + @"\Sheets\" + _closedSheetNames[index] + SheetsManager.CardSheetExtension;
                        // LoadingScreenManager.main.LoadingSliderNormalized = .25f;

                        // Debug.Log(path);
                        StartCoroutine(WaitForSheetToOpen(path));
                    });
                }
            });
        }

        IEnumerator WaitForSheetToOpen(string path)
        {
            yield return new WaitUntil(() =>
              {
                  FinishOpenSheet(path);
                  return true;
              });
        }

        void FinishOpenSheet(string path)
        {
            try
            {
                if (SheetsManager.OpenCardSheet(path, SheetsManager.SystemEncryptKey, out string ID))
                {
                    WarSystem.WriteToLog("Sheet successfully opened", Logging.MessageType.logEvent);
                }
                else
                {
                    WarSystem.WriteToLog("Sheet open failure", Logging.MessageType.critical);
                }
            }
            catch (System.Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }
            finally
            {
                // LeanTween.delayedCall(.5f, () =>
                // {
                //     LoadingScreenManager.main.SetLoadingScreen(false, "Fetching " + "\'" + _closedSheetNames[index] + "\' from " + "\'" + WarSystem.ConnectedServerName + "\'");
                // });
            }
        }


        /// <summary>
        /// Display the home sheet
        /// </summary>
        public void OpenHomeSheet()
        {
            SheetsManager.SetFirstSheetCurrent();
        }


        void OnEnable()
        {
            SheetsManager.OnOpenCardSheet += OnOpenCardSheet;
            SheetsManager.OnSetSheetCurrent += OnSetSheetCurrent;
            SheetsManager.OnCloseCardSheet += OnCloseCardSheet;
        }

        void OnDisable()
        {
            SheetsManager.OnOpenCardSheet -= OnOpenCardSheet;
            SheetsManager.OnSetSheetCurrent -= OnSetSheetCurrent;
            SheetsManager.OnCloseCardSheet -= OnCloseCardSheet;
        }
    }
}