using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using WarManager.Sharing.Security;

using StringUtility;

namespace WarManager.Sharing
{
    public class EditAccount : MonoBehaviour
    {
        public TMP_InputField userNameInput;
        public Button logInButton;
        public TMP_Text statusInfo;

        public GameObject Loading;

        public void TryEditAccount_buttonListener()
        {
            string userName = userNameInput.text;

            if (Account.Exists(userName))
            {
                WaitToVerify(userName);
            }
        }

        void Update()
        {
            logInButton.interactable = userNameInput.text.Length > 5;
        }



        IEnumerator WaitToVerify(string user)
        {
            Loading.SetActive(true);
            yield return new WaitForSeconds(2);
            Loading.SetActive(false);
            if (Account.Exists(user))
            {
                AdminstratorVerificationHandler.Main.RequestVerification(CanEditAccount, "Change Account Info? /n" + user);
            }
            else
            {
                statusInfo.text = "Cannot find: " + user;
            }
        }

        void CanEditAccount(bool canReplace, string user)
        {
            if (canReplace)
            {
                
            }
            else
            {
                statusInfo.text = "Request denied";
            }
        }

        void ReplaceAccount()
        {

        }
    }
}
