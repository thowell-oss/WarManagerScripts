/* ContextMenuAction.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Sirenix.OdinInspector;


namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles the context menu button behavior
    /// </summary>
    public class ContextMenuAction : MonoBehaviour
    {

        /// <summary>
        /// active state sprite
        /// </summary>
        [TabGroup("Sprites")]
        public Sprite ActiveSprite;

        /// <summary>
        /// passive state sprite
        /// </summary>
        [TabGroup("Sprites")]
        public Sprite PassiveSprite;

        /// <summary>
        /// Active Color
        /// </summary>
        [TabGroup("Color")]
        public Color ActiveColor = Color.white;

        /// <summary>
        /// mixed state sprite
        /// </summary>
        [TabGroup("Sprites")]
        public Sprite MixedSprite;

        /// <summary>
        /// Passive Color
        /// </summary>
        [TabGroup("Color")]
        public Color PassiveColor = Color.gray;

        /// <summary>
        /// Disabled image
        /// </summary>
        [TabGroup("Color")]
        public Color MixedColor = Color.gray;

        /// <summary>
        /// the image
        /// </summary>
        public Image _image;

        /// <summary>
        /// The disabled image
        /// </summary>
        public Image DisabledImage;

        /// <summary>
        /// The button
        /// </summary>
        public Button _button;

        public UnityEvent<ContextMenuButtonState> OnActivate;

        /// <summary>
        /// The action that will be called when the button is pressed
        /// </summary>
        private Action<ContextMenuButtonState> _callBack;

        /// <summary>
        /// The state the button is in
        /// </summary>
        /// <value></value>
        public ContextMenuButtonState CurrentState { get; private set; }


        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="callBack"></param>
        public void Init(Action<ContextMenuButtonState> callBack)
        {
            if (_image == null)
                GetComponent<Image>();

            if (_button == null)
                GetComponent<Button>();

            if (callBack == null)
                throw new NullReferenceException("The call back cannot be null");

            _callBack = callBack;
        }

        /// <summary>
        /// Init the button
        /// </summary>
        /// <param name="callBack">the call back</param>
        /// <param name="active">the active sprite</param>
        public void Init(Action<ContextMenuButtonState> callBack, Sprite active, Sprite passive)
        {
            if (_image == null)
                GetComponent<Image>();

            if (_button == null)
                GetComponent<Button>();

            if (callBack == null)
                throw new NullReferenceException("The call back cannot be null");

            _callBack = callBack;
            ActiveSprite = active;
            PassiveSprite = passive;
        }

        /// <summary>
        /// Set the state of the button
        /// </summary>
        /// <param name="currentState">the state the button needs to reflect</param>
        public void SetState(ContextMenuButtonState currentState)
        {
            if (_image == null)
                GetComponent<Image>();

            if (_button == null)
                GetComponent<Button>();

            CurrentState = currentState;

            switch (currentState)
            {
                case ContextMenuButtonState.Active:
                    SetActiveState();
                    break;

                case ContextMenuButtonState.Passive:
                    SetPassiveState();
                    break;

                case ContextMenuButtonState.Mixed:
                    SetMixedState();
                    break;

                default:
                    SetDisabledState();
                    break;

            }
        }

        /// <summary>
        /// Set the state to an active state (on)
        /// </summary>
        private void SetActiveState()
        {

            DisabledImage.gameObject.SetActive(false);
            _image.enabled = true;

            _button.interactable = true;

            if (ActiveSprite != null)
                _image.sprite = ActiveSprite;

            _image.color = ActiveColor;
        }

        /// <summary>
        /// Set the state to a passive state (off)
        /// </summary>
        private void SetPassiveState()
        {
            DisabledImage.gameObject.SetActive(false);
            _image.enabled = true;

            _button.interactable = true;

            if (PassiveSprite != null)
                _image.sprite = PassiveSprite;

            _image.color = PassiveColor;
        }

        /// <summary>
        /// Set the state to a both passive and active state (if on and off)
        /// </summary>
        private void SetMixedState()
        {

            _button.interactable = true;
            if (MixedSprite != null)
                _image.sprite = MixedSprite;
        }

        /// <summary>
        /// Set disabled state (disable the button)
        /// </summary>
        private void SetDisabledState()
        {
            DisabledImage.gameObject.SetActive(true);
            _image.enabled = false;

            _button.interactable = false;
            //_image.sprite = MixedSprite;

            DisabledImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Call the action (button press)
        /// </summary>
        public void CallAction()
        {
            if (_callBack != null)
            {
                _callBack(CurrentState);
            }

            if (OnActivate != null)
            {
                OnActivate.Invoke(CurrentState);
            }
        }
    }

    /// <summary>
    /// Each state the contextMenu button could be in
    /// </summary>
    public enum ContextMenuButtonState
    {
        Active,
        Passive,
        Mixed,
        Disabled,
    }
}
