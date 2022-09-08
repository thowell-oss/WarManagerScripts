/* CardManual.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.IO;

using UnityEngine;
using WarManager.Backend.CardsElementData;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles how the card shoule look/function with data from an associated data set
    /// </summary>
    [Notes.Author(1.5, "Handles how the card should look and function with data from the associated data set.")]
    public class DataSetView : CategoryComparable, IEquatable<DataSetView>, IComparable<DataSetView>
    {

        /// <summary>
        /// The element data for how the card should look
        /// </summary>
        private List<CardElementViewData> _elementData = new List<CardElementViewData>();

        /// <summary>
        /// The name of the manual
        /// </summary>
        [JsonPropertyName("*name")]
        public string ManualName { get; set; }


        /// <summary>
        /// The name of the view
        /// </summary>
        [JsonPropertyName("*view name")]
        public string ViewName { get; set; } = "New View";

        /// <summary>
        /// The descrption of the view
        /// </summary>
        [JsonPropertyName("*view description")]
        public string ViewDescription { get; set; } = "This is a new view";

        /// <summary>
        /// The version of the manual
        /// </summary>
        [JsonPropertyName("version")]
        public double Version { get; private set; } = 1.0;

        /// <summary>
        /// if one or more cards are close together, can they become one card over several grid units? <- might cause bugs for any duplicate checking tools...
        /// </summary>
        [JsonPropertyName("extend horizontally?")]
        public bool CanExtendCardsHorizontally { get; set; } = false;

        /// <summary>
        /// if one or more cards are close together, can they become one card over several grid units? <- might cause bugs for any duplicate checking tools...
        /// </summary>
        [JsonPropertyName("extend vertically?")]
        public bool CanExtendCardsVertically { get; set; } = false;

        /// <summary>
        /// Can the card data be edited?
        /// </summary>
        [JsonPropertyName("*Edit?")]
        public bool CanEditCard { get; private set; } = true;

        /// <summary>
        /// Can the card data be viewed?
        /// </summary>
        [JsonPropertyName("*View?")]
        public bool CanViewCard { get; private set; } = true;

        /// <summary>
        /// private backing field
        /// </summary>
        private string _id = null;

        /// <summary>
        /// The unique finger print of the implemented class
        /// </summary>
        [JsonPropertyName("id")]
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = System.Guid.NewGuid().ToString();
                }

                return _id;
            }

            protected set
            {
                _id = value;
            }

        }

        /// <summary>
        /// Can the other associated data be edited?
        /// </summary>
        [JsonPropertyName("*can edit data")]
        public bool CanEditData { get; private set; } = true;

        /// <summary>
        /// The subcategories of the specific manual within the dataset
        /// </summary>
        [JsonPropertyName("*categories")]
        public string[] Categories
        {
            get
            {
                if (PermissionCategories == null || PermissionCategories.Length < 1)
                {
                    PermissionCategories = new string[1] { "*" };
                }

                return PermissionCategories;
            }
        }

        public string columnInfo { get; set; }

        /// <summary>
        /// The elements the card contains
        /// </summary>
        /// <value></value>
        [JsonPropertyName("elements")]
        public CardElementViewData[] ElementDataArray
        {
            get
            {
                return _elementData.ToArray();
            }

            set
            {
                _elementData.Clear();
                _elementData.AddRange(value);
            }
        }

        [JsonIgnore]
        public bool ErrorCreatingCardManual { get; private set; }

        /// <summary>
        /// ets the name of the manual to 'New Card Manual'
        /// </summary>
        /// <param name="canEdit">can the manual be edited?</param>
        /// <param name="canView">can the manual be viewed?</param>
        public DataSetView(bool errorCreatingManual = false, bool canEdit = false, bool canView = false)
        {
            ManualName = "New Card Manual";
            ViewName = "New View";
            Version = 1.0;
            CanEditCard = canEdit;
            CanViewCard = CanViewCard;

            ErrorCreatingCardManual = errorCreatingManual;

            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="errorCreatingManual"></param>
        /// <param name="canEdit"></param>
        /// <param name="canView"></param>
        public DataSetView(string[] categories, bool errorCreatingManual = false, bool canEdit = false, bool canView = false)
        {
            if (categories == null)
                throw new NullReferenceException("The categories are null");

            if (categories.Length < 1)
                throw new ArgumentException("The categories length is less than 1");

            PermissionCategories = categories;


            ManualName = "New Card Manual";
            ViewName = "New View";
            Version = 1.0;
            CanEditCard = canEdit;
            CanViewCard = CanViewCard;

            ErrorCreatingCardManual = errorCreatingManual;

            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get the column to present in the list presentation (for searching)
        /// </summary>
        /// <returns></returns>
        public int GetListPresentationColumn()
        {
            foreach (var x in ElementDataArray)
            {
                if (x.ColumnType == "text" || x.ColumnType == "link")
                {
                    return x.GetFirstColumn();
                }
            }

            return 0;
        }

        /// <summary>
        /// Card Manual constructor
        /// </summary>
        /// <param name="name">the name of the constructor</param>
        /// <param name="canEdit">can the data be edited?</param>
        /// <param name="canView">can the data be viewed?</param>
        /// <param name="elements">the elements of the card</param>
        /// <param name="version">the version of the card element layout</param>
        public DataSetView(string name, string viewName, string viewDescription, string id, bool canEdit,
            bool canView, List<CardElementViewData> elements, string[] categories, bool canEditData, double version = 1.0)
        {
            if (name == null || name == string.Empty)
                throw new NotSupportedException("the manual name cannot be null or empty");

            if (id == null) throw new NullReferenceException("the id cannot be a null string");
            if (id == string.Empty) throw new ArgumentException("the id cannot be an empty string");
            if (id.Length < 20) throw new ArgumentException("the id must be longer than 20 characters");

            if (viewName == null) throw new NullReferenceException("the view name cannot be null");
            if (viewName == string.Empty) throw new System.ArgumentException("The view name cannot be an empty string");

            if (viewDescription == null) throw new NullReferenceException("The view description cannot be null");
            if (viewDescription == string.Empty) throw new ArgumentException("The view description cannot be an empty string");

            if (categories == null) throw new NullReferenceException("The categories array must not be null");
            if (categories.Length < 1) throw new ArgumentException("the categories array is less than 1 element. It cannot be viewed.");


            ManualName = name;

            if (elements == null)
                throw new NullReferenceException("the elements cannot be null");

            _elementData = elements;

            CanEditCard = canEdit;
            CanViewCard = canView;
            CanEditData = canEditData;

            if (version <= 1)
                Version = 1;
            else
                Version = version;

            ViewName = viewName;
            ViewDescription = viewDescription;

            PermissionCategories = categories;

            ID = id;
        }

        /// <summary>
        /// Get the json info from the card manual
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return JsonSerializer.Serialize<DataSetView>(this);
        }

        /// <summary>
        /// Create a card manual based off of a json document
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataSetView CreateView(string json)
        {

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                List<CardElementData> cardElementsList = new List<CardElementData>();

                JsonElement root = document.RootElement;

                JsonElement Jname = root.GetProperty("*name");
                string name = Jname.GetString();

                JsonElement Jversion = root.GetProperty("version");
                double version = Jversion.GetDouble();

                JsonElement JcanEdit = root.GetProperty("*Edit?");
                bool canEdit = JcanEdit.GetBoolean();

                JsonElement jcanView = root.GetProperty("*View?");
                bool canView = jcanView.GetBoolean();

                JsonElement jelements = root.GetProperty("elements");

                bool extendHorizontally = false;

                if (root.TryGetProperty("extend horizontally?", out var extendHorizontalElement))
                {
                    extendHorizontally = extendHorizontalElement.GetBoolean();
                }

                bool extendVertically = false;

                if (root.TryGetProperty("extend horizontally?", out var extendVerticalElement))
                {
                    extendVertically = extendVerticalElement.GetBoolean();
                }


                var cardElementViewDataList = CardElementDataFactory.GetElementData(jelements, name);

                #region old

                // foreach (var element in jelements.EnumerateArray())
                // {
                //     int id = 0;

                //     if (element.TryGetProperty("*col", out JsonElement ID))
                //     {
                //         ID.TryGetInt32(out id);
                //     }

                //     string type = "none";

                //     if (element.TryGetProperty("*type", out JsonElement Type))
                //     {
                //         type = Type.GetString();
                //     }

                //     List<int> extractedLocation = new List<int>();

                //     if (element.TryGetProperty("*location", out JsonElement Location))
                //     {
                //         foreach (var loc in Location.EnumerateArray())
                //         {
                //             int value;
                //             loc.TryGetInt32(out value);
                //             extractedLocation.Add(value);
                //         }
                //     }

                //     string properties = "";

                //     if (element.TryGetProperty("*properties", out JsonElement Prop))
                //     {
                //         properties = Prop.GetString();
                //     }

                //     bool critical = false;

                //     if (element.TryGetProperty("*critical?", out JsonElement Critical))
                //     {
                //         critical = Critical.GetBoolean();
                //     }

                //     int[] finalLocation = new int[2] { 0, 0 };

                //     if (extractedLocation.Count > 0)
                //         finalLocation[0] = extractedLocation[0];

                //     if (extractedLocation.Count > 1)
                //         finalLocation[1] = extractedLocation[1];

                //     #region v1.5

                //     List<int> otherCols = new List<int>();

                //     if (element.TryGetProperty("*other cols", out var jOtherCols))
                //     {
                //         foreach (var x in jOtherCols.EnumerateArray())
                //         {
                //             otherCols.Add(x.GetInt32());
                //         }

                //         //Debug.Log("other columns " + string.Join(",", otherCols));
                //     };

                //     List<int> rotation = new List<int>();

                //     if (element.TryGetProperty("*rotation", out var jElementRotation))
                //     {
                //         foreach (var x in jElementRotation.EnumerateArray())
                //         {
                //             rotation.Add(x.GetInt32());
                //         }

                //         //Debug.Log("rotation " + string.Join(",", rotation));
                //     }

                //     List<int> scale = new List<int>();

                //     if (element.TryGetProperty("*scale", out var jElementScale))
                //     {
                //         foreach (var x in jElementScale.EnumerateArray())
                //         {
                //             scale.Add(x.GetInt32());
                //         }

                //         //Debug.Log("scale " + string.Join(",", scale));
                //     }

                //     List<string> layout = new List<string>();

                //     if (element.TryGetProperty("*layout", out var jLayout))
                //     {
                //         foreach (var x in jLayout.EnumerateArray())
                //         {
                //             layout.Add(x.GetString());
                //         }
                //     }

                //     #endregion

                //     CardElementData c = new CardElementData(id, type, finalLocation, rotation.ToArray(),
                //         scale.ToArray(), otherCols.ToArray(), layout.ToArray(), critical, properties, null);
                //     cardElementsList.Add(c);
                // }

                #endregion

                #region v1.5

                List<string> manualCategories = new List<string>();

                if (root.TryGetProperty("*categories", out var jCategories))
                {
                    foreach (var x in jCategories.EnumerateArray())
                    {
                        string data = x.GetString();

                        if (!string.IsNullOrEmpty(data))
                        {
                            manualCategories.Add(data);
                        }
                    }
                }

                if (manualCategories.Count == 0)
                {
                    manualCategories.Add("*");
                }

                bool canEditData = false;

                if (root.TryGetProperty("*can edit data", out var jCanEditData))
                {
                    canEditData = jCanEditData.GetBoolean();
                }

                string cardManualID = "";

                if (root.TryGetProperty("id", out var jID))
                {
                    cardManualID = jID.GetString();
                }

                if (string.IsNullOrEmpty(cardManualID))
                {
                    //throw new ArgumentException("The card manual ID cannot be null or empty");
                    cardManualID = System.Guid.NewGuid().ToString();
                }

                string viewName = null;

                if (root.TryGetProperty("*view name", out var jViewName))
                {
                    viewName = jViewName.GetString();
                }

                string viewDescription = null;

                if (root.TryGetProperty("*view description", out var jViewDescription))
                {
                    viewDescription = jViewDescription.GetString();
                }

                #endregion

                try
                {
                    DataSetView m = new DataSetView(name, viewName, viewDescription, cardManualID, canEdit, canView, cardElementViewDataList, manualCategories.ToArray(), canEditData, version);
                    m.CanExtendCardsHorizontally = extendHorizontally;
                    m.CanExtendCardsVertically = extendVertically;
                    return m;
                }
                catch (Exception ex)
                {
                    NotificationHandler.Print($"Error creating view {viewName}: {ex.Message}");
                    return new DataSetView(true);
                }
            }
        }



        /// <summary>
        /// compare to interface
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(DataSetView other)
        {
            if (other != null)
                return ViewName.CompareTo(other.ViewName);

            return 1;
        }

        /// <summary>
        /// equals interface
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DataSetView other)
        {
            if (other != null)
            {
                if (other.ID == ID) return true;
            }

            return false;
        }

        /// <summary>
        /// Get the json data for the Card Manual
        /// </summary>
        /// <returns></returns>
        public string GetViewJSON()
        {
            string data = JsonSerializer.Serialize<DataSetView>(this);
            return data;
        }

        public override string ToString()
        {
            return ViewName;
        }
    }
}
