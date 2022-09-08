/* Author: Taylor Howell
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles tooltip display system
    /// </summary>
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem main;

        public ToolTip currentToolTip;

        public float delayTime = .5f;

        private bool showing = false;

        public void Awake()
        {
            main = this;
        }


        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Hide();
            }
        }

        public void Show(string header, string content)
        {

            float x = delayTime;

            if (InputSystem.Main.InputMode == InputMode.Touch)
            {
                x = 0;
            }

            LeanTween.delayedCall(this.gameObject, delayTime, () =>
            {
                currentToolTip.gameObject.SetActive(true);
                currentToolTip.Print(header, content);
                showing = true;
            });
        }

        public void Hide()
        {
            LeanTween.cancel(this.gameObject);
            currentToolTip.gameObject.SetActive(false);

            showing = false;
        }
    }
}
