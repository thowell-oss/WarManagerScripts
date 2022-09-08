using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Cards;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend
{
    [Notes.Author("Handles data and terminology of a row of data")]
    public class DataEntry
    {

        /// <summary>
        /// The row ID of the data entry
        /// </summary>
        /// <value></value>
        public string RowID { get; private set; }

        /// <summary>
        /// The actor of the data
        /// </summary>
        /// <value></value>
        public Actor Actor { get; set; }

        /// <summary>
        /// The private list of values
        /// </summary>
        /// <param name="DataValue"></param>
        /// <returns></returns>
        private List<DataValue> _values = new List<DataValue>();

        protected IEnumerable<DataValue> Values
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// The count of data in the entry
        /// </summary>
        /// <value></value>
        public int ValueCount
        {
            get
            {
                return _values.Count;
            }
        }

        /// <summary>
        /// Is the dataEntry empty?
        /// </summary>
        /// <value></value>
        public bool Empty
        {
            get
            {
                return _values == null || _values.Count < 1;
            }
        }

        /// <summary>
        /// The dataset reference
        /// </summary>
        /// <value></value>
        public DataSet DataSet { get; private set; } = null;

        /// <summary>
        /// Empty data entry
        /// </summary>
        public DataEntry()
        {
            RowID = string.Empty;
            DataSet = WarSystem.DefaultDataSet;
        }

        public DataEntry(DataSet set)
        {
            DataSet = set;
        }

        /// <summary>
        /// Create a non empty data entry
        /// </summary>
        /// <param name="row">the row</param>
        /// <param name="values">the values</param>
        public DataEntry(string row, DataValue[] values, DataSet set)
        {

            RowID = row;
            _values.AddRange(values);
            DataSet = set;

            foreach (var v in values)
            {
                v.DataEntry = this;
            }
        }

        /// <summary>
        /// Get the data value from a specific colum
        /// </summary>
        /// <param name="col">the column location</param>
        /// <returns>returns the data value with the specific column</returns>
        public DataValue GetValueAt(int col)
        {
            if (col < 0 || col >= ValueCount)
                throw new System.IndexOutOfRangeException("column index is out of range of the available columns: " + ValueCount + " columns: " + col);

            if (DataSet != null && !DataSet.IsAllowedTag(_values[col].HeaderName))
                throw new System.Exception("The tag is not allowed by the dataset");

            return _values[col];
        }

        /// <summary>
        /// Try get the value at a specific location if possible
        /// </summary>
        /// <param name="col">the index location of the column (integer)</param>
        /// <param name="value">the out parameter data value</param>
        /// <returns>returns true if the value has been found, false if otherwise</returns>
        public bool TryGetValueAt(int col, out DataValue value)
        {

            if (ValueCount <= 0) //handles empty rows of data
            {
                value = null;
                return false;
            }

            if (col < 0 || col >= ValueCount) // handles col out of bounds issues
            {
                value = null;
                return false;
            }

            if (DataSet != null && !DataSet.IsAllowedTag(_values[col].HeaderName))
            {
                value = null;
                return false;
            }

            value = _values[col];

            return true;
        }

        /// <summary>
        /// Get a value with a specific header name
        /// </summary>
        /// <param name="header">the header name</param>
        /// <returns>returns the header</returns>
        public bool TryGetValueWithHeader(string header, out DataValue value)
        {

            // Debug.Log(" is allowed tag (" + header + ")? " + DataSet.IsAllowedTag(header));

            if (!DataSet.IsAllowedTag(header))
            {
                value = null;
                return false;
            }

            foreach (var v in _values)
            {

                // Debug.Log("\'" + v.HeaderName + "\'" + "<->" + "\'" + header + "\'");

                if (v.HeaderName.Trim() == header.Trim())
                {
                    // Debug.Log("found header" + v.HeaderName);

                    value = v;
                    return true;
                }
            }

            // Debug.Log("could not find header " + header);

            value = null;
            return false;
        }

        /// <summary>
        /// Get a dictionary of all the available allowed data values using a key as a allowed tag
        /// </summary>
        /// <param name="allowedTagsOnly">should the allowed tags be used to filter the values?</param>
        /// <returns>returns a list of allowed header value pairs</returns>
        public Dictionary<string, DataValue> GetHeaderValuePairs(bool allowedTagsOnly = true)
        {

            Dictionary<string, DataValue> final = new Dictionary<string, DataValue>();

            if (_values == null)
                throw new System.Exception("The values are null");

            if (allowedTagsOnly && DataSet != null)
            {
                List<string> allowedTags = DataSet.AllowedTags;

                // Debug.Log("tags - " + string.Join(", ", allowedTags));

                for (int i = 0; i < allowedTags.Count; i++)
                {
                    if (TryGetValueWithHeader(allowedTags[i], out var value))
                    {
                        final.Add(allowedTags[i], value);
                    }
                }
            }
            else if (allowedTagsOnly && DataSet == null)
            {
                throw new System.NullReferenceException("The data set is null");
            }
            else
            {
                foreach (var v in Values)
                {
                    string tag = v.HeaderName;
                    final.Add(tag, v);
                }
            }

            return final;
        }

        /// <summary>
        /// Get all the allowed values
        /// </summary>
        /// <returns>returns an array of allowed values</returns>
        public DataValue[] GetAllowedDataValues()
        {
            if (DataSet == null)
                throw new System.NullReferenceException("The data set cannot be null");

            var tags = DataSet.AllowedTags;

            // Debug.Log(string.Join(",", tags));

            var valuesDict = GetHeaderValuePairs(true);

            List<DataValue> values = new List<DataValue>();

            for (int i = 0; i < tags.Count; i++)
            {
                if (valuesDict.TryGetValue(tags[i], out var value))
                {
                    values.Add(value);
                    //Debug.Log(i + ") " + tags[i] + " - " + value.ParseToPargraph());
                }
            }

            return values.ToArray();
        }

        /// <summary>
        /// Get the allowed values in list string format
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllowedValues()
        {
            var dataValues = GetAllowedDataValues();

            List<string> values = new List<string>();

            for (int i = 0; i < dataValues.Length; i++)
            {
                values.Add(dataValues[i].Value.ToString());
            }

            return values;
        }

        /// <summary>
        /// Get the allowed values without the empty criteria
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllowedValues_PrettyPrint()
        {
            var values = GetAllowedValues();

            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (values[i].Trim() == string.Empty)
                {
                    values.RemoveAt(i);
                }
            }

            return values;
        }

        /// <summary>
        /// Returns the allowed values in value type pairs
        /// </summary>
        /// <returns>returns the array of value type pairs</returns>
        public ValueTypePair[] GetAllowedValueTypePairs()
        {
            var values = GetAllowedDataValues();

            List<ValueTypePair> valueTypePairs = new List<ValueTypePair>();

            foreach (var v in values)
            {
                valueTypePairs.Add(new ValueTypePair()
                {
                    Value = v.Value,
                    Type = v.ValueType,
                    ColumLocation = v.CellLocation.column
                });
            }

            return valueTypePairs.ToArray();
        }


        /// <summary>
        /// Try get the column info from a specific column
        /// </summary>
        /// <param name="column">the column</param>
        /// <param name="info">the column info</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool TryGetColumnInfo(int column, out ColumnInfo info)
        {
            var data = GetAllowedColumnInfo();

            info = data.Find(x => x.ColumnLocation == column);

            if (info == null)
                return false;

            return true;
        }

        /// <summary>
        /// Try get the column info from a specific header name
        /// </summary>
        /// <param name="header">the header</param>
        /// <param name="info">the column info</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool TryGetColumnInfo(string header, out ColumnInfo info)
        {
            var data = GetAllowedColumnInfo();

            info = data.Find(x => x.HeaderName == header);

            if (info == null)
                return false;

            return true;
        }

        /// <summary>
        /// Get the list of column info from the allowed data values
        /// </summary>
        /// <returns>the list of data values</returns>
        public List<ColumnInfo> GetAllowedColumnInfo()
        {
            List<ColumnInfo> data = new List<ColumnInfo>();

            var values = GetAllowedDataValues();
            foreach (var x in values)
            {
                data.Add(x.GetColumnInfo());
            }

            return data;
        }

        /// <summary>
        /// Update the value at a given column location
        /// </summary>
        /// <param name="pair">the value type pair to update the data</param>
        /// <param name="col">the column location</param>
        public void UpdateValueAt(ValueTypePair pair, int col)
        {
            if (DataSet != null && !DataSet.CanEditData)
                return;

            if (col < 0 && col >= _values.Count)
                throw new System.IndexOutOfRangeException("The column is out of range of the associated values");

            if (_values[col].Value == pair.Value)
                return;

            if (pair.Value == null)
            {
                pair.Value = "";
            }

            if (pair.Type == null || pair.Type == string.Empty)
                pair.Type = ColumnInfo.GetValueTypeOfParagraph;

            var v = _values[col];

            v.ReplaceData(pair);
        }

        /// <summary>
        /// Update the RowID of the data entry
        /// </summary>
        /// <param name="newRowId">the new row ID</param>
        public void UpdateRowID(string newRowId)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                int col = _values[i].CellLocation.column;
                _values[i].CellLocation = (col, newRowId);
            }

            _values[0].ReplaceData(new ValueTypePair(newRowId, ColumnInfo.GetValueTypeOfParagraph));

            RowID = newRowId;

            Debug.Log("updating row " + RowID);
        }


        /// <summary>
        /// returns the data as a string array to be stored in the backend
        /// </summary>
        /// <returns>returns a string array</returns>
        public string[] GetRawData()
        {
            string[] data = new string[_values.Count];

            for (int i = 0; i < _values.Count; i++)
            {

                if (_values[i].Value == null)
                {
                    data[i] = string.Empty;
                }
                else if (!_values[i].Empty)
                {
                    data[i] = _values[i].Value.ToString();
                }
                else
                {
                    data[i] = "";
                }
            }

            if (data != null && data.Length > 0)
                data[0] = RowID.ToString();
            return data;
        }

        /// <summary>
        /// Provides a copy of the dataset
        /// </summary>
        /// <returns></returns>
        public DataEntry Copy()
        {
            DataEntry newEntry = new DataEntry(RowID, _values.ToArray(), DataSet);
            return newEntry;
        }

        /// <summary>
        /// Provides a copy of the dataset
        /// </summary>
        /// <returns></returns>
        public DataEntry Copy(string rowID)
        {
            // if (rowID < 0)
            //     rowID = 0;

            if (rowID == null || rowID.Trim().Length < 1)
                throw new System.ArgumentException("the row id cannot be null or empty");

            DataEntry newEntry = new DataEntry(rowID, _values.ToArray(), DataSet);
            return newEntry;
        }

        public void PrintJSON()
        {
            var data = GetHeaderValuePairs();
            var printTags = DataSet.PrintableTags;
        }
    }
}
