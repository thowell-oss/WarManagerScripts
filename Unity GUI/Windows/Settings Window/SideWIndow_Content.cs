/* SideWindowContent.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Helps identify and organize content for the side window
    /// </summary>
    public interface ISideWindowContent
    {
        /// <summary>
        /// The unique GUID for the element
        /// </summary>
        public string ElementID { get; }

        /// <summary>
        /// The type of information the element displays
        /// </summary>
        public SideWindow_Element_Types ElementType { get; }
    }
}
