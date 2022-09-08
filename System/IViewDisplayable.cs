/* IViewDisplayable.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handles the contract for common displayable objects in the war manager
    /// </summary>
    /// <typeparam name="Tobj">the object the displayable is showing</typeparam>
    public interface IViewDisplayable<Tobj> where Tobj : ICompareWarManagerPoint
    {
        /// <summary>
        /// The display id
        /// </summary>
        /// <value></value>
        string ID { get; }

        /// <summary>
        /// The value object being displayed
        /// </summary>
        /// <value></value>
        Tobj Card { get; }


        /// <summary>
        /// Initialize the IViewDisplayable
        /// </summary>
        /// <param name="value">the object being referenced for initalization</param>
        void ResetCard(Tobj value, string sheetID);


        /// <summary>
        /// Updates the display to reflect the latest data
        /// </summary>
        /// <param name="LOD">tells the level of detail to update (how beautifully should the card update?)</param>
        void UpdateDisplay(int LOD);

    }
}
