
/* IFileContentInfo.cs
 * Author: Taylor Howell
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handles the getting and setting of information when saving and opening content
    /// </summary>
    public interface IFileContentInfo
    {
        /// <summary>
        /// Convert all content in the object to a string builder for storage
        /// </summary>
        /// <returns>returns a stringbuilder to use to save the file</returns>
        public string[] GetContent();

        /// <summary>
        /// Convert the stringbuilder into content to see
        /// </summary>
        /// <param name="str">the stringbuilder</param>
        /// <returns>returns true if the content was successfully created, false if not</returns>
        public bool SetContent(string[] args);

    }
}
