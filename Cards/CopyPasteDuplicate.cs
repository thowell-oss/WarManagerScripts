using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager.Cards
{
    public class CopyPasteDuplicate : MonoBehaviour
    {
        public delegate void pasteCard_delegate();
        public static event pasteCard_delegate OnPasteCard;

        public WarManagerCameraController cameraController;

        public List<(Point offset, Card card)> CopiedCards = new List<(Point, Card)>();

        public int copyCount = 0;


        #region  singleton

        public static CopyPasteDuplicate Main;

        public void Awake()
        {
            if (Main == null)
                Main = this;

            else
                Destroy(this);
        }

        #endregion

        /// <summary>
        /// Can the cards be pasted?
        /// </summary>
        /// <value></value>
        public bool CanPaste
        {
            get
            {
                return CopiedCards.Count > 0;
            }
        }

        /// <summary>
        /// copy cards selected from the selection manager
        /// </summary>
        public void CopySelectedCards()
        {
            CopySelectedCards(GhostCardBehavior.Main.Location);
        }

        /// <summary>
        /// copy cards selected from the selection manager
        /// </summary>
        /// <param name="focalOffsetLocation">the location every other card sets its offset from (in order to maintain location integrity)</param>
        public void CopySelectedCards(Point focalOffsetLocation, bool copyToClipboard = true)
        {
            CopiedCards = new List<(Point offset, Card)>();

            StringBuilder b = new StringBuilder();

            List<string> DataSetsThatCannotBeCopied = new List<string>();

            if (SheetsCardSelectionManager.Main.CardTotal > 0)
            {
                var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards().ToArray();
                foreach (var card in cards)
                {

                    if (card.DataSet.SelectedView.CanViewCard)
                    {
                        var offset = card.point - focalOffsetLocation;
                        CopiedCards.Add((offset, card));

                        b.AppendLine(card.GetAllowedEntryToString());

                        copyCount++;
                    }
                    else
                    {
                        DataSetsThatCannotBeCopied.Add(card.DataSet.DatasetName);
                    }
                }
            }

            if (DataSetsThatCannotBeCopied.Count > 0 && copyCount != 0)
            {
                MessageBoxHandler.Print_Immediate("Some Data Sets cannot be copied because you do not have permission to view them: " + string.Join(", ", DataSetsThatCannotBeCopied), "Note");
            }
            else if (DataSetsThatCannotBeCopied.Count > 0 && copyCount == 0)
            {
                MessageBoxHandler.Print_Immediate("You cannot copy these cards because you do not have permission to view them: " + string.Join(", ", DataSetsThatCannotBeCopied), "Error");
            }

            GUIUtility.systemCopyBuffer = b.ToString();
        }

        /// <summary>
        /// copy a selected card
        /// </summary>
        /// <param name="card">the selected card</param>
        /// <param name="offsetLocation">the offset for the card to maintain offset integrity</param>
        public void CopyCard(Card card, Point offsetLocation)
        {

            if (card.DataSet.SelectedView.CanViewCard)
            {

                CopiedCards = new List<(Point offset, Card)>();
                CopiedCards.Add((card.point - offsetLocation, card));
                copyCount += 1;

                GUIUtility.systemCopyBuffer = card.GetAllowedEntryToString();
            }
            else
            {
                MessageBoxHandler.Print_Immediate("You cannot copy these cards because you do not have permission to view them: " + string.Join(", ", card.DataSet.DatasetName), "Error");
            }
        }

        /// <summary>
        /// paste cards
        /// </summary>
        public void Paste()
        {

            if (CanPaste)
            {
                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    List<string> setIds = new List<string>();
                    setIds.AddRange(sheet.GetDatasetIDs());

                    Point dropLocation = GhostCardBehavior.Main.Location;

                    var layer = sheet.CurrentLayer;

                    List<Card> addedCardsList = new List<Card>();

                    bool paste = true;

                    foreach (var cardOffsetPair in CopiedCards)
                    {
                        Point res = cardOffsetPair.offset + dropLocation;

                        if (!res.IsInGridBounds || sheet.ObjExists(res, sheet.CurrentLayer))
                        {
                            Debug.LogError(res + " " + dropLocation + " " + cardOffsetPair.offset);
                            paste = false;
                            MessageBoxHandler.Print_Immediate("Some cards either exist where another card will be pasted, or the pasted card cannot be pasted out of bounds", "Error Pasting Cards");
                            return;
                        }
                    }

                    if (paste)
                    {
                        foreach (var cardOffsetPair in CopiedCards)
                        {
                            try
                            {
                                if (cardOffsetPair.card.DatasetID.StartsWith("sys") || setIds.Find((x) => x == cardOffsetPair.card.DatasetID) != null)
                                {

                                    if (TryAdd(dropLocation + cardOffsetPair.offset, layer, sheet, cardOffsetPair.card.DatasetID, cardOffsetPair.card.RowID.ToString(), out var c))
                                    {
                                        // Debug.Log("pasting cards " + dropLocation);
                                        addedCardsList.Add(c);
                                        c.SheetID = sheet.ID;
                                    }

                                }
                                else
                                {
                                    MessageBoxHandler.Print_Immediate($"Cannot paste copied cards because \'{sheet.Name}\' does not have the associated data set (\'{cardOffsetPair.card.DatasetID}\').", "Error");
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogError(ex.Message);
                            }
                        }


                        SheetsCardSelectionManager.Main.DeselectCurrent();

                        foreach (var card in addedCardsList)
                        {
                            card.Select(true);
                        }

                        SimpleUndoRedoManager.main.NewSnapShot();
                    }
                }
            }
        }

        public void CheckVisiblity()
        {
            OnPasteCard?.Invoke();
        }

        /// <summary>
        /// duplicate cards
        /// </summary>
        public void Duplicate()
        {
            foreach (var card in SheetsCardSelectionManager.Main.GetCurrentSelectedCards())
            {
                if (SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
                {
                    TryAdd(card.point + Point.down, card.Layer, sheet, card.DatasetID, card.RowID.ToString(), out var c);
                    SimpleUndoRedoManager.main.NewSnapShot();
                }
            }
        }

        private bool TryAdd(Point dropLocation, Layer layer, Sheet<Card> sheet, string dataSetID, string CardRepId, out Card card)
        {
            if (CardUtility.GetCard(dropLocation, layer, sheet.ID) != null)
            {
                CardUtility.TryShift(dropLocation, CardUtility.DefaultShiftDirection, layer, sheet.ID, 1, out var snapShots);
            }

            if (CardUtility.GetCard(dropLocation, layer, sheet.ID) == null)
            {
                string id = System.Guid.NewGuid().ToString();

                Card c = new Card(dropLocation, id, sheet.ID, layer.ID, dataSetID, CardRepId);
                card = c;

                if (CardUtility.TryAddCard(c))
                {
                    if (OnPasteCard != null)
                    {
                        OnPasteCard.Invoke();
                        return true;
                    }
                }
            }

            card = null;
            return false;
        }


        /// <summary>
        /// Remove selected cards
        /// </summary>
        public void Remove()
        {
            var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                cards[i].Remove();
            }
        }

        /// <summary>
        /// Lock card
        /// </summary>
        /// <param name="lockCard"></param>
        public void Lock(bool lockCard)
        {
            var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();

            List<string> unLockedDataSets = new List<string>();

            foreach (var card in cards)
            {
                bool proceed = true;

                if (lockCard == false) //unlocking the card
                {
                    if (!card.DataSet.SelectedView.CanEditCard || !card.DataSet.SelectedView.CanViewCard) // check to make sure that the user is allowed to view/edit the card
                    {
                        proceed = false;//if not then prevent the user from tinkering with the card

                        if (!unLockedDataSets.Contains(card.DataSet.DatasetName))
                            unLockedDataSets.Add(card.DataSet.DatasetName);
                    }
                }

                if (proceed)
                    card.Lock(lockCard);
            }

            if (unLockedDataSets.Count > 0)
            {
                MessageBoxHandler.Print_Immediate("Some Cards you cannot unlock because you do not have permission: \n" + string.Join(", ", unLockedDataSets), "Note"); //tell the user why they cannot unlock the cards
            }
        }

        /// <summary>
        /// select card
        /// </summary>
        /// <param name="select"></param>
        public void SelectAll(bool select)
        {
            if (!select)
            {
                SheetsCardSelectionManager.Main.DeselectCurrent();
            }
            else
            {

                var cards = CardUtility.GetCardsFromCurrentSheet();

                foreach (var card in cards)
                {
                    if (card.DataSet.SelectedView.CanViewCard && card.DataSet.SelectedView.CanEditCard) //can the user tinker with the card?
                    {
                        card.Select(true);
                    }
                }

                SheetsCardSelectionManager.Main.RefreshContextMenu();

            }
        }
    }
}
