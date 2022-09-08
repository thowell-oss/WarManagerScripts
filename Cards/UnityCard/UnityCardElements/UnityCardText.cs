/* UnityCardText.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using WarManager.Backend.CardsElementData;
using WarManager.Backend;
using WarManager;

namespace WarManager.Cards.Elements
{
    /// <summary>
    /// The unity card text element
    /// </summary>
    [Serializable]
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(RectTransform))]
    [Notes.Author("The unity card text element")]
    public class UnityCardText : PoolableObject, IUnityCardElement
    {
        [SerializeField] private int _ID;
        [SerializeField] private string _elementTag = "text";

        public int fontSize = 20;
        public Color TextColor = Color.black;
        public string TextJustification = "left";

        private CardElementTextData _data;

        /// <summary>
        /// bold
        /// </summary>
        public bool bold;

        /// <summary>
        /// italics
        /// </summary>
        public bool italics;

        /// <summary>
        /// strikethrough
        /// </summary>
        public bool strikeThrough;
        RectTransform _rectTransform;

        /// <summary>
        /// scale 
        /// </summary>
        public Vector2 _scale;

        /// <summary>
        /// rotation
        /// </summary>
        private Vector2 _rotation;

        /// <summary>
        /// The text information to add
        /// </summary>
        /// <value></value>
        public string text { get; set; }

        /// <summary>
        /// The card element id
        /// </summary>
        /// <value></value>
        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        /// <summary>
        /// The type of element being displayed
        /// </summary>
        /// <value></value>
        public string ElementTag
        {
            get
            {
                return _elementTag;
            }
            set
            {
                _elementTag = value;
            }
        }
        public bool Critical { get; set; }

        /// <summary>
        /// Is this object active in the scene?
        /// </summary>
        /// <value></value>
        public bool Active
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }

        public CardElementViewData ElementViewData { get; private set; }

        /// <summary>
        /// The text
        /// </summary>
        private TMP_Text Text;

        /// <summary>
        /// The location of the object
        /// </summary>
        /// <returns></returns>
        public Vector3 Location = new Vector3(0, 0, 0);


        // Start is called before the first frame update
        void Awake()
        {
            if (Text == null)
                Text = GetComponent<TMP_Text>();
        }

        public void Refresh()
        {
            if (Text == null)
                Text = GetComponent<TMP_Text>();

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _rectTransform.sizeDelta = _scale * 20;

            Text.color = TextColor;

            Text.fontSize = fontSize;

            transform.localPosition = Location;
            transform.localRotation = Quaternion.Euler(0, 0, _rotation.x);

            switch (TextJustification)
            {
                case "center":
                    Text.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    break;

                case "right":
                    Text.horizontalAlignment = HorizontalAlignmentOptions.Right;
                    break;

                default:
                    Text.horizontalAlignment = HorizontalAlignmentOptions.Left;
                    break;
            }

            Text.text = _data.LeadingWords + text + _data.TrailingWords;

            if (Text.text == null || Text.text.Trim() == string.Empty)
            {
                Text.text = "<empty>";
            }

            if (!bold && !italics)
            {
                Text.fontStyle = FontStyles.Normal;
            }
            else if (bold)
            {
                if (italics)
                {
                    Text.fontStyle = FontStyles.Bold | FontStyles.Italic;
                }
                else
                {
                    Text.fontStyle = FontStyles.Bold;
                }
            }
            else if (italics)
            {
                Text.fontStyle = FontStyles.Italic;
            }
        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        /// <summary>
        /// Set the text to display a specific string
        /// </summary>
        /// <param name="info"></param>
        public void SetText(string info)
        {
            Text.text = info;
        }

        /// <summary>
        /// Try to set the properties for a card text element
        /// </summary>
        /// <param name="element">the card text element to set the properties for</param>
        /// <param name="text">the data</param>
        /// <returns></returns>
        public static bool TrySetProperties(UnityCardText element, CardElementTextData text, DataEntry entry)
        {

            #region old
            // if (element == null)
            //     Debug.LogError("The element is null");

            // if (data == null)
            //     Debug.LogError("the data is null");

            // if (element.ElementTag == null)
            //     Debug.LogError("The element tag is null");

            // if (data.ElementTag == null)
            //     Debug.LogError("The data element tag is null");

            // if (element.ElementTag != data.ElementTag)
            //     return false;

            // element.gameObject.SetActive(true);

            // element.ID = data.ID;
            // element.Location = new Vector2(data.Location[0], data.Location[1]);
            // element.Critical = data.Critical;

            // if (data.Payload != null && data.Payload != string.Empty)
            // {
            //     string[] payload = data.Payload.Split(',');

            //     // Debug.Log(string.Join(",", payload));

            //     Vector2 size = new Vector2(5, 5);

            //     if (payload.Length >= 1)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[0], out amt))
            //         {
            //             size = new Vector2(amt, size.y);
            //         }
            //         else
            //         {
            //             string message = "Property 1 of the text was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 2)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[1], out amt))
            //         {
            //             size = new Vector2(size.x, amt);
            //         }
            //         else
            //         {
            //             string message = "Property 2 of the text was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     element._scale = size;

            //     if (payload.Length >= 3)
            //     {
            //         // Debug.Log(payload[2]);

            //         element.fontSize = Int32.Parse(payload[2]);
            //     }

            //     element.TextColor = Color.black;
            //     if (payload.Length >= 4)
            //     {
            //         ColorUtility.TryParseHtmlString(payload[3], out element.TextColor);
            //     }

            //     if (payload.Length >= 5)
            //     {
            //         element.TextJustification = payload[4];
            //     }

            // string finalText = "";

            // if (payload.Length >= 4 && canView)
            // {
            //     for (int i = 3; i < payload.Length; i++)
            //     {
            //         try
            //         {
            //             if (Int32.TryParse(payload[i], out var loc))
            //                 finalText = " " + dataPiece.GetData(loc);
            //         }
            //         catch (System.Exception ex)
            //         {
            //             Debug.LogError(ex.Message);
            //             finalText += " error ";
            //         }
            //     }
            // }

            // string someText = "";

            // try
            // {
            //     if (canView)
            //     {
            //         someText = dataPiece.GetData(data.ID);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     someText = ex.Message;
            // }

            // if (data.DisplayInfo != null)
            // {
            //     if (data.CanBeVisible)
            //     {
            //         element.text = string.Join(" ", data.DisplayInfo);
            //     }
            //     else
            //     {
            //         element.text = "";
            //     }
            // }
            // else
            // {
            //     element.text = "(Nothing to display)";
            // }

            // element.Refresh();

            // }

            #endregion

            if (element == null)
                throw new NullReferenceException("the unity card text is null");
            element.ElementViewData = text;
            element._data = text;
            element.TextJustification = text.TextJustification;
            element.fontSize = text.FontSize;
            element._scale = new Vector2((float)text.Scale[0], (float)text.Scale[1]);
            element.Location = new Vector3((float)text.Location[0], (float)text.Location[1], (float)text.Location[2]);
            ColorUtility.TryParseHtmlString(text.ColorHex, out element.TextColor);
            element.bold = text.Bold;
            element.italics = text.Italics;

            element._rotation = new Vector2((float)text.Rotation[0], (float)text.Rotation[1]);

            string data = "";

            // if (text.ColumnCount > 1)
            //     Debug.Log(entry.DataSet.DatasetName + " " + string.Join(", ", text.Columns));

            string[] dataStringArr = new string[text.ColumnCount];
            int i = 0;
            foreach (var col in text.Columns)
            {
                if (entry.TryGetValueAt(col, out var value))
                {
                    dataStringArr[i] = value.ParseToParagraph();
                }
                else
                {
                    dataStringArr = new string[1] { "" };
                }

                i++;
            }

            data = string.Join(text.MultiColumnString, dataStringArr);

            data = data.Trim();

            element.text = data;

            element.Refresh();

            return true;
        }

        public void DisableNonEssential()
        {
            if (!Critical)
                Active = false;
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardElementTextData d)
            {
                return UnityCardText.TrySetProperties(this, d, entry);
            }

            return false;
        }
    }
}

