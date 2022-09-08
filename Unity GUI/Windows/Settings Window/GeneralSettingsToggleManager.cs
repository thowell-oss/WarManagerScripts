/* GeneralSettingsToggleManager.cs
 * Author: Taylor Howell
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles the settings in the settings window (GUI side)
    /// </summary>
    [Notes.Author("Handles the settings in the settings window")]
    public class GeneralSettingsToggleManager : MonoBehaviour
    {
        [Header("Account")]
        public TMPro.TMP_Dropdown LanguageDropDown;
        public TMPro.TMP_Dropdown DepartmentDropDown;
        [Header("Shifting Tab")]
        public Toggle FillGapAfterShiftFromBottom;
        public Toggle FillGapAfterShiftFromSide;
        public Toggle AllowSideShifting;
        public Toggle AllowUpDownShifting;
        [Header("Saving")]
        public Toggle AutoSave;
        public TMPro.TMP_InputField SaveTime;
        public TMPro.TMP_InputField OfflineLocation;
        public TMPro.TMP_InputField ServerLocation;

        [Space]
        public TMPro.TMP_Text ZoomPowertext;
        public Slider ZoomPowerSlider;


        public void Awake()
        {
            SetSettings();
        }

        /// <summary>
        /// Call this whenever a window needs to refresh or update
        /// </summary>
        public void SetSettings()
        {
            LanguageDropDown.value = (int)GeneralSettings.Account_Current_Language;

            FillGapAfterShiftFromBottom.isOn = GeneralSettings.FillGapAfterShiftFromBottom;
            FillGapAfterShiftFromSide.isOn = GeneralSettings.FillGapAfterShiftFromSide;
            AllowSideShifting.isOn = GeneralSettings.AllowSideShifting;
            AllowUpDownShifting.isOn = GeneralSettings.AllowUpDownShifting;

            AutoSave.isOn = GeneralSettings.Save_AutoSave;
            SaveTime.text = GeneralSettings.Save_AutoSave_Time_Seconds.ToString();

            OfflineLocation.text = GeneralSettings.Save_Location_Offline;
            ServerLocation.text = GeneralSettings.Save_Location_Server;
        }

        /// <summary>
        /// This is how we change the settins
        /// </summary>
        /// <param name="t"></param>
        public void Toggle(Toggle t)
        {
            if (t == FillGapAfterShiftFromBottom)
            {
                GeneralSettings.FillGapAfterShiftFromBottom = t.isOn;

                if (t.isOn)
                {
                    FillGapAfterShiftFromSide.isOn = false;
                }

                return;
            }

            if (t == FillGapAfterShiftFromSide)
            {
                GeneralSettings.FillGapAfterShiftFromSide = t.isOn;

                if (t.isOn)
                {
                    FillGapAfterShiftFromBottom.isOn = false;
                }

                return;
            }

            if (t == AllowSideShifting)
            {
                GeneralSettings.AllowSideShifting = t.isOn;

                if (t.isOn)
                {
                    FillGapAfterShiftFromSide.isOn = false;
                }

                return;
            }

            if (t == AllowUpDownShifting)
            {
                GeneralSettings.AllowUpDownShifting = t.isOn;

                if (t.isOn)
                {
                    FillGapAfterShiftFromBottom.isOn = false;
                }

                return;
            }

            if (t == AutoSave)
            {
                GeneralSettings.Save_AutoSave = t.isOn;
                return;
            }

            SetSettings();
        }

        /// <summary>
        /// Drop down value change
        /// </summary>
        /// <param name="dpDown"></param>
        public void DropDownChange(TMPro.TMP_Dropdown dpDown)
        {
            if (dpDown = LanguageDropDown)
            {
                if (dpDown.value == 0)
                {
                    GeneralSettings.Account_Current_Language = Language.English;
                }

                if (dpDown.value == 1)
                {
                    GeneralSettings.Account_Current_Language = Language.Spanish;
                }

                Debug.Log("Language changed " + GeneralSettings.Account_Current_Language.ToString());
            }

            if (DepartmentDropDown == dpDown)
            {
                GeneralSettings.Account_Department = (Department)dpDown.value;
            }

            SetSettings();
        }

        public void SavingTabChangeInfo()
        {
            string txt = ServerLocation.text;

            if (txt != GeneralSettings.Save_Location_Server)
            {
                if (WarSystem.TrySetNewRoot(txt))
                {
                    //save settings persistantly
                }
            }

            txt = SaveTime.text;

            if (Int32.TryParse(txt, out var x))
            {
                GeneralSettings.SetAutoSaveTime(x);
            }

            SetSettings();
        }

        /// <summary>
        /// use the on screen input display
        /// </summary>
        /// <param name="use"></param>
        public void ChangeUseOnScreenInputDisplay(bool use)
        {
            GeneralSettings.UseOnScreenInputGui = use;
        }

        /// <summary>
        /// Update Zoom Power
        /// </summary>
        /// <param name="power"></param>
        public void UpdateZoomPower(float power)
        {
            GeneralSettings.zoomPower = power;
            GeneralSettingsSaver.SaveZoomPower();
            ZoomPowertext.text = power.ToString();
        }

        /// <summary>
        /// Sets the zoom power objects to the values
        /// </summary>
        public void GetZoomPower()
        {
            ZoomPowerSlider.value = GeneralSettings.zoomPower;
            ZoomPowertext.text = GeneralSettings.zoomPower.ToString();
        }
    }
}
