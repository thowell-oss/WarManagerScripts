/* MessageBox Handle.rcs
 * Author: Taylor Howell
 */

using System;
using System.Collections.Generic;

namespace WarManager
{
    /// <summary>
    /// Handles the events tied to the mesage box controller
    /// </summary>
    public static class MessageBoxHandler
    {
        public delegate void ShowMessageBox(string message, string title);
        public static event ShowMessageBox OnShowMessage;
        public static event ShowMessageBox OnShowMessageImmediate;

        public delegate void AskQuestion(string message, string title, Action<bool> callBack);
        public static event AskQuestion OnAskQuestion;
        public static event AskQuestion OnAskQuestionImmediate;

        private static Queue<(string message, string title, Action<bool> callBack)> MessageBoxQueue = new Queue<(string message, string title, Action<bool>)>();


        private static bool _isBoxReady = false;

        /// <summary>
        /// Print a message to the message box -> adds this to the message queue
        /// </summary>
        /// <param name="message">the message to print</param>
        /// <param name="title">the title</param>
        [Obsolete("Please use print immediate instead")]

        public static void Print_AddToQueue(string message, string title = "Notice")
        {
            // MessageBoxQueue.Enqueue((message, title, null));
            // StartQueue();
        }

        /// <summary>
        /// Print a message immediately to the message box
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="title">the title</param>
        public static void Print_Immediate(string message, string title)
        {

            UnityEngine.Debug.Log(message);

            if (OnShowMessageImmediate != null)
                OnShowMessageImmediate(message, title);
        }

        /// <summary>
        /// Overloaded method asks a question answers in yes/no -> adds this message to the queue
        /// </summary>
        /// <param name="message">the mesage</param>
        /// <param name="title">the title</param>
        /// <param name="callBackResult">the callback delegate</param>
        [Obsolete("Please use print immediate instead")]
        public static void Print_AddToQueue(string message, string title, Action<bool> callBackResult)
        {
            // MessageBoxQueue.Enqueue((message, title, callBackResult));
            // StartQueue();
        }


        /// <summary>
        /// Force ask a question to the message box immediately
        /// </summary>
        /// <param name="message">the message content</param>
        /// <param name="title">the title of the message</param>
        /// <param name="callBackResult">the call back answer</param>
        public static void Print_Immediate(string message, string title, Action<bool> callBackResult)
        {
            if (OnAskQuestionImmediate != null)
                OnAskQuestionImmediate(message, title, callBackResult);
        }

        /// <summary>
        /// Starts the handling of the queue if possible
        /// </summary>
        private static void StartQueue()
        {
            if (_isBoxReady && MessageBoxQueue.Count == 1)
            {
                HandleQueue();
            }
        }

        /// <summary>
        /// Call this when the message box is ready for the next message
        /// </summary>
        public static void SetMessageBoxReady()
        {
            _isBoxReady = true;
            HandleQueue();
        }

        private static void HandleQueue()
        {
            if (MessageBoxQueue.Count > 0)
            {
                var result = MessageBoxQueue.Dequeue();

                if (result.callBack == null)
                {
                    if (OnShowMessage != null)
                        OnShowMessage(result.message, result.title);
                }
                else
                {
                    if (OnAskQuestion != null)
                        OnAskQuestion(result.message, result.title, result.callBack);
                }
            }
        }
    }
}
