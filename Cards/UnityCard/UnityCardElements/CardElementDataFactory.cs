
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Text.Json;
using System.Text.Json.Serialization;

using System.Text.RegularExpressions;

namespace WarManager.Backend.CardsElementData
{

    public class CardElementDataFactory
    {
        /// <summary>
        /// the color hex pattern for regexes
        /// </summary>
        /// <returns></returns>
        private static readonly string RegexHexColorPattern = @"^#(([0-9a-f]{3,4})|([0-9a-f]{6})|([0-9a-f]{8}))$";

        /// <summary>
        /// Parse the JSON and convert it into a list of CardElementViewData
        /// </summary>
        /// <param name="JSON">the json file</param>
        /// <returns>returns a list of card element view data</returns>
        public static List<CardElementViewData> GetElementData(string JSON)
        {
            using (JsonDocument doc = JsonDocument.Parse(JSON))
            {
                return GetElementData(doc.RootElement);
            }
        }

        /// <summary>
        /// Get the properties for all elements within a certain view
        /// </summary>
        /// <param name="rootElement">the root element</param>
        /// <returns>returns a list of data for the card elements</returns>
        public static List<CardElementViewData> GetElementData(JsonElement rootElement, string debugId = "")
        {

            List<CardElementViewData> dataList = new List<CardElementViewData>();

            foreach (var element in rootElement.EnumerateArray())
            {
                try
                {
                    JsonElement cols = GetElement("*other cols", element);
                    JsonElement type = GetElement("*type", element);
                    JsonElement version = GetElement("element version", element);
                    JsonElement location = GetElement("*location", element);
                    JsonElement rotation = GetElement("*rotation", element);
                    JsonElement scale = GetElement("*scale", element);
                    JsonElement criticalElement = GetElement("*critical?", element);

                    List<int> otherCols = new List<int>();
                    foreach (var col in cols.EnumerateArray())
                    {
                        otherCols.Add(col.GetInt32());
                    }

                    if (otherCols.Count < 1)
                        throw new ArgumentException("cannot have an element not matching to any columns");

                    double doubleVersion = version.GetDouble();

                    double[] parsedLocation = GetJsonDoubleData(location, 3);
                    double[] parsedRotation = GetJsonDoubleData(rotation, 2);
                    double[] parsedScale = GetJsonDoubleData(scale, 2);


                    string typeString = type.GetString();

                    CardElementViewData data = GetData(element, typeString);

                    if (data == null)
                        throw new ArgumentException("the data cannot be null");


                    data.SetColumns = otherCols;
                    data.ColumnType = typeString;
                    data.Version = doubleVersion;
                    data.Location = parsedLocation;
                    data.Rotation = parsedRotation;
                    data.Scale = parsedScale;
                    data.Critical = criticalElement.GetBoolean();

                    dataList.Add(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message + " " + debugId);
                }
            }

            return dataList;
        }

        /// <summary>
        /// Get the data for a specific type of card element, throw an error if not found
        /// </summary>
        /// <param name="root">the root json element</param>
        /// <param name="type">the name of the JSON element</param>
        /// <returns>returns a card element view data</returns>
        private static CardElementViewData GetData(JsonElement root, string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("The element type cannot be null");

            switch (type)
            {
                case "text":
                    return GetTextData(root);

                case "background":
                    return GetBackgroundData(root);

                case "glance":
                    return GetGlanceElementData(root);

                case "sticker":
                    return GetStickerData(root);

                case "dial":
                    return GetDialData(root);

                case "link":
                    return GetButton(root);

                case "pie chart":
                    return GetPieChartElementData(root);

                case "bar":
                    return GetLoadingBarElementData(root);

                default:
                    throw new ArgumentException("No such type recognized as " + type);
            }
        }


        /// <summary>
        /// Get the JSON data to an array of doubles
        /// </summary>
        /// <param name="element">the jsonElement</param>
        /// <param name="length">the expected array length (less than one means don't care)</param>
        /// <returns>returns a double array</returns>
        private static double[] GetJsonDoubleData(JsonElement element, int length)
        {
            if ((length > 0 && element.GetArrayLength() != length) || element.GetArrayLength() == 0)
                throw new ArgumentOutOfRangeException("the length of the array is not correct " + element.GetArrayLength() + " " + length);

            List<double> data = new List<double>();

            foreach (var x in element.EnumerateArray())
            {
                data.Add(x.GetDouble());
            }

            return data.ToArray();
        }

