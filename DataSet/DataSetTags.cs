using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WarManager
{
    /// <summary>
    /// Handles the tags for datasets
    /// </summary>
    [Notes.Author("Handles the tags for datasets")]
    public class DataSetTags
    {
        private List<string[]> _tags = new List<string[]>();

        /// <summary>
        /// The tags from the data set tags
        /// </summary>
        /// <value></value>
        public List<string> SerializedTags { get; set; } = new List<string>();

        /// <summary>
        /// Is the list of tags empty?
        /// </summary>
        /// <value></value>
        public bool Empty
        {
            get
            {
                return _tags.Count < 1;
            }
        }

        public IEnumerable<string[]> Tags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// Empty data set tags
        /// </summary>
        public DataSetTags()
        {

        }

        /// <summary>
        /// Add tags to the data set
        /// </summary>
        /// <param name="tags">the given list of tags</param>
        public DataSetTags(List<string> tags)
        {
            _tags = CheckForPipes(tags);
            SerializedTags = tags;
        }


        /// <summary>
        /// Check to see if pipes exist in the tag list
        /// </summary>
        /// <param name="tags">the added tags</param>
        /// <returns>returns the list of string arrays</returns>
        public static List<string[]> CheckForPipes(List<string> tags)
        {

            List<string[]> result = new List<string[]>();

            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].Contains("|"))
                {
                    result.Add(tags[i].Split('|'));
                }
                else
                {
                    result.Add(new string[1] { tags[i] });
                }
            }

            return result;
        }

        /// <summary>
        /// Convert a list of string arrays into a list of strings joined by a pipe
        /// </summary>
        /// <param name="tags">tags - list of string arrays</param>
        /// <returns>returns a list of strings</returns>
        public static List<string> ConvertTagsForSerialization(List<string[]> tags)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].Length > 1)
                {
                    result.Add(string.Join("|", tags[i]));
                }
                else
                {
                    result.Add(tags[i][0]);
                }
            }

            return result;
        }

        /// <summary>
        /// Does a certain Tag exist in the tag list?
        /// </summary>
        /// <param name="tag">the tag to check</param>
        /// <returns>returns true if the tag exists, false if not</returns>
        public bool TagExists(string tag)
        {
            foreach (var tagArray in Tags)
            {
                for (int i = 0; i < tagArray.Length; i++)
                {
                    if (tagArray[i] == tag)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get all the tags
        /// </summary>
        /// <returns>returns the list of tag arrays</returns>
        List<string[]> GetTags()
        {
            List<string[]> x = new List<string[]>();
            x.AddRange(_tags);

            return x;
        }

        public override string ToString()
        {
            string result = "";

            foreach (var tagArray in Tags)
            {
                result += string.Join(",", tagArray);
            }

            return result;
        }
    }
}


