

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Sharing;
using WarManager.Sharing.Security;
using WarManager.Backend;
using WarManager;

using WarManager.Unity3D.Windows;
using System.Linq;

namespace WarManager.Unity3D
{
    public class PermissionsUI : MonoBehaviour
    {

        /// <summary>
        /// Show Permissions
        /// </summary>
        public void ShowPermissions(Action back, string backTitle)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Permissions", 20));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            if (WarSystem.CurrentActiveAccount.IsAdmin)
            {
                EditPermissionsThatUserCanView(content, () => { ShowPermissions(back, backTitle); }, "Permissions");
                content.Add(new SlideWindow_Element_ContentInfo(20));
                content.Add(new SlideWindow_Element_ContentInfo("Add New Permissions", () =>
                {
                    Permissions permissions = new Permissions("New Permissions", false, new string[1] { "*" });
                    EditPermissions(permissions, () => { ShowPermissions(back, backTitle); }, "Permissions", false);
                }, ActiveSheetsDisplayer.main.AddSprite));
            }
            else
            {
                ShowPermissionsThatUserCanView(content);
            }

            SlideWindowsManager.main.AddReferenceContent(content);
        }

        /// <summary>
        /// Show Permissions that you can view
        /// </summary>
        private void ShowPermissionsThatUserCanView(List<SlideWindow_Element_ContentInfo> content)
        {
            List<Permissions> permissions = new List<Permissions>();

            foreach (var x in WarSystem.AllPermissions)
            {
                permissions.Add(x.Value);
                content.Add(new SlideWindow_Element_ContentInfo(x.Value.Name, string.Join(", ", x.Value.Categories)));
            }
        }

        /// <summary>
        /// show permissions that the user can edit
        /// </summary>
        /// <param name="content"></param>
        private void EditPermissionsThatUserCanView(List<SlideWindow_Element_ContentInfo> content, Action back, string backTitle)
        {
            List<Permissions> permissions = new List<Permissions>();

            WarSystem.LoadPermissions();

            foreach (var x in WarSystem.AllPermissions)
            {
                if (x.Value != null)
                {
                    permissions.Add(x.Value);
                }
            }

            permissions.Sort((x, y) =>
            {
                return x.Name.CompareTo(y.Name);
            });

            foreach (var x in permissions)
            {
                string admin = "";
                if (x.IsAdmin)
                {
                    admin = "(admin) ";
                }

                content.Add(new SlideWindow_Element_ContentInfo($"{x.Name}", admin + string.Join(", ", x.Categories)));
                content.Add(new SlideWindow_Element_ContentInfo($"Edit {x.Name}", () =>
                {
                    if (WarSystem.CurrentActiveAccount.IsAdmin)
                        EditPermissions(x, back, backTitle);
                    else
                        MessageBoxHandler.Print_Immediate("You do not have administrator access to edit permissions.", "Note");
                }));
                content.Add(new SlideWindow_Element_ContentInfo(20));
            }
        }

