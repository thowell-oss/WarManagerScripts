/* GroupDragCards.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WarManager.Cards
{
    /// <summary>
    /// Drag cards by group
    /// </summary>
    public class GroupDragCards
    {
        /// <summary>
        /// The card with extra chores (becauser all cards are on a grid and are uniform, all decisions reguarding where to shift are based off of this card).
        /// </summary>
        public Card LeadCard { get; private set; }

        public GroupDragCards(List<Card> cards)
		{
            cards.Sort();

            foreach (Card c in cards)
			{
                Debug.Log(c.point);
			}

            LeadCard = cards[0];
		}
    }
}
