
/* Author: Taylor Howell
 * 
 */

using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D
{
    [ExecuteInEditMode()]
    /// <summary>
    /// The tooltip behavior script
    /// </summary>
    public class ToolTip : MonoBehaviour
    {
        public TextMeshProUGUI headerField;
        public TextMeshProUGUI contentField;

        public LayoutElement layout;

        public int headerCharacterWrapLimit;
        public int contentCharacterWrapLimit;

        public float _touchScreenPivotOffsetX = .025f;
        public float _touchScreenPivotOffsetY = .025f;

        RectTransform rect;

        Image image;

        public void Awake()
        {
            image = GetComponent<Image>();
            rect = GetComponent<RectTransform>();
        }


        /// <summary>
        /// Print the message on the tooltip
        /// </summary>
        /// <param name="header">header string</param>
        /// <param name="content">content string</param>
        public void Print(string header, string content)
        {
            image.color = new Color(0, 0, 0, 0);
            headerField.text = "";
            contentField.text = "";

            if (header == null || content == null)
            {
                ManageResizingTooltip();
                return;
            }

            float x = .25f;

            if (InputSystem.Main.InputMode == InputMode.Touch)
            {
                x = .0125f;
            }


            LeanTween.delayedCall(x, () =>
            {
                headerField.gameObject.SetActive(!string.IsNullOrEmpty(header));

                if (headerField.gameObject.activeSelf)
                    headerField.text = header;

                contentField.text = content;

                ManageResizingTooltip(x);

            });
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                ManageResizingTooltip();
            }

            Vector3 position = Input.mousePosition;

            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            if (pivotX < .5f)
            {
                if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                    pivotX = -.025f;

                if (InputSystem.Main.InputMode == InputMode.Touch)
                {
                    pivotX = -_touchScreenPivotOffsetX;
                }
            }
            else
            {
                if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                    pivotX = 1.025f;

                if (InputSystem.Main.InputMode == InputMode.Touch)
                {
                    pivotX = 1 + _touchScreenPivotOffsetX;
                }
            }

            if (pivotY < .5f)
            {
                if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                    pivotY = -.025f;

                if (InputSystem.Main.InputMode == InputMode.Touch)
                {
                    pivotY = -_touchScreenPivotOffsetY;
                }
            }
            else
            {
                if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                    pivotY = 1.025f;

                if (InputSystem.Main.InputMode == InputMode.Touch)
                {
                    pivotY = 1 + _touchScreenPivotOffsetY;
                }
            }

            rect.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        private void ManageResizingTooltip(float time = .25f)
        {
            int headerLen = headerField.text.Length;
            int contentLen = contentField.text.Length;

            layout.enabled = (headerLen > headerCharacterWrapLimit || contentLen > contentCharacterWrapLimit) ? true : false;

            LeanTween.value(this.gameObject, (Color c) => { image.color = c; }, new Color(0, 0, 0, 0), new Color(.03f, .03f, .04f, 1), time).setEaseInOutCubic();
        }

        void OnDisable()
        {
            LeanTween.cancel(gameObject);
        }
    }
}