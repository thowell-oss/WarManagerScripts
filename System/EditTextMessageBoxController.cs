
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    public static class EditTextMessageBoxController
    {
        public delegate void EditText_delegate(string currentText, string title, Action<string> callBack, bool password);
        public static event EditText_delegate OnEditText;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentText">the current text that the user desires to change</param>
        /// <param name="contextTitle">the context of the modal window title</param>
        /// <param name="resultCallback">the change text callback</param>
        public static void OpenModalWindow(string currentText, string contextTitle, Action<string> resultCallback, bool password = false)
        {
            if (OnEditText != null)
                OnEditText(currentText, contextTitle, resultCallback, password);
        }
    }
}
