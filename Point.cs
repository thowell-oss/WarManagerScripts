/* Point.cs
 * Author: Taylor Howell
 */

using System;

namespace WarManager
{
    /// <summary>
    /// point library for handling custom grid systems
    /// </summary>
    [Serializable]
    [Notes.Author("point library for handling custom grid systems (immutable)")]
    public struct Point : IEquatable<Point>, IComparable<Point>
    {
        /// <summary>
        /// The x coordinate of the point
        /// </summary>
        public int x { get; private set; }

        /// <summary>
        /// The y coorindate of the point
        /// </summary>
        public int y { get; private set; }

        public Point(int X, int Y)
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
        public static Point zero
        {
            get
            {
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (1, 1)
        /// </summary>
        /// <value></value>
        public static Point one
        {
            get
            {
                return new Point(1, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (0, 1)
        /// </summary>
        /// <returns></returns>
        public static Point up
        {
            get
            {
                return new Point(0, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordiantes (0, -1)
        /// </summary>
        /// <returns></returns>
        public static Point down
        {
            get
            {
                return new Point(0, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (-1, 0)
        /// </summary>
        /// <returns></returns>
        public static Point left
        {
            get
            {
                return new Point(-1, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordiantes (1, 0)
        /// </summary>
        /// <returns></returns>
        public static Point right
        {
            get
            {
                return new Point(1, 0);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates(1, 1)
        /// </summary>
        /// <returns></returns>
        public static Point northWest
        {
            get
            {
                return new Point(1, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (1, -1)
        /// </summary>
        /// <returns></returns>
        public static Point southWest
        {
            get
            {
                return new Point(-1, -1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (1, -1)
        /// </summary>
        /// <returns></returns>
        public static Point northEast
        {
            get
            {
                return new Point(1, 1);
            }
        }

        /// <summary>
        /// Returns a point with the coordinates (-1, -1)
        /// </summary>
        /// <returns></returns>
        [Obsolete("southeast written incorrectly...")]
        public static Point southEast
        {
            get
            {
                return new Point(-1, -1);
            }
        }

        #endregion

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

        /// <summary>
        /// Converts a grid point into a local partition point
        /// </summary>
        /// <param name="p">the point to convert</param>
        /// <returns>returns a local point</returns>
        public static Point GridToLocal(Point p)
        {
            int x = p.x % 10;
            int y = p.y % 10;

            if (y != 0)
            {
                y = 10 + y;
            }
            return new Point(x, y);
        }

        /// <summary>
        /// Convert a local point into a grid point
        /// </summary>
        /// <param name="p">the point to convert</param>
        /// <param name="topLeftCorner">the top left corner of the partition</param>
        /// <returns>returns a grid point</returns>
        public static Point LocalToGrid(Point p, Point topLeftCorner)
        {
            int x = p.x + topLeftCorner.x;
            int y = -p.y - topLeftCorner.y;

            return new Point(x, y);
        }

        /// <summary>
        /// Returns a grid int into a world float
        /// </summary>
        /// <param name="x">the grid axis integer</param>
        /// <param name="worldOffset">how much the world offsets</param>
        /// <param name="CardMult">the card separation multiplier</param>
        /// <returns>returns a world axis float</returns>
        public static float GridToWorld(int x, float worldOffset, float CardMult)
        {
            return x * CardMult + worldOffset;
        }

        /// <summary>
        /// Convert a grid point into a world point
        /// </summary>
        /// <param name="point">the grid point</param>
        /// <param name="worldOffset">the world offset</param>
        /// <param name="gridSize">the grid size</param>
        /// <returns>retuns a tuple</returns>
        public static Pointf GridToWorld(Point point, Pointf worldOffset, Pointf gridSize)
        {
            var x = GridToWorld(point.x, worldOffset.x, gridSize.x);
            var y = GridToWorld(point.y, worldOffset.y, gridSize.y);

            return new Pointf(x, y);
        }

        /// <summary>
        /// Convert a grid point into a world point using a warGrid
        /// </summary>
        /// <param name="point">the point to convert</param>
        /// <param name="grid">the given grid</param>
        /// <returns>returns a pointf of the world location</returns>
        public static Pointf GridToWorld(Point point, WarGrid grid)
        {
            return GridToWorld(point, grid.Offset, grid.GridScale);
        }

        /// <summary>
        /// Returns a world float into a grid int
        /// </summary>
        /// <param name="x">the world float</param>
        /// <param name="worldOffset">the world offset</param>
        /// <param name="CardMult">the card separation multiplier</param>
        /// <returns>retuns a grid int</returns>
        public static int WorldToGrid(float x, float worldOffset, float CardMult)
        {
            double final = (x / CardMult) - worldOffset;
            int finalInt = (int)Math.Round(final);

            return finalInt;
        }

        /// <summary>
        /// Convert a world point into a grid point
        /// </summary>
        /// <param name="point">the world point</param>
        /// <param name="worldOffset">the world offset</param>
        /// <param name="gridSize">the grid size</param>
        /// <returns>retuns a Point</returns>
        public static Point WorldToGrid(Pointf point, Pointf worldOffset, Pointf gridSize)
        {
            var x = WorldToGrid(point.x, worldOffset.x, gridSize.x);
            var y = WorldToGrid(point.y, worldOffset.y, gridSize.y);

            return new Point(x, y);
        }

        /// <summary>
        /// Convert a world point into a grid point using a given WarGrid
        /// </summary>
        /// <param name="point">the world p</param>
        /// <param name="grid">the given grid</param>
        /// <returns>returns a grid point</returns>
        public static Point WorldToGrid(Pointf point, WarGrid grid)
        {
            if (grid == null)
                throw new NullReferenceException("The war grid cannot be null");

            return WorldToGrid(point, grid.Offset, grid.GridScale);
        }

        /// <summary>
        /// Returns the distance between two points
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <returns>returns a double of the distance squared</returns>
        public static double DistanceSquared(Point a, Point b)
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
        /// Normalize the point
        /// </summary>
        /// <returns>returns the normalized point of the point</returns>
        public Pointf NormalizeToFloatPoint()
        {
            var divisor = Math.Sqrt(Math.Pow((double)x, 2) + Math.Pow((double)y, 2));
            Pointf p = new Pointf((float)(x / divisor), (float)(y / divisor));

            return p;
        }

        /// <summary>
        /// Normalize the point
        /// </summary>
        /// <returns>returns the normalized point into a rounded point form</returns>
        public Point NormalizeToPoint()
        {
            Pointf p = NormalizeToFloatPoint();

            double xSide = Math.Round((double)x);
            double ySide = Math.Round((double)y);

            return new Point((int)xSide, (int)ySide);
        }

        /// <summary>
        /// Attempts to format a string into a point (using Int32) (formats: 'x,y' and '(x,y)')
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="output">the resulting output Point</param>
        /// <returns>returns true if the conversion was succesful, false if not</returns>
        public static bool TryParse32(string input, out Point output)
        {
            if (string.IsNullOrEmpty(input))
            {
                output = Point.zero;
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

                int x;
                int y;

                if (Int32.TryParse(str[0], out x))
                {
                    if (Int32.TryParse(str[1], out y))
                    {
                        output = new Point(x, y);
                        return true;
                    }
                }
            }


            output = Point.zero;
            return false;
        }

        /// <summary>
        /// If the point is out of bounds, it can be corrected by calling this method
        /// </summary>
        /// <param name="p">the point to correct</param>
        /// <returns></returns>
        public static Point GetNearestValidPoint(Point p)
        {
            if (p.x < 0)
            {
                p = new Point(0, p.y);
            }

            if (p.y > 0)
            {
                p = new Point(p.x, 0);
            }

            return p;
        }


        /// <summary>
        /// Get the nearest point out of a <paramref name="selection"/> of points
        /// </summary>
        /// <param name="start">the start point to compare distances from</param>
        /// <param name="selection">the selection of points to compare distances with the <paramref name="start"/> point</param>
        /// <returns>returns the closest point</returns>
        /// <exception cref="ArgumentException">thrown when <paramref name="selection"/> array is empty </exception>
        /// <exception cref="NullReferenceException">thrown when <paramref name="selection"/> array is null</exception>
        public static Point GetNearestPoint(Point start, Point[] selection)
        {
            if (selection == null)
                throw new NullReferenceException("selection cannot be null");

            if (selection.Length == 0)
                throw new ArgumentException("No points to compare distance with");

            Point closest = selection[0];
            double? distance = null;

            for (int i = 0; i < selection.Length; i++)
            {
                double selectedDist = (double)GetDistanceBetweenPoints(start, selection[i]);

                if (distance == null || distance > selectedDist)
                {
                    distance = selectedDist;
                    closest = selection[i];
                }
            }

            return closest;
        }

        /// <summary>
        /// Get the distance between two points
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <returns>returns the distance</returns>
        public static double GetDistanceBetweenPoints(Point a, Point b) =>
            Math.Pow(Math.Pow((double)(a.x - b.x), 2), .5) + Math.Pow(Math.Pow((double)(a.y - b.y), 2), .5);


        public int[] ConvertToIntArray()
        {
            return new int[2] { x, y };
        }

        #region Operators
        public static bool operator ==(Point a, Point b)
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

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }


        public static Point operator *(Point a, int x)
        {
            return new Point(a.x * x, a.y * x);
        }

        public static Point operator *(int x, Point a)
        {
            return a * x;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        public static Point operator -(Point a)
        {
            return new Point(-a.x, -a.y);
        }

        /// <summary>
        /// Sort the points by direction
        /// </summary>
        /// <param name="a">the first point</param>
        /// <param name="b">the second point</param>
        /// <param name="direction">the direction</param>
        /// <returns>returns an integer used in compliance with the 'foo.CompareTo()' standard</returns>
        public static int SortByDirection(Point a, Point b, Point direction)
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

        public bool Equals(Point other)
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

        public int CompareTo(Point other)
        {
            if (other == null)
                return 0;

            int a = x.CompareTo(other.x);
            int b = y.CompareTo(other.y);

            if (b == 0)
            {
                return a;
            }

            return -b;
        }
    }
}