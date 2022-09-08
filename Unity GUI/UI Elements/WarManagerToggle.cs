

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D
{

    public class WarManagerToggle : MonoBehaviour
    {
        [SerializeField]
        public bool isOn = true;

        [SerializeField]
        private Slider slider;

        public Color onColor;
        public Color OffColor;

        [SerializeField]
        private RectTransform image;

        public UnityEngine.Events.UnityEvent<bool> OnToggle;

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            Init(true);
        }

        public void Init(bool on)
        {

            if (on)
            {
                Toggle();
                Toggle();
            }
            else
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            isOn = !isOn;

            if (OnToggle != null)
                OnToggle.Invoke(isOn);

            if (isOn)
            {
                LeanTween.value(this.gameObject, leanSlider, 0, 1, .15f).setEaseOutCubic();
                LeanTween.color(image, onColor, .15f);

            }
            else
            {
                LeanTween.value(this.gameObject, leanSlider, 1, 0, .15f).setEaseOutCubic();
                LeanTween.color(image, OffColor, .15f);

            }
        }

        private void leanSlider(float value)
        {
            slider.value = value;
        }
      
    }
}
