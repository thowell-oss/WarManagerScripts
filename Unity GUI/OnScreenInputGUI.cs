using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarManager
{
    public class OnScreenInputGUI : MonoBehaviour
    {
        public RectTransform OnScreenObject;
        public TMPro.TMP_Text textAsset;
        public List<Sprite> mouseButtonSprites = new List<Sprite>();
        public Image MouseButtonImage;
        public RectTransform OnScreenLocation;
        public RectTransform OffScreenLocation;

        public bool useOnScreenInputGui;


        public string testText = "";
        public bool test = false;

        void Start() => TurnOff();

        private float timer = 1;
        public float time = 1;

        bool timerOn = false;

        bool closing = false;

        #region singleton

        public static OnScreenInputGUI main;

        void Awake()
        {
            if (main != null)
                Destroy(main);

            main = this;
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
            useOnScreenInputGui = GeneralSettings.UseOnScreenInputGui;

            if (Input.GetMouseButton(0))
            {
                PrintOnScreenInfo("mouse 0");
            }

            if (Input.GetMouseButton(1))
            {
                PrintOnScreenInfo("mouse 1");
            }

            if (Input.GetMouseButton(2))
            {
                PrintOnScreenInfo("mouse 2");
            }

            if (test)
            {
                PrintOnScreenInfo(testText);
                test = false;
            }

            if (timerOn)
            {
                time -= 1 * Time.deltaTime;

                if (time <= 0)
                {
                    TurnOff();
                }
            }
            else
            {
                time = 1;
            }
        }

        public void PrintOnScreenInfo(string info)
        {
            if (info.ToLower().StartsWith("mouse"))
            {
                textAsset.text = "";
                MouseButtonImage.gameObject.SetActive(true);

                if (info.EndsWith("0"))
                {
                    MouseButtonImage.sprite = mouseButtonSprites[0];
                }
                else if (info.EndsWith("1"))
                {
                    MouseButtonImage.sprite = mouseButtonSprites[1];
                }
                else
                {
                    MouseButtonImage.sprite = mouseButtonSprites[2];
                }
            }
            else
            {
                MouseButtonImage.gameObject.SetActive(false);
                textAsset.text = info;
            }

            ShowGUI();
        }

        public void ToggleGUI()
        {
            if (useOnScreenInputGui)
            {
                TurnOff();
            }
            else
            {
                ShowGUI();
            }
        }

        /// <summary>
        /// show the on screen input object
        /// </summary>
        void ShowGUI()
        {
            if (!useOnScreenInputGui)
                return;

            timerOn = false;
            time = 1;

            if (LeanTween.isTweening(OnScreenObject) && closing)
            {
                LeanTween.cancel(OnScreenObject);
                closing = false;
            }

            if (!LeanTween.isTweening(OnScreenObject))
            {
                LeanTween.move(OnScreenObject.gameObject, OnScreenLocation.position, .25f).setEaseInOutCubic();
            }

            timerOn = true;
        }

        private void TurnOff()
        {
            if (!LeanTween.isTweening(OnScreenObject))
            {
                closing = true;

                LeanTween.move(OnScreenObject.gameObject, OffScreenLocation.position, .25f).setEaseInOutCubic();
                MouseButtonImage.gameObject.SetActive(false);
                textAsset.text = "";
                timerOn = false;
            }
        }
    }
}