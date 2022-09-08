

using System;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    public struct Pointf
    {
        /// <summary>
        /// The x coordinate of the pointf
        /// </summary>
        [JsonPropertyName("x")]
        public float x { get; private set; }

        /// <summary>
        /// The y coorindate of the pointf
        /// </summary>
        [JsonPropertyName("y")]
        public float y { get; private set; }

        public Pointf(float X, float Y)
        {
            x = X;
            y = Y;
        }

        /// <summary>
        /// If the point is within the grid bounds (x >= 0 and y <= 0)
        /// </summary>
        /// <returns>returns true if the point is within bounds, false if not</returns>
        public bool IsInGridBounds
        {
            get
            {
                if (x >= 0 && y <= 0)
                    return true;
                return false;
            }
        }

        #region Arbitrary Points

        /// <summary>
        /// Returns a point with the coordiantes (0, 0)
        /// </summary>
        /// <returns></returns>
        public static Pointf zero
        {
            get
            {
                return new Pointf(0, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordiantes (1, 1)
        /// </summary>
        /// <value></value>
        public Pointf one
        {
            get
            {
                return new Pointf(1, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (0, 1)
        /// </summary>
        /// <returns></returns>
        public static Pointf up
        {
            get
            {
                return new Pointf(0, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordiantes (0, -1)
        /// </summary>
        /// <returns></returns>
        public static Pointf down
        {
            get
            {
                return new Pointf(0, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (-1, 0)
        /// </summary>
        /// <returns></returns>
        public static Pointf left
        {
            get
            {
                return new Pointf(-1, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordiantes (1, 0)
        /// </summary>
        /// <returns></returns>
        public static Pointf right
        {
            get
            {
                return new Pointf(1, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates(1, 1)
        /// </summary>
        /// <returns></returns>
        public static Pointf northWest
        {
            get
            {
                return new Pointf(1, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (1, -1)
        /// </summary>
        /// <returns></returns>
        public static Pointf southWest
        {
            get
            {
                return new Pointf(1, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (1, -1)
        /// </summary>
        /// <returns></returns>
        public static Pointf northEast
        {
            get
            {
                return new Pointf(1, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (-1, -1)
        /// </summary>
        /// <returns></returns>
        public static Pointf southEast
        {
            get
            {
                return new Pointf(-1, -1);
            }
        }

        #endregion

        /// <summary>
        /// Returns a grid int into a world float
        /// </summary>
        /// <param name="x">the grid axis integer</param>
        /// <param name="offset">how much the world offsets</param>
        /// <param name="scale">the card separation multiplier</param>
        /// <returns>returns a world axis float</returns>
        public static float GridToWorld(int x, float offset, float scale)
        {
            return Point.GridToWorld(x, offset, scale);
        }

        /// <summary>
        /// Convert a grid point into a world tuple
        /// </summary>
        /// <param name="point">the grid point</param>
        /// <param name="offset">the world offset</param>
        /// <param name="gridSize">the grid size</param>
        /// <returns>retuns a tuple</returns>
        public static Pointf GridToWorld(Point point, Pointf offset, Pointf gridSize)
        {
            return Point.GridToWorld(point, offset, gridSize);
        }

        /// <summary>
        /// Convert a grid point into a world point using a given warGrid
        /// </summary>
        /// <param name="point">the grid point</param>
        /// <param name="grid">the given war grid</param>
        /// <returns>returns a pointf of the world point</returns>
        public static Pointf GridToWorld(Point point, WarGrid grid)
        {
            if (point == null)
                throw new NullReferenceException("The point cannot be null");

            if (grid == null)
                throw new NullReferenceException("the grid cannot be null");

            return Point.GridToWorld(point, grid.Offset, grid.GridScale);
        }

        /// <summary>
        /// Returns a world float into a grid int
        /// </summary>
        /// <param name="x">the world float</param>
        /// <param name="offset">the world offset</param>
        /// <param name="scale">the card separation multiplier</param>
        /// <returns>retuns a grid int</returns>
        public static int WorldToGrid(float x, float offset, float scale)
        {
            return Point.WorldToGrid(x, offset, scale);
        }

        /// <summary>
        /// Convert a world point into a grid point
        /// </summary>
        /// <param name="point">the world point</param>
        /// <param name="offset">the world offset</param>
        /// <param name="scale">the grid size</param>
        /// <returns>retuns a Point</returns>
        public static Point WorldToGrid(Pointf point, Pointf offset, Pointf scale)
        {
            return Point.WorldToGrid(point, offset, scale);
        }

        /// <summary>
        /// Convert a world point into a grid point using a given war grid
        /// </summary>
        /// <param name="point">the world point</param>
        /// <param name="grid">the grid point</param>
        /// <returns>returns a grid point</returns>
        public static Point WorldToGrid(Pointf point, WarGrid grid)
        {
            return Point.WorldToGrid(point, grid.Offset, grid.GridScale);
        }

        /// <summary>
        /// Returns the distance between two points
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <returns>returns a double of the distance squared</returns>
        public static double DistanceSquared(Pointf a, Pointf b)
        {
            double x = Math.Pow((double)(a.x - b.x), 2);
            double y = Math.Pow((double)(a.y - b.y), 2);

            return x + y;
        }

        /// <summary>
        /// Get the vector magnitude of the point (squared)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>retuns a double</returns>
        public double SquareMagnitude()
        {
            return Math.Pow((double)x, 2) + Math.Pow((double)y, 2);
        }

        /// <summary>
        /// Attempts to format a string into a point (using Int32) (formats: 'x,y' and '(x,y)')
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="output">the resulting output Point</param>
        /// <returns>returns true if the conversion was succesful, false if not</returns>
        public static bool TryParse(string input, out Pointf output)
        {
            if (string.IsNullOrEmpty(input))
            {
                output = Pointf.zero;
                return false;
            }

            input.Trim();

            if (input.StartsWith("("))
            {
                input = input.Remove(0);
            }

            if (input.EndsWith(")"))
            {
                input = input.Replace(")", "");
            }

            if (input.Contains(" "))
            {
                input.Replace(" ", "");
            }

            if (input.Contains(","))
            {
                string[] str = input.Split(',');

                float x;
                float y;

                if (float.TryParse(str[0], out x))
                {
                    if (float.TryParse(str[1], out y))
                    {
                        output = new Pointf(x, y);
                        return true;
                    }
                }
            }

            output = Pointf.zero;
            return false;
        }

        /// <summary>
        /// If the point is out of bounds, it can be corrected by calling this method
        /// </summary>
        /// <param name="p">the point to correct</param>
        /// <returns></returns>
        public static Pointf GetNearestValidPoint(Pointf p)
        {
            if (p.x < 0)
            {
                p = new Pointf(0, p.y);
            }

            if (p.y > 0)
            {
                p = new Pointf(p.x, 0);
            }

            return p;
        }

        #region Operators
        public static bool operator ==(Pointf a, Pointf b)
        {
            if (a.x == b.x)
            {
                if (a.y == b.y)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool operator !=(Pointf a, Pointf b)
        {
            return !(a == b);
        }

        public static Pointf operator *(Pointf a, float x)
        {
            return new Pointf(a.x * x, a.y * x);
        }

        public static Pointf operator *(float x, Pointf a)
        {
            return a * x;
        }

        public static Pointf operator +(Pointf a, Pointf b)
        {
            return new Pointf(a.x + b.x, a.y + b.y);
        }

        public static Pointf operator -(Pointf a, Pointf b)
        {
            return new Pointf(a.x - b.x, a.y - b.y);
        }

        public static Pointf operator -(Pointf a)
        {
            return new Pointf(-a.x, -a.y);
        }

        /// <summary>
        /// Sort the points by direction
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <param name="direction">the direction</param>
        /// <returns>returns an integer used in compliance with the 'foo.CompareTo()' standard</returns>
        public static int SortByDirection(Pointf a, Pointf b, Point direction)
        {
            if (a == null)
                throw new NullReferenceException("the a point cannot be null");

            if (b == null)
                throw new NullReferenceException("the b point cannot be null");

            if (direction == null)
                throw new NullReferenceException("The direction cannot be null");

            int final = 0;

            if (direction.x > 0)
            {
                final = -a.x.CompareTo(b.x);
            }
            else if (direction.x < 0)
            {
                final = a.x.CompareTo(b.x);
            }

            if (final != 0)
            {
                return final;
            }

            if (direction.y > 0)
            {
                final = -a.y.CompareTo(b.y);
            }
            else if (direction.y < 0)
            {
                final = a.y.CompareTo(b.y);
            }
            return final;
        }

        #endregion

        public bool Equals(Pointf other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>
        /// Two ways to convert a point into a string - pretty print for viewing and the other for storage
        /// </summary>
        /// <param name="prettyPrint">should the to string print with spaces and parenthesis?</param>
        /// <returns>returns the string of the x and y values</returns>
        public string ToString(bool prettyPrint = true)
        {
            string str = "";

            if (prettyPrint)
            {
                str = "( " + x.ToString() + ", " + y.ToString() + ")";
            }
            else
            {
                str = x.ToString() + "," + y.ToString();
            }

            return str;
        }

    }
}
