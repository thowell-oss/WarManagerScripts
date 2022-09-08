
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Cards
{
    public class CardGroup : IComparable<CardGroup>, IEquatable<CardGroup>
    {
        /// <summary>
        /// The list of grouped cards
        /// </summary>
        /// <typeparam name="Card"></typeparam>
        /// <returns></returns>
        private List<Card> _groupedCards = new List<Card>();

        /// <summary>
        /// The action to remove the card group from a master list
        /// </summary>
        private Action _removeGroupCallBack;

        /// <summary>
        /// The amount of cards in the list of grouped cards
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return _groupedCards.Count;
            }
        }

        /// <summary>
        /// The cards in the group
        /// </summary>
        /// <value></value>
        public ReadOnlyCollection<Card> Cards
        {
            get
            {
                ReadOnlyCollection<Card> c = new ReadOnlyCollection<Card>(_groupedCards);
                return c;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cards">the cards to add to the list</param>
        public CardGroup(List<Card> cards, Action removeCallBack)
        {
            if (cards == null || cards.Count == 0)
                throw new NotSupportedException("A list of cards less than one or a null list is not supported");

            _removeGroupCallBack = removeCallBack;

            foreach (var card in cards)
            {
                if (card != null)
                    AddCardToGroup(card);
            }
        }

        /// <summary>
        /// Does the group contain a certian card?
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool Contains(Card card)
        {
            if (_groupedCards.Find((x) => x == card) != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add the cards to the group
        /// </summary>
        /// <param name="card"></param>
        public void AddCardToGroup(Card card)
        {
            _groupedCards.Add(card);
        }

        /// <summary>
        /// Remove the cards from the group
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCardFromGroup(Card card)
        {
            _groupedCards.Remove(card);

            if (_groupedCards.Count <= 0)
            {
                _removeGroupCallBack();
            }
        }

        /// <summary>
        /// Remove all cards from the group
        /// </summary>
        /// <returns>returns a list of cards</returns>
        public List<Card> RemoveAllCards()
        {
            _removeGroupCallBack();
            return _groupedCards;
        }

        /// <summary>
        /// Card selection system
        /// </summary>
        /// <param name="selected"></param>
        public void CardSelect(bool selected)
        {
            foreach (var card in _groupedCards)
            {
                if(selected)
                {
                    card.Select(true);
                }
                else
                {
                    card.Deselect();
                }
            }
        }

        /// <summary>
        /// Cause the grouped cards to being moving and dragging
        /// </summary>
        public void CardBeginMove()
        {
            foreach (var card in _groupedCards)
            {
                card.StartDrag(false);
            }
        }

        /// <summary>
        /// Cause the ui cards to update position
        /// </summary>
        public void CardMove()
        {
            foreach (var card in _groupedCards)
            {
                card.EndDrag(false);
            }
        }

        /// <summary>
        /// Delete the cards from the sheet
        /// </summary>
        public void RemoveCards()
        {
            for (int i = _groupedCards.Count - 1; i >= 0; i--)
            {
                _groupedCards[i].Remove();
                RemoveCardFromGroup(_groupedCards[i]);
            }
        }

        public int CompareTo(CardGroup other) //compare length of the card groups
        {
            return Count.CompareTo(other.Count);
        }

        public bool Equals(CardGroup other)
        {
            return this == other;
        }

        public static CardGroup operator +(CardGroup a, CardGroup b)
        {
            Action removeCallBack = a._removeGroupCallBack;
            List<Card> cards = a.RemoveAllCards();
            cards.AddRange(b.RemoveAllCards());
            return new CardGroup(cards, removeCallBack);
        }

        public static CardGroup operator +(CardGroup a, List<Card> cards)
        {
            CardGroup b = new CardGroup(cards, a._removeGroupCallBack);
            return a + b;
        }

        public static CardGroup operator -(CardGroup a, CardGroup b)
        {
            Action removeCallBack = a._removeGroupCallBack;
            List<Card> cards = a.RemoveAllCards();
            List<Card> bCards = b.RemoveAllCards();

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i] == bCards.Find(x => x == cards[i]))
                {
                    cards.RemoveAt(i);
                }
            }

            return new CardGroup(cards, removeCallBack);
        }

        public static CardGroup operator -(CardGroup a, List<Card> cards)
        {
            CardGroup b = new CardGroup(cards, a._removeGroupCallBack);
            return a - b;
        }

        public static bool operator ==(CardGroup a, CardGroup b)
        {
            if ((object)a == null && (object)b == null)
                return true;

            if ((object)a == null)
                return false;

            if ((object)b == null)
                return false;

            if (a.Count != b.Count)
                return false;


            for (int i = 0; i < a._groupedCards.Count; i++)
            {
                if (a._groupedCards[i] == b._groupedCards.Find(x => x == a._groupedCards[i]))
                    return false;
            }

            return true;
        }

        public static bool operator !=(CardGroup a, CardGroup b)
        {
            return !(a == b);
        }
    }
}
