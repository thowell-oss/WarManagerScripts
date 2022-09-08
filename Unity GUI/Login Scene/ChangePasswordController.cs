using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace WarManager.Sharing.Security
{
    public class ChangePasswordController : MonoBehaviour
    {
        public TMPro.TMP_InputField UserNameTextBox;
        public TMPro.TMP_InputField PasswordTextBox;
        public TMPro.TMP_InputField Password2TextBox;

        public TMPro.TMP_Text StatusText;

        public Button ChangePasswordButton;

        public bool _newAccountVerified { get; private set; } = false;

        private LoginCredentialsManager credentialsManager;

        public void ChangePassword_ButtonListener()
        {
            if (IsNewPasswordValid())
            {
                AdminstratorVerificationHandler.Main.RequestVerification(ChangePasswordAction, "Change Password? ");
            }
        }

        void Update()
        {
            ChangePasswordButton.interactable = IsNewPasswordValid();
        }

        private bool IsNewPasswordValid()
        {
            if (credentialsManager == null)
                credentialsManager = new LoginCredentialsManager();

            if (!credentialsManager.VerifyPassword(PasswordTextBox.text))
            {
                _newAccountVerified = false;
                StatusText.text = "Your password contain 8 characters, one special character, one upper case, and one lower case character.";
                return false;
            }

            if (PasswordTextBox.text != Password2TextBox.text)
            {
                _newAccountVerified = false;
                StatusText.text = "The two passwords typed are not the same.";
                return false;
            }

            return true;
        }

        private void ChangePasswordAction(bool canChange, string user)
        {
            if (canChange)
            {

            }
        }
    }
}
