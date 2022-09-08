/* DataSet.cs
 * Author: Taylor Howell
 */

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

using UnityEngine;

using WarManager;
using WarManager.Sharing;

using Notes;

using StringUtility;
using WarManager.Backend.CardsElementData;

namespace WarManager.Backend
{
    /// <summary>
    /// Contains a list of data and instructions for how the data should be displayed - this is the 'lense' or meta-data processor
    /// </summary>
    [Author(8.13, "Contains a list of data and instructions for how the data should be displayed - this is the 'lense' or meta-data processor")]
    public class DataSet : IEquatable<DataSet>, IComparable<DataSet>
    {
        /// <summary>
        /// The name of the dataset
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*dataset name")]
        public string DatasetName { get; private set; }

        [JsonPropertyName("version")]
        public double Version { get; private set; } = 1.0;

        [JsonPropertyName("json_version")]
        public double JsonVersion { get; private set; } = 2.0;

        /// <summary>
        /// The ID of the dataset
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string ID { get; private set; }

        /// <summary>
        /// The owner of the dataset
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*owner")]
        public string Owner { get; private set; }

        /// <summary>
        /// the allowed tags
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public DataSetTags AllowedTagsHandler { get; private set; } = new DataSetTags();

        /// <summary>
        /// The tags the dataset is allowed to display
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        [JsonPropertyName("*allowed tags")]
        public List<string> AllowedTags
        {
            get
            {
                return AllowedTagsHandler.SerializedTags;
            }
        }


        [JsonPropertyName("*print format")]
        public string PrintFormat { get; private set; } = "";


        /// <summary>
        /// The printable tags handler
        /// </summary>
        [JsonIgnore]
        public DataSetTags PrintableTagsHandler { get; private set; } = new DataSetTags();


        /// <summary>
        /// The tags that are allowed to be printed
        /// </summary>
        [JsonPropertyName("*printable tags")]
        public List<string> PrintableTags
        {
            get
            {
                return PrintableTagsHandler.SerializedTags;
            }
        }


        /// <summary>
        /// Dictates if the user can edit the data with the selected view? or simply just view it?
        /// </summary>
        [JsonPropertyName("*edit data?")]
        public bool CanEditData
        {
            get => SelectedView.CanEditData;
        }

        /// <summary>
        /// Can the user edit cards with the selected view?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool CanEditCards
        {
            get => SelectedView.CanEditCard;
        }

        [JsonPropertyName("*default search filter")]
        public string SearchFilterString { get; private set; } = "-";

        /// <summary>
        /// The data set color values
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Color Color { get; set; }

        /// <summary>
        /// The html string conversion of the data set color
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*color")]
        public string DataSetColorString
        {
            get
            {
                return ColorUtility.ToHtmlStringRGBA(Color);
            }
            set
            {
                Color x = Color.grey;
                ColorUtility.TryParseHtmlString(value, out x);
                Color = x;
            }
        }

        /// <summary>
        /// The file location of the dataset
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string DataSetLocation { get; private set; }

        /// <summary>
        /// The categories the data set is associated to
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        [JsonIgnore]
        private List<string> _categories = new List<string>();

        [JsonPropertyName("*categories")]
        public string[] Categories
        {
            get
            {
                if (_categories == null || _categories.Count < 1)
                {
                    _categories = new List<string>();
                    _categories.Add("*");
                }

                return _categories.ToArray();
            }
        }

        /// <summary>
        /// The location of the list of data
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*data path")]
        public string DataListLocation { get; private set; }

        /// <summary>
        /// Instructions for how the card should look/function
        /// </summary>
        /// <value></value>
        [JsonPropertyName("format")]
        public DataSetView SelectedView { get; private set; }

        /// <summary>
        /// All the Card Manual Views (for JSON parser - use 'get allowed card manuals' instead)
        /// </summary>
        [JsonPropertyName("views")]
        public List<DataSetView> Views { get; protected set; }

        /// <summary>
        /// The Data Entry Count
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public int DataCount
        {
            get
            {
                return _fileEntries.Count;
            }
        }

        /// <summary>
        /// The Generated data displaying what kind of information is in each column
        /// </summary>
        /// <typeparam name="ColumnInfo">the class that holds this information</typeparam>
        /// <returns></returns>
        [JsonIgnore]
        private List<ColumnInfo> DataFileColumnInfo = new List<ColumnInfo>();

        /// <summary>
        /// Convert the column info list into json data
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("column info")]
        private string[] ColumnInfoJson => ColumnInfo.GetJson(DataFileColumnInfo);

        /// <summary>
        /// The data file
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DataFileInstance DataFile { get; private set; }

        /// <summary>
        /// the array of strings (tags) that is the header row for the data from the csv file
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string[] Tags
        {
            get
            {
                return DataFile.Header;
            }
        }


        /// <summary>
        /// Values with no information in them in the correct column order with column data (if possible)
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DataValue[] TemplateValueArray { get; private set; }

        /// <summary>
        /// the list of file entries
        /// </summary>
        /// <typeparam name="DataEntry"></typeparam>
        /// <returns></returns>
        [JsonIgnore]
        private Dictionary<string, DataEntry> _fileEntries = new Dictionary<string, DataEntry>();

        /// <summary>
        /// Get the IEnumerable of Data Entries
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public IEnumerable<DataEntry> Entries
        {
            get
            {
                return _fileEntries.Values;
            }
        }

        /// <summary>
        /// Search for items in the data set using this dictionary
        /// </summary>
        /// <typeparam name="string[]"></typeparam>
        /// <typeparam name="DataEntry"></typeparam>
        /// <returns></returns>
        [JsonIgnore]
        public Dictionary<IList<string>, DataEntry> SearchableList { get; private set; } = new Dictionary<IList<string>, DataEntry>();

