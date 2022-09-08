/* ISelectable.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handles ability to select and deselect objects
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Has the object been selected?
        /// </summary>
        /// <value></value>
        bool Selected { get; }

        /// <summary>
        /// Select the object
        /// </summary>
        /// <returns>returns true if the object was successfully selected, false if not</returns>
        bool Select(bool selectMultple);

        /// <summary>
        /// Deselect the object
        /// </summary>
        /// <returns>returns true if the object was successfully deselected, false if not</returns>
        bool Deselect();

    }
}