
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handles managing the grid system per sheet
    /// </summary>
    public class WarGrid
    {
        /// <summary>
        /// The scale of the grid
        /// </summary>
        /// <value></value>
        public Pointf GridScale { get; private set; } = new Pointf(5, 5);

        /// <summary>
        /// Grid offset
        /// </summary>
        /// <value></value>
        public Pointf Offset { get; private set; } = new Pointf(0, 0);

        /// <summary>
        /// The smallest grid scale that is possible
        /// </summary>
        /// <returns></returns>
        public static Pointf MinumumGridScale { get; private set; } = new Pointf(.25f, .25f);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="offset">offset array</param>
        /// <param name="gridScale">grid scale array</param>
        public WarGrid(double[] offset, double[] gridScale)
        {
            if (offset == null)
                throw new NullReferenceException("the offset array is null, this is not allowed");

            if (offset.Length < 2)
                throw new NotSupportedException("a offset array with a size length than 2 is not supported");

            if (gridScale == null)
                throw new NullReferenceException("the grid scale array is null, this is not allowed");

            if (gridScale.Length < 2)
                throw new NotSupportedException("a grid scale array with a size length than 2 is not supported");


            Pointf scale = new Pointf((float)gridScale[0], (float)gridScale[1]);

            if (scale.x >= MinumumGridScale.x && scale.y >= MinumumGridScale.y)
            {
                GridScale = scale;
            }
            else
            {
                GridScale = MinumumGridScale;
            }


            Offset = new Pointf((float)offset[0], (float)offset[1]);
        }

        public WarGrid(Pointf offset, Pointf gridScale)
        {
            Offset = offset;

            if (gridScale.x >= MinumumGridScale.x && gridScale.y >= MinumumGridScale.y)
            {
                GridScale = gridScale;
            }
            else
            {
                GridScale = MinumumGridScale;
            }
        }
    }
}
