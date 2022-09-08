using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles lower level card selection systems
    /// </summary>
    public class CardSelectionController
    {
        /// <summary>
        /// The card data assigned with the cluster
        /// </summary>
        private List<Card> _cardData = new List<Card>();

        /// <summary>
        /// The cluster reference
        /// </summary>
        private CardCluster _assocatedCluster;

        public CardSelectionController(List<Card> manager, CardCluster cluster)
        {
            _cardData = manager;
            _assocatedCluster = cluster;
        }

        /// <summary>
        /// get the card from the associated card data
        /// </summary>
        /// <param name="loc">the location in the cluster</param>
        /// <returns>returns a card if possible, false if not</returns>
        public Card GetCard(Point loc)
        {
            return _cardData.Find((x) =>(x.point == loc));
        }

        /// <summary>
        /// Gets the directions adjacent to the grid point (N, E, S, W - not diagonal)
        /// </summary>
        /// <param name="centralCard">the central location</param>
        /// <returns>returns a tuple (int, int) of the adjacent cards</returns>
        public static Point[] GetCrossAdjacentGridPositions(Point centralCard)
        {
            Point[] directions = new Point[4] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) };
            Point[] gridPositions = new Point[4];

            for (int i = 0; i < directions.Length; i++)
            {
                gridPositions[i] = new Point(centralCard.x + directions[i].x, centralCard.y + directions[i].y);
            }

            return gridPositions;
        }

        /// <summary>
        /// Gets the diagonal directions adjacent to the grid point (NW, NE, SE, SW - diagonal)
        /// </summary>
        /// <param name="centeralCard">the centeral grid point</param>
        /// <returns>returns a tuple (int, int) of the adjacent cards</returns>
        public Point[] GetDiagonalAdjacentGridPositions(Point centralCard)
        {
            Point[] directions = new Point[4] { new Point(-1, 1), new Point(1, 1), new Point(1, -1), new Point(-1, -1) };
            Point[] gridPositions = new Point[4];

            for (int i = 0; i < directions.Length; i++)
            {
                gridPositions[i] = new Point(centralCard.x + directions[i].x, centralCard.y + directions[i].y);
            }

            return gridPositions;
        }

        /// <summary>
        /// Gets any positions adjacent to the card (including diagonal adjacent locations). The list starts at NW and goes clockwise until it reaches W (NW, N, NE, E, SE, S, SW, W).
        /// </summary>
        public List<Point> GetAllAdjacentGridPositions(Point location)
        {
            List<Point> pos = new List<Point>();

            pos.AddRange(GetCrossAdjacentGridPositions(location));
            pos.AddRange(GetDiagonalAdjacentGridPositions(location));

            List<Point> finalPos = new List<Point>();

            for (int i = 0; i < pos.Count / 2; i++)
            {
                finalPos.Add(pos[i]);
                finalPos.Add(pos[i + 4]);
            }

            return finalPos;
        }

        /// <summary>
        /// Get Adjacent cards in a line
        /// </summary>
        /// <param name="startPoint">the start locaton to look for a line of cards</param>
        /// <param name="vector">direction to look for cards</param>
        /// <returns>returns a list of cards if cards are found, if not an empty list will be returned</returns>
        public List<Card> GetAdjacentCardsByVector(Point startPoint, Point vector)
        {
            if (vector.x < 0)
                vector = new Point(0, vector.y);

            if (vector.x > 1)
                vector = new Point(1, vector.y);

            if (vector.y < 0)
                vector = new Point(vector.x, 0);

            if (vector.y > 1)
                vector = new Point(vector.x, 1);

            List<Card> cards = new List<Card>();

            Point nextLocation = new Point(startPoint.x, startPoint.y);

            Card c = GetCard(nextLocation);

            while (c != null)
            {
                cards.Add(c);

                nextLocation = new Point(nextLocation.x + vector.x, nextLocation.y + vector.y);

                c = GetCard(nextLocation);
            }

            nextLocation = new Point(startPoint.x - vector.x, startPoint.y - vector.y);

            c = GetCard(nextLocation);

            while (c != null)
            {
                cards.Add(c);

                nextLocation = new Point(nextLocation.x - vector.x, nextLocation.y - vector.y);

                c = GetCard(nextLocation);
            }

            return cards;
        }
    }
}
