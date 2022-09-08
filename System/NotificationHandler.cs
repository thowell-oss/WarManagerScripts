/* 
 * NotificationHandler.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Mediates between the backend and UI
    /// </summary>
    [Notes.Author("Mediates between the backend and UI")]
    public static class NotificationHandler
    {
        public delegate void printText_delegate(string text);
        public static event printText_delegate OnPrintText;

        public delegate void includeCallToAction_delegate(string text, string callToActionMessage, Action callBack);
        public static event includeCallToAction_delegate OnSendAction;

        private static bool _readyToDisplay;

        /// <summary>
        /// Is the notification system ready to display messages?
        /// </summary>
        public static bool readyToDisplay
        {
            get
            {
                return _readyToDisplay;
            }
            set
            {
                _readyToDisplay = value;

                if (_readyToDisplay)
                {
                    if (OnPrintText != null)
                        PushMessagesQueue();

                    if (OnSendAction != null)
                        PushButtonQueue();
                }
            }
        }

        /// <summary>
        /// The messages back log queue (when the messages cannot be displayed for a certain time (like when the software boots up))
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private static Queue<string> MessagesBacklogQueue = new Queue<string>();

        /// <summary>
        /// the button messages queue
        /// </summary>
        /// <param name="ButtonMessagesQueue"></param>
        /// <typeparam name="(string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Action)"></typeparam>
        private static Queue<(string, string, Action)> ButtonMessagesQueue = new Queue<(string, string, Action)>();

        /// <summary>
        /// Print a message and include a button
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="callToAction">the call to action text on the button </param>
        /// <param name="callback">the callback when the button is pressed</param>
        public static void Print(string message, string callToAction, Action callBack)
        {

            if (Application.isPlaying)
            {
                if (readyToDisplay)
                    OnSendAction?.Invoke(message, callToAction, callBack);
                else
                    ButtonMessagesQueue.Enqueue((message, callToAction, callBack));
            }

        }

        /// <summary>
        /// Print text to the notification box (drawer)
        /// </summary>
        /// <param name="message">the message to display to the notification handler</param>
        public static void Print(string message)
        {

            if (Application.isPlaying)
            {

                if (OnPrintText != null && readyToDisplay)
                {
                    OnPrintText(message);
                }
                else
                {
                    MessagesBacklogQueue.Enqueue(message);
                }

#if UNITY_EDITOR
                UnityEngine.Debug.LogError("WM Log: " + message);
#endif
            }
            else
            {
                UnityEngine.Debug.Log("WM Log: " + message);
            }


        }

        /// <summary>
        /// Push the backlogged messages to the notification system
        /// </summary>
        private static void PushMessagesQueue()
        {
            int totalSentMessagesSent = MessagesBacklogQueue.Count; //prevent infinite loop potential problems

            while (totalSentMessagesSent > 0)
            {
                string m = MessagesBacklogQueue.Dequeue();
                Print(m);
                totalSentMessagesSent--;
            }
        }

        /// <summary>
        /// Push the backlogged messages to the notification system
        /// </summary>
        private static void PushButtonQueue()
        {
            while (ButtonMessagesQueue.Count > 0)
            {
                var m = ButtonMessagesQueue.Dequeue();
                Print(m.Item1, m.Item2, m.Item3);
            }
        }
    }
}