        /// <summary>
        /// Convert JSON into an element - throw an error if not found
        /// </summary>
        /// <param name="elementName">the name of the JSON element</param>
        /// <param name="rootElement">the root element</param>
        /// <returns>returns a json element if found</returns>
        private static JsonElement GetElement(string elementName, JsonElement rootElement)
        {
            if (rootElement.TryGetProperty(elementName, out var el))
            {
                return el;
            }

            throw new ArgumentException("Cannot find the selected element " + elementName);
        }

        /// <summary>
        /// Get the text data
        /// </summary>
        /// <param name="element">the json element to get the data from</param>
        /// <returns>returns card text element data</returns>
        private static CardElementTextData GetTextData(JsonElement element)
        {
            JsonElement fontSizeElement = GetElement("*size", element);
            JsonElement boldElement = GetElement("*bold", element);
            JsonElement italicsElement = GetElement("*italics", element);
            JsonElement underlineElement = GetElement("*underline", element);
            JsonElement strikeElement = GetElement("*strike", element);
            JsonElement justificationElement = GetElement("*justification", element);
            JsonElement overflowElement = GetElement("*overflow", element);
            JsonElement richTextElement = GetElement("*rich text", element);
            JsonElement colorElement = GetElement("*color", element);
            JsonElement splitStringElement = GetElement("*multicolumn split characters", element);

            int fontSize = fontSizeElement.GetInt32();
            bool bold = boldElement.GetBoolean();
            bool italics = italicsElement.GetBoolean();
            bool underline = underlineElement.GetBoolean();
            bool strike = strikeElement.GetBoolean();
            string justify = justificationElement.GetString();
            string overflow = overflowElement.GetString();
            bool richText = richTextElement.GetBoolean();
            string colorHex = colorElement.GetString();
            string splitString = splitStringElement.GetString();

            string leadingWords = "";
            string trailingWords = "";

            
            if(element.TryGetProperty("leading words", out var leading))
            {
                leadingWords = leading.GetString();
            }

            if(element.TryGetProperty("trailing words", out var trailing))
            {
                trailingWords = trailing.GetString();
            }

            if (fontSize <= 0)
                throw new ArgumentOutOfRangeException("the font size cannot be less than 1");

            if (string.IsNullOrEmpty(justify))
                justify = "left";

            if (string.IsNullOrEmpty(overflow))
                overflow = "none";

            if (string.IsNullOrEmpty(colorHex))
                colorHex = "#111";

            return new CardElementTextData()
            {
                MultiColumnString = splitString,
                FontSize = fontSize,
                Bold = bold,
                Italics = italics,
                Underline = underline,
                StrikeThrough = strike,
                TextJustification = justify,
                OverflowType = overflow,
                RichText = richText,
                ColorHex = colorHex,
                LeadingWords = leadingWords,
                TrailingWords = trailingWords
            };
        }

        /// <summary>
        /// Get the pie chart data
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static PieChartElementData GetPieChartElementData(JsonElement element)
        {
            JsonElement dataElement = GetElement("pie chart data", element);

            List<PieChartData> data = new List<PieChartData>();

            foreach (var x in dataElement.EnumerateArray())
            {
                PieChartData newData = new PieChartData()
                {
                    Column = x.GetProperty("column").GetInt32(),
                    Min = x.GetProperty("minimum value").GetInt32(),
                    Max = x.GetProperty("maximum value").GetInt32(),
                    HexColor = x.GetProperty("color").GetString()
                };

                data.Add(newData);
            }

            JsonElement offsetElement = GetElement("label orbit offset", element);
            JsonElement orbitDistanceElement = GetElement("label orbit distance", element);

            return new PieChartElementData()
            {
                Data = data,
                LabelOffset = GetJsonDoubleData(offsetElement, 2),
                LabelOrbitDistanceFromCenter = orbitDistanceElement.GetDouble(),
                TitleFontSize = GetElement("title font size", element).GetInt32(),
                TitleColorHex = GetElement("title color", element).GetString(),
                LabelFontSize = GetElement("label font size", element).GetInt32(),
                LabelValueFormat = GetElement("value format", element).GetString(),
                LabelColorHex = GetElement("label color", element).GetString(),
            };
        }

