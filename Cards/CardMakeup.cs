/* CardMakeup.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager.Cards
{
    /// <summary>
    /// Manages the displaying of information on the card
    /// </summary>
    [Notes.Author("Manages the displaying of information on the card")]
    public class CardMakeup
    {

        /// <summary>
        /// the card element data that holds property information
        /// </summary>
        /// <typeparam name="UnityCardElementDisplayInfo"></typeparam>
        /// <returns></returns>
        private List<CardElementViewData> _cardViewData = new List<CardElementViewData>();

        /// <summary>
        /// The card reference
        /// </summary>
        private Card _card;

        /// <summary>
        /// The count of elements that the card contains
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return _cardViewData.Count;
            }
        }

        /// <summary>
        /// The array of card elements
        /// </summary>
        /// <value></value>
        public CardElementViewData[] CardElementArray
        {
            get
            {
                return _cardViewData.ToArray();
            }
        }

        /// <summary>
        /// The data entry the card will display
        /// </summary>
        /// <value></value>
        public DataEntry Entry => _card.Entry;

        /// <summary>
        /// The current view the card will model after
        /// </summary>
        /// <value></value>
        public DataSetView View { get; private set; }

        /// <summary>
        /// The card actor
        /// </summary>
        public Actor CardActor { get; private set; }

        /// <summary>
        /// Initialize the card makeup
        /// </summary>
        /// <param name="card">the card</param>
        public void Initialize(Card card)
        {
            if (card == null)
                throw new NullReferenceException("the card cannot be null");

            _card = card;

        }

        /// <summary>
        /// Refresh the card makeup
        /// </summary>
        public void OnAwake()
        {
            if (WarSystem.DataSetManager == null)
                throw new DataSetManagerMissingException("Cannot find the dataset manager");


            // if (Entry != null)
            //     Debug.Log("Found Entry " + string.Join(", ", Entry.GetRawData()));

            // Debug.Log(Entry.RowID);


            if (Entry == null)
                throw new NullReferenceException("the data entry cannot be null");

            View = Entry.DataSet.SelectedView;

            _cardViewData.Clear();
            if (View.CanViewCard) _cardViewData.AddRange(Entry.DataSet.SelectedView.ElementDataArray);



            if (Entry.Actor != null && CardActor == null)
            {
                CardActor = Entry.Actor;
                WarSystem.ActiveCardActors.Add(_card);
            }

            if (CardActor != null)
                CardActor.OnAwake();

        }

        /// <summary>
        /// Call the card frontend to sleep
        /// </summary>
        public void OnSleep()
        {
            if (CardActor != null)
                CardActor.OnSleep();
        }

        public void UpdateUI()
        {
            _card.UpdateCallback(3);
        }

        #region  old

        // /// <summary>
        // /// Set the card element data
        // /// </summary>
        // /// <param name="viewDataArray"></param>
        // [Obsolete]
        // private void SetCardElements(CardElementViewData[] viewDataArray)
        // {
        //     _cardViewData = new List<CardElementViewData>();
        //     _cardViewData.AddRange(viewDataArray);
        // }


        /// <summary>
        /// Set the card elements with the associated card data set and data piece
        /// </summary>
        /// <param name="set"></param>
        /// <param name="piece"></param>
        [Obsolete]
        private void SetCardElements(DataEntry entry)
        {
            #region old

            // if (entry.DataSet.SelectedView.Version == 1)
            // {

            //     for (int j = 0; j < elementDataArray.Length; j++)
            //     {
            //         int id = elementDataArray[j].ID;

            //         elementDataArray[j].CanBeVisible = entry.DataSet.SelectedView.CanViewCard;
            //         elementDataArray[j].CanEdit = entry.DataSet.SelectedView.CanEditCard;

            //         if (id < entry.ValueCount && id >= 0)
            //         {
            //             string inputInfo = "";

            //             if (entry.TryGetValueAt(id, out var dataValue))
            //             {
            //                 inputInfo = dataValue.ParseToParagraph();
            //             }

            //             if (elementDataArray[j].ElementTag != "text" && elementDataArray[j].ElementTag != "glance" && elementDataArray[j].ElementTag != "link")
            //             {
            //                 elementDataArray[j].DisplayInfo = new string[1] { inputInfo };
            //             }
            //             else if (elementDataArray[j].ElementTag == "text")
            //             {
            //                 List<string> input = new List<string>();
            //                 input.Add(inputInfo);

            //                 if (elementDataArray[j].PayloadLength >= 6)
            //                 {
            //                     string[] pload = elementDataArray[j].Payload.Split(',');

            //                     for (int i = 3; i < pload.Length; i++)
            //                     {
            //                         int x = -1;
            //                         if (Int32.TryParse(pload[i], out x))
            //                         {
            //                             if (x > 0 && x < entry.ValueCount)
            //                             {
            //                                 if (entry.TryGetValueAt(x, out var value))
            //                                 {
            //                                     input.Add(value.ParseToParagraph());
            //                                 }
            //                                 else
            //                                 {
            //                                     input.Add("");
            //                                 }
            //                             }
            //                         }
            //                     }
            //                 }

            //                 elementDataArray[j].DisplayInfo = input.ToArray();
            //                 // UnityEngine.Debug.Log(string.Join(" ", input) + " " + _card.ID + " " + GetHashCode());
            //             }
            //             else if (elementDataArray[j].ElementTag == "glance")
            //             {
            //                 List<string> input = new List<string>();
            //                 input.Add(inputInfo);

            //                 if (elementDataArray[j].PayloadLength >= 6)
            //                 {
            //                     string[] pload = elementDataArray[j].Payload.Split(',');
            //                     string[] extraCols = pload[5].Split('|');

            //                     // UnityEngine.Debug.Log(string.Join(",", extraCols));

            //                     for (int i = 0; i < extraCols.Length; i++)
            //                     {
            //                         if (Int32.TryParse(extraCols[i], out int x))
            //                         {
            //                             // UnityEngine.Debug.Log(x);

            //                             if (x > 0 && x < entry.ValueCount)
            //                             {
            //                                 if (entry.TryGetValueAt(x, out var value))
            //                                 {
            //                                     input.Add(value.ParseToParagraph());
            //                                 }
            //                                 else
            //                                 {
            //                                     input.Add("");
            //                                 }
            //                                 // UnityEngine.Debug.Log(str);
            //                             }
            //                         }
            //                     }
            //                 }

            //                 elementDataArray[j].DisplayInfo = input.ToArray();
            //                 // UnityEngine.Debug.Log(string.Join(" ", input));
            //             }
            //             else
            //             {
            //                 // UnityEngine.Debug.Log("found link");

            //                 List<string> input = new List<string>();
            //                 input.Add(inputInfo);

            //                 if (elementDataArray[j].PayloadLength >= 6)
            //                 {
            //                     string[] pload = elementDataArray[j].Payload.Split(',');
            //                     string[] extraCols = pload[5].Split('|');

            //                     // UnityEngine.Debug.Log(string.Join(",", extraCols));

            //                     for (int i = 0; i < extraCols.Length; i++)
            //                     {
            //                         if (Int32.TryParse(extraCols[i], out int x))
            //                         {
            //                             // UnityEngine.Debug.Log(x);

            //                             if (x > 0 && x < entry.ValueCount)
            //                             {
            //                                 if (entry.TryGetValueAt(x, out var value))
            //                                 {
            //                                     input.Add(value.ParseToParagraph());
            //                                 }
            //                                 else
            //                                 {
            //                                     input.Add("");
            //                                 }
            //                                 // UnityEngine.Debug.Log(str);
            //                             }
            //                         }
            //                     }
            //                 }

            //                 elementDataArray[j].DisplayInfo = input.ToArray();
            //             }
            //         }
            //         else
            //         {
            //             // UnityEngine.Debug.Log("data piece id is not valid " + id.ToString() + " " + piece.DataCount.ToString());
            //         }
            //     }
            // }
            // else
            // {
            //     UnityEngine.Debug.Log("The version is not 1");
            // }

            #endregion
        }

        #endregion

        // > create a data entry inside a dataset with a selected view. 
        // > call refresh in order to activate the card
        // 
    }
}
