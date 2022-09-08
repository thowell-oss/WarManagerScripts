/* GridPartition.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

namespace WarManager
{
    [Serializable]
    /// <summary>
    /// Handles a section of the sheet
    /// </summary>
    [Notes.Author("Handles quick location based access to the objects when searching by location")]
    public class GridPartition<Tobj> where Tobj : ICompareWarManagerPoint
    {
        /// <summary>
        /// The rect bounds of the grid partition
        /// </summary>
        public Rect GridBounds { get; private set; }

        /// <summary>
        /// the width/height of the bound
        /// </summary>
        private readonly int boundAmt = 10;

        /// <summary>
        /// The location of all cards in the partition
        /// </summary>
        public Tobj[][] Objects { get; private set; }

        /// <summary>
        /// Is this grid partion just a place holder partition
        /// </summary>
        /// <value>returns true if the grid partiton has not been set up properly yet, false if otherwise</value>
        public bool placeHolder { get; private set; } = false;

        /// <summary>
        /// turns the grid partition into a placeholder
        /// </summary>
        public GridPartition()
        {
            placeHolder = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="upperLeftBound">the upper left bound grid point</param>
        public GridPartition(Point upperLeftBound)
        {
            Point UpperLeftBound = upperLeftBound;
            Point LowerRightBound = new Point(upperLeftBound.x + boundAmt, upperLeftBound.y - boundAmt);

            GridBounds = new Rect(UpperLeftBound, LowerRightBound);

            Objects = new Tobj[boundAmt][];

            for (int i = 0; i < Objects.Length; i++)
            {
                Objects[i] = new Tobj[boundAmt];
            }

        }

        /// <summary>
        /// Add an object to the grid partition
        /// </summary>
        /// <param name="value">the value to add</param>
        /// <returns>returns true if the card can be added to the grid, false if not</returns>
        public bool Add(Tobj value)
        {
            if (!GridBounds.IsInBounds(value.point))
                throw new NotSupportedException("Cannot add a card outside the grid partition bounds " + value.point + GridBounds.TopLeftCorner);

            Point p = new Point(0, 0);
            p = Point.GridToLocal(value.point);

            if (Objects[p.x][p.y] == null)
            {
                Objects[p.x][p.y] = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Does a (non-null) object exist at a certain point?
        /// </summary>
        /// <param name="gridPoint">the grid point</param>
        /// <returns>returns true if the card exists, false if not</returns>
        public bool Exist(Point gridPoint)
        {
            Point p = Point.GridToLocal(gridPoint);

            if (Objects[p.x][p.y] != null)
                return true;

            return false;
        }

        /// <summary>
        /// Remove an object from the grid partition
        /// </summary>
        /// <param name="gridPoint">the location of the object point</param>
        public void Remove(Point gridPoint)
        {
            if (!GridBounds.IsInBounds(gridPoint))
                throw new NotSupportedException("Cannot manage a card outside the grid partition bounds");

            Point p = Point.GridToLocal(gridPoint);
            Objects[p.x][p.y] = default(Tobj);
        }

        /// <summary>
        /// Get an object from the grid partition
        /// </summary>
        /// <param name="gridPoint">the grid point</param>
        /// <returns>returns a card from the grid, might also be null</returns>
        public Tobj GetObj(Point gridPoint)
        {
            if (!GridBounds.IsInBounds(gridPoint))
                throw new NotSupportedException("Cannot manage a card outside the grid partition bounds");

            Point p = Point.GridToLocal(gridPoint);
            return Objects[p.x][p.y];
        }

        /// <summary>
        /// Get a row of Type at a grid point
        /// </summary>
        /// <param name="gridPoint">the grid location of the row</param>
        /// <returns>returns an array of Type T</returns>
        public Tobj[] GetRow(Point gridPoint)
        {
            if (!GridBounds.IsInBounds(gridPoint))
                throw new NotSupportedException("Cannot manage a card outside the grid partition bounds");

            Point p = Point.GridToLocal(gridPoint);

            Tobj[] cards = new Tobj[boundAmt];

            for (int i = 0; i < boundAmt; i++)
            {
                cards[i] = Objects[i][p.y];
            }

            return cards;
        }

        /// <summary>
        /// Get a column of Type at a grid point
        /// </summary>
        /// <param name="gridPoint">the grid location of the column</param>
        /// <returns>returns an array of Type T</returns>
        public Tobj[] GetColumn(Point gridPoint)
        {
            if (!GridBounds.IsInBounds(gridPoint))
                throw new NotSupportedException("Cannot add a card outside the grid partition bounds");

            Point p = Point.GridToLocal(gridPoint);

            Tobj[] cards = new Tobj[boundAmt];

            for (int i = 0; i < boundAmt; i++)
            {
                cards[i] = Objects[p.x][i];
            }

            return cards;
        }

        /// <summary>
        /// Get a copy of all the objects in the grid location
        /// </summary>
        /// <returns></returns>
        public Tobj[] GetAllObjects()
        {
            List<Tobj> objs = new List<Tobj>();

            for (int i = 0; i < boundAmt; i++)
            {
                for (int j = 0; j < boundAmt; j++)
                {
                    if (Objects[i][j] != null)
                    {
                        objs.Add(Objects[i][j]);
                    }
                }
            }

            return objs.ToArray();
        }

        //get perimeter

        //get crossPositive

        //get crossNegative

        //grid partition state (if it is LOD 0, LOD 1, off)

        //calculate DistanceFromCamera
    }
}
