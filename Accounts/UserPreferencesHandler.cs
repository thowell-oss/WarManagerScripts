
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using System.IO;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WarManager.Backend;

namespace WarManager.Sharing
{
    [Notes.Author("Handles retrieving and changing user preferences")]
    public class UserPreferencesHandler
    {
        /// <summary>
        /// The file location of the server
        /// </summary>
        private static string _fileLocation = GeneralSettings.Save_Location_Server + @"\Data\War System\AcctPrefs.json";

        private static List<UserPreferences> _serverPreferences = new List<UserPreferences>();

        private static UserPreferences _prefs = null;

        /// <summary>
        /// Is the automatic login turned on?
        /// </summary>
        /// <value></value>
        public static bool AutomaticLoginEnabled { get; set; }

        /// <summary>
        /// The user preferences
        /// </summary>
        /// <value></value>
        public static UserPreferences Preferences
        {
            get
            {
                if (_prefs == null && WarSystem.IsConnectedToServer)
                {
                    _prefs = GetUserPreferences(WarSystem.CurrentActiveAccount);
                }
                else if (_prefs == null)
                {
                    throw new Exception("Not connected to server");
                }

                return _prefs;
            }
        }

        /// <summary>
        /// Refresh the preferences list
        /// </summary>
        public static void Refresh()
        {
            List<UserPreferences> prefs = new List<UserPreferences>();

            if (File.Exists(_fileLocation))
            {
                string file = "";

                using (FileStream stream = new FileStream(_fileLocation, FileMode.OpenOrCreate))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        file = reader.ReadToEnd();
                    }
                }

                if (file.Length > 0)
                {
                    using (JsonDocument document = JsonDocument.Parse(file))
                    {
                        foreach (var element in document.RootElement.EnumerateArray())
                        {
                            string userName = element.GetProperty("user name").GetString();
                            var ids = element.GetProperty("always logged in?");
                            string phone = element.GetProperty("phone").GetString();

                            JsonElement pinnedSheetsElement = element.GetProperty("pinned sheets");

                            JsonElement lastOpenedSheets = element.GetProperty("last opened sheets");
                            JsonElement currentOpenedSheet = element.GetProperty("last current sheet");

                            JsonElement accountIDElement = element.GetProperty("acct id");


                            List<string> idList = new List<string>();

                            foreach (var id in ids.EnumerateArray())
                            {
                                idList.Add(id.GetString());
                            }

                            List<string> sheetIDs = new List<string>();

                            foreach (var sheetId in pinnedSheetsElement.EnumerateArray())
                            {
                                sheetIDs.Add(sheetId.GetString());
                            }


                            List<string> lastSheetsID = new List<string>();
                            foreach (var x in lastOpenedSheets.EnumerateArray())
                            {
                                lastSheetsID.Add(x.GetString());
                            }

                            string lastCurrentOpenedSheet = currentOpenedSheet.GetString();


                            bool useContextButtons = false;
                            if (element.TryGetProperty("use context buttons", out var useButtons))
                            {
                                useContextButtons = useButtons.GetBoolean();
                            }
                            else
                            {
                                Debug.Log("cannot find the context buttons");
                            }



                            Dictionary<string, string> views = new Dictionary<string, string>();

                            if (element.TryGetProperty("selected views", out var viewsElement))
                            {
                                foreach (var x in viewsElement.EnumerateObject())
                                {
                                    string key = x.Name;
                                    string value = x.Value.GetString();

                                    views.Add(key, value);
                                }
                            }

                            prefs.Add(new UserPreferences()
                            {
                                UserName = userName,
                                AccountId = accountIDElement.GetString(),
                                AlwaysLoggedInMachineIDs = idList.ToArray(),
                                PhoneNumber = phone,
                                PinnedSheets = sheetIDs,
                                LastOpenedSheets = lastSheetsID,
                                LastCurrentSheet = lastCurrentOpenedSheet,
                                UseContextButtons = useContextButtons,
                                SelectedViews = views
                            });
                        }
                    }
                }
            }
            else
            {
                File.Create(_fileLocation);
            }

