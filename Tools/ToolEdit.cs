/* ToolEdit.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Cards;

namespace WarManager.Tools
{
    /// <summary>
    /// Possible adjacent card contexts
    /// </summary>
    public enum ToolEditAdjacentContext
    {
        /// <summary>
        /// There is no context information to show
        /// </summary>
        None,
        /// <summary>
        /// The card is just below the point
        /// </summary>
        South,
        /// <summary>
        /// The card is to the right of the point
        /// </summary>
        East,
        /// <summary>
        /// The card is above the point
        /// </summary>
        North,
        /// <summary>
        /// The card is to the left of the point
        /// </summary>
        West,
        /// <summary>
        /// The adjacent card below is one or more higher than the left card stack
        /// </summary>
        NorthAndWest,
        /// <summary>
        /// The adjacent card below is one ore more higher than the right card stack
        /// </summary>
        NorthAndEast,
        /// <summary> 
        /// The adjacent card below is one or more lower the left card stack
        /// </summary>
        SouthAndWest,
        /// <summary>
        /// The adjacent card below is one or more lower the right card stack
        /// </summary>
        SouthAndEast,
        /// <summary>
        /// The adjacent card is SE of the point
        /// </summary>
        TopLeftCorner,
    }

    /// <summary>
    /// Handles behavior when the edit tool is active
    /// </summary>
    public static class ToolEdit
    {
        /// <summary>
        /// Get cards between two coordintes
        /// </summary>
        /// <param name="start">the first x,y coordiante</param>
        /// <param name="end">the second x, y coordiante</param>
        /// <returns>returns a list of cards that exist in the rect</returns>
        public static List<Card> GetCardsFromRect((int x, int y) start, (int x, int y) end)
        {
            if (ToolsManager.SelectedTool != ToolTypes.Edit && ToolsManager.SelectedTool != ToolTypes.Highlight)
            {
                return null;
            }

            List<Card> cards = new List<Card>();

            if (start.x > end.x)
            {
                int temp = end.x;
                end.x = start.x;
                start.x = temp;
            }

            if (start.y > end.y)
            {
                int temp = end.y;
                end.y = start.y;
                start.y = temp;
            }

            var layers = CardUtility.CurrentSheet.GetAllLayers();


            for (int i = start.x; i <= end.x; i++)
            {
                for (int k = start.y; k <= end.y; k++)
                {
                    for (int j = 0; j < layers.Count; j++)
                    {
                        cards.Add(CardUtility.GetCard(new Point(i, k), layers[j]));
                    }
                }
            }

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i] == null)
                {
                    cards.RemoveAt(i);
                }
            }

            if (cards.Count < 1)
                return null;

            return cards;
        }

        /// <summary>
        /// Get information about the adjacent cards around a given point
        /// </summary>
        /// <param name="x">the x coordinate of the given point</param>
        /// <param name="y">the y coordiante of the given point</param>
		/// <param name="layer"> the current layer we are adjusting</param>
        /// <returns>returns tool edit adjacent context info</returns>
        public static ToolEditAdjacentContext GetContextInfo(int x, int y, Layer layer)
        {
            if (ToolsManager.SelectedTool != ToolTypes.Edit)
                return ToolEditAdjacentContext.None;

            Point loc = new Point(x, y);

            List<Point> positions = CardUtility.GetAllAdjacentGridPositions(loc);

            int amt = 0;

            Card[] cards = new Card[8];

            for (int i = 0; i < cards.Length; i++)
            {
                Card c = CardUtility.GetCard(positions[i], layer);
                if (c != null)
                {
                    cards[i] = c;
                    amt++;
                }

                if (amt > 3)
                {
                    return ToolEditAdjacentContext.None;
                }
            }

            if (amt == 0)
                return ToolEditAdjacentContext.None;

            if (amt == 1)
            {
                if (cards[1] != null)
                {
                    return ToolEditAdjacentContext.North;
                }

                if (cards[3] != null)
                {
                    return ToolEditAdjacentContext.East;
                }

                if (cards[4] != null)
                {
                    return ToolEditAdjacentContext.TopLeftCorner;
                }

                if (cards[5] != null)
                {
                    return ToolEditAdjacentContext.South;
                }

                if (cards[7] != null)
                {
                    return ToolEditAdjacentContext.West;
                }

                return ToolEditAdjacentContext.None;
            }
            else
            {
                if (cards[5] != null && cards[3] != null)
                {
                    return ToolEditAdjacentContext.SouthAndEast;
                }

                if (cards[5] != null && cards[7] != null)
                {
                    return ToolEditAdjacentContext.SouthAndWest;
                }
            }

            return ToolEditAdjacentContext.None;
        }
    }
}
