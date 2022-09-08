using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

using WarManager;

namespace WarManager.Unity3D
{
    public class QuickMenuBehavior : MonoBehaviour
    {

        [TabGroup("Main")]
        public bool MenuActive = true;

        [TabGroup("Animation")]
        public float animSpeed = .25f;

        [TabGroup("Animation")]
        public bool Animating => LeanTween.isTweening(rect.gameObject);

        [TabGroup("Animation")]
        public Vector2 endingScale;

        [TabGroup("Animation")]
        public Vector2 StartingScale;

        [TabGroup("Touch Screen")]
        public float _touchScreenPivotOffsetX = .025f;
        [TabGroup("Touch Screen")]
        public float _touchScreenPivotOffsetY = .025f;

        [TabGroup("Object Reference")]
        public GameObject Content;

        [TabGroup("Object Reference")]
        public RectTransform rect;
        
        
        public void Start()
        {
            DisableMenu();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (MenuActive)
                    DisableMenu();
                else
                    EnableMenu();
            }
        }

        public void EnableMenu()
        {
            Animate_Open();
        }

        public void DisableMenu()
        {
            Animate_Close();
        }


        private void SetMenuLocation()
        {
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
            rect.transform.position = position;
        }

        /// <summary>
        /// Animate open
        /// </summary>
        public void Animate_Open()
        {
            if (Animating)
            {
                LeanTween.cancel(rect.gameObject);
            }

            LeanTween.value(rect.gameObject, ScaleElement, rect.sizeDelta, endingScale, animSpeed).setEaseOutCubic();

            rect.gameObject.SetActive(true);

            LeanTween.delayedCall(animSpeed, () =>
            {
                Content.SetActive(true);
            });

            MenuActive = true;
        }

        /// <summary>
        /// Animate close
        /// </summary>
        public void Animate_Close()
        {
            if (Animating)
                LeanTween.cancel(rect.gameObject);

            LeanTween.value(rect.gameObject, ScaleElement, rect.sizeDelta, StartingScale, animSpeed).setEaseInCubic();

            Content.SetActive(false);

            LeanTween.delayedCall(animSpeed, () =>
           {
               rect.gameObject.SetActive(false);
           });

            MenuActive = false;
        }

        /// <summary>
        /// scale the element
        /// </summary>
        /// <param name="scale">the scale</param>
        private void ScaleElement(Vector2 scale)
        {
            rect.sizeDelta = scale;
        }
    }
}
