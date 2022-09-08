
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;

namespace WarManager.Testing.Unit
{

    public class RectUnitTests
    {

        /// <summary>
        /// double check error handling
        /// </summary>
        [Test]
        public void RectHandlesWithOneLocation()
        {
            Rect rect = new Rect(new Point(5, -5));

            Assert.True(rect.SpacesTaken().Count == 1);
        }

        /// <summary>
        /// double checking that it holds for a rectangle
        /// </summary>
        [Test]
        public void RectHandlesWithMultipleLocations()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));
            Assert.True(rect.SpacesTaken().Count == 9);
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesLeftPerimeterCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));
            Assert.True(rect.GetLeftPerimeterSpacesTaken().Count == 3);

            Rect rect1 = new Rect(new Point(5, -5), new Point(7, -55));
            Assert.AreEqual(51, rect1.GetLeftPerimeterSpacesTaken().Count);
        }

        /// <summary>
        /// rect top perimeter is incrementing from 5 up to 7 on the x axis
        /// </summary>
        [Test]
        public void RectLeftPerimeterIncrementingCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));

            int x = 2;

            foreach (var pnt in rect.GetLeftPerimeterSpacesTaken())
            {
                Assert.AreEqual(5, pnt.x);
                Assert.AreEqual(-5 - x, pnt.y);

                x--;
            }
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesRightPerimeterCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));
            Assert.True(rect.GetRightPerimeterSpacesTaken().Count == 3);

            Rect rect1 = new Rect(new Point(5, -5), new Point(75, -55));
            Assert.AreEqual(51, rect1.GetRightPerimeterSpacesTaken().Count);
        }

        /// <summary>
        /// rect top perimeter is incrementing from 5 up to 7 on the x axis
        /// </summary>
        [Test]
        public void RectRightPerimeterIncrementingCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));

            int x = 0;

            foreach (var pnt in rect.GetRightPerimeterSpacesTaken())
            {
                Assert.AreEqual(7, pnt.x);
                Assert.AreEqual(-5 - x, pnt.y);

                x++;
            }
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesBottomPerimeterCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));
            Assert.True(rect.GetBottomPerimeterSpacesTaken().Count == 3);

            Rect rect1 = new Rect(new Point(5, -5), new Point(75, -55));
            Assert.AreEqual(71, rect1.GetBottomPerimeterSpacesTaken().Count);
        }

        /// <summary>
        /// rect bottom perimeter is incrementing from 5 up to 7 on the x axis
        /// </summary>
        [Test]
        public void RectTopPerimeterIncrementingCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));

            int x = 0;

            //foreach (var pnt in rect.GetTopPerimeterSpacesTaken())
            //{
            //    Assert.AreEqual(5 + x, pnt.x);
            //    Assert.AreEqual(-5, pnt.y);

            //    x++;
            //}
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesTopPerimeterCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));
            Assert.True(rect.GetTopPerimeterSpacesTaken().Count == 3);

            int x = 0;

            //foreach(var pnt in rect.GetTopPerimeterSpacesTaken())
            //{
            //    Assert.AreEqual(5 + x, pnt.x);
            //    Assert.AreEqual(-5, pnt.y);

            //    x++;
            //}

            Rect rect1 = new Rect(new Point(5, -5), new Point(75, -55));
            Assert.AreEqual(71, rect1.GetTopPerimeterSpacesTaken().Count);
        }

        /// <summary>
        /// rect top perimeter is incrementing from 5 up to 7 on the x axis
        /// </summary>
        [Test]
        public void RectBottomPerimeterIncrementingCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));

            int x = 0;

            //foreach (var pnt in rect.GetTopPerimeterSpacesTaken())
            //{
            //    Assert.AreEqual(5 + x, pnt.x);
            //    Assert.AreEqual(-5, pnt.y);

            //    x++;
            //}
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesSmallPerimeterCorrectly()
        {
            Rect rect = new Rect(new Point(5, -5), new Point(7, -7));

            //foreach (var x in rect.GetPerimeter())
            //{
            //    UnityEngine.Debug.Log(x);
            //}

            Assert.AreEqual(12 - 4, rect.GetPerimeter().Count);
        }

        /// <summary>
        /// check the left perimeter to work correctly
        /// </summary>
        [Test]
        public void RectHandlesLargePerimeterCorrectly()
        {

            Rect rect1 = new Rect(new Point(5, -5), new Point(55, -75));

            // foreach (var x in rect1.GetPerimeter())
            // {
            //     UnityEngine.Debug.Log(x);
            // }

            Assert.AreEqual(2 * (55 - 5) + 2 * (-5 + 75), rect1.GetPerimeter().Count);
        }

        
        /// <summary>
        /// the update rect method should update the rect based on the top left node (while keeping the width and height intact)
        /// </summary>
        [Test]
        public void UpdateRectActuallyWorks()
        {
            Rect r = new Rect(new Point(5, -5), new Point(7, -7));

            r.UpdateRect(new Point(10, -10), 2, 2);

            Assert.True(r.BottomRightCorner == new Point(12, -12));
        }
    }
}
