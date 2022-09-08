
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handle the notification popup button 
    /// </summary>
    [Notes.Author("Handle the notification popup button ")]
    public class NotificationPopupButtonController : MonoBehaviour
    {
        [SerializeField] TMP_Text _messageText;
        [SerializeField] TMP_Text _callToActionText;

        private Action _callBackAction;

        /// <summary>
        /// Set up the new notification
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="callToAction">the call to action</param>
        /// <param name="callBack">the call back</param>
        public void SetNotification(string message, string callToAction, Action callBack)
        {
            _messageText.text = message;
            _callToActionText.text = callToAction;

            _callBackAction = callBack;
        }

        public void FireCallBack()
        {
            if (_callBackAction != null)
                _callBackAction();
        }
    }
}
