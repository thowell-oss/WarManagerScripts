

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    [RequireComponent(typeof(RadialDialController))]
    [RequireComponent(typeof(RectTransform))]
    public class UnityCardModernRadialDial : PoolableObject, IUnityCardElement
    {
        public int ID { get; private set; }

        public string ElementTag { get; } = "dial";

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
        public bool Critical { get; set; }

        public CardElementViewData ElementViewData { get; private set; }

        private RadialDialController dial;
        private RectTransform rect;


        private Vector3 _dialPosition = Vector2.zero;
        private Vector2 _dialScale = new Vector2(5, 5);
        private int _dialTextFontSize = 6;
        private Color _dialColor = Color.green;
        private Color _dialBackgroundColor = Color.grey;
        private Color _dialTextColor = Color.black;

        private float _largestValue = 100;
        private float _smallestValue = 0;
        private float _currentAmount = 50;

        private bool _isError = true;


        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other);
        }

        public void DisableNonEssential()
        {

        }

        /// <summary>
        /// Refresh the dial
        /// </summary>
        public void Refresh()
        {
            if (dial == null)
            {
                dial = GetComponent<RadialDialController>();
            }

            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }

            rect.sizeDelta = _dialScale;
            rect.anchoredPosition = _dialPosition;

            dial.BackgroundColor = _dialBackgroundColor;
            dial.DialColor = _dialColor;
            dial.TextColor = _dialTextColor;
            dial.TextFontSize = _dialTextFontSize;

            if (!_isError)
            {
                dial.SetDial(_currentAmount, _smallestValue, _largestValue);
            }
            else
            {
                dial.SetDialAsError();
            }
        }

        /// <summary>
        /// Set up the dial
        /// </summary>
        /// <param name="element">the dial element</param>
        /// <param name="data">the data</param>
        public static bool TrySetDial(UnityCardModernRadialDial element, CardDialElementData data, DataEntry entry)
        {

            #region old
            // if (element == null)
            //     throw new NullReferenceException("The element cannot be null");

            // if (data == null)
            //     throw new NullReferenceException("the data cannot be null");

            // if (element.ElementTag == null)
            //     throw new NullReferenceException("the element tag cannot be null");

            // if (element.ElementTag != data.ElementTag)
            //     return false;

            // element.gameObject.SetActive(true);

            // element._dialPosition = new Vector2(data.Location[0], data.Location[1]);
            // element.ID = data.ID;
            // element.Critical = data.Critical;

            // if (data.Payload != null && data.Payload != string.Empty)
            // {
            //     string[] payload = data.Payload.Split(',');

            //     Vector2 size = new Vector2(1, 1);

            //     if (payload.Length >= 1)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[0], out amt))
            //         {
            //             amt = amt / 2;

            //             size = new Vector2(amt, size.y);
            //             // Debug.Log("x");
            //         }
            //         else
            //         {
            //             string message = "Property 1 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 2)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[1], out amt))
            //         {
            //             amt = amt / 2;

            //             size = new Vector2(size.x, amt);
            //             // Debug.Log("added border amt " + amt);
            //         }
            //         else
            //         {
            //             string message = "Property 2 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     element._dialScale = size;

            //     if (payload.Length >= 3)
            //     {
            //         if (Int32.TryParse(payload[2], out var amt))
            //         {
            //             element._dialTextFontSize = amt;
            //         }
            //         else
            //         {
            //             string message = "Property 3 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 4)
            //     {

            //         if (ColorUtility.TryParseHtmlString(payload[3], out var color))
            //         {
            //             element._dialColor = color;
            //         }
            //         else
            //         {
            //             string message = "Property 4 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 5)
            //     {

            //         if (ColorUtility.TryParseHtmlString(payload[4], out var color))
            //         {
            //             element._dialbackgroundColor = color;
            //         }
            //         else
            //         {
            //             string message = "Property 5 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 6)
            //     {

            //         if (ColorUtility.TryParseHtmlString(payload[5], out var color))
            //         {
            //             element._dialTextColor = color;
            //         }
            //         else
            //         {
            //             string message = "Property 6 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 7)
            //     {
            //         if (float.TryParse(payload[6], out float f))
            //         {
            //             element._totalAmount = f;
            //         }
            //         else
            //         {
            //             string message = "Property 7 of the dial was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     float current = -1;

            //     if (data.DisplayInfo != null && data.DisplayInfo.Length > 0 && data.DisplayInfo[0] != null)
            //     {

            //         if (float.TryParse(data.DisplayInfo[0], out float f))
            //         {
            //             current = f;
            //             element._isError = false;
            //         }
            //     }

            //     element._currentAmount = current;

            //     if (element._currentAmount == -1)
            //         element._isError = true;
            // }


            #endregion

            element._dialScale = new Vector2((float)data.Scale[0], (float)data.Scale[1]);
            element._dialPosition = new Vector3((float)data.Location[0], (float)data.Location[1], (float)data.Location[2]);
            element._dialTextFontSize = data.TextFontSize;

            element.ElementViewData = data; 

            ColorUtility.TryParseHtmlString(data.DialBackgroundColor, out var backgroundColor);
            element._dialBackgroundColor = backgroundColor;

            ColorUtility.TryParseHtmlString(data.DialFallBackColor, out var mainColor);
            element._dialColor = mainColor;

            ColorUtility.TryParseHtmlString(data.TextColor, out var textColor);
            element._dialTextColor = textColor;

            element._largestValue = (float)data.LargestValue;
            element._smallestValue = (float)data.SmallestValue;

            if (entry.TryGetValueAt(data.ToColumnArray[0], out var value))
            {
                var actualValue = value.ParseToRational();
                element._currentAmount = (float)actualValue;
                element._isError = false;
            }
            else
            {
                element._isError = true;
            }

            element.Refresh();
            return true;
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardDialElementData d)
            {
                return TrySetDial(this, d, entry);
            }

            return false;
        }
    }
}