        /// <summary>
        /// Get the loading bar
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static LoadingBarElementData GetLoadingBarElementData(JsonElement element)
        {
            JsonElement barColorJson = GetElement("bar color", element);
            JsonElement backgroundColorJson = GetElement("background color", element);
            JsonElement useGradientJson = GetElement("use gradient", element);
            JsonElement topGradientColorJson = GetElement("bar color gradient top", element);
            JsonElement minJson = GetElement("minimum value", element);
            JsonElement maxJson = GetElement("maximum value", element);
            JsonElement titleFontJson = GetElement("title text font size", element);
            JsonElement titleColorJson = GetElement("title text color", element);
            JsonElement percentTextJson = GetElement("percent text?", element);
            JsonElement percentTextFontJson = GetElement("percent text font size", element);
            JsonElement percentTextColorJson = GetElement("percent text color", element);
            JsonElement useLineJson = GetElement("use line", element);
            JsonElement lineColorJson = GetElement("line color", element);

            return new LoadingBarElementData()
            {
                BarColorHex = barColorJson.GetString(),
                BackgroundColorHex = backgroundColorJson.GetString(),
                UseGradientColor = useGradientJson.GetBoolean(),
                BarColorGradientTop = topGradientColorJson.GetString(),
                Min = minJson.GetDouble(),
                Max = maxJson.GetDouble(),
                TitleTextFontSize = titleFontJson.GetInt32(),
                TitleTextColor = titleColorJson.GetString(),
                UsePercentText = percentTextJson.GetString(),
                PercentTextFontSize = percentTextFontJson.GetInt32(),
                PercentTextColor = percentTextColorJson.GetString(),
                UseLine = useLineJson.GetBoolean(),
                LineColor = lineColorJson.GetString()
            };
        }


        /// <summary>
        /// Get glance data
        /// </summary>
        /// <param name="element">the json element</param>
        /// <returns>returns the glance element if successful</returns>
        private static CardGlanceElementData GetGlanceElementData(JsonElement element)
        {
            JsonElement iconSizeElement = GetElement("icon size", element);
            JsonElement maxIconCountElement = GetElement("max icon count", element);
            JsonElement flowDirectionElement = GetElement("flow direction", element);
            JsonElement glanceIconsElement = GetElement("glance icons", element);

            double[] iconSize = GetJsonDoubleData(iconSizeElement, 2);
            int maxIconCount = maxIconCountElement.GetInt32();
            GlanceFlowDirection direction = (GlanceFlowDirection)flowDirectionElement.GetInt32();

            List<GlanceIconData> data = new List<GlanceIconData>();

            foreach (var iconData in glanceIconsElement.EnumerateArray())
            {
                JsonElement idElement = GetElement("id", iconData);
                JsonElement pathElement = GetElement("path", iconData);
                JsonElement colorElement = GetElement("color", iconData);
                JsonElement showTagElement = GetElement("show if tag exists", iconData);

                GlanceIconData glanceIconStructure = new GlanceIconData(idElement.GetString(), pathElement.GetString(),
                colorElement.GetString(), showTagElement.GetBoolean());

                data.Add(glanceIconStructure);
            }

            return new CardGlanceElementData()
            {
                IconSize = iconSize,
                MaxIconCount = maxIconCount,
                FlowDirection = direction,
                GlanceIcons = data.ToArray()
            };
        }

        /// <summary>
        /// Get the sticker data
        /// </summary>
        /// <param name="element">the json element</param>
        /// <returns>returns sticker data</returns>
        private static CardStickerElementData GetStickerData(JsonElement element)
        {
            JsonElement stickerDataElement = GetElement("stickers", element);

            List<StickerData> data = new List<StickerData>();

            foreach (var x in stickerDataElement.EnumerateArray())
            {
                JsonElement idElement = GetElement("id", x);
                JsonElement pathElement = GetElement("path", x);
                JsonElement colorElement = GetElement("color", x);

                StickerData newData = new StickerData(idElement.GetString(), pathElement.GetString(), colorElement.GetString());

                data.Add(newData);
            }

            return new CardStickerElementData()
            {
                StickersData = data.ToArray(),
            };
        }

