
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;


namespace WarManager.Backend
{
    [Notes.Author("Handles the data recognition results for the column as well as column location information")]
    public class ColumnInfo : IComparable<ColumnInfo>, IEquatable<ColumnInfo>
    {

        /// <summary>
        /// The location of the column
        /// </summary>
        [JsonPropertyName("column")]
        public int ColumnLocation { get; set; }

        /// <summary>
        /// The string name of the header
        /// </summary>
        /// <value></value>
        [JsonPropertyName("header")]
        public string HeaderName { get; private set; }

        /// <summary>
        /// Dicatates how the user can interact with the certain column
        /// </summary>
        /// <value></value>
        [JsonPropertyName("permissions")]
        public ValuePermissions Permissions { get; private set; }

        /// <summary>
        /// The value type of the specific column
        /// </summary>
        /// <value></value>
        [JsonPropertyName("value type")]
        public string ValueType { get; private set; }


        /// <summary>
        /// the optional values that can be used (for a drop down)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("options")]
        public string[] KeywordValues { get; set; } = new string[0];

        /// <summary>
        /// the description of the data
        /// </summary>
        /// <value></value>
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("custom format")]
        public string CustomRegex { get; set; } = "";

        /// <summary>
        /// private backing field
        /// </summary>
        private DataEntry _entry;

        /// <summary>
        /// The data entry
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DataEntry Entry
        {
            get => _entry;

            set
            {
                if (_entry == null)
                    _entry = value;
                else
                    throw new NullReferenceException("the entry is not null");
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="columnLocation">the location of the column</param>
        /// <param name="permissions">the value permissions</param>
        public ColumnInfo(int columnLocation, ValuePermissions permissions)
        {
            ColumnLocation = columnLocation;
            HeaderName = "Unknown Header Name";
            Permissions = permissions;
            ValueType = GetValueTypeOfParagraph;
        }

        /// <summary>
        /// General setup constructor
        /// </summary>
        /// <param name="columnLocation">the column location that these values are associated with</param>
        /// <param name="permissions">the value permissons (what you get to do with this data)</param>
        /// <param name="headerName">the name of the column (header)</param>
        /// <param name="valueType">the type of value</param>
        public ColumnInfo(int columnLocation, ValuePermissions permissions, string headerName, string description, string valueType)
        {
            ColumnLocation = columnLocation;
            Permissions = permissions;

            if (string.IsNullOrEmpty(headerName))
            {
                throw new System.Exception("The header is null or empty");
            }

            if (string.IsNullOrEmpty(valueType))
            {
                throw new System.Exception("The value type is null or empty");
            }

            HeaderName = headerName;
            ValueType = valueType;

            Description = description;
        }

        /// <summary>
        /// Constructor for keywords
        /// </summary>
        /// <param name="columnLocation">the column location that these values are associated with</param>
        /// <param name="permissions">the permissions for these values</param>
        /// <param name="headerName">the header name</param>
        /// <param name="optionalValues">the value options (The value type is set to keyword)</param>
        public ColumnInfo(int columnLocation, ValuePermissions permissions, string headerName, string description, string[] optionalValues)
        {
            ColumnLocation = columnLocation;
            Permissions = permissions;

            if (string.IsNullOrEmpty(headerName))
            {
                throw new System.Exception("The header is null or empty");
            }


            HeaderName = headerName;
            ValueType = GetValueTypeOfKeyword;
            KeywordValues = optionalValues;

            Description = description;
        }

        /// <summary>
        /// Constructor for JSON
        /// </summary>
        /// <param name="column">the column location that these values are associated with</param>
        /// <param name="permissions">the permissions for these values</param>
        /// <param name="header">the header name</param>
        /// <param name="valueType">the type of value</param>
        /// <param name="options">the value options (The value type is set to keyword)</param>
        [JsonConstructor]
        public ColumnInfo(int column, ValuePermissions permissions, string header, string valueType, string description, string[] options)
        {
            ColumnLocation = column;
            Permissions = permissions;

            if (string.IsNullOrEmpty(header))
            {
                throw new System.Exception("The header is null or empty");
            }


            HeaderName = header;
            ValueType = GetValueTypeOfKeyword;
            KeywordValues = options;
            Description = description;
        }

        #region valueTypes

        /// <summary>
        /// allows for new line characters
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfParagraph
        {
            get
            {
                return "paragraph";
            }
        }

        /// <summary>
        /// a word type has no new line characters (usually two or three words max)
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfWord
        {
            get
            {
                return "word";
            }
        }

        /// <summary>
        /// allows numbers, letters, spaces and special characters. Does not allow a new line character (/n)
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfSentence
        {
            get
            {
                return "sentence";
            }
        }

        /// <summary>
        /// keyword values with a drop down
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfKeyword
        {
            get
            {
                return "keyword";
            }
        }

        /// <summary>
        /// use a decimal point
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfRational
        {
            get
            {
                return "double";
            }
        }

        /// <summary>
        /// the integer values (no decimal point)
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfInt
        {
            get
            {
                return "integer";
            }
        }


        /// <summary>
        /// the point system, usually a location on a sheet
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfPoint
        {
            get
            {
                return "point";
            }
        }

        /// <summary>
        /// the binary choice (usually yes or no)
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfBoolean
        {
            get
            {
                return "true or false";
            }
        }

        /// <summary>
        /// use the phone number regex
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfPhone
        {
            get
            {
                return "phone";
            }
        }

        /// <summary>
        /// use email regex
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfEmail
        {
            get => "email";
        }


        /// <summary>
        /// use a multi-selectable keyword system
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfMultiSelectKeyword
        {
            get => "multi-select keyword";
        }

        /// <summary>
        /// a data entry value type
        /// </summary>
        /// <remarks> example of the formatted setup -> <data set id>:<row id>,<data set id>:<row id>...  </remarks>
        /// <value></value>
        public static string GetValueTypeOfDataEntry
        {
            get => "data entry";
        }

        public static string GetValueTypeOfButton
        {
            get => "button";
        }

        /// <summary>
        /// activate the custom regex
        /// </summary>
        /// <value></value>
        public static string GetValueTypeOfCustom
        {
            get => "custom format";
        }

        #endregion

        public int CompareTo(ColumnInfo other)
        {
            if (other == null)
                return 0;

            return ColumnLocation.CompareTo(other.ColumnLocation);
        }

        public bool Equals(ColumnInfo other)
        {
            if (other == null)
                return false;

            return ColumnLocation == other.ColumnLocation;
        }

        /// <summary>
        /// Get json from a list of column info
        /// </summary>
        /// <param name="infos">the column info list</param>
        /// <returns>returns the json in a string array</returns>
        public static string[] GetJson(IEnumerable<ColumnInfo> infos)
        {
            List<string> data = new List<string>();

            foreach (var x in infos)
            {
                data.Add(JsonSerializer.Serialize<ColumnInfo>(x));
            }

            return data.ToArray();
        }

        /// <summary>
        /// Get the json of the column info
        /// </summary>
        /// <param name="info">the colInfo class</param>
        /// <returns>returns the string</returns>
        public static string GetJson(ColumnInfo info)
        {
            return JsonSerializer.Serialize<ColumnInfo>(info);
        }

        public override string ToString()
        {
            return "(" + ColumnLocation + ") " + HeaderName + " (" + ValueType + ")";
        }
    }
}
