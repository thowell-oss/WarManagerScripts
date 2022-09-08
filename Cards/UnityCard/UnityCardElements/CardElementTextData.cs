using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{
    [Notes.Author("Handles text for the card element data")]
    public sealed class CardElementTextData : CardElementViewData
    {
        /// <summary>
        /// This is the string that sticks between multiple columns when displayed
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*multicolumn split characters")]
        public string MultiColumnString { get; set; } = "";

        /// <summary>
        /// The font size of the card text data
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*size")]
        public int FontSize { get; set; } = 12;

        /// <summary>
        /// The type of font - bold. 
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*bold")]
        public bool Bold { get; set; } = false;

        /// <summary>
        /// The type of font - italics. 
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*italics")]
        public bool Italics { get; set; } = false;

        /// <summary>
        /// The type of font - underline. 
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*underline")]
        public bool Underline { get; set; } = false;

        /// <summary>
        /// The type of font - strike through. 
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*strike")]
        public bool StrikeThrough { get; set; } = false;

        /// <summary>
        /// The text justification
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*justification")]
        public string TextJustification { get; set; } = "Left";

        /// <summary>
        /// How should the text overflow from its scaled box?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*overflow")]
        public string OverflowType { get; set; } = "Overflow";

        /// <summary>
        /// Should this text allow rich text?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*rich text")]
        public bool RichText { get; set; } = false;

        /// <summary>
        /// The color of the text in hex format
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*color")]
        public string ColorHex { get; set; } = "#111";

        [JsonPropertyName("leading words")]
        public string LeadingWords { get; set; } = "";

        [JsonPropertyName("trailing words")]
        public string TrailingWords { get; set; } = "";

        /// <summary>
        /// Handles some layout cases
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public sealed override string[] Layout { get; set; }
    }
}
