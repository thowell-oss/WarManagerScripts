using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using WarManager;

namespace StringUtility
{
    public static class StringUtility_v1
    {
        private static string filePath = @"Resources\Howell_StringCleaner_v1";
        public static readonly string GeneralAllowedRegexPattern = @"^[\w\-\(\)\*\.\#\@\!\+\=\/ ]+[^ ]$";


        /// <summary>
        /// Replaces a given string with '*' stars instead
        /// </summary>
        /// <param name="input"></param>
        /// <returns>retunrns a string of stars</returns>
        private static string ReplaceWithProfanityText(int length, char replaceChar)
        {
            string finalString = "";

            for (int i = 0; i < length; i++)
            {
                finalString = finalString + replaceChar;
            }

            return finalString;
        }

        /// <summary>
        /// Replace a profane word with a message in a series of words
        /// </summary>
        /// <param name="input"></param>
        /// <param name="message">The message to replace the profane word</param>
        /// <returns></returns>
        public static string ReplaceProfanityWithMessage(this string input, string message)
        {
            string[] text = input.Split(' ');
            for (int i = 0; i < text.Length; i++)
            {
                if (ContainsProfanity(text[i]))
                {
                    text[i] = message;
                }
            }

            return String.Join(" ", text);
        }

        /// <summary>
        /// Omits a given profane word in a series of words
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string OmitProfanity(this string input)
        {
            string[] text = input.Split(' ');

            for (int i = 0; i < text.Length; i++)
            {
                if (ContainsProfanity(text[i]))
                {
                    text[i] = string.Empty;
                }
            }

            return String.Join(" ", text);
        }

        /// <summary>
        /// Replaces a given word in a series of words with '****'
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replaceChar">the replacement character for the profane word</param>
        /// <returns></returns>
        public static string ReplaceProfanity(this string input, char replaceChar = '*')
        {
            string[] text = input.Split(' ');

            for (int i = 0; i < text.Length; i++)
            {
                if (ContainsProfanity(text[i]))
                {
                    text[i] = ReplaceWithProfanityText(text[i].Length, replaceChar);
                }
            }

            return String.Join(" ", text);
        }

        /// <summary>
        /// Checks the string against a list of obscene words.
        /// Thou shalt not take thy Lord's name in vain...
        /// </summary>
        /// <param name="input"></param>
        /// <returns>returns true if the text is profaine, false if not</returns>
        public static bool ContainsProfanity(this string input)
        {
            if (input == null)
                throw new NullReferenceException("The input is null");

            input.Trim();

            if (input == string.Empty)
                return false;

            input = input.ToLower();

            try
            {
                using (StreamReader reader = new StreamReader(filePath + @"\profanity.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        if (input == reader.ReadLine())
                        {
                            return true;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError("Profanity issue: " + ex.Message);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the string is a general allowed string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="backupOutput">the backup string in case the input is invalid</param>
        /// <returns>returns the resulting string (either the original, or the backup)</returns>
        public static string CheckStringMatch(this string input, string backupOutput)
        {
            input = input.Trim();

            if (Regex.IsMatch(input, GeneralAllowedRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return input;
            }
            else
            {
                return backupOutput;
            }
        }


        /// <summary>
        /// Changes the string from "foo" to "'foo'"
        /// </summary>
        /// <param name="input">the input string</param>
        /// <returns>the output</returns>
        public static string SetStringQuotes(this string input)
        {
            return "\'" + input + "\'";
        }

        /// <summary>
        /// Changes the string from "foo" to "foos" depending on the quantity of the objects being counted
        /// </summary>
        /// <param name="input"></param>
        /// <param name="amt">the integer amount being counted</param>
        /// <returns></returns>
        public static string ConvertQty(this string input, int amt)
        {
            if (amt == 1)
            {
                return input;
            }
            else
            {
                return input + "s";
            }
        }

        /// <summary>
        /// Checks to see if the name is not null, not empty, not profane, trimmed, unique (if the words are given), and does not contain any other abnormal characters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="compareStrings">use the words to make sure the first word is unique</param>
        /// <returns>returns true if the string is valid</returns>
        public static string ValidateWord(this string input, string[] compareStrings = null)
        {
            if (!isValid(input))
            {
                return GetNewUniqueString("New Obj", 4);
            }

            if (compareStrings != null && compareStrings.Length > 0)
            {
                return GenerateIncrementList(input, compareStrings);
            }

            if (GeneralSettings.UseProfanityFilter)
            {
                return ReplaceProfanity(input);
            }

            if (Regex.IsMatch(input, GeneralAllowedRegexPattern))
            {
                return input;
            }
            else
            {
                return GetNewUniqueString("New Obj", 4);
            }
        }

        /// <summary>
        /// Creates a string that's unique by icrement (Ex. obj (1), obj (2), obj (3))
        /// </summary>
        /// <param name="string"></param>
        /// <param name="list">the list of strings to check against</param>
        /// <returns>returns an updated string that should look like 'New {obj} (2)'</returns>
        public static string GenerateIncrementList(this string input, string[] list)
        {
            int longest = 0;
            bool foundSimilar = false;

            for (int i = 0; i < list.Length; i++)
            {
                var match = Regex.Match(list[i], @" \([0-9]+\)$");

                if (match.Value != string.Empty && match.Value.Length < list[i].Length)
                {
                    var resultStr = list[i].Remove(list[i].Length - match.Value.Length);

                    input = input.Trim();
                    resultStr = resultStr.Trim();

                    if (input == resultStr)
                    {
                        foundSimilar = true;

                        var newMatch = Regex.Match(match.Value, @"[0-9]+").Value;

                        int nextValue = 0;
                        Int32.TryParse(newMatch, out nextValue);

                        if (nextValue > longest)
                        {
                            longest = nextValue;
                        }
                    }
                }

                if (list[i] == input)
                {
                    foundSimilar = true;
                }
            }

            if (foundSimilar)
            {
                return input + " (" + (longest + 1) + ")";
            }
            else
            {
                return input;
            }
        }

        /// <summary>
		/// Is the string not null and empty?
		/// </summary>
		/// <param name="input">the string in question</param>
		/// <returns>returns true if the string not null and empty</returns>
		public static bool isValid(this string input)
        {
            if (input == null)
                return false;

            input.Trim();

            if (input == string.Empty)
                return false;

            return true;
        }

        public static string CleanString_Color_Hex(this string input)
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

        public static bool IsEmailString(this string value)
        {
            Regex r = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            value = value.Trim();

            if (r.Match(value).Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        [Notes.Author("https://social.technet.microsoft.com/wiki/contents/articles/26805.c-calculating-percentage-similarity-of-2-strings.aspx", 1.0, "Levenshtien distance")]
        public static int ComputeLevenshteinDistance(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        [Notes.Author("https://social.technet.microsoft.com/wiki/contents/articles/26805.c-calculating-percentage-similarity-of-2-strings.aspx", 1.0, "Calculate Similarity of a group of words")]
        public static double CalculateSimilarity(this string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        /// <summary>
        /// Trim the array
        /// </summary>
        /// <param name="source">the original array</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static T[] TrimArray<T>(this T[] source, int start, int end)
        {

            if (end <= 0 || start >= end)
                return source;

            int i = start;

            T[] result = new T[end - start];

            while (i < end && i < source.Length)
            {
                result[i - start] = source[i];

                i++;
            }

            return result;
        }

    }
}
