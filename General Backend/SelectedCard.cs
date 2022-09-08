/* SelectedCard.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;



namespace WarManager.Cards
{
	/// <summary>
	/// A structure of the selected card contains a card and a gui card
	/// </summary>
	public class SelectedCard<TguiCard> : IEquatable<SelectedCard<TguiCard>>, IComparable<SelectedCard<TguiCard>>
	{
		/// <summary>
		/// The Card backend reference
		/// </summary>
		public Card Card { get; private set; }
		/// <summary>
		/// The Card frontend reference
		/// </summary>
		public TguiCard GUICard { get; private set; }

		public SelectedCard()
		{

		}

		public SelectedCard(Card c, TguiCard guiCard)
		{
			GUICard = guiCard;
			Card = c;
		}

		public override string ToString()
		{
			return Card.ToString() + " | " + GUICard.ToString();
		}

		public bool Equals(SelectedCard<TguiCard> other)
		{
			return other.Card.ID == Card.ID;
		}

		public int CompareTo(SelectedCard<TguiCard> other)
		{
			return Card.ID.CompareTo(other.Card.ID);
		}
	}
}
