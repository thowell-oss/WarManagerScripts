using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{

    public class TitleCardActor : Actor
    {
        public override string Name => "Title";

        public override string Description => "Create a title to be posted on the sheet.";

        public override string DataSetID => "75b3577a-2a0e-4951-aed7-fc429b521d4f";

        private DataSet _dataSet;
        private DataEntry _entry;

        public override DataEntry GetDataEntry(string rowID, string args)
        {
            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), new List<string>() { "title" }, new string[2] { "id", "title" });
            if (_dataSet.EntryExists(rowID))
            {
                _entry = _dataSet.GetEntry(rowID);
                _entry.Actor = this;

                return _entry;
            }

            _entry = CreateNewDataEntry<TitleCardActor>(rowID, _dataSet, this);
            return _entry;
        }

        public override void RemoveEntry()
        {
            if (_entry != null)
                _dataSet.RemoveEntry(_entry);
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            return new List<DataValue>()
            {
                new DataValue((0, rowID), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((1, rowID), "title", "New Title Card", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
            };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {9, 2.75},
                    ColorHex = "#222"
                },

                new CardElementTextData()
                {
                    Location = new double[3] {-17, 0, 1},
                    Scale = new double[2] {14,5},
                    TextJustification = "Left",
                    FontSize = 21,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {1},
                    MultiColumnString = " ",
                    Bold = true
                },
            };

            return data;
        }
    }
}
