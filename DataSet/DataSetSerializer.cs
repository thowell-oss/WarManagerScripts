
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Backend
{
    /// <summary>
    /// Handles serializing the data set
    /// </summary>
    [Notes.Author("Handles serializing the data set")]
    public class DataSetSerializer
    {
        /// <summary>
        /// Serialize the data set
        /// </summary>
        /// <param name="set">the data set to serialize</param>
        public void SerializeDataSet(DataSet set)
        {
            var jsonData = set.GetDatasetJson();

            Debug.Log(jsonData);

            // FileInfo path = new FileInfo(set.DataSetLocation);

            // if (path.Directory.Exists)
            // {
            //     File.WriteAllText(set.DataSetLocation, jsonData);
            // }
        }
    }
}
