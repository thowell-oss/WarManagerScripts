using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// Handles the default card element data incoming (usually from a JSON source)
    /// </summary>
    [Notes.Author("Handles the default card element data incoming (usually from a JSON source)")]
    public abstract class CardElementViewData
    {
        /// <summary>
        /// The column information
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        [JsonIgnore]
        protected List<int> _columns = new List<int>();

        /// <summary>
        /// Get the columns from the list
        /// </summary>
        [JsonIgnore]
        public IEnumerable<int> Columns => _columns;

        /// <summary>
        /// The count of how many columns are in the column count
        /// </summary>
        [JsonIgnore]
        public int ColumnCount => _columns.Count;

        /// <summary>
        /// Converts the columns into an array
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("*other cols")]
        public int[] ToColumnArray => _columns.ToArray();

        /// <summary>
        /// The type of information being displayed with the column
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*type")]
        public string ColumnType { get; set; } = "none";

        /// <summary>
        /// The version of the information
        /// </summary>
        /// <value></value>
        [JsonPropertyName("element version")]
        public double Version { get; set; } = 2;

        /// <summary>
        /// The Location of the element
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*location")]
        public double[] Location { get; set; } = new double[3] { 0, 0, 0 };

        /// <summary>
        /// The Rotation of the column
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*rotation")]
        public double[] Rotation { get; set; } = new double[2] { 0, 0 };

        /// <summary>
        /// The scale of the column
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*scale")]
        public double[] Scale { get; set; } = new double[2] { 2, 2 };

        /// <summary>
        /// The layout string
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public abstract string[] Layout { get; set; }

        /// <summary>
        /// Is this column a critical column? (And must be displayed all the time)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*critical?")]
        public bool Critical { get; set; } = false;

        /// <summary>
        /// Set the columns to a new list of columns
        /// </summary>
        /// <param name="columns">the new list of columns</param>
        /// <remarks>the columns must not be empty or null</remarks>
        public List<int> SetColumns
        {
            set
            {
                if (value != null && value.Count > 0)
                    _columns = value;
                else if (value == null)
                {
                    throw new System.NullReferenceException("Cannot set the columns because the new list is null");
                }
                else if (value.Count == 0)
                {
                    throw new System.IndexOutOfRangeException("Cannot set the columns because the new list is empty");
                }
            }
        }

        /// <summary>
        /// Get the first column integer
        /// </summary>
        /// <returns></returns>
        public int GetFirstColumn()
        {
            if (_columns != null && _columns.Count > 0)
                return _columns[0];
            else
            {
                return 0;
            }
        }
    }
}
