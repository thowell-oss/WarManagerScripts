/* CardElementData.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using StringUtility;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// Handles creation and transport of card element data
    /// </summary>
    public class CardElementData : IUnityCardElement
    {
        /// <summary>
        /// The ID of the card element (and its row reference)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("col")]
        public int ID { get; private set; } = -1;

        /// <summary>
        /// Any other secondary column locations to reference
        /// </summary>
        [JsonPropertyName("*other cols")]
        public int[] otherIDs { get; private set; } = new int[1] { -1 };

        /// <summary>
        /// The type of card element
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*type")]
        public string ElementTag { get; private set; } = "None";

        /// <summary>
        /// The location of the element on the card
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*location")]
        public int[] Location { get; private set; } = new int[2] { 0, 0 };

        /// <summary>
        /// The rotation of the element on the card 
        /// </summary>
        [JsonPropertyName("*rotation")]
        public int[] Rotation { get; private set; } = new int[2] { 0, 0 };


        /// <summary>
        /// The scale of the element
        /// </summary>
        [JsonPropertyName("*scale")]
        public int[] Scale { get; private set; } = new int[2] { 0, 0 };

        [JsonIgnore]
        private string _payload;

        /// <summary>
        /// Contains a csv of information needed to customize the card element
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*properties")]
        public string Payload
        {
            get
            {
                string[] str = _payload.Split(',');
                foreach (var st in str)
                {
                    st.Trim();
                }

                return String.Join(",", str);
            }
            set
            {
                _payload = value;
            }
        }


        [JsonPropertyName("*critical?")]
        public bool Critical { get; set; }

        /// <summary>
        /// Returns the length of the payload
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public int PayloadLength
        {
            get
            {
                string[] str = _payload.Split(',');
                return str.Length;
            }
        }


        [JsonIgnore]
        public bool Active { get => throw new NotImplementedException(); set => throw new NotImplementedException(); } //not used to store data

        /// <summary>
        /// The information that the card element will display
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string[] DisplayInfo { get; set; }


        /// <summary>
        /// Can the element be visible to the user?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool CanBeVisible { get; set; } = true;

        /// <summary>
        /// Can the element be edited by the user?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool CanEdit { get; set; } = true;

        [JsonIgnore]
        public DataSetView CardManual { get; set; }

        public CardElementViewData ElementViewData => throw new NotImplementedException();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">the id of the card element data</param>
        /// <param name="tag">the element tag</param>
        /// <param name="location">the location of the element on the card</param>
        [JsonConstructor]
        public CardElementData(int id, string tag, int[] location, int[] rotation, int[] scale, int[] otherIds, string[] layout, bool essential, string payload, DataSetView manual)
        {
            if (id < 0)
                throw new NotSupportedException("the id must be equal or greater than 0");

            ID = id;

            if (tag == null)
                throw new NullReferenceException("The element tag cannot be null");

            ElementTag = tag;

            if (location != null && location.Length >= 2)
                Array.Copy(location, Location, 2);

            Critical = essential;

            if (payload == null)
                throw new NullReferenceException("the payload must contain values");

            Payload = payload;

            // if (manual == null)
            //     throw new NullReferenceException("The card manual is null");

            CardManual = manual;
        }

        public int CompareTo(IUnityCardElement other) //IComparable (for sorting)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {
            throw new NotImplementedException();
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            throw new NotImplementedException();
        }
    }
}