        /// <summary>
        /// Get the dial data
        /// </summary>
        /// <param name="element">the json element</param>
        /// <returns>returns the dial data</returns>
        private static CardDialElementData GetDialData(JsonElement element)
        {
            JsonElement textColorElement = GetElement("text color", element);
            JsonElement fallBackColorElement = GetElement("fall back color", element);
            JsonElement backgroundColorElement = GetElement("background color", element);
            JsonElement textFontSizeElement = GetElement("text font size", element);
            JsonElement smallestValueElement = GetElement("smallest value", element);
            JsonElement largestValueElement = GetElement("largest value", element);
            JsonElement dialColors = GetElement("dial colors", element);

            var textColor = textColorElement.ToString();
            var fallbackColor = fallBackColorElement.ToString();
            var backgroundColor = backgroundColorElement.ToString();

            int fontSize = textFontSizeElement.GetInt32();

            if (textColor == null)
                throw new NullReferenceException("the text color cannot be null");
            if (fallbackColor == null)
                throw new NullReferenceException("the fall back color cannot be null");

            if (backgroundColor == null)
                throw new NullReferenceException("the background color cannot be null");


            Regex regex = new Regex(RegexHexColorPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!regex.IsMatch(textColor))
                throw new ArgumentException("the text color does not match a hex pattern: " + textColor);

            if (!regex.IsMatch(fallbackColor))
                throw new ArgumentException("the fallback color does not match a hex pattern: " + fallbackColor);

            if (!regex.IsMatch(backgroundColor))
                throw new ArgumentException("the background color does not match a hex pattern: " + backgroundColor);

            var largestValue = largestValueElement.GetDouble();
            var smallestValue = smallestValueElement.GetDouble();

            if (smallestValue > largestValue)
            {
                double temp = smallestValue;
                smallestValue = largestValue;
                largestValue = temp;
            }

            if (smallestValue == largestValue)
                throw new ArgumentException("The smallest value cannot be the same as the largest value");

            List<DialColorSetting> settings = new List<DialColorSetting>();

            foreach (var x in dialColors.EnumerateArray())
            {
                JsonElement colorElement = GetElement("color", x);
                JsonElement startElement = GetElement("start", x);

                string color = colorElement.GetString();
                double start = startElement.GetDouble();

                if (start < smallestValue)
                    start = smallestValue;

                if (start >= largestValue)
                    start = largestValue - 1;

                settings.Add(new DialColorSetting(color, start));
            }

            return new CardDialElementData()
            {
                TextColor = textColor,
                DialFallBackColor = fallbackColor,
                DialBackgroundColor = backgroundColor,
                TextFontSize = fontSize,
                SmallestValue = smallestValue,
                LargestValue = largestValue,
                DialColors = settings.ToArray()
            };
        }

        /// <summary>
        /// Get the button element
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>returns the button element</returns>
        private static CardButtonElementData GetButton(JsonElement element)
        {
            JsonElement ColorElement = GetElement("background color", element);
            JsonElement fontSize = GetElement("font size", element);

            string color = ColorElement.GetString();

            if (color == null)
                throw new NullReferenceException("the background color cannot be null");

            Regex regex = new Regex(RegexHexColorPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!regex.IsMatch(color))
                throw new ArgumentException("the background color does not match a hex pattern: " + color);

            return new CardButtonElementData()
            {
                BackgroundColor = color,
                FontSize = fontSize.GetInt32()
            };

        }

        /// <summary>
        /// Get the element data for the background
        /// </summary>
        /// <param name="element">the root json element</param>
        /// <returns>returns the background data</returns>
        private static CardBackgroundElementData GetBackgroundData(JsonElement element)
        {
            JsonElement colorElement = GetElement("*color", element);
            string color = colorElement.GetString();

            if (string.IsNullOrEmpty(color))
                color = "#ffeced";

            Regex regex = new Regex(RegexHexColorPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!regex.IsMatch(color))
                throw new ArgumentException("the background color does not match a hex pattern: " + color);

            return new CardBackgroundElementData()
            {
                ColorHex = color,
            };
        }
    }

}
