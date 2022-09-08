using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager.Help
{
    [Notes.Author("Controls showing different tips")]
    public class LoadingInfoSlider : MonoBehaviour
    {

        public Vector2 OpenScale;
        public Vector2 ClosedScale;

        public RectTransform Background;

        public List<GameObject> itemsInBackground = new List<GameObject>();

        public bool Closed = false;

        public bool Toggle = false;

        public float TransitionTime = .25f;

        public RectTransform Icon;

        public RectTransform IconOffLocation;
        public RectTransform IconOnLocation;

        public RectTransform Frame, FrameOffLocation, FrameOnLocation;

        public TMPro.TMP_Text TitleText;
        public TMPro.TMP_Text DescriptionText;

        public Color TitleColor;
        public Color DescriptionColor;

        void Update()
        {
            if (Toggle)
            {
                TogglePanel();
                Toggle = false;
            }
        }

        public void TogglePanel()
        {
            if (Closed)
            {
                Open();
            }
            else
            {
                Close();
            }
        }


        public void Open()
        {
            Closed = false;

            LeanTween.cancel(Icon.gameObject);
            LeanTween.cancel(Background.gameObject);

            LeanTween.value(Background.gameObject, SetScale, Background.sizeDelta, OpenScale, TransitionTime).setEaseOutCubic();

            LeanTween.delayedCall(TransitionTime, () =>
                {
                    SetItemsActive(!Closed);
                    LeanTween.value(Icon.gameObject, MoveIcon, (Vector2)IconOffLocation.anchoredPosition, (Vector2)IconOnLocation.anchoredPosition, TransitionTime).setEaseOutCirc();
                    LeanTween.value(Frame.gameObject, MoveFrame, (Vector2)FrameOffLocation.anchoredPosition, (Vector2)FrameOnLocation.anchoredPosition, TransitionTime * 2).setEaseOutCubic();
                    LeanTween.value(TitleText.gameObject, ChangeTitleTextColor, Color.clear, TitleColor, TransitionTime * 2).setEaseOutCubic();
                    LeanTween.value(DescriptionText.gameObject, ChangeDescriptionTextColor, Color.clear, DescriptionColor, TransitionTime * 2).setEaseOutCubic();
                });
        }

        public void Close()
        {
            Closed = true;

            LeanTween.cancel(Icon.gameObject);
            LeanTween.cancel(Background.gameObject);

            LeanTween.value(Background.gameObject, SetScale, Background.sizeDelta, ClosedScale, TransitionTime).setEaseInCubic();

            SetItemsActive(!Closed);

        }

        private void SetScale(Vector2 scale)
        {
            Background.sizeDelta = scale;
        }

        private void MoveIcon(Vector2 location)
        {
            Icon.anchoredPosition = location;
        }

        private void MoveFrame(Vector2 location)
        {
            Frame.anchoredPosition = location;
        }

        private void ChangeTitleTextColor(Color color)
        {
            TitleText.color = color;
        }

        private void ChangeDescriptionTextColor(Color color)
        {
            DescriptionText.color = color;
        }

        private void SetItemsActive(bool active)
        {
            foreach (var x in itemsInBackground)
            {
                x.gameObject.SetActive(active);
            }
        }
    }
}
