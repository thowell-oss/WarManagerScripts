using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Calculates the necessary curves between as many points as needed in a bezier fashion
    /// </summary>
    [Notes.Author("Calculates the necessary curves between as many points as needed in a bezier fashion")]
    public static class BezierCurveCalculation
    {

        /// <summary>
        /// Get a list of locations using a set of points
        /// </summary>
        /// <param name="points">the points</param>
        /// <param name="density">the density of the points returned</param>
        /// <returns>returns a list of locations to use</returns>
        public static List<Vector3> GetBezierPoints(List<Transform> points, int density)
        {
            List<Vector3> locations = new List<Vector3>();

            if (points == null || points.Count < 1)
                return new List<Vector3>();

            if (points.Count == 1)
                return new List<Vector3>()
        {
            points[0].position
        };


            for (float x = 0; x < density; x++)
            {
                locations.Add(CalculateBezierLocation(x / density, points));
            }

            return locations;
        }

        /// <summary>
        /// Calculate the bezier location based off of a list of transform points
        /// </summary>
        /// <param name="t">the interpolation</param>
        /// <param name="points">the list of points</param>
        /// <returns>returns a vector3 location</returns>
        public static Vector3 CalculateBezierLocation(float t, List<Transform> points)
        {
            if (points == null || points.Count < 1)
                return Vector3.zero;

            if (points.Count == 1) return points[0].position;


            List<Vector3> locations = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                locations.Add(points[i].position);
            }

            return CalculateBezierLocation(t, locations);
        }

        /// <summary>
        /// Calculate the bezier location based off of a list of vector3 points
        /// </summary>
        /// <param name="t">the interpolation</param>
        /// <param name="locations">the locations of all points</param>
        /// <returns>returns a vector3</returns>
        public static Vector3 CalculateBezierLocation(float t, List<Vector3> locations)
        {
            if (locations == null || locations.Count < 1)
                return Vector3.zero;

            if (locations.Count == 1) return locations[0];

            Vector3 sum = Vector3.zero;

            for (int i = 0; i < locations.Count; i++)
            {
                sum += CalculateSumOfOne(t, i, locations);
            }

            return sum;
        }


        /// <summary>
        /// Calculate the sum of one point
        /// </summary>
        /// <param name="n">the total</param>
        /// <param name="t">the interpolation</param>
        /// <param name="i">the current increment</param>
        /// <returns></returns>
        private static Vector3 CalculateSumOfOne(float t, int i, List<Vector3> locations)
        {
            float n = locations.Count - 1;

            float a = Mathf.Pow(1 - t, n - i);

            float b = Mathf.Pow(t, i);

            Vector3 location = locations[i];

            return a * b * location;
        }

    }
}


