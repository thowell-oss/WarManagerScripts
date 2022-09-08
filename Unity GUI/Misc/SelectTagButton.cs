

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WarManager.Unity3D
{
    public class SelectTagButton : MonoBehaviour
    {
        [SerializeField] private string _tagName;
        TMPro.TMP_Text TitleText;

        public UnityListTagSelectDropdown DropDown;

        public void SetTagButton(string tagName, UnityListTagSelectDropdown dropdown)
        {
            _tagName = tagName;
            TitleText.text = _tagName;
            DropDown = dropdown;
        }

        public void Activate()
        {
            DropDown.SelectTag(_tagName);
        }

        public void Cancel()
        {
            DropDown.Cancel();
        }
    }
}
