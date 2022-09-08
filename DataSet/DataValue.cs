
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace WarManager.Backend
{
    /// <summary>
    /// Handles the data being transported and the associated value meta information
    /// </summary>
    [Notes.Author("Handles the data being transported and the associated value meta information")]
    public class DataValue
    {

        /// <summary>
        /// The Data Entry that contains the value
        /// </summary>
        /// <value></value>
        public DataEntry DataEntry { get; set; }

        /// <summary>
        /// The location of the cell of data
        /// </summary>
        /// <value></value>
        public (int column, string rowID) CellLocation { get; set; }

        /// <summary>
        /// The meta data information of the column
        /// </summary>
        protected ColumnInfo ColumnInfo { get; private set; }

        /// <summary>
        /// The string name of the header
        /// </summary>
        /// <value></value>
        public string HeaderName
        {
            get
            {
                return ColumnInfo.HeaderName;
            }
        }

        /// <summary>
        /// The value permissions of the data value
        /// </summary>
        /// <value></value>
        public ValuePermissions Permissions
        {
            get
            {
                return ColumnInfo.Permissions;
            }
        }

        /// <summary>
        /// The value stored
        /// </summary>
        /// <value></value>
        public object Value { get; private set; }

        /// <summary>
        /// The type of value stored
        /// </summary>
        /// <value></value>
        public string ValueType
        {
            get
            {
                return ColumnInfo.ValueType;
            }
        }

        /// <summary>
        /// Is the data value empty?
        /// </summary>
        /// <value></value>
        public bool Empty { get; private set; }


        /// <summary>
        /// Create an empty data value
        /// </summary>
        public DataValue(ValuePermissions permissions)
        {
            CellLocation = (-1, "");

            ColumnInfo = new ColumnInfo(CellLocation.column, permissions);

            Value = null;
            Empty = true;
        }

        /// <summary>
        /// Create a a non-empty data value
        /// </summary>
        /// <param name="location">the cell location of the data value</param>
        /// <param name="header">the header name</param>
        /// <param name="value">the value to get</param>
        /// <param name="valueType">the type of value</param>
        /// <param name="description">The description of the header</param>
        public DataValue((int column, string rowID) location, string header, object value, string valueType, ValuePermissions permissions, string description = "")
        {
            if (location.rowID != null && location.column >= 0)
            {
                CellLocation = location;
            }
            else
            {
                throw new System.Exception("The location is either null or out of bounds");
            }

            if (value == null)
                throw new NullReferenceException("The value cannot be null");

            ColumnInfo = new ColumnInfo(location.column, permissions, header, description, valueType);

            Value = value;

            Empty = false;
        }


        /// <summary>
        /// Create a a non-empty data value
        /// </summary>
        /// <param name="location">the cell location of the data value</param>
        /// <param name="header">the header name</param>
        /// <param name="value">the value to get</param>
        /// <param name="valueType">the type of value</param>
        /// <param name="description">The description of the header</param>
        /// <param name="keywords">The description of the header</param>
        public DataValue((int column, string rowID) location, string header, object value, string valueType, ValuePermissions permissions, string description, string[] keywords)
        {
            if (location.column >= 0 && location.rowID != null)
            {
                CellLocation = location;
            }
            else
            {
                throw new System.Exception("The location is either null or out of bounds");
            }

            if (keywords == null)
                keywords = new string[0];

            if (value == null)
                throw new NullReferenceException("The value cannot be null");

            ColumnInfo = new ColumnInfo(location.column, permissions, header, description, valueType);

            Value = value;

            ColumnInfo.KeywordValues = keywords;

            Empty = false;
        }


        /// <summary>
        /// Create a a non-empty data value
        /// </summary>
        /// <param name="row">the row location of the data</param>
        /// <param name="value">the value to get</param>
        /// <param name="columnInfo">the associated column info</param>
        public DataValue(string row, object value, ColumnInfo columnInfo)
        {
            if (columnInfo == null)
                throw new NullReferenceException("The column info cannot be null");

            if (value == null)
                throw new NullReferenceException("The value cannot be null");

            CellLocation = (columnInfo.ColumnLocation, row);
            Value = value;
            ColumnInfo = columnInfo;
        }

        /// <summary>
        /// Attempts to replace the data and checks to see that the value type is the same
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="type">the type of value</param>
        /// <returns>returns true if the value type if the same, false if not</returns>
        public void ReplaceData(ValueTypePair pair)
        {
            if (ColumnInfo.ValueType != pair.Type)
                throw new ArgumentException("the current value type does not match the type of data being inserted.");

            Value = pair.Value;
        }

        /// <summary>
        /// Clear the value and set it to null
        /// </summary>
        public void Clear()
        {
            Value = null;
        }

        /// <summary>
        /// Copy the data value into a new data value
        /// </summary>
        /// <param name="a">the data value to copy</param>
        /// <returns>returns the data value</returns>
        public static DataValue Copy(DataValue a)
        {
            return new DataValue(a.CellLocation.rowID, a.Value, a.ColumnInfo);
        }

        /// <summary>
        /// Compare the the two data values in certian criteria to ensure the two values of are the same type even if the data itself is different
        /// </summary>
        /// <param name="a">the first value</param>
        /// <param name="b">the second value</param>
        /// <returns>return true if the values are the same cell location, header name, and value type</returns>
        public static bool IsSimilarDataValue(DataValue a, DataValue b)
        {
            if (a.CellLocation == b.CellLocation)
            {
                if (a.HeaderName == b.HeaderName)
                {
                    if (a.ValueType == b.ValueType)
                    {
                        return true;
                    }
                }
            }

            if (a.Empty)
            {
                NotificationHandler.Print("Issue: the first value is empty");
                Debug.Log("Issue: the first value is empty");
            }

            if (b.Empty)
            {
                NotificationHandler.Print("Issue: the second value is empty");
                Debug.Log("Issue: the second value is empty");
            }

            return false;
        }

        #region parsing

        /// <summary>
        /// Convert the object to an integer
        /// </summary>
        /// <returns></returns>
        public int ParseToInt32()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfInt)
            {
                return (int)Value;
            }
            else
            {
                string str = Value.ToString();

                if (Int32.TryParse(str, out var result))
                {
                    return result;
                }
            }

            throw new System.Exception("Cannot parse the value to an int because it is not the correct valueType. Current value type: " + ValueType);
        }


        /// <summary>
        /// Convert the object into a point
        /// </summary>
        /// <returns></returns>
        public Point ParseToPoint()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfPoint)
                return (Point)Value;

            throw new System.Exception("Cannot parse the value to a point because it is not the correct valueType. Current value type: " + ValueType);
        }

        /// <summary>
        /// Convert the object into a double
        /// </summary>
        /// <returns></returns>
        public double ParseToRational()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfRational)
            {
                return (double)Value;
            }
            else
            {
                string str = Value.ToString();

                if (Double.TryParse(str, out var result))
                {
                    return result;
                }
            }



            throw new System.Exception("Cannot parse the value to a double because it is not the correct valueType. Current value type: " + ValueType);
        }

        public UnitedStatesPhoneNumber ParseToPhone()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfPhone)
            {

                if (Value == null)
                    Value = "";

                if (UnitedStatesPhoneNumber.TryParse(Value.ToString(), out var phone))
                {
                    return phone;
                }
                else
                {
                    var somePhone = new UnitedStatesPhoneNumber("000", "000", "0000");
                    somePhone.Error = true;
                    return somePhone;
                }
            }
            else
            {
                throw new System.Exception("Cannot parse the value to a phone because it is not the correct valueType. Current value type: " + ValueType);
            }
        }

        /// <summary>
        /// Convert the object into a paragraph - also can function as a catch all toString()
        /// </summary>
        /// <returns></returns>
        public string ParseToParagraph()
        {
            // if (ValueType == ColumnInfo.GetValueTypeOfParagraph || ValueType == ColumnInfo.GetValueTypeOfKeyword || ValueType == ColumnInfo.GetValueTypeOfWord)
            //     return (string)Value;

            // throw new System.Exception("Cannot parse the value to a paragraph because it is not the correct valueType. Current value type: " + ValueType);

            if (Value != null)
                return Value.ToString();
            else return "";
        }

        /// <summary>
        /// Convert the object into a word
        /// </summary>
        /// <returns></returns>
        public string ParseToWord()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfWord)
                return (string)Value;

            throw new System.Exception("Cannot parse the value to a word because it is not the correct valueType. Current value type: " + ValueType);
        }

        /// <summary>
        /// Convert the object into a keyword
        /// </summary>
        /// <returns></returns>
        public string ParseToKeyword()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfKeyword)
                return (string)Value;

            throw new System.Exception("Cannot parse the value to a keyword because it is not the correct valueType. Current value type: " + ValueType);
        }

        /// <summary>
        /// Convert the object into a boolean
        /// </summary>
        /// <returns></returns>
        public bool ParseToBoolean()
        {
            if (ValueType == ColumnInfo.GetValueTypeOfBoolean)
                return (bool)Value;

            throw new System.Exception("Cannot parse the value to a boolean because it is not the correct valueType. Current value type: " + ValueType);
        }


        #endregion

        /// <summary>
        /// Get the column info of the datavalue
        /// </summary>
        /// <returns></returns>
        public ColumnInfo GetColumnInfo()
        {
            return ColumnInfo;
        }

        /// <summary>
        /// Parse the value to paragraph
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ParseToParagraph();
        }
    }

    /// <summary>
    /// Handles the permissions that dictate how the user can interact with the data
    /// </summary>
    public enum ValuePermissions
    {
        /// <summary>
        /// Not allowed to view or edit
        /// </summary>
        None,

        /// <summary>
        /// Only can view
        /// </summary>
        ViewOnly,

        /// <summary>
        /// Allowed to view and edit
        /// </summary>
        Full,
    }
}
