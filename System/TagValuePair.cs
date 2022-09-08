
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    public class TagValuePair<T, TValue> : IComparable<TagValuePair<T, TValue>> 
    {
        /// <summary>
        /// The id of the tag (col id?)
        /// </summary>
        /// <value></value>
        public T TagID { get; private set; }

        /// <summary>
        /// The Tag name of the tag value pair (immutable)
        /// </summary>
        /// <value></value>
        public string Tag { get; private set; }

        /// <summary>
        /// The value of the tag value pair
        /// </summary>
        /// <value></value>
        public TValue Value { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">the col id of the tag</param>
        /// <param name="tag">the tag name</param>
        /// <param name="value">the curernt value of the tag</param>
        public TagValuePair(T id, string tag, TValue value)
        {
            if (id == null)
                throw new NullReferenceException("The ids cannot be null");

            if (tag == null)
                throw new NullReferenceException("the tags cannot be null");

            if (value == null)
                throw new NullReferenceException("the values cannot be null");

            TagID = id;
            Tag = tag;
            Value = value;
        }

        public (string, TValue) Get()
        {
            return (Tag, Value);
        }

        public override string ToString()
        {
            return $"({TagID}){Tag}={Value}";
        }

        public static List<TagValuePair<T, TValue>> CreateTags(IList<T> ids, IList<string> tags, IList<TValue> values)
        {
            if (ids == null)
                throw new NullReferenceException("The list of ids cannot be null");

            if (tags == null)
                throw new NullReferenceException("The tags cannot be null");

            if (values == null)
                throw new NullReferenceException("The values cannot be null");

            if (ids.Count != tags.Count && ids.Count != values.Count)
                throw new System.NotSupportedException("The length of ids, tags and values must be the same");

            List<TagValuePair<T, TValue>> tagValuePairs = new List<TagValuePair<T, TValue>>();

            for (int i = 0; i < ids.Count; i++)
            {
                tagValuePairs.Add(new TagValuePair<T, TValue>(ids[i], tags[i], values[i]));
            }

            return tagValuePairs;
        }


        /// <summary>
        /// Check to see if the tag contains a string or the value contains a given value
        /// </summary>
        /// <param name="str">the string</param>
        /// <param name="value">the value</param>
        /// <returns>returns true if one of these is true, false if both are not true </returns>
        public bool Contains(string str, TValue value)
        {
            return TagContains(str) || ValueContains(value);
        }

        /// <summary>
        /// Returns true if the tag contains the given string
        /// </summary>
        /// <param name="str">the given string</param>
        /// <returns>returns true/false</returns>
        public bool TagContains(string str)
        {
            return Tag.Contains(str);
        }

        /// <summary>
        /// Returns true if the value is equal to the tValue
        /// </summary>
        /// <param name="value">the given value to compare</param>
        /// <returns>returns true/false</returns>
        public bool ValueContains(TValue value)
        {
            if(value.Equals(Value))
                return true;

            return false;
        }

        #region EqualityComparing

        public override bool Equals(object obj)
        {
            return obj is TagValuePair<T, TValue> pair &&
                   EqualityComparer<T>.Default.Equals(TagID, pair.TagID) &&
                   Tag == pair.Tag &&
                   EqualityComparer<TValue>.Default.Equals(Value, pair.Value);
        }

        public override int GetHashCode()
        {
            int hashCode = 1738724339;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(TagID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Tag);
            hashCode = hashCode * -1521134295 + EqualityComparer<TValue>.Default.GetHashCode(Value);
            return hashCode;
        }

        public int CompareTo(TagValuePair<T, TValue> other)
        {
            return Tag.CompareTo(other);
        }

        public static bool operator ==(TagValuePair<T, TValue> a, TagValuePair<T, TValue> b)
        {
            if ((object)a == null && (object)b == null)
                return true;

            if ((object)a == null)
                return false;

            if ((object)a == null)
                return false;

            return a.TagID.Equals(b.TagID) && a.Tag.Equals(b.Tag);
        }


        public static bool operator !=(TagValuePair<T, TValue> a, TagValuePair<T, TValue> b)
        {
            return !(a == b);
        }

        #endregion
    }
}
