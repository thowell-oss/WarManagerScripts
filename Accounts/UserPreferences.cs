using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager.Sharing
{
    /// <summary>
    /// represents user's preferences
    /// </summary>
    [Notes.Author("The user preferences")]
    public class UserPreferences
    {
        /// <summary>
        /// the user name
        /// </summary>
        /// <value></value>
        [JsonPropertyName("user name")]
        public string UserName { get; set; } = "";

        /// <summary>
        /// The id of the account
        /// </summary>
        [JsonPropertyName("acct id")]
        public string AccountId { get; set; } = "";

        /// <summary>
        /// Should the user log out at the end of the session?
        /// </summary>
        [JsonPropertyName("always logged in?")]
        public string[] AlwaysLoggedInMachineIDs { get; set; } = new string[0];

        /// <summary>
        /// your phone number
        /// </summary>
        /// <value></value>
        [JsonPropertyName("phone")]
        public string PhoneNumber { get; set; } = "";

        /// <summary>
        /// the pinned sheets that user has at the top of the references slide window
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        [JsonPropertyName("pinned sheets")]
        public List<string> PinnedSheets { get; set; } = new List<string>();

        /// <summary>
        /// the sheets that were opened last time War Manager was being used
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        [JsonPropertyName("last opened sheets")]
        public List<string> LastOpenedSheets { get; set; } = new List<string>();

        /// <summary>
        /// Get the last opened sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("last current sheet")]
        public string LastCurrentSheet { get; set; } = "";

        /// <summary>
        /// Use the context buttons
        /// </summary>
        /// <value></value>
        [JsonPropertyName("use context buttons")]
        public bool UseContextButtons { get; set; } = true;

        /// <summary>
        /// tuple of selected views for each data set
        /// </summary>
        /// <value></value>
        [JsonPropertyName("selected views")]
        public Dictionary<string, string> SelectedViews { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Get last opened sheets
        /// </summary>
        /// <returns>returns the list of last opened sheets</returns>
        public List<SheetMetaData> GetLastOpenedSheets()
        {
            List<SheetMetaData> sheets = new List<SheetMetaData>();

            var closedSheets = WarSystem.CurrentSheetsManifest.GetClosedSheets(WarSystem.CurrentActiveAccount);

            if (LastOpenedSheets != null && LastOpenedSheets.Count > 0)
            {
                for (int i = 0; i < LastOpenedSheets.Count; i++)
                {
                    if (LastOpenedSheets[i] != null && LastOpenedSheets[i].Trim().Length > 0)
                    {
                        var sheet = closedSheets.Find(x => x.ID == LastOpenedSheets[i]);
                        sheets.Add(sheet);
                    }
                }
            }

            return sheets;
        }

        /// <summary>
        /// Get last current sheet  
        /// </summary>
        /// <returns></returns>
        public bool TryGetLastCurrentSheet(out SheetMetaData sheet)
        {
            if (LastCurrentSheet != null && LastCurrentSheet.Trim().Length > 0)
            {
                if (WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(LastCurrentSheet, out var result))
                {

                    if (FileControl<SheetMetaData>.TryGetServerFile(result, "v1", WarSystem.CurrentActiveAccount, out var file))
                    {
                        sheet = file;
                        return true;
                    }
                }
            }

            sheet = null;
            return false;
        }

        /// <summary>
        /// Get closed sheets that are apart of the pinned sheets category
        /// </summary>
        /// <returns></returns>
        public List<SheetMetaData> GetClosedPinnedSheets()
        {
            List<SheetMetaData> sheets = new List<SheetMetaData>();
            var closedSheets = WarSystem.CurrentSheetsManifest.GetClosedSheets(WarSystem.CurrentActiveAccount);

            if (PinnedSheets == null || PinnedSheets.Count < 1)
                return sheets;

            for (int i = 0; i < PinnedSheets.Count; i++)
            {
                if (PinnedSheets[i] != null && PinnedSheets[i].Trim() != string.Empty)
                {
                    var sheet = closedSheets.Find(x => x.ID == PinnedSheets[i]);

                    if (sheet != null)
                        sheets.Add(sheet);
                }
            }

            return sheets;
        }

        /// <summary>
        /// Get the active list of pinned sheets
        /// </summary>
        /// <returns>returns a list of active sheets</returns>
        public List<Sheet<Card>> GetActivePinnedSheets()
        {
            List<Sheet<Card>> sheets = new List<Sheet<Card>>();

            if (PinnedSheets == null || PinnedSheets.Count < 1)
                return sheets;

            for (int i = 0; i < PinnedSheets.Count; i++)
            {
                if (PinnedSheets[i] != null && PinnedSheets[i].Trim() != string.Empty)
                {
                    if (SheetsManager.TryGetActiveSheet(PinnedSheets[i], out var sheet))
                    {
                        sheets.Add(sheet);
                    }
                }
            }

            return sheets;
        }

        /// <summary>
        /// Save the statuses of the current opened sheets for next session
        /// </summary>
        public void SaveCurrentOpenedSheets()
        {
            LastOpenedSheets.Clear();

            foreach (var x in SheetsManager.Sheets)
            {
                LastOpenedSheets.Add(x.ID);
            }

            LastCurrentSheet = SheetsManager.CurrentSheetID;
        }

        /// <summary>
        /// Saves the current data set views for each dataset
        /// </summary>
        public void SaveCurrentDataSetSelectedViews()
        {
            // List<DataSetSelectedView> selectedViews = new List<DataSetSelectedView>();

            // foreach (var x in WarSystem.DataSetManager.Datasets)
            // {
            //     selectedViews.Add(new DataSetSelectedView(x.ID, x.SelectedView.ID));
            // }

            // SelectedViews = selectedViews;

            SelectedViews.Clear();

            foreach (var x in WarSystem.DataSetManager.Datasets)
            {
                SelectedViews.Add(x.ID, x.SelectedView.ID);
            }
        }
    }
}
