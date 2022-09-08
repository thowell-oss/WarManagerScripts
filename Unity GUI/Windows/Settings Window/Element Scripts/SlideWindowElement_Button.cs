/* SlideWindowElement_Button.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Button element for slide windows
    /// </summary>
    public class SlideWindowElement_Button : MonoBehaviour, ISlideWindow_Element
    {

        public GameObject MoreButton;

        /// <summary>
        /// The information to process
        /// </summary>
        public SlideWindow_Element_ContentInfo info { get; set; }

        public GameObject targetGameObject => this.gameObject;

        public string SearchContent => info.Label + info.ElementType;

        public Image IconImage;
        public TMPro.TMP_Text LabelText;

        public Image Background;

        private Action<int> _callback;

        public void UpdateElement()
        {
            IconImage.sprite = info.Sprite;

            if (IconImage.sprite == null)
                IconImage.color = Color.clear;
            else
                IconImage.color = Color.white;

            LabelText.text = info.Label;
            _callback = info.Callback;

            MoreButton.gameObject.SetActive(info.Button_PickMenuMore.Count > 0);
        }

        /// <summary>
        /// Called when the button is clicked
        /// </summary>
        public void OnClick()
        {
            if (_callback != null)
                _callback.Invoke(info.CallBackActionType);
        }

        public void RevealMore()
        {
            PickMenu.PickMenuManger.main.OpenPickMenu(info.Button_PickMenuMore, MoreButton.transform.position);
        }
    }
}
