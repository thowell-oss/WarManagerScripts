
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// Represents data for a sticker element
    /// </summary>
    [Notes.Author("represents the data for a sticker element")]
    public class CardStickerElementData : CardElementViewData
    {

        /// <summary>
        /// The array of stickers
        /// </summary>
        /// <value></value>
        [JsonPropertyName("stickers")]
        public StickerData[] StickersData { get; set; } = new StickerData[0];

        /// <summary>
        /// layout
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public override string[] Layout { get; set; }
    }

    /// <summary>
    /// Handles the data for each sticker
    /// </summary>
    public class StickerData : IComparable<StickerData>, IEquatable<StickerData>
    {
        /// <summary>
        /// The tag id of the sticker icon
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string TagId { get; private set; }

        /// <summary>
        /// the location of the sticker icon on server
        /// </summary>
        /// <value></value>
        [JsonPropertyName("path")]
        public string IconPath { get; private set; }

        /// <summary>
        /// the color of the icon (hex value)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("color")]
        public string HexColor { get; private set; }


        [JsonConstructor]
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="iconPath"></param>
        /// <param name="hexColor"></param>
        public StickerData(string tagId, string iconPath, string hexColor)
        {
            if (tagId == null)
                throw new NullReferenceException("the tag id cannot be null");

            if (iconPath == null)
                throw new NullReferenceException("the icon path cannot be null");

            if (hexColor == null)
                throw new NullReferenceException("the hex color cannot be null");

            if (tagId == string.Empty)
                throw new ArgumentException("the tag id cannot be empty");

            if (iconPath == string.Empty)
                throw new ArgumentException("the icon path cannot be empty");

            if (hexColor == string.Empty)
                throw new ArgumentException("the hex color cannot be empty");

            TagId = tagId;

            IconPath = iconPath;

            HexColor = hexColor;

        }

        public override string ToString()
        {
            return TagId + " " + IconPath + " " + HexColor;
        }

        public int CompareTo(StickerData other)
        {
            if (other == null || other.TagId == null)
                return 1;

            if (TagId == null)
                return -1;

            return TagId.CompareTo(other.TagId);
        }

        public bool Equals(StickerData other)
        {
            if (other == null)
                return false;

            if (TagId == null && other.TagId == null)
            {
                return false;
            }
            else
            {
                if (other.TagId == null || TagId == null)
                    return false;

                return other.TagId == TagId;
            }
        }
    }
}