        /// <summary>
        /// Data Set Constructor
        /// </summary>
        /// <param name="name">the user defined name of the dataset</param>
        /// <param name="owner">the owner and creator of the dataset</param>
        /// <param name="allowedTags">the tags the users are allowed to see</param>
        /// <param name="printFormat">the type of block this dataset will print after</param>
        /// <param name="printableTags">the tags (or headers) within the data that are allowed to be printed out</param>
        /// <param name="canEditData">can the user edit the data?</param>
        /// <param name="searchFilter">the string of data to automatically fill in the filter box when searching for data</param>
        /// <param name="htmlHexCodeColor">the html string hexcode color of the data when showing it on the card</param>
        /// <param name="version">the dataset version</param>
        /// <param name="jsonVersion">the json version</param>
        /// <param name="id">the id of the dataset</param>
        /// <param name="categories">the category types associated with the dataset (ex departments)</param>
        /// <param name="dataSetFilePath">The file location of the dataset</param>
        /// <param name="views">The views handle the rulesets for how War Manager behaves with the data</param>
        /// <param name="file">the file containing a reference to all data</param>
        /// <param name="columnInfos">the list of data instances that give information on the type of data</param>
        /// <param name="dataSetFilePathLocation">the location of the data</param>
        /// <exception cref="NullReferenceException">Null reference exception thrown if any nullable perameters can be null</exception>
        /// <exception cref="ArgumentException">Exception thrown if some arguments, like any list or arrays do not contain any elements</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception thrown if the version is less than zero</exception>
        /// <remarks>Creates the dataset which is the brain for making decisions on how War Manager should interperate the imported data</remarks>
        public DataSet(string name, string owner, List<string> allowedTags, string printFormat, List<string> printableTags, bool canEditData, string searchFilter,
         string htmlHexCodeColor, double version, double jsonVersion, string id, List<string> categories, string dataSetFilePath, List<DataSetView> views,
          DataFileInstance file, List<ColumnInfo> columnInfos = null, string dataSetFilePathLocation = null)
        {
            if (name == null)
                throw new NullReferenceException("the name cannot be null");

            if (owner == null)
                throw new NullReferenceException("the owner cannot be null");

            if (categories == null)
                throw new NullReferenceException("the categories cannot be null");

            if (categories.Count < 1)
                throw new ArgumentException("the categories list must be greater than zero");

            if (htmlHexCodeColor == null || htmlHexCodeColor == string.Empty)
                htmlHexCodeColor = "#000"; //black

            if (version < 0) throw new ArgumentOutOfRangeException("version", "the version cannot be less than zero");

            if (id == null) throw new NullReferenceException("The id cannot be less than zero");

            if (id.Length < 20) throw new ArgumentException("the id length must be greater than or equal to 20");

            if (dataSetFilePath == null)
                throw new NullReferenceException("the data set file path cannot be null");

            if (file == null)
                throw new NullReferenceException("the file cannot be null");

            DatasetName = name;
            Owner = owner;

            if (allowedTags == null)
                throw new NullReferenceException("The tags list cannot be null");
            AllowedTagsHandler = new DataSetTags(allowedTags);

            if (printFormat == null)
                throw new NullReferenceException("the print format cannot be null");
            PrintFormat = printFormat;

            if (PrintableTags == null)
                throw new NullReferenceException("the printable tags list cannot be null");
            PrintableTagsHandler = new DataSetTags(printableTags);

            if (views == null)
                throw new NullReferenceException("The views cannot be null");

            if (views.Count < 1)
                throw new ArgumentException("the views list cannot be less than one");


            SearchFilterString = searchFilter;

            DataSetColorString = htmlHexCodeColor;

            Version = version;

            DataFile = file;

            if (id == null || id == string.Empty || id.Length < 10)
                id = Guid.NewGuid().ToString();

            ID = id;

            _categories = categories;

            SelectedView = views[0];

            Views = views;

            DataFileColumnInfo = columnInfos;

            PopulateDataEntries();
            PopulateSearchDictionary();

            DataSetLocation = dataSetFilePathLocation;
        }

        /// <summary>
        /// Get the dataset for the actor
        /// </summary>
        /// <param name="dataSetId">the id of the actor</param>
        /// <param name="elementData">the element data</param>
        /// <param name="data">the data file instance</param>
        /// <param name="allowedTags">the tags allowed to be viewed/edited</param>
        /// <returns>returns the data set</returns>
        public static DataSet GetDataSet(string dataSetId, List<DataValue> datavalues, List<CardElementViewData> elementData, DataFileInstance data, List<string> allowedTags)
        {

            List<ColumnInfo> infos = new List<ColumnInfo>();

            for (int i = 0; i < datavalues.Count; i++)
            {
                ColumnInfo info = datavalues[i].GetColumnInfo();
                infos.Add(info);
            }

            var set = new DataSet("System", "War Manager Assistant", allowedTags, string.Empty,
                new List<string>(), true, string.Empty, "#D90000", 1.0, 2.0, dataSetId, new List<string>() { "Default" }, string.Empty,
                new List<DataSetView>() { new DataSetView("System", "System View", "This view is for the system cards", Guid.NewGuid().ToString(),
                true, true, elementData, new string[1] {"Default"},
                true, 1)}, data, infos);

            return set;
        }

        /// <summary>
        /// Replace the data with a new data file instance (expensive operation)
        /// </summary>
        /// <param name="file">the new data file</param>
        public void ReplaceData(DataFileInstance file)
        {
            if (file == null)
                throw new NullReferenceException("the file cannot be null");

            DataFile = file;

            _fileEntries.Clear();

            WarSystem.WriteToLog($"{DatasetName} data replaced by {WarSystem.CurrentActiveAccount.UserName}", Logging.MessageType.logEvent);
            WarSystem.DeveloperPushNotificationHandler.ReplacedData = true;

            PopulateDataEntries();
            PopulateSearchDictionary();

            SaveAndReloadDataSet();
        }

