
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using WarManager.Backend;
using WarManager.Cards;

namespace WarManager
{
    [Notes.Author("Handles the storage and identity of the line")]
    public class AnnotateLine
    {
        [JsonIgnore]
        /// <summary>
        /// The list of points
        /// </summary>
        /// <typeparam name="Pointf"></typeparam>
        /// <returns></returns>
        private List<Pointf> _linePoints = new List<Pointf>();

        /// <summary>
        /// the color of the line
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public Color LineColor { get; private set; } = Color.blue;

        [JsonPropertyName("color")]
        public string LineColorHex => ColorUtility.ToHtmlStringRGBA(LineColor);

        /// <summary>
        /// The thickness of the line
        /// </summary>
        /// <value></value>
        [JsonPropertyName("thickness")]
        public float Thickness { get; private set; } = 5;

        /// <summary>
        /// iterate through the list of points
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public IEnumerable<Pointf> Points
        {
            get => _linePoints;
        }

        /// <summary>
        /// the count of points
        /// </summary>
        [JsonIgnore]
        public int Count => _linePoints.Count;

        [JsonPropertyName("Points")]
        Pointf[] GetPoints => _linePoints.ToArray();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="points">the list of points</param>
        /// <param name="lineColor">the line color</param>
        /// <param name="thickness">the line thickness</param>
        public AnnotateLine(List<Pointf> points, Color lineColor, float thickness)
        {
            if (points == null)
                throw new NullReferenceException("the list of points is null");

            if (points.Count < 2)
                throw new ArgumentException("there are less than two points in the list");

            if (lineColor == null)
                throw new NullReferenceException("the line color cannot be null");

            if (thickness <= 0)
                throw new NullReferenceException("the thickness must be greater than zero");

            _linePoints = points;
            LineColor = lineColor;
            Thickness = thickness;
        }

        public string GetJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}