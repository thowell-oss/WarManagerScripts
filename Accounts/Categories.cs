using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.IO;


using UnityEngine;

namespace WarManager.Sharing.Security
{
    public class Categories
    {
        /// <summary>
        /// An array of strings where all the server categories are located
        /// </summary>
        /// <value></value>
        public string[] ServerCategories { get; private set; } = new string[0];

        /// <summary>
        /// The location where the categories list would be located
        /// </summary>
        /// <value></value>
        public static string CategoriesLocation { get; } = GeneralSettings.Save_Location_Server + @"\Data\Categories.txt";

        [JsonIgnore]
        /// <summary>
        /// The personal category (probably a device id)
        /// </summary>
        public string Local;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="local">the personal category string to access all local items</param>
        public Categories(string local)
        {
            if (local == null || local == string.Empty || local == "*")
                throw new System.NotSupportedException("The personal category string cannot be null or empty");

            Local = local;
            ServerCategories = new string[0];

            WarSystem.OnConnectionToServerChanged += GetServerCategories;
        }


        /// <summary>
        /// Refresh the list of categories on the server
        /// </summary>
        /// <param name="connectedToServer">is war manager connected to the server?</param>
        public void GetServerCategories(bool connectedToServer)
        {

            if (connectedToServer)
            {
                List<string> cats = new List<string>();
                if (File.Exists(CategoriesLocation))
                {
                    using (Stream stream = new FileStream(CategoriesLocation, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream)
                            {
                                string str = reader.ReadLine();
                                str = str.Trim();
                                cats.Add(str);
                            }
                        }
                    }
                }

                ServerCategories = cats.ToArray();
            }
            else
            {
                ServerCategories = new string[0];
            }
        }

        public void Close()
        {
            WarSystem.OnConnectionToServerChanged -= GetServerCategories;
        }

        /// <summary>
        /// Checks to make sure the permissions that the acount has is synced with the master categories list
        /// </summary>
        /// <param name="c"></param>
        /// <param name="accnt"></param>
        /// <returns></returns>
        public static Categories RefreshCategories(Account accnt)
        {
            Categories c = new Categories(WarSystem.DeviceID);

            if (accnt != null && WarSystem.IsConnectedToServer)
            {
                c.GetServerCategories(true);

                var permCats = accnt.Permissions.PermissionCategories;
                var allCats = c.ServerCategories;

                List<string> newCategories = new List<string>();

                for (int i = 0; i < permCats.Length; i++)
                {
                    bool found = false;

                    for (int j = 0; j < allCats.Length; j++)
                    {
                        if (permCats[i] == allCats[j])
                            found = true;
                    }

                    if (!found)
                    {
                        newCategories.Add(permCats[i]);
                    }
                }

                Categories.UpdateServerCategories(newCategories);
            }

            return c;
        }

        /// <summary>
        /// Adds categories to the server
        /// </summary>
        /// <param name="newCategories">the list of new categories</param>
        public static void UpdateServerCategories(List<string> newCategories)
        {
            newCategories.Remove("*");

            if (newCategories.Count < 1)
                return;

            if (WarSystem.IsConnectedToServer)
            {
                if (File.Exists(CategoriesLocation))
                {
                    using (Stream stream = new FileStream(CategoriesLocation, FileMode.Append))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            for (int i = 0; i < newCategories.Count; i++)
                            {
                                writer.WriteLine(newCategories[i]);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("file does not exist");
                }
            }
        }
    }


}
