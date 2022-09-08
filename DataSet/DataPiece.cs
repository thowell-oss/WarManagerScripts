/* DataPiece.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles maintaining a specific row of data
    /// </summary>
    [Notes.Author("Handles maintaining a specific row of data")]
    public class DataPiece
    {
        private long _rowID;

        /// <summary>
        /// The row location (or id)
        /// </summary>
        /// <value></value>
        public long RowID
        {
            get
            {
                return _rowID;
            }
            protected set
            {
                _rowID = value;
            }
        }

        /// <summary>
        /// The dataset associated with the data piece
        /// </summary>
        /// <value></value>
        public string DataSetId { get; set; }

        private string[] _data;

        /// <summary>
        /// The full array of data
        /// </summary>
        /// <value></value>
        public string[] Data
        {
            get
            {
                return _data;
                throw new System.NullReferenceException("The Data is null");
            }
        }

        private string[] _tags;

        /// <summary>
        /// the full array of tags
        /// </summary>
        /// <value></value>
        public string[] Tags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// The length of the row of data
        /// </summary>
        /// <value></value>
        public int DataCount
        {
            get
            {
                var data = Data;
                return data.Length;
            }
        }

        /// <summary>
        /// The length of the row of tags
        /// </summary>
        /// <value></value>
        public int TagCount
        {
            get
            {
                var tags = Tags;
                return tags.Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="row">the row id</param>
        /// <param name="data">the row string</param>
        public DataPiece(long row, string[] data, string[] Tags, string dataSetId)
        {
            if (row <= -2)
                throw new System.NotSupportedException("row must be either negative one (null ref) or a positive integer greater than or equal to zero");

            if (data == null)
                throw new System.NullReferenceException("The data string cannot be null");

            _data = data;

            _tags = Tags;

            // UnityEngine.Debug.Log("data " + string.Join(",", _data));

            //Debug.Log("Tags: " + _tags + " Data: " + _data);

            _rowID = row;

            if (dataSetId == null)
                throw new System.NullReferenceException("The data set cannot be null");

            DataSetId = dataSetId;
        }

        /// <summary>
        /// Get a string from the array of data
        /// </summary>
        /// <param name="col">the index of the string</param>
        /// <returns>returns a non-null string</returns>
        public string GetData(int col)
        {
            if (DataCount > col)
            {
                return Data[col];
            }

            return "";

            throw new System.NullReferenceException("The requested column is out of bounds of the data length. Requested Column: " + col + " Data length: " + DataCount);
        }

        /// <summary>
        /// Get a tag string from the array of tags
        /// </summary>
        /// <param name="col">the index of the tag</param>
        /// <returns>returns a non-null string</returns>
        public string GetTag(int col)
        {
            if (TagCount > col)
            {
                return Tags[col];
            }

            throw new System.NullReferenceException("The requested column is out of bounds of the tag length. Requested Column: " + col + " Tag length: " + TagCount);
        }

        /// <summary>
        /// Get the tag value pair of a set from the data piece
        /// </summary>
        /// <param name="col">the location of the tag value pair</param>
        /// <returns></returns>
        public TagValuePair<int, string> GetSet(int col)
        {
            string tag = GetTag(col);
            string value = GetData(col);

            return new TagValuePair<int, string>(col, tag, value);
        }

        /// <summary>
        /// Get all the tag value pair sets in the data piece
        /// </summary>
        /// <returns>returns a non null list</returns>
        public List<TagValuePair<int, string>> GetSets()
        {
            List<TagValuePair<int, string>> tagValues = new List<TagValuePair<int, string>>();

            for (int i = 0; i < TagCount; i++)
            {
                if (i < DataCount)
                {
                    tagValues.Add(new TagValuePair<int, string>(i, Tags[i], Data[i]));
                }
            }

            return tagValues;
        }

        /// <summary>
        /// Split the data into a string if possible
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="output">the output array</param>
        /// <returns></returns>
        private bool TrySplitData(string input, out string[] output)
        {
            if (input == null || input == string.Empty)
            {
                output = null;
                return false;
            }

            output = input.Split(',');

            //Debug.Log(string.Join(",", output));

            return true;
        }

        public override string ToString()
        {
            string str = "";

            for (int i = 0; i < Data.Length; i++)
            {
                if (Tags.Length > i)
                {

                    str = Tags[i] + ": " + Data[i] + ", ";
                }
            }

            return str;
        }
    }
}
