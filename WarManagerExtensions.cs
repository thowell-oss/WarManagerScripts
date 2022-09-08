
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    public static class WarManagerExtensions
    {

        /// <summary>
        /// Convert a double array to a float point
        /// </summary>
        /// <param name="d">the double</param>
        /// <returns></returns>
        public static Pointf ConvertToPointf(this double[] d)
        {

            if (d == null)
                throw new NullReferenceException("The double array is null");

            if (d.Length != 2)
            {
                throw new NotSupportedException("An array with a length greater or less than 2 is not supported");
            }

            return new Pointf((float)d[0], (float)d[1]);
        }

        public static Vector2 ConvertToUnityVector2(this double[] d)
        {
            if (d == null)
                throw new NullReferenceException("The double array is null");

            if (d.Length != 2)
            {
                throw new NotSupportedException("An array with a length greater or less than 2 is not supported");
            }

            return new Vector2((float)d[0], (float)d[1]);
        }

        /// <summary>
        /// Convert a int array to Point
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Point ConvertToPoint(this int[] i)
        {
            if (i == null)
            {
                throw new NullReferenceException("The integar array is null");
            }

            if (i.Length != 2)
            {
                throw new NotSupportedException("An array with a length greater or less than 2 is not supported");
            }

            return new Point(i[0], i[1]);
        }


        /// <summary>
        /// Convert a vector2 to a point
        /// </summary>
        /// <param name="v">the given vector 2</param>
        /// <returns>returns a point</returns>
        public static Point ConvertToPoint(this Vector2 v)
        {
            if (v == null)
                throw new NullReferenceException("The vector 2 is null");

            float x = v.x;
            float y = v.y;

            int xInt = Mathf.RoundToInt(x);
            int yInt = Mathf.RoundToInt(y);

            return new Point(xInt, yInt);
        }

        /// <summary>
        /// Convert a vector 2 int into a war manager point
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Point ConvertToPoint(this Vector2Int v)
        {
            if (v == null)
                throw new NullReferenceException("The vector 2 is null");

            return new Point(v.x, v.y);
        }


        /// <summary>
        /// Convert unity3d vector into a double
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] ConvertToDouble(this Vector2 v)
        {
            if (v == null)
                throw new NullReferenceException("The vector 2 is null");

            return new double[2] { v.x, v.y };
        }

        /// <summary>
        /// Convert a pointf into a unity3d vector 2
        /// </summary>
        /// <param name="p">the float point (pointf)</param>
        /// <returns></returns>
        public static Vector2 ConvertToVector2(Pointf p)
        {
            return new Vector2(p.x, p.y);
        }
    }
}