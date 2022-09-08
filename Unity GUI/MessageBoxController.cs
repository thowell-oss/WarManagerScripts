/* MessageBox.cs
 * Author: Taylor Howell
 */

using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace WarManager.Unity3D
{
    /// <summary>
    /// The message box game object class 
    /// </summary>
    public class MessageBoxController : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// Is the window Open?
        /// </summary>
        public bool isOpen { get; private set; }

        [Space]

        public GameObject YesButton;
        public GameObject NoButton, OkayButton;

        public GameObject CopyButton;

        [Space]
        public GameObject AntiGuiBackground;
        public GameObject Window;

        public GameObject DarkBackround;

        [Space]
        public Transform offLocation;
        private float _onLocation = 0;
        private float _time = .125f;

        [Space]
        public TMPro.TMP_Text Title;
        public TMPro.TMP_Text Message;

        LTDescr mov, a, d1, d2;

        /// <summary>
        /// Has the callback been executed?
        /// </summary>
        private bool _callbackExcecuted = false;

        /// <summary>
        /// The call back reference when answering a question
        /// </summary>
        private Action<bool> _callBack;

        [SerializeField] RectTransform messageBoxRect;
        public float scaleXAmt;
        public float scaleOffXAmt;

        public float scaleYAmt;
        public float scaleOffYAmt;

        /// <summary>
        /// Prepares and opens the message box
        /// </summary>
        /// <param name="title">the title of the message box</param>
        /// <param name="message">the message of the message box</param>
        public void ShowMessage(string message, string title = "Notice")
        {
            Title.text = title;
            Message.text = message;

            _callbackExcecuted = false;

            _callBack = null;

            Open();
        }


        /// <summary>
        /// Interrupts what the message box might be displaying to show this message instead
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="title">the title of the message</param>
        public void ShowMessage_Interrupt(string message, string title = "Notice")
        {
            if (isOpen)
            {
                Close(false);
            }

            ShowMessage(message, title);
        }

        /// <summary>
        /// Prepares the and opens the message box to answer a question
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="title">the title</param>
        /// <param name="callBack">the action call back to answer the question</param>
        public void ShowMessage(string message, string title, Action<bool> callBack)
        {

            Title.text = title;
            Message.text = message;

            _callbackExcecuted = false;

            _callBack = callBack;

            Open();

        }

        /// <summary>
        /// Interrupts what the message box might be displaying to show this message instead
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="title">the title of the message</param>
        /// <param name="callBack">the call back to answer the message</param>
        public void ShowMessage_Interrupt(string message, string title, Action<bool> callBack)
        {
            if (isOpen)
            {
                Close(false);
            }

            ShowMessage(message, title, callBack);
        }

        /// <summary>
        /// Close the window
        /// </summary>
        void Close(bool isBoxReady)
        {
            Message.gameObject.SetActive(false);
            Title.gameObject.SetActive(false);

            CopyButton.gameObject.SetActive(false);

            OkayButton.SetActive(false);
            YesButton.SetActive(false);
            NoButton.SetActive(false);

            DarkBackround.SetActive(false);

            messageBoxRect.LeanSize(new Vector2(scaleOffXAmt, scaleOffYAmt), .3f).setEaseInBack();
            a = LeanTween.alpha(AntiGuiBackground, 0, .25f);

            // LeanTween.delayedCall(.25f, () =>
            // {
            //messageBoxRect.LeanSize(new Vector2(scaleOffXAmt, scaleOffYAmt), .25f).setEaseInOutBack();
            d1 = LeanTween.delayedCall(_time, () => AntiGuiBackground.SetActive(false));

            LeanTween.delayedCall(.3f, () =>
            {
                mov = LeanTween.moveLocalX(Window, offLocation.position.x, .01f).setEaseInExpo();

                d2 = LeanTween.delayedCall(_time, () => Window.SetActive(false));


                if (!_callbackExcecuted && _callBack != null)
                    _callBack(false);

                isOpen = false;

                if (isBoxReady)
                {
                    MessageBoxHandler.SetMessageBoxReady();
                }


            });
            // });

        }

        /// <summary>
        /// Open the window
        /// </summary>
        void Open()
        {

            Window.SetActive(true);

            AntiGuiBackground.SetActive(true);

            // Debug.Log("opened");

            isOpen = true;

            OkayButton.SetActive(false);
            YesButton.SetActive(false);
            NoButton.SetActive(false);

            DarkBackround.SetActive(true);

            messageBoxRect.LeanSize(new Vector2(scaleOffXAmt, scaleOffYAmt), .01f);

            mov = LeanTween.moveLocalX(Window, _onLocation, .01f);
            a = LeanTween.alpha(AntiGuiBackground, 1, .25f);

            messageBoxRect.LeanSize(new Vector2(scaleXAmt, scaleYAmt), .35f).setEaseOutBack();

            // LeanTween.delayedCall(.25f, () =>
            // {
            //     messageBoxRect.LeanSize(new Vector2(scaleXAmt, scaleYAmt), .25f).setEaseInOutBack();


            LeanTween.delayedCall(.35f, () =>
            {
                Title.gameObject.SetActive(true);
                Message.gameObject.SetActive(true);

                ToolsManager.SelectedTool = ToolTypes.None;

                CopyButton.gameObject.SetActive(true);

                if (_callBack != null)
                {
                    OkayButton.SetActive(false);
                    YesButton.SetActive(true);
                    NoButton.SetActive(true);
                }
                else
                {
                    OkayButton.SetActive(true);
                    YesButton.SetActive(false);
                    NoButton.SetActive(false);
                }
            });
            // });
        }

        /// <summary>
        /// The user answer no to the modal question (or okay)
        /// </summary>
        public void No()
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
            ToolsManager.Mode = WarMode.Sheet_Editing;

            Close(true);
        }

        /// <summary>
        /// The user answered yes to the modal question
        /// </summary>
        public void Yes()
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
            ToolsManager.Mode = WarMode.Sheet_Editing;

            _callBack(true);
            _callbackExcecuted = true;

            Close(true);
        }

        public void CopyMessage()
        {
            TextEditor editor = new TextEditor
            {
                text = Message.text
            };

            editor.SelectAll();
            editor.Copy();
        }

        public void OnEnable()
        {
            MessageBoxHandler.OnShowMessage += ShowMessage;
            MessageBoxHandler.OnAskQuestion += ShowMessage;
            MessageBoxHandler.OnShowMessageImmediate += ShowMessage_Interrupt;
            MessageBoxHandler.OnAskQuestionImmediate += ShowMessage_Interrupt;
        }

        public void OnDisable()
        {
            if (isOpen)
                Close(true);

            MessageBoxHandler.OnShowMessage -= ShowMessage;
            MessageBoxHandler.OnAskQuestion -= ShowMessage;
            MessageBoxHandler.OnShowMessageImmediate -= ShowMessage_Interrupt;
            MessageBoxHandler.OnAskQuestionImmediate -= ShowMessage_Interrupt;

            LeanTween.cancelAll();

            // LeanTween.removeTween(0, d1.uniqueId);
            // LeanTween.removeTween(1, d2.uniqueId);
            // LeanTween.removeTween(2, mov.uniqueId);
            // LeanTween.removeTween(3, a.uniqueId);

            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
            ToolsManager.Mode = WarMode.Sheet_Editing;
        }

        // public void OnPointerEnter(PointerEventData eventData)
        // {
        //     ToolsManager.SelectedTool = ToolTypes.None;
        // }

        // public void OnPointerExit(PointerEventData eventData)
        // {
        //     ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        //     ToolsManager.Mode = WarMode.Sheet_Editing;
        // }
    }
}
