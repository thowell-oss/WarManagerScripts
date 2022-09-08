
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Text.RegularExpressions;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// class represents the structured card glance data
    /// </summary>
    [Notes.Author("class represents the structured card glance data")]
    public class CardGlanceElementData : CardElementViewData
    {
        /// <summary>
        /// The size of the icons
        /// </summary>
        /// <value></value>
        [JsonPropertyName("icon size")]
        public double[] IconSize { get; set; } = new double[2] { 1, 1 };

        /// <summary>
        /// The max count of the icons
        /// </summary>
        /// <value></value>
        [JsonPropertyName("max icon count")]
        public int MaxIconCount { get; set; } = 4;

        /// <summary>
        /// The type of flow that the icons will have (left, right or center)
        /// </summary>
        /// <returns></returns>
        [JsonPropertyName("flow direction")]
        public GlanceFlowDirection FlowDirection { get; set; } = (GlanceFlowDirection)0;

        /// <summary>
        /// The array of specific glance icons that might show up if possible
        /// </summary>
        /// <value></value>
        [JsonPropertyName("glance icons")]
        public GlanceIconData[] GlanceIcons { get; set; } = new GlanceIconData[0];

        /// <summary>
        /// Layout
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public override string[] Layout { get; set; } = new string[0];
    }

    /// <summary>
    /// card glace fill direction
    /// </summary>
    [Notes.Author("card glance fill direction")]
    public enum GlanceFlowDirection
    {
        left,
        center,
        right,
    }

    /// <summary>
    /// The glance icon data
    /// </summary>
    [Notes.Author("handles storage of glance icon data")]
    public class GlanceIconData : IComparable<GlanceIconData>, IEquatable<GlanceIconData>
    {
        /// <summary>
        /// file path regex
        /// </summary>
        /// <returns></returns>
        private static readonly string _pathRegexString = @"^(([a-zA-Z]{1}:|\\)(\\[^\\/<>:\|\*\?" + "\"]+)+" + @"\.[^\\/<>:\|]{3,4})$";

        /// <summary>
        /// color hex regex
        /// </summary>
        /// <returns></returns>
        private static readonly string _colorHexRegexString = @"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$";

        /// <summary>
        /// The tag id of the glance icon
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string TagId { get; private set; }

        /// <summary>
        /// the location of the icon on server
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

        /// <summary>
        /// Dictates if the icon should show up if the tag exists, or if the icon should show up if the tag does not exist
        /// </summary>
        /// <value></value>
        [JsonPropertyName("show if tag exists")]
        public bool ShowIfTagExists { get; private set; } = true;

        [JsonConstructor]
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="iconPath"></param>
        /// <param name="hexColor"></param>
        public GlanceIconData(string tagId, string iconPath, string hexColor, bool showIfTagExists)
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

            if (iconPath.EndsWith(".exe"))
                throw new ArgumentException("cannot use the icon path to fire an executable");

            Regex colorPathRegex = new Regex(_colorHexRegexString, RegexOptions.IgnoreCase);

            if (!colorPathRegex.IsMatch(hexColor))
                throw new ArgumentException("the color string is not the correct hex format: " + hexColor);


            TagId = tagId;

            IconPath = iconPath;

            HexColor = hexColor;

            ShowIfTagExists = showIfTagExists;
        }

        public override string ToString()
        {
            return TagId + " " + IconPath + " " + HexColor;
        }

        public int CompareTo(GlanceIconData other)
        {
            if (other == null || other.TagId == null)
                return 1;

            if (TagId == null)
                return -1;

            return TagId.CompareTo(other.TagId);
        }

        public bool Equals(GlanceIconData other)
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