/* DeviceTypeChecker.cs
 * Author: alipaknahad
 * Notes: Script sourced from Unity Forums: https://forum.unity.com/threads/detecting-between-a-tablet-and-mobile.367274/ 
 * Modified By: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace WarManager.Unity3D
{
    /// <summary>
    /// The type of devices (aspect ratio)
    /// </summary>
    public enum ENUM_Device_Type
    {
        Tablet,
        Phone
    }

    public static class DeviceTypeChecker
    {
        public static bool isTablet;

        /// <summary>
        /// Get the aspect ratio
        /// </summary>
        /// <returns></returns>
        private static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        /// <summary>
        /// Get the device type that the software is running on
        /// </summary>
        /// <returns></returns>
        public static ENUM_Device_Type GetDeviceType()
        {
#if UNITY_IOS
    bool deviceIsIpad = UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
            if (deviceIsIpad)
            {
                return ENUM_Device_Type.Tablet;
            }
            bool deviceIsIphone = UnityEngine.iOS.Device.generation.ToString().Contains("iPhone");
            if (deviceIsIphone)
            {
                return ENUM_Device_Type.Phone;
            }
#elif UNITY_ANDROID
 
        float aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);
        bool isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);
 
        if (isTablet)
        {
            return ENUM_Device_Type.Tablet;
        }
        else
        {
            return ENUM_Device_Type.Phone;
        }
#endif

            return ENUM_Device_Type.Tablet;
        }
    }
}
