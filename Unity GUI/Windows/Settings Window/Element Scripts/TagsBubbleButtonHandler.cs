
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

namespace WarManager.Unity3D
{
    public class TagsBubbleButtonHandler : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text bubbleText;
        private Action _buttonClickAction;

        /// <summary>
        /// Set the bubble text and callback
        /// </summary>
        /// <param name="text">the text</param>
        /// <param name="btnClickCallBack">the call back action</param>
        public void SetBubble(string text, Action btnClickCallBack)
        {
            bubbleText.text = text;
            _buttonClickAction = btnClickCallBack;
        }
           
        /// <summary>
        /// the on click button method handler
        /// </summary>
        public void OnClick()
        {
            _buttonClickAction();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
