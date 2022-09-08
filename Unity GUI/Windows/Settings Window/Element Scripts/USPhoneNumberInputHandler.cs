using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using WarManager.Unity3D.Windows;

namespace WarManager.Unity3D
{
    /// <summary>
    /// handles the phone number input GUI
    /// </summary>
    [Notes.Author("handles the phone number input GUI")]
    public class USPhoneNumberInputHandler : MonoBehaviour
    {
        /// <summary>
        /// the area text
        /// </summary>
        /// <returns></returns>
        [SerializeField] TMP_InputField _areaText;

        /// <summary>
        /// The middle three text
        /// </summary>
        /// <returns></returns>
        [SerializeField] TMP_InputField _middleThreeText;

        /// <summary>
        /// the last four text
        /// </summary>
        /// <returns></returns>
        [SerializeField] TMP_InputField _lastFourText;

        [SerializeField] SlideWindow_Element_EditableLabel Label;

        private bool submitted = true;

        private bool setup = false;

        [SerializeField] UnitedStatesPhoneNumber phoneNumber;

        void Awake()
        {
            _areaText.characterValidation = TMP_InputField.CharacterValidation.Digit;
            _middleThreeText.characterValidation = TMP_InputField.CharacterValidation.Digit;
            _lastFourText.characterValidation = TMP_InputField.CharacterValidation.Digit;

            _areaText.characterLimit = 3;
            _middleThreeText.characterLimit = 3;
            _lastFourText.characterLimit = 4;
        }

        void Update()
        {
            // if (Input.anyKey)
            // {
            //     if (_areaText.text.Length == 3)
            //     {
            //         if (_middleThreeText.text.Length == 3)
            //         {
            //             if (_lastFourText.text.Length == 0)
            //             {
            //                 GoTo(1);
            //             }

            //         }
            //         else if (_middleThreeText.text.Length == 0)
            //         {
            //             GoTo(0);
            //         }
            //     }
            // }

            // if (_areaText.text.Length == 3 && _middleThreeText.text.Length == 3 && _lastFourText.text.Length == 4)
            // {
            //     if (!submitted && setup)
            //     {
            //         Submit();
            //         submitted = true;
            //     }
            // }
            // else
            // {
            //     submitted = false;
            // }
        }

        /// <summary>
        /// Set the phone number
        /// </summary>
        /// <param name="number">the phone number</param>
        public void SetPhoneNumber(UnitedStatesPhoneNumber number)
        {
            _areaText.text = number.AreaCode;
            _middleThreeText.text = number.MiddleThree;
            _lastFourText.text = number.LastFour;

            phoneNumber = number;

            setup = true;
        }

        /// <summary>
        /// Select the next input box
        /// </summary>
        /// <param name="inputElement"></param>
        private void GoTo(int inputElement)
        {
            _areaText.onEndEdit.Invoke("");

            if (inputElement == 0)
            {
                _middleThreeText.onSelect.Invoke("");
            }

            if (inputElement == 1)
            {
                _middleThreeText.onEndEdit.Invoke("");
                _lastFourText.onSelect.Invoke("");
            }
        }

        #region edit result events

        public void OnEndEditArea(string data)
        {

            //if (phoneNumber.Error)
            //return;

            // Debug.Log(data);
            // Debug.Log(phoneNumber.MiddleThree);
            // Debug.Log(phoneNumber.LastFour);
            phoneNumber = new UnitedStatesPhoneNumber(data, phoneNumber.MiddleThree, phoneNumber.LastFour);

            if (_areaText.text.Length == 3 && _middleThreeText.text.Length == 3 && _lastFourText.text.Length == 4)
            {
                Submit();
            }
        }

        public void OnEndEditMiddleThree(string data)
        {
            //if (phoneNumber.Error)
            // return;

            phoneNumber = new UnitedStatesPhoneNumber(phoneNumber.AreaCode, data, phoneNumber.LastFour);

            if (_areaText.text.Length == 3 && _middleThreeText.text.Length == 3 && _lastFourText.text.Length == 4)
            {
                Submit();
            }
        }

        public void OnEndEditLastFour(string data)
        {
            //if (phoneNumber.Error)
            // return;

            phoneNumber = new UnitedStatesPhoneNumber(phoneNumber.AreaCode, phoneNumber.MiddleThree, data);

            if (_areaText.text.Length == 3 && _middleThreeText.text.Length == 3 && _lastFourText.text.Length == 4)
            {
                Submit();
            }
        }

        public void Submit()
        {
            string str = phoneNumber.FullNumberUS;
            // Label.OnEndEditPhoneNumber(phoneNumber);

            Label.OnEndEditText(str);

            Debug.Log("submitted " + phoneNumber.NumberUS);
        }

        void OnDisable()
        {
            setup = false;
        }

        #endregion
    }
}
