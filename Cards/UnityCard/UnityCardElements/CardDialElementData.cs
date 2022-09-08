using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{

    /// <summary>
    /// Contains the data for the War Manager modern dial
    /// </summary>
    [Notes.Author("Contains data for the War Manager modern dial")]
    public class CardDialElementData : CardElementViewData
    {
        /// <summary>
        /// The regex for the color hex
        /// </summary>
        /// <returns></returns>
        private static readonly string _colorHexRegex = @"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$";

        /// <summary>
        /// private backing field
        /// </summary>
        private string _textColor = "#111";

        /// <summary>
        /// The hex code color of the text
        /// </summary>
        /// <value></value>
        [JsonPropertyName("text color")]
        public string TextColor
        {
            get => _textColor;
            set
            {
                Regex regex = new Regex(_colorHexRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (!regex.IsMatch(value))
                    throw new ArgumentException("the color is not the correct hex expression: " + value);

                _textColor = value;
            }

        }

        private string _fallbackColor = "#eee";

        /// <summary>
        /// The hex code color of the dial
        /// </summary>
        /// <value></value>
        [JsonPropertyName("fall back color")]
        public string DialFallBackColor
        {
            get => _fallbackColor;

            set
            {
                Regex regex = new Regex(_colorHexRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (!regex.IsMatch(value))
                    throw new ArgumentException("the color is not the correct hex expression: " + value);

                _fallbackColor = value;
            }
        }

        private string _backgroundColor = "#111";

        /// <summary>
        /// the hex code color of the fall back dial background
        /// </summary>
        /// <value></value>
        [JsonPropertyName("background color")]
        public string DialBackgroundColor
        {
            get => _backgroundColor;

             set
            {
                Regex regex = new Regex(_colorHexRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (!regex.IsMatch(value))
                    throw new ArgumentException("the color is not the correct hex expression: " + value);

                _backgroundColor = value;
            }
        }

        /// <summary>
        /// the font size of the text inside the dial
        /// </summary>
        /// <value></value>
        [JsonPropertyName("text font size")]
        public int TextFontSize { get; set; } = 12;

        /// <summary>
        /// the smallest value allowed when calculating the dial range (inclusive)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("smallest value")]
        public double SmallestValue { get; set; } = 0;

        /// <summary>
        /// The largest value allowed when calculating the dial range (inclusive)
        /// </summary>
        /// <value></value>
        [JsonPropertyName("largest value")]
        public double LargestValue { get; set; } = 100;

        /// <summary>
        /// the array of dial colors and their partition locations
        /// </summary>
        /// <value></value>
        [JsonPropertyName("dial colors")]
        public DialColorSetting[] DialColors { get; set; } = new DialColorSetting[0];

        /// <summary>
        /// layout
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public override string[] Layout { get; set; }

    }

    /// <summary>
    /// Handles colors at certain partitions of the dial
    /// </summary>
    [Notes.Author("Handles colors at certain partitions of the dial")]
    public class DialColorSetting : IEquatable<DialColorSetting>, IComparable<DialColorSetting>
    {
        /// <summary>
        /// The regex for the color hex
        /// </summary>
        /// <returns></returns>
        private static readonly string _colorHexRegex = @"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$";

        private string _color = "#9f9";

        /// <summary>
        /// The color value at that location 
        /// </summary>
        /// <value></value>
        [JsonPropertyName("color")]
        public string Color
        {
            get => _color;
            set
            {
                Regex regex = new Regex(_colorHexRegex, RegexOptions.IgnoreCase);
                if (!regex.IsMatch(value))
                    throw new ArgumentException("the color is not the correct hex expression: " + value);

                _color = value;
            }
        }

        /// <summary>
        /// the starting value that the color will appear when
        /// </summary>
        /// <value></value>
        [JsonPropertyName("start")]
        public double StartValue { get; set; } = 50;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="color"></param>
        /// <param name="StartValue"></param>
        [JsonConstructor]
        public DialColorSetting(string color, double startValue)
        {
            if (color == null)
                throw new NullReferenceException("the color cannot be null");

            Regex regex = new Regex(@"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (string.Empty == color)
                throw new ArgumentException("the color string is empty");

            if (!regex.IsMatch(color))
                throw new ArgumentException("the color is not the correct hex expression: " + color);

            Color = color;
            startValue = StartValue;
        }

        public bool Equals(DialColorSetting other)
        {
            if (other == null || other.Color == null)
                return false;

            if (Color == null)
                return false;

            return other.Color == Color && StartValue == other.StartValue;
        }

        public int CompareTo(DialColorSetting other)
        {
            if (other == null || other.Color == null)
                return 1;

            if (Color == null)
                return 1;

            int x = Color.CompareTo(other.Color);

            if (x == 0)
                return StartValue.CompareTo(other.StartValue);

            return x;
        }
    }

}