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

    public class test_csvserialization
    {

        private static readonly string csvFilePath = @"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\CSV Data\Job Data Fix.csv";

        #region drivers

        // A Test behaves as an ordinary method
        [Test]
        [Timeout(5)]
        public void test_EmptyFilePath_notNull()
        {
            DataFileInstance d = CSVSerialization.Deserialize("");
            Assert.AreNotEqual(d, null);
        }

        [Test]
        [Timeout(5)]
        public void test_nonEmptyFilePath_nonEmptyHeader()
        {
            var d = action_ImportCSVFile();
            Assert.AreNotEqual(d.Header, null);
            Assert.AreNotEqual(d.Header.Length, 0);
        }

        [Test]
        [Timeout(5)]
        public void test_nonEmptyFilePath_nonEmptyData()
        {
            var d = action_ImportCSVFile();

            var data = d.GetAllData();

            Assert.AreNotEqual(data, null);
            Assert.AreNotEqual(data.Count, 0);

            for (int i = 0; i < data.Count; i++)
            {
                Assert.AreNotEqual(data[i], null);
                Assert.AreNotEqual(data[i], 0);
            }
        }

        #region old

        // [Test]
        // [Timeout(1)]
        // public void test_replaceEntry_allDataSameExceptForEntry()
        // {

        //     var someFile = action_ImportCSVFile();
        //     var data = someFile.GetAllData();

        //     testAction_rangeDataCorrectAfterReplacingDataInCertianRange(0, 20);
        //     testAction_rangeDataCorrectAfterReplacingDataInCertianRange(data.Count - 21, data.Count - 1);
        // }

        // [Test]
        // [Timeout(1)]
        // public void test_addEntry_DataCountOneGreaterThanOldDataCount()
        // {

        //     var someFile = action_ImportCSVFile();
        //     var data = someFile.GetAllData();

        //     // someFile.AppendNewRow(new string[0]);

        //     var newData = someFile.GetAllData();

        //     Assert.GreaterOrEqual(newData.Count, data.Count + 1);
        //     Assert.AreEqual(newData.Count, data.Count + 1);
        // }

        // [Test]
        // [Timeout(1)]
        // public void test_addEntry_DataCorrect()
        // {
        //     var someFile = action_ImportCSVFile();
        //     var data = someFile.GetAllData();

        //     var len = data[0].Length;

        //     string[] str = new string[len];

        //     List<string[]> newData = new List<string[]>();

        //     // UnityEngine.Debug.Log("starting loop");

        //     for (int l = 0; l < 1; l++)
        //     {

        //         str[l] += "test";

        //         newData.Add(str);

        //         // someFile.AppendNewRow(str);
        //     }

        //     Assert.AreEqual(someFile.DataCount, data.Count + 1); // check the count of the two data lists
        //     Assert.AreEqual(someFile.GetAllData()[someFile.DataCount - 1].Length, newData[newData.Count - 1].Length); // check the length of the strings in both data lists
        //     Assert.IsTrue(someFile.GetAllData()[someFile.DataCount - 1] == str); // check that the data in both lists is the same
        // }

        // [Test]
        // [Timeout(2)]
        // public void test_ReplaceData_DataReplacedSuccessfully()
        // {
        //     string id = Guid.NewGuid().ToString();

        //     string[] testCase = new string[3] { id, "test", "test" };

        //     DataFileInstance someFileInstance = action_ImportCSVFile();

        //     var checkData = someFileInstance.GetAllData();

        //     // someFileInstance.ReplaceRow(id, testCase);

        //     string[] data = someFileInstance.GetData(1);

        //     Assert.AreEqual(testCase.Length, data.Length);

        //     for (int i = 0; i < data.Length; i++)
        //     {
        //         Debug.Log(testCase[i] + " " + data[i]);
        //         Assert.AreEqual(testCase[i], data[i]);
        //     }

        //     Assert.Greater(someFileInstance.DataCount, 3);
        //     Assert.AreEqual(checkData[3], someFileInstance.GetAllData()[3]);
        // }

        // [Test]
        // [Timeout(2)]
        // public void test_SaveFileAfterReplace_DataSavedAndRepopulated()
        // {
        //     string id = Guid.NewGuid().ToString();

        //     string[] testCase = new string[3] { id, "test", "test" };

        //     DataFileInstance somefile = action_ImportCSVFile();
        //     // somefile.ReplaceRow(id, testCase);

        //     somefile.SerializeFile();
        //     var newFile = action_ImportCSVFile();

        //     var data = newFile.GetAllData();

        //     Debug.Log(somefile.DataCount);
        //     Debug.Log(newFile.DataCount);

        //     Assert.IsTrue(newFile.DataCount > 0);
        //     Assert.IsTrue(newFile.DataCount == somefile.DataCount);
        // }

        #endregion

        [Test]
        [Timeout(2)]
        public void test_CantOpenFileTest()
        {
            var file = CSVSerialization.Deserialize(@"D:\War Manager Concept\serverChangeTestLocation\Test_T_Drive\CSV Data\Users.csv");
            Assert.Greater(file.DataCount, 0);
        }


        #endregion

        #region portable Test/Actions

        // private void testAction_rangeDataCorrectAfterReplacingDataInCertianRange(int start, int count)
        // {
        //     for (int j = start; j < count; j++)
        //     {

        //         //  UnityEngine.Debug.Log(j);

        //         var file = action_ImportCSVFile();

        //         var oldData = file.GetAllData();

        //         // file.ReplaceRow(Guid.NewGuid().ToString(), new string[0]);

        //         var newData = file.GetAllData();

        //         for (int i = 0; i < newData.Count; i++)
        //         {
        //             if (i != j)
        //             {
        //                 Assert.IsTrue(AreSame(oldData[i], newData[i]));
        //             }
        //         }

        //         // UnityEngine.Debug.Log("All other files checked");

        //         Assert.IsEmpty(newData[j]);
        //     }
        // }

        // [Test]
        // public void test_csvserializationCorrect()
        // {
        //     string str = CSVSerialization.ConvertCellToCSV("\"johnny, jhunn");

        //     Debug.Log(str);

        //     Assert.IsTrue(str == "\"\"\"johnny, jhunn\"");
        // }

        [Test]
        public void test_csvDeserialization()
        {
            string data = "\"\"\"johnny, jhunn\"\"\"";


            List<string[]> strList = CSVSerialization.ParseCSVBytes(data);

            //Debug.Log(strList[0][0]);

            Assert.True(strList[0][0] == "\"johnny, jhunn\"");
        }

        #endregion


        #region portable actions

        private DataFileInstance action_ImportCSVFile()
        {
            return CSVSerialization.Deserialize(csvFilePath);
        }

        private bool AreSame(string[] a, string[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        #endregion


    }
}
