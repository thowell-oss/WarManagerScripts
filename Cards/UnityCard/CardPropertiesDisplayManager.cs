using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Unity3D;
using WarManager.Unity3D.Windows;

using WarManager.Sharing;
using WarManager.Sharing.Security;
using WarManager.Backend;
using System;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles showing the properties of the card in the properties slide window pane
    /// </summary>
    public static class CardPropertiesDisplayManager
    {
        private static List<Card> _selectedCards = new List<Card>();

        /// <summary>
        /// The amount of selected cards in the card display manager
        /// </summary>
        /// <value></value>
        public static int Count
        {
            get
            {
                return _selectedCards.Count;
            }
        }

        /// <summary>
        /// Set the cards to be shown in the properties pane
        /// </summary>
        /// <param name="card"></param>
        public static void DisplayRecentSelected(Card card)
        {
            if (WarSystem.DataSetManager.TryGetDataset(card.DatasetID, out var set))
            {
                DataSetViewer.main.ShowDataEntryInfo(set.GetEntry(card.RowID), () => { SlideWindowsManager.main.ClearProperties(); SlideWindowsManager.main.CloseProperties(false); }, "Close", false);
            }
        }

        public static void DisplayCards(List<Card> cards)
        {
            // if (cards == null)
            //     if (!SlideWindowsManager.main.Properties.Closed)
            //         SlideWindowsManager.main.ClearProperties();

            // if (cards.Count == 1)
            // {
            //     if (!SlideWindowsManager.main.Properties.Closed)
            //     {
            //         var entry = cards[0].Entry;
            //         DataSetViewer.main.ShowDataEntryInfo(entry, () => { ActiveSheetsDisplayer.main.ViewReferences(); }, "References");
            //     }
            // }
            // else if (cards.Count == 0)
            // {
            //     if (!SlideWindowsManager.main.Properties.Closed)
            //         SlideWindowsManager.main.ClearProperties();
            // }
            // else if (cards.Count > 1)
            // {

            // if (!SlideWindowsManager.main.Properties.Closed)
            // {
            //     SlideWindowsManager.main.ClearProperties();
            //     List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            //     content.Add(new SlideWindow_Element_ContentInfo(30));

            //     content.Add(new SlideWindow_Element_ContentInfo("Note", "Cannot edit multiple cards at once yet."));

            //     DataEntryUtility utility = new DataEntryUtility();
            //     var entries = utility.GetEntriesFromCards(cards);
            //     var values = utility.GetSimilarDataValuesFromEntries(entries);

            //     foreach (var x in values)
            //     {
            //         content.Add(new SlideWindow_Element_ContentInfo(x.HeaderName, x.Value.ToString()));
            //     }


            //     SlideWindowsManager.main.AddPropertiesContent(content);
            // }

            //     if (!SlideWindowsManager.main.Properties.Closed)
            //         SlideWindowsManager.main.ClearProperties();
            // }

            // SlideWindowsManager.main.ClearProperties();
            // List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            // content.Add(new SlideWindow_Element_ContentInfo(50));

            // if (cards == null || cards.Count < 1)
            // {
            //     content.Add(new SlideWindow_Element_ContentInfo("No Cards Selected", null));
            //     SlideWindowsManager.main.AddPropertiesContent(content);
            //     return;
            // }

            // string selectedDatasetID = cards[0].DatasetID;

            // var dataset = cards[0].DataSet;

            // if (dataset != null)
            // {

            //     List<DataValue> values = new List<DataValue>();
            //     values.AddRange(dataset.GetEntry((int)cards[0].DataRepID).GetAllowedDataValues());

            //     foreach (var x in values)
            //     {
            //         content.Add(new SlideWindow_Element_ContentInfo(x.HeaderName, x.ParseToParagraph()));
            //     }
            // }
            // else
            // {
            //     content.Add(new SlideWindow_Element_ContentInfo("No Valid Data Set ", null));
            // }

            // SlideWindowsManager.main.AddPropertiesContent(content);

            // List<DataEntry> entries = new List<DataEntry>();
            // entries.Add(dataset.GetEntry((int)cards[0].DataRepID));

            // foreach (var card in cards)
            // {

            //     if (card.DatasetID != selectedDatasetID)
            //     {
            //         List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            //         content.Add(new SlideWindow_Element_ContentInfo(50));
            //         content.Add(new SlideWindow_Element_ContentInfo("Selected cards have different datasets.", null));
            //         SlideWindowsManager.main.AddPropertiesContent(content);

            //         return;
            //     }
            //     else
            //     {
            //         entries.Add(dataset.GetEntry((int)card.DataRepID));
            //     }
            // }


            // for (int i = 0; i < entries.Count; i++)
            // {
            //     var values = entries[i].GetAllowedDataValues();

            //     foreach(ValuePermissions)

            // }
        }

        [Obsolete("Cannot refresh properties window this way")]
        private static void RefreshPropertiesWindow()
        {
            // List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            // content.Add(new SlideWindow_Element_ContentInfo("Card Properties", null));

            // if (_selectedCards == null)
            // {
            //     content.Add(new SlideWindow_Element_ContentInfo("", "No Cards Selected"));
            // }
            // else
            // {

            //     if (_selectedCards.Count == 1)
            //     {
            //         string datasetID = _selectedCards[0].DatasetID;
            //         string rowId = _selectedCards[0].DataRepID;

            //         if (WarSystem.DataSetManager.TryGetDataset(datasetID, out var set))
            //         {
            //             var dataPiece = set.GetData(rowId);
            //             if (dataPiece != null && set != null)
            //             {

            //                 for (int i = 0; i < dataPiece.DataCount; i++)
            //                 {
            //                     if (i < dataPiece.TagCount)
            //                     {
            //                         string tag = dataPiece.GetTag(i);

            //                         if (set.IsAllowedTag(tag))
            //                         {
            //                             content.Add(new SlideWindow_Element_ContentInfo(tag, dataPiece.GetData(i)));
            //                         }
            //                     }
            //                 }

            //                 content.Add(new SlideWindow_Element_ContentInfo(25));
            //                 content.Add(new SlideWindow_Element_ContentInfo("Data Set", set.DatasetName));
            //                 content.Add(new SlideWindow_Element_ContentInfo("Data Set ID", datasetID));
            //             }
            //         }
            //         else
            //         {
            //             content.Add(new SlideWindow_Element_ContentInfo("Data Set", "Data missing"));
            //             content.Add(new SlideWindow_Element_ContentInfo("Data", "No Data found"));
            //         }


            //         content.Add(new SlideWindow_Element_ContentInfo(25));
            //         content.Add(new SlideWindow_Element_ContentInfo("Card ID", _selectedCards[0].ID));
            //         content.Add(new SlideWindow_Element_ContentInfo("Locked?", _selectedCards[0].CardLocked.ToString()));
            //         content.Add(new SlideWindow_Element_ContentInfo("Hidden?", _selectedCards[0].CardHidden.ToString()));
            //     }
            //     else if (_selectedCards.Count > 1)
            //     {
            //         content.Add(new SlideWindow_Element_ContentInfo("", "Multi-select not supported yet"));
            //     }
            //     else
            //     {
            //         content.Add(new SlideWindow_Element_ContentInfo("", "No Cards Selected"));
            //     }
            // }

            // SlideWindowsManager.main.AddPropertiesContent(content);

        }
    }
}