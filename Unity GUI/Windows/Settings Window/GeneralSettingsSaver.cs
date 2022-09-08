using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    [Notes.Author("handles loading and saving user data settings from player prefs")]
    public class GeneralSettingsSaver
    {
        /// <summary>
        /// Load all the settings from player prefs
        /// </summary>
        public static void Load()
        {
            GetZoomPower();
        }


        /// <summary>
        /// save zoom power
        /// </summary>
        public static void SaveZoomPower()
        {
            PlayerPrefs.SetFloat("user_ZoomPower", GeneralSettings.zoomPower);
        }

        private static void GetZoomPower()
        {
            GeneralSettings.zoomPower = PlayerPrefs.GetFloat("user_ZoomPower", 7);
        }
    }
}
