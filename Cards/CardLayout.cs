/* BoxLayout.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;

using System.Drawing;

using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// The card box layout system - handles the location of each of the corners relative to its grid location
    /// </summary>
    [Notes.Author("The card box layout system - handles the location of each of the corners relative to its grid location")]
    public class CardLayout
    {

        /// <summary>
        /// private backing field
        /// </summary>
        private Point _point;

        /// <summary>
        /// The Point of the 2D grid position in grid coordinates. Note: If you want to reloacate the card, do so by using the CardManger.MoveCard() methods.
        /// </summary>
        public Point point
        {
            get => _point;
            set
            {
                if (!value.IsInGridBounds)
                    return;

                _point = value;

                if (CardStretchPoints == null)
                {
                    CardStretchPoints = new Rect(_point, _point);
                }
                else
                {
                    CardStretchPoints.TopLeftCorner = _point;
                }
            }
        }
        /// <summary>
        /// the points of the card layout if the card is being stretched
        /// </summary>
        public Rect CardStretchPoints { get; set; }

        public bool isCardStretching => CardStretchPoints.SpacesTaken().Count > 1;

        /// <summary>
        /// The z-axis layer that the card is on
        /// </summary>
        public int Layer { get; set; } = 0;

        /// <summary>
        /// How far can the cards offset?
        /// </summary>
        public static (float x, float y) OffsetLimits { get; set; } = (20, 20);

        /// <summary>
        /// The actual offset x and y values
        /// </summary>
        private float _offsetX, _offsetY;

        /// <summary>
        /// The Offset of the card position from the grid
        /// </summary>
        public (float x, float y) Offset
        {
            get
            {
                return (_offsetX, _offsetY);
            }
        }

        /// <summary>
        /// The offset has been arbitrarily set
        /// </summary>
        public bool OffsetArbitrarilyDefined { get; private set; }

        /// <summary>
        /// Calculates the size of the card boundary (not scaled)
        /// </summary>
        public (float, float) Dimensions { get; set; } = (2.25f, .75f);

        /// <summary>
        /// The scale of the dimensions
        /// </summary>
        public float Scale { get; set; } = .1f;

        /// <summary>
        /// The location of the top left corner
        /// </summary>
        public (float, float) CornerTopLeft
        {
            get
            {
                return (-Dimensions.Item1 * Scale, Dimensions.Item2 * Scale);
            }
        }

        /// <summary>
        /// The location of the top right corner
        /// </summary>
        public (float, float) CornerTopRight
        {
            get
            {
                return (Dimensions.Item1 * Scale, Dimensions.Item2 * Scale);
            }
        }

        /// <summary>
        /// The location of the bottom right corner
        /// </summary>
        public (float, float) CornerBottomRight
        {
            get
            {
                return (Dimensions.Item1 * Scale, -Dimensions.Item2 * Scale);
            }
        }

        /// <summary>
        /// The location of the bottom left corner
        /// </summary>
        public (float, float) CornerBottomLeft
        {
            get
            {
                return (-Dimensions.Item1 * Scale, -Dimensions.Item2 * Scale);
            }
        }

        /// <summary>
        /// Create the card and set its location
        /// </summary>
        /// <param name="position"></param>
        public CardLayout(int x, int y)
        {
            point = new Point(x, y);
        }

        /// <summary>
        /// Set the offset
        /// </summary>
        /// <param name="offset">The new offset of the card</param>
        /// <param name="arbitrary">is the offset arbitrary or reactive to the other cards?</param>
        public void SetOffset((float x, float y) offset, bool arbitrary)
        {
            if (!OffsetArbitrarilyDefined)
            {
                _offsetY = offset.y;
                _offsetX = offset.x;

                if (_offsetX > OffsetLimits.x)
                {
                    _offsetX = OffsetLimits.x;
                }

                if (_offsetX < 0)
                    _offsetX = 0;

                if (_offsetY < OffsetLimits.y)
                    _offsetY = OffsetLimits.y;

                if (_offsetY > 0)
                {
                    _offsetY = 0;
                }
            }
            else
            {
                _offsetY += offset.y;
                _offsetX += offset.x;

                if (_offsetX > OffsetLimits.x)
                {
                    _offsetX = OffsetLimits.x;
                }

                if (_offsetX < 0)
                    _offsetX = 0;

                if (_offsetY < OffsetLimits.y)
                    _offsetY = OffsetLimits.y;

                if (_offsetY > 0)
                {
                    _offsetY = 0;
                }

            }

            OffsetArbitrarilyDefined = arbitrary;
        }

        /// <summary>
        /// Get the card location in world space
        /// </summary>
        /// <param name="startingPos">the universal offset</param>
        /// <param name="gridMultiplier">the multiplier used to distribute cards</param>
        /// <returns>returns a tuple of the corrected world position</returns>
        public Pointf GetCardGlobalLocation(Pointf startingPos, Pointf gridMultiplier)
        {
            float finalX = (point.x * gridMultiplier.x) + startingPos.x + Offset.Item1;
            float finalY = (point.y * gridMultiplier.x) + startingPos.x + Offset.Item2;

            return new Pointf(finalX, finalY);
        }

        /// <summary>
        /// Get the card location in world space
        /// </summary>
        /// <param name="startingPos">the universal offset</param>
        /// <param name="gridMultiplier">the multiplier used to distribute cards</param>
        /// <returns>returns a tuple of the corrected world position</returns>
        public static (float, float) GetCardGlobalLocation(Point GridPosition, (float, float) Offset, (float, float) startingPos, (float, float) gridMultiplier)
        {
            float finalX = (GridPosition.x * gridMultiplier.Item1) + startingPos.Item1 + Offset.Item1;
            float finalY = (GridPosition.y * gridMultiplier.Item2) + startingPos.Item2 + Offset.Item2;

            return (finalX, finalY);
        }

        /// <summary>
        /// Convert world space into a grid location (rounded)
        /// </summary>
        /// <param name="location">the world space location</param>
        /// <param name="startingPos">the global offset</param>
        /// <param name="gridMultiplier">the grid multiplier for each card</param>
        /// <returns>returns a tuple (int,int) of the resulting grid location</returns>
        public Point GetCardGridLocation((float, float) location, (float, float) startingPos, (float, float) gridMultiplier)
        {
            float GridX = (location.Item1 - startingPos.Item1 - Offset.Item1) / gridMultiplier.Item1;
            float GridY = (location.Item2 - startingPos.Item2 - Offset.Item2) / gridMultiplier.Item2;

            int x = (int)System.Math.Round((decimal)GridX);
            int y = (int)System.Math.Round((decimal)GridY);

            return new Point(x, y);
        }

        /// <summary>
        /// Convert world space into a grid location (rounded)
        /// </summary>
        /// <param name="point">the world space location</param>
        /// <param name="startingPos">the global offset</param>
        /// <param name="gridMultiplier">the grid multiplier for each card</param>
        /// <returns>returns a tuple (int,int) of the resulting grid location</returns>
        public Point GetCardGridLocation(Pointf point, WarGrid grid)
        {
            float GridX = (point.x - grid.Offset.x - Offset.Item1) / grid.GridScale.x;
            float GridY = (point.y - grid.Offset.y - Offset.Item2) / grid.GridScale.y;

            int x = (int)System.Math.Round((decimal)GridX);
            int y = (int)System.Math.Round((decimal)GridY);

            return new Point(x, y);
        }

        /// <summary>
        /// Convert world space into a grid location (rounded)
        /// </summary>
        /// <param name="location">the world space location</param>
        /// <param name="startingPos">the global offset</param>
        /// <param name="gridMultiplier">the grid multiplier for each card</param>
        /// <returns>returns a tuple (int,int) of the resulting grid location</returns>
        public static Point GetCardGridLocation((float, float) location, (float, float) Offset, (float, float) startingPos, (float, float) gridMultiplier)
        {
            float GridX = (location.Item1 - startingPos.Item1 - Offset.Item1) / gridMultiplier.Item1;
            float GridY = (location.Item2 - startingPos.Item2 - Offset.Item2) / gridMultiplier.Item2;


            int x = (int)System.Math.Round((decimal)GridX);
            int y = (int)System.Math.Round((decimal)GridY);

            return new Point(x, y);
        }

        /// <summary>
        /// Get the dimensions with the scale
        /// </summary>
        /// <returns>returns a tuple</returns>
        public (float, float) GetScaledCardDimensions()
        {
            return (Dimensions.Item1 * Scale, Dimensions.Item2 * Scale);
        }
    }
}
