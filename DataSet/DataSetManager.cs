
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;
using WarManager.Sharing.Security;
using WarManager.Sharing;
using WarManager.Unity3D;
using System.Text;

using WarManager.Backend.CardsElementData;

namespace WarManager
{
    /// <summary>
    /// Handles utility functions for datasets.
    /// </summary>
    [Notes.Author("Handles utility functions for datasets.")]
    public class DataSetManager
    {
        /// <summary>
        /// The list of active datasets for the current session
        /// </summary>
        /// <typeparam name="DataSet"></typeparam>
        /// <returns></returns>
        private Dictionary<string, DataSet> _activeDatasets = new Dictionary<string, DataSet>();


        /// <summary>
        /// The IEnumerable of the loaded datasets
        /// </summary>
        /// <value></value>
        public IEnumerable<DataSet> Datasets
        {
            get
            {
                return _activeDatasets.Values;
            }
        }

        /// <summary>
        /// the IEnumerable of all the ids of the datasets
        /// </summary>
        /// <value></value>
        public IEnumerable<string> DataSetIds
        {
            get
            {
                return _activeDatasets.Keys;
            }
        }

        /// <summary>
        /// The count of all datasets contained in the Manager
        /// </summary>
        /// <value></value>
        public int DataSetCount
        {
            get
            {
                return _activeDatasets.Count;
            }
        }

        /// <summary>
        /// The system cards manager
        /// </summary>
        /// <value></value>
        public SystemCardsManager ActionCardsManager { get; private set; }


        /// <summary>
        /// /// Retrieve the accessible data sets and initalize the Data Set Manager Instance
        /// </summary>
        public DataSetManager()
        {
            RefreshDatasets();
            ActionCardsManager = new SystemCardsManager();
        }


