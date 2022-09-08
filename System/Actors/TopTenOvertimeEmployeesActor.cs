using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{

    /// <summary>
    /// Calculates the top 10 team members with the most clocked time this week
    /// </summary>
    [Notes.Author("Calculates the top 10 team members with the most clocked time this week")]
    public sealed class TopTenOvertimeEmployeesActor : Actor
    {
        public override string Name => "Top 10 Highest Clocked Hours";

        public override string Description => "This card shows the top ten employees with the most clocked time";

        public override string DataSetID => "140c20a3-fb27-40c2-b8f4-1da2377cac65";

        private DataEntry _entry;
        private DataSet _dataSet;

        private string teamMemebersDataSetID = "326bd75f-12cc-407c-8275-09ce8b5206dd";
        private string teamNameColumn = "Name";
        private string hoursTagColumnName = "Hours";


        public override void OnAwake()
        {
            base.OnAwake();

            if (Card.SheetID == SheetsManager.CurrentSheetID)
            {
                Debug.Log("recalculating sheet");
                Recalculate();
            }
        }

        public override void OnChangeSheet()
        {
            base.OnChangeSheet();

            if (Card.SheetID == SheetsManager.CurrentSheetID)
            {
                Debug.Log("recalculating sheet");
                Recalculate();
            }
        }

        /// <summary>
        /// recalculate the values of the top 10
        /// </summary>
        private bool Recalculate()
        {
            var dataSets = WarSystem.DataSetManager.GetDataSetsFromSheet(Card.SheetID, false);
            var teamMembersDataSet = dataSets.Find(x => x.ID == teamMemebersDataSetID);

            List<KeyValuePair<double, string>> topTenTeam = new List<KeyValuePair<double, string>>();

            if (teamMembersDataSet != null)
            {
                foreach (var x in teamMembersDataSet.Entries)
                {
                    string name = "";
                    string hours = "0";

                    if (x.TryGetValueWithHeader(teamNameColumn, out var value))
                    {
                        name = value.ParseToParagraph();
                    }

                    if (x.TryGetValueWithHeader(hoursTagColumnName, out var value1))
                    {
                        hours = value1.ParseToParagraph();
                    }

                    if (double.TryParse(hours, out var hrs) && name != null && name != string.Empty)
                    {
                        //Debug.Log(hrs + " " + name);
                        topTenTeam.Add(new KeyValuePair<double, string>(hrs, name));
                    }
                }

                if (topTenTeam.Count > 0)
                {
                    topTenTeam.Sort((x, y) =>
                    {
                        return -x.Key.CompareTo(y.Key);
                    });


                    int i = 0;

                    List<(ValueTypePair, int loc)> data = new List<(ValueTypePair, int loc)>();

                    while (i < topTenTeam.Count && i < 10)
                    {
                        var dataPair = new ValueTypePair(topTenTeam[i].Key, ColumnInfo.GetValueTypeOfParagraph);
                        data.Add((dataPair, i + 1));

                        var namePair = new ValueTypePair(topTenTeam[i].Value, ColumnInfo.GetValueTypeOfParagraph);
                        data.Add((namePair, i + 11));

                        i++;
                    }

                    UpdateData(data);

                    //SaveInfo(_dataSet, _entry);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Could not find the team members dataset " + teamMemebersDataSetID);
#endif
            }

            return true;
        }

        public override DataEntry GetDataEntry(string rowID, string args)
        {

            var list = new List<string>() { "one", "two", "three", "four", "five",
            "six", "seven", "eight", "nine", "ten", "name one", "name two", "name three", "name four", "name five",
            "name six", "name seven", "name eight", "name nine", "name ten" };

            var allTitles = new List<string>();
            allTitles.AddRange(list);
            allTitles.Insert(0, "id");

            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), list, allTitles.ToArray());

            if (_dataSet.EntryExists(rowID))
            {
                var entry = _dataSet.GetEntry(rowID);
                entry.Actor = this;

                return entry;
            }

            _entry = CreateNewDataEntry<TopTenOvertimeEmployeesActor>(rowID, _dataSet, this);
            return _entry;
        }

        public override void RemoveEntry()
        {
            if (_dataSet != null && _entry != null)
            {
                _dataSet.RemoveEntry(_entry);
            }
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            return new List<DataValue>()
            {
                new DataValue((0, rowID), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((1, rowID), "one", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((2, rowID), "two", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((3, rowID), "three", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((4, rowID), "four", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((5, rowID), "five", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((6, rowID), "six", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((7, rowID), "seven", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((8, rowID), "eight", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((9, rowID), "nine", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((10, rowID), "ten", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((11, rowID), "name one", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((12, rowID), "name two", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((13, rowID), "name three", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((14, rowID), "name four", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((15, rowID), "name five", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((16, rowID), "name six", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((17, rowID), "name seven", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((18, rowID), "name eight", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((19, rowID), "name nine", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((20, rowID), "name ten", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
            };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {11, 18},
                    ColorHex = "#222"
                }
            };



            for (int i = 0; i < 10; i++)
            {
                var loadingBarElement = new LoadingBarElementData()
                {
                    UseGradientColor = false,
                    Scale = new double[] { 5, 26 },
                    Rotation = new double[] { -90, 0 },
                    BarColorHex = "#f22",
                    BarColorGradientTop = "#f22",
                    BackgroundColorHex = "#2221",
                    Min = 0,
                    Max = 120,
                    TitleTextFontSize = 12,
                    TitleTextColor = "#2220",
                    UsePercentText = "none",
                    UseLine = false
                };


                loadingBarElement.SetColumns = new List<int>() { i + 1 };
                loadingBarElement.Location = new double[] { 10, 28 - (4.5 * i), 1 };

                data.Add(loadingBarElement);


                var nameTextElement = new CardElementTextData()
                {
                    Scale = new double[2] { 7, 5 },
                    TextJustification = "left",
                    FontSize = 12,
                    ColorHex = "#eee",
                    MultiColumnString = " ",
                    Bold = false,
                    LeadingWords = i + 1 + ") ",
                };

                nameTextElement.SetColumns = new List<int>() { i + 11 };
                nameTextElement.Location = new double[] { -21, 28 - (4.5 * i), 3 };

                var hoursTextElement = new CardElementTextData()
                {
                    Scale = new double[2] { 7, 5 },
                    TextJustification = "left",
                    FontSize = 10,
                    ColorHex = "#eee",
                    MultiColumnString = " ",
                    Italics = true,
                    Bold = false,
                    TrailingWords = " hours"
                };

                hoursTextElement.SetColumns = new List<int>() { i + 1 };
                hoursTextElement.Location = new double[] { 0, 28 - (4.5 * i), 3 };

                data.Add(hoursTextElement);
                data.Add(nameTextElement);
            }

            var titleTextElement = new CardElementTextData()
            {
                Scale = new double[2] { 9, 5 },
                TextJustification = "left",
                FontSize = 14,
                ColorHex = "#eee",
                MultiColumnString = " ",
                Bold = true,
                SetColumns = new List<int>() { -1 },
                LeadingWords = "Top Ten Highest Hours",
                Location = new double[] { -21, 32, 1 },
            };

            data.Add(titleTextElement);

            return data;

        }
    }
}
