using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager
{
    [RequireComponent(typeof(RectTransform))]
    [Notes.Author("Handles the top menu bar and tools bar")]
    public class MenuBarController : MonoBehaviour
    {
        public bool IsTouchOn;

        private bool currentTouch;

        public float NormalLength = 45f;
        public float TouchLength = 60f;

        public Vector2 ToolBarNormalPosition;
        public Vector2 ToolBarTouchPosition;

        RectTransform MenuBar;
        public RectTransform ToolBar;

        public GridLayoutGroup[] HorizontalButtonGroups;
        public VerticalLayoutGroup[] VerticalButtonGroups;

        public float TweenTime = .125f;
        public float StartTweenTime = .5f;
        private float timer;
        private bool start = false;
        private bool startTween = true;

        private bool tweening = false;

        private void Awake()
        {
            MenuBar = GetComponent<RectTransform>();
        }

        void Start()
        {
            foreach (GridLayoutGroup g in HorizontalButtonGroups)
            {
                if (g != null)
                    g.spacing = new Vector2(-20, 0);
            }

            foreach (VerticalLayoutGroup g in VerticalButtonGroups)
            {
                if (g != null)
                    g.spacing = -20;
            }

            currentTouch = !IsTouchOn;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentTouch == IsTouchOn && !tweening)
            {
                start = true;
                return;
            }

            if (start)
            {
                timer = 0;
                start = false;
                tweening = true;
            }

            if (IsTouchOn)
            {
                MenuBar.sizeDelta = Vector2.Lerp(MenuBar.sizeDelta, new Vector2(MenuBar.sizeDelta.x, TouchLength), .4f);
                ToolBar.anchoredPosition = Vector2.Lerp(ToolBar.anchoredPosition, ToolBarTouchPosition, .2f);
                ToolBar.sizeDelta = Vector2.Lerp(ToolBar.sizeDelta, new Vector2(TouchLength, NormalLength * 8), .4f);

                foreach (GridLayoutGroup g in HorizontalButtonGroups)
                {
                    g.spacing = Vector2.Lerp(g.spacing, new Vector2(NormalLength, 0), .2f);
                }

                foreach (VerticalLayoutGroup g in VerticalButtonGroups)
                {
                    g.spacing = Mathf.Lerp(g.spacing, NormalLength, .2f);
                }
            }
            else
            {
                MenuBar.sizeDelta = Vector2.Lerp(MenuBar.sizeDelta, new Vector2(MenuBar.sizeDelta.x, NormalLength), .4f);
                ToolBar.anchoredPosition = Vector2.Lerp(ToolBar.anchoredPosition, ToolBarNormalPosition, .2f);
                ToolBar.sizeDelta = Vector2.Lerp(ToolBar.sizeDelta, new Vector2(NormalLength, NormalLength * 6), .4f);

                foreach (GridLayoutGroup g in HorizontalButtonGroups)
                {
                    g.spacing = Vector2.Lerp(g.spacing, new Vector2(NormalLength / 2, 0), .2f);
                }

                foreach (VerticalLayoutGroup g in VerticalButtonGroups)
                {
                    g.spacing = Mathf.Lerp(g.spacing, NormalLength / 2, .2f);
                }
            }

            if (Input.touchCount > 0 && !IsTouchOn)
                ToggleTouch();

            timer += 1 * Time.deltaTime;

            if (startTween)
            {
                if (timer >= StartTweenTime)
                {
                    currentTouch = IsTouchOn;
                    startTween = false;
                    tweening = false;
                }
            }
            else
            {
                if (timer >= TweenTime)
                {
                    currentTouch = IsTouchOn;
                    tweening = false;
                }
            }

        }

        /// <summary>
        /// Touch systems toggle on and off
        /// </summary>
        public void ToggleTouch()
        {
            IsTouchOn = !IsTouchOn;
        }

        public void SetInputMode(InputMode mode)
        {
            if (mode == InputMode.MouseAndKeyboard)
            {
                IsTouchOn = false;
            }
            else
            {
                IsTouchOn = true;
            }
        }

        public void OnEnable()
        {
            InputSystem.OnChangeInputMode += SetInputMode;
        }

        public void OnDisable()
        {
            InputSystem.OnChangeInputMode -= SetInputMode;
        }
    }
}
