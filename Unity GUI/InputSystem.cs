using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WarManager
{

    [Notes.Author("Handles the input system for war manager")]
    public class InputSystem : MonoBehaviour
    {
        public float DoublePressTime;
        public float PressTime;

        public InputMode InputMode { get; private set; }

        public delegate void changeInputMode_delegate(InputMode mode);

        /// <summary>
        /// Event is called when a different input mode is detected
        /// </summary>
        public static event changeInputMode_delegate OnChangeInputMode;

        #region singleton pattern
        public static InputSystem Main;

        public void Awake()
        {
            if (Main != null)
            {
                Destroy(this);
                Debug.LogError("There is another touch system in the scene");
            }
            else
            {
                Main = this;
            }
        }

        #endregion

        void Start()
        {
            // Input.simulateMouseWithTouches = false;

            if (OnChangeInputMode != null)
            {
                OnChangeInputMode(InputMode);
            }
        }

        void Update()
        {
            if (Input.touchCount > 0 && InputMode != InputMode.Touch)
            {
                ChangeInputMode(InputMode.Touch);

                // Debug.Log("Input mode changed");
            }

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && InputMode != InputMode.MouseAndKeyboard && Input.touchCount < 1)
            {
                ChangeInputMode(InputMode.MouseAndKeyboard);

                // Debug.Log("Input mode changed");
            }

            if ((Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && InputMode != InputMode.MouseAndKeyboard && Input.touchCount < 1)
            {
                ChangeInputMode(InputMode.MouseAndKeyboard);
            }
        }


        /// <summary>
        /// Change the input mode and call the event
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeInputMode(InputMode mode)
        {
            InputMode = mode;

            if (OnChangeInputMode != null)
            {
                OnChangeInputMode(mode);
            }
        }
    }

    public enum InputMode
    {
        MouseAndKeyboard,
        Touch,

    }
}
