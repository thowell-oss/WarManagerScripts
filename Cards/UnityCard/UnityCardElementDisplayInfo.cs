/* UnityCardElementsDisplayInfo.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend.CardsElementData;

namespace WarManager
{
    /// <summary>
    /// Handles to transport of information from card makeup to card elements manager for a specific card element
    /// </summary>
    public class UnityCardElementDisplayInfo : IComparable<UnityCardElementDisplayInfo>
    {

        /// <summary>
        /// The type of element to display
        /// </summary>
        /// <value></value>
        public string ElementType { get; private set; }

        /// <summary>
        /// The payload of description infomation (and other properties)
        /// </summary>
        /// <value></value>
        public CardElementData PropertiesData;

        /// <summary>
        /// The direct information to display
        /// </summary>
        /// <value></value>
        public string[] Display { get; set; } = new string[0];


        /// <summary>
        /// Constructor: Handles the initialization of the card element display info
        /// </summary>
        /// <param name="elementType">the type of element being displayed</param>
        /// <param name="payload">the payload description data</param>
        /// <param name="display">the information to display</param>
        public UnityCardElementDisplayInfo(string elementType, CardElementData propertiesData, string[] display)
        {
            ElementType = elementType;
            PropertiesData = propertiesData;
            Display = display;
        }

        public override string ToString()
        {
            string displayInfo = string.Join(" ", Display);

            return $"\'{ElementType}\': {displayInfo}";
        }

        public int CompareTo(UnityCardElementDisplayInfo other)
        {
            int comp = this.ElementType.CompareTo(other);

            if(comp != 0)
            {
                return comp;
            }
            else
            {
                return string.Join("", Display).CompareTo(String.Join("", other.Display));
            }
        }
    }
}
