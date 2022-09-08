using System;
using System.Collections;
using System.Collections.Generic;

using StringUtility;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;
using WarManager.Cards;

namespace WarManager
{
    /// <summary>
    /// Checks for tags
    /// </summary>
    public sealed class TagCheckActor : Actor
    {
        public override string Name => "Keyword Checker";

        public override string Description => "Checks to make sure rows or columns of cards do not contain certain keywords. If they do, a message box will appear asking you either to remove or undo. ";

        private List<Card> _allowedCards = new List<Card>();

        private bool _checking = false;
        private bool _mustContain => MustContain();

        private DataSet _dataSet;
        private DataEntry _entry;

        public override string DataSetID => "b9697564-52e2-4ec8-9e58-3914232b2b4c";


        public override void OnInit(Card card)
        {
            base.OnInit(card);
            CheckRowOrColumnKeywords(FixSuggestion.remove);
        }

        public override void Act()
        {
            base.Act();

            if (!_checking)
                CheckRowOrColumnKeywords(FixSuggestion.remove);
        }

        public override void OnChangeSheet()
        {
            base.OnChangeSheet();
            CheckRowOrColumnKeywords(FixSuggestion.remove);
        }

        public override void OnDrop()
        {
            base.OnDrop();

            _allowedCards.Clear();

            CheckRowOrColumnKeywords(FixSuggestion.remove);
        }

        private string GetKeyword()
        {
            var data = Card.Entry.GetAllowedDataValues()[0];
            var keyword = data.ParseToParagraph();

            return keyword;
        }

        private string GetDataSetName()
        {
            var values = Card.Entry.GetAllowedDataValues();

            if (values.Length > 4)
            {
                return values[4].ParseToParagraph();
            }

            return "";
        }

