/*CardSelectionHandler .cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;


namespace WarManager.Cards
{
    /// <summary>
    /// Handles what cards are selected
    /// </summary>
    public class CardSelectionHandler<TguiCard> : IComparable<CardSelectionHandler<TguiCard>>
    {
        /// <summary>
        /// The list of selected cards
        /// </summary>
        public List<SelectedCard<TguiCard>> SelectedCards { get; set; } = new List<SelectedCard<TguiCard>>();

        /// <summary>
        /// The card currently being hoverd on
        /// </summary>
        public SelectedCard<TguiCard> SelectedHoverCard { get; set; } = null;

        /// <summary>
        /// The sheet id of the sheet the selection handler is managing
        /// </summary>
        public string SheetID;

        public delegate void deselect();
        public static deselect OnDeselect;

        /// <summary>
        /// The amount of selected cards
        /// </summary>
        public int SelectedCardAmt
        {
            get
            {
                return SelectedCards.Count;
            }
        }

        /// <summary>
        /// Is there only one card?
        /// </summary>
        public bool IsOneCard
        {
            get
            {
                return SelectedCards.Count == 1;
            }
        }

        /// <summary>
        /// Are there any selected cards?
        /// </summary>
        public bool AreCardsSelected
        {
            get
            {
                return SelectedCards.Count > 0;
            }
        }

        /// <summary>
        /// Add a card to the selection list
        /// </summary>
        /// <param name="card">a selected card reference</param>
        public void AddCard(SelectedCard<TguiCard> card)
        {
            SelectedCards.Add(card);
            SelectedCards.Sort();

            //ContextMenuHandler.Refresh();
        }

        /// <summary>
        /// Returns a list of the backend references to cards
        /// </summary>
        /// <returns>returns a list of cards, if not possible the list will be empty</returns>
        public List<Card> GetAllBackendCards()
        {
            List<Card> cards = new List<Card>();

            foreach (SelectedCard<TguiCard> selectedCard in SelectedCards)
            {
                cards.Add(selectedCard.Card);
            }

            return cards;
        }

        public List<TguiCard> GetAllFrontendCards()
        {
            List<TguiCard> cards = new List<TguiCard>();

            foreach (SelectedCard<TguiCard> selectedCard in SelectedCards)
            {
                cards.Add(selectedCard.GUICard);
            }

            return cards;
        }

        /// <summary>
        /// Remove a card from the selection list
        /// </summary>
        /// <param name="card">a remove card reference</param>
        public void RemoveCard(SelectedCard<TguiCard> card)
        {
            SelectedCards.Remove(card);
            SelectedCards.Sort();

            //ContextMenuHandler.Refresh();
        }

        /// <summary>
        /// Remove card by ID
        /// </summary>
        /// <param name="id">the given id</param>
        public void RemoveCard(string id)
        {
            var card = SelectedCards.Find(x => x.Card.ID == id);
            if (card != null)
            {
                RemoveCard(card);
            }

            SelectedCards.Sort();
        }

        /// <summary>
        /// Get the single card if there is only one card
        /// </summary>
        /// <returns>returns the single selected card</returns>
        public SelectedCard<TguiCard> GetCard()
        {
            if (SelectedCards.Count != 1)
                throw new NotSupportedException("Cannot return the card - cardAmt: " + SelectedCards.Count);

            return SelectedCards[0];
        }

        /// <summary>
        /// Does the selection contain a certian card?
        /// </summary>
        /// <param name="id">the card id to check</param>
        /// <returns>returns true if a card with the id was found, false if not</returns>
        public bool Contains(string id)
        {
            var card = SelectedCards.Find(x => x.Card.ID == id);

            if (card != null)
                return true;

            return false;
        }

        /// <summary>
        /// Clear the list of selected cards
        /// </summary>
        public void Clear()
        {
            if (OnDeselect != null)
                OnDeselect();

            SelectedCards.Clear();

            //ContextMenuHandler.Refresh();
        }

        public int CompareTo(CardSelectionHandler<TguiCard> other)
        {
            return GetHashCode().CompareTo(other.GetHashCode());
        }
    }
}
