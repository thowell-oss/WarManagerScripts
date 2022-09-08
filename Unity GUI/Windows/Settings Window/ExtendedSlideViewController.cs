using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;
using UnityEngine.EventSystems;

using WarManager;
using WarManager.Unity3D;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// the slide controller which dictates the menu behavior
    /// </summary>
    [Notes.Author("The slide controller which dictates the menu behavior")]
    public class ExtendedSlideViewController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        /// <summary>
        /// is the menu open?
        /// </summary>
        public bool IsOpen = false;

        /// <summary>
        /// the transition speed of off and on for speed scaling purposes
        /// </summary>
        public float TransitionSpeed = 1;

        /// <summary>
        /// off and on location
        /// </summary>
        public RectTransform OffLocation, OnLocation;

        /// <summary>
        /// the items to toggle on and off when opening and closing the menu
        /// </summary>
        /// <typeparam name="RectTransform"></typeparam>
        /// <returns></returns>
        public List<RectTransform> items = new List<RectTransform>();

        /// <summary>
        /// the rect transform of the object (for scaling, and moving)
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        ///  used for testing with the Update() method - toggles the menu on and off
        /// </summary>
        public bool toggle;

        /// <summary>
        /// the master slide window controller
        /// </summary>
        public SlideWindowController MasterSlideWindow;

        /// <summary>
        /// The scroll rect;
        /// </summary>
        public ScrollRect _scrollRect;

        /// <summary>
        /// the on scale of the menu
        /// </summary>
        public Vector2 OnScale;

        /// <summary>
        /// off scale of the menu
        /// </summary>
        public Vector2 OffScale;

        /// <summary>
        /// the reveal menu
        /// </summary>
        public Image RevealImage;

        /// <summary>
        /// the on and off colors of the reveal menu
        /// </summary>
        public Color OffColor, OnColor;

        /// <summary>
        /// Can the user search?
        /// </summary>
        /// <value></value>
        public bool CanSearch { get; set; }

        /// <summary>
        /// The can search unity event
        /// </summary>
        public UnityEvent<bool> CanSearchEvent;

        private Vector2 ResetContentAnchoredPosition;

        /// <summary>
        /// Called once when the item is created
        /// </summary>
        void Start()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();


            rectTransform.position = OffLocation.position;
            IsOpen = false;

            ResetContentAnchoredPosition = _scrollRect.content.anchoredPosition;
        }

        /// <summary>
        /// called once every frame
        /// </summary>
        void Update()
        {
            if (toggle && !MasterSlideWindow.Closed)
            {
                toggle = false;

                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }

            if (MasterSlideWindow.Closed && IsOpen)
            {
                Close();
            }
        }

        /// <summary>
        /// Toggle the menu on
        /// </summary>
        /// <param name="location"></param>
        public void ToggleOn(Vector3 location)
        {
            OnLocation.transform.position = location;
            Open();
        }

        /// <summary>
        /// Open the menu
        /// </summary>
        public void Open()
        {

            if (IsOpen)
                return;

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            SetItemsActive(false);

            rectTransform.position = OffLocation.position;
            rectTransform.sizeDelta = OffScale;

            RevealImage.color = OffColor;

            LeanTween.value(rectTransform.gameObject, MoveRect, rectTransform.position, OnLocation.position, TransitionSpeed).setEaseInOutCubic();
            LeanTween.value(rectTransform.gameObject, ScaleRect, rectTransform.sizeDelta, OnScale, TransitionSpeed).setEaseInOutCubic();

            LeanTween.delayedCall(TransitionSpeed, () =>
                {
                    _scrollRect.content.anchoredPosition = ResetContentAnchoredPosition;
                    SetItemsActive(true);
                    IsOpen = true;

                    LeanTween.value(RevealImage.gameObject, SetRevealColor, RevealImage.color, OnColor, TransitionSpeed / 2).setEaseInOutCubic();

                    CanSearchEvent?.Invoke(true);
                });
        }

        /// <summary>
        /// Close the menu
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
                return;

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            CanSearchEvent?.Invoke(false);

            SetItemsActive(false);
            IsOpen = false;

            LeanTween.value(rectTransform.gameObject, MoveRect, rectTransform.position, OffLocation.position, TransitionSpeed).setEaseInOutCubic();
            LeanTween.value(rectTransform.gameObject, ScaleRect, rectTransform.sizeDelta, OffScale, TransitionSpeed).setEaseInOutCubic();

        }

        #region set properties for lean tween

        /// <summary>
        /// move the rect of the extended view controller
        /// </summary>
        /// <param name="location"></param>
        private void MoveRect(Vector3 location)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            rectTransform.position = location;
        }

        /// <summary>
        /// scale the rect of the slide view window
        /// </summary>
        /// <param name="scale"></param>
        private void ScaleRect(Vector2 scale)
        {
            rectTransform.sizeDelta = scale;
        }

        /// <summary>
        /// Set the color of the reveal item
        /// </summary>
        /// <param name="color"></param>
        private void SetRevealColor(Color color)
        {
            RevealImage.color = color;
        }

        #endregion

        /// <summary>
        /// Set the items active (or not)
        /// </summary>
        /// <param name="active">set the items active (or not)</param>
        private void SetItemsActive(bool active)
        {
            foreach (var item in items)
            {
                item.gameObject.SetActive(active);
            }
        }

        #region Unity events

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolsManager.SelectedTool = ToolTypes.None;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        }

        #endregion
    }
}
