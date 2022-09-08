
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;


namespace WarManager.Unity3D
{
    public class TextEditModalWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Is the window Open?
        /// </summary>
        public bool isOpen { get; private set; }

        public EditTextAnimationHandler animHandler;

        [Space]

        public GameObject YesButton;
        public GameObject NoButton;

        private string cancelText;

        [Space]
        public GameObject AntiGuiBackground;
        public GameObject Window;
        [Space]
        public Transform offLocation;
        private float _onLocation = 0;
        private float _time = .25f;

        [Space]
        public TMPro.TMP_Text Title;
        public TMPro.TMP_InputField MultilineInput, SingleLineInput;

        public WarManagerCameraController warManagerCameraController;

        bool animating = false;

        public bool UseAnimation = true;

        /// <summary>
        /// Has the callback been executed?
        /// </summary>
        private bool _callbackExcecuted = false;

        /// <summary>
        /// The call back reference when answering a question
        /// </summary>
        private Action<string> _callBack;


        void Update()
        {
            if (isOpen)
            {
                if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
                {
                    if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    {
                        Yes();
                    }
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    No();
                }

                if (ToolsManager.SelectedTool != ToolTypes.None && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
                {
                    No();
                }
            }
        }

        /// <summary>
        /// Prepares and opens the message box
        /// </summary>
        /// <param name="contextTitle">the title of the message box</param>
        /// <param name="message">the message of the message box</param>
        public void SetText(string currentText, string contextTitle = "", bool multilineInput = false)
        {
            cancelText = currentText;

            if (Title != null)
                Title.text = contextTitle;

            _callbackExcecuted = false;

            // if (multilineInput)
            // {
            //     if (SingleLineInput != null)
            //         SingleLineInput.gameObject.SetActive(false);
            //     MultilineInput.gameObject.SetActive(true);
            //     MultilineInput.text = currentText;
            //     MultilineInput.ActivateInputField();
            // }
            // else
            // {
            //     SingleLineInput.gameObject.SetActive(true);
            //     MultilineInput.gameObject.SetActive(false);
            //     SingleLineInput.text = currentText;
            //     SingleLineInput.ActivateInputField();
            // }


            LeanTween.delayedCall(.25f, () =>
            {
                Open();
            });
        }

        /// <summary>
        /// Prepares the and opens the message box to answer a question
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="title">the title</param>
        /// <param name="callBack">the action call back to answer the question</param>
        public void ShowMessage(string message, string title, Action<string> callBack, bool password)
        {
            if (Title != null)
                Title.text = title;

            //dDebug.Log("message");

            _callbackExcecuted = false;

            if (callBack == null)
                throw new NullReferenceException("the call back cannot be null");

            _callBack = callBack;

            if (password)
            {
                MultilineInput.contentType = TMPro.TMP_InputField.ContentType.Password;
            }
            else
            {
                MultilineInput.contentType = TMPro.TMP_InputField.ContentType.Autocorrected;
            }

            if (_callBack != null)
                SetText(message, title, true);
        }

        /// <summary>
        /// Close the window
        /// </summary>
        void Close(bool accept)
        {


            if (ToolsManager.SelectedTool == ToolTypes.None)
                ToolsManager.SelectTool(ToolsManager.PreviousTool, true);

            //Debug.Log("animating");

            animating = true;

            if (UseAnimation)
            {
                LeanTween.moveLocalX(Window, offLocation.position.x, _time).setEaseOutSine();
                LeanTween.alpha(AntiGuiBackground, 0, _time);

                LeanTween.delayedCall(_time, () => AntiGuiBackground.SetActive(false));
                LeanTween.delayedCall(_time, () => Window.SetActive(false));


                //Debug.Log("removing window");
            }

            if (accept)
            {
                if (MultilineInput.gameObject.activeInHierarchy)
                {
                    //Debug.Log("excecuting callback multiline " + _callbackExcecuted);

                   // Debug.Log(_callBack == null);

                    if (!_callbackExcecuted && _callBack != null)
                    {
                        //Debug.Log("excecuting");
                        //Debug.Log("firing call back");
                        _callBack(MultilineInput.text);
                    }
                }
                else
                {
                    Debug.Log("excecuting callback single line");

                    if (!_callbackExcecuted && _callBack != null)
                        _callBack = (x) => { x = SingleLineInput.text; };
                }
                isOpen = false;
            }
            else
            {
                if (!_callbackExcecuted && _callBack != null)
                    _callBack = (x) => { x = cancelText; };
            }

            // ToolsManager.SelectTool(ToolsManager.PreviousTool, true);

            if (warManagerCameraController != null)
                warManagerCameraController.InMenu = false;

            //Debug.Log("Close");
            animHandler.Animate_Close();
        }

        /// <summary>
        /// Open the window
        /// </summary>
        void Open()
        {

            animHandler.Animate_Open();

            // Debug.Log("opened the modal window");

            if (_callBack == null)
            {
                Close(false);
            }

            if (warManagerCameraController != null)
                warManagerCameraController.InMenu = true;

            ToolsManager.SelectedTool = ToolTypes.None;

            if (UseAnimation)
            {
                YesButton.SetActive(true);
                NoButton.SetActive(true);

                LeanTween.moveLocalX(Window, _onLocation, _time).setEaseOutSine();
                LeanTween.alpha(AntiGuiBackground, 1, _time);

                Window.SetActive(true);
                AntiGuiBackground.SetActive(true);
            }

            isOpen = true;

            LeanTween.delayedCall(1, () =>
            {
                animating = false;
            });

        }


        /// <summary>
        /// The user answer no to the modal question (or okay)
        /// </summary>
        public void No()
        {
            Close(false);
            _callbackExcecuted = false;
        }

        /// <summary>
        /// The user answered yes to the modal question
        /// </summary>
        public void Yes()
        {
            Close(true);
            _callbackExcecuted = true;
        }

        public void OnEnable()
        {
            EditTextMessageBoxController.OnEditText += ShowMessage;
            //Debug.Log("enabled");
        }

        public void OnDisable()
        {
            if (isOpen)
                Close(false);

            EditTextMessageBoxController.OnEditText -= ShowMessage;

            LeanTween.cancelAll();

            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);

            if (warManagerCameraController != null)
                warManagerCameraController.InMenu = false;


            //Debug.Log("disabled");

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ToolsManager.SelectedTool != ToolTypes.None)
                ToolsManager.SelectedTool = ToolTypes.None;

            if (warManagerCameraController != null)
                warManagerCameraController.InMenu = true;

            // Debug.Log("pointer enter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ToolsManager.SelectedTool == ToolTypes.None)
                ToolsManager.SelectTool(ToolsManager.PreviousTool, true);

            if (warManagerCameraController != null)
                warManagerCameraController.InMenu = false;


            // Debug.Log("pointer exit");
        }
    }
}
