

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles loading images stored in the server
    /// </summary>
    public static class ServerIconsManager
    {
        /// <summary>
        /// Stored icons that have been loaded this session for quick retrieval
        /// </summary>
        /// <typeparam name="string">the name of the image</typeparam>
        /// <typeparam name="Sprite">the sprite instance</typeparam>
        /// <returns></returns>
        private static Dictionary<string, Sprite> _loadedSpriteDictionary = new Dictionary<string, Sprite>();

        /// <summary>
        /// Get a list of icons
        /// </summary>
        /// <param name="iconNames">the array of icon names to find</param>
        /// <param name="fallBackIcon">the fall back icon if the icon could not be found</param>
        /// <returns>returns a list of icons, empty if none were found</returns>
        public static List<Sprite> GetIcons(string[] iconNames, Sprite fallBackIcon)
        {
            List<Sprite> icons = new List<Sprite>();

            for (int i = 0; i < iconNames.Length; i++)
            {
                if (TryGetIcon(iconNames[i], out var icon))
                {
                    icons.Add(icon);
                }
                else
                {
                    icons.Add(fallBackIcon);
                }
            }

            return icons;
        }


        /// <summary>
        /// Attempt to get an icon from the server
        /// </summary>
        /// <param name="iconName">the file name of the icon</param>
        /// <param name="icon">the out sprite icon</param>
        /// <returns>retruns true if the icon was successufully retrieved, false if not</returns>
        public static bool TryGetIcon(string iconName, out Sprite icon)
        {
            if (string.IsNullOrWhiteSpace(iconName))
            {
                icon = null;
                return false;
            }

            iconName = iconName.Trim();

            if (_loadedSpriteDictionary.TryGetValue(iconName, out var sprite))
            {
                icon = sprite;
                return true;
            }

            string iconPath = GeneralSettings.Server_Icons + @"\" + iconName;

            if (!File.Exists(iconPath))
            {
                NotificationHandler.Print("Could not find " + iconName);

                icon = null;
                return false;
            }

            Sprite someIcon;

            if (TryGetImage(iconPath, out var ic))
            {
                someIcon = ic;
                icon = someIcon;
                return true;
            }

            icon = null;
            return false;

        }


        /// <summary>
        /// Attempt to get load an image into unity
        /// </summary>
        /// <param name="path">the path of the image</param>
        /// <param name="icon">the icon out perameter</param>
        /// <returns>returns true if the icon was found</returns>
        public static bool TryGetImage(string path, out Sprite icon)
        {

            FileInfo info = new FileInfo(path);

            try
            {

                byte[] data = File.ReadAllBytes(path);

                Texture2D texture = null;

                if (info.Extension == ".png")
                {
                    texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                }
                else if (info.Extension == ".jpg")
                {
                    texture = new Texture2D(128, 128, TextureFormat.RGB24, false);
                }

                if (texture != null && texture.LoadImage(data))
                {
                    texture.name = Path.GetFileNameWithoutExtension(info.Name);
                    icon = Sprite.Create(texture, new UnityEngine.Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    if (_loadedSpriteDictionary.ContainsKey(info.Name))
                    {
                        _loadedSpriteDictionary.Remove(info.Name);
                    }

                    _loadedSpriteDictionary.Add(info.Name, icon);

                    // Debug.Log("getting icon " + info.Name);

                    return true;
                }
                else
                {
                    NotificationHandler.Print("Could not display " + info.Name + " becuase it is an unsupported image format");

                    icon = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Could not fetch " + info.Name + " " + ex.Message);
                icon = null;
                return false;
            }
        }

    }
}
