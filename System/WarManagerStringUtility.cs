/* WarManagerStringUtility.cs
 * Author: Taylor Howell
 */

using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handle edge case strings
    /// </summary>
    public static class WarManagerStringUtility
    {
        /// <summary>
        /// Is the string not null and empty?
        /// </summary>
        /// <param name="input">the string in question</param>
        /// <returns>returns true if the string not null and empty</returns>
        public static bool isValid(string input)
        {
            if (input == null)
                return false;

            input.Trim();

            if (input == string.Empty)
                return false;

            return true;
        }

        /// <summary>
        /// Checks to see if the user gave an appropriate name, if not a name will be given
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="context">the type of object that the string is coming from</param>
        /// <param name="idLength">the length needed of the unique id</param>
        /// <returns>returns an appropriate name</returns>
        public static string CleanString_Name(string input, string context, int idLength)
        {
            if (!isValid(input))
            {
                return GetNewUniqueString(context, idLength);
            }

            if (Regex.IsMatch(input, @"^[\w\-\(\)\*\.\#\@\!\+\=\/ ]+[^ ]$") && !ContainsProfanity(input))
            {
                return input;
            }
            else
            {
                return GetNewUniqueString(context, idLength);
            }
        }

        /// <summary>
        /// is the string appropriate for naming?
        /// </summary>
        /// <param name="input">the input string in question</param>
        /// <returns>returns true if the string is valid, false if not</returns>
        public static bool IsCleanString_Name(string input)
        {
            string str = CleanString_Name(input, " ", 1);
            if (str != input)
                return false;

            return true;
        }

        /// <summary>
        /// Does this string contain profanity?
        /// </summary>
        /// <param name="input">the input string in question</param>
        /// <returns>returns true if the string does contain profanity</returns>
        public static bool ContainsProfanity(string input)
        {
            return false;
        }

        public static string CleanString_Color_Hex(string input)
        {
            if (!isValid(input))
            {
                return "#FFFF00";
            }

            input.Trim();

            if (Regex.IsMatch(input, @"^#([a-fA-F0-9]{6})$"))
            {
                return input;
            }

            return "#FFFF00";
        }

        /// <summary>
        /// Creates a new unique string
        /// </summary>
        /// <param name="objectType">the type of object being validated</param>
        /// <param name="idLength">the length of the new clean string id</param>
        /// <returns>returns a new string</returns>
        public static string GetNewUniqueString(string objectType, int idLength)
        {
            if (idLength < 1)
                idLength = 1;

            if (idLength > 15)
                idLength = 15;

            string id = Guid.NewGuid().ToString().Substring(0, idLength);

            if (objectType != null)
                return "New " + objectType + " " + id;
            else
                return "New " + id;
        }
    }
}
