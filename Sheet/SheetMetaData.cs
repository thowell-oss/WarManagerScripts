
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

using UnityEngine;
using WarManager.Sharing;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles the serialization and instance of a sheet meta data
    /// </summary>
    [Notes.Author(1.2, "Handles the serialization and instance of a sheet meta data")]
    public class SheetMetaData : IComparable<SheetMetaData>, IEquatable<SheetMetaData>
    {
        /// <summary>
        /// The name of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("name")]
        public string SheetName { get; private set; } = "New Sheet";

        /// <summary>
        /// The user's description of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("description")]
        public string SheetDescription { get; set; } = "";

        /// <summary>
        /// Set the background color
        /// </summary>
        /// <value></value>
        [JsonPropertyName("background color")]
        public string BackgroundColor { get; set; } = "#242424";

        /// <summary>
        /// The version of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("version")]
        public double Version { get; private set; }

        /// <summary>
        /// The id of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string ID { get; private set; }

        /// <summary>
        /// The owner of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("owner")]
        public string Owner { get; private set; }

        /// <summary>
        /// The location of the sheet
        /// </summary>
        /// <value></value>
        [JsonPropertyName("sheet file location")]
        public string SheetPath { get; private set; }

        /// <summary>
        /// Can the user edit the sheet?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*edit?")]
        public bool CanEdit { get; private set; }

        [JsonPropertyName("form template?")]
        public bool FormTemplate { get; set; } = false;

        /// <summary>
        /// The Grid size
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*grid size")]
        public double[] GridSize { get; set; } = new double[2] { 6, 3 };

        /// <summary>
        /// The categories associated with the dataset
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*categories")]
        public string[] Categories { get; private set; }

        /// <summary>
        /// Is the file currently in use?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("last opened")]
        public DateTime LastTimeOpened { get; private set; }

        /// <summary>
        /// The last position the camera was located during the last save
        /// </summary>
        /// <value></value>
        [JsonPropertyName("last camera location")]
        public double[] LastCameraLocation { get; set; } = new double[2] { 0, 0 };


        /// <summary>
        /// The last known position of the drop point location during the last save
        /// </summary>
        /// <value></value>
        [JsonPropertyName("last drop point location")]
        public int[] LastDropPointLocation { get; set; } = new int[2] { 0, 0 };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="owner"></param>
        /// <param name="version"></param>
        /// <param name="id"></param>
        /// <param name="filePath"></param>
        /// <param name="categories"></param>
        public SheetMetaData(string name, string owner, double version, string id, string filePath, bool canEdit, double[] gridSize, string[] categories, DateTime lastTimeOpened, double[] lastCamLocation, int[] lastPointLocation)
        {

            // Debug.Log("creating sheet");

            SheetName = name;
            Version = version;
            ID = id;
            Owner = owner;
            SheetPath = filePath;
            CanEdit = canEdit;

            if (gridSize.Length >= 2)
            {
                GridSize = gridSize;
            }

            Categories = categories;
            LastTimeOpened = lastTimeOpened;

            LastCameraLocation = lastCamLocation;

            if (lastCamLocation == null || lastCamLocation.Length != 2)
            {
                // if (SheetName.Contains("D10"))
                // Debug.LogError("could not find last camera location -> " + SheetName);

                // if (LastCameraLocation != null)
                //     Debug.Log(LastCameraLocation.Length + " " + SheetName);

                LastCameraLocation = new double[2] { 40, -20 };
            }

            LastDropPointLocation = lastPointLocation;

            if (LastDropPointLocation == null || LastDropPointLocation.Length != 2)
                LastDropPointLocation = new int[2];
        }

        /// <summary>
        /// Get the json representation of the sheet meta data instance
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            var str = JsonSerializer.Serialize<SheetMetaData>(this);
            return str;
        }

        /// <summary>
        /// Get the file control of the sheet meta data
        /// </summary>
        /// <param name="creationTime">the time the file was created</param>
        /// <param name="lastAccessTime">the time the file was last accessed</param>
        /// <returns>returns the file control</returns>
        public FileControl<SheetMetaData> GetFileControl(DateTime creationTime, DateTime lastAccessTime)
        {
            var c = new FileControl<SheetMetaData>(this, false, Version.ToString(), Categories, Owner, creationTime, lastAccessTime);
            return c;
        }


        /// <summary>
        /// Convert json into sheet meta data
        /// </summary>
        /// <param name="json">the json string</param>
        /// <returns></returns>
        public static SheetMetaData CreateMetaDataFromJson(string json)
        {
            if (json == null || json == string.Empty)
                throw new System.NotSupportedException("The json data cannot be null or empty");

            // Debug.Log("reading file");

            try
            {
                using (var doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;

                    JsonElement Jname = root.GetProperty("name");
                    string name = Jname.GetString();

                    string description = "";

                    if (root.TryGetProperty("description", out JsonElement descriptionElement))
                    {
                        description = descriptionElement.GetString();
                    }

                    bool formTemplate = false;

                    if (root.TryGetProperty("form template?", out JsonElement formElement))
                    {
                        formTemplate = formElement.GetBoolean();
                    }

                    JsonElement Jversion = root.GetProperty("version");
                    double version = Jversion.GetDouble();

                    JsonElement Jid = root.GetProperty("id");
                    string id = Jid.GetString();

                    JsonElement Jowner = root.GetProperty("owner");
                    string owner = Jowner.GetString();

                    JsonElement Jlocation = root.GetProperty("sheet file location");
                    string sheetLocation = Jlocation.GetString();

                    JsonElement Jedit = root.GetProperty("*edit?");
                    bool canEdit = Jedit.GetBoolean();

                    List<double> gridSizeList = new List<double>();

                    JsonElement jgridsize = root.GetProperty("*grid size");
                    foreach (var grid in jgridsize.EnumerateArray())
                    {
                        gridSizeList.Add(grid.GetDouble());
                    }

                    List<string> categoriesList = new List<string>();

                    JsonElement Jcategories = root.GetProperty("*categories");
                    foreach (var cat in Jcategories.EnumerateArray())
                    {
                        categoriesList.Add(cat.GetString());
                    }

                    JsonElement jopen = root.GetProperty("last opened");
                    DateTime open = jopen.GetDateTime();

                    double[] lastCameraLocation = null;

                    if (root.TryGetProperty("last camera location", out var jCameraLocation))
                    {
                        List<double> camLoc = new List<double>();

                        foreach (var l in jCameraLocation.EnumerateArray())
                        {
                            camLoc.Add(l.GetDouble());
                        }

                        lastCameraLocation = new double[2] { camLoc[0], camLoc[1] };

                        double x = camLoc[0];
                        double y = camLoc[1];

                        // Debug.Log(x + ", " + y + " " + name);
                    }
                    else
                    {
                        // Debug.Log("Could not find coordinates " + name);
                    }

                    int[] lastDropPointLocation = null;

                    if (root.TryGetProperty("last drop point location", out var jDropLocation))
                    {
                        List<int> dropLoc = new List<int>();

                        foreach (var l in jDropLocation.EnumerateArray())
                        {
                            dropLoc.Add(l.GetInt32());
                        }

                        lastDropPointLocation = dropLoc.ToArray();
                    }

                    // if (lastCameraLocation != null)
                    // Debug.Log(lastCameraLocation[0] + " " + lastCameraLocation[1]);

                    SheetMetaData smd = new SheetMetaData(name, owner, version, id, sheetLocation, canEdit, gridSizeList.ToArray(), categoriesList.ToArray(), open, lastCameraLocation, lastDropPointLocation);
                    smd.SheetDescription = description;
                    smd.FormTemplate = formTemplate;

                    return smd;
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError(ex.Message);
                NotificationHandler.Print("There was an error parsing sheet meta data.");
                return null;
            }
        }

        /// <summary>
        /// Create an identical sheet meta data with the same ID
        /// </summary>
        /// <returns></returns>
        public SheetMetaData Clone()
        {
            SheetMetaData newData = new SheetMetaData(SheetName, Owner, Version, ID, SheetPath, CanEdit, GridSize,
            Categories, LastTimeOpened, LastCameraLocation, LastDropPointLocation);
            return newData;
        }

        public int CompareTo(SheetMetaData other)
        {
            if (other == null)
                return 1;

            return GetJson().CompareTo(other.GetJson());
        }

        public bool Equals(SheetMetaData other)
        {
            if (other == null)
                return false;

            return GetJson() == other.GetJson();
        }
    }
}
