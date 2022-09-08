using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    [Notes.Author("The sum card counts all cards on the sheet")]
    public sealed class SumCardActor : Actor
    {
        public override string Name => "Card Counter";

        public override string Description => "The sum card counts all cards on the sheet";

        public override string DataSetID => "941d5a04-03fe-4bfe-b4d1-f73440f02e79";

        private int _lastAmt = -1;

        private DataSet _dataSet;
        private DataEntry _entry;

        /// <summary>
        /// Get the list of data sets
        /// </summary>
        /// <returns></returns>
        public List<DataSet> GetDataSetsList()
        {
            if (SheetsManager.SheetCount < 1)
                return new List<DataSet>();

            if (Card == null)
                return new List<DataSet>();

            if (SheetsManager.TryGetActiveSheet(Card.SheetID, out var sheet))
            {
                return WarSystem.DataSetManager.GetDataSetsFromSheet(sheet.ID, false);
            }

            return new List<DataSet>();
        }

        public override void Act()
        {
            base.Act();

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {

                string DataSetType = Card.Entry.GetValueAt(2).ToString();

                if (DataSetType.Trim() == string.Empty)
                {
                    int amt = sheet.CardCount;

                    if (_lastAmt != amt)
                    {
                        _lastAmt = amt;

                        Card.Entry.UpdateValueAt(new ValueTypePair("<size=20>" + amt + "</size>", ColumnInfo.GetValueTypeOfParagraph), 1);
                        UpdateUI();
                        SaveInfo(Card.DataSet, Card.Entry);
                    }
                }
                else
                {

                    int count = 0;

                    foreach (var x in sheet.GetAllObj())
                    {
                        if (x.DataSet.DatasetName.ToLower().Trim() == DataSetType.ToLower().Trim())
                        {
                            count++;
                        }
                    }

                    if (_lastAmt != count)
                    {
                        _lastAmt = count;

                        Card.Entry.UpdateValueAt(new ValueTypePair("<size=20>" + count + "</size>\n", ColumnInfo.GetValueTypeOfParagraph), 1);
                        UpdateUI();
                        SaveInfo(Card.DataSet, Card.Entry);
                    }
                }
            }
        }

        public override DataEntry GetDataEntry(string RowID, string args)
        {

            //DataFileInstance instance = new DataFileInstance(null, new string[3] { "id", "sum", "Data Set" }, new List<string[]>() { new string[1] { "" } });

            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), new List<string>() { "sum", "Data Set" }, new string[3] { "id", "sum", "Data Set" });
            if (_dataSet.EntryExists(RowID))
            {
                _entry = _dataSet.GetEntry(RowID);
                _entry.Actor = this;
                return _entry;
            }

            _entry = CreateNewDataEntry<SumCardActor>(RowID, _dataSet, this);
            return _entry;
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {

            return new List<DataValue>()
            {
                new DataValue((0, rowID), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((1, rowID), "sum", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((2, rowID), "Data Set", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full, "Specify the Data Set of cards to count"),
            };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {4.5, 2.75},
                    ColorHex = "#1d1d1d"
                },

                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {1, 2},
                    MultiColumnString = " "
                },
            };

            return data;
        }

        public override void RemoveEntry()
        {
            if (_entry != null)
                _dataSet.RemoveEntry(_entry);
        }
    }
}
