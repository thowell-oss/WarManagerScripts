
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;

using UnityEngine;


namespace WarManager.Backend
{
    /// <summary>
    /// Handles the instance of the file downloaded from the serve
    /// </summary>
    [Notes.Author("Handles the instance of the file downloaded from the server")]
    public class DataFileInstance
    {

        /// <summary>
        /// The name of the file
        /// </summary>
        /// <value></value>
        /// <exception cref="Exception">Exception thrown when the file info is null</exception>
        public string FileName
        {
            get
            {
                if (Info != null)
                    return Info.Name;

                throw new Exception("The file info is null");
            }
        }


        /// <summary>
        /// The extension of the file
        /// </summary>
        /// <exception cref="Exception">Exception thrown when the file info is null</exception>
        public string FileExtension
        {
            get
            {
                if (Info != null)
                {
                    return Info.Extension;
                }

                throw new Exception("The file info is null");
            }
        }

        /// <summary>
        /// The directory (folder) of the file
        /// </summary>
        public string FileDirectory
        {
            get
            {
                if (Info != null)
                    return Info.DirectoryName;
                else return string.Empty;
            }
        }

        /// <summary>
        /// The file path of the file instance
        /// </summary>
        /// <value></value>
        public string FilePath { get; private set; }


        /// <summary>
        /// The file path information
        /// </summary>
        public FileInfo Info { get; private set; }

        /// <summary>
        /// Get the last time the file was opened
        /// </summary>
        /// <value></value>
        public DateTime LastTimeOpenedUTC
        {
            get
            {
                return Info.LastAccessTimeUtc;
            }
        }

        /// <summary>
        /// The last time something was written to the file
        /// </summary>
        public DateTime LastTimeDataWrittenUTC
        {
            get
            {
                return Info.LastWriteTime;
            }
        }

        /// <summary>
        /// The Header of the file
        /// </summary>
        /// <value></value>
        public string[] Header { get; private set; } = new string[0];

        /// <summary>
        /// The length of the header (also is the total row length)
        /// </summary>
        public int HeaderLength
        {
            get
            {
                if (Header != null)
                    return Header.Length;

                return 0;
            }
        }


        private List<string[]> _dataBackingField = new List<string[]>();


        /// <summary>
        /// The full data of the string
        /// </summary>
        /// <typeparam name="string[]"></typeparam>
        /// <returns></returns>
        private List<string[]> _data
        {
            get
            {
                return _dataBackingField;
            }
            set
            {
                _dataBackingField = value;
            }
        }


        /// <summary>
        /// Get the amount of data
        /// </summary>
        /// <value></value>
        public int DataCount
        {
            get
            {
                return _data.Count;
            }
        }


        /// <summary>
        /// returns the IEnumerable of the data
        /// </summary>
        /// <value></value>
        public IEnumerable<string[]> Data
        {
            get
            {
                return _data;
            }
        }


        /// <summary>
        /// Empty/null file
        /// </summary>
        public DataFileInstance()
        {

        }

        /// <summary>
        /// Empty file
        /// </summary>
        /// <param name="filePath">the file path</param>
        public DataFileInstance(string filePath)
        {

            if (filePath != null && filePath != string.Empty)
            {

                FileInfo fileInfo = new FileInfo(filePath);

                if (!fileInfo.Exists && fileInfo.Directory.Exists)
                {
                    using (Stream str = File.Create(filePath))
                    {

                    }
                }
                else if (!fileInfo.Exists && !fileInfo.Directory.Exists)
                {
                    throw new FileNotFoundException("Could not find the file with the path " + filePath);
                }

                FilePath = filePath;

                Info = new FileInfo(filePath);
                Info.LastAccessTimeUtc = DateTime.Now;
            }

            Header = new string[0];
            _data = new List<string[]>();
        }

        /// <summary>
        /// handles the imported data of the file
        /// </summary>
        /// <param name="header">the header</param>
        /// <param name="data">the associated data</param>
        public DataFileInstance(string filepath, string[] header, List<string[]> data, bool openOnly = true)
        {
            if (filepath != null)
            {

                if (File.Exists(filepath))
                {
                    FilePath = filepath;

                    Info = new FileInfo(filepath);
                    Info.LastAccessTimeUtc = DateTime.Now;
                }
                else
                {
                    if (openOnly)
                        throw new FileNotFoundException("Could not find the file with the path " + filepath);
                    else
                    {

                        File.Create(filepath);
                        Info = new FileInfo(filepath);
                        Info.LastAccessTimeUtc = DateTime.Now;
                    }
                }
            }

            Header = header;
            _data = data;

            // Debug.Log(data.Count);

            // int i = 0;

            // foreach(var d in _data)
            // {
            //     UnityEngine.Debug.Log( i + ") " + string.Join(", ", d));
            //     i++;
            // }
        }

        /// <summary>
        /// Get the data file instance from a csv file path
        /// </summary>
        /// <param name="path">the csv file path</param>
        /// <returns>returns the DataFileInstance</returns>
        /// <exception cref="NullReferenceException">thrown when the <paramref name="path"/> string is null</exception>
        /// <exception cref="ArgumentException">thrown when the <paramref name="path"/> is an empty string</exception>
        public static DataFileInstance DeserializeCSVFile(string path)
        {
            if (path == null)
                throw new System.NullReferenceException(path);

            if (path == string.Empty)
                throw new System.ArgumentException("The path is empty");

            return CSVSerialization.Deserialize(path);
        }

