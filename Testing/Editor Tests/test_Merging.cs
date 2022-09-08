using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager.Testing.Unit
{
    [Notes.Author("merge tool testing")]
    public class test_Merging : MonoBehaviour
    {
        [Test, MaxTime(100000)]
        public void Merge()
        {
            string oldFilePath = @"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\CSV Data\Unit Tests\Users1.csv";
            string newFilePath = @"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\CSV Data\Unit Tests\Users2.csv";

            DataFileInstance oldFileInstance = CSVSerialization.Deserialize(oldFilePath);
            DataFileInstance newFileInstance = CSVSerialization.Deserialize(newFilePath);

            DataSetReplaceHandler handler = new DataSetReplaceHandler(oldFileInstance, newFileInstance);
            handler.CalculateMerge(.5, .7);


            // foreach (var x in handler.FinalMergeData)
            // {
            //     UnityEngine.Debug.Log(string.Join(", ", x.Key) + "\n" + string.Join(", ", x.Value));
            // }

            // foreach (var x in handler.NotFinalMergeData)
            // {
            //     UnityEngine.Debug.Log(string.Join(", ", x.oldData) + "\n" + string.Join(", ", x.newData));
            // }

            // UnityEngine.Debug.Log(handler.FinalMergeData.Count + " " + handler.MatchingData.Count);

        }
    }
}