        /// <summary>
        /// Get the specific data by row
        /// </summary>
        /// <param name="rowId">The location or ID of the data pieces </param>
        /// <returns></returns>
        public DataPiece GetData(int rowId)
        {

            if (DataFile == null || DataFileInstance.IsEmptyHeader(DataFile))
            {
                NotificationHandler.Print("Data file not parsed for " + DatasetName);
                return new DataPiece(-1, new string[0], new string[0], ID);
            }

            DataPiece p;

            if (DataFile.TryGetData(rowId, out var data))
            {
                p = new DataPiece(rowId, data, DataFile.Header, ID);
            }
            else
            {
                p = new DataPiece(-1, new string[0], new string[0], ID);
            }

            return p;
        }

        /// <summary>
        /// Create the data entries
        /// </summary>
        private void PopulateDataEntries()
        {
            if (DataFile == null || DataFileInstance.IsEmptyHeader(DataFile))
            {
                NotificationHandler.Print("Data file not parsed for " + DatasetName);
                _fileEntries = new Dictionary<string, DataEntry>();
            }

            // string rowId = Guid.NewGuid().ToString();

            foreach (var d in DataFile.Data)
            {
                string id = string.Empty;
                if (DataFile.DataCount > 0)
                {
                    if (DataFile.Header[0].ToUpper().Contains("ID"))
                    {
                        if (d.Length > 0)
                        {
                            if (string.IsNullOrEmpty(d[0]))
                            {
                                id = Guid.NewGuid().ToString();
                            }
                            else
                            {

                                // if (int.TryParse(d[0], System.Globalization.NumberStyles.Integer,
                                // System.Globalization.CultureInfo.InvariantCulture, out int actualID))
                                // {
                                id = d[0];

                                // Debug.Log("getting actual ID " + id + " " + DatasetName);
                                // }
                            }
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Cannot open an empty data file");
                }

                try
                {
                    _fileEntries.Add(id, CreateDataEntry(id, DataFile.Header, d));
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print("Error adding Data Entry to " + DatasetName + " " + ex.Message);
                    _fileEntries.Add(Guid.NewGuid().ToString(), new DataEntry(this));
                }

                //rowId++;
            }

            // UnityEngine.Debug.Log("entries created: " + rowId);
            //UnityEngine.Debug.Log($"dataset {DatasetName} " + string.Join("\n\n", ColumnInfoJson));
        }

        /// <summary>
        /// Get the data entry from a specific row
        /// </summary>
        /// <param name="rowId">the row location</param>
        /// <returns>returns a new data entry</returns>
        private DataEntry CreateDataEntry(string rowId, string[] header, string[] data)
        {
            if (DataFile == null || DataFileInstance.IsEmptyHeader(DataFile) || data == null || data.Length < 1)
            {
                NotificationHandler.Print("Data file not parsed for " + DatasetName);
                return new DataEntry();
            }

            List<DataValue> values = new List<DataValue>();


            if (DataFileColumnInfo == null || DataFileColumnInfo.Count < 1)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    ColumnInfo info = new ColumnInfo(i, GetValuePermissions(header[i]), header[i], "", ColumnInfo.GetValueTypeOfParagraph);

                    DataFileColumnInfo.Add(info);

                    var value = new DataValue(rowId, data[i], info);
                    values.Add(value);

                    // Debug.Log("Data file column info is null or the list is empty");

                }
            }
            else
            {

                DataFileColumnInfo.Sort();

                for (int i = 0; i < data.Length; i++)
                {
                    ColumnInfo colInfo = DataFileColumnInfo.Find(x => x.ColumnLocation == i);

                    if (colInfo != null)
                    {
                        ColumnInfo info = null;

                        if (colInfo.KeywordValues.Length > 0)
                            info = new ColumnInfo(colInfo.ColumnLocation, colInfo.Permissions, colInfo.HeaderName, colInfo.Description, colInfo.ValueType, colInfo.KeywordValues);
                        else
                            info = new ColumnInfo(colInfo.ColumnLocation, colInfo.Permissions, colInfo.HeaderName, colInfo.Description, colInfo.ValueType);

                        var value = new DataValue(rowId, data[i], info);
                        values.Add(value);

                        //Debug.Log(DatasetName + ": found column info - creating new one and adding to list \n\n" + ColumnInfo.GetJson(colInfo));

                    }
                    else
                    {
                        colInfo = new ColumnInfo(i, GetValuePermissions(header[i]), header[i], "", ColumnInfo.GetValueTypeOfParagraph);

                        ColumnInfo info = null;

                        if (colInfo.KeywordValues.Length > 0)
                            info = new ColumnInfo(colInfo.ColumnLocation, colInfo.Permissions, colInfo.HeaderName, colInfo.Description, colInfo.ValueType, colInfo.KeywordValues);
                        else
                            info = new ColumnInfo(colInfo.ColumnLocation, colInfo.Permissions, colInfo.HeaderName, colInfo.Description, colInfo.ValueType);

                        var value = new DataValue(rowId, data[i], info);

                        DataFileColumnInfo.Add(colInfo);

                        values.Add(value);

                        //Debug.Log( DatasetName + ": captured column info - creating new one and adding to list " + i);
                    }
                }
            }

            var entry = new DataEntry(rowId, values.ToArray(), this);

            foreach (var value in values)
            {
                value.GetColumnInfo().Entry = entry;
            }

            if (TemplateValueArray == null)
            {
                SetTemplateValueArray(values);
            }

            return entry;
        }

        /// <summary>
        /// Populate the search dictionary
        /// </summary>
        public void PopulateSearchDictionary()
        {
            SearchableList.Clear();

            foreach (DataEntry x in Entries)
            {
                SearchableList.Add(x.GetAllowedValues(), x);
            }
        }

        /// <summary>
        /// Set the template value array
        /// </summary>
        /// <param name="values">a sample list of used values</param>
        public void SetTemplateValueArray(List<DataValue> values)
        {
            TemplateValueArray = new DataValue[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                DataValue v = DataValue.Copy(values[i]);
                v.Clear();
                TemplateValueArray[i] = v;
            }
        }


        /// <summary>
        /// Get a new template data entry
        /// </summary>
        /// <param name="row">the row location of the data entry (set to -1)</param>
        /// <returns>Returns the template data entry</returns>
        public DataEntry GetTemplateDataEntry(string row)
        {
            return new DataEntry(row, TemplateValueArray, this);
        }

