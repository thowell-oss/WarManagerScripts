using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    [Notes.Author("Handels spawning the onscreen keyboard")]
    public class ForceKeyboardInput : MonoBehaviour
    {
        public void OnScreenKeyBoard(string text)
        {
            // if (InputSystem.Main.InputMode == InputMode.Touch)
            //     TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.Default, true, false, false, false, "Start Typing", 500);

            //cannot be used for standalone win32 apps there is no backend API for unity to call
        }
    }
}
