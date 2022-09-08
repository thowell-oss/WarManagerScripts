using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    public sealed class NoteCardActor : Actor
    {
        public override string Name => "Note";

        public override string Description => "Create a note to be posted on the sheet.";

        public override string DataSetID => "af2319cb-e954-4510-bfdb-709acef2b914";

        private DataSet _dataSet;
        private DataEntry _entry;

        public override DataEntry GetDataEntry(string rowID, string args)
        {
            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), new List<string>() { "note", "bold", "italics", "background color",
            "text color", "font size" }, new string[7] { "id", "note", "bold", "italics", "background color", "text color", "font size" });
            if (_dataSet.EntryExists(rowID))
            {
                var entry = _dataSet.GetEntry(rowID);
                entry.Actor = this;

                return entry;
            }

            _entry = CreateNewDataEntry<NoteCardActor>(rowID, _dataSet, this);
            return _entry;
        }

        public override void RemoveEntry()
        {
            if (_entry != null)
                _dataSet.RemoveEntry(_entry);
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {

            //Debug.Log("getting the list of data values");

            return new List<DataValue>()
            {
                new DataValue((0, rowID), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((1, rowID), "note", "New Note Card", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((2, rowID), "bold", "false", ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "Should the note be bold?", new string[2] {"true", "false"}),
                new DataValue((3, rowID), "italics", "true", ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "Should the note be italicized?", new string[2] {"true", "false"}),
                new DataValue((4, rowID), "background color", "#222", ColumnInfo.GetValueTypeOfKeyword, ValuePermissions.Full, "Hex code color of the background"),
                new DataValue((5, rowID), "text color", "#222", ColumnInfo.GetValueTypeOfKeyword, ValuePermissions.Full, "Hex code color of the text"),
                new DataValue((6, rowID), "font size", "12", ColumnInfo.GetValueTypeOfInt, ValuePermissions.Full, "Font size of the note"),
            };
        }

        protected override void OnUpdateData(List<int> updatedColumns)
        {

          //  Debug.Log("on update data");

            var background = GetElementWidthId<CardBackgroundElementData>(0);
            var text = GetElementWidthId<CardElementTextData>(1);

            for (int i = 0; i < updatedColumns.Count; i++)
            {
                if (_entry.TryGetValueAt(updatedColumns[i], out var value))
                {
                    switch (updatedColumns[i])
                    {

                        case 1:
                            // do nothing
                            break;

                        case 2:

                            text.Bold = value.ParseToParagraph() == "true";
                            break;
                        case 3:

                            text.Italics = value.ParseToParagraph() == "true";
                            break;

                        case 4:

                            background.ColorHex = value.ParseToKeyword();
                            break;

                        case 5:

                            text.ColorHex = value.ParseToKeyword();
                            break;

                        case 6:

                            text.FontSize = value.ParseToInt32();
                            break;

                        default:

                            break;
                    }
                }
            }

            base.OnUpdateData(updatedColumns);
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {4.5, 2.75},
                    ColorHex = "#222"
                },

                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {1},
                    MultiColumnString = " ",
                    Italics = true
                },
            };

            return data;
        }
    }
}
