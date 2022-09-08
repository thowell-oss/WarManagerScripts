/* Import_CSV.cs
 * Author: Taylor Howell
 */

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Sharing
{
    /// <summary>
    /// Handles importing CSV files and getting information from the file after import
    /// </summary>
    public class Import_CSV
    {

        /// <summary>
        /// the information that was imported
        /// </summary>
        public string[] ImportedInfo { get; private set; }

        /// <summary>
        /// Get the header row which tells what each column of information is supposed to be
        /// </summary>
        public string[] HeaderRow
        {
            get
            {
                return GetLine(0);
            }
        }

        /// <summary>
        /// The width of the file
        /// </summary>
        public int Width
        {
            get
            {
                return HeaderRow.Length;
            }
        }

        /// <summary>
        /// The height of the file
        /// </summary>
        public int Height
        {
            get
            {
                return ImportedInfo.Length;
            }
        }

        /// <summary>
        /// Import a csv file with a given file name
        /// </summary>
        /// <param name="fileName">the given file name</param>
        /// <returns>returns a string builder array with all the given text. Each array element corresponds to a line</returns>
        public string[] Import(string fileName)
        {
            if (!File.Exists(fileName))
            {
                ImportedInfo = null;
                Debug.LogError("File Not Found");
                throw new FileNotFoundException($"Cannot find file at location {fileName}");
            }

            List<string> info = new List<string>();

            using (Stream stream = File.OpenRead(fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        info.Add(str);
 
                    }
                }
            }

           

            ImportedInfo = info.ToArray();
            return info.ToArray();
        }

        /// <summary>
        /// Removes reference to the imported file
        /// </summary>
        public void Clear()
        {
            ImportedInfo = null;
        }

        /// <summary>
        /// Attempt to get the tag using coordinates
        /// </summary>
        /// <param name="row">the row of the desired cell</param>
        /// <param name="column">the column of the desired cell</param>
        /// <param name="title">the title of the tag</param>
        /// <param name="result">teh result of the tag</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public bool TryGetTag(int column, int row, out string title, out string result)
        {
            string t = GetCell(column, 0);
            string info = GetCell(column, row);

            if (t != null && info != null)
            {
                title = t;
                result = info;
                return true;
            }

            title = null;
            result = null;

            return false;
        }

        /// <summary>
        /// Get a line from the imported csv file
        /// </summary>
        /// <param name="row">the row of cells to return</param>
        /// <returns>returns an array string</returns>
        public string[] GetLine(int row)
        {
            if (ImportedInfo != null)
            {
                if (ImportedInfo.Length > row)
                {
                    string line = ImportedInfo[row];
                    return line.Split(',');
                }
            }

            return null;
        }

        /// <summary>
        /// Get a specfic cell information
        /// </summary>
        /// <param name="row">the row cell location</param>
        /// <param name="col">the col cell location</param>
        /// <returns>returns the string if possible, null if not </returns>
        public string GetCell(int col, int row)
        {
            string[] line = GetLine(row);

            if (line != null)
            {
                if (line.Length > col)
                {
                    return line[col];
                }
            }

            return null;
        }
    }
}

