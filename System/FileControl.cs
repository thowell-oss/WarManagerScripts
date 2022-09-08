
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

namespace WarManager.Sharing
{
    /// <summary>
    /// Controls who can view the file
    /// </summary>
    /// <typeparam name="T">the type of file</typeparam>
    [Notes.Author(1.2, "Controls who can view the file")]
    public class FileControl<T> : IComparable<FileControl<T>>
    {
        /// <summary>
        /// Is the file requirement to view greedy?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool Greedy { get; private set; }

        /// <summary>
        /// The required categories in order to open the file
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string[] RequiredCategories { get; private set; } = new string[0];

        /// <summary>
        /// Who created the file?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string Owner { get; private set; }

        /// <summary>
        /// When was the file created?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DateTime CreationTime { get; private set; }

        /// <summary>
        /// When was the last time the file was opened?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public DateTime LastTimeFileOpened { get; private set; }

        /// <summary>
        /// File version
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string FileVersion { get; private set; } = "1";

        /// <summary>
        /// Is this file an offline only file?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool OfflineFile { get; private set; } = false;

        /// <summary>
        /// The path where the file was retained
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string FilePath { get; private set; }

        /// <summary>
        /// File ref
        /// </summary>
        [JsonIgnore]
        public T Data { get; private set; }

        /// <summary>
        /// For offline files
        /// </summary>
        public FileControl(string version)
        {
            OfflineFile = true;
            FileVersion = version;
        }

        /// <summary>
        /// For server files
        /// </summary>
        /// <param name="isGreedy">the the requirement to open the file greedy?</param>
        /// <param name="fileVersion">the version of the file</param>
        /// <param name="categories">the array of strings containing categories</param>
        /// <param name="owner">who is the owner of the file?</param>
        /// <param name="creationTime">when was the file created?</param>
        /// <param name="lastTimeFileOpened">when was the last time the file was opened?</param>
        public FileControl(T file, bool isGreedy, string fileVersion, string[] categories, string owner, DateTime creationTime, DateTime lastTimeFileOpened)
        {
            if (file == null)
                throw new NullReferenceException("the file cannot be null");

            Data = file;

            OfflineFile = false;

            Greedy = isGreedy;

            if (fileVersion == null || fileVersion == string.Empty)
                throw new NotSupportedException("the file version cannot be null or empty");

            FileVersion = fileVersion;

            if (categories == null || categories.Length < 1)
                throw new NotSupportedException("the file categories must contain a category to be accessed");
            RequiredCategories = categories;

            if (owner == null || owner == string.Empty)
                throw new NotSupportedException("The creator name cannot be null or empty");

            Owner = owner;

            if (creationTime == null)
                throw new NotSupportedException("the creation time cannot be null");

            CreationTime = creationTime;

            if (lastTimeFileOpened == null)
                throw new NotSupportedException("the list time file opened cannot be null");

            LastTimeFileOpened = lastTimeFileOpened;
        }

        /// <summary>
        /// Unlock the offline file
        /// </summary>
        /// <param name="controller">the file security controller</param>
        /// <param name="version">the file version</param>
        /// <param name="file">(out) the resulting file (if the unlock was successful)</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryGetOfflineFile(FileControl<T> controller, string version, out T file)
        {
            bool canOpen = controller.OfflineFile;

            if (canOpen)
            {
                file = controller.Data;

                return true;
            }

            file = default(T);
            return false;
        }

        /// <summary>
        /// Attempt to open a server file
        /// </summary>
        /// <param name="fileSecurity">the server file</param>
        /// <param name="version">the war manager file version</param>
        /// <param name="categories">the permissions categories</param>
        /// <param name="file">(out) the resulting file</param>
        /// <param name="openOnce">should the file be allowed to open just this once?</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryGetServerFile(FileControl<T> fileSecurity, string version, Account account, out T file, bool openOnce = false)
        {

            if (fileSecurity.Data == null)
            {
                file = default(T);
                return false;
            }

            if (fileSecurity.OfflineFile)
            {
                file = default(T);
                return false;
            }

            string fileVersion = "v" + fileSecurity.FileVersion;

            if (version != fileVersion)
            {
                file = default(T);

                Debug.LogError("Version incorrect \'" + version + "\'  \'" + fileVersion + "\'");

                return false;
            }

            #region old

            // bool canOpen = false;

            // int x = 0;

            // if (!account.ContainsAllCategoriesAccessCharacter)
            // {
            //     var categories = account.FullAccessCategories;

            //     for (int i = 0; i < fileSecurity.RequiredCategories.Length; i++)
            //     {
            //         for (int j = 0; j < categories.Length; j++)
            //         {
            //             if (fileSecurity.RequiredCategories[i] == categories[j])
            //             {
            //                 x++;
            //             }
            //         }
            //     }

            //     int totalRequried = fileSecurity.RequiredCategories.Length;

            //     if (fileSecurity.Greedy && x == totalRequried)
            //     {
            //         canOpen = true;
            //     }
            //     else
            //     {
            //         canOpen = x >= 1;
            //     }
            // }
            // else
            // {
            //     // UnityEngine.Debug.Log("Account can access all categories");
            //     canOpen = true;
            // }

            // if (canOpen)
            // {
            //     file = fileSecurity.Data;

            //     return true;
            // }

            #endregion

            bool canAccess = false;

            if (fileSecurity.Greedy)
            {
                if (account.Permissions >= fileSecurity.RequiredCategories)
                {
                    canAccess = true;
                }
            }
            else
            {
                if (account.Permissions <= fileSecurity.RequiredCategories)
                {
                    canAccess = true;
                }
            }

            if (fileSecurity.Owner == account.UserName) // if this item is owned by the user then they can access it
                canAccess = true;

            if (canAccess)
            {
                file = fileSecurity.Data;
                return true;
            }
            else
            {
                file = default(T);
                return false;
            }
        }

        /// <summary>
        /// compare one file with the other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(FileControl<T> other)//Icomparable
        {
            return GetHashCode().CompareTo(other.GetHashCode());
        }
    }
}
