using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

namespace WarManager
{
    public class SimpleUndoRedoSnapshot
    {
        /// <summary>
        /// The list of cards in all sheets and thier details at a given time
        /// </summary>
        /// <value></value>
        private List<(Sheet<Card> sheet, SimpleUndoRedo_ObjDet[] dets)> _snapshot = new List<(Sheet<Card>, SimpleUndoRedo_ObjDet[])>();
        private string currentSheetID;

        /// <summary>
        /// Empty Constructor - create a snapshot of all the sheets
        /// </summary>
        public SimpleUndoRedoSnapshot()
        {
            if (SheetsManager.SheetCount > 0)
            {
                currentSheetID = SheetsManager.CurrentSheetID;

                var sheets = SheetsManager.GetActiveCardSheets();

                foreach (var sheet in sheets)
                {
                    List<Card> cards = sheet.GetAllObj();
                    List<SimpleUndoRedo_ObjDet> detList = new List<SimpleUndoRedo_ObjDet>();

                    foreach (var card in cards)
                    {
                        SimpleUndoRedo_ObjDet newDet = new SimpleUndoRedo_ObjDet(card, card.point, card.Layer, sheet);
                        detList.Add(newDet);
                    }

                    _snapshot.Add((sheet, detList.ToArray()));
                }
            }
        }

        /// <summary>
        /// Set the cards to their state expressed in the snap shot
        /// </summary>
        public void SetSnapShot()
        {
            foreach (var snap in _snapshot)
            {
                List<SimpleUndoRedo_ObjDet> cardsList = new List<SimpleUndoRedo_ObjDet>();
                cardsList.AddRange(snap.dets);

                List<Card> cardsInSheet = snap.sheet.GetAllObj();

                List<Card> CardsToRemove = GetCardsToRemove(cardsInSheet, cardsList);
                List<SimpleUndoRedo_ObjDet> CardsToAdd = GetCardsToAdd(cardsList, cardsInSheet);


                foreach (var card in CardsToRemove)
                {
//                    Debug.Log("removing card " + card.point);
                    CardUtility.RemoveCard(card.point, card.Layer, card.SheetID);
                }

                List<(Card Card, bool locked)> CardsToMove = new List<(Card Card, bool locked)>();
                foreach (var det in cardsList)
                {
                    var c = cardsInSheet.Find(x => x == det.Card);

                    if (c != null)
                    {
                        // Debug.Log("Set Locked " + det.Locked);

                        if (c.CardLocked && !det.Locked && c.CanLockOrUnlock)
                        {
                            c.Lock(false);
                        }

                        if (!c.CardLocked)
                        {
                            CardUtility.RemoveCard(c.point, c.Layer, snap.sheet.ID);
                            c.point = det.CardLocation;
                            c.Layer = det.Layer;
                            c.SheetID = snap.sheet.ID;

                            CardsToMove.Add((c, det.Locked));
                        }
                    }
                }

                foreach (var info in CardsToMove)
                {
                    var card = info.Card;

                    CardUtility.TryAddCard(card);

                    if (info.locked)
                        Debug.Log("locking card");

                    card.Lock(info.locked);
                }

                foreach (var info in CardsToAdd)
                {
                    if (CardUtility.TryAddCard(info.Card))
                    {
                        Debug.Log("adding card " + info.Card.point);
                    }
                }

            }

            if (SheetsManager.SheetCount > 0)
            {

                SheetsManager.SetSheetCurrent(currentSheetID);
            }
            else
            {
                SheetsManager.SetLastSheetCurrent();
            }

        }

        public List<Card> GetAllCards(List<SimpleUndoRedo_ObjDet> cardsList)
        {
            List<Card> final = new List<Card>();

            foreach (var card in cardsList)
            {
                final.Add(card.Card);
            }

            return final;
        }

        /// <summary>
        /// Get the list of cards from list a that are not in list b
        /// </summary>
        /// <param name="a">the first given list of cards</param>
        /// <param name="b">the second given list of cards</param>
        /// <returns>returns a list of cards</returns>
        private List<SimpleUndoRedo_ObjDet> GetCardsToAdd(List<SimpleUndoRedo_ObjDet> a, List<Card> b)
        {
            List<SimpleUndoRedo_ObjDet> final = new List<SimpleUndoRedo_ObjDet>();

            for (int i = 0; i < a.Count; i++)
            {
                var c = b.Find(x => x == a[i].Card);

                if (c == null)
                {
                    final.Add(a[i]);
                }
            }

            return final;
        }

        private List<Card> GetCardsToRemove(List<Card> a, List<SimpleUndoRedo_ObjDet> b)
        {
            List<Card> final = new List<Card>();

            for (int i = 0; i < a.Count; i++)
            {
                var c = b.Find(x => x.Card == a[i]);

                if (c == null)
                {
                    final.Add(a[i]);
                }
            }

            return final;
        }

    }
}
