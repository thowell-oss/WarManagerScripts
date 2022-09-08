
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// Represents the data for the card button element
    /// </summary>
    [Notes.Author("Represents the data for the card button element")]
    public class CardButtonElementData : CardElementViewData
    {

        /// <summary>
        /// The regex for the color hex
        /// </summary>
        /// <returns></returns>
        private static readonly string _colorHexRegex = @"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$";


        /// <summary>
        /// private backing field
        /// </summary>
        private string _backgroundColor = "#0000";

        /// <summary>
        /// The Background color
        /// </summary>
        /// <value></value>
        [JsonPropertyName("background color")]
        public string BackgroundColor
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
        /// The font size of the card button link
        /// </summary>
        /// <value></value>
        [JsonPropertyName("font size")]
        public int FontSize { get; set; }

        /// <summary>
        /// layout
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public override string[] Layout { get; set; }
    }
}