        /// <summary>
        /// Checks to see if the header is null or empty
        /// </summary>
        /// <param name="file">the file to check</param>
        /// <returns></returns>
        public static bool IsEmptyHeader(DataFileInstance file)
        {
            if (file.Header == null || file.Header == new string[0])
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Export all data in 2d array form
        /// </summary>
        /// <returns></returns>
        public List<string[]> GetAllData()
        {
            List<string[]> final = new List<string[]>();

            for (int i = 0; i < _data.Count; i++)
            {
                final.Add(_data[i]);
            }

            return final;
        }

        /// <summary>
        /// Get the specific data from a specific row
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public string[] GetData(int row)
        {

            if (row >= _data.Count)
                throw new IndexOutOfRangeException("The row is out of range of the data string. " +
                "Row requested: " + row + ". Actual data count: " + _data.Count);

            return _data[row];
        }


        /// <summary>
        /// Attempt to get data if possible
        /// </summary>
        /// <param name="row">the row location of the data</param>
        /// <param name="result">the result of the data</param>
        /// <returns>returns true if the data was found, false if not</returns>
        public bool TryGetData(int row, out string[] result)
        {
            if (_data.Count <= row || row < 0)
            {
                result = new string[0];
                return false;
            }
            else
            {
                result = _data[row];
                return true;
            }
        }

        #region legacy

        /// <summary>
        /// Append the file
        /// </summary>
        /// <param name="str">the string array to append</param>
        /// <remarks>does not save the file to the server</remarks>
        // public void AppendNewRow(string[] str)
        // {

        //Debug.Log("appending row " + string.Join(",", str));

        // string[] final = str;
        // _data.Add(final);

        //Debug.Log("done appending row " + string.Join(",", str));
        // }

        /// <summary>
        /// Replace a row in the file
        /// </summary>
        /// <param name="row">the location of the row in the file (starts at 1 and goes to the count of the list)</param>
        /// <param name="data">the string array to convert</param>
        /// <remarks>does not save the file to the server</remarks>
        /// <exception cref="ArgumentOutOfRangeException">thrown when the location peramater is out of range of the data</exception>
        // public void ReplaceRow(string[] data)
        // {

        // if (row == string.Empty) throw new ArgumentOutOfRangeException("The " + nameof(row) + " parameter is empty for the data list " + row);


        // //Debug.Log("Replacing row " + row + ") " + string.Join(", ", str) + " " + _data.Count);

        // // _data.RemoveAt(row);
        // _data.Remove(data);
        // _data.Add(data);

        // if (row < _data.Count)
        //     _data.Insert(row, str);
        // else
        //     _data.Add(str);

        //Debug.Log(" Done Replacing row " + row + ") " + string.Join(", ", str) + " " + _data.Count);
        // }

        /// <summary>
        /// Delete a row in the file
        /// </summary>
        /// <param name="row">the row</param>
        // public void RemoveRow(string row, string[] data)
        // {

        // if (row < 0 || row >= _data.Count)
        // {
        //     throw new IndexOutOfRangeException("The " + nameof(row) + " parameter is out of range of the data list " + row);
        // }

        // if (row == null || row.Trim().Length < 1)
        //     throw new ArgumentException("the row id cannot be null or empty");

        //Debug.Log("Removing row " + row + ") " + string.Join(", ", _data[row]));

        // _data.RemoveAt(row);

        // _data.Remove(data);
        // }

        #endregion

        /// <summary>
        /// Save the file
        /// </summary>
        /// <returns></returns>
        public bool SerializeFile()
        {
            if (Info != null)
                Info.LastWriteTime = DateTime.Now;
            else
                return false;

            //Debug.Log("Serializing " + this.FileName);

            return CSVSerialization.Serialize(this);
        }

        /// <summary>
        /// Replace information in the data file instance
        /// </summary>
        /// <param name="location">the location</param>
        /// <param name="replaceStr">the replace string</param>
        public void ReplaceData(int location, string[] replaceStr)
        {
            if (_data.Count > location && location >= 0)
                _data[location] = replaceStr;
        }

        /// <summary>
        /// Remove the information from the data file instance
        /// </summary>
        /// <param name="location">the location of the data</param>
        public void RemoveData(int location)
        {
            if (_data.Count > location && location >= 0)
                _data.RemoveAt(location);
        }

        /// <summary>
        /// Replace the data
        /// </summary>
        /// <param name="dataEntries"></param>
        public void ReplaceData(IEnumerable<DataEntry> dataEntries)
        {

            // Debug.Log("getting ready to add data");

            _data.Clear();

            List<string[]> data = new List<string[]>();

            foreach (var x in dataEntries)
            {
                var amt = x.GetRawData();
                data.Add(amt);
            }

            // Debug.Log("added data");

            _data = data;
        }

        public void ReplaceData(List<string[]> newData)
        {
            _data.Clear();
            _data = newData;
        }

        /// <summary>
        /// find all rows of data that contain <paramref name="text"/> in the specific row
        /// </summary>
        /// <param name="text">the text to search for</param>
        /// <param name="columnLocation">the specific columnLocation</param>
        /// <returns></returns>
        public List<string[]> FindAll(string text, int columnLocation)
        {
            List<string[]> subData = new List<string[]>();

            subData = _data.FindAll(x => x[columnLocation] == text);

            return subData;
        }
    }
}
