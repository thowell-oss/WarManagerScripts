/* WarSystem.cs
 * Author: Taylor Howell
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net;

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

using WarManager.Sharing;
using WarManager.Sharing.Security;
using WarManager.Logging;

using WarManager.Backend;

using WarManager.Forms;
using System.Threading.Tasks;
using WarManager.Cards;
using WarManager.Unity3D;

namespace WarManager
{
    /// <summary>
    /// Handles any broad system actions
    /// </summary>
    //[InitializeOnLoadAttribute]
    [Notes.Author("Handles any broad system actions")]
    public static class WarSystem
    {
        /// <summary>
        /// Handles the logging for the current session
        /// </summary>
        /// <value></value>
        private static LogHandler _logger;

        /// <summary>
        /// Should the logging be shown in the debug console?
        /// </summary>
        public static bool ShowLogInDebugConsole = false;

        /// <summary>
        /// The account that is currently logged into the system
        /// </summary>
        /// <value></value>
        public static Account CurrentActiveAccount { get; private set; }


        /// <summary>
        /// Get account preferences
        /// </summary>
        /// <value></value>
        public static UserPreferences AccountPreferences => UserPreferencesHandler.Preferences;

        /// <summary>
        /// delegate
        /// </summary>
        public delegate void Initialize_delegate();

        /// <summary>
        /// Called when the session has been initialized
        /// </summary>
        public static event Initialize_delegate OnInit;

        /// <summary>
        /// The details of the device running the software
        /// </summary>
        /// <value></value>
        public static string ConnectedDeviceStamp
        {
            get
            {
                return "\n\tTime: " + CurrentDate + " " + CurrentTime + "\n\tDevice ID: " + DeviceID +
                "\n\tIP Address: " + IPAddress + "\nServer Name: \'" + ConnectedServerName + "\'";
            }
        }

        public static string ConnectedDeviceStampNoServerName
        {
            get
            {
                return "\n\tTime: " + CurrentDate + " " + CurrentTime + "\n\tDevice ID: " + DeviceID +
                "\n\tIP Address: " + IPAddress;
            }
        }

        /// <summary>
        /// The server primary color (in hex or RGB form)
        /// </summary>
        /// <value></value>
        public static string ConnectedServerColor { get; private set; } = "#000";

        /// <summary>
        /// The server logo image location
        /// </summary>
        /// <value></value>
        public static string ConnectedServerLogo { get; private set; } = "triangle.png";

        /// <summary>
        /// Get the a short readable current date
        /// </summary>
        /// <value></value>
        public static string CurrentDate
        {
            get
            {
                return DateTime.Now.ToShortDateString();
            }
        }

        /// <summary>
        /// Get a short readable current time
        /// </summary>
        /// <value></value>
        public static string CurrentTime
        {
            get
            {
                return DateTime.Now.ToShortTimeString();
            }
        }

        /// <summary>
        /// Get the IP address (IPv4 or IPv6 if possible)
        /// </summary>
        /// <value></value>
        public static string IPAddress
        {
            get
            {
                //return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();

                string IPAddress = "";
                IPHostEntry Host = default(IPHostEntry);
                string Hostname = null;
                Hostname = System.Environment.MachineName;
                Host = Dns.GetHostEntry(Hostname);
                foreach (IPAddress IP in Host.AddressList)
                {
                    if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        IPAddress = Convert.ToString(IP);
                    }
                }

                return IPAddress;

            }
        }


        /// <summary>
        /// Get the device ID
        /// </summary>
        /// <value></value>
        public static string DeviceID
        {
            get
            {
                return UnityEngine.SystemInfo.deviceUniqueIdentifier;
            }
        }

        /// <summary>
        /// Is War Manager connected to the server?
        /// </summary>
        /// <value></value>
        public static bool IsConnectedToServer { get; private set; }

        /// <summary>
        /// The name of the server War Manager is connected to
        /// </summary>
        /// <value></value>
        public static string ConnectedServerName { get; private set; }

        public delegate void Delegate_ConnectedToServerChange(bool connected);
        public static event Delegate_ConnectedToServerChange OnConnectionToServerChanged;

        /// <summary>
        /// Get a personalized greeting to any messages (if possible)
        /// </summary>
        /// <value></value>
        public static string Greeting
        {
            get
            {
                if (CurrentActiveAccount != null)
                    return "Hey " + CurrentActiveAccount.FirstName + " " + CurrentActiveAccount.LastName + ", ";

                return "Hey, ";
            }
        }

        /// <summary>
        /// The file version of the War Manager
        /// </summary>
        public static readonly string ServerVersion = "v1";

        /// <summary>
        /// The current released version of War Manager
        /// </summary>
        public static readonly string ReleaseVersion = "2.9.2h1";

        /// <summary>
        /// The Major release name of War Manager
        /// </summary>
        public static readonly string MajorReleaseName = "Fenix";

        /// <summary>
        /// Povides extra information about the kind of release - Is it a preview, demo, beta, trial, or full version?
        /// </summary>
        public static readonly string ReleaseType = "Closed Beta";

        /// <summary>
        /// The default dataset
        /// </summary>
        private static DataSet _defaultDataset;

        public static GroupMeDeveloperPushNotificationHandler DeveloperPushNotificationHandler;

        /// <summary>
        /// The default fall back Data Set
        /// </summary>
        /// <value></value>
        public static DataSet DefaultDataSet
        {
            get
            {
                if (_defaultDataset == null)
                {
                    _defaultDataset = DataSetManager.GetDefaultDataSetRules();
                }

                return _defaultDataset;
            }
        }

        /// <summary>
        /// private backing field
        /// </summary>
        private static FormsController _formsController;

        /// <summary>
        /// The War Manager Forms Controller
        /// </summary>
        public static FormsController FormsController { get; private set; }

        /// <summary>
        /// All the categories in the server
        /// </summary>
        /// <value></value>
        public static Categories CurrentCategories { get; private set; }

        /// <summary>
        /// All the permissions loaded from the server when the account was created
        /// </summary>
        /// <typeparam name="string">the name of the permission</typeparam>
        /// <typeparam name="Permissions">the permission class</typeparam>
        /// <returns></returns>
        public static Dictionary<string, Permissions> AllPermissions { get; private set; } = new Dictionary<string, Permissions>();

        /// <summary>
        /// Handles the datasets
        /// </summary>
        /// <value></value>
        public static DataSetManager DataSetManager { get; private set; }

        /// <summary>
        /// Contains meta data about all sheets on the server
        /// </summary>
        /// <value></value>
        public static SheetsServerManifest CurrentSheetsManifest { get; private set; }

        /// <summary>
        /// Handles incoming and outgoing user notification messages
        /// </summary>
        /// <value></value>
        public static UserPersistantNotificationsHandler UserMessageNotificationsHandler { get; private set; }

        /// <summary>
        /// The Card Actors
        /// </summary>
        /// <value></value>
        public static CardActors ActiveCardActors { get; private set; }

        /// <summary>
        /// Is a user logged in and connected to a server?
        /// </summary>
        /// <value></value>
        public static bool IsLoggedIn => CurrentActiveAccount != null && IsConnectedToServer;

        /// <summary>
        /// Ends the current session by preparing War Manager for termination
        /// </summary>
        private static void EndSession()
        {
            SelectedCardsListManager.OnCardDragChanged -= ActiveCardActors.OnDragCardChanged;

            UserPreferencesHandler.RememberSession(WarSystem.CurrentActiveAccount);

            Application.wantsToQuit -= AttemptQuit;
            if (SheetsManager.SheetCount < 1 && CurrentCategories == null)
            {

            }
            else
            {
                EmailClient.SendNotificationSMTPEmailToDev(CurrentActiveAccount.UserName + " - \'" + WarSystem.ConnectedServerName + "\'", "Logging out\n" +
                 CurrentActiveAccount.FirstName + " " + CurrentActiveAccount.LastName +
                  "\n" + WarSystem.ConnectedDeviceStamp, false);

                WriteToDev(CurrentActiveAccount.UserName + " is logging out.", WarManager.Logging.MessageType.logEvent);
                WriteToDev(DeveloperPushNotificationHandler.CreateLogoutMessage(), Logging.MessageType.logEvent);

                UserMessageNotificationsHandler.Close();

                CurrentCategories.Close();
                CurrentActiveAccount = null;

                if (DataSetManager != null)
                    DataSetManager.ClearDataSets();

                AllPermissions = new Dictionary<string, Permissions>();
                CurrentCategories = null;
                _defaultDataset = null;


                SheetsManager.ClearSheetDictionary();

                if (_logger != null)
                {
                    _logger.EndSession(false);
                }
            }
        }

        /// <summary>
        /// Displays the quit confirmation window
        /// </summary>
        public static bool AttemptQuit()
        {
            bool final = true;

            if (CurrentActiveAccount != null || SheetsManager.SheetCount > 0)
            {
                MessageBoxHandler.Print_Immediate("Would you like to quit? Unsaved changes will be lost", "Question", (x) =>
                {
                    if (x)
                    {
                        EndSession();
                        TerminateNormally();
                    }

                    final = x;
                });
            }

            return false;
        }

        private static void HandleQuit(bool canQuit)
        {
            if (canQuit)
            {
                TerminateNormally();
            }
        }

        /// <summary>
        /// Exposed for testing purposes, use 'AttemptQuit() instead'
        /// </summary>
        private static void TerminateNormally()
        {
            // Debug.Log("Terminating normally");
            Application.Quit();

#if UNITY_EDITOR
            // UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        static void OnApplicationQuit()
        {
            // Debug.Log("Application ending after " + Time.time + " seconds");
            Terminate(-2);
        }

        /// <summary>
        /// when an error occurs it will quit the application and do a few other system checks
        /// </summary>
        public static void Terminate(int code = -1)
        {
            Debug.Log("terminating");

            if (_logger != null)
            {
                _logger.EndSession(true, code.ToString());
            }

            EndSession();

            Application.Quit(code);

#if UNITY_EDITOR
            //EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// Write an event to the logger
        /// </summary>
        /// <param name="text">the message</param>
        /// <param name="type">the message type</param>
        public static void WriteToLog(string text, WarManager.Logging.MessageType type)
        {


            if (_logger == null)
                _logger = new LogHandler();

            _logger.NewMessage(text, type);

            if (type == Logging.MessageType.critical)
            {
                WriteToDev(text, type);

                if (DeveloperPushNotificationHandler != null)
                    DeveloperPushNotificationHandler.IncrementErrors();
            }

            if (type == Logging.MessageType.error && DeveloperPushNotificationHandler != null)
                DeveloperPushNotificationHandler.IncrementErrors();

        }

        /// <summary>
        /// Send a push notification to the developer
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void WriteToDev(string text, WarManager.Logging.MessageType type)
        {
            if (DeveloperPushNotificationHandler == null)
                DeveloperPushNotificationHandler = new GroupMeDeveloperPushNotificationHandler();

            DeveloperPushNotificationHandler.SendPushMessage(text, type);
        }

        /// <summary>
        /// Start a new session, makes the logged in account publicly accessible and starts the logger
        /// </summary>
        /// <param name="loggedInAccount">the account that was verified</param>
        public static void Login(Account loggedInAccount)
        {
            if (loggedInAccount == null)
                throw new ArgumentException("the logged in account is null");

            try
            {
                // WarManager.Special.IPAPI.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            GeneralSettingsSaver.Load();

            if (CurrentActiveAccount == null)
            {

                DeveloperPushNotificationHandler = new GroupMeDeveloperPushNotificationHandler();
                InitializeSession(loggedInAccount);

                return;
            }
            else
            {
                UnityEngine.Debug.Log("current active account is not null");
            }

            Terminate(-1);
        }

        /// <summary>
        /// Initialize the session (after logging in).
        /// </summary>
        /// <param name="account">the logged in account</param>
        private static void InitializeSession(Account account)
        {
            try
            {
                UserPreferencesHandler.GetMachineID();

                CardUtility.Init();

                Application.wantsToQuit += AttemptQuit;

                CurrentActiveAccount = account;


                Debug.Log("account initialized");

                if (AttemptConnectionToServer())
                {

                    // CurrentCategories = Categories.RefreshCategories(accnt);
                    // CurrentSheetsManifest = SheetsServerManifest.Refresh(null);

                    // var dataset = GetDefaultDataSetRules();
                    // Debug.Log(dataset.GetDatasetJson());


                    // RefreshLoadedDataSets(accnt);

                    Application.quitting += EndSession;

                    if (Application.systemLanguage != SystemLanguage.English)
                    {
                        NotificationHandler.Print("Unfortunately English is the only fully supported language at this time." +
                     " However you can change the language in the settings menu.");
                    }
                    else
                    {
                        //NotificationHandler.Print("English set as the default Language");
                    }

                    UserMessageNotificationsHandler = new UserPersistantNotificationsHandler();

                    if (UserMessageNotificationsHandler.Deserialize())
                    {
                        UserMessageNotificationsHandler.CreateBackUpFile();
                    }

                    SetGreeting(account);

                    ActiveCardActors = new CardActors();
                    SelectedCardsListManager.OnCardDragChanged += ActiveCardActors.OnDragCardChanged;

                    FormsController = new FormsController();

                    DataSetManager.SetDataSetViewsFromUserPreferences(AccountPreferences);

                    if (OnInit != null)
                        OnInit();
                }
                else
                {
                    Terminate(-2);
                }

            }
            catch (Exception ex)
            {
                EmailClient.SendNotificationSMTPEmailToDev("Error logging in -" + account.UserName, ex.Message);
                WarSystem.WriteToDev("Error logging in -" + account.UserName, Logging.MessageType.error);
            }

        }

        /// <summary>
        /// Does the software have access to the online server?
        /// </summary>
        /// <returns>returns true if there is access, false if not</returns>
        public static bool AttemptConnectionToServer()
        {

            if (GeneralSettings.Save_Location_Server == null || GeneralSettings.Save_Location_Server == string.Empty)
            {
                GeneralSettings.Save_Location_Server = PlayerPrefs.GetString("server root", "");
            }

            if (GeneralSettings.Save_Location_Server != string.Empty && Directory.Exists(GeneralSettings.Save_Location_Server + "\\"))
            {
                int x = HandleConnection(out string serverName);

                ConnectedServerName = serverName;

                if (x != 0)
                {
                    if (x != -2)
                    {
                        NotificationHandler.Print(Greeting + "it looks like you don't have the latest version. Please contact support for help.");
                        WriteToLog("The current build does not have the latest version. War Manager will not connect to the server...", Logging.MessageType.error);
                    }
                    else
                    {
                        NotificationHandler.Print(Greeting + "the server is currently down (probably for maintenance). Please try again later or contact support for help.");
                        WriteToLog("It seems that the server is down for maintenance. War Manager will not connect to the server...", Logging.MessageType.error);
                    }

                    RefreshConnectionStatus(false);
                    return false;
                }

                RefreshConnectionStatus(true);
                return true;
            }

            WriteToLog("Directory " + GeneralSettings.Save_Location_Server + "\\" + "does not exist.", Logging.MessageType.error);

            RefreshConnectionStatus(false);
            return false;
        }

        /// <summary>
        /// Refresh the system when the connection status has been updated
        /// </summary>
        /// <param name="connectionStatus">Is the war manager connected to the server?</param>
        private static void RefreshConnectionStatus(bool connectionStatus)
        {

            IsConnectedToServer = connectionStatus;

            if (_logger == null)
            {
                _logger = new LogHandler();
            }

            if (IsConnectedToServer)
            {
                WriteToLog("Connected to server " + ConnectedServerName, Logging.MessageType.logEvent);

            }
            else
            {
                WriteToLog("Could not connect and therefore refresh the data", Logging.MessageType.error);

            }

            if (IsConnectedToServer && CurrentActiveAccount != null)
            {
                LoadPermissions();
                CurrentCategories = Categories.RefreshCategories(CurrentActiveAccount);

                if (CurrentActiveAccount.FullAccessCategories != null && CurrentActiveAccount.FullAccessCategories.Length > 0)
                {
                    RefreshLoadedDataSets();
                    CurrentSheetsManifest = SheetsServerManifest.Refresh(CurrentSheetsManifest);
                }
            }

            if (OnConnectionToServerChanged != null)
                OnConnectionToServerChanged(IsConnectedToServer);
        }



        /// <summary>
        /// Load all the permissions
        /// </summary>
        public static void LoadPermissions()
        {
            AllPermissions = Permissions.PermissionsDictionary;
        }

        /// <summary>
        /// Set the greeting to be in the war manager notification handler (used for the start of the session).
        /// </summary>
        /// <param name="accnt">The account that has been loaded</param>
        public static void SetGreeting(Account accnt)
        {

            string[] greetings = new string[6] { "Hello", "Welcome", "Hey", "Hi", "It\'s", "It\'s that time, " };

            string permissionsString = "user";

            if (accnt.Permissions != (Permissions)null)
                permissionsString = accnt.Permissions.Name;

            var r = new System.Random();
            int x = r.Next(7);

            string greeting = "";

            string name = " " + accnt.FirstName + " " + accnt.LastName;

            if (x == 6)
            {
                if (DateTime.Now.Hour < 8)
                {
                    greeting = "Top of the Morning," + name;
                }
                else if (DateTime.Now.Hour < 12)
                {
                    greeting = "Good Morning," + name;
                }
                else
                {
                    if (((int)DateTime.Today.DayOfWeek) > 4 || ((int)DateTime.Today.DayOfWeek) == 0)
                    {
                        greeting = "Happy " + DateTime.Today.DayOfWeek.ToString() + ", " + name;
                    }
                    else
                    {
                        if (DateTime.Now.Hour < 17)
                        {
                            greeting = "Good Afternoon, " + name;
                        }
                        else if (DateTime.Now.Hour < 22)
                        {
                            greeting = "Good Evening," + name;
                        }
                        else
                        {
                            greeting = "Working late, " + name + " ? ";
                        }
                    }
                }
            }
            else
            {
                greeting = greetings[x] + name;
            }

            var messageLength = UserMessageNotificationsHandler.GetNotificationsRecieved(CurrentActiveAccount.UserName).Count;
            string messageMessage = "";

            if (messageLength < 1)
            {
                messageMessage = "No messages in your inbox";
            }
            else if (messageLength == 1)
            {
                messageMessage = "You have 1 message in your inbox";
            }
            else
            {
                messageMessage = "You have " + messageLength + " messages in your inbox";
            }

            string greet = greeting + " !\n\t\nUser: " + accnt.UserName + " \nRole: " + permissionsString + "\n" + messageMessage;

            // if (AccountPreferences != null && AccountPreferences.LastCurrentSheet != null && AccountPreferences.LastCurrentSheet != string.Empty)
            // {
            //     NotificationHandler.Print(greet, "Continue where you left off...", SheetsManager.AttemptLoadSheetPreferences);
            // }
            // else
            // {
            // NotificationHandler.Print(greet);
            // }

            //UserPreferencesHandler.AskAboutAutomaticLogin();
            NotificationHandler.Print(greet, "View Account", () => { ActiveSheetsDisplayer.main.ShowAccountInfo(true); });
        }

        /// <summary>
        /// Continue where you left off...
        /// </summary>
        /// <see cref="SheetsManager.RecordLastOpenedSheets"/>
        private static void OpenLastUsedSheets()
        {
            if (PlayerPrefs.HasKey("open-sheets"))
            {
                string ids = PlayerPrefs.GetString("open-sheets");

                string[] sheetIds = ids.Split(',');

                var manifestData = CurrentSheetsManifest.Sheets;

                int i = 0;

                foreach (var x in sheetIds)
                {

                    foreach (var fileControlData in manifestData)
                    {
                        if (fileControlData.Data.ID == x && !SheetsManager.IsSheetActive(x))
                        {
                            try
                            {
                                if (WarManager.Sharing.FileControl<SheetMetaData>.TryGetServerFile(fileControlData, WarSystem.ServerVersion, WarSystem.CurrentActiveAccount, out var file))
                                {
                                    string path = GeneralSettings.Save_Location_Server + @"\Sheets\" + file.SheetName + SheetsManager.CardSheetExtension;
                                    SheetsManager.OpenCardSheet(path, SheetsManager.SystemEncryptKey, out var id);
                                    return;
                                }
                                else
                                {
                                    NotificationHandler.Print("You do not have permission to view \'" + fileControlData.Data.SheetName + "\'");
                                }
                            }
                            catch (System.Exception ex)
                            {
                                NotificationHandler.Print("Error locating or opening sheet " + ex.Message);
                            }
                        }
                    }

                    i++;
                }


                //string lastSetCurrent = PlayerPrefs.GetString("last-set-current");
                //if (!string.IsNullOrEmpty(lastSetCurrent)) SheetsManager.SetSheetCurrent(lastSetCurrent);

            }
        }

        /// <summary>
        /// Load the data sets from the server if possible
        /// </summary>
        public static void RefreshLoadedDataSets()
        {
            if (!IsConnectedToServer)
                return;

            if (DataSetManager != null)
                DataSetManager.ClearDataSets();

            if (WarSystem.CurrentActiveAccount == null)
                throw new NullReferenceException("The account cannot be null when getting the associated datasets");

            DataSetManager = new DataSetManager();

            // Debug.Log("dataset manager created");
        }

        /// <summary>
        /// Get a list of data sets from an active sheet
        /// </summary>
        /// <param name="sheetId">the id of the sheet</param>
        /// <param name="alertUser">should the user be alerted when a data set is not found</param>
        /// <returns>returns a list of data sets, if no data sets are found the data set list is empty</returns>
        [Obsolete("Use DataSetManager.GetDataSetFromSheet() instead", true)]
        public static List<DataSet> GetDataSetsFromSheet(string sheetId, bool alertUser = true)
        {
            if (sheetId == null)
                throw new NullReferenceException("The sheet id cannot be null");

            if (sheetId == string.Empty)
            {
                throw new NotSupportedException("The sheet id cannot be empty");
            }

            List<DataSet> dataSets = new List<DataSet>();
            if (SheetsManager.TryGetActiveSheet(sheetId, out var sheet))
            {
                string[] dataSetIdArray = sheet.GetDatasetIDs();

                foreach (var id in dataSetIdArray)
                {
                    if (DataSetManager.TryGetDataset(id, out var set))
                    {
                        dataSets.Add(set);
                    }
                }
            }

            return dataSets;
        }



        /// <summary>
        /// Get the sheet meta data from a specific sheet that has been downloaded from the server location
        /// </summary>
        /// <param name="id">the id of the sheet to get</param>
        /// <param name="theSheet">the out param of the meta data sheet</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool GetSheetMetaData(string id, out SheetMetaData theSheet)
        {
            if (id == null)
                throw new NullReferenceException("The sheet id cannot be null");

            if (id == string.Empty)
            {
                throw new NotSupportedException("The sheet id cannot be empty");
            }

            if (CurrentSheetsManifest == null)
                throw new System.Exception("The curent sheets manifest has not been set up.");

            if (CurrentSheetsManifest.TryGetSheet(id, out var sheet))
            {
                theSheet = sheet;
                return true;
            }

            theSheet = null;
            return false;
        }

        /// <summary>
        /// Handles the connection to the server - from comparing the version of the software with the server 
        /// version, to finding the root path and server info file
        /// </summary>
        /// <returns>returns the connection type (int)</returns>
        public static int HandleConnection(out string serverName)
        {

            if (string.IsNullOrEmpty(GeneralSettings.Save_Location_Server))
                GeneralSettings.Save_Location_Server = PlayerPrefs.GetString("server root");

            string serverInfo = GeneralSettings.Save_Location_Server_ServerInfo;

            if (string.IsNullOrEmpty(serverInfo))
            {
                NotificationHandler.Print("could not find the server location - root connection string empty.");
                serverName = "";
                return -3;
            }

            // Debug.Log("searching for " + serverInfo);

            int i = 0;
            int res = 0;
            string streamText;

            string finalServerName = "";

            if (File.Exists(serverInfo))
            {
                using (FileStream stream = new FileStream(serverInfo, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string streamStr = "";

                        while (!reader.EndOfStream)
                        {
                            streamStr = reader.ReadLine();

                            streamText = streamStr;

                            if (i == 0)
                            {

                                res = streamText.CompareTo(ServerVersion);

                                if (res == 1)
                                    res = -1;

                                if (streamText == "down" || streamText == "maintenance" || streamText == string.Empty)
                                {
                                    // WriteToLog("Server status -2: Either down or maintenance", Logging.MessageType.critical); //cannot log when not connected to server???
                                    EmailClient.SendNotificationSMTPEmailToDev("Bat Error", "There was a bat error when attempting to connect to server", false);
                                    serverName = "";
                                    return -2;
                                }

                            }

                            if (i == 1)
                            {
                                finalServerName = streamText;
                            }

                            if (i == 2)
                            {
                                ConnectedServerColor = streamText;
                            }

                            if (i == 3)
                            {
                                ConnectedServerLogo = streamText;
                            }
                            i++;
                        }

                        serverName = finalServerName;

                        return res;
                    }
                }
            }
            else
            {
                Debug.Log("file does not exist " + serverInfo);
                serverName = "";
                return -4;
            }
        }

        public static bool TrySetNewRoot(string path)
        {
            try
            {
                if (File.Exists(path + GeneralSettings.GetServerInfoPathOnly))
                {
                    if (HandleConnection(out string serverName) == 1)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                return false;
            }

            return false;
        }
    }
}
