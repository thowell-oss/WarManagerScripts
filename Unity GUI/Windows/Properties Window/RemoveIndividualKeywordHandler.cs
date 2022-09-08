using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    public class RemoveIndividualKeywordHandler : MonoBehaviour
    {
        public int index;
        public SlideWindowSearch SearchController;
        private TMPro.TMP_InputField inputfield;

        public void RemoveKeyword()
        {
            SearchController.RemoveFilterKeyword(index);
        }

        public void OnValueChanged()
        {
            if(inputfield == null)
                inputfield = GetComponent<TMPro.TMP_InputField>();

            SearchController.FilterKeywords[index] = inputfield.text;
        }
    }
}