        /// <summary>
        /// Edit the permissions
        /// </summary>
        /// <param name="permissions">the permissions</param>
        private void EditPermissions(Permissions permissions, Action back, string backTitle, bool canDelete = true)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            if (!WarSystem.CurrentActiveAccount.IsAdmin)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Denied.", "You do not have administrator access to edit permissions."));
                SlideWindowsManager.main.AddReferenceContent(content);
                return;
            }

            content.Add(new SlideWindow_Element_ContentInfo("Edit Name", permissions.Name, (x) =>
            {
                try
                {
                    Permissions p = new Permissions(x, permissions.IsAdmin, permissions.Categories.ToArray(), permissions.FilePath);
                    WarSystem.WriteToLog($"Permissions: {permissions} changed to {p} ", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                    //EditPermissions(p, back, backTitle, canDelete);
                    permissions = p;
                }
                catch (Exception ex)
                {
                    MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                }
            }));

            string admin = "Yes";

            if (!permissions.IsAdmin)
                admin = "No";

            var isAdminContent = new SlideWindow_Element_ContentInfo("Administrator?", admin, (x) =>
            {
                if (x == "Yes")
                {
                    var p = new Permissions(permissions.Name, true, permissions.Categories.ToArray(), permissions.FilePath);
                    WarSystem.WriteToLog($"Permissions: {permissions} changed to {p} ", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                    permissions = p;
                }
                else
                {
                    var p = new Permissions(permissions.Name, false, permissions.Categories.ToArray(), permissions.FilePath);
                    WarSystem.WriteToLog($"Permissions: {permissions} changed to {p} ", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                    permissions = p;
                }
            });

            isAdminContent.ContentType = ColumnInfo.GetValueTypeOfKeyword;
            isAdminContent.ContentOptions = new List<string>() { "Yes", "No" };

            content.Add(isAdminContent);

            content.Add(new SlideWindow_Element_ContentInfo("Categories", "Adding a star '*' at the very beginning tells War Manager that the user can see everything"));

            var categoryInfo = new SlideWindow_Element_ContentInfo("Edit Categories", string.Join(",", permissions.Categories), (x) =>
                {
                    //here we change the value of the permissions

                    bool save = true;

                    for (int i = 0; i < x.Length; i++)
                    {
                        if (!char.IsLetter(x[i]) && !char.IsNumber(x[i]) && !char.IsWhiteSpace(x[i]) && x[i] != ',' && x[i] != '*')
                        {
                            save = false;
                            MessageBoxHandler.Print_Immediate("Cannot save categories because the category contains an invalid character at " + i.ToString() + " " + x[i].ToString(), "Error");
                        }
                    }

                    if (save)
                    {
                        string[] categories = x.Split(',');

                        try
                        {
                            Permissions p = new Permissions(permissions.Name, permissions.IsAdmin, categories, permissions.FilePath);

                            WarSystem.WriteToLog($"Permissions Categories: {permissions} changed to {p} ", Logging.MessageType.logEvent);
                            WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                            permissions = p;
                        }
                        catch (Exception ex)
                        {
                            MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                        }
                    }
                });

            categoryInfo.DescriptionHeader = "Categories";
            categoryInfo.DescriptionInfo = "Content must be only letters, numbers, '*', and spaces; and separated by a comma.\nExample: Foreman,Sheet Metal,2035 Planning";

            content.Add(categoryInfo);

            content.Add(new SlideWindow_Element_ContentInfo("Save", () =>
            {
                try
                {
                    Permissions.SavePermissions(permissions, WarSystem.CurrentActiveAccount);
                    WarSystem.AllPermissions[permissions.Name] = permissions;

                    WarSystem.WriteToLog($"Saved Permissions: {permissions}", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;

                    MessageBoxHandler.Print_Immediate("Save Successful", "Note");
                    back();
                }
                catch (Exception ex)
                {
                    MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                }
            }, null));

            if (canDelete)
            {
                content.Add(new SlideWindow_Element_ContentInfo(40));
                content.Add(new SlideWindow_Element_ContentInfo("Delete", () =>
                {
                    WarSystem.WriteToLog($"Deleting Permissions: {permissions}", Logging.MessageType.logEvent);
                    WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                    Permissions.DeletePermissions(permissions, back);
                }, ActiveSheetsDisplayer.main.DeleteForeverSprite));
            }

            SlideWindowsManager.main.AddReferenceContent(content);
        }

        public void ShowPermissionsByName(List<string> permissionsNames, string backTitle, Action back)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(30));

            foreach (var x in Permissions.GetPermissionsByName(permissionsNames))
            {

                string admin = "";
                if (x.IsAdmin)
                    admin = " (admin)";

                content.Add(new SlideWindow_Element_ContentInfo(x.Name + admin, string.Join(",", x.Categories)));
                content.Add(new SlideWindow_Element_ContentInfo("View Who Can See...", () =>
                {
                    ShowAccounts(Account.GetAccountsList(), x.Name);
                }, null));
                content.Add(new SlideWindow_Element_ContentInfo(20));
            }

            SlideWindowsManager.main.AddReferenceContent(content);
        }


        /// <summary>
        /// Show the accounts
        /// </summary>
        /// <param name="accounts">the list of accounts</param>
        public void ShowAccounts(List<Account> accounts, string permissionsName)
        {
            List<SlideWindow_Element_ContentInfo> contentInfo = new List<SlideWindow_Element_ContentInfo>();
            contentInfo.Add(new SlideWindow_Element_ContentInfo("Back", ActiveSheetsDisplayer.main.ViewReferences, ActiveSheetsDisplayer.main.BackSprite));
            contentInfo.Add(new SlideWindow_Element_ContentInfo(30));
            contentInfo.Add(new SlideWindow_Element_ContentInfo("Users with " + permissionsName + " permissions", 20));
            foreach (var x in accounts)
            {
                contentInfo.Add(new SlideWindow_Element_ContentInfo(x.UserName, x.Permissions.Name));
                contentInfo.Add(new SlideWindow_Element_ContentInfo(20));
            }

            SlideWindowsManager.main.AddReferenceContent(contentInfo);
        }
    }
}
