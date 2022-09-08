using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Cards
{
    public static class CardGroupsManager
    {
        /// <summary>
        /// The list of card groups
        /// </summary>
        /// <typeparam name="CardGroup"></typeparam>
        /// <returns></returns>
        private static List<CardGroup> _cardGroups = new List<CardGroup>();

        /// <summary>
        /// Add a group of cards to the list
        /// </summary>
        /// <param name="cards"></param>
        public static CardGroup AddCardGroup(List<Card> cards)
        {
            CardGroup newCardGroup = null;

            Action removeCardGroupAction = () => RemoveCardGroup(newCardGroup);
            newCardGroup = new CardGroup(cards, removeCardGroupAction);

            _cardGroups.Add(newCardGroup);

            _cardGroups.Sort();

            return newCardGroup;
        }

        /// <summary>
        /// Remove a card group
        /// </summary>
        /// <param name="c"></param>
        private static void RemoveCardGroup(CardGroup c)
        {
            if (c != null)
                _cardGroups.Remove(c);
        }

    }
}
