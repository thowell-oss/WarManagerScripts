/* CardCluster.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Cards
{
	/// <summary>
	/// Handles a partition of cards
	/// </summary>
	public class CardCluster
	{
		/// <summary>
		/// Stores and manages lower level card management systems
		/// </summary>
		private List<Card> CardData = new List<Card>();

		/// <summary>
		/// Handles card actions (moving, shifting).
		/// </summary>
		private CardActionsController CardActions { get; set; }

		/// <summary>
		/// Handles complex selection systems
		/// </summary>
		private CardSelectionController CardSelection { get; set; }

		#region bounds variables

		public Rect bounds { get; private set; }

		#endregion


		/// <summary>
		/// create an invisible box between two points (upper left and lower right) where all cards are inside of the cluster
		/// </summary>
		private void CalculateBounds()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Is the location inside the bounds of the cluster?
		/// </summary>
		/// <param name="location"></param>
		/// <returns>returns true if the location is inside the bounds</returns>
		public bool IsInBounds(Point location)
		{
			return bounds.IsInBounds(location);
		}


		/// <summary>
		/// Get a row of adjacent cards from a specific point in the bounds
		/// </summary>
		/// <param name="startPoint">the given point</param>
		/// <returns>retuns a list of cards if possible, null if not</returns>
		public List<Card> GetRowOfCards(Point startPoint)
		{
			if (IsInBounds(startPoint))
			{
				return CardSelection.GetAdjacentCardsByVector(startPoint, new Point(1, 0));
			}
			else
			{
				Debug.Log("Not in bounds");
				return null;
			}
		}

		/// <summary>
		/// Gets a column of adjacent cards from a given point in the bounds
		/// </summary>
		/// <param name="startPoint">the given point</param>
		/// <returns>retuns a list of cards if possible, null if not</returns>
		public List<Card> GetColumnOfCards(Point startPoint)
		{
			if (IsInBounds(startPoint))
			{
				return CardSelection.GetAdjacentCardsByVector(startPoint, new Point(0, 1));
			}
			else
			{
				Debug.Log("Not in bounds");
				return null;
			}
		}

		/// <summary>
		/// Update the card data after an action on cards has been taken
		/// </summary>
		public void UpdateCardData()
		{
			CalculateBounds();
			// check to see if there is any cards within the bounds, not in the cluster
			//check to see if there is a cluster intersection
		}
	}
}
