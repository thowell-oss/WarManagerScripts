using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    /// <summary>
    /// Looks for other cards on a sheet to see if there are duplicates
    /// </summary>
    [Notes.Author("looks for other cards on a sheet to see if there are duplicates")]
    public sealed class DuplicateCardFinderActor : Actor
    {
        public override string Name { get => "Duplicate Card Finder"; }
        public override string Description { get => "This card will look for other cards and see if there are duplicates. "; }

        public override string DataSetID => "5bc9541b-27ee-4a70-8d06-cef398325e65";

        private string NoDuplicatesColor = "#1d1d1d";
        private string FoundDuplicateColor = "#2f2f2f";
        private string CardErrorColor = "#222";

        private bool _foundDuplicates = false;

        private DataSet _dataSet;
        private DataEntry _entry;


        private List<string> duplicateCards = new List<string>();

        public override void OnInit(Card card)
        {
            base.OnInit(card);

            //Debug.Log("initialized");

            AdjustBackgroundColor(NoDuplicatesColor);
        }

        public override void TickOnSheet()
        {
            base.TickOnSheet();

            var cards = CardUtility.GetCardsFromCurrentSheet();
            bool foundDuplicate = false;

            List<string> dupCards = new List<string>();

            for (int i = 0; i < cards.Count; i++)
            {
                for (int j = 0; j < cards.Count; j++)
                {
                    if (j != i && cards[i].point != cards[j].point)
                    {
                        if (cards[i].RowID == cards[j].RowID && cards[i].DatasetID == cards[j].DatasetID)
                        {
                            foundDuplicate = true;
                            dupCards.Add(cards[i].Entry.GetAllowedValues()[0]);
                        }
                    }
                }
            }

            if (duplicateCards.Count != dupCards.Count)
            {
                List<(ValueTypePair pair, int loc)> data = new List<(ValueTypePair pair, int loc)>();
                data.Add((new ValueTypePair(string.Join(", ", dupCards), ColumnInfo.GetValueTypeOfParagraph), 1));
                data.Add((new ValueTypePair(dupCards.Count / 2, ColumnInfo.GetValueTypeOfParagraph), 2));
               

                duplicateCards.Clear();
                duplicateCards = dupCards;

                FoundDuplicate(foundDuplicate);

                UpdateData(data);
            }
        }

        /// <summary>
        /// has a duplicate been found?
        /// </summary>
        /// <param name="foundDuplicate"></param>
        private void FoundDuplicate(bool foundDuplicate)
        {
            if (foundDuplicate != _foundDuplicates)
            {
                _foundDuplicates = foundDuplicate;

                if (foundDuplicate)
                    AdjustBackgroundColor(FoundDuplicateColor);
                else
                    AdjustBackgroundColor(NoDuplicatesColor);
            }
        }

        /// <summary>
        /// Set the background color
        /// </summary>
        /// <param name="colorHex">the background color </param>
        private void AdjustBackgroundColor(string colorHex)
        {
            //Debug.Log("adjusted background");

            var elements = Card.MakeUp.CardElementArray;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is CardBackgroundElementData background)
                {
                    background.ColorHex = colorHex;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override DataEntry GetDataEntry(string RowID, string args)
        {

            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), new List<string>() { "cards", "duplicate total" }, new string[3] { "id", "cards", "duplicate total" });

            if (_dataSet.EntryExists(RowID))
            {
                var entry = _dataSet.GetEntry(RowID);
                entry.Actor = this;

                return entry;
            }

            _entry = CreateNewDataEntry<DuplicateCardFinderActor>(RowID, _dataSet, this);
            return _entry;
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            return new List<DataValue>()
            {
                new DataValue((0, rowID), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.ViewOnly),
                new DataValue((1, rowID), "cards", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
                new DataValue((2, rowID), "duplicate total", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full),
            };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>() {
            new CardBackgroundElementData()
                {
                    ColorHex = "#1d1d1d",
                    Scale = new double[2] {3,2.5}
                },
                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {2},
                    MultiColumnString = " "
                }
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
