using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager;
using WarManager.Sharing.Security;

namespace WarManager.Unity3D
{

    public class PasswordChangeHandler
    {

        /// <summary>
        /// Change password GUI
        /// </summary>
        public static void HandlePasswordChange()
        {
            if (WarSystem.CurrentActiveAccount == null)
                return;

            EditTextMessageBoxController.OpenModalWindow("", "Enter Current Password", (x) =>
            {
                LeanTween.delayedCall(1, () =>
                {

                    string oldHash = Encryption.GetMD5Hash(x);

                    if (WarSystem.CurrentActiveAccount.HashKey == oldHash)
                    {
                        ChangePassword(oldHash);
                    }
                    else
                    {
                        HandleError("Password entered is not correct, try again?", HandlePasswordChange, null);
                    }

                });
            }, true);
        }

        /// <summary>
        /// Handle an error (with call backs)
        /// </summary>
        /// <param name="message">the error message</param>
        /// <param name="yes">yes call back</param>
        /// <param name="no">no call back</param>
        private static void HandleError(string message, System.Action yes, System.Action no)
        {
            MessageBoxHandler.Print_Immediate(message, "Error", (x) =>
            {
                if (x)
                {
                    LeanTween.delayedCall(1, () =>
                    {
                        if (yes != null)
                            yes();
                    });
                }
                else
                {
                    if (no != null)
                        no();
                }
            });
        }

        /// <summary>
        /// old password is correct - create a new password and check to make sure it is not the old password
        /// </summary>
        /// <param name="oldPassword">the old password</param>
        private static void ChangePassword(string oldPassword)
        {
            EditTextMessageBoxController.OpenModalWindow("", "Enter New Password", (x) =>
            {
                LeanTween.delayedCall(1, () =>
                {
                    EditTextMessageBoxController.OpenModalWindow("", "Enter New Password Again (x2)", (y) =>
                    {
                        LeanTween.delayedCall(1, () =>
                        {

                            if(x != y)
                            {
                                HandleError("The first and second new passwords typed are not the same, try again?", () => ChangePassword(oldPassword), null);
                                return;
                            }

                            var credentialsManager = new LoginCredentialsManager();

                            if (credentialsManager.VerifyPassword(x))
                            {
                                var newhash = Encryption.GetMD5Hash(x);

                                if (newhash == oldPassword)
                                {
                                    HandleError("Password entered is the same as the old password, try again?", () => ChangePassword(oldPassword), null);
                                    return;
                                }
                                else
                                {
                                    var a = WarSystem.CurrentActiveAccount;

                                    try
                                    {
                                        if (WarSystem.CurrentActiveAccount.ChangePassword(oldPassword, newhash))
                                        {
                                            LeanTween.delayedCall(1, () =>
                                            {
                                                MessageBoxHandler.Print_Immediate("Password Change Successful", "Boom!");
                                            });


                                            //EmailClient.SendNotificationSMTPEmailToDev(a.UserName + " - \'" + WarSystem.ConnectedServerName + "\'", "Password changed by user:\n" + a.FirstName + " " + a.LastName + "\n" + WarSystem.ConnectedDeviceStamp);
                                        }
                                        else
                                        {
                                            LeanTween.delayedCall(1, () =>
                                            {
                                                MessageBoxHandler.Print_Immediate("Error Changing Password. Check spelling and try again.", "Error!");
                                                ChangePassword(oldPassword);
                                            });
                                        }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        MessageBoxHandler.Print_Immediate(ex.Message, "Error");
                                        EmailClient.SendNotificationSMTPEmailToDev(a.UserName + " - \'" + WarSystem.ConnectedServerName + "\'", "Attempt to change password by user failed:\n" + a.FirstName + " " + a.LastName + "\n" + WarSystem.ConnectedDeviceStamp + "\nError: " + ex.Message);
                                    }
                                }
                            }
                            else
                            {
                                HandleError($"{LoginCredentialsManager.passwordInstructions} Try again?", () => ChangePassword(oldPassword), null);
                            }

                        });
                    }, true);

                });
            }, true);
        }
    }
}
