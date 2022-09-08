
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{
    /// <summary>
    /// The background element data
    /// </summary>
    [Notes.Author("The background element data")]
    public sealed class CardBackgroundElementData : CardElementViewData
    {
        /// <summary>
        /// The background color hex
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*color")]
        public string ColorHex { get; set; } = "#ffeced";

        /// <summary>
        /// background border thickness
        /// </summary>
        /// <value></value>
        [JsonPropertyName("border thickness")]
        public double BorderThickness { get; set; } = .5;

        /// <summary>
        /// layout
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*layout")]
        public sealed override string[] Layout { get; set; }

    }
}
