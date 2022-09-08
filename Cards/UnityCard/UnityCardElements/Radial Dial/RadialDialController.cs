using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace WarManager
{
    public class RadialDialController : MonoBehaviour
    {

        public Image DialImage;
        public Image BackroundImage;
        public TMP_Text percentText;

        public int TextFontSize;
        public Color DialColor;
        public Color BackgroundColor;
        public Color TextColor;

        private bool refreshing = false;
        public bool Set = false;
        public float finalAmt = 0;

        // // Update is called once per frame
        // void Update()
        // {
        //     if (Set)
        //     {
        //         LeanTween.value(gameObject, AnimateFillAmt, DialImage.fillAmount, finalAmt, .25f).setEaseOutBack();
        //         LeanTween.value(gameObject, AnimateColor, DialImage.color, DialColor, .25f).setEaseInExpo();
        //         RefreshDial();
        //         Set = false;
        //     }
        // }

        /// <summary>
        /// Set the dial values and refresh it
        /// </summary>
        /// <param name="currentAmount">the current amount</param>
        /// <param name="maxValue">the amount it takes for the dial to be full</param>
        public void SetDial(float currentAmount, float minValue, float maxValue = 100)
        {
            currentAmount = Mathf.Clamp(currentAmount, minValue, maxValue);

            if (maxValue < minValue + 1)
                maxValue = minValue + 1;

            float result = (currentAmount - minValue) / (maxValue - minValue);

            finalAmt = result;

            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, AnimateFillAmt, DialImage.fillAmount, finalAmt, .5f).setEaseOutBack();
            LeanTween.value(gameObject, AnimateColor, DialImage.color, DialColor, .5f).setEaseInExpo();
            RefreshDial();
        }


        /// <summary>
        /// Set the dial to display that there is an error
        /// </summary>
        public void SetDialAsError()
        {
            finalAmt = 0;
            RefreshDial();
            percentText.text = "ERROR";
        }

        /// <summary>
        /// Refresh the dial values
        /// </summary>
        public void RefreshDial()
        {
            percentText.text = string.Format("{0:0.00%}", finalAmt);

            BackroundImage.color = BackgroundColor;

            percentText.fontSize = TextFontSize;

            percentText.color = TextColor;
        }

        private void AnimateFillAmt(float x)
        {
            DialImage.fillAmount = x;
        }

        private void AnimateColor(Color col)
        {
            DialImage.color = col;
        }
    }
}
