using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles the searching for cards in a sheet")]
    public class UISearchBar : MonoBehaviour
    {
        TMP_InputField inputField;
        public SheetsCommands commands;

        // Start is called before the first frame update
        void Awake()
        {
            if (inputField == null)
                inputField = GetComponent<TMP_InputField>();

            inputField.ActivateInputField();
        }

        public void OnValueChange()
        {
            commands.Find(inputField.text);
        }

        public void Cancel()
        {
            inputField.text = "";
        }
    }
}
