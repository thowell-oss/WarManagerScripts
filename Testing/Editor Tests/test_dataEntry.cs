using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager.Testing.Unit
{

    [Notes.Author("Handles the testing of the data recognition/caching system")]
    public class test_dataEntry : MonoBehaviour
    {
        [Test]
        [Timeout(1)]
        public void test_createEmptyEntry_verifyEmpty()
        {
            DataEntry entry = new DataEntry();

            Assert.IsTrue(entry.Empty);
            Assert.IsTrue(entry.RowID == string.Empty);
            Assert.IsTrue(entry.ValueCount == 0);
        }

        [Test]
        [Timeout(1)]
        public void test_createEmptyDataValueWithFullPermissions_VerifyPermissionsWithEmpty()
        {
            DataValue value = new DataValue(ValuePermissions.Full);

            Assert.NotNull(value);

            Assert.IsTrue(value.Permissions == ValuePermissions.Full);
            Assert.IsTrue(value.Empty);
        }

        [Test]
        [Timeout(1)]
        public void test_createEmptyDataValueWithViewPermissions_VerifyPermissionsWithEmpty()
        {
            DataValue value = new DataValue(ValuePermissions.ViewOnly);
            Assert.IsTrue(value.Permissions == ValuePermissions.ViewOnly);
            Assert.IsTrue(value.Empty);
        }

        [Test]
        [Timeout(1)]
        public void test_createEmptyDataValueWithNoPermissions_VerifyPermissionsWithEmpty()
        {
            DataValue value = new DataValue(ValuePermissions.None);
            Assert.IsTrue(value.Permissions == ValuePermissions.None);
            Assert.IsTrue(value.Empty);
            Assert.IsTrue(value.Empty);
        }

        [Test]
        [Timeout(1)]
        public void test_createNonEmptyEntry_VerifyNonEmpty()
        {
            var data = GetData();

            List<DataValue> values = new List<DataValue>();

            string rowID = Guid.NewGuid().ToString();

            for (int i = 0; i < data.Count; i++)
            {
                values.Add(new DataValue((i, rowID), data[i].Item1, data[i].Item2, data[i].Item3, ValuePermissions.Full));
            }

            DataEntry entry = new DataEntry(rowID, values.ToArray(), null);

            Assert.IsTrue(entry.ValueCount == data.Count);
            Assert.IsFalse(entry.Empty);
            Assert.IsTrue(entry.RowID == rowID);

            DataValue value = entry.GetValueAt(2);

            Assert.IsTrue(value.CellLocation.column == 2);

            Point p = value.ParseToPoint();

            UnityEngine.Debug.Log(p);

            Assert.IsTrue(p == Point.one);

            UnityEngine.Debug.Log("Raw Data: " + string.Join(",", entry.GetRawData()));
        }

        [Test]
        [Timeout(1)]
        public void test_GetEntrySeparateFromWarSystem_SuccessfulNonEmpty()
        {
            var entry = GetTestDataEntry(true);

            Assert.NotNull(entry);
            Assert.NotNull(entry.DataSet);
            Assert.Greater(entry.ValueCount, 0);
        }

        [Test]
        [Timeout(1)]
        public void test_GetEntryAt_SuccessfulRetrieval()
        {
            var entry = GetTestDataEntry(true);

            Assert.IsTrue((int)entry.GetValueAt(0).Value == 1);
            Assert.IsTrue((string)entry.GetValueAt(1).Value == "Eddy");
            Assert.IsTrue((Point)entry.GetValueAt(2).Value == Point.one);
        }

        [Test]
        [Timeout(1)]
        public void test_GetEntryAtWithNoPermissions_UnsuccessfulRetreival()
        {
            var entry = GetTestDataEntry(new List<string>(), false);

            Assert.IsFalse(entry.TryGetValueAt(1, out var value));
            Assert.IsNull(value);
        }

        [Test]
        [Timeout(1)]
        public void test_GetEntryWithHeaderNoPermissions_UnsuccessfulRetrevial()
        {
            var entry = GetTestDataEntry(new List<string>(), false);

            Assert.IsFalse(entry.TryGetValueWithHeader("name", out var value));
            Assert.IsNull(value);
        }

        [Test]
        [Timeout(1)]
        public void test_GetEntryWithHeader_SuccessfulRetrieval()
        {
            var entry = GetTestDataEntry(true);

            Assert.IsTrue(entry.TryGetValueWithHeader("id", out var one));
            Assert.IsTrue((int)one.Value == 1);

            Assert.IsTrue(entry.TryGetValueWithHeader("name", out var two));
            Assert.IsTrue((string)two.Value == "Eddy");

            Assert.IsTrue(entry.TryGetValueWithHeader("location", out var three));
            Assert.IsTrue((Point)three.Value == Point.one);
        }


        [Test]
        [Timeout(1)]
        public void test_UpdateEntry_SuccessfulUpdateOfValue()
        {
            var entry = GetTestDataEntry(true);

            ValueTypePair v = new ValueTypePair(Point.zero, ColumnInfo.GetValueTypeOfPoint);

            DataValue dv = entry.GetValueAt(2);

            entry.UpdateValueAt(v, 2);

            Assert.IsTrue(entry.GetValueAt(2).Value == v.Value);
            Assert.IsFalse(entry.GetValueAt(2).Value != dv.Value);
            Assert.IsTrue(entry.GetValueAt(2) == dv);
        }


        [Test]
        [Timeout(1)]
        public void test_NewDataEntry_SuccesfulAppendToDataSet()
        {
            var entry = GetTestDataEntry(true);

            int count = entry.DataSet.DataCount;

            entry.DataSet.AppendEntry(entry);

            Assert.IsTrue(entry.DataSet.DataCount == count + 1);
            Debug.Log(entry.DataSet.DataCount);
            // Assert.NotNull(entry.DataSet.GetEntry(count));
            //Assert.IsTrue(entry.Values == entry.DataSet.GetEntry(count).Values);

            Assert.NotNull(entry.DataSet.GetEntry(entry.RowID));
            Assert.AreEqual(entry.DataSet.GetEntry(entry.RowID).ValueCount, entry.ValueCount);
            //Assert.Contains(entry.DataSet.GetEntry(entry.RowID).Values, entry.Values.ToArray());
        }

        [Test]
        [Timeout(1)]
        public void test_ReplaceDataEntry_SuccessfulReplaceEntryToDataSet()
        {
            var entry = GetTestDataEntry(true);
            Assert.IsTrue(entry.DataSet.DataCount == 0);
            entry.DataSet.AppendEntry(entry);
            Assert.IsTrue(entry.DataSet.DataCount == 1);

            Debug.Log(entry.ValueCount);

            entry.UpdateValueAt(new ValueTypePair(Point.one, ColumnInfo.GetValueTypeOfPoint), 2);
            Assert.IsTrue(entry.DataSet.GetEntry(entry.RowID).GetValueAt(2).Value == entry.GetValueAt(2).Value);
        }


        [Test]
        [Timeout(1)]
        public void test_UpdateEntryWithTypeMismatch_UnsuccessfulUpdateOfValue()
        {
            var entry = GetTestDataEntry(true);

            ValueTypePair v = new ValueTypePair("test", ColumnInfo.GetValueTypeOfBoolean);

            Assert.Catch<ArgumentException>(() => entry.UpdateValueAt(v, 2));

            Assert.IsFalse(v.Value == entry.GetValueAt(2));
        }


        #region portable actions

        private List<(string, object, string)> GetData()
        {
            var x = ("id", 1, ColumnInfo.GetValueTypeOfInt);

            var y = ("name", "Eddy", ColumnInfo.GetValueTypeOfWord);

            var z = ("location", Point.one, ColumnInfo.GetValueTypeOfPoint);

            List<(string, object, string)> data = new List<(string, object, string)>();
            data.Add(x);
            data.Add(y);
            data.Add(z);

            return data;
        }

        private List<DataValue> GetDataValues(string row)
        {
            var data = GetData();

            List<DataValue> values = new List<DataValue>();


            for (int i = 0; i < data.Count; i++)
            {
                values.Add(new DataValue((i, row), data[i].Item1, data[i].Item2, data[i].Item3, ValuePermissions.Full));
            }

            return values;
        }

        public DataSet GetTestDataSet(List<string> allowedTags, bool canEditData)
        {

            DataSet d = new DataSet("Test", "Test", allowedTags, "body", new List<string>() { "*" }, canEditData, "", "#000",
            1, 2, System.Guid.NewGuid().ToString(), new List<string>() { "*" }, "", new List<DataSetView>() { new DataSetView(true, true) }, new DataFileInstance());

            return d;
        }


        /// <summary>
        /// Get the default data entry for testing
        /// </summary>
        /// <param name="canEdit"></param>
        /// <returns></returns>
        private DataEntry GetTestDataEntry(bool canEdit)
        {
            List<string> allowedTags = new List<string>();

            allowedTags.Add("id");
            allowedTags.Add("name");
            allowedTags.Add("location");

            DataSet d = GetTestDataSet(allowedTags, canEdit);

            string rowId = Guid.NewGuid().ToString();

            return new DataEntry(rowId, GetDataValues(rowId).ToArray(), d);
        }

        private DataEntry GetTestDataEntry(List<string> allowedTags, bool canEdit)
        {
            string rowId = Guid.NewGuid().ToString();
            return new DataEntry(rowId, GetDataValues(rowId).ToArray(), GetTestDataSet(allowedTags, canEdit));
        }

        #endregion
    }
}
