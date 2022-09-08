using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using StringUtility;

namespace WarManager
{

    public class DataSetReplaceHandler
    {

        public List<KeyValuePair<string[], List<KeyValuePair<string[], double>>>> MatchingData = new List<KeyValuePair<string[], List<KeyValuePair<string[], double>>>>();

        public Dictionary<string[], string[]> FinalMergeData = new Dictionary<string[], string[]>();
        public List<(string[] oldData, string[] newData)> NotFinalMergeData = new List<(string[] oldData, string[] newData)>();

        private List<string[]> _unusedDataCheck = new List<string[]>();

        public List<string[]> DataToAdd = new List<string[]>();

        public DataFileInstance OldData { get; private set; }
        public DataFileInstance NewData { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldData">the old data</param>
        /// <param name="newData">the new data</param>
        public DataSetReplaceHandler(DataFileInstance oldData, DataFileInstance newData)
        {
            OldData = oldData;
            NewData = newData;
        }

        /// <summary>
        /// Compare headers to make sure the columns are organized correctly before replacing the data
        /// </summary>
        public void CompareHeaders()
        {
            if (OldData.HeaderLength != NewData.HeaderLength)
                throw new System.ArgumentException($"The {NewData.FileName} does not have the same number of columns as {OldData.FileName}.\n{NewData.FileName}: {string.Join(", ", NewData.Header)}\n{OldData.FileName}: {string.Join(", ", OldData.Header)}");

            for (int i = 0; i < OldData.HeaderLength; i++)
            {
                if (OldData.Header[i].Trim().ToLower() != NewData.Header[i].Trim().ToLower())
                {
                    throw new System.ArgumentException($"The {NewData.FileName} at column number {i + 1} is not spelled the same as the data set column. " +
                   "{NewData.FileName}: {NewData.Header[i]}, {OldData.FileName}: {OldData.Header[i]}. Check to make sure each column from the new data matches the dataset");
                }
            }
        }

        /// <summary>
        /// Calculate the merge data
        /// </summary>
        /// <param name="optionThreshold">the threshold that the data could be considered similar</param>
        /// <param name="oldData">the old data set info</param>
        /// <param name="newData">the new data</param>
        public void CalculateMerge(double optionThreshold, double automaticMergeThreshold)
        {

            _unusedDataCheck.AddRange(NewData.Data);

            foreach (var oldInfo in OldData.Data)
            {
                List<KeyValuePair<string[], double>> categorizedData = new List<KeyValuePair<string[], double>>();

                foreach (var newInfo in NewData.Data)
                {

                    string first = string.Join(" ", newInfo);
                    string last = string.Join(" ", oldInfo);

                    var final = first.CalculateSimilarity(last);

                    if (final >= optionThreshold)
                    {
                        categorizedData.Add(new KeyValuePair<string[], double>(newInfo, final));
                        //Debug.Log(final + " " + string.Join(", ", newInfo));
                    }
                }

                categorizedData.Sort((x, y) =>
                {
                    return -x.Value.CompareTo(y.Value);
                });


                MatchingData.Add(new KeyValuePair<string[], List<KeyValuePair<string[], double>>>(oldInfo, categorizedData));

            }

            HandleMerge(automaticMergeThreshold);

        }


        /// <summary>
        /// Actually calculate the merging process
        /// </summary>
        /// <param name="maticMergeThreshold">the percent amount when the merging should happen</param>
        private void HandleMerge(double automaticMergeThreshold)
        {
            foreach (var x in MatchingData)
            {
                var data = x.Key;

                if (x.Value.Count > 0 && x.Value[0].Value > automaticMergeThreshold)
                {
                    FinalMergeData.Add(x.Key, x.Value[0].Key);
                    _unusedDataCheck.Remove(x.Value[0].Key);
                }
                else
                {
                    if (x.Value.Count > 0)
                        NotFinalMergeData.Add((x.Key, x.Value[0].Key));
                    else
                        NotFinalMergeData.Add((x.Key, new string[0]));
                }
            }

            while (_unusedDataCheck.Count > 0)
            {
                NotFinalMergeData.Add((new string[0], _unusedDataCheck[0]));
                _unusedDataCheck.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get the new data file instance after running calculations
        /// </summary>
        /// <returns></returns>
        public DataFileInstance GetNewDataFileInstance(out List<string[]> dataAdded, out List<string[]> dataRemoved)
        {
            List<string[]> mergeData = new List<string[]>();

            List<string[]> added = new List<string[]>();
            List<string[]> removed = new List<string[]>();

            foreach (var x in FinalMergeData)
            {

                string[] nextData = x.Value;
                nextData[0] = x.Key[0];

                mergeData.Add(nextData);
            }

            foreach (var y in NotFinalMergeData)
            {
                if (y.oldData.Length == 0)
                {
                    mergeData.Add(y.newData);
                    added.Add(y.newData);
                }
                else
                {
                    removed.Add(y.oldData);
                }
            }



            DataFileInstance newInstance = new DataFileInstance(OldData.FilePath, OldData.Header, mergeData);

            dataAdded = added;
            dataRemoved = removed;

            return newInstance;
        }
    }
}