        private bool MustContain()
        {
            var data = Card.Entry.GetAllowedDataValues()[2];

            if (data.ParseToParagraph() == "Contains")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the row or column of cards to check
        /// </summary>
        /// <param name="suggestion">should the user undo or remove the card?</param>
        private void CheckRowOrColumnKeywords(FixSuggestion suggestion)
        {

            // Debug.Log("checking " + Time.realtimeSinceStartupAsDouble);

            var vector = Card.Entry.GetAllowedDataValues()[1];

            string direction = vector.ParseToParagraph();

            List<Card> cards = new List<Card>();

            if (direction == "Column")
            {
                cards = CardUtility.GetAdjacentCardsLine(Card.point, Point.down, 100, Card.Layer, Card.SheetID);
            }
            else if (direction == "Row")
            {
                cards = CardUtility.GetAdjacentCardsLine(Card.point, Point.right, 100, Card.Layer, Card.SheetID);
            }

            cards.Remove(this.Card);

            CheckCards(GetKeyword(), cards, suggestion, GetDataSetName());
            CheckContains(cards);
        }

        /// <summary>
        /// Check the cards
        /// </summary>
        /// <param name="keyword">the keyword that we are searching for</param>
        /// <param name="cards">the list of cards</param>
        /// <param name="suggestion">the suggestion</param>
        private void CheckCards(string keyword, List<Card> cards, FixSuggestion suggestion, string dataSetMask)
        {

            _checking = true;

            if (keyword == null || keyword.Length < 1)
            {
                _checking = false;
                return;
            }

            foreach (var x in cards)
            {
                if (dataSetMask.Trim() == string.Empty || x.DataSet.DatasetName.ToLower() == dataSetMask.ToLower())
                {
                    var values = x.Entry.GetAllowedValues();

                    if (!_allowedCards.Contains(x))
                    {

                        if (x.point != Card.point)
                        {

                            bool contains = false;

                            foreach (var value in values)
                            {
                                if (value.Contains(keyword))
                                {
                                    if (!_mustContain)
                                    {
                                        HandleCard("The card " + values[0] + " contains " + keyword.SetStringQuotes(), x, suggestion, () => { CheckCards(keyword, cards, suggestion, dataSetMask); });

                                        return;
                                    }
                                    else
                                    {
                                        contains = true;
                                    }
                                }
                            }

                            if (!contains && _mustContain)
                            {
                                HandleCard("The card " + values[0] + " does not have " + keyword.SetStringQuotes(), x, suggestion, () => { CheckCards(keyword, cards, suggestion, dataSetMask); });
                                return;
                            }
                        }
                    }
                }
            }

            _checking = false;
            // Debug.Log("done.");

            //var titleTextElement = GetElementWidthId<CardElementTextData>(0);
            // UpdateUI();
        }

        /// <summary>
        /// Clean the allowed cards cache of cards that no longer exist within that context
        /// </summary>
        /// <param name="cards">the cards</param>
        private void CheckContains(List<Card> cards)
        {
            for (int i = 0; i < _allowedCards.Count; i++)
            {
                if (!cards.Contains(_allowedCards[i]))
                {
                    // Debug.Log("removed " + _allowedCards[i].Entry.GetAllowedValues()[0]);
                    _allowedCards.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Send a message to the user giving ideas about what to do
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="card">the card</param>
        /// <param name="suggestion">the suggestion about how to fix the problem</param>
        /// <param name="recursiveCallback">recursive call back when everything is done to keep searching</param>
        private void HandleCard(string message, Card card, FixSuggestion suggestion, Action recursiveCallback)
        {
            if (suggestion == FixSuggestion.remove || SimpleUndoRedoManager.main.UndoCount < 1)
            {
                MessageBoxHandler.Print_Immediate(message + " - Remove?", "Error", (y) =>
                {
                    if (y)
                    {
                        CardUtility.RemoveCard(card.point, card.Layer, card.SheetID);
                        SheetsManager.ReloadCurrentSheet();
                    }
                    else
                    {
                        _allowedCards.Add(card);
                    }

                    recursiveCallback();
                });
            }
            else
            {
                MessageBoxHandler.Print_Immediate(message + " - Undo?", "Error", (y) =>
               {
                   if (y)
                   {
                       SimpleUndoRedoManager.main.Undo();
                   }
                   else
                   {
                       _allowedCards.Add(card);
                   }

                   recursiveCallback();
               });
            }
        }

        /// <summary>
        /// Refresh the card when new information comes in
        /// </summary>
        /// <param name="values"></param>
        public override void OnUpdateInput(List<ValueTypePair> values)
        {
            _allowedCards.Clear();
            base.OnUpdateInput(values);
            UpdateUI();
            CheckRowOrColumnKeywords(FixSuggestion.remove);
        }

        public override DataEntry GetDataEntry(string rowID, string args)
        {
            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(),
             new List<string>() { "Keyword", "Direction", "Contain?", "Distance", "DataSet" },
              new string[6] { "id", "Keyword", "Direction", "Contain?", "Distance", "DataSet" });

            if (_dataSet.EntryExists(rowID))
            {
                _entry = _dataSet.GetEntry(rowID);
                _entry.Actor = this;

                return _entry;
            }

            _entry = CreateNewDataEntry<TagCheckActor>(rowID, _dataSet, this);
            return _entry;
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            DataValue Id = new DataValue((0, rowID), "Id", "", ColumnInfo.GetValueTypeOfWord, ValuePermissions.Full, "");
            DataValue TagValue = new DataValue((1, rowID), "Keyword", "", ColumnInfo.GetValueTypeOfWord, ValuePermissions.Full, "The keyword of interest.");
            DataValue DirectionValue = new DataValue((2, rowID), "Direction", "Column", ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "The direction to search - either \"Column\" or \"Row\" ", new string[2] { "Column", "Row" });
            DataValue ContainValue = new DataValue((3, rowID), "Contain?", "Must Not Contain",
             ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "The message box will show if the card either 'contains' or 'does not contain' a specified keyword", new string[2] { "Contains", "Must Not Contain" });
            DataValue DistanceValue = new DataValue((4, rowID), "Distance", "100",
                        ColumnInfo.GetValueTypeOfInt, ValuePermissions.Full, "The max detection distance from this card");
            DataValue DataSetValue = new DataValue((5, rowID), "DataSet", "",
                        ColumnInfo.GetValueTypeOfSentence, ValuePermissions.Full, "The Data Set that will be searched (leave this empty if you want all to be checked).");

            return new List<DataValue>()
            { Id, TagValue, DirectionValue, ContainValue, DistanceValue, DataSetValue};
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
                    SetColumns = new List<int>() {1, 2},
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

    /// <summary>
    /// Suggestions to fix cards with incorrect keywords
    /// </summary>
    public enum FixSuggestion
    {
        undo,
        remove,
    }
}
