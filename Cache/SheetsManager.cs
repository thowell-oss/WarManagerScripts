/* SheetsManager.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using WarManager;
using WarManager.Cards;
using WarManager.Unity3D;
using WarManager.Sharing;
using WarManager.Sharing.Security;
using StringUtility;

namespace WarManager.Backend
{
    /// <summary>
    /// Manages the opened sheets
    /// </summary>
    public static class SheetsManager
    {
        /// <summary>
        /// The camera controller
        /// </summary>
        public static WarManagerCameraController Camera { get; set; }

        /// <summary>
        /// The current sheet being viewed
        /// </summary>
        public static string CurrentSheetID { get; private set; }

        /// <summary>
        /// The last current sheet being viewed before the current sheet now
        /// </summary>
        public static string PreviousCurrentSheetID { get; private set; }


        /// <summary>
        /// Is the home sheet active?
        /// </summary>
        public static bool HomeSheetActive => HomeSheetID == CurrentSheetID;

        /// <summary>
        /// The number of sheets loaded in war manager
        /// </summary>
        /// <value></value>
        public static int SheetCount
        {
            get
            {
                return _activeCardSheetDictionary.Count;
            }
        }

        /// <summary>
        /// The opened card sheets
        /// </summary>
        private static Dictionary<string, Sheet<Card>> _activeCardSheetDictionary { get; set; } = new Dictionary<string, Sheet<Card>>();

        /// <summary>
        /// Loop through the sheets
        /// </summary>
        /// <value></value>
        public static IEnumerable<Sheet<Card>> Sheets => _activeCardSheetDictionary.Values;


        public delegate void open_sheet(string id);
        /// <summary>
        /// Called when a new card sheet is opened
        /// </summary>
        public static event open_sheet OnOpenCardSheet;

        public delegate void close_sheet(string id);
        /// <summary>
        /// Called when a sheet is closed
        /// </summary>
        public static event close_sheet OnCloseCardSheet;

        public delegate void delegate_setCurrentSheet(string id);
        /// <summary>
        /// Called when a sheet is set to current
        /// </summary>
        public static event delegate_setCurrentSheet OnSetSheetCurrent;

        public delegate void delegate_FindText(string text);

        /// <summary>
        /// Called when the user is looking for specific tag/text on the cards
        /// </summary>
        public static event delegate_FindText OnFindText;

        /// <summary>
        /// The war manager sheet extension
        /// </summary>
        public static string CardSheetExtension = ".wmsht";

        /// <summary>
        /// The card sheet file version
        /// </summary>
        public static double CardSheetFileVersion = 1;

        public static string CurrentFindText = "";

        /// <summary>
        /// private backing field
        /// </summary>
        private static string _homeSheetID;

        /// <summary>
        /// The id of the home sheet
        /// </summary>
        /// <value></value>
        public static string HomeSheetID
        {
            get
            {
                if (_homeSheetID == null || _homeSheetID == string.Empty)
                {
                    foreach (var x in Sheets)
                    {
                        if (x.Name == GeneralSettings.HomeSheetName)
                        {
                            _homeSheetID = x.ID;
                            return _homeSheetID;
                        }
                    }
                }
                return _homeSheetID;
            }

            set
            {
                if (value != null && value != string.Empty)
                    _homeSheetID = value;
            }
        }


        /// <summary>
        /// Used for system wide encryption for file assurance
        /// </summary>
        /// <value></value>
        public static byte[] SystemEncryptKey
        {
            get
            {
                return new byte[16] { 0x00, 0x54, 0xAD, 0x4D, 0xAF, 0x1E, 0x34, 0xF3, 0xA5, 0x54, 0x1E, 0x4D, 0x4F, 0xF8, 0x0A, 0xE0 };
            }
        }

        /// <summary>
        /// The default scale of card sheets
        /// </summary>
        /// <value></value>
        public static double[] DefaultCardScale
        {
            get
            {
                return GeneralSettings.DefaultGridScale;
            }
        }

        private static bool _showDataSetColorBars = true;

        /// <summary>
        /// Show the data set color bars
        /// </summary>
        public static bool ShowDataSetColorBars
        {
            get => _showDataSetColorBars;
            set
            {
                _showDataSetColorBars = value;
                ReloadCurrentSheet();
            }
        }

        /// <summary>
        /// Set a new sheet to be viewed
        /// </summary>
        /// <param name="id">the next current sheet</param>
        public static void SetSheetCurrent(string id)
        {
            if (id == null)
                return;

            // if (!HandleConnectionToServer())
            // {
            //     return;
            // }

            if (!WarSystem.IsConnectedToServer)
                return;

            if (_activeCardSheetDictionary.ContainsKey(id))
            {
                SheetsServerManifest.Refresh(WarSystem.CurrentSheetsManifest);

                PreviousCurrentSheetID = CurrentSheetID;

                CurrentSheetID = id;
                if (OnSetSheetCurrent != null)
                {
                    OnSetSheetCurrent(id);
                }

                var cardSheet = _activeCardSheetDictionary[id];

                if (WarSystem.FormsController != null)
                {
                    if (!cardSheet.Persistent) WarSystem.FormsController.StartForms(cardSheet);
                    else if (WarSystem.FormsController.UsingForms) WarSystem.FormsController.StopFormsEvent();
                }

                WarSystem.WriteToLog("Sheet set to view: " + _activeCardSheetDictionary[id].Name + " (" + id + ")", Logging.MessageType.logEvent);
            }
            else
            {
                WarSystem.WriteToLog("Attempt to set card to view failed: " + id, Logging.MessageType.error);
            }
        }

        /// <summary>
        /// Reload the current sheet
        /// </summary>
        public static void ReloadCurrentSheet()
        {
            try
            {
                if (SheetCount > 0 && !string.IsNullOrEmpty(CurrentSheetID))
                {
                    SetSheetCurrent(CurrentSheetID);
                }
                else
                {
                    SetFirstSheetCurrent();
                    //UnityEngine.Debug.Log("sent home");
                }
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }
        }

        private static bool HandleConnectionToServer()
        {
            // UnityEngine.Debug.Log("Attempting to set card sheet current");

            if (WarSystem.CurrentSheetsManifest == null)
            {
                NotificationHandler.Print("Sheets manifest is null, attempting to connect to server.");

                try
                {
                    WarSystem.AttemptConnectionToServer();

                    if (!WarSystem.IsLoggedIn)
                    {
                        NotificationHandler.Print("Connection failed. User is not logged in.");
                        return false; //returns if the server is not available
                    }
                    else
                    {
                        NotificationHandler.Print("Connection successful:\n" + WarSystem.ConnectedDeviceStamp);
                    }
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print("Error connecting to server. " + ex.Message + "\nConnection failed.");
                    return false;
                }
            }

            return WarSystem.IsLoggedIn;
        }

        /// <summary>
        /// Sets the first sheet in a list (usually the home sheet) active
        /// </summary>
        public static void SetFirstSheetCurrent()
        {
            var sheets = GetListOfActiveSheets();

            if (sheets.Count > 0)
                SetSheetCurrent(sheets[0].ID);
        }

        /// <summary>
        /// Set the current sheet to be the home sheet
        /// </summary>

        public static void OpenHomeCardSheet_Startup()
        {

            if (SheetsManager.SheetCount > 1)
                return;

            if (!WarSystem.IsConnectedToServer)
            {
                if (WarSystem.CurrentActiveAccount == null)
                {
                    NotificationHandler.Print("You are not logged in to the server. This might be because the server is down, or because you are not logged in.");
                }

                return;
            }

            var sheets = GetActiveCardSheets();

            if (sheets.Length > 1)
            {
                foreach (var sheet in sheets)
                {
                    if (sheet.Name == GeneralSettings.HomeSheetName)
                    {
                        HomeSheetID = sheet.ID;

                        return;
                    }
                }

                SetSheetCurrent(HomeSheetID);
            }

            var closedSheets = new List<SheetMetaData>();

            if (WarSystem.CurrentSheetsManifest != null)
            {
                closedSheets = WarSystem.CurrentSheetsManifest.GetFilteredListOfSheetMetaData(new List<string>(), WarSystem.CurrentActiveAccount);
            }
            else
            {
                NotificationHandler.Print("Error, sheet manifest is null");
            }

            foreach (var sheet in closedSheets)
            {
                if (sheet.SheetName == GeneralSettings.HomeSheetName)
                {
                    string path = GeneralSettings.Save_Location_Server + @"\Sheets\" + sheet.SheetName + CardSheetExtension;

                    if (OpenCardSheet(path, SystemEncryptKey, out string ID))
                    {
                        WarSystem.WriteToLog("Home Sheet successfully opened", Logging.MessageType.info);
                        HomeSheetID = sheet.ID;
                    }
                    else
                    {
                        WarSystem.WriteToLog("Home Sheet open failure", Logging.MessageType.critical);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to load sheet preferences
        /// </summary>
        public static void AttemptLoadSheetPreferences()
        {

            if (WarSystem.AccountPreferences == null)
                throw new NullReferenceException("account preferences are null");

            if (WarSystem.AccountPreferences.TryGetLastCurrentSheet(out var lastCurrentSheet))
            {
                var sheets = WarSystem.AccountPreferences.GetLastOpenedSheets();

                if (sheets.Count == 1)
                    return;

                int time = 0;

                for (int i = 0; i < sheets.Count; i++)
                {
                    if (sheets[i] != null && sheets[i].SheetPath != null)
                        OpenCardSheet(sheets[i].SheetPath, SystemEncryptKey, out var id);
                    time = i * 1;
                }

                LeanTween.delayedCall(.5f, () =>
                {
                    SetSheetCurrent(lastCurrentSheet.ID);
                    SimpleUndoRedoManager.main.Clear();
                });
            }

            if (SheetCount == 0)
            {
                OpenHomeCardSheet_Startup();
            }
        }

        /// <summary>
        /// Set the first sheet in the list of sheets to the current sheet. If all sheets are closed, a new sheet is spawned in its place
        /// </summary>
        public static void SetLastSheetCurrent()
        {
            var sheets = GetActiveCardSheets();

            if (sheets == null || SheetCount < 1)
            {
                string id = NewCardSheet("Card Sheet", new string[1] { "*" }, GeneralSettings.DefaultGridScale, new string[0], false);
                return;
            }

            UnityEngine.Debug.Log("setting last sheet current ");

            var sheetId = sheets[sheets.Length - 1].ID;
            SetSheetCurrent(sheetId);
        }

        /// <summary>
        /// Get the active sheet meta data
        /// </summary>
        /// <param name="id">the active sheet id</param>
        /// <returns>returns the file controlled sheet meta data, null if the operation was not successful</returns>
        public static SheetMetaData GetSheetMetaData(string id)
        {
            if (id == null)
                throw new NullReferenceException("The sheet id cannot be null");

            if (!WarSystem.IsLoggedIn)
                return null;

            var sheets = WarSystem.CurrentSheetsManifest.Sheets;

            foreach (var sheet in sheets)
            {
                if (sheet.Data.ID == id)
                {

                    if (FileControl<SheetMetaData>.TryGetServerFile(sheet, WarSystem.ServerVersion, WarSystem.CurrentActiveAccount, out var file))
                    {
                        return file;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("could not get sheet " + sheet.Data.SheetName);
                    }

                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the war grid from a specific sheet
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <returns>returns the sheet war grid, if not possible a default war grid is returned</returns>
        public static WarGrid GetWarGrid(string id)
        {
            if (id == null)
                throw new NullReferenceException("The id cannot be null");

            if (id == string.Empty)
                return new WarGrid(new double[2] { 0, 0 }, DefaultCardScale);

            var scale = GetGridScale(id);
            var offset = GetGridOffset(id);

            return new WarGrid(offset, scale);
        }

        /// <summary>
        /// Get the grid scale of a specific sheet
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <returns>returns the double of the grid scale for the sheet, otherwise it returns the default scale</returns>
        public static double[] GetGridScale(string id)
        {
            if (id == null)
                throw new NullReferenceException("sheet id is null");

            if (!WarSystem.IsLoggedIn)
                return DefaultCardScale;

            if (id == string.Empty)
            {
                return DefaultCardScale;
            }

            var sheet = SheetsManager.GetSheetMetaData(id);

            if (sheet != null)
            {
                return sheet.GridSize;
            }

            // if (WarSystem.GetSheetMetaData(CurrentSheetID, out var sheet))
            // {
            //     return sheet.GridSize;
            // }

            return DefaultCardScale;
        }

        /// <summary>
        /// Get the offset of the grid
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <returns></returns>
        public static double[] GetGridOffset(string id) //grid offset disabled for now
        {
            return new double[2] { 0, 0 };
        }

        /// <summary>
        /// Rename the current sheet
        /// </summary>
        /// <param name="newName">the proposed new name</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool RenameSheet(string newName, string id)
        {
            if (id == null)
                throw new NullReferenceException("id cannot be null");

            if (id == string.Empty)
                throw new NotSupportedException("the id cannot be an empty string");

            if (newName == null)
                throw new NullReferenceException("the requested new name cannot be null");

            if (id == HomeSheetID)
                return false;

            if (!WarSystem.IsLoggedIn)
                return false;

            if (newName.Trim() == GeneralSettings.HomeSheetName)
            {
                return false;
            }

            newName = newName.Trim();

            if (newName == string.Empty)
                throw new NotSupportedException("the new name cannot be empty");

            if (!WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(id, out var sheetMetaData))
                return false;

            var metaData = sheetMetaData.Data;

            if (!metaData.CanEdit)
            {
                NotificationHandler.Print("This sheet cannot be renamed");
                UnityEngine.Debug.Log("The sheet cannot be renamed");
                return false;
            }

            newName = ValidateString(newName);

            string location = GeneralSettings.Save_Location_Server + @"\Sheets\";
            string sheetPath = location + newName + CardSheetExtension;

            if (File.Exists(location + newName + " Meta.json") || File.Exists(location + newName + CardSheetExtension))
            {
                return false;
            }

            if (!File.Exists(location + metaData.SheetName + CardSheetExtension) || !File.Exists(location + metaData.SheetName + " Meta.json"))
                TrySaveSheet_SystemKey(metaData.ID);

            Sheet<Card> sheet = GetActiveSheet(id);
            sheet.Name = newName;
            sheet.FileName = newName;

            var newData = NewCardSheetMetadata(newName, metaData.Owner, metaData.ID, newName, sheetPath, metaData.CanEdit, metaData.GridSize, metaData.Categories, sheetMetaData.CreationTime);

            if (WarSystem.CurrentSheetsManifest.ReplaceMetaData(newData))
            {

                File.Move(location + metaData.SheetName + " Meta.json", location + newName + " Meta.json");

                if (File.Exists(location + metaData.SheetName + " Meta.json"))
                {
                    File.Delete(location + metaData.SheetName + " Meta.json");
                }


                File.Move(location + metaData.SheetName + CardSheetExtension, location + newName + CardSheetExtension);

                if (File.Exists(location + metaData.SheetName + CardSheetExtension))
                {
                    File.Delete(location + metaData.SheetName + CardSheetExtension);
                }

                if (sheet.Persistent)
                    SaveCardSheet(sheet, SystemEncryptKey);
                SetSheetCurrent(id);

                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("was not able to replace meta data");
            }

            return false;
        }

        /// <summary>
        /// Does the sheet exist?
        /// </summary>
        /// <param name="id">the id of the sheet</param>
        /// <returns>returns true if the sheet exists, false if not</returns>
        public static bool IsSheetActive(string id)
        {
            if (_activeCardSheetDictionary.Count < 1)
                return false;

            if (id == null)
                throw new NullReferenceException("The id string cannot be null");

            if (id == string.Empty)
                return false;

            if (!WarSystem.IsLoggedIn)
                throw new WarManager.WarManager_NotLoggedInException();

            return _activeCardSheetDictionary.ContainsKey(id);
        }


        /// <summary>
        /// Is the sheet with the given Id the current viewing sheet?
        /// </summary>
        /// <param name="id">the given sheet id</param>
        /// <returns>returns true if the sheet id is the same as the currentSheetId. False if not</returns>
        public static bool IsSheetCurrent(string id)
        {
            if (CurrentSheetID != id)
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to get the current sheet if the sheet is available
        /// </summary>
        /// <param name="sheet">the sheet to get</param>
        /// <returns>returns true if the sheet was found, false if not</returns>
        public static bool TryGetCurrentSheet(out Sheet<Card> sheet)
        {
            string id = SheetsManager.CurrentSheetID;

            if (id == null || id == string.Empty)
            {
                sheet = null;
                return false;
            }

            if (!WarSystem.IsLoggedIn)
            {
                sheet = null;
                return false;
            }

            sheet = GetActiveSheet(id);

            if (sheet == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to get the active sheet with a certain id
        /// </summary>
        /// <param name="id">the id of the sheet to get</param>
        /// <param name="sheet">(out) the resulting sheet -> the sheet will be null if the operation was not successful</param>
        /// <returns>returns true if getting the sheet was successful, false if not</returns>
        public static bool TryGetActiveSheet(string id, out Sheet<Card> sheet)
        {
            if (!IsSheetActive(id))
            {
                sheet = null;
                return false;
            }

            sheet = GetActiveSheet(id);
            return true;
        }


        /// <summary>
        /// returns the sheet with the given id
        /// </summary>
        /// <param name="id">the given id of the sheet</param>
        /// <returns>returns the sheet if the sheet exists, null if not</returns>
        public static Sheet<Card> GetActiveSheet(string id)
        {
            if (IsSheetActive(id))
            {
                return _activeCardSheetDictionary[id];
            }

            if (!WarSystem.IsLoggedIn)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Load the sheet
        /// </summary>
        /// <param name="newCardSheet"></param>
        private static void LoadSheetToDictionary(Sheet<Card> newCardSheet)
        {
            if (!_activeCardSheetDictionary.TryGetValue(newCardSheet.ID, out var val))
            {
                _activeCardSheetDictionary.Add(newCardSheet.ID, newCardSheet);
            }

            if (OnOpenCardSheet != null)
                OnOpenCardSheet(newCardSheet.ID);

            SetSheetCurrent(newCardSheet.ID);

            WarSystem.WriteToLog("New Sheet created: " + newCardSheet.Name + " (" + newCardSheet.ID + ").", Logging.MessageType.logEvent);
        }

        /// <summary>
        /// uses the filepath to read and open a card sheet
        /// </summary>
        /// <param name="filePath">the file location of the card sheet</param>
        /// <returns>returns true if the card sheet was successfully opened</returns>
        public static bool OpenCardSheet(string filePath, byte[] key, out string ID)
        {

            try
            {
                var payload = Import_Sheet<Card>.ImportLocalSheet(filePath, key, 5);

                if (payload != null)
                {
                    if (TryCreateNewInstanceOfCardSheet(payload.Sheet, payload.Data, true, out var id))
                    {
                        ID = id;

                        LoadSheetToDictionary(payload.Sheet);

                        //LoadingScreenManager.main.LoadingSliderNormalized = 1;

                        if (WarSystem.IsConnectedToServer && WarSystem.CurrentActiveAccount != null)
                            GhostCardManager.Visible = true;

                        if (WarSystem.CurrentActiveAccount == null)
                            GhostCardManager.Visible = false;

                        WarSystem.WriteToLog("Sheet successfully opened: " + payload.Sheet.Name + " " + payload.Sheet.ID, Logging.MessageType.info);
                        WarSystem.DeveloperPushNotificationHandler.OpenedSheets = true;
                        return true;
                    }
                    else
                    {
                        WarSystem.WriteToLog("Imported data to instance creation of the sheet failed: " + payload.Sheet.Name + " " + payload.Sheet.ID, Logging.MessageType.error);
                    }
                }
            }
            catch (FileNotFoundException fEx)
            {
                MessageBoxHandler.Print_Immediate(fEx.Message, "Error opening file");
            }

            WarSystem.WriteToLog("Failed to import sheet", Logging.MessageType.error);
            ID = null;
            return false;

        }

        /// <summary>
        /// Create a new card sheet
        /// </summary>
        /// <param name="name">The user defined name</param>
        /// <returns>returns the sheet id to be used in order to access information</returns>
        public static string NewCardSheet(string name, string[] categories, double[] gridScale, string[] datasets,
         bool persistent = true, List<Card> cards = null)
        {
            if (WarSystem.CurrentActiveAccount == null)
            {
                NotificationHandler.Print("Cannot open a sheet because no account is logged in.");
            }

            if (WarSystem.CurrentSheetsManifest == null)
                throw new NullReferenceException("the sheets manifest is null");

            if (!WarSystem.IsLoggedIn)
                throw new NotSupportedException("Must be logged in to the server");

            if (categories == null || categories.Length < 1)
                throw new NullReferenceException("There must be something in the categories array");

            foreach (var c in categories)
            {
                if (string.IsNullOrEmpty(c.Trim()))
                {
                    throw new NullReferenceException("There must be something in the categories array");
                }
            }

            if (gridScale == null || gridScale.Length < 1)
            {
                gridScale = GeneralSettings.DefaultGridScale;
            }

            string id = System.Guid.NewGuid().ToString();

            if (TryCreateNewInstanceOfCardSheet(name, id, datasets, persistent, out Sheet<Card> newCardSheet))
            {
                string sheetName = newCardSheet.Name;
                string fileName = newCardSheet.FileName;
                string owner = WarSystem.CurrentActiveAccount.UserName;
                string path = GeneralSettings.Save_Location_Server_Sheets;
                // var categories = permissions.FullAccessCategories;
                DateTime creation = DateTime.Now;
                double version = SheetsManager.CardSheetFileVersion;

                // double[] gridSize = GeneralSettings.DefaultGridScale;

                var sheetMetaData = NewCardSheetMetadata(sheetName, owner, id, fileName, path, true, gridScale, categories, creation, version);
                WarSystem.CurrentSheetsManifest.AddSheet(sheetMetaData);

                SheetDropPointManager.SetDropPoint(sheetMetaData.Data.LastDropPointLocation.ConvertToPoint(), id);

                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        newCardSheet.AddObj(card);
                    }
                }

                LoadSheetToDictionary(newCardSheet);
                return id;
            }

            UnityEngine.Debug.Log("Could not create new sheet");

            return null;
        }

        /// <summary>
        /// Create new card sheet meta data
        /// </summary>
        /// <param name="sheetName">the name of the sheet</param>
        /// <param name="owner">who created the sheet?</param>
        /// <param name="id">the sheet id</param>
        /// <param name="sheetFileName">the name of the sheet file (just the name, not the path or ext). Usually the same thing as the sheet name</param>
        /// <param name="sheetFilePathFolder">which folder the sheet needs to go in</param>
        /// <param name="categories">the string array of categories the sheet is associated with</param>
        /// <param name="createdTime">when the sheet was created</param>
        /// <param name="version">the sheet and meta data file version</param>
        /// <returns>returns a the sheet meta data with the file control wrapper</returns>
        private static FileControl<SheetMetaData> NewCardSheetMetadata(string sheetName, string owner, string id, string sheetFileName, string sheetFilePathFolder, bool canEdit,
        double[] gridSize, string[] categories, DateTime createdTime, double version = 1)
        {
            SheetMetaData d = new SheetMetaData(sheetName, owner, version, id, sheetFilePathFolder + @"\" + sheetFileName + CardSheetExtension, canEdit, gridSize, categories, DateTime.Now, null, null);
            FileControl<SheetMetaData> fileControlledData = d.GetFileControl(createdTime, createdTime);

            return fileControlledData;
        }

        /// <summary>
        /// Add a dataset to the current sheet
        /// </summary>
        /// <param name="newDataSetId">the id of the new data set</param>
        /// <returns>returns true if the operation is successful, false if not</returns>
        public static bool AddNewDataSetToCurrentSheet(string newDataSetId)
        {
            return AddNewDataSetToExistingSheet(CurrentSheetID, newDataSetId);
        }

        /// <summary>
        /// Add a dataset to an existing sheet
        /// </summary>
        /// <param name="sheetId">the sheet id</param>
        /// <param name="newDataSetId">the id of the new dataset</param>
        /// <returns>returns true if the operation is successful, false if not</returns>
        /// <exception cref="NullReferenceException">thrown when one (or both) of the strings is/are null</exception>
        /// <exception cref="ArgumentException">thrown when one (or both) of the strings is/are empty</exception>
        public static bool AddNewDataSetToExistingSheet(string sheetId, string newDataSetId)
        {
            if (sheetId == null)
                throw new NullReferenceException("the sheet id is null");

            if (sheetId == string.Empty)
                throw new ArgumentException("the sheet id string is empty");

            if (newDataSetId == null)
                throw new NullReferenceException("the sheet id is null");

            if (newDataSetId == string.Empty)
                throw new ArgumentException("the sheet id string is empty");

            if (TryGetActiveSheet(sheetId, out var sheet))
            {
                sheet.AddDataset(newDataSetId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a new cardSheet preconstruction and opens it
        /// </summary>
        /// <param name="sheetName">the name of the sheet to create</param>
        /// <param name="fileName">the file name of the sheet</param>
        /// <returns>returns true if the new sheet was created successfully, false if not</returns>
        private static bool TryCreateNewInstanceOfCardSheet(string sheetName, string id, string[] datasetIds, bool persistent, out Sheet<Card> s)
        {
            sheetName = ValidateString(sheetName);

            Sheet<Card> newSheet = new Sheet<Card>(id, sheetName, sheetName, persistent);

            foreach (var datasetID in datasetIds)
            {
                newSheet.AddDataset(datasetID);
            }

            s = newSheet;

            return true;
        }

        /// <summary>
        /// Creates a card sheet mid construction
        /// </summary>
        /// <param name="sheet">the sheet to create</param>
        /// <param name="cardData">the list of data to add to the sheet</param>
        /// <returns>returns the id</returns>
        private static bool TryCreateNewInstanceOfCardSheet(Sheet<Card> sheet, List<string[]> cardData, bool persistent, out string id)
        {
            if (sheet == null)
                throw new NullReferenceException("Sheet cannot be null");

            id = sheet.ID;

            if (cardData != null)
            {

                if (WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(sheet.ID, out var sheetMetaData))
                {
                    _activeCardSheetDictionary.Add(sheet.ID, sheet);

                    SheetDropPointManager.SetDropPoint(sheetMetaData.Data.LastDropPointLocation.ConvertToPoint(), sheet.ID);

                    List<Card> cards = new List<Card>();
                    for (int i = 0; i < cardData.Count; i++)
                    {
                        // UnityEngine.Debug.Log(String.Join(",", cardData[i]));

                        string[] data = new string[cardData[i].Length + 1];

                        data[0] = sheet.ID;
                        for (int k = 0; k < cardData[i].Length; k++)
                        {
                            data[k + 1] = cardData[i][k];
                        }

                        Card c = new Card(data);
                        cards.Add(c);

                        // UnityEngine.Debug.Log("Added card " + c.ID);
                        //WarSystem.WriteToLog("Added card " + c.ID + c.point, logging.MessageType.logEvent);
                    }

                    // sheet.AddObjRange(cards);

                    foreach (var card in cards)
                    {
                        sheet.AddObj(card);
                    }

                    // UnityEngine.Debug.Log("Added cards to sheet " + cardData.Count + " " + sheet.ID);

                    return true;
                }
                else
                {
                    WarSystem.WriteToLog("The sheet could not be created. Failed to find the sheet meta data" + sheet.Name, Logging.MessageType.error);
                    throw new NotSupportedException("Could not find the sheet meta data");
                }
            }

            WarSystem.WriteToLog("The sheet could not be created. The list of cards was null" + sheet.Name, Logging.MessageType.error);

            return false;
        }

        /// <summary>
        /// Close all sheets in the session
        /// </summary>
        /// <param name="key">the key to encrypt the file</param>
        /// <param name="save">should these sheets be saved?</param>
        /// <returns>returns true if all sheets were closed successfully, false if even one wasn't</returns>
        public static bool CloseSheets(byte[] key, bool save = true)
        {
            if (key == null || key.Length < 1)
                return false;

            var sheets = GetActiveCardSheets();

            bool success = true;

            foreach (var sheet in sheets)
            {
                if (!CloseSheet(sheet.ID, true))
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Remove an active card sheet from the session
        /// </summary>
        /// <param name="id">the id of the sheet to remove</param>
        /// <param name="key">the key to encrypt the file</param>
        /// <param name="save">should the file be saved?</param>
        /// <returns>returns true if the sheet was closed successfully, false if not</returns>
        public static bool CloseSheet(string id, bool overrideHomeSheet = false, bool bypassQuestion = false)
        {
            if (id == null || id == string.Empty)
                return false;

            // UnityEngine.Debug.Log("Closing sheet");

            Sheet<Card> cardSheet;

            if (_activeCardSheetDictionary.TryGetValue(id, out cardSheet))
            {
                // UnityEngine.Debug.Log("Found sheet");

                if (!bypassQuestion)
                    MessageBoxHandler.Print_Immediate($"Close {cardSheet.Name.SetStringQuotes()}? Unsaved changes will be lost", "Notice",
                        (x) => { FinishClosing(x, cardSheet, overrideHomeSheet); });
                else FinishClosing(true, cardSheet, overrideHomeSheet);
            }

            return false;
        }

        /// <summary>
        /// finish closing the sheet
        /// </summary>
        /// <param name="proceed">should the sheet proceed to be closed?</param>
        /// <param name="cardSheet">teh card sheet to close</param>
        /// <param name="overrideHomeSheet">override the homeSheet (shouldn't be normally closeable)</param>
        private static void FinishClosing(bool proceed, Sheet<Card> cardSheet, bool overrideHomeSheet = false)
        {
            if (proceed)
            {
                var sheets = GetListOfActiveSheets();
                if (cardSheet.ID != HomeSheetID || (cardSheet.ID == HomeSheetID && overrideHomeSheet))
                {

                    if (WarSystem.CurrentSheetsManifest.TryRemoveSheetFromList(cardSheet.ID, out var file))
                    {
                        _activeCardSheetDictionary.Remove(cardSheet.ID);

                        if (OnCloseCardSheet != null)
                        {
                            OnCloseCardSheet(cardSheet.ID);
                        }

                        WarSystem.ActiveCardActors.RemoveActorsBySheet(cardSheet);

                        SimpleUndoRedoManager.main.ClearUndo();


                        if (sheets.Count > 0)
                            SetSheetCurrent(sheets[0].ID);

                        return;
                    }

                    if (sheets.Count > 0)
                        SetSheetCurrent(sheets[0].ID);
                }
            }
        }

        /// <summary>
        /// Attempt to delete a sheet
        /// </summary>
        /// <param name="id">the selected id of the sheet to delete</param>
        /// <returns>returns true if the sheet was successfully deleted</returns>
        public static void DeleteSheet(string id)
        {
            if (id == null || id == string.Empty)
                return;

            if (id == HomeSheetID)
                return;

            MessageBoxHandler.Print_Immediate("Are you sure you want to delete? Unsaved changes will be lost.", "Question", x => { CompleteDeleteSheet_MessageBoxResult(x, id); });
        }

        /// <summary>
        /// Complete remove card after
        /// </summary>
        /// <param name="removeCard">has the user chosen to remove the sheet</param>
        /// <param name="id">the id of the sheet to remove</param>
        private static void CompleteDeleteSheet_MessageBoxResult(bool removeCard, string id)
        {

            if (id == HomeSheetID)
                return;

            if (removeCard)
            {
                string deletePath = GeneralSettings.Save_Location_Server_Sheets + @"\Deleted";

                if (TryGetActiveSheet(id, out var sheet))
                {
                    if (File.Exists(GeneralSettings.Save_Location_Server_Sheets + @"\" + sheet.Name + CardSheetExtension) && File.Exists(GeneralSettings.Save_Location_Server_Sheets + @"\" + sheet.Name + " Meta.json"))
                    {
                        string moveToName = sheet.Name;

                        if (File.Exists(deletePath + @"\" + sheet.Name + " Meta.json") || File.Exists(deletePath + @"\" + sheet.Name + CardSheetExtension))
                        {
                            moveToName = sheet.Name + " " + DateTime.Now.ToFileTime().ToString();
                        }

                        if (WarSystem.CurrentSheetsManifest != null && WarSystem.CurrentSheetsManifest.TryRemoveSheetFromList(id, out var sheetData))
                        {
                            File.Move(GeneralSettings.Save_Location_Server_Sheets + @"\" + sheet.Name + " Meta.json", deletePath + @"\" + moveToName + " Meta.json");
                            File.Move(GeneralSettings.Save_Location_Server_Sheets + @"\" + sheet.Name + CardSheetExtension, deletePath + @"\" + moveToName + CardSheetExtension);
                        }
                    }

                    FinishClosing(true, sheet);
                }
                else
                {
                    NotificationHandler.Print("Could not find the sheet to delete");
                }
            }
            else
            {
                SetSheetCurrent(SheetsManager.CurrentSheetID);
            }
        }

        /// <summary>
        /// Get the list of active sheets   
        /// </summary>
        /// <returns></returns>
        private static List<Sheet<Card>> GetListOfActiveSheets()
        {
            List<Sheet<Card>> sheets = new List<Sheet<Card>>();

            foreach (var x in Sheets)
            {
                sheets.Add(x);
            }

            return sheets;
        }

        /// <summary>
        /// Add a dataset to the sheet
        /// </summary>
        /// <param name="datasetID">the dataset</param>
        public static void AddDataset(Sheet<Card> sheet, string datasetID)
        {
            if (WarSystem.DataSetManager.TryGetDataset(datasetID, out var set))
            {
                sheet.AddDataset(datasetID);
                ReloadCurrentSheet();
            }
            else
            {
                MessageBoxHandler.Print_Immediate("You do not have access to the selected dataset.", "Error");
            }
        }

        /// <summary>
        /// Get datasets from a sheet
        /// </summary>
        /// <param name="sheet">the datasets from the sheet</param>
        /// <returns>return the list of datasets</returns>
        public static List<DataSet> GetDataSets(Sheet<Card> sheet)
        {
            var sets = sheet.GetDatasetIDs();


            List<DataSet> dataSets = new List<DataSet>();

            foreach (var x in sets)
            {
                if (WarSystem.DataSetManager.TryGetDataset(x, out var set))
                {
                    dataSets.Add(set);
                }
            }

            return dataSets;
        }

        /// <summary>
        /// Remove the data set and all associated cards from the sheet 
        /// </summary>
        /// <param name="sheet">the sheet</param>
        /// <param name="dataSetId">the id of the dataset</param>
        public static void RemoveDataSet(Sheet<Card> sheet, string dataSetId)
        {
            if (WarSystem.DataSetManager.TryGetDataset(dataSetId, out var set))
            {

                MessageBoxHandler.Print_Immediate($"Are you sure you want to remove {set.DatasetName}?\nAll cards from the dataset will be lost.", "Question", (y) =>
                {
                    if (y)
                    {
                        if (sheet.ContainsDataSet(dataSetId))
                        {
                            foreach (var x in sheet.ContentDict)
                            {
                                if (x.Value is Card c)
                                {
                                    if (c.DatasetID == dataSetId)
                                    {
                                        sheet.RemoveObj(c.point, c.Layer);
                                    }
                                }
                            }

                            sheet.RemoveDataset(dataSetId);
                            ReloadCurrentSheet();
                        }
                        else
                        {
                            throw new ArgumentException("the sheet does not contain the data set");
                        }
                    }
                    else
                    {
                        MessageBoxHandler.Print_Immediate("You do not have access to the selected dataset", "Error");
                    }
                });
            }
        }

        /// <summary>
        /// Removes the entry from all sheets that are open if possible
        /// </summary>
        public static void RemoveEntryFromAllOpenSheets(DataEntry entry)
        {
            foreach (var x in Sheets)
            {
                List<Card> cards = x.GetAllObj();

                List<Card> cardsWithEntry = cards.FindAll(x => x.Entry.DataSet == entry.DataSet && x.Entry.RowID == entry.RowID);

                foreach (var y in cardsWithEntry)
                {
                    CardUtility.RemoveCard(y.point, y.Layer, x.ID);
                }
            }

            ReloadCurrentSheet();
        }

        /// <summary>
        /// Save all the opened sheets
        /// </summary>
        /// <returns>returns true if all sheets were saved successfully, false if not</returns>
        public static bool SaveAllSheets()
        {

            RecordLastOpenedSheets();

            var sheets = SheetsManager.GetActiveCardSheets();

            bool saved = true;

            foreach (var s in sheets)
            {
                try
                {
                    if (!SheetsManager.TrySaveSheet_SystemKey(s.ID))
                    {
                        saved = false;
                    }
                }
                catch (SheetNotPersistentException sheetEx)
                {
                    //nothing happens because all sheets are saved anyway
                    UnityEngine.Debug.Log("Attempt to save non-persistent sheet failed (" + sheetEx.Sheet.Name + "). This is expected.");
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print("Error saving " + ex.Message);
                }
            }

            return saved;
        }

        /// <summary>
        /// Records the last opened sheets into player prefs
        /// </summary>
        private static void RecordLastOpenedSheets()
        {
            List<string> sheetIds = new List<string>();

            var sheets = SheetsManager.GetActiveCardSheets();

            foreach (var sheet in sheets)
            {
                sheetIds.Add(sheet.ID);
            }

            string ids = string.Join(",", sheetIds);


            //injecting some unity here
            UnityEngine.PlayerPrefs.SetString("open-sheets", CurrentSheetID);
            UnityEngine.PlayerPrefs.SetString("last-set-current", CurrentSheetID);
            UnityEngine.PlayerPrefs.Save();
        }

        /// <summary>
        /// Save the card sheet using a system key
        /// </summary>
        /// <param name="sheetId">the id of the card sheet to save</param>
        /// <exception cref="SheetNotPersistentException">thrown when the sheet is not persistent - the file should not be saved</exception>
        public static bool TrySaveSheet_SystemKey(string sheetId)
        {
            if (sheetId == null || sheetId == string.Empty)
                return false;


            // UnityEngine.Debug.Log("sheet ID " + sheetId);

            if (TryGetActiveSheet(sheetId, out var cardSheet))
            {
                if (!cardSheet.Persistent)
                    throw new SheetNotPersistentException(cardSheet, "Cannot save the sheet when it is not a persistent sheet");

                if (SaveCardSheet(cardSheet, SystemEncryptKey))
                {
                    WarSystem.WriteToLog("Saving sheet " + cardSheet.Name, Logging.MessageType.info);
                    return true;
                }
                else
                {
                    NotificationHandler.Print("Failed to save sheet " + cardSheet.Name);
                    WarSystem.WriteToLog("Failed to save sheet " + cardSheet.Name, Logging.MessageType.error);
                }

                // UnityEngine.Debug.Log("could not save sheet");
            }

            UnityEngine.Debug.Log("could not get the active sheet");

            EmailClient.SendNotificationSMTPEmailToDev("Could not save sheet on " + WarSystem.ConnectedServerName, "A sheet could not be saved: " + sheetId + "\n" + WarSystem.ConnectedDeviceStamp, true);

            return false;
        }

        /// <summary>
        /// Save the sheet
        /// </summary>
        /// <param name="cardSheet">the card sheet to save</param>
        /// <param key="key">the key to encrypt the file</param>
        public static bool SaveCardSheet(Sheet<Card> cardSheet, byte[] key, bool savingOneSheet = true)
        {
            if (cardSheet == null)
                throw new NullReferenceException("The sheet cannot be null");

            // if (cardSheet.CardCount <= 0)
            // {
            //     NotificationHandler.Print("Cannot save an empty sheet: \'" + cardSheet.Name + "\'");
            //     return false;
            // }

            if (!cardSheet.Persistent)
                return false;

            try
            {
                var export = new Export_Sheet<Card>();
                if (export.ExportSheet(cardSheet, key))
                {

                    // UnityEngine.Debug.Log(cardSheet.Name);

                    if (!WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(cardSheet.ID, out var fileControl))
                    {
                        // UnityEngine.Debug.Log("could not find the current sheets manifest");
                        MessageBoxHandler.Print_Immediate("Could not find the sheets manifest", "Error");
                    }

                    SheetsServerManifest.SaveServerMetaData(fileControl);
                    // UnityEngine.Debug.Log("file saved");
                    return true;
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Your file was not saved.", "Error!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
                MessageBoxHandler.Print_Immediate("There was an error attempting to save the sheet \'" + cardSheet.Name + "\': " + ex.Message, "Error");
            }

            return false;
        }

        /// <summary>
        /// Get all the active card sheets
        /// </summary>
        /// <returns>returns an array of card sheets</returns>
        public static Sheet<Card>[] GetActiveCardSheets()
        {
            List<Sheet<Card>> sheets = new List<Sheet<Card>>();

            foreach (var key in _activeCardSheetDictionary.Keys)
            {
                sheets.Add(_activeCardSheetDictionary[key]);
            }

            sheets.Sort((x, y) =>
            {
                return x.Name.CompareTo(y.Name);
            });

            Sheet<Card>[] copy = new Sheet<Card>[sheets.Count];
            Array.Copy(sheets.ToArray(), copy, copy.Length);

            return copy;
        }


        /// <summary>
        /// Gathers all sheets and checks the names to see if the new name is too similar
        /// </summary>
        /// <param name="newName"></param>
        /// <returns>returns the appropriate name</returns>
        private static string ValidateString(string newName)
        {
            List<string> allNames = new List<string>();

            newName = newName.CheckStringMatch("Card Sheet");

            var sheets = GetActiveCardSheets();
            var manifest = WarSystem.CurrentSheetsManifest;

            foreach (var sheet in sheets)
            {
                allNames.Add(sheet.Name);
            }

            foreach (var m in manifest.Sheets)
            {
                string name = m.Data.SheetName;
                allNames.Add(name);
            }

            return newName.ValidateWord(allNames.ToArray());
        }

        /// <summary>
        /// Called to clear the dictionary upon session end
        /// </summary>
        public static void ClearSheetDictionary()
        {
            _activeCardSheetDictionary = new Dictionary<string, Sheet<Card>>();
        }

        /// <summary>
        /// Find text on cards
        /// </summary>
        /// <param name="text"></param>
        public static void Find(string text)
        {
            if (OnFindText != null)
                OnFindText(text);

            CurrentFindText = text;
        }

        public static void CancelFind()
        {
            if (OnFindText != null)
                OnFindText("");

            CurrentFindText = "";
        }
    }
}
