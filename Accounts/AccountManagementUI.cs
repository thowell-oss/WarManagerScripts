
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Sharing;
using WarManager.Sharing.Security;

using WarManager.Backend;
using WarManager.Unity3D.Windows;
using StringUtility;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Manage accounts via admin access
    /// </summary>
    [Notes.Author("Manage accounts via admin access")]
    public class AccountManagementUI
    {
        /// <summary>
        /// Populate all accounts on this server
        /// </summary>
        /// <param name="back">the back button</param>
        /// <param name="backTitle">the back title</param>
        public void ShowAllAccounts(Action back, string backTitle)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(30));

            GetAccounts(content, back, backTitle);

            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo("Add Account", () => { MessageBoxHandler.Print_Immediate("Coming soon,", "Note"); }, ActiveSheetsDisplayer.main.AddSprite));

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }


        /// <summary>
        /// Get the accounts to look at
        /// </summary>
        /// <param name="content">the content</param>
        /// <param name="back">back button</param>
        /// <param name="backTitle">back button title</param>
        private void GetAccounts(List<SlideWindow_Element_ContentInfo> content, Action back, string backTitle)
        {
            foreach (var x in Account.GetAccountsList())
            {
                string admin = "";
                if (x.IsAdmin)
                    admin = "(admin)";

                content.Add(new SlideWindow_Element_ContentInfo(x.UserName, x.PermissionsName + " <size=70%>" + admin));

                // content.Add(new SlideWindow_Element_ContentInfo("Edit " + x.UserName.SetStringQuotes(), () => { EditAccount(x, () => { ShowAllAccounts(back, backTitle); }, "Accounts"); }, ActiveSheetsDisplayer.main.editCardSheetSprite));
                content.Add(new SlideWindow_Element_ContentInfo("Edit", () => { ShowAccountInfo(x, true, WarSystem.CurrentActiveAccount); }));
                // content.Add(new SlideWindow_Element_ContentInfo("Reset Password", () => { ResetPassword(x); }, null));
                content.Add(new SlideWindow_Element_ContentInfo(20));
            }
        }

        /// <summary>
        /// Edit the account properties
        /// </summary>
        /// <param name="account">the account</param>
        /// <param name="back">the back button</param>
        /// <param name="backTitle">the back title</param>
        private void EditAccount(Account account, Action back, string backTitle)
        {
            if (!WarSystem.CurrentActiveAccount.IsAdmin)
            {
                MessageBoxHandler.Print_Immediate("You must be an administrator to edit " + account.UserName.SetStringQuotes(), "Error");
                return;
            }


            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));

            var preferences = UserPreferencesHandler.GetUserPreferences(account);

            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo(account.UserName, null));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Phone Number", preferences.PhoneNumber, (x) =>
            {
                if (x != null)
                {

                }
            }));
            content.Add(new SlideWindow_Element_ContentInfo("Verify New Phone Number", () => { HandlePhoneVerification(preferences.PhoneNumber, preferences, account, WarSystem.CurrentActiveAccount); }));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Permissions", account.PermissionsName));
            content.Add(new SlideWindow_Element_ContentInfo("Change Permissions", () => { EditAccountPermissions(account, () => { EditAccount(account, back, backTitle); }, "Edit " + account.UserName.SetStringQuotes()); }, ActiveSheetsDisplayer.main.PermissionsSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Password", 20));
            content.Add(new SlideWindow_Element_ContentInfo("Reset Password", () => { ResetPassword(account); }, null));

            content.Add(new SlideWindow_Element_ContentInfo(40));
            content.Add(new SlideWindow_Element_ContentInfo("Save Changes", () => { Account.SerializeAccountChanges(account); back(); }, null));

            content.Add(new SlideWindow_Element_ContentInfo(30));
            if (account != WarSystem.CurrentActiveAccount)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Delete Account", () => { DeleteAccount(account); back(); }, ActiveSheetsDisplayer.main.DeleteForeverSprite));
            }
            else
            {
                content.Add(new SlideWindow_Element_ContentInfo("Where is the delete my account button?", "Currently you cannot delete your own account. If this is an issue, contact support."));
            }

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Edit account permissions
        /// </summary>
        /// <param name="account">the account to edit</param>
        /// <param name="back">the back action</param>
        /// <param name="backTitle">the back title</param>
        private void EditAccountPermissions(Account account, Action back, string backTitle)
        {

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(30));

            content.Add(new SlideWindow_Element_ContentInfo(account.Permissions.Name, string.Join(", ", account.Permissions.Categories)));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            WarSystem.LoadPermissions();
            foreach (var x in WarSystem.AllPermissions)
            {
                if (x.Value != account.Permissions)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(x.Value.Name, string.Join(", ", x.Value.Categories)));
                    content.Add(new SlideWindow_Element_ContentInfo($"Set {account.UserName.SetStringQuotes()} permissions", () => { SetPermissions(account, x.Value, back); }, null));
                    content.Add(new SlideWindow_Element_ContentInfo(20));
                }
            }

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        /// <param name="account">the account</param>
        /// <param name="permissions">the permissions values</param>
        private void SetPermissions(Account account, Permissions permissions, Action refreshAction)
        {
            MessageBoxHandler.Print_Immediate($"Are you sure you want to set the {account.UserName.SetStringQuotes()} account permissions to {permissions.Name}?", "Question", (x) =>
            {
                if (x)
                {
                    try
                    {

                        if (Account.ChangeAccountPermissions(account, permissions, WarSystem.CurrentActiveAccount))
                        {
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Permission change successful", "Success");
                                refreshAction();
                                return;
                            });
                        }
                        else
                        {
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Permission change failed - contact support", "Error");
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate($"Permission change failed -contact support\n{ex.Message}", "Error");
                            });
                    }
                }
            });
        }

        /// <summary>
        /// Reset the password
        /// </summary>
        /// <param name="account"></param>
        private void ResetPassword(Account account)
        {

            if (!WarSystem.CurrentActiveAccount.IsAdmin)
            {

                MessageBoxHandler.Print_Immediate("You must be an administrator to reset the password for " + account.UserName.SetStringQuotes(), "Error");
                return;
            }

            MessageBoxHandler.Print_Immediate($"Reset password for {account.UserName}?", "Question", (x) =>
            {

                if (x)
                {
                    if (Account.ResetPassword(account, WarSystem.CurrentActiveAccount))
                    {
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Password reset and email sent to user", "Success");
                        });
                    }
                    else
                    {
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Failed to reset password and send email to user. Contact support.", "Error");
                        });
                    }
                }

            });
        }

        /// <summary>
        /// Delete the account
        /// </summary>
        /// <param name="account"></param>
        private void DeleteAccount(Account account)
        {
            if (!WarSystem.CurrentActiveAccount.IsAdmin)
            {

                MessageBoxHandler.Print_Immediate("You must be an administrator to delete " + account.UserName.SetStringQuotes(), "Error");
                return;
            }

            if (account == WarSystem.CurrentActiveAccount)
            {
                MessageBoxHandler.Print_Immediate("Cannot delete your own account while logged in.", "Error");
                return;
            }

            MessageBoxHandler.Print_Immediate($"Delete {account.UserName}?", "Question", (x) =>
            {

                if (x)
                {
                    if (Account.RemoveAccount(account, WarSystem.CurrentActiveAccount))
                    {
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Account Deleted", "Success");
                        });
                    }
                    else
                    {
                        LeanTween.delayedCall(1, () =>
                        {
                            MessageBoxHandler.Print_Immediate("Failed to delete account. Contact support.", "Error");
                        });
                    }
                }

            });
        }

        /// <summary>
        /// Show your account information
        /// </summary>
        /// <param name="ForceWindowPaneOpen">should the window pane be forced open?</param>
        public void ShowAccountInfo(Account account, bool ForceWindowPaneOpen, Account currentAccount)
        {

            var contentList = new List<SlideWindow_Element_ContentInfo>();

            contentList.Add(new SlideWindow_Element_ContentInfo("References", ActiveSheetsDisplayer.main.ViewReferences, null));
            contentList.Add(new SlideWindow_Element_ContentInfo(20));

            if (account != currentAccount && !currentAccount.IsAdmin)
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Nope", "You do not have administrator access"));
                SlideWindowsManager.main.AddReferenceContent(contentList, ForceWindowPaneOpen);
                return;
            }

            //bool foundPhone = false;

            if (account != null)
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Account Info", 20));
                contentList.Add(new SlideWindow_Element_ContentInfo(20));
                contentList.Add(new SlideWindow_Element_ContentInfo("User Name", account.UserName));

                if (WarSystem.AccountPreferences.PhoneNumber != null && WarSystem.AccountPreferences.PhoneNumber.Trim().Length > 0)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("Phone", WarSystem.AccountPreferences.PhoneNumber));
                    // foundPhone = true;
                }

                contentList.Add(new SlideWindow_Element_ContentInfo("Real Name", account.FirstName + " " + account.LastName));
                contentList.Add(new SlideWindow_Element_ContentInfo("Permissions", account.PermissionsName));
                // contentList.Add(new SlideWindow_Element_ContentInfo("View Permissions", 0, (x) =>
                // {
                //     ActiveSheetsDisplayer.main.ViewPermissions(() =>
                //      {
                //          ShowAccountInfo(account, true, currentAccount);
                //      }, $"Account Settings");
                // }, ActiveSheetsDisplayer.main.PermissionsSprite));
                contentList.Add(new SlideWindow_Element_ContentInfo($"Change {account.UserName} Permissions", () =>
                {
                    EditAccountPermissions(account, () =>
                    {
                        ShowAccountInfo(account, ForceWindowPaneOpen, currentAccount);

                    }, account.UserName);

                }, ActiveSheetsDisplayer.main.PermissionsSprite));

                contentList.Add(new SlideWindow_Element_ContentInfo(20));
                contentList.Add(new SlideWindow_Element_ContentInfo("Details", 20));
                var str = string.Join(", ", account.FullAccessCategories);
                contentList.Add(new SlideWindow_Element_ContentInfo("Categories", str));
                contentList.Add(new SlideWindow_Element_ContentInfo("Admin?", account.IsAdmin.ToString()));
                contentList.Add(new SlideWindow_Element_ContentInfo("Language", account.UserSelectedLanguage.ToString()));
                contentList.Add(new SlideWindow_Element_ContentInfo(50));

                if (account == currentAccount)
                    contentList.Add(new SlideWindow_Element_ContentInfo("Change Password", 0, (x) => { PasswordChangeHandler.HandlePasswordChange(); },
                        null));

                if (account != currentAccount && currentAccount.IsAdmin)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo("Reset Password", () => { ResetPassword(account); }));
                }

                UserPreferencesHandler.CheckAutomaticLogin();

                if (account == currentAccount)
                {

                    if (UserPreferencesHandler.AutomaticLoginEnabled)
                    {
                        contentList.Add(new SlideWindow_Element_ContentInfo("Turn Off Automatic Log in", 0, (x) =>
                        {
                            UserPreferencesHandler.DisableAutomaticLogin();
                            ShowAccountInfo(account, ForceWindowPaneOpen, currentAccount);
                        }, null));
                    }
                    else
                    {
                        contentList.Add(new SlideWindow_Element_ContentInfo("Turn On Automatic Log in", 0, (x) =>
                        {
                            UserPreferencesHandler.EnableAutomaticLogin();
                            ShowAccountInfo(account, ForceWindowPaneOpen, currentAccount);
                        }, null));
                    }
                }

                if (account == currentAccount || currentAccount.IsAdmin)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Verify Phone", 20));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Why does War Manager need my Phone Number?", "In order to use some messaging capabilities, " +
                    "(like texting yourself an employee phone number), War Manager needs to know who to text."));
                    contentList.Add(new SlideWindow_Element_ContentInfo("Note", "War Manager does not sell your information to the dark web"));

                    string phone = "";

                    contentList.Add(new SlideWindow_Element_ContentInfo("Phone Number", "", (x) =>
                    {
                        phone = x;
                    }));

                    var phoneNumberInput = new SlideWindow_Element_ContentInfo("Verify Phone Number", 0, (x) => { HandlePhoneVerification(phone, WarSystem.AccountPreferences, account, currentAccount); }, null);
                    phoneNumberInput.DescriptionHeader = "Verify Phone";
                    phoneNumberInput.DescriptionInfo = "Type in your phone number and select \'Verify Phone\' below. Answer the security question and you are good to go.";
                    contentList.Add(phoneNumberInput);


                    contentList.Add(new SlideWindow_Element_ContentInfo(20));
                }

                if (account == currentAccount)
                {
                    // string text = "Turn Off Context Buttons";
                    // if (!SheetCardClusterContextHandler.Main.ActivateCluster)
                    // {
                    //     text = "Turn On Context Buttons";
                    // }

                    // contentList.Add(new SlideWindow_Element_ContentInfo(text, 0, (x) =>
                    // {
                    //     SheetCardClusterContextHandler.Main.ToggleActivateCluster();
                    //     ShowAccountInfo(account, ForceWindowPaneOpen, currentAccount);

                    // }, null));
                }

                if (account.AccountID != currentAccount.AccountID && currentAccount.IsAdmin)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(40));
                    contentList.Add(new SlideWindow_Element_ContentInfo($"Delete {account.UserName}", () => { DeleteAccount(account); }));
                }
                else if (account.AccountID == currentAccount.AccountID)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(40));
                    contentList.Add(new SlideWindow_Element_ContentInfo($"Where is the delete button?", "Currently you can only delete other accounts as an administrator, not your own. If this is an issue contact support"));
                }
            }
            else
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Not logged in", null));
            }

            SlideWindowsManager.main.AddReferenceContent(contentList, ForceWindowPaneOpen);
        }


        /// <summary>
        /// Handle the phone verification process
        /// </summary>
        /// <param name="phone"></param>
        private void HandlePhoneVerification(string phone, UserPreferences prefs, Account account, Account currentActiveAccount)
        {
            if (UnitedStatesPhoneNumber.TryParse(phone, out var result))
            {
                var y = UnityEngine.Random.Range(0, 100);
                var z = UnityEngine.Random.Range(0, 100);
                var j = UnityEngine.Random.Range(0, 1);

                while (z == y)
                    z = UnityEngine.Random.Range(0, 100);

                string message = "War Manager Phone Verification Code: " + y;

                Debug.Log(result.FullNumberUS);

                TwilioSMSHandler handler = new TwilioSMSHandler();
                handler.SendMessage(message, result.FullNumberUS, false, true);

                if (j == 0)
                {
                    MessageBoxHandler.Print_Immediate("Did you get " + y + "?", "Question", (x) =>
                    {
                        if (x)
                        {
                            prefs.PhoneNumber = result.FullNumberUS;
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Phone Number Verified", "Note");
                                WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                            });

                            UserPreferencesHandler.SavePreferences();

                            ShowAccountInfo(account, false, currentActiveAccount);
                        }
                    });
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Did you get " + z + "?", "Question", (x) =>
                    {
                        if (!x)
                        {
                            prefs.PhoneNumber = result.FullNumberUS;
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Phone Number Verified", "Note");
                                WarSystem.DeveloperPushNotificationHandler.ChangedAccountSettings = true;
                            });

                            UserPreferencesHandler.SavePreferences();

                            ShowAccountInfo(account, false, currentActiveAccount);
                        }
                    });
                }
            }
            else
            {
                MessageBoxHandler.Print_Immediate("Incorrect format - your phone number must be 10 digits long", "Error");
            }
        }

    }
}
