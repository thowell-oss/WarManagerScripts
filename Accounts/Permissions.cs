/* Permissions.cs
 *Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

using UnityEngine;

using StringUtility;


namespace WarManager.Sharing.Security
{
    /// <summary>
    /// Dictates what the users can and cannot view/edit
    /// </summary>
    [Notes.Author("Dictates what the user can and cannot view/edit")]
    public class Permissions : IComparable<Permissions>, IEquatable<Permissions>
    {
        /// <summary>
        /// The name of the permissions set
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*name")]
        public string Name { get; private set; }

        /// <summary>
        /// Is this user an admin?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("*admin?")]
        public bool IsAdmin { get; private set; }

        private List<string> _permissionCategories = new List<string>();

        /// <summary>
        /// The categories the user has full access to use
        /// </summary>
        /// <value></value>

        [JsonPropertyName("*categories")]
        public string[] PermissionCategories { get => _permissionCategories.ToArray(); }

        /// <summary>
        /// The file path of the permissions
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string FilePath { get; private set; } = "";

        /// <summary>
        /// Get access to a specific category
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public string this[int index]
        {
            get => _permissionCategories[index];
        }

        /// <summary>
        /// The amount of permission categories
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public int Count
        {
            get => _permissionCategories.Count;
        }

        /// <summary>
        /// IEnumerable categories
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public IEnumerable<string> Categories
        {
            get => PermissionCategories;
        }

        /// <summary>
        /// Does this permission have the all tag '*'?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool ContainsAllCategoriesAccessCharacter
        {
            get
            {
                if (PermissionCategories != null && PermissionCategories.Length > 0)
                {
                    if (PermissionCategories[0] == "*")
                        return true;
                }

                return false;
            }
        }

        [JsonIgnore]
        public static Dictionary<string, Permissions> PermissionsDictionary => GetPermissions();

        [JsonConstructor]
        public Permissions(string name, bool isAdmin, string[] categories)
        {
            PermissionsConstructor(name, isAdmin, categories, string.Empty);
        }

        public Permissions(string name, bool isAdmin, string[] categories, string filePath)
        {
            PermissionsConstructor(name, isAdmin, categories, filePath);
        }

        /// <summary>
        /// The permissions constructor for handling all overloading constructors
        /// </summary>
        /// <param name="name">the name of the permissions</param>
        /// <param name="isAdmin">is the user an admin?</param>
        /// <param name="categories">the categories associated with the permissions</param>
        /// <param name="filePath">the file path and location</param>
        private void PermissionsConstructor(string name, bool isAdmin, string[] categories, string filePath)
        {
            if (name == null)
                throw new NullReferenceException("the name cannot be null");

            if (string.Empty == name)
                throw new ArgumentException("the name cannot be null");

            if (filePath == null)
                throw new NullReferenceException("the file path cannot be null");

            foreach (var x in name)
            {
                if (!char.IsDigit(x) && !char.IsLetter(x) && !char.IsPunctuation(x) && x != ' ')
                {
                    throw new ArgumentException("the character " + x + "is not allowed as a permissions name");
                }
            }

            if (categories == null)
                throw new NullReferenceException("Categories cannot be null");

            if (categories.Length == 0)
                throw new ArgumentException("the categories cannot be empty");

            foreach (var x in categories)
            {
                if (string.IsNullOrEmpty(x))
                    throw new ArgumentException("the string cannot be null or empty");
            }

            Name = name;
            _permissionCategories.AddRange(categories);
            IsAdmin = isAdmin;
            FilePath = filePath;
        }

        /// <summary>
        /// Gets all the categories including the name, if the account is null, the personal id will not be shown
        /// </summary>
        /// <param name="personal"></param>
        /// <returns></returns>
        public string[] GetFullAccessCategories(Account account)
        {
            List<string> catList = new List<string>();
            if (PermissionCategories != null)
            {
                catList.AddRange(PermissionCategories);
            }

            if (account != null)
                catList.Add(account.AccountID);

            return catList.ToArray();
        }

        /// <summary>
        /// Get the json value of the Permissions
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            string str = JsonSerializer.Serialize<Permissions>(this);
            return str;
        }

        /// <summary>
        /// Create an instance of the permissions from a json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Permissions GetPermissionsFromJson(string json, string filePath)
        {
            if (json == null || json == string.Empty)
                throw new System.NotSupportedException("Json string cannot be null or empty");

            if (filePath == null)
                throw new NullReferenceException("the file path cannot be null");

            Permissions p = null;

            try
            {
                var document = JsonDocument.Parse(json);

                JsonElement root = document.RootElement;
                JsonElement Jname = root.GetProperty("*name");
                JsonElement Jadmin = root.GetProperty("*admin?");
                JsonElement Jcategories = root.GetProperty("*categories");

                List<string> list = new List<string>();

                foreach (var c in Jcategories.EnumerateArray())
                {
                    string str = c.GetString().Trim();

                    list.Add(str);
                }

                p = new Permissions(Jname.GetString().Trim(), Jadmin.GetBoolean(), list.ToArray(), filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            return p;
        }


        /// <summary>
        /// Get all user created permissions from the server
        /// </summary>
        /// <returns></returns>
        public static List<Permissions> GetAllPermissions()
        {
            // if (!WarSystem.IsConnectedToServer)
            //     throw new System.NotSupportedException("Cannot access permissions when there is no server access");

            List<string> loadedNamesList = new List<string>();

            string path = GeneralSettings.Save_Location_Server_Permissions;

            string[] files = Directory.GetFiles(path);

            List<Permissions> permissionsList = new List<Permissions>();

            foreach (var file in files)
            {
                string str = "";
                using (StreamReader reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                    {
                        str += reader.ReadLine();
                    }


                    var p = Permissions.GetPermissionsFromJson(str, file);

                    string nameCheck = loadedNamesList.Find((x) => x == p.Name);


                    if (nameCheck == null)
                    {
                        loadedNamesList.Add(p.Name);
                        loadedNamesList.Sort();

                        permissionsList.Add(p);
                    }
                    else
                    {
                        NotificationHandler.Print("There was a duplicate permission that could not be loaded: \'" + p.Name + "\'");
                    }
                }
            }

            return permissionsList;
        }

        /// <summary>
        /// Save permissions back to the server
        /// </summary>
        /// <param name="permissions">the permissions being saved</param>
        /// <returns>returns true if the permissions is saved, false if not</returns>
        public static bool SavePermissions(Permissions permissions, Account account)
        {
            if (!account.IsAdmin)
                throw new ArgumentException("Cannot edit permissions without administrator privileges");

            if (permissions.FilePath != string.Empty)
            {
                FileInfo info = new FileInfo(permissions.FilePath);

                if (Directory.Exists(info.DirectoryName))
                {
                    string data = JsonSerializer.Serialize<Permissions>(permissions);

                    if (File.Exists(info.FullName))
                        info.Delete();

                    using (StreamWriter writer = info.CreateText())
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            writer.Write(data[i]);
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException("Cannot find the directory " + info.DirectoryName);
                }
            }
            else
            {
                var permissionsDirectory = GeneralSettings.Save_Location_Server_Permissions;

                string permissionsPath = permissionsDirectory + @"\" + permissions.Name + ".json";

                if (permissionsPath == null || permissionsPath == string.Empty)
                {
                    throw new IOException("The name and location to save the permissions is empty. Contact support.");
                }

                if (File.Exists(permissionsPath))
                {
                    throw new IOException("Permissions with the name " + permissions.Name.SetStringQuotes() + " already exists");
                }
                else
                {
                    permissions = new Permissions(permissions.Name, permissions.IsAdmin, permissions.Categories.ToArray(), permissionsPath);

                    if (permissions.FilePath != string.Empty)
                        return SavePermissions(permissions, account); // save the permissions correctly this time...
                }
            }

            return false;
        }

        /// <summary>
        /// Delete the permissions
        /// </summary>
        /// <param name="permissions">the permissions to delete</param>
        public static void DeletePermissions(Permissions permissions, Action backAction)
        {
            MessageBoxHandler.Print_Immediate("Are you sure you want to <i>permanently</i> delete " + permissions.Name + "?", "Question", (result) =>
            {
                if (result)
                {
                    string permissionsName = permissions.Name;

                    try
                    {
                        if (permissions.FilePath != null && permissions.FilePath != string.Empty)
                        {
                            File.Delete(permissions.FilePath);
                            WarSystem.AllPermissions.Remove(permissionsName);

                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate($"{permissionsName} deleted.", "Complete");
                                backAction();
                            });
                        }
                        else
                        {
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate($"Error! Permissions was not saved", "Error");
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate($"Error!\n{ex.Message}", "Error");
                        });
                    }
                }
            });
        }

        /// <summary>
        /// Get all the permissions organized in a dictionary
        /// </summary>
        /// <returns>returns a dictionary with the key as the permissions name and permissions reference as the value</returns>
        public static Dictionary<string, Permissions> GetPermissions()
        {
            var permissions = GetAllPermissions();

            Dictionary<string, Permissions> dict = new Dictionary<string, Permissions>();

            foreach (var p in permissions)
            {
                dict.Add(p.Name, p);
            }

            return dict;
        }

        /// <summary>
        /// Get the permissions by name
        /// </summary>
        /// <param name="names">the list of names</param>
        /// <returns></returns>
        public static IEnumerable<Permissions> GetPermissionsByName(List<string> names)
        {
            IEnumerable<Permissions> selectedPermissions =
                from Permissions p in GetAllPermissions()
                where names.Contains(p.Name)
                select p;

            return selectedPermissions;
        }

        /// <summary>
        /// Get all the permissions that contain on of the array of categories
        /// </summary>
        /// <param name="perm">the permissions</param>
        /// <param name="categories">the categories to check</param>
        /// <returns>returns the list of categories</returns>
        public static List<Permissions> GetAllPermissionsWithCategoriesNonGreedy(Permissions[] perm, string[] categories)
        {

            List<Permissions> result = new List<Permissions>();

            foreach (var x in perm)
            {
                int i = 0;
                bool found = false;
                while (i < categories.Length && !found)
                {
                    if (x.PermissionCategories.Contains(categories[i]))
                    {
                        found = true;
                        result.Add(x);
                    }

                    i++;
                }
            }

            return result;

        }

        /// <summary>
        /// Does the user with the selected permissions have access to said item
        /// </summary>
        /// <param name="permissions">the permissions</param>
        /// <param name="categories">the categories</param>
        /// <param name="greedy">true = the permissions must contain all categories (more secure), false = the permissions must contain at least one (less secure) </param>
        /// <returns>returns true if the item can be accessed, false if not</returns>
        public static bool CanAccessItem(Permissions permissions, IList<string> categories, bool greedy)
        {

            if (permissions == (Permissions)null)
                throw new NullReferenceException("the permissions cannot be null");

            if (categories == null)
                throw new NullReferenceException("the categories cannot be null");

            if (permissions.Count > 0 && permissions[0].Trim() == "*")
            {
                return true;
            }

            if (categories.Count > 0 && categories[0].Trim() == "*")
                return true;

            int len = 0;

            foreach (var x in categories)
            {
                if (permissions.ContainsKeywordPermission(x))
                {
                    len++;
                }
            }

            if (greedy && len == categories.Count)
                return true;
            else if (!greedy && len > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Get a list of accounts using the permissions
        /// </summary>
        /// <param name="greedy">is the list require the greedy strategy</param>
        /// <returns>the list of accounts</returns>
        public List<Account> GetAccounts(bool greedy)
        {
            return Account.GetAccountsListByPermissions(new List<Permissions>() { this }, greedy);
        }

        /// <summary>
        /// Evaluate if greedy item compared to permissions item can be accessed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>

        public static bool operator >=(Permissions a, string[] b)
        {
            return CanAccessItem(a, b, true);
        }

        /// <summary>
        /// Evaluate if non-greedy item compared to permissions item can be accessed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Permissions a, string[] b)
        {
            return CanAccessItem(a, b, false);
        }

        /// <summary>
        /// Load in the permissions and get all the categories (a little more expensive)
        /// </summary>
        /// <returns>returns a string array of all current categories</returns>
        public static string[] GetAllCategories()
        {
            return GetAllCategories(PermissionsDictionary);
        }

        /// <summary>
        /// Get all the categories from a permissions dictionary
        /// </summary>
        /// <param name="permissionsDict">the permissions dictionary</param>
        /// <returns>returns a string array of the categories</returns>
        public static string[] GetAllCategories(Dictionary<string, Permissions> permissionsDict)
        {

            if (permissionsDict == null || permissionsDict.Count < 1)
            {
                NotificationHandler.Print("Permissions dictionary not found");
                return new string[0];
            }

            List<string> categories = new List<string>();

            foreach (var p in permissionsDict)
            {
                for (int j = 0; j < p.Value.PermissionCategories.Length; j++)
                {
                    bool found = false;


                    for (int i = 0; i < categories.Count; i++)
                    {
                        if (categories[i] == p.Value.PermissionCategories[j])
                        {
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        if (p.Value.PermissionCategories[j] == "*")
                        {
                            categories.Insert(0, "*");
                        }
                        else
                        {
                            categories.Add(p.Value.PermissionCategories[j]);
                        }
                    }
                }
            }

            return categories.ToArray();
        }

        /// <summary>
        /// Do the permissions contain a certian permission keyword?
        /// </summary>
        /// <param name="keyword">the keyword to find</param>
        /// <returns>returns true if the keyword is found, false if not</returns>
        public bool ContainsKeywordPermission(string keyword)
        {
            foreach (var cat in PermissionCategories)
            {
                if (cat.Trim() == keyword.Trim())
                {
                    return true;
                }
            }

            return false;
        }

        public int CompareTo(Permissions other)
        {
            if (other == (Permissions)null)
                return 1;
            if (other.Name == null)
                return 1;

            if (Name == null)
                return -1;

            return Name.CompareTo(other.Name);
        }

        public bool Equals(Permissions other)
        {
            if (other == (Permissions)null)
                return false;

            if (other.Name == null)
            {
                if (Name == null)
                    return true;
                if (Name != null)
                    return false;
            }

            return Name == other.Name;
        }

        /// <summary>
        /// Checks to see if the permissions has the same categories as each other
        /// </summary>
        /// <param name="other">the other permissions instance</param>
        /// <returns>returns true if the other permissions has the same exact categories as the other</returns>
        public bool SameCategories(Permissions other)
        {
            if (other.PermissionCategories.Length == PermissionCategories.Length)
            {
                int sumOfSame = 0;

                foreach (var x in PermissionCategories)
                {
                    if (other.PermissionCategories.Contains(x))
                    {
                        sumOfSame++;
                    }
                }

                return sumOfSame == PermissionCategories.Length;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Name + " " + string.Join(", ", PermissionCategories) + " admin?: " + IsAdmin;
        }
    }
}