        /// <summary>
        /// Import the datasets and check the permissions to make sure the new datasets match with the current loaded permissions
        /// </summary>
        private void RefreshDatasets()
        {
            var dataSets = LoadAllDataSets();

            List<string> dataSetIds = new List<string>();

            //Debug.Log("cleared datasets");

            _activeDatasets = new Dictionary<string, DataSet>();

            foreach (var controls in dataSets)
            {
                DataSet set;

                try
                {
                    if (FileControl<DataSet>.TryGetServerFile(controls, WarSystem.ServerVersion, WarSystem.CurrentActiveAccount, out set) || WarSystem.CurrentActiveAccount.IsAdmin)
                    {
                        if (set != null)
                        {
                            if (_activeDatasets.ContainsKey(set.ID))
                            {
                                var loadedSet = _activeDatasets[set.ID];
                                NotificationHandler.Print("Data Set " + set.DatasetName + " cannot be loaded because it contains the same fingerprint ID as " + loadedSet.ID);
                            }
                            else
                            {
                                //Debug.Log("loaded " + set.DatasetName);
                                _activeDatasets.Add(set.ID, set);
                            }

                            dataSetIds.Add(set.ID);

                            // Debug.Log(set.)
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Could not load Dataset " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds a dataset to the dictionary
        /// </summary>
        /// <param name="set"></param>
        public void AddDataset(DataSet set)
        {
            _activeDatasets.Add(set.ID, set);
        }

        /// <summary>
        /// Attempts to get a loaded dataset with the selected id
        /// </summary>
        /// <param name="id">the id of the dataset</param>
        /// <param name="set">the dataset</param>
        /// <returns>retruns true if the operation was successful, false if not</returns>
        public bool TryGetDataset(string id, out DataSet set)
        {

            if (id == null)
                throw new NullReferenceException("The id cannot be null");

            if (id.Trim() == "")
                throw new Exception("The id cannot be empty");


            if (id.StartsWith("sys"))
            {
                set = ActionCardsManager.ParseID(id, "").DataSet;
                return true;
            }

            if (_activeDatasets.TryGetValue(id, out var dataset))
            {
                set = dataset;
                return true;
            }

            set = WarSystem.DefaultDataSet;
            return false;
        }

        /// <summary>
        /// Try get the data set by the name of the data set - the values are case insensitive
        /// </summary>
        /// <param name="datasetName">the name of the dataset</param>
        /// <param name="set">the out parameter of the dataset</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool TryGetDataSetByName_CaseInsensitive(string datasetName, out DataSet set)
        {
            if (datasetName == null)
                throw new NullReferenceException("The id cannot be null");

            if (datasetName.Trim() == "")
                throw new Exception("The id cannot be empty");

            foreach (var x in Datasets)
            {
                if (x.DatasetName.Trim().ToLower() == datasetName.Trim().ToLower())
                {
                    set = x;
                    return true;
                }
            }

            set = null;
            return false;
        }

        /// <summary>
        /// Get a list of datasets (without the datasets given)
        /// </summary>
        /// <param name="difference">the list of datasets that should not appear in the list</param>
        public List<DataSet> GetDataSets(List<DataSet> difference)
        {
            if (difference == null)
                difference = new List<DataSet>();

            List<DataSet> results = new List<DataSet>();

            foreach (var x in Datasets)
            {
                var someDataSet = difference.Find(y => x.ID == y.ID);

                if (someDataSet == null && x != null)
                {
                    results.Add(x);
                }
            }

            return results;
        }

        /// <summary>
        /// Clears all datasets from the dictionary
        /// </summary>
        public void ClearDataSets()
        {
            _activeDatasets.Clear();
        }

        /// <summary>
        /// Does the dataset manager contain the Data Set ID?
        /// </summary>
        /// <param name="id">the id</param>
        /// <returns>returns true if the Data Set Manager contains the id</returns>
        public bool ContainsDataSetID(string id)
        {
            if (id == null)
                throw new NullReferenceException("the id cannot be null");

            if (id.Trim() == string.Empty)
                throw new ArgumentException("the id cannot be an empty string.");

            if (id.StartsWith("sys"))
                return true;

            return _activeDatasets.ContainsKey(id);
        }

        /// <summary>
        /// Get all the datasets in the server
        /// </summary>
        /// <returns></returns>
        private List<FileControl<DataSet>> LoadAllDataSets()
        {
            List<FileControl<DataSet>> fileControlDatasets = new List<FileControl<DataSet>>();

            if (!WarSystem.IsConnectedToServer)
                throw new System.NotSupportedException("Cannot access datasets when there is no server access");

            string[] paths = Directory.GetFiles(GeneralSettings.Save_Location_Server_Datasets);

            List<string> notedPaths = new List<string>();


            foreach (var path in paths)
            {
                DateTime creationTime = System.IO.File.GetCreationTime(path);
                DateTime openTime = System.IO.File.GetLastAccessTime(path);

                try
                {
                    string str = System.IO.File.ReadAllText(path);

                    var dataset = DataSet.CreateDataSetFromJson(str, path);

                    if (dataset != null)
                    {
                        string pathCheck = notedPaths.Find((x) => x == dataset.ID);

                        if (pathCheck == null)
                        {
                            FileControl<DataSet> newControl = DataSet.CreateFileControl(dataset, path, dataset.Owner, creationTime, openTime);

                            fileControlDatasets.Add(newControl);
                            notedPaths.Add(dataset.ID);
                        }
                        else
                        {
                            NotificationHandler.Print("There was a duplicate data set that could not be loaded - \'" + dataset.DatasetName + "\' (" + dataset.ID + ")");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    NotificationHandler.Print("Error: " + ex.Message);
                }
            }

            return fileControlDatasets;
        }

        /// <summary>
        /// Get the data sets from a selected card
        /// </summary>
        /// <param name="c">the card to get the dataset from</param>
        /// <returns>returns the dataset</returns>
        public DataSet GetDataSetFromCard(Cards.Card c) //do not try to get the data set from the card, that would be infinite recursion...
        {
            if (c == null)
                throw new NullReferenceException("the card cannot be null");

            if (string.IsNullOrEmpty(c.ID.Trim()))
            {
                throw new NullReferenceException("the card id cannot be null or empty");
            }

            if (string.IsNullOrEmpty(c.DatasetID.Trim()))
            {
                throw new NullReferenceException("the card dataset id cannot be null or empty");
            }

            if (TryGetDataset(c.DatasetID, out var dataset))
            {
                return dataset;
            }

            return null;
        }

        /// <summary>
        /// Get the data entry that the card represents
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>returns the data entry</returns>
        public DataEntry GetDataEntryFromCard(Cards.Card card) // do not get the entry from the card, that would be infinite recursion...
        {
            if (card == null)
                throw new NullReferenceException("The card cannot be null");

            if (card.DatasetID.StartsWith("sys"))
            {
                return ActionCardsManager.ParseID(card.DatasetID, card.RowID);
            }
            else
            {
                var set = card.DataSet;


                if (set == null)
                    throw new NullReferenceException("The data set cannot be null");

                var entryID = card.RowID;

                DataEntry entry = set.GetEntry(entryID);

                return entry;
            }
        }

        /// <summary>
        /// Get a list of data sets from an active sheet
        /// </summary>
        /// <param name="sheetId">the id of the sheet</param>
        /// <param name="alertUser">should the user be alerted when a data set is not found</param>
        /// <returns>returns a list of data sets, if no data sets are found the data set list is empty</returns>
        /// <exception cref="NullReferenceException">thrown if the sheet id is empty</exception>
        /// <exception cref="ArgumentException">thrown when the sheet id is empty</exception>
        public List<DataSet> GetDataSetsFromSheet(string sheetId, bool alertUser = true)
        {
            if (sheetId == null)
                throw new NullReferenceException("The sheet id cannot be null");

            if (sheetId == string.Empty)
            {
                throw new ArgumentException("The sheet id cannot be empty");
            }

            List<DataSet> dataSets = new List<DataSet>();
            if (SheetsManager.TryGetActiveSheet(sheetId, out var sheet))
            {
                string[] dataSetIdArray = sheet.GetDatasetIDs();

                foreach (var id in dataSetIdArray)
                {
                    if (TryGetDataset(id, out var set))
                    {
                        dataSets.Add(set);
                    }
                }
            }

            return dataSets;
        }

        /// <summary>
        /// Get the default card manual
        /// </summary>
        /// <returns>returns a default data set</returns>
        public static DataSet GetDefaultDataSetRules()
        {
            string path = GeneralSettings.Save_Location_Server_Datasets + @"\War System\Default.json";

            try
            {
                StringBuilder b = new StringBuilder();

                using (StreamReader str = new StreamReader(path))
                {
                    while (!str.EndOfStream)
                    {
                        b.Append(str.ReadLine());
                    }
                }

                string json = b.ToString();

                var dataset = DataSet.CreateDataSetFromJson(json, path);

                return dataset;
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);

                DataSetView m = new DataSetView();

                //var t = new WarManager.Backend.CardsElementData.CardElementData(0, "text", new int[2] { -12, 0 }, new int[2] { 0, 0 }, new int[2] { 1, 1 }, new int[1] { -1 }, new string[0], true, "12,black,center", null);
                //var b = new WarManager.Backend.CardsElementData.CardElementData(0, "background", new int[2] { 0, 0 }, new int[2] { 0, 0 }, new int[2] { 1, 1 }, new int[1] { -1 }, new string[0], true, "#ffeced,2,2", null);

                var text = new CardElementTextData()
                {
                    Location = new double[2] { -12, 0 },
                    FontSize = 12,
                    TextJustification = "center"
                };

                var background = new CardBackgroundElementData()
                {
                    ColorHex = "#eee",
                };

                m.ElementDataArray = new CardElementViewData[2] { text, background };

                List<DataSetView> views = new List<DataSetView>()
                {
                    m
                };

                List<string> catList = new List<string>();
                catList.Add("DEF");

                string colorString = ColorUtility.ToHtmlStringRGBA(Color.grey);

                DataSet default_DataSet = new DataSet("Default", "Def", new List<string>() { "*" }, "", new List<string>() { "" }, false, null, colorString, 1.0, 2, Guid.NewGuid().ToString(), catList, "none", views, new DataFileInstance());

                return default_DataSet;
            }
        }

        /// <summary>
        /// Remove a data set from the active data sets
        /// </summary>
        /// <param name="id">the id of the dataset</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool RemoveDataSet(string id)
        {
            if (id == null)
                throw new NullReferenceException("the id cannot be null.");

            if (id.Trim() == string.Empty)
                throw new ArgumentException("the id cannot be an empty string.");

            if (_activeDatasets.ContainsKey(id))
            {
                return _activeDatasets.Remove(id);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder dataSetNamesList = new StringBuilder();

            foreach (var x in Datasets)
            {
                dataSetNamesList.Append(x.DatasetName + ",");
            }

            dataSetNamesList = dataSetNamesList.Remove(dataSetNamesList.Length - 2, 1);
            return dataSetNamesList.ToString();

        }

        /// <summary>
        /// Set the selected views from the user preferences
        /// </summary>
        /// <param name="preferences">the user preferences</param>
        public void SetDataSetViewsFromUserPreferences(UserPreferences preferences)
        {
            SetDataSetViews(preferences.SelectedViews);
        }

        /// <summary>
        /// Takes a dictionary with the data set id as the key and the view id as the value, changes all views, and reloads after finishing
        /// </summary>
        /// <param name="datasetViewPairs">the dictionary</param>
        public void SetDataSetViews(Dictionary<string, string> datasetViewPairs)
        {
            foreach (var x in datasetViewPairs)
            {
                if (TryGetDataset(x.Key, out var set))
                {
                    try
                    {
                        set.SetView(WarSystem.CurrentActiveAccount, x.Value);
                    }
                    catch (Exception ex)
                    {
                        NotificationHandler.Print("Error setting views " + ex.Message);
                    }
                }
            }

            SheetsManager.ReloadCurrentSheet();
        }

        /// <summary>
        /// Add the DataSet to the list and save it to the server
        /// </summary>
        /// <param name="set">the Data Set</param>
        public void Add(DataSet set)
        {
            string key = set.ID;

            //save it and reload the dataset manager??

            throw new NotImplementedException();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return _activeDatasets.GetEnumerator();
        }
    }
}
