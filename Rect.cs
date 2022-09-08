/* Rect.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Rectangle
    /// </summary>
    public class Rect
    {
        private Point _topLeftCorner;

        /// <summary>
        /// The top left corner of the rect
        /// </summary>
        public Point TopLeftCorner
        {
            get
            {
                return _topLeftCorner;
            }
            set
            {
                _topLeftCorner = value;

                if (!_topLeftCorner.IsInGridBounds)
                {
                    _topLeftCorner = Point.GetNearestValidPoint(_topLeftCorner);
                }
            }
        }

        private Point _bottomRightCorner;

        /// <summary>
        /// The bottom right corner of the rect
        /// </summary>
        public Point BottomRightCorner
        {
            get
            {
                return _bottomRightCorner;
            }
            set
            {
                _bottomRightCorner = value;

                if (!_bottomRightCorner.IsInGridBounds)
                {
                    _bottomRightCorner = Point.GetNearestValidPoint(_bottomRightCorner);
                }
            }
        }

        /// <summary>
        /// The bottom left corner of the rect
        /// </summary>
        public Point BottomLeftCorner
        {
            get
            {
                return new Point(TopLeftCorner.x, BottomRightCorner.y);
            }
        }

        /// <summary>
        /// The top right corner of the rect
        /// </summary>
        public Point TopRightCorner
        {
            get
            {
                return new Point(BottomRightCorner.x, TopLeftCorner.y);
            }
        }

        /// <summary>
        /// The width of the rect
        /// </summary>
        public int Width
        {
            get
            {
                return Math.Abs(TopLeftCorner.x - BottomRightCorner.x);
            }
        }

        /// <summary>
        /// The height of the rectangle
        /// </summary>
        public int Height
        {
            get
            {
                return Math.Abs(TopLeftCorner.y - BottomRightCorner.y);
            }
        }

        /// <summary>
        /// Get the center of the rect in world coordinates
        /// </summary>
        /// <param name="grid">the war grid</param>
        /// <returns></returns>
        public Pointf GetWorldCenter(WarGrid grid)
        {
            return GetWorldCenter(grid.Offset, grid.GridScale);
        }

        /// <summary>
        /// Get the center of the rect points in world coordinates
        /// </summary>
        /// <param name="offset">the offset</param>
        /// <param name="gridSize">the grid scale</param>
        /// <returns></returns>
        public Pointf GetWorldCenter(Pointf offset, Pointf gridSize)
        {

            Pointf tl = Point.GridToWorld(TopLeftCorner, offset, gridSize);
            Pointf br = Point.GridToWorld(BottomRightCorner, offset, gridSize);

            float w = tl.x - br.x;

            //Debug.Log(w + " " + tl.x + " " + br.x);

            float h = tl.y - br.y;

            w = Math.Abs(w) / 2;
            h = Math.Abs(h) / 2;

            w = tl.x + w;
            h = tl.y - h;

            return new Pointf(w, h);
        }

        /// <summary>
        /// Create a rectangle (of size 1)
        /// </summary>
        /// <param name="point">the point location</param>
        /// <exception cref="NullReferenceException">thrown when the <paramref name="point"/> is null </exception>
        public Rect(Point point)
        {
            if (point == null) throw new NullReferenceException("the point cannot be null");

            TopLeftCorner = point;
            BottomRightCorner = point;
        }

        /// <summary>
        /// Create a rectangle
        /// </summary>
        /// <param name="topLeftCorner">top left rectangle corner</param>
        /// <param name="bottomRightCorner">bottom right rectangle corner</param>
        /// <exception cref="NullReferenceException">thrown when the <paramref name="bottomRightCorner"/> or <paramref name="topLeftCorner"/> is null </exception>
        public Rect(Point topLeftCorner, Point bottomRightCorner)
        {
            if (topLeftCorner == null)
                throw new NullReferenceException("The top left corner cannot be null");

            if (bottomRightCorner == null)
                throw new NullReferenceException("the bottom right corner cannot be null");

            topLeftCorner = Point.GetNearestValidPoint(topLeftCorner);
            bottomRightCorner = Point.GetNearestValidPoint(bottomRightCorner);

            _topLeftCorner = topLeftCorner;
            _bottomRightCorner = bottomRightCorner;
        }

        /// <summary>
        /// Check to see if the given point is inside the bounds of the rectangle
        /// </summary>
        /// <param name="point">the given point</param>
        /// <returns>returns true if the point is in the bounds of the rectangle, false if not</returns>
        public bool IsInBounds(Point point)
        {
            if (point.x >= TopLeftCorner.x && point.x <= BottomRightCorner.x)
            {
                if (point.y <= TopLeftCorner.y && point.y >= BottomRightCorner.y)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Set the top left corner and the rectangle will move (and maintain its width and height)
        /// </summary>
        /// <param name="topLeftCorner">the top left corner</param>
        /// <param name="normalizedHeightOffset">the height of the rect</param>
        /// <param name="normalizedWidthOffset">the width of the rect</param>
        public void UpdateRect(Point topLeftCorner, int normalizedWidthOffset, int normalizedHeightOffset)
        {
            if (normalizedWidthOffset < 0)
                normalizedWidthOffset = -normalizedWidthOffset;

            if (normalizedHeightOffset < 0)
                normalizedHeightOffset = -normalizedHeightOffset;

            BottomRightCorner = new Point(topLeftCorner.x + normalizedWidthOffset, topLeftCorner.y - normalizedHeightOffset);
        }

        /// <summary>
        /// Draw a rect bounds between two points
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <returns>retuns a rect between two points</returns>
        public static Rect DrawRect(Point a, Point b)
        {
            if (a == null)
                throw new NullReferenceException("the first point cannot be null");

            if (b == null)
                throw new NullReferenceException("the second point cannot be null");

            Point topLeft = Point.zero;
            Point bottomRight = Point.zero;

            if (a.x > b.x)
            {
                topLeft = new Point(b.x, topLeft.y);
                bottomRight = new Point(a.x, bottomRight.y);
            }
            else
            {
                topLeft = new Point(a.x, topLeft.y);
                bottomRight = new Point(b.x, bottomRight.y);
            }

            if (a.y > b.y)
            {
                topLeft = new Point(topLeft.x, a.y);
                bottomRight = new Point(bottomRight.x, b.y);
            }
            else
            {
                topLeft = new Point(topLeft.x, b.y);
                bottomRight = new Point(bottomRight.x, a.y);
            }

            return new Rect(topLeft, bottomRight);
        }

        /// <summary>
        /// Get the grid spaces taken by the rect. 
        /// </summary>
		/// <remarks>The list is not sorted by any particular order</remarks>
        /// <returns>returns a list of spaces</returns>
        public List<Point> SpacesTaken()
        {
            List<Point> spacesList = new List<Point>();

            for (int i = 0; i <= Width; i++)
            {
                for (int j = 0; j <= Height; j++)
                {
                    Point p = TopLeftCorner + new Point(i, -j);
                    spacesList.Add(p);
                }
            }

            spacesList.Sort();

            return spacesList;
        }

        /// <summary>
        /// Gets the location of all spaces on top of the rect
        /// </summary>
        /// <returns>returns the list of points</returns>
        public List<Point> GetTopPerimeterSpacesTaken()
        {

            List<Point> spaces = new List<Point>();

            for (int i = TopLeftCorner.x; i <= BottomRightCorner.x; i++)
            {
                spaces.Add(new Point(i, TopLeftCorner.y));
            }

            return spaces;
        }

        /// <summary>
        /// Get the locations of all points on the bottom perimeter of the rect
        /// </summary>
        /// <returns>returns the list of points</returns>
        public List<Point> GetBottomPerimeterSpacesTaken()
        {
            List<Point> spaces = new List<Point>();

            for (int i = BottomRightCorner.x; i >= TopLeftCorner.x; i--)
            {
                spaces.Add(new Point(i, BottomRightCorner.y));
            }

            return spaces;
        }

        /// <summary>
        /// Get the locations of all points on the right perimeter of the rect
        /// </summary>
        /// <returns>returns a list of points</returns>
        public List<Point> GetRightPerimeterSpacesTaken()
        {
            List<Point> spaces = new List<Point>();

            for (int i = TopLeftCorner.y; i >= BottomRightCorner.y; i--)
            {
                spaces.Add(new Point(BottomRightCorner.x, i));
            }

            return spaces;
        }

        /// <summary>
        /// Get the locations of all points on the left perimeter of the rect
        /// </summary>
        /// <returns>returns a list of points</returns>
        public List<Point> GetLeftPerimeterSpacesTaken()
        {
            List<Point> spaces = new List<Point>();

            for (int i = BottomRightCorner.y; i <= TopLeftCorner.y; i++)
            {
                spaces.Add(new Point(TopLeftCorner.x, i));
            }

            return spaces;
        }

        /// <summary>
        /// Get the perimeter of the square
        /// </summary>
        /// <returns>returns a list of points</returns>
        public List<Point> GetPerimeter()
        {
            List<Point> spaces = new List<Point>();

            spaces.AddRange(GetTopPerimeterSpacesTaken());
            spaces.AddRange(GetRightPerimeterSpacesTaken());
            spaces.AddRange(GetBottomPerimeterSpacesTaken());
            spaces.AddRange(GetLeftPerimeterSpacesTaken());

            for (int i = spaces.Count - 1; i >= 0; i--) // remove any duplicates
            {
                List<Point> data = spaces.FindAll(x => x == spaces[i]);

                if (data.Count > 1)
                {
                    spaces.RemoveAt(i);
                }
            }

            return spaces;
        }

        /// <summary>
        /// Finds a rect from a list of points
        /// </summary>
        /// <param name="pointsList">the list of points</param>
        /// <returns></returns>
        public static Rect DrawRectFromListOfSpaces(List<Point> pointsList)
        {
            if (pointsList == null)
                throw new NullReferenceException("The list of points cannot be null");

            if (pointsList.Count < 2)
            {
                throw new NotSupportedException("A list of points with a count less than two is not supported.");
            }

            pointsList.Sort(delegate (Point a, Point b)
            {
                return Point.SortByDirection(a, b, Point.southEast);
            });


            return new Rect(pointsList[0], pointsList[pointsList.Count - 1]);
        }

    }
}
