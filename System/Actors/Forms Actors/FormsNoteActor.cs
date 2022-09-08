using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{

    public sealed class FormsNoteActor : Actor
    {
        public override string Name => "Note Actor (Forms)";

        public override string Description => "Displays notes for users to see - used in forms";

        public override string DataSetID => "ace803e7-f1ba-4426-af7f-459fb82046a2";


        private DataEntry entry;
        private DataSet set;

        public override DataEntry GetDataEntry(string RowID, string args)
        {

            set = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(),
            new List<string>() { "Note" }, new string[2] { "id", "Note" });

            entry = new DataEntry(RowID, GetDefaultDataValues(RowID).ToArray(), set)
            {
                Actor = new FormsNoteActor()
            };

            return entry;
        }

        public override void RemoveEntry()
        {
            if (set != null && entry != null)
                set.RemoveEntry(entry);
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            DataValue value = new DataValue((1, rowID), "Note", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.ViewOnly);

            return new List<DataValue>()
             {value};
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {3,2.5},
                    ColorHex = "#eee"
                },
                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#111",
                    SetColumns = new List<int>() {1},
                }
            };

            return data;

        }
    }
}