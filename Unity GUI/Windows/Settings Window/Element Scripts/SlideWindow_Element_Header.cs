/* SlideWindow_Element_Header.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    public class SlideWindow_Element_Header : MonoBehaviour, ISlideWindow_Element
    {
        public TMPro.TMP_Text LabelText;

        public SlideWindow_Element_ContentInfo info { get; set; }

        public GameObject targetGameObject => this.gameObject;

        public string SearchContent => info.Label + info.ElementType;

        public void UpdateElement()
        {
            LabelText.text = info.Label;

            if (info.Height > 12)
            {
                LabelText.enableAutoSizing = false;
                LabelText.fontSize = info.Height;
            }
            else
            {
                LabelText.enableAutoSizing = true;
                LabelText.fontSize = info.Height;
            }
        }

        public override string ToString()
        {
            return info.ToString() + " (" + info.ElementID + ") ";
        }
    }
}
