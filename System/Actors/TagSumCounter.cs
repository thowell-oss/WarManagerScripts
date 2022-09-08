using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    [Notes.Author("Count the sum of a selection of cards")]
    public sealed class TagSumCounter : Actor
    {

        public override string Name => "Tag Sum";

        public override string Description => "Count the sum of a selection of cards";

        public override string DataSetID => "69d9e1a9-1b32-4fb8-a7d5-2a2f3c6a36ef";

        private int lastAmt = -1;

        private DataSet _dataSet;
        private DataEntry _entry;

        public override void Act()
        {
            base.Act();

            if (Card == null)
                return;

            int sumCount = 0;

            string tag = Card.Entry.GetValueAt(2).ParseToParagraph().ToLower().Trim();

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var cards = sheet.GetAllObj();

                foreach (var x in cards)
                {
                    if (x != this.Card)
                    {
                        List<string> values = x.Entry.GetAllowedValues();

                        for (int i = 0; i < values.Count; i++)
                        {
                            if (values[i].ToLower().Trim() == tag)
                            {
                                sumCount++;
                            }
                        }
                    }
                }
            }
            else
            {
                sumCount = -1;
            }

            if (lastAmt != sumCount)
            {
                lastAmt = sumCount;
                // UpdateData(new ValueTypePair(sumCount, ColumnInfo.GetValueTypeOfRational), 1);
            }
        }

        public override DataEntry GetDataEntry(string rowID, string args)
        {
            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(),
            new List<string>() { "Sum", "Selected Tag" }, new string[3] { "Id", "Sum", "Selected Tag" });

            if (_dataSet.EntryExists(rowID))
            {
                _entry = _dataSet.GetEntry(rowID);
                _entry.Actor = this;
                return _entry;
            }

            _entry = CreateNewDataEntry<TagSumCounter>(rowID, _dataSet, this);
            return _entry;

        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            DataValue id = new DataValue((0, rowID), "Id", "", ColumnInfo.GetValueTypeOfWord, ValuePermissions.Full);
            DataValue Sum = new DataValue((1, rowID), "Sum", 0, ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full);
            DataValue Tag = new DataValue((2, rowID), "Selected Tag", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full);

            return new List<DataValue>()
            {
                id, Sum, Tag
            };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {4.5, 1},
                    ColorHex = "#1d1d1d"
                },
                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {1},
                    MultiColumnString = " - ",
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
