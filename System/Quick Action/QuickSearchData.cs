
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StringUtility;

namespace WarManager.Backend
{
    /// <summary>
    /// Quick Search Data For Each DataSet
    /// </summary>
    public class QuickSearchData
    {
        /// <summary>
        /// The name of the dataset
        /// </summary>
        /// <value></value>
        public string DataSetName { get; set; }

        /// <summary>
        /// the color of the dataset
        /// </summary>
        /// <value></value>
        public Color DataSetColor { get; set; }

        private int ColumnToSearch { get; set; } = 0;

        /// <summary>
        /// the data set search dictionary
        /// </summary>
        /// <typeparam name="string[]"></typeparam>
        /// <typeparam name="DataEntry"></typeparam>
        /// <returns></returns>
        private Dictionary<IList<string>, DataEntry> _dataSetSearchDictionary = new Dictionary<IList<string>, DataEntry>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataSetName">the dataset name</param>
        /// <param name="dataSetColor">the dataset color</param>
        /// <param name="data">the dataset search dictionary</param>
        public QuickSearchData(string dataSetName, Color dataSetColor, Dictionary<IList<string>, DataEntry> data, int colToSearch = 0)
        {

            if (dataSetName == null)
                throw new NullReferenceException("the data cannot be null");

            if (dataSetColor == null)
                throw new NullReferenceException("the dataset color is null");

            if (dataSetName == string.Empty)
                throw new ArgumentException("the dataset name cannot be empty");

            if (data == null)
                throw new NullReferenceException("the data cannot be null");

            DataSetName = dataSetName;
            DataSetColor = dataSetColor;
            _dataSetSearchDictionary = data;

            ColumnToSearch = colToSearch;
        }

        /// <summary>
        /// Get the top entries from the search string
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public List<(double, DataEntry, int colToSearch)> GetTopEntries(string searchString)
        {
            List<(double, DataEntry, int colToSearch)> data = new List<(double, DataEntry, int colToSearch)>();

            foreach (var x in _dataSetSearchDictionary)
            {
                double topSearch = 0;
                string topResult = "";

                foreach (var entryData in x.Key)
                {
                    double percentResult = searchString.CalculateSimilarity(entryData);
                    if (topSearch < percentResult)
                    {
                        topSearch = percentResult;
                        topResult = entryData;
                    }
                }

                if (topResult != string.Empty)
                {
                    data.Add((topSearch, x.Value, ColumnToSearch));
                }
            }

            return data;
        }
    }
}
