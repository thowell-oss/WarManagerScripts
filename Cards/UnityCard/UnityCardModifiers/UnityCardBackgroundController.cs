/* UnityCardBackgroundController.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;
using WarManager.Cards.Elements;

using WarManager.Unity3D;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles the background objects of the card -> the background and border (and associated properties)
    /// </summary>
    [SerializeField]
    public class UnityCardBackgroundController : MonoBehaviour, IUnityCardElement
    {
        public SpriteRenderer Background;
        public SpriteRenderer Border;

        public SpriteRenderer DataSetColorBar;

        public SpriteRenderer BoltIcon;

        public BoxCollider2D CardCollider;

        [SerializeField] Vector3 boltIconLocation;

        private bool _isActor;

        public bool Actor
        {
            get => _isActor;

            set
            {
                if (value)
                {
                    DataSetColorBar.gameObject.SetActive(false);
                    BoltIcon.gameObject.SetActive(true);
                }
                else
                {
                    DataSetColorBar.gameObject.SetActive(true);
                    BoltIcon.gameObject.SetActive(false);
                }
            }
        }


        /// <summary>
        /// The color of the border
        /// </summary>
        /// <value></value>
        public Color BorderColor
        {
            get
            {
                return Border.color;
            }
            set
            {
                SetBorderColor(value);
            }
        }

        /// <summary>
        /// Handles the border visibility
        /// </summary>
        /// <value></value>
        public bool BorderActive
        {
            get
            {
                return Border.gameObject.activeSelf;
            }

            set
            {
                Border.gameObject.SetActive(value);
            }
        }

        [SerializeField]
        private float _borderThickness = .2f;

        /// <summary>
        /// The thickness of the border
        /// </summary>
        /// <value></value>
        public float BorderThickness
        {
            get
            {
                return _borderThickness;
            }
            set
            {
                _borderThickness = Mathf.Clamp(value, .05f, 1);
                Refresh();
            }
        }

        [SerializeField] private Color _backgroundColor; //for json

        /// <summary>
        /// The Color of the background
        /// </summary>
        /// <value></value>
        public Color BackgroundColor
        {
            get
            {
                return Background.color;
            }
            set
            {
                SetBackgroundColor(value);
                _backgroundColor = value;
            }
        }

        /// <summary>
        /// private backing field
        /// </summary>
        private Color _dataSetColorBar;

        /// <summary>
        /// The Color of the data set color bar
        /// </summary>
        /// <value></value>
        public Color DataSetBarColor
        {
            get
            {
                return _dataSetColorBar;
            }
            set
            {
                _dataSetColorBar = value;

                if (SheetsManager.ShowDataSetColorBars)
                {
                    DataSetColorBar.color = value;
                    BoltIcon.color = value;
                }
                else
                {
                    DataSetColorBar.color = Color.clear;
                    BoltIcon.color = Color.clear;
                }
            }
        }

        [SerializeField] private Vector2 _size;


        /// <summary>
        /// The size of the card
        /// </summary>
        /// <value></value>
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                SetCardSize(value);
            }
        }

        public UnityCard _unityCard;

        /// <summary>
        /// The rect for stretching a card
        /// </summary>
        /// <value></value>
        public Rect StretchInfo { get; set; }

        /// <summary>
        /// The column id of the element
        /// </summary>
        /// <value></value>
        public int ID { get; set; }

        /// <summary>
        /// The element tag is "background"
        /// </summary>
        /// <value></value>
        public string ElementTag { get; } = "background";

        /// <summary>
        /// Can the object be seen?
        /// </summary>
        /// <value></value>
        public bool Active
        {
            get
            {
                return gameObject.activeSelf;
            }

            set
            {
                gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// The background is always critical
        /// </summary>
        /// <value></value>
        public bool Critical
        {
            get => true;
            set => throw new System.NotSupportedException("The background is always critical");
        }

        public CardElementViewData ElementViewData { get; private set; }

        /// <summary>
        /// Set the border color
        /// </summary>
        /// <param name="color"></param>
        private void SetBorderColor(Color color)
        {
            if (LeanTween.isTweening(Border.gameObject))
            {
                LeanTween.cancel(Border.gameObject);
            }
            else
            {
                // if (color == Color.clear)
                // {
                //     LeanTween.delayedCall(.35f, () => Border.gameObject.SetActive(false));
                // }
            }

            LeanTween.value(Border.gameObject, SetColorBorder_tween, Border.color, color, .35f).setEaseOutCubic();
        }

        private void SetColorBorder_tween(Color color)
        {
            Border.color = color;
        }

        /// <summary>
        /// Set the background color
        /// </summary>
        /// <param name="color"></param>
        private void SetBackgroundColor(Color color)
        {
            Background.color = color;
        }

        /// <summary>
        /// Set the card size
        /// </summary>
        /// <param name="newSize"></param>
        private void SetCardSize(Vector2 newSize)
        {
            CheckSize(newSize);
            // Refresh();
        }

        /// <summary>
        /// Recalculate the size 
        /// </summary>
        /// <param name="newSize"></param>
        private void CheckSize(Vector2 newSize)
        {
            float smallestSize = 1;
            float largestSize = 20;

#if UNITY_EDITOR
            if (newSize.x < smallestSize)
                Debug.LogError("The background size cannot get smaller than " + smallestSize);

            if (newSize.y > largestSize)
                Debug.LogError("The background cannot be larger than " + largestSize);
#endif

            float x = Mathf.Clamp(newSize.x, smallestSize, largestSize);
            float y = Mathf.Clamp(newSize.y, smallestSize, largestSize);
            Vector2 final = new Vector2(x, y);

            // Debug.Log(final);

            _size = final;
        }

        /// <summary>
        /// Refresh the background
        /// </summary>
        public void Refresh()
        {
            // LeanTween.cancel(Background.gameObject);
            // LeanTween.value(Background.gameObject, Animate, Background.size, _size, .25f).setEaseOutExpo();
            AnimateBackgroundSize(Size, false);



        }

        /// <summary>
        /// Tween the card background
        /// </summary>
        /// <param name="newSize">the new size the backround will tween to</param>
        /// <param name="additiveSize">does the new size value add to the old size, or simply reset the old size?</param>
        /// <param name="time">the amount of time it takes to animate to the new size (default .25)</param>
        public void AnimateBackgroundSize(Vector2 newSize, bool additiveSize, float time = .25f, bool easeOut = true, bool setDataSetBarActive = true)
        {
            if (setDataSetBarActive == false)
            {
                LeanTween.color(BoltIcon.gameObject, Color.clear, .125f);
                LeanTween.color(DataSetColorBar.gameObject, Color.clear, .125f);


                LeanTween.delayedCall(.125f, () =>
                {
                    BoltIcon.gameObject.SetActive(false);
                    DataSetColorBar.gameObject.SetActive(false);
                });
            }

            Vector2 final = newSize;

            if (additiveSize)
            {
                final = new Vector2(_size.x + newSize.x, _size.y + newSize.y);
            }

            if (StretchInfo != null)
            {
                Vector2 stretchSize = new Vector2(StretchInfo.Width * _unityCard.Grid.GridScale.x, StretchInfo.Height * _unityCard.Grid.GridScale.y);
                final += stretchSize;
            }

            LeanTween.cancel(Background.gameObject);

            if (easeOut)
            {
                LeanTween.value(Background.gameObject, Animate, Background.size, final, time).setEaseOutExpo();
            }
            else
            {
                LeanTween.value(Background.gameObject, Animate, Background.size, final, time).setEaseInExpo();
            }
        }

        private void Animate(Vector2 size)
        {
            CardCollider.size = size;
            Background.size = size;
            DataSetColorBar.transform.localScale = new Vector3(.2f, size.y - .2f, 1);

            float location = (size.x / 2) - 0.1f;
            DataSetColorBar.transform.localPosition = new Vector3(-location, 0, -0.1f);

            Vector2 borderSize = new Vector2(size.x + BorderThickness, size.y + BorderThickness);
            Border.size = borderSize;

            boltIconLocation = new Vector3(-location, (size.y / 2) - .14f, -.2f);
            BoltIcon.transform.localPosition = boltIconLocation;
        }


        public static bool TrySetProperties(UnityCardBackgroundController element, CardBackgroundElementData data)
        {

            #region old
            // if (element.ElementTag != data.ElementTag)
            //     return false;

            // element.ID = data.ID;

            // string[] payload = data.Payload.Split(',');

            // if (payload.Length >= 1)
            // {
            //     Color color = Color.white;
            //     if (ColorUtility.TryParseHtmlString(payload[0], out color))
            //     {
            //         element.BackgroundColor = color;
            //     }
            //     else
            //     {
            //         string message = "Property 1 of the card background was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }
            // }

            // Vector2 size = new Vector2(4, 2);

            // if (payload.Length >= 2)
            // {
            //     float amt = 10;
            //     if (float.TryParse(payload[1], out amt))
            //     {
            //         size = new Vector2(amt, size.y);
            //         //element.Size = size;
            //     }
            //     else
            //     {
            //         string message = "Property 2 of the card background was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }
            // }


            // if (payload.Length >= 3)
            // {
            //     float amt = 10;
            //     if (float.TryParse(payload[2], out amt))
            //     {
            //         size = new Vector2(size.x, amt);
            //         //element.Size = size;
            //         // Debug.Log("added border amt " + amt);
            //     }
            //     else
            //     {
            //         string message = "Property 3 of the card background was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }
            // }

            // element.Size = size;

            // if (payload.Length >= 4)
            // {
            //     int amt = 2;
            //     if (Int32.TryParse(payload[3], out amt))
            //     {
            //         element.BorderThickness = amt;
            //         Debug.Log("added border amt");
            //     }
            //     else
            //     {
            //         string message = "Property 4 of the card background was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }
            // }

            // element.Refresh();

            // if (payload.Length > 4)
            // {
            //     string message = "Extra data was not parsed into the card background.";
            //     NotificationHandler.Print(message);
            // }

            #endregion

            element.ElementViewData = data;

            ColorUtility.TryParseHtmlString(data.ColorHex, out var color);
            element.BackgroundColor = color;
            element.Size = new Vector2((float)data.Scale[0], (float)data.Scale[1]);
            element.BorderThickness = (float)data.BorderThickness;

            element.Refresh();

            return true;
        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {
            
        }
        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardBackgroundElementData d)
            {
                return TrySetProperties(this, d);
            }

            return false;
        }
    }
}
