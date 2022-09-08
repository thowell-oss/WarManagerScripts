using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager
{
    [Notes.Author("Handles the dictionary of drop points for each active sheet")]
    public static class SheetDropPointManager
    {

        /// <summary>
        /// The dictionary of drop points cached for the session
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Point"></typeparam>
        /// <returns></returns>
        private static Dictionary<string, Point> DropPoints = new Dictionary<string, Point>();

        /// <summary>
        /// the dictionary of custom drop points on a given sheet
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, List<Point>> _customDropPoints = new Dictionary<string, List<Point>>();

        public delegate void delegate_dropPointChange(string sheetId, Point p);
        public static event delegate_dropPointChange OnDropPointChangeLocation;

        public delegate void delegate_customDropPointsChange(List<Point> p);
        public static delegate_customDropPointsChange OnCustomDropPointsChange;

        /// <summary>
        /// Get the drop point for the currrent active sheet
        /// </summary>
        /// <returns></returns>
        public static Point GetDropPoint()
        {
            return GetDropPoint(SheetsManager.CurrentSheetID);
        }

        /// <summary>
        /// Get the points from a specific sheet
        /// </summary>
        /// <param name="sheetId">the id of the sheet</param>
        /// <returns>returns the list of points</returns>
        private static List<Point> GetPoints(string sheetId)
        {
            if (_customDropPoints.ContainsKey(sheetId))
            {
                return _customDropPoints[sheetId];
            }

            List<Point> points = new List<Point>();
            _customDropPoints.Add(sheetId, points);
            return points;
        }

        /// <summary>
        /// Set a list of custom points
        /// </summary>
        /// <param name="sheetId">the sheet id</param>
        /// <param name="points">the points</param>
        public static void SetNewCustomPoints(string sheetId, IList<Point> points)
        {
            if (sheetId == null)
                throw new NullReferenceException("the sheet id is null");

            if (sheetId.Trim().Length < 1)
                throw new ArgumentException("the sheet id is an empty string");

            List<Point> thePoints = GetPoints(sheetId);
            thePoints.Clear();
            thePoints.AddRange(points);

            OnCustomDropPointsChange?.Invoke(thePoints);
        }

        /// <summary>
        /// Get the drop point from a specific sheet
        /// </summary>
        /// <param name="sheetId">the drop point of a specific sheet</param>
        /// <returns>returns the point if successful otherwise it returns (0,0)</returns>
        public static Point GetDropPoint(string sheetId)
        {
            if (sheetId == null)
            {
                return Point.zero;
            }

            if (DropPoints.ContainsKey(sheetId))
            {
                return DropPoints[sheetId];
            }

            var sheet = SheetsManager.GetSheetMetaData(sheetId);

            if (sheet == null)
                throw new NullReferenceException("The sheet is null");

            var loc = sheet.LastDropPointLocation;
            Point point = new Point(loc[0], loc[1]);

            DropPoints.Add(sheetId, point);

            return point;
        }


        /// <summary>
        /// Set the drop point of the current active sheet
        /// </summary>
        /// <param name="newPoint">the new drop point</param>
        public static void SetDropPoint(Point newPoint)
        {
            if (SheetsManager.CurrentSheetID != null)
            {
                SetDropPoint(newPoint, SheetsManager.CurrentSheetID);

            }
        }

        /// <summary>
        /// Set the drop point of a specific sheet
        /// </summary>
        /// <param name="newPoint">the new drop point</param>
        /// <param name="sheetId">the id of the sheet</param>
        public static void SetDropPoint(Point newPoint, string sheetId)
        {

            if (string.IsNullOrEmpty(sheetId))
                throw new System.NullReferenceException("The sheet id is null or empty");

            if (DropPoints.ContainsKey(sheetId))
            {
                DropPoints[sheetId] = newPoint;
            }
            else
            {
                GetDropPoint(sheetId);
                DropPoints[sheetId] = newPoint;
            }

            if (OnDropPointChangeLocation != null)
                OnDropPointChangeLocation(sheetId, newPoint);

        }

    }
}
