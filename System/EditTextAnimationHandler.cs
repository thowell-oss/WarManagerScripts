using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles the edit animation stuff
    /// </summary>
    [Notes.Author("Handle the edit animation stuff")]
    public class EditTextAnimationHandler : MonoBehaviour
    {
        /// <summary>
        /// the start scale
        /// </summary>
        public Vector2 _startScale;

        /// <summary>
        /// the ending scale
        /// </summary>
        public Vector2 endingScale;

        /// <summary>
        /// is the edit window animating?
        /// </summary>
        public bool Animating => LeanTween.isTweening(background.gameObject);

        /// <summary>
        /// Is the edit window open?
        /// </summary>
        public bool Open;

        public RectTransform background;

        public float scaleSpeed = .125f;

        public GameObject Buttons;
        public TMPro.TMP_InputField InputField;
        public GameObject Title;
        public GameObject KeyboardShortcuts;

        public GameObject BackgroundObj;

        // Start is called before the first frame update
        void Start()
        {
            _startScale = background.localScale;
            if (background.gameObject.activeInHierarchy)
                Animate_Close();
        }


        /// <summary>
        /// Toggle the animation
        /// </summary>
        public void ToggleAnimateBox()
        {
            if (Open)
            {
                Animate_Close();
            }
            else
            {
                Animate_Open();
            }
        }

        /// <summary>
        /// Animate open
        /// </summary>
        public void Animate_Open()
        {
            if (Animating)
            {
                LeanTween.cancel(background.gameObject);
            }

            LeanTween.value(background.gameObject, ScaleElement, background.sizeDelta, endingScale, scaleSpeed).setEaseOutBack();

            background.gameObject.SetActive(true);

            LeanTween.delayedCall(scaleSpeed, () =>
            {
                Buttons.SetActive(true);
                InputField.gameObject.SetActive(true);
                InputField.ActivateInputField();
                Title.gameObject.SetActive(true);
                KeyboardShortcuts.gameObject.SetActive(true);
                //BackgroundObj.gameObject.SetActive(true);
            });

            Open = true;
        }

        /// <summary>
        /// Animate close
        /// </summary>
        public void Animate_Close()
        {
            if (Animating)
                LeanTween.cancel(background.gameObject);

            LeanTween.value(background.gameObject, ScaleElement, background.sizeDelta, _startScale, scaleSpeed).setEaseInBack();

            Buttons.SetActive(false);
            InputField.gameObject.SetActive(false);
            Title.gameObject.SetActive(false);
            KeyboardShortcuts.gameObject.SetActive(false);
            //BackgroundObj.gameObject.SetActive(false);

            LeanTween.delayedCall(scaleSpeed, () =>
           {
               background.gameObject.SetActive(false);

           });

            Open = false;
        }

        /// <summary>
        /// scale the element
        /// </summary>
        /// <param name="scale">the scale</param>
        private void ScaleElement(Vector2 scale)
        {
            background.sizeDelta = scale;
        }
    }
}