        /// <summary>
        /// Get the data entry
        /// </summary>
        /// <param name="rowId">the row id location of the data entry </param>
        /// <returns></returns>
        public DataEntry GetEntry(string rowId)
        {

            if (rowId == null)
            {
#if UNITY_EDITOR
                Debug.Log("the row id cannot be null");
                throw new NullReferenceException("the row id cannot be null");
#endif
            }

            //Debug.Log("issue with data set adding + 1");

            // var entry = _fileEntries.Find(x => x.RowID == rowId);

            if (_fileEntries.TryGetValue(rowId, out var entry))
            {

                return entry;
            }
            else
            {
                //Debug.LogError("could not find entry");
                throw new ArgumentException("Cannot find entry with id " + rowId + " " + DatasetName);
            }

            // if (_fileEntries.Count > rowId && rowId >= 0)
            //     return _fileEntries[rowId];

            // return new DataEntry();
        }

        /// <summary>
        /// Does the data set contain the row id?
        /// </summary>
        /// <param name="rowId">the row id</param>
        /// <returns>returns true if the row id is found, false if not</returns>
        public bool EntryExists(string rowId)
        {
            return _fileEntries.ContainsKey(rowId);
        }

        public override string ToString()
        {
            return DatasetName + " " + ID;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool Equals(DataSet other) // IEquatable
        {
            if (other.ID == ID)
                return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(DataSet a, DataSet b)
        {
            if ((object)a == null)
            {
                if ((object)b == null)
                    return true;
                return false;
            }

            if ((object)b == null)
                return false;


            if (a.ID == b.ID)
                return true;

            return false;
        }

        public static bool operator !=(DataSet a, DataSet b)
        {
            return !(a == b);
        }

        public int CompareTo(DataSet other) // IComparable
        {
            return ID.CompareTo(other.ID);
        }

        /// <summary>
        /// Get the json string representation of the dataset
        /// </summary>
        /// <returns></returns>
        public string GetDatasetJson()
        {
            string str = JsonSerializer.Serialize<DataSet>(this);
            return str;
        }

        /// <summary>
        /// Checks to see if the tag is in the allowed tags list set by the user
        /// </summary>
        /// <param name="tagName">the given tag to check for</param>
        /// <returns>returns true if the tag is found, false if not</returns>
        public bool IsAllowedTag(string tagName)
        {
            foreach (var tag in AllowedTags)
            {
                if (tag.Trim() == tagName)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Get the value permissions for a specific tag name
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public ValuePermissions GetValuePermissions(string tagName)
        {
            var res = ValuePermissions.None;

            if (IsAllowedTag(tagName))
            {
                if (CanEditData)
                {
                    res = ValuePermissions.Full;
                }
                else
                {
                    res = ValuePermissions.ViewOnly;
                }
            }

            return res;
        }


        /// <summary>
        /// Create a new dataset using a given json string
        /// </summary>
        /// <param name="json">the given json string</param>
        /// <returns></returns>
        public static DataSet CreateDataSetFromJson(string json, string datasetFilePath)
        {
            if (json == null || json == string.Empty)
                throw new NotSupportedException("json must not be null or empty");

            DataSet newDataset = null;

            string dataSetName = "";

            try
            {
                using (var document = JsonDocument.Parse(json))
                {
                    JsonElement root = document.RootElement;

                    string datasetName = GetStringWrapper(root, "*dataset name", "nothing", true);
                    dataSetName = datasetName;

                    double vrsn = GetDoubleWrapper(root, "version", dataSetName, true);

                    double jsonVersion = GetDoubleWrapper(root, "json_version");

                    string datasetid = GetStringWrapper(root, "id", dataSetName, true);

                    if (string.IsNullOrEmpty(datasetid))
                        datasetid = Guid.NewGuid().ToString();

                    string owner = GetStringWrapper(root, "*owner", dataSetName, true);


                    List<string> tags = new List<string>();

                    if (TryGetElementWrapper(root, "*allowed tags", out JsonElement tagsElement, dataSetName, true))
                    {
                        foreach (var tag in tagsElement.EnumerateArray())
                        {
                            tags.Add(tag.GetString());
                        }
                    }


                    string color = GetStringWrapper(root, "*color", dataSetName, true);

                    List<string> categoriesList = new List<string>();

                    if (TryGetElementWrapper(root, "*categories", out JsonElement element, dataSetName, true))
                    {
                        foreach (var cat in element.EnumerateArray())
                        {
                            categoriesList.Add(cat.GetString());
                        }
                    }

                    string dataFilePath = GetStringWrapper(root, "*data path", datasetName, true);

                    FileInfo info = new FileInfo(dataFilePath);

                    if (dataFilePath != "DEFAULT")
                    {
                        if (!System.IO.File.Exists(dataFilePath))
                        {
                            throw new FileNotFoundException("Could not find file with path " + info.Name + " (" + datasetName + ")");
                        }
                        else if (info.Extension != ".csv")
                        {
                            throw new FileLoadException("The data csv file is not in the correct format " + info.Extension + " (" + datasetName + ")");
                        }
                    }

                    #region old

                    // JsonElement Jname = root.GetProperty("*dataset name");
                    // string datasetName = Jname.GetString();
                    // dataSetName = datasetName;

                    // JsonElement Jversion = root.GetProperty("version");
                    // double vrsn = Jversion.GetDouble();

                    // double jsonVersion = 0;

                    // JsonElement JjsonVersion = root.GetProperty("json_version");
                    // if (JjsonVersion.TryGetDouble(out double x))
                    // {
                    //     jsonVersion = x;
                    // }

                    // JsonElement Jid = root.GetProperty("id");
                    // string datasetid = Jid.GetString();

                    // if (datasetid.Trim() == string.Empty)
                    // {
                    //     datasetid = Guid.NewGuid().ToString();
                    // }



                    // JsonElement Jowner = root.GetProperty("*owner");
                    // string owner = Jowner.GetString();

                    // JsonElement Jtags = root.GetProperty("*allowed tags");

                    // JsonElement jcolor = root.GetProperty("*color");
                    // string color = jcolor.GetString();


                    // JsonElement Jlocation = root.GetProperty("*data path");
                    // string dataFilePath = Jlocation.GetString();

                    #endregion

                    string printType = "";

                    if (root.TryGetProperty("*print format", out var JprintType))
                    {
                        printType = JprintType.GetString();
                    }

                    List<string> printables = new List<string>();

                    if (root.TryGetProperty("*printable tags", out JsonElement Jprintables))
                    {
                        foreach (JsonElement p in Jprintables.EnumerateArray())
                        {
                            printables.Add(p.GetString());
                        }
                    }

                    // bool canEdit = false;

                    // if (root.TryGetProperty("*edit data?", out JsonElement Jedit))
                    // {
                    //     canEdit = Jedit.GetBoolean();
                    // }

                    string searchFilter = "-";

                    if (root.TryGetProperty("*default search filter", out JsonElement Jfilter))
                    {
                        searchFilter = Jfilter.GetString();
                    }

                    DataFileInstance file = CSVSerialization.Deserialize(dataFilePath);

                    DataSetView c = null;

                    List<DataSetView> views = new List<DataSetView>();

                    List<ColumnInfo> ColumnInfoList = new List<ColumnInfo>();

                    if (root.TryGetProperty("format", out var jFormat))
                    {
                        string viewJson = jFormat.GetRawText();
                        c = DataSetView.CreateView(viewJson);

                        views.Add(c);

                    }
                    else //card manual version 1.5
                    {
                        if (root.TryGetProperty("views", out var Jviews))
                        {
                            foreach (var view in Jviews.EnumerateArray())
                            {
                                try
                                {
                                    string data = view.GetRawText();

                                    DataSetView cardView = DataSetView.CreateView(data);
                                    views.Add(cardView);

                                }
                                catch (Exception ex)
                                {
                                    NotificationHandler.Print("Cannot load view " + ex.Message);
                                }
                            }
                        }
                    }

                    if (root.TryGetProperty("column info", out var jCols))
                    {
                        foreach (var columnInfo in jCols.EnumerateArray())
                        {
                            int column = -1;
                            ValuePermissions permissions = ValuePermissions.ViewOnly;
                            string header = null;
                            string valueType = ColumnInfo.GetValueTypeOfParagraph;

                            List<string> options = new List<string>();

                            string description = "";

                            if (columnInfo.TryGetProperty("column", out var col))
                            {
                                column = col.GetInt32();
                            }


                            if (columnInfo.TryGetProperty("permissions", out var jPermissions))
                            {
                                permissions = (ValuePermissions)jPermissions.GetInt32();
                            }

                            if (columnInfo.TryGetProperty("header", out var jHeader))
                            {
                                header = jHeader.GetString();
                            }

                            if (columnInfo.TryGetProperty("value type", out var jValueType))
                            {
                                valueType = jValueType.GetString();
                            }

                            if (columnInfo.TryGetProperty("options", out var jOptions))
                            {
                                foreach (var valueOptions in jOptions.EnumerateArray())
                                {
                                    string someString = valueOptions.GetString();

                                    if (someString != null)
                                    {
                                        options.Add(someString);
                                    }
                                }

                                options.Sort();
                            }

                            if (columnInfo.TryGetProperty("description", out var Jdescription))
                            {
                                description = Jdescription.GetString();
                            }

                            string customRegex = "";

                            if (columnInfo.TryGetProperty("custom format", out var jregex))
                            {
                                customRegex = jregex.GetString();
                            }


                            if (header != null && valueType != null && valueType.Length > 0 && options.Count < 1)
                            {

                                //Debug.Log("value type " + valueType);

                                var x = new ColumnInfo(column, permissions, header, description, valueType);
                                x.CustomRegex = customRegex;
                                ColumnInfoList.Add(x);
                            }
                            else if (header != null && valueType != null && valueType.Length > 0 && options.Count > 0)
                            {
                                //UnityEngine.Debug.Log($"options {string.Join(",", options)}");
                                var x = new ColumnInfo(column, permissions, header, description, options.ToArray());
                                x.CustomRegex = customRegex;
                                ColumnInfoList.Add(x);
                            }
                            else
                            {
                                ColumnInfo newColumnInfo = new ColumnInfo(column, permissions);
                                newColumnInfo.CustomRegex = customRegex;
                                ColumnInfoList.Add(newColumnInfo);
                            }
                        }
                    }




                    if (views.Count < 1)
                    {
                        throw new ArgumentException($"{datasetName}: cannot find any views");
                    }


                    //Debug.Log(string.Join(",", ColumnInfo.GetJson(ColumnInfoList)));

                    newDataset = new DataSet(datasetName, owner, tags, printType, printables, false, searchFilter, color, vrsn, jsonVersion, datasetid, categoriesList,
                     datasetFilePath, views, file, ColumnInfoList, datasetFilePath);


                    return newDataset;
                }
            }
            catch (Exception ex)
            {
                //Debug.LogError("Error Loading " + dataSetName + ": " + ex.Message);

                NotificationHandler.Print($"Could not fetch {dataSetName}: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Get a string safely from this wrapper
        /// </summary>
        /// <param name="element">the json element</param>
        /// <param name="propertyName">the element name we are looking for</param>
        /// <param name="throwErrorIfNotFound">throws an error if not not found (if enabled)</param>
        /// <param name="datasetIdentifier">some way to identify the data set if things go south</param>
        /// <returns>returns the string if successfully found, if not returns an empty string</returns>
        private static string GetStringWrapper(JsonElement element, string propertyName, string dataSetIdentifier = "", bool throwErrorIfNotFound = false)
        {
            if (element.TryGetProperty(propertyName, out var newElement))
            {
                return newElement.GetString();
            }

            if (throwErrorIfNotFound)
            {
                throw new ArgumentException("Cannot find the property " + propertyName + " - " + dataSetIdentifier);
            }

            return "";
        }

        /// <summary>
        /// Get a string safely from this wrapper
        /// </summary>
        /// <param name="element">the json element</param>
        /// <param name="propertyName">the element name we are looking for</param>
        /// <param name="throwErrorIfNotFound">throws an error if not not found (if enabled)</param>
        /// <param name="datasetIdentifier">some way to identify the data set if things go south</param>
        /// <returns>returns the double if successfully found, if not returns a 0</returns>
        private static double GetDoubleWrapper(JsonElement element, string propertyName, string dataSetIdentifier = "", bool throwErrorIfNotFound = false)
        {
            if (element.TryGetProperty(propertyName, out var newElement))
            {
                return newElement.GetDouble();
            }

            if (throwErrorIfNotFound)
            {
                throw new ArgumentException("Cannot find the property " + propertyName + " - " + dataSetIdentifier);
            }

            return 0;
        }

        /// <summary>
        /// Get a string safely from this wrapper
        /// </summary>
        /// <param name="element">the json element</param>
        /// <param name="propertyName">the element name we are looking for</param>
        /// <param name="throwErrorIfNotFound">throws an error if not not found (if enabled)</param>
        /// <param name="datasetIdentifier">some way to identify the data set if things go south</param>
        /// <param name="outElement">the out json element</param>
        /// <returns>returns the element if successfully found, if not returns a 0</returns>
        private static bool TryGetElementWrapper(JsonElement element, string propertyName, out JsonElement outElement, string dataSetIdentifier = "", bool throwErrorIfNotFound = false)
        {

            if (element.TryGetProperty(propertyName, out var newElement))
            {
                outElement = newElement;
                return true;
            }

            if (throwErrorIfNotFound)
            {
                throw new ArgumentException("Cannot find the property " + propertyName + " - " + dataSetIdentifier);
            }

            outElement = default(JsonElement);
            return false;
        }

        /// <summary>
        /// Try to set the card manual by a string array of categories
        /// </summary>
        /// <param name="subCategories">the array string of sub categories</param>
        /// <returns>returns true if the manual was found and set, false if not</returns>
        public bool TrySetView(Account account, string id)
        {
            var views = GetAllowedViews(account);

            foreach (var x in views)
            {

                // Debug.Log(x.ViewName + " " + x.ID + " - selected id: " + id);

                if (x.ID == id)
                {

                    SelectedView = x;

                    WarSystem.WriteToLog("View Changed on " + ID + " from " + SelectedView.ID + " to " + x.ID, Logging.MessageType.logEvent);
                    SheetsManager.ReloadCurrentSheet();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Set the view if possible - does not reload the scene
        /// </summary>
        /// <param name="account">the user account</param>
        /// <param name="id">the id of the view</param>
        public void SetView(Account account, string id)
        {
            var views = GetAllowedViews(account);

            foreach (var x in views)
            {
                if (x.ID == id)
                {
                    SelectedView = x;
                    return;
                }
            }

            throw new ArgumentException("cannot find view with the id");
        }

        /// <summary>
        /// Get the Card Manuals
        /// </summary>
        /// <param name="acct">a specific account</param>
        /// <returns></returns>
        public DataSetView[] GetAllowedViews(Account acct)
        {
            List<DataSetView> cardViews = new List<DataSetView>();

            foreach (var view in Views)
            {
                if (acct.Permissions <= view.Categories)
                {
                    cardViews.Add(view);
                }
                else if (view.Categories.Contains(acct.UserName))
                {
                    cardViews.Add(view);
                }
            }

            return cardViews.ToArray();
        }

        /// <summary>
        /// Get the printed tags in the correct printing order
        /// </summary>
        /// <returns>returns the printing tags</returns>
        private List<string[]> GetPrintTags()
        {
            List<string[]> result = new List<string[]>();

            for (int i = 0; i < PrintableTags.Count; i++)
            {
                List<string> str = new List<string>();

                string[] splitString = PrintableTags[i].Split(new char[1] { '|' });

                for (int k = 0; k < splitString.Length; k++)
                {
                    splitString[k] = splitString[k].Trim();
                }

                str.AddRange(splitString);
                result.Add(str.ToArray());
            }

            return result;
        }

        /// <summary>
        /// Get the string list of printable information
        /// </summary>
        /// <param name="entry">the data entry</param>
        /// <returns>returns the list of printable strings</returns>
        public List<(string name, string value)> GetPrintableInfo(DataEntry entry, List<string[]> printTags)
        {
            List<string[]> allowedPrintingTags = new List<string[]>();
            if (printTags != null && printTags.Count > 0)
            {
                allowedPrintingTags = printTags;
            }
            else
            {
                allowedPrintingTags = GetPrintTags();
            }


            List<(string name, string value)> result = new List<(string name, string value)>();

            if (entry == null)
                throw new NullReferenceException("The entry is null");

            if (entry.DataSet.ID != ID)
                throw new ArgumentException("The entry's dataset ID is not the same as the id");

            List<DataValue> values = new List<DataValue>();
            values.AddRange(entry.GetAllowedDataValues());

            for (int i = 0; i < allowedPrintingTags.Count; i++)
            {
                if (allowedPrintingTags[i].Length == 1)
                {
                    var value = values.Find(x => x.HeaderName == allowedPrintingTags[i][0]);

                    if (value != null)
                        result.Add((value.HeaderName, value.ParseToParagraph()));
                    else
                        result.Add(("<empty>", ""));
                }
                else if (allowedPrintingTags[i].Length > 1)
                {
                    (string name, string value)[] data = new (string, string)[allowedPrintingTags[i].Length];

                    for (int k = 0; k < allowedPrintingTags[i].Length; k++)
                    {
                        var value = values.Find(x => x.HeaderName == allowedPrintingTags[i][k]);

                        if (value != null)
                        {
                            data[k] = (value.HeaderName, value.ParseToParagraph());
                        }
                        else
                        {
                            data[k] = ("<empty>", "");
                        }
                    }

                    string names = "";
                    string someValues = "";
                    for (int m = 0; m < data.Length; m++)
                    {
                        names = names + "/" + data[m].name.Trim();
                        someValues = someValues + " " + data[m].value.Trim();
                    }

                    names = names.Remove(0, 1);
                    someValues = someValues.Remove(0, 1);

                    result.Add((names, someValues));
                }
                else
                {
                    result.Add(("<empty>", ""));
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a data set and inserts it into a file control for better permissions security
        /// </summary>
        /// <param name="json">the json info</param>
        /// <param name="dataSetFilePath">the file path</param>
        /// <param name="owner">the owner of the file</param>
        /// <param name="creationTime">the creation time of the file</param>
        /// <returns>returns a filecontrol dataset file </returns>
        public static FileControl<DataSet> CreateFileControl(DataSet dataSet, string dataSetFilePath, string owner, DateTime creationTime, DateTime lastAccessTime)
        {
            if (dataSet == null)
                throw new NullReferenceException("the dataset cannot be null");

            FileControl<DataSet> dataSetFile;

            if (dataSet.Categories == null || dataSet.Categories.Length < 1)
            {
                dataSetFile = new FileControl<DataSet>(dataSet.Version.ToString());
            }
            else
            {
                bool greedy = true;
                dataSetFile = new FileControl<DataSet>(dataSet, greedy, dataSet.Version.ToString(), dataSet.Categories, owner, creationTime, lastAccessTime);
            }


            return dataSetFile;
        }

        // /// <summary>
        // /// Replace the data piece with new data - use ReplaceEntry() Instead
        // /// </summary>
        // /// <param name="piece">The data piece</param>
        // /// <returns>returns true if the save was successful</returns>
        // [Obsolete("ReplaceDataEntry() Instead")]
        // public bool ReplaceDataPiece(DataPiece piece)
        // {
        //     //Debug.Log(piece.RowID);

        //     DataFile.ReplaceRow((int)piece.RowID, piece.Data);

        //     //Debug.Log(string.Join(",", piece.Data));

        //     return SaveAndReloadDataSet();
        // }

        /// <summary>
        /// Add a new data piece to the file - Use AppendEntries() Instead
        /// </summary>
        /// <param name="piece">The new data piece</param>
        /// <returns>returns true if the save was successful</returns>
        [Obsolete("Use AppendData() instead")]
        public bool AddNewDataPiece(DataPiece piece)
        {
            //DataFile.AppendNewRow(piece.Data);
            return SaveAndReloadDataSet();
        }

        #region  old
        /// <summary>
        /// Replace the data entry with new data and reload the dataset
        /// </summary>
        /// <param name="entry">the data entry</param>
        /// <returns>returns true if the save was successful, false if not</returns>
        // public bool ReplaceEntry(DataEntry entry)
        // {

        //     // if (entry.Row > _fileEntries.Count)
        //     //     throw new ArgumentOutOfRangeException("the entry row value is larger than the count " + entry.Row + " - " + _fileEntries.Count);

        //     if (!_fileEntries.c)
        //         throw new ArgumentException("The entry has not been found - cannot replace an entry that has not been already created");

        //     // if (entry.Row >= _fileEntries.Count)
        //     //     return AppendEntry(entry);

        //     DataFile.ReplaceRow(entry.RowID, entry.GetRawData());

        //     // Debug.Log("Replacing Entry " + string.Join(",", _fileEntries[entry.Row].GetRawData()[1]) + " " + entry.Row);

        //     //Debug.Log("Removing " + string.Join(", ", _fileEntries[entry.Row].GetRawData()));
        //     // Debug.Log("Inserting " + string.Join(", ", entry.GetRawData()));

        //     _fileEntries.Remove(entry);
        //     // if (_fileEntries.Count > entry.Row)
        //     // _fileEntries.Insert(entry.Row, entry);
        //     // else
        //     _fileEntries.Add(entry);

        //     // Debug.Log(DatasetName + " replacing entry " + string.Join(", ", entry.GetRawData()));

        //     return SaveAndReloadDataSet();
        // }

        #endregion

        /// <summary>
        /// Replaces the data entry in the file
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool ReplaceEntry(DataEntry entry)
        {
            if (_fileEntries.ContainsKey(entry.RowID))
            {
                _fileEntries[entry.RowID] = entry;
                WarSystem.DeveloperPushNotificationHandler.EditedData = true;
                return SaveAndReloadDataSet();
            }

            return false;
        }

        /// <summary>
        /// Remove an entry from the file and save the file
        /// </summary>
        /// <param name="entry">the entry to remove</param>
        /// <returns>returns true if the operation was successful, false if not.</returns>
        public bool RemoveEntry(DataEntry entry)
        {

            if (_fileEntries.Remove(entry.RowID))
            {
                // DataFile.RemoveRow(entry.RowID, entry.GetRawData());
                WarSystem.DeveloperPushNotificationHandler.EditedData = true;
                return SaveAndReloadDataSet();
            }

            return false;
        }

        // <summary>
        /// Add a new data entry to the file
        /// </summary>
        /// <param name="piece">The new data piece</param>
        /// <returns>returns true if the save was successful</returns>
        public bool AppendEntry(DataEntry entry)
        {
            if (!_fileEntries.ContainsKey(entry.RowID))
            {
                //  DataFile.AppendNewRow(entry.GetRawData());
                _fileEntries.Add(entry.RowID, entry);
                if (WarSystem.DeveloperPushNotificationHandler != null)
                    WarSystem.DeveloperPushNotificationHandler.EditedData = true;
                //Debug.Log(DatasetName + " appending entry " + string.Join(", ", entry.GetRawData()));

                return SaveAndReloadDataSet();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Save the file and reload the current sheet and dataset
        /// </summary>
        /// <returns>returns true if the save was successful</returns>
        private bool SaveAndReloadDataSet()
        {

            // Debug.Log("checking connection");

            if (WarSystem.IsConnectedToServer)
            {
                List<string> str = new List<string>();

                //Debug.Log("preparing to replace");

                DataFile.ReplaceData(Entries);

                // foreach (var x in DataFile.GetAllData())
                // {
                //     str.Add(string.Join(", ", x));
                // }

                //Debug.Log(string.Join("\n", str));

                if (DataFile.SerializeFile())
                {
                    WarSystem.RefreshLoadedDataSets(); //does not reload the data set (but the whole object)
                    SheetsManager.ReloadCurrentSheet(); //does not reload the data set (but the whole object)

                    return true;
                }
                else
                {
                    if (DataFile.Info != null)
                    {
                        WarSystem.WriteToLog("Error! could not save file " + DataFile.FileName, Logging.MessageType.critical);
                    }
                    else
                    {
                        WarSystem.WriteToLog("Error! could not save file (unknown file name)", Logging.MessageType.critical);
                    }
                }
            }
            else
            {
                NotificationHandler.Print("Cannot Save - Not connected to server ");
            }

            return false;
        }

        /// <summary>
        /// Get a new id
        /// </summary>
        /// <returns></returns>
        [Obsolete("use a new guid instead", true)]
        public int GetNewId()
        {
            // List<DataEntry> entries = new List<DataEntry>();
            // entries.AddRange(_fileEntries.ToArray());

            // entries.Sort((x, y) =>
            // {
            //     if (x == null || y == null)
            //         return 0;

            //     return x.Row.CompareTo(y.Row);

            // });

            // int id = 0;

            // for (int i = 0; i < entries.Count; i++)
            // {
            //     if (entries[i].Row == id) id++;
            //     else return id;
            // }

            return 0;
        }


        #region old csv parse

        /// <summary>
        /// Get Data Pieces
        /// </summary>
        /// <param name="filePath">the file path where the pieces are located</param>
        /// <returns>returns a list of data pieces</returns>
        private static bool TryGetListOfDataPieces(string dataSetID, string filePath, out List<DataPiece> pieces, out string[] tags)
        {
            // Debug.Log(filePath);

            if (filePath == null || filePath == string.Empty || filePath.ToLower() == "default")
            {
                pieces = new List<DataPiece>();
                tags = new string[0];
                return false;
            }

            List<DataPiece> dataPieces = new List<DataPiece>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                bool header = true;

                string headerString = "";
                long row = 1;
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        headerString = reader.ReadLine();

                        //Debug.Log(headerString);
                        header = false;
                    }

                    string info = reader.ReadLine();

                    if (string.IsNullOrEmpty(info)) // empty file
                    {
                        pieces = dataPieces;
                        tags = headerString.Split(',');
                        return true;
                    }

                    List<string> FinalList = new List<string>();

                    List<string> col = new List<string>();
                    col.AddRange(info.Split(','));

                    for (int i = 0; i < col.Count; i++)
                    {
                        string finalStr = "";

                        // Debug.Log("wrapped");

                        var regex = Regex.Match(col[i], "^\"\"");

                        col[i] = col[i].Trim();

                        if (col[i].StartsWith("\"") && !col[i].EndsWith("\"") && !col[i].StartsWith("\"\""))
                        {
                            // Debug.Log("starts but does not end with " + "\" " + col[i]);

                            string extraInfo = col[i];
                            bool continueSearch = true;

                            while (continueSearch)
                            {
                                if (i >= col.Count - 1 && !reader.EndOfStream)
                                {
                                    string[] next = new string[0];
                                    extraInfo = HandleNewLines(col[i], reader, out next);

                                    col.AddRange(next);
                                    //Debug.Log("New Line Info " + extraInfo + " " + i + " " + col.Count);
                                    continueSearch = false;
                                }
                                else // if (i < col.Count && col.Count > 0)
                                {
                                    extraInfo = HandleQuoteCommas(col.ToArray(), i, out i);
                                    // Debug.Log("Reading line info " + extraInfo + " " + i + " " + col.Count);
                                    continueSearch = false;
                                }
                            }

                            finalStr += extraInfo;
                        }
                        else
                        {
                            if (i < col.Count)
                            {
                                finalStr = col[i];
                            }
                        }

                        FinalList.Add(finalStr);
                    }

                    info = string.Join(",", FinalList);

                    //Debug.Log("joined string");

                    // Debug.Log(string.Join("|", FinalList) + " ->" + FinalList.Count);

                    //Debug.Log(info);

                    DataPiece p = new DataPiece(row, FinalList.ToArray(), headerString.Split(','), dataSetID);

                    dataPieces.Add(p);

                    row++;
                }
            }

            // Debug.Log(dataPieces.Count);

            pieces = dataPieces;
            tags = new string[0];
            return true;
        }

        private static string HandleNewLines(string starter, StreamReader reader, out string[] remainingString)
        {
            string s = starter;
            string str = "";


            string[] result = new string[0];

            while (!str.Contains("\"") && !reader.EndOfStream)
            {
                if (str != string.Empty)
                    s = s + ", " + str;

                str = reader.ReadLine();
                //Debug.Log("Reading line " + str);
            }

            if (str.Contains(","))
            {
                var extra = str.Split(',');

                if (!extra[0].Contains(",") && extra[0].Contains("\""))
                {
                    s = s + ", " + extra[0];
                    //Debug.Log(s);

                    result = new string[extra.Length - 1];
                    Array.Copy(extra, 1, result, 0, result.Length);
                }
            }

            //Debug.Log(string.Join("|", result));

            remainingString = result;

            s = s.Replace('\"', ' ');
            s = s.Trim();

            return s;
        }

        private static string HandleQuoteCommas(string[] info, int start, out int leftOff)
        {
            string result = "";

            bool completed = false;
            int i = start;

            bool hasContained = false;

            while (i < info.Length && !completed)
            {
                if (info[i].Contains("\"") && hasContained)
                {
                    completed = true;
                }
                else if (info[i].Contains("\""))
                {
                    hasContained = true;
                }

                if (result != string.Empty)
                    result = result + "," + info[i];
                else
                {
                    result = info[i];
                }
                i++;
            }

            // Debug.Log("RESULT" + result);

            result = result.Replace('\"', ' ');
            result = result.Trim();

            leftOff = i - 1;
            return result;
        }

        #endregion
    }
}
