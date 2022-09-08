/* SnapShot.cs
 * Author: Taylor Howell
 */

using System.Collections.Generic;
using System;

using WarManager.Cards;

namespace WarManager
{
    /// <summary>
    /// A snapshot of what the war manager board looks like - used for undo and redo
    /// </summary>
    public class SnapShot
    {

        /// <summary>
        /// A reference to the card
        /// </summary>
        public Card CardReference;

        /// <summary>
        /// Was the card hidden?
        /// </summary>
        public bool Locked;

        /// <summary>
        /// Was the card locked?
        /// </summary>
        public bool Hidden;

        /// <summary>
        /// Was the card selected?
        /// </summary>
        public bool Selected;

        /// <summary>
        /// Was the card added?
        /// </summary>
        public bool Added;

        /// <summary>
        /// Was the card removed?
        /// </summary>
        public bool Removed;

        /// <summary>
        /// The ending point where the card was located after the move
        /// </summary>
        public Point CurrentPoint;

		/// <summary>
		/// The ending layer
		/// </summary>
        public Layer CurrentLayer;
		/// <summary>
		/// The ending sheet
		/// </summary>
        public string CurrentSheet;

    }
}
