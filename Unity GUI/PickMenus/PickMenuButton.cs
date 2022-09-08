
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace WarManager.Unity3D.PickMenu
{
    [RequireComponent(typeof(Button))]
    public class PickMenuButton : MonoBehaviour
    {
        [SerializeField] TMP_Text TitleText;
        private Button button;

        public Color disabledColor = Color.black;

        private Action SelectionAction;

        void Start()
        {
            button = GetComponent<Button>();
        }

        public void Select()
        {
            if (SelectionAction != null)
            {
                SelectionAction();
                PickMenuManger.main.ClosePickMenu();
            }
        }

        public void SetButton(string title, Action action, bool interactible)
        {
            if (button == null)
                button = GetComponent<Button>();

            TitleText.text = title;
            SelectionAction = action;

            if(!interactible)
            {
                TitleText.color = disabledColor;
            }
            else
            {
                TitleText.color = Color.white;
            }

            button.interactable = interactible;
        }
    }
}