            _serverPreferences = prefs;

        }

        /// <summary>
        /// Get the user preferences
        /// </summary>
        public static UserPreferences GetUserPreferences(Account account)
        {
            Refresh();

            var preferences = _serverPreferences.Find((x) => x.AccountId == account.AccountID);

            if (preferences == null)
            {
                var x = new UserPreferences()
                {
                    AccountId = account.AccountID,
                    UserName = account.UserName
                };

                _serverPreferences.Add(x);

                return x;
            }
            else
            {
                return preferences;
            }
        }

        /// <summary>
        /// Save the preferences
        /// </summary>
        public static void SavePreferences()
        {
            string prefStr = JsonSerializer.Serialize<List<UserPreferences>>(_serverPreferences);


            File.Delete(_fileLocation);

            using (StreamWriter writer = File.CreateText(_fileLocation))
            {
                writer.Write(prefStr);
            }

        }

        /// <summary>
        /// Save the statuses of all the opened sheets
        /// </summary>
        /// <param name="preferences"></param>
        public static void RememberSession(Account account)
        {

            string phone = Preferences.PhoneNumber;

            var prefs = GetUserPreferences(account);

            if (phone != null)
                prefs.PhoneNumber = phone;

            if (AutomaticLoginEnabled && !prefs.AlwaysLoggedInMachineIDs.Contains(GetMachineID()))
            {
                List<string> ids = new List<string>();
                ids.AddRange(prefs.AlwaysLoggedInMachineIDs);
                ids.Add(GetMachineID());
                prefs.AlwaysLoggedInMachineIDs = ids.ToArray();
            }
            else if (!AutomaticLoginEnabled && prefs.AlwaysLoggedInMachineIDs.Contains(GetMachineID()))
            {
                List<string> ids = new List<string>();
                ids.AddRange(prefs.AlwaysLoggedInMachineIDs);
                ids.Remove(GetMachineID());
                prefs.AlwaysLoggedInMachineIDs = ids.ToArray();
            }


            prefs.SaveCurrentOpenedSheets();
            prefs.SaveCurrentDataSetSelectedViews();
            SavePreferences();
        }

        /// <summary>
        /// Get preferences with this machine ID
        /// </summary>
        /// <returns>an array of preferences</returns>
        public static UserPreferences[] GetPrefsWithThisMachineID()
        {
            string id = GetMachineID();
            Refresh();

            List<UserPreferences> prfs = _serverPreferences.FindAll(x => x.AlwaysLoggedInMachineIDs.Contains(id));
            return prfs.ToArray();
        }

        /// <summary>
        /// Get the ID of this machine using player prefs
        /// </summary>
        /// <returns>returns the id of this machine</returns>
        public static string GetMachineID()
        {
            string key = "machine-id";

            if (!PlayerPrefs.HasKey(key))
            {
                string str = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(key, str);
                return str;
            }

            return PlayerPrefs.GetString(key);
        }

        public static void CheckAutomaticLogin()
        {
            string machineID = GetMachineID();

            if (GetPrefsWithThisMachineID().Length == 0 && !Preferences.AlwaysLoggedInMachineIDs.Contains(machineID))
            {
                AutomaticLoginEnabled = false;
            }


            if (Preferences.AlwaysLoggedInMachineIDs.Contains(machineID))
            {
                AutomaticLoginEnabled = true;
            }
        }

        public static void AskAboutAutomaticLogin()
        {
            string machineID = GetMachineID();

            if (GetPrefsWithThisMachineID().Length == 0 && !Preferences.AlwaysLoggedInMachineIDs.Contains(machineID))
            {
                AutomaticLoginEnabled = false;

                NotificationHandler.Print("Turn on automatic login?", "Yep", () =>
                {
                    var prefs = GetUserPreferences(WarSystem.CurrentActiveAccount); //double check initialization

                    List<string> ids = new List<string>();
                    ids.AddRange(Preferences.AlwaysLoggedInMachineIDs);

                    if (!ids.Contains(machineID))
                    {
                        ids.Add(machineID);

                        Preferences.AlwaysLoggedInMachineIDs = ids.ToArray();
                        SavePreferences();

                        LeanTween.delayedCall(.5f, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Automatic login is now turned on. You will skip the login screen next time War Manager boots up.", "Note");
                            AskAboutAutomaticLogin();
                        });
                    }
                    else
                    {
                        LeanTween.delayedCall(.5f, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Automatic login is already turned on.", "Note");
                        });
                    }
                });
            }


            if (Preferences.AlwaysLoggedInMachineIDs.Contains(machineID))
            {
                AutomaticLoginEnabled = true;

                NotificationHandler.Print("Turn off automatic login?", "Yep", () =>
                {
                    var prefs = GetUserPreferences(WarSystem.CurrentActiveAccount);

                    List<string> ids = new List<string>();
                    ids.AddRange(prefs.AlwaysLoggedInMachineIDs);

                    if (ids.Contains(machineID))
                    {
                        ids.Remove(machineID);
                        Preferences.AlwaysLoggedInMachineIDs = ids.ToArray();
                        SavePreferences();

                        LeanTween.delayedCall(.5f, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Automatic login is now turned off. You will need your password the next time War Manager boots up.", "Note");
                            AskAboutAutomaticLogin();
                        });
                    }
                    else
                    {
                        LeanTween.delayedCall(.5f, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Automatic login is already turned off.", "Note");
                        });
                    }
                });
            }
        }

        /// <summary>
        /// enable the automatic login
        /// </summary>
        public static void EnableAutomaticLogin()
        {
            if (!AutomaticLoginEnabled)
            {
                string machineID = GetMachineID();
                var prefs = GetUserPreferences(WarSystem.CurrentActiveAccount); //double check initialization

                List<string> ids = new List<string>();
                ids.AddRange(Preferences.AlwaysLoggedInMachineIDs);

                if (!ids.Contains(machineID))
                {
                    ids.Add(machineID);

                    prefs.AlwaysLoggedInMachineIDs = ids.ToArray();
                    SavePreferences();

                    Debug.Log("added automatic login");

                    LeanTween.delayedCall(.5f, () =>
                    {
                        MessageBoxHandler.Print_Immediate("Automatic login is now turned on. You will skip the login screen next time War Manager boots up.", "Note");
                        //AskAboutAutomaticLogin();
                    });
                }

                AutomaticLoginEnabled = true;
            }
        }

        /// <summary>
        /// Disable the automatic login
        /// </summary>
        public static void DisableAutomaticLogin()
        {
            if (AutomaticLoginEnabled)
            {
                string machineID = GetMachineID();
                var prefs = GetUserPreferences(WarSystem.CurrentActiveAccount);

                List<string> ids = new List<string>();
                ids.AddRange(prefs.AlwaysLoggedInMachineIDs);

                if (ids.Contains(machineID))
                {
                    ids.Remove(machineID);
                    prefs.AlwaysLoggedInMachineIDs = ids.ToArray();
                    SavePreferences();

                    Debug.Log("removed automatic login");

                    LeanTween.delayedCall(.5f, () =>
                    {
                        MessageBoxHandler.Print_Immediate("Automatic login is now turned off. You will need your password the next time War Manager boots up.", "Note");
                        //AskAboutAutomaticLogin();
                    });
                }
                AutomaticLoginEnabled = false;
            }
        }
    }
}
