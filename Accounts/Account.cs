/*Account.cs
 *Author: Taylor Howell
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

using UnityEngine;

using WarManager.Sharing.Security;
using WarManager.Backend;
using WarManager;

namespace WarManager.Sharing
{
    /// <summary>
    /// A class that stores and gets information related to accounts
    /// </summary>
    public sealed class Account : IComparable<Account>, IEquatable<Account>
    {

        /// <summary>
        /// The Name of the User
        /// </summary>
        /// <value></value>
        public string UserName { get; private set; }

        /// <summary>
        /// The the categories the user can view/edit
        /// </summary>
        /// <value></value>
        public Permissions Permissions { get; private set; }

        /// <summary>
        /// The name of the permissions associated with this account
        /// </summary>
        /// <value></value>
        public string PermissionsName
        {
            get
            {
                return Permissions.Name;
            }
        }

        /// <summary>
        /// Does this account have access to all categories? (except for other's private categories)
        /// </summary>
        /// <value></value>
        public bool ContainsAllCategoriesAccessCharacter
        {
            get
            {
                return Permissions.ContainsAllCategoriesAccessCharacter;
            }
        }

        /// <summary>
        /// Is this user an administrator?
        /// </summary>
        /// <value></value>
        public bool IsAdmin
        {
            get
            {
                return Permissions.IsAdmin;
            }
        }

        /// <summary>
        /// Returns an array of categories including the private system Id string
        /// </summary>
        /// <value></value>
        public string[] FullAccessCategories
        {
            get
            {
                return Permissions.GetFullAccessCategories(null);
            }
        }

        /// <summary>
        /// The password hash key
        /// </summary>
        /// <value></value>
        public string HashKey { get; private set; }

        /// <summary>
        /// The selected language of the user
        /// </summary>
        /// <value></value>
        public Language UserSelectedLanguage { get; private set; }

        /// <summary>
        /// The first name of the user
        /// </summary>
        /// <value></value>
        public string FirstName { get; private set; }

        /// <summary>
        /// The last name of the user
        /// </summary>
        /// <value></value>
        public string LastName { get; private set; }

        /// <summary>
        /// The id of the account
        /// </summary>
        /// <value></value>
        public string AccountID { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">user name of the account</param>
        /// <param name="hashKey">the password of the account</param>
        /// <param name="permissions">the permissions (what the user can and cannot do)</param>
        /// <param name="language">the language that the user wants to see/hear/speak</param>
        /// <param name="firstName">the real first name of the user</param>
        /// <param name="lastName">the real last name of the user</param>
        /// <param name="sysID">the system id (what war manger uses for private permissions)</param>
        public Account(string name, string hashKey, Permissions permissions, string language, string firstName, string lastName, string sysID)
        {
            if (name == null || name == string.Empty)
                throw new NotSupportedException("The user name cannot be null or empty");

            UserName = name;

            if (hashKey == null || hashKey == string.Empty)
                throw new NotSupportedException("the password cannot be null or empty");

            HashKey = hashKey;

            if (permissions == (Permissions)null)
                throw new NullReferenceException("The permissions cannot be null");

            Permissions = permissions;

            Language lan = Language.English;
            Enum.TryParse<Language>(language, out lan);
            UserSelectedLanguage = lan;

            if (firstName == null)
                throw new NullReferenceException("The first name cannot be null");

            FirstName = firstName;

            if (lastName == null)
                throw new NullReferenceException("the last name cannot be null");

            LastName = lastName;

            if (sysID == null || sysID == string.Empty)
                throw new NotSupportedException("the system id cannot be null or empty");

            AccountID = sysID;
        }

        public static void DecryptLoginFile(byte[] key)
        {
            string file = Encryption.DecryptGetFile(key, GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv");
        }

        /// <summary>
        /// Get all accounts on the War Manager Server 
        /// (Obsolete use <see cref="GetAccountsList(string)"/> instead).
        /// </summary>
        /// <returns>returns the list of accounts</returns>
        [Obsolete("Use GetAccountsList() instead")]
        public static List<Account> GetAccounts()
        {

            List<Account> accounts = new List<Account>();

            try
            {
                var importer = new Import_CSV();
                importer.Import(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv");

                for (int i = 1; i < importer.Height; i++)
                {
                    string userName = importer.GetCell(0, i);
                    string permissionsName = importer.GetCell(2, i).Trim();
                    string systemID = importer.GetCell(6, i);

                    if (WarSystem.AllPermissions.TryGetValue(permissionsName, out var permissions))
                    {
                        Account a = new Account(userName, "getting acct info", permissions, importer.GetCell(3, i), importer.GetCell(4, i), importer.GetCell(5, i), systemID);
                        // Debug.Log("Found account with permissions \'" + permissionsName + "\'");
                        accounts.Add(a);
                    }
                    else
                    {
                        Debug.Log("failure getting account with permissions \'" + permissionsName + "\'");
                    }
                }

                // Debug.Log("Completed");
            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Error getting accounts " + ex.Message);
            }

            return accounts;
        }

        /// <summary>
        /// Login to War Manager
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="password"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string LogIn(string UserName, string password, out Account a)
        {
            try
            {

                int x = WarSystem.HandleConnection(out string name);
                string serverName = name;
                // Debug.Log("connection " + x);

                if (x == 0)
                {
                    WarSystem.LoadPermissions();

                    var importer = new Import_CSV();
                    importer.Import(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv");

                    // Debug.Log("Permissions loaded");

                    int csvHeight = importer.Height;

                    string hash = Encryption.GetMD5Hash(password);

                    for (int i = 1; i < csvHeight; i++)
                    {
                        if (UserName == importer.GetCell(0, i))
                        {
                            string dataHash = importer.GetCell(1, i);

                            if (hash == dataHash)
                            {

                                // Debug.Log("hash is correct");

                                string permissionsName = importer.GetCell(2, i);

                                permissionsName = permissionsName.Trim();

                                Permissions permissions = null;

                                if (permissionsName != null && permissionsName != string.Empty)
                                {
                                    if (WarSystem.AllPermissions.TryGetValue(permissionsName, out permissions))
                                    {
                                        string SystemID = importer.GetCell(6, i);

                                        if (string.IsNullOrWhiteSpace(SystemID))
                                            throw new NotSupportedException("the system id cannot be null or empty");

                                        a = new Account(UserName, dataHash, permissions, importer.GetCell(3, i), importer.GetCell(4, i), importer.GetCell(5, i), SystemID);

                                        // Debug.Log("acct created - logging in");

                                        // EmailClient.SendNotificationSMTPEmailToDev(a.UserName + " - \'" + serverName + "\'", "Logging In\n" + a.FirstName + " " + a.LastName + "\n" + WarSystem.ConnectedDeviceStampNoServerName + "\nServer: \'" + serverName + "\'");
                                        WarSystem.WriteToDev($"{a.UserName} is logging in to {WarSystem.ConnectedServerName} ", Logging.MessageType.logEvent);
                                        WarSystem.Login(a);
                                        return "success";
                                    }
                                    else
                                    {
                                        NotificationHandler.Print("Login Failed: The permissions \'" + permissionsName + "\' does not exist.");
                                    }
                                }
                                // Debug.Log("creating acct");
                            }

                            // Debug.Log("could not find hash");
                        }

                        // Debug.Log("nope");
                    }

                    a = null;
                    return "The user name or password combo is not correct";
                }

                // Debug.Log("Server connection failed");

                a = null;
                return "Server Connection Failed";
            }
            catch (System.IO.IOException ex)
            {
                MessageBoxHandler.Print_Immediate($"Not Connected. Make sure you are connected to the server and no applications are controlling the files.", "Error");
                EmailClient.SendNotificationSMTPEmailToDev("Login IO Error", "Attempt to login failed:\nUser: " + UserName + "\nError: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBoxHandler.Print_Immediate($"Could not Log in\n{ex.Message}", "Error");
                EmailClient.SendNotificationSMTPEmailToDev("Login General Error", "Attempt to login failed:\nUser: " + UserName + "\nError: " + ex.Message);
            }


            a = null;
            return "Could not log in";
        }

        /// <summary>
        /// Used to verfiy that an administrator has allowed a certian action
        /// </summary>
        /// <param name="userName">the user name of the administrator</param>
        /// <param name="password">the password of the administrator</param>
        /// <returns></returns>
        public static bool TryAdminVerifyCredentials(string userName, string password, out string response)
        {
            try
            {
                if (WarSystem.AttemptConnectionToServer())
                {
                    WarSystem.LoadPermissions();

                    var importer = new Import_CSV();
                    importer.Import(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv");

                    int csvHeight = importer.Height;

                    string hash = Encryption.GetMD5Hash(password);

                    for (int i = 1; i < csvHeight; i++)
                    {
                        if (userName == importer.GetCell(0, i))
                        {
                            //  Debug.Log("found user name");

                            string dataHash = importer.GetCell(1, i);

                            if (hash == dataHash)
                            {

                                // Debug.Log("Passwords are correct");

                                string permissionsName = importer.GetCell(2, i);

                                permissionsName = permissionsName.Trim();

                                Permissions permissions = null;

                                if (permissionsName != null && permissionsName != string.Empty)
                                {
                                    if (WarSystem.AllPermissions.TryGetValue(permissionsName, out permissions))
                                    {
                                        if (permissions.IsAdmin)
                                        {
                                            response = "Success!";
                                            return true;
                                        }
                                    }
                                }

                                // string SystemID = importer.GetCell(6, i);

                                // if (SystemID == null || SystemID == string.Empty)
                                //     throw new NotSupportedException("the system id cannot be null or empty");

                                // a = new Account(userName, dataHash, permissions, importer.GetCell(3, i), importer.GetCell(4, i), importer.GetCell(5, i), SystemID);
                            }
                        }
                    }
                    response = "The user name or password combo is not correct";
                }

                response = "Server Connection Failed";

                return false;
            }
            catch (System.IO.IOException)
            {
                MessageBoxHandler.Print_Immediate($"Not Connected. Make sure you are connected to the server.", "Error");
            }
            // catch (Exception ex)
            // {
            //     MessageBoxHandler.Print($"Could not Log in\n{ex.Message}", "Error");
            // }

            response = "Could not validate";
            return false;
        }


        /// <summary>
        /// Create the user account
        /// </summary>
        /// <returns>returns true if the user account has been created</returns>
        public static bool CreateAccount(string userName, string password, string permissionsName, string Language, string firstName, string lastName, string authorizedUserName)
        {
            Guid id = Guid.NewGuid();

            try
            {
                using (FileStream stream = new FileStream(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv", FileMode.Append))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string hash = CreatePassword(password);
                        writer.WriteLine(userName + "," + hash + "," + permissionsName + "," + Language + "," + firstName + "," + lastName + "," + id.ToString());

                        EmailClient.SendQuickEmail(userName, "Welcome To War Manager", $"Hello {firstName} {lastName},\n" +
                        "\tIt looks like the email went through, and you are now free to jump into War Manager.\n\n" +
                        "If you run into trouble, check out the website at https://warmanagerstorage.blob.core.windows.net/wmcontainerstorage/Installer%20Website/index.html . From there, we can get you back on track asap.\n\n " +
                        "Also, feel free to email via taylor.howell@jrcousa.com for anything War Manager.\n\n " +
                        "(If you received this email in error, please delete this email and notify taylor.howell@jrcousa.com).");

                        // EmailClient.SendNotificationSMTPEmailToDev("New War Manager User Alert",
                        //  $"There is a new War Manager user:\n {userName} - {firstName} {lastName}\n ({permissionsName})\n\nThe account was authorized by \'{authorizedUserName}\'");

                        WarSystem.WriteToDev($"A new user {userName} ({firstName} {lastName}) appeared! (Authorized by {authorizedUserName})", Logging.MessageType.logEvent);

                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Does an account with a given user name exist?
        /// </summary>
        /// <param name="userName">the given user name</param>
        /// <returns>returns true if the account is found, false if not</returns>
        public static bool Exists(string userName)
        {
            if (userName == null)
                throw new NullReferenceException("the user name cannot be null");

            if (userName == string.Empty)
                throw new NotSupportedException("the user name cannot be an empty string");

            if (!File.Exists(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv"))
                throw new FileNotFoundException("Cannot find accounts on server");

            using (FileStream stream = new FileStream(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv", FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string acctStr = reader.ReadLine();
                        string[] str = acctStr.Split(',');
                        if (str[0] == userName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Replace an account with a given user name into a new account with the same system key
        /// </summary>
        /// <param name="oldUserName">the old user name</param>
        /// <param name="newAccount">the new account information</param>
        /// <param name="newHashKey">the new hash key</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool ReplaceAccount(string oldUserName, Account newAccount, string newHashKey)
        {
            if (!File.Exists(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv"))
                throw new FileNotFoundException("Cannot find accounts on server");

            bool foundAcct = false;

            List<string> accts = new List<string>();

            using (FileStream stream = new FileStream(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv", FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string acctStr = reader.ReadLine();

                        string[] str = acctStr.Split(',');

                        if (str[0] == oldUserName && !foundAcct)
                        {
                            foundAcct = true;
                            acctStr = newAccount.UserName + "," + newHashKey + "," + newAccount.PermissionsName + "," + newAccount.UserSelectedLanguage + "," + newAccount.FirstName + "," + newAccount.LastName + "," + str[str.Length - 1];
                        }

                        accts.Add(acctStr);
                    }
                }
                if (!foundAcct)
                {
                    return false;
                }

                stream.Position = 0;

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    for (int i = 0; i < accts.Count; i++)
                    {
                        writer.WriteLine(accts[i]);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create a hashed key from a password (one - way encryption)
        /// </summary>
        /// <param name="password">the password to hash</param>
        /// <returns>returns a hashed key</returns>
        public static string CreatePassword(string password)
        {
            return Encryption.GetMD5Hash(password);
        }

        /// <summary>
        /// Get the current account key as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetCurrentAccountHashKeyByteArray()
        {
            return System.Text.Encoding.UTF8.GetBytes(HashKey);
        }

        /// <summary>
        /// Get the list of accounts
        /// </summary>
        /// <returns>a list of accounts</returns>
        public static List<Account> GetAccountsList()
        {
            return GetAccountsList(out var x);
        }

        /// <summary>
        /// Get the list of acounts for non greedy situations (like sheets)
        /// </summary>
        /// <param name="permissions">the permissions</param>
        /// <returns>returns the list of accounts</returns>
        public static List<Account> GetAccountsListByPermissions(List<Permissions> permissionsList, bool greedy)
        {
            List<string> categories = new List<string>();

            foreach (var x in permissionsList)
            {
                categories.AddRange(x.Categories);
            }

            if (greedy)
            {
                return GetAccountsListGreedy(categories.ToArray());
            }
            else
            {

                return GetAccountsListNonGreedy(categories.ToArray());
            }
        }

        /// <summary>
        /// Get the list of accounts that is able to access the categories (not greedy)
        /// </summary>
        /// <param name="categories">the string of categories</param>
        /// <returns>returns the list of accounts that contains the categories</returns>
        public static List<Account> GetAccountsListNonGreedy(string[] categories)
        {
            if (categories == null)
                categories = new string[0];

            var allAccounts = GetAccountsList();

            var selectedAccounts = new List<Account>();

            foreach (var account in allAccounts)
            {
                if (account.ContainsAllCategoriesAccessCharacter || account.IsAdmin)
                {
                    selectedAccounts.Add(account);
                }
                else
                {
                    bool found = false;
                    int i = 0;

                    while (!found && i < categories.Length)
                    {
                        if (account.FullAccessCategories.Contains(categories[i]))
                        {
                            found = true;
                            selectedAccounts.Add(account);
                        }

                        i++;
                    }
                }
            }

            return selectedAccounts;
        }

        /// <summary>
        /// Get the list of accounts that is able to access the categories (not greedy)
        /// </summary>
        /// <param name="categories">the string of categories</param>
        /// <returns>returns the list of accounts that contains the categories</returns>
        public static List<Account> GetAccountsListGreedy(string[] categories)
        {
            if (categories == null)
                categories = new string[0];

            var allAccounts = GetAccountsList();

            var selectedAccounts = new List<Account>();

            foreach (var account in allAccounts)
            {
                if (account.ContainsAllCategoriesAccessCharacter || account.IsAdmin)
                {
                    selectedAccounts.Add(account);
                }
                else
                {
                    int i = 0;

                    while (i < categories.Length)
                    {
                        if (account.FullAccessCategories.Contains(categories[i]))
                        {

                        }

                        i++;
                    }

                    if (i == categories.Length)
                        selectedAccounts.Add(account);
                }
            }

            return selectedAccounts;
        }


        /// <summary>
        /// Get the list of accounts
        /// </summary>
        /// <param name="dataFile">get the data file that generated the accounts</param>
        /// <returns>a list of accounts</returns>
        private static List<Account> GetAccountsList(out DataFileInstance dataFile)
        {
            string path = GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv";

            DataFileInstance csvAccountsInfo = DataFileInstance.DeserializeCSVFile(path);

            dataFile = csvAccountsInfo;

            List<Account> accounts = new List<Account>();

            foreach (var x in csvAccountsInfo.Data)
            {
                bool cannotUse = false;

                for (int i = 0; i < x.Length; i++)
                {
                    if (string.IsNullOrEmpty(x[i]))
                        cannotUse = true;
                }

                if (!cannotUse && WarSystem.AllPermissions.TryGetValue(x[2], out var permissions))
                {
                    Account a = new Account(x[0], x[1], permissions, x[3], x[4], x[5], x[6]);
                    accounts.Add(a);
                }
                else
                {
                    //could not load permissions or something does not exist...
                }
            }

            return accounts;
        }



        /// <summary>
        /// Attempt to login to War Manager (v2)
        /// </summary>
        /// <param name="userName">the user name</param>
        /// <param name="passKey">the password</param>
        /// <param name="a">the resulting account (if logged in)</param>
        /// <param name="message">the message telling what happened</param>
        /// <returns>returns true if the login was successful, false if not</returns>
        /// <exception cref="NullReferenceException">thrown when the <paramref name="userName"/> or <paramref name="passKey"/> string is null</exception>
        /// <exception cref="ArgumentException">thrown when the <paramref name="userName"/> or <paramref name="passKey"/> is an empty string</exception>
        public static bool AttemptLoginToWarManager(string userName, string passKey, out Account a, out string message)
        {

            if (userName == null)
                throw new System.NullReferenceException(nameof(userName) + " is null");

            if (userName == string.Empty)
                throw new System.ArgumentException(nameof(userName) + " is empty");

            if (passKey == null)
                throw new System.NullReferenceException(nameof(passKey) + " is null");

            if (passKey == string.Empty)
                throw new System.ArgumentException(nameof(passKey) + " is empty");

            string key = string.Empty;


            key = Encryption.GetMD5Hash(passKey);


            try
            {

               WarSystem.HandleConnection(out var serverName);

                // Debug.Log("connection handled");

               WarSystem.LoadPermissions(); // do we need this?

                //Debug.Log("permissions loaded");

                var accounts = GetAccountsList();

                //Debug.Log("found accounts");

                var selectedAccount = accounts.Find(x => x.UserName == userName && x.HashKey == key);

                if (selectedAccount != null)
                {
                    //Debug.Log("sending email");

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        EmailClient.SendNotificationSMTPEmailToDev(selectedAccount.UserName + " - \'" + serverName +
                         "\'", "Logging In\n" + selectedAccount.FirstName + " " + selectedAccount.LastName +
                         "\n" + WarSystem.ConnectedDeviceStampNoServerName + "\nServer: \'" + serverName + "\'");
                    });

                    //Debug.Log("found account");

                   WarSystem.Login(selectedAccount);
                    a = selectedAccount;

                    // Debug.Log("logged in"); 

                    //WarSystem.WriteToDev($"{a.UserName} just logged in to {WarSystem.ConnectedServerName}.", Logging.MessageType.logEvent);
                    message = "Success!";

                    return true;
                }
                else
                {
                    message = "The user name or password is incorrect.";

                    a = null;
                    return false;
                }
            }
            catch (System.IO.IOException ex)
            {
                MessageBoxHandler.Print_Immediate($"Not Connected. Make sure you are connected to the server and no applications are controlling the files.", "Error");
                EmailClient.SendNotificationSMTPEmailToDev("Login IO Error", "Attempt to login failed:\nUser: " + userName + "\nError: " + ex.Message);
                WarSystem.WriteToDev("Login Failed " + userName + " - IO error", Logging.MessageType.critical);
            }
            catch (Exception ex)
            {
                MessageBoxHandler.Print_Immediate($"Could not Log in\n{ex.Message}", "Error");
                EmailClient.SendNotificationSMTPEmailToDev("Login General Error", "Attempt to login failed:\nUser: " + userName + "\nError: " + ex.Message);
                WarSystem.WriteToDev("Login Failed " + userName + " - some non-IO error", Logging.MessageType.error);
            }


            message = "There was an error";

            a = null;
            return false;
        }

        /// <summary>
        /// change the password
        /// </summary>
        /// <param name="oldPassword">the old password</param>
        /// <param name="newPassword">the new password</param>
        /// <exception cref="NullReferenceException">thrown when the <paramref name="oldPassword"/> or <paramref name="newPassword"/> string is null</exception>
        /// <exception cref="ArgumentException">thrown when the <paramref name="oldPassword"/> or <paramref name="newPassword"/> is an empty string</exception>
        public bool ChangePassword(string oldPassword, string newPassword)
        {

            if (oldPassword == null)
                throw new System.NullReferenceException(nameof(oldPassword) + " is null");

            if (oldPassword == string.Empty)
                throw new System.ArgumentException(nameof(oldPassword) + " is empty");

            if (newPassword == null)
                throw new System.NullReferenceException(nameof(newPassword) + " is null");

            if (newPassword == string.Empty)
                throw new System.ArgumentException(nameof(newPassword) + " is empty");


            if (HashKey == oldPassword)
            {
                HashKey = newPassword;

                try
                {
                    bool x = SerializeAccountChanges(this);

                    if (x)
                    {
                        EmailClient.SendNotificationSMTPEmailToDev($"Account Password Changed {UserName}", $"Account password was changed:\n{ToString(true)} Server: {WarSystem.ConnectedDeviceStamp}");
                        WarSystem.WriteToDev($"Account Password Changed for {UserName} Succeeded", Logging.MessageType.logEvent);
                        WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                    }
                    else
                    {
                        EmailClient.SendNotificationSMTPEmailToDev($"Account Password Changed Failed {UserName}", $"Account password change attempt Failed:\n{ToString(true)} Server: {WarSystem.ConnectedDeviceStamp}");
                        WarSystem.WriteToDev($"Account Password Changed for {UserName} Failed - incorrect format??", Logging.MessageType.logEvent);
                    }
                }
                catch (Exception ex)
                {
                    EmailClient.SendNotificationSMTPEmailToDev($"Account Password Changed Failed (Error) {UserName}", $"Account password change attempt Failed:\n{ToString(true)} Server: {WarSystem.ConnectedDeviceStamp} \n\nError: {ex.Message}");
                    WarSystem.WriteToDev($"Account Password Changed for {UserName} Failed - general error", Logging.MessageType.error);
                }
            }
            else
            {
                throw new Exception("old password is not correct");
            }

            return false;
        }

        /// <summary>
        /// Change the permissions of a selected account
        /// </summary>
        /// <param name="accountToModify">the account to modify</param>
        /// <param name="newPermissions">the new permissions</param>
        /// <param name="currentAccount">the current account</param>
        /// <returns>returns true if the account modification was successful, false if not</returns>
        public static bool ChangeAccountPermissions(Account accountToModify, Permissions newPermissions, Account currentAccount)
        {
            if (!currentAccount.Permissions.IsAdmin)
                return false;

            //Debug.Log("changed permissions - now serializing");

            WarSystem.WriteToLog($"Account permissions changed from {accountToModify.Permissions} to {newPermissions}: {accountToModify.UserName}", Logging.MessageType.logEvent);
            WarSystem.WriteToDev($"Account permissions changed from {accountToModify.Permissions} to {newPermissions}: {accountToModify.UserName}", Logging.MessageType.logEvent);
            WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;

            accountToModify.Permissions = newPermissions;

            //Debug.Log("changed permissions - now serializing");

            return SerializeAccountChanges(accountToModify);
        }


        /// <summary>
        /// Generate a random password, reset the password and email the user
        /// </summary>
        /// <param name="account">the account</param>
        /// <param name="currentAccount">the current account that is calling the reset password</param>
        /// <returns>returns true if the password changing was successful, and email was sent, false if not</returns>
        public static bool ResetPassword(Account account, Account currentAccount)
        {
            if (!currentAccount.Permissions.IsAdmin)
                return false;

            string word = WarManager.Special.RandomWordFetcher.GetRandomWord();
            word += WarManager.Special.RandomWordFetcher.GetRandomWord();
            word += "@Wm25";

            var resetPassword = Encryption.GetMD5Hash(word);

            if (EmailClient.SendSMTPEmail(new string[1] { account.UserName }, "Password Reset", "Hi " + account.FirstName + ",\n\nHere is your temporary password:\n\n " + resetPassword, true, false))
            {
                account.HashKey = resetPassword;
                return SerializeAccountChanges(account);
            }


            WarSystem.WriteToLog($"Password reset for {account.UserName}", Logging.MessageType.logEvent);
            WarSystem.WriteToDev($"Password reset for {account.UserName}", Logging.MessageType.logEvent);
            WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;

            return false;
        }

        /// <summary>
        /// Delete an account from the accounts list in War Manager
        /// </summary>
        /// <param name="accountToModify">the account to modify</param>
        /// <param name="currentAccount">the current account</param>
        /// <returns>returns true if the account was removed, false if not</returns>
        public static bool RemoveAccount(Account accountToModify, Account currentAccount)
        {
            if (!currentAccount.Permissions.IsAdmin)
                return false;

            if (accountToModify == currentAccount) //can't delete your own account
                return false;

            var accounts = GetAccountsList(out var dataFileInstance);

            for (int i = 0; i < accounts.Count; i++)
            {
                if (accounts[i].AccountID == accountToModify.AccountID)
                {

                    WarSystem.WriteToLog($"Account Deleted for {accountToModify.UserName} by {currentAccount.UserName}", Logging.MessageType.logEvent);
                    WarSystem.WriteToDev($"Account Deleted for {accountToModify.UserName} by {currentAccount.UserName}", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;

                    dataFileInstance.RemoveData(i);
                    return dataFileInstance.SerializeFile();
                }
            }

            return false;
        }

        /// <summary>
        /// Serialize the account changes
        /// </summary>

        public static bool SerializeAccountChanges(Account account)
        {
            Debug.Log("grabbing accounts");

            string path = GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv";

            var accounts = GetAccountsList(out var dataFileInstance);

            Debug.Log("grabbed accounts 2");

            Debug.Log(accounts.Count);

            var data = dataFileInstance.GetAllData();

            for (int j = 0; j < data.Count; j++)
            {

                if (data[j][6] == account.AccountID)
                {
                    string[] accountData = new string[7] { account.UserName, account.HashKey,
                            account.PermissionsName, account.UserSelectedLanguage.ToString(), account.FirstName, account.LastName, account.AccountID };

                    //throw new NotImplementedException("the account settings need to be refactored");

                    dataFileInstance.ReplaceData(j, accountData);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;

                    dataFileInstance.SerializeFile();

                    //Debug.Log("serialized");
                    return true;
                }
            }

            return false;
        }

        public int CompareTo(Account other)
        {
            if (other == null)
                return 1;

            return this.AccountID.CompareTo(other.AccountID);
        }

        public bool Equals(Account other)
        {

            if (other != null && other.AccountID == this.AccountID)
                return true;

            return false;
        }

        public override string ToString()
        {
            return $"{UserName} - ({Permissions.Name}). Real Name: {FirstName}, {LastName}. ID: ({AccountID})";
        }

        public string ToString(bool addNewLineCharacters)
        {
            if (!addNewLineCharacters)
                return $"{UserName} - ({Permissions.Name}). Real Name: {FirstName}, {LastName}. ID: ({AccountID})";

            return $"{UserName} - ({Permissions.Name})\n Real Name: {FirstName}, {LastName}\n ID: ({AccountID})";
        }
    }
}
