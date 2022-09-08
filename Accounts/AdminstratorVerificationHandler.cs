
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Sharing.Security
{
    public class AdminstratorVerificationHandler : MonoBehaviour
    {
        public TMPro.TMP_InputField userName;
        public TMPro.TMP_InputField password;
        public Button loginButton;
        public GameObject loadingGameObject;
        public TMPro.TMP_Text statusText;

        public TMPro.TMP_Text ContextText;

        /// <summary>
        /// Are the credentials active or deactivated?
        /// </summary>
        /// <value></value>
        public bool IsOn { get; private set; }

        public GameObject AntiGuibackground;

        public Transform offLocation;
        public Transform OnLocation;

        private float watiTime = 5f;

        Action<bool, string> _callBack;

        public static AdminstratorVerificationHandler Main;

        public void Awake()
        {
            if (Main != null)
            {
                Debug.LogError("There should be only one admin verifier in the scene");
                Destroy(this.gameObject);
            }
            else
            {
                Main = this;
            }
        }

        public void Start()
        {
            SetOff();
        }

        public void Verify()
        {
            if (Account.TryAdminVerifyCredentials(userName.text, password.text, out var response))
            {
                string uName = userName.text;

                ClearCredentials();
                statusText.text = response;
                SetOff();

                _callBack(true, uName);

            }
            else
            {
                ClearCredentials();
            }
        }

        /// <summary>
        /// Set a request to verify admin credentials for secure interaction
        /// </summary>
        /// <param name="callBack">the action to excecute if the credentials are verified</param>
        public void RequestVerification(Action<bool,string> callBack, string context)
        {
            if (callBack == null)
                throw new NullReferenceException("the callback cannot be null");

            _callBack = callBack;

            transform.position = OnLocation.transform.position;
            AntiGuibackground.SetActive(true);

            ContextText.text = context;

            IsOn = true;
        }

        private void SetOff()
        {
            transform.position = offLocation.position;
            AntiGuibackground.SetActive(false);
            ClearCredentials();

            IsOn = false;
        }

        /// <summary>
        /// Clear the credentials
        /// </summary>
        public void ClearCredentials()
        {
            userName.text = "";
            password.text = "";
        }

        /// <summary>
        /// Attempt to verify credentials
        /// </summary>
        public void Commit()
        {
            StartCoroutine(TryCredentials());
        }

        public void Cancel()
        {
            SetOff();
            _callBack(false, "User Canceled");
        }

        IEnumerator TryCredentials()
        {
            loadingGameObject.SetActive(true);
            yield return new WaitForSeconds(watiTime);
            Verify();
            loadingGameObject.SetActive(false);
        }
    }
}
