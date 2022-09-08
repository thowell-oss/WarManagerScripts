using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Cards
{
	/// <summary>
	/// Handles lower level card actions (shifting, moving)
	/// </summary>
	public class CardActionsController
	{
		/// <summary>
		/// The data assigned with the cluster
		/// </summary>
		private List<Card> _cardData;

		/// <summary>
		/// The cluster reference
		/// </summary>
		private CardCluster _associatedCluster;

		public CardActionsController(List<Card> manager, CardCluster cluster)
		{
			_cardData = manager;
			_associatedCluster = cluster;

		}

		/// <summary>
		/// get the card from the associated card data
		/// </summary>
		/// <param name="loc">the location in the cluster</param>
		/// <returns>returns a card if possible, false if not</returns>
		public Card GetCard(Point loc)
		{
			return _cardData.Find((x) => (x.point == loc));
		}

		/// <summary>
		/// Moves the card on the sheet and also in the card heap
		/// </summary>
		/// <param name="loc">The location the card is currently</param>
		/// <param name="newLoc">the new location</param>
		public void MoveCard(Card c, Point newLoc, bool createSnapShot = true)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Shift a list of cards
		/// </summary>
		/// <param name="cards">the cards being shifted</param>
		/// <param name="pushCards">should the cards be pushed or pulled?</param>
		/// <param name="overrideShiftingRules">should the general settings be ignored?</param>
		public void ShiftCards(List<Card> cards, Point vector, bool pushCards = true, bool overrideShiftingRules = false)
		{
			cards.Sort();

			if (vector.y < 0 || vector.x > 0)
			{
				for (int i = cards.Count - 1; i >= 0; i--)
				{
					ShiftCards(cards[i].point, vector, pushCards, overrideShiftingRules);
				}
				return;
			}

			foreach (Card c in cards)
			{
				ShiftCards(c.point, vector, pushCards, overrideShiftingRules);
			}

			_associatedCluster.UpdateCardData();
		}


		/// <summary>
		/// Shift cards according to a 2D vector
		/// </summary>
		/// <param name="loc">the location to start</param>
		/// <param name="vector">the vector</param>
		/// <param name="pushCards">Should the cards be pushed away or pulled?</param>
		/// /// <param name="overrideShiftingRules">should the general settings be ignored?</param>
		public void ShiftCards(Point loc, Point vector, bool pushCards = true, bool overrideShiftingRules = false)
		{
			ShiftCardsRecursive(loc, vector, !pushCards, overrideShiftingRules);

			_associatedCluster.UpdateCardData();
		}

		/// <summary>
		/// Internal shift cards according to 2D vector (recursive) use ShiftCards(loc, vector) instead
		/// </summary>
		/// <param name="loc">the location to start shifting cards</param>
		/// <param name="vector">the direction to shift cards</param>
		/// <param name="seekDirection">should the cards be pulled or pushed when seeking? (set true to pull cards)</param>
		/// <param name="overrideShiftingRules">Overrides the limits to shifting set by general settings</param>
		private void ShiftCardsRecursive(Point loc, Point vector, bool seekDirection, bool overrideShiftingRules)
		{
			if (!GeneralSettings.AllowSideShifting && !GeneralSettings.AllowUpDownShifting)
				return;

			if (!GeneralSettings.AllowSideShifting && !overrideShiftingRules)
			{
				if (vector.x != 0)
				{
					vector = new Point(0, vector.y);
					//Debug.LogWarning("Side shifting not allowed");
				}
			}

			if (!GeneralSettings.AllowUpDownShifting && !overrideShiftingRules)
			{
				if (vector.y != 0)
				{
					vector = new Point(vector.x, 0);
					//Debug.LogWarning("up down shifting not allowed");
				}
			}

			if (vector == Point.zero)
			{
				return;
			}

			Card c = GetCard(loc);

			if (c != null)
			{
				int x = vector.x;
				int y = vector.y;

				int dirX = 0;
				int dirY = 0;

				if (x > 0)
					dirX = 1;
				if (x < 0)
					dirX = -1;

				if (y > 0)
					dirY = 1;
				if (y < 0)
					dirY = -1;

				if (seekDirection)
				{
					dirX = -dirX;
					dirY = -dirY;

					Point dir = new Point(dirX, dirY);
					MoveCard(c, new Point(loc.x + vector.x, loc.y + vector.y));
					ShiftCardsRecursive( new Point(loc.x + dir.x, loc.y + dir.y), vector, seekDirection, overrideShiftingRules);
				}
				else
				{
					Point dir = new Point(dirX, dirY);
					ShiftCardsRecursive(new Point(loc.x + dir.x, loc.y + dir.y), vector, seekDirection, overrideShiftingRules);
					MoveCard(c, new Point(loc.x + vector.x, loc.y + vector.y));
				}

				CardUtility.AddCardToUpdateQueue(c);
			}
		}
	}
}