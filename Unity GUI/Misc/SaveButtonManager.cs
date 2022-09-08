/* SaveButtonManager.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class SaveButtonManager : MonoBehaviour
    {
        public Button OnlineButton;
        public GameObject OfflineImage;

        public GameObject OfflineMessage;
        public GameObject OnlineMessage;

        public GameObject FileSaveMessage;
        public TMPro.TMP_Text FileSaveText;

        public GameObject UploadSucessful;
        public GameObject Syncing;

        public bool IsOffline, isSnycing, fileSaved;

        private bool offlineMessageWasActive = false;
        private bool onlineMessageWasActive = false;

        private bool wasOffline;

        private bool start = true;

        void Start()
        {
            StartCoroutine(CheckOnline());
            StartCoroutine(AutoSave());
            StartCoroutine(MangeIcons());
            isSnycing = true;
        }

        IEnumerator MangeIcons()
        {
            while (true)
            {
                UpdateIcons();
                yield return new WaitForSeconds(.5f);
            }
        }


        private void UpdateIcons()
        {
            if (IsOffline)
            {
                SetIcon(0);
                return;
            }

            if (fileSaved)
            {
                SetIcon(2);
                StartCoroutine(EndSaveFileIcon());
            }

            if (isSnycing)
            {
                SetIcon(3);
                return;
            }

            SetIcon(1);
        }

        /// <summary>
        /// Cycle between icons based off of an iteration state
        /// </summary>
        /// <param name="iconIterator"></param>
        private void SetIcon(int iconIterator)
        {
            switch (iconIterator)
            {
                case 0:
                    OnlineButton.gameObject.SetActive(false);
                    OfflineImage.SetActive(true);
                    if (!offlineMessageWasActive && !wasOffline)
                    {
                        StartCoroutine(OfflineMessageDisable());
                        offlineMessageWasActive = true;
                        wasOffline = true;
                    }

                    OnlineMessage.SetActive(false);
                    onlineMessageWasActive = false;
                    StopCoroutine(OnlineMessageDisable());
                    UploadSucessful.SetActive(false);
                    Syncing.SetActive(false);
                    break;

                case 1:
                    OnlineButton.gameObject.SetActive(true);
                    OfflineImage.SetActive(false);
                    OfflineMessage.SetActive(false);
                    StopCoroutine(OfflineMessageDisable());
                    if (!onlineMessageWasActive && wasOffline)
                    {
                        StartCoroutine(OnlineMessageDisable());
                        onlineMessageWasActive = true;
                        wasOffline = false;
                    }

                    offlineMessageWasActive = false;
                    UploadSucessful.SetActive(false);
                    Syncing.SetActive(false);
                    break;

                case 2:
                    OnlineButton.gameObject.SetActive(false);
                    OfflineImage.SetActive(false);
                    UploadSucessful.SetActive(true);
                    StopCoroutine(OfflineMessageDisable());
                    StopCoroutine(OnlineMessageDisable());
                    Syncing.SetActive(false);
                    offlineMessageWasActive = false;
                    onlineMessageWasActive = false;
                    break;

                case 3:
                    OnlineButton.gameObject.SetActive(false);
                    OfflineImage.SetActive(false);
                    UploadSucessful.SetActive(false);
                    StopCoroutine(OfflineMessageDisable());
                    StopCoroutine(OnlineMessageDisable());
                    Syncing.SetActive(true);
                    offlineMessageWasActive = false;
                    onlineMessageWasActive = false;
                    break;

                default:
                    OnlineButton.gameObject.SetActive(false);
                    OfflineImage.SetActive(false);
                    UploadSucessful.SetActive(false);
                    StopCoroutine(OfflineMessageDisable());
                    StopCoroutine(OnlineMessageDisable());
                    Syncing.SetActive(false);
                    offlineMessageWasActive = false;
                    onlineMessageWasActive = false;
                    break;
            }
        }




        IEnumerator OfflineMessageDisable()
        {
            OfflineMessage.SetActive(true);
            yield return new WaitForSeconds(5);
            OfflineMessage.SetActive(false);
            offlineMessageWasActive = true;
            StopCoroutine(OfflineMessageDisable());
        }

        IEnumerator OnlineMessageDisable()
        {
            OnlineMessage.SetActive(true);
            yield return new WaitForSeconds(5);
            OnlineMessage.SetActive(false);
            onlineMessageWasActive = true;
            StopCoroutine(OfflineMessageDisable());
        }

        /// <summary>
        /// Save the sheets to a file
        /// </summary>
        public void SaveSheets()
        {
            isSnycing = true;

            if (SheetsManager.SaveAllSheets())
            {
                fileSaved = true;
            }

            StartCoroutine(EndSaveFileIcon());
        }

        public void SaveSheet_uiButtonEvent()
        {
            if (SheetsManager.SheetCount > 0)
            {
                SaveSheets();
            }
        }

        /// <summary>
        /// Stops the auto save and restarts it. Used in order to check the save time cached the general settings
        /// </summary>
        public void ResetAutoSaveTime()
        {
            StopCoroutine(AutoSave());
            StartCoroutine(AutoSave());
        }


        IEnumerator AutoSave()
        {
            yield return new WaitForSeconds(GeneralSettings.Save_AutoSave_Time_Seconds);

            if (GeneralSettings.Save_AutoSave && SheetsManager.SheetCount > 0)
            {
                SaveSheets();
                NotificationHandler.Print("Auto Saved Sheets at " + System.DateTime.Now.ToLocalTime());
            }

            StopCoroutine(AutoSave());
            StartCoroutine(AutoSave());
        }

        IEnumerator CheckOnline()
        {
            float timer = 2;

            if (!start)
            {
                yield return new WaitForSeconds(5 * 60);

                start = false;
                isSnycing = true;
                IsOffline = !WarSystem.AttemptConnectionToServer();
                StopCoroutine(CheckOnline());
                StartCoroutine(CheckOnline());
            }
            else
            {
                timer = 0;
            }

            yield return new WaitForSeconds(timer);

            isSnycing = false;
        }

        IEnumerator EndSaveFileIcon()
        {
            yield return new WaitForSeconds(2);
            isSnycing = false;
            yield return new WaitForSeconds(1);
            fileSaved = false;
            SetIcon(1);
            StopCoroutine(EndSaveFileIcon());
        }

        IEnumerator FileSaveNotification(string sheetName)
        {
            FileSaveText.text = "Saved: " + sheetName;
            FileSaveMessage.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            FileSaveMessage.SetActive(false);
        }

        public void RefreshSaveButton(string id)
        {
            OnlineButton.interactable = SheetsManager.SheetCount > 0;

        }

        public void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += RefreshSaveButton;
            GeneralSettings.OnResetAutoSave += ResetAutoSaveTime;
        }

        public void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= RefreshSaveButton;
            GeneralSettings.OnResetAutoSave -= ResetAutoSaveTime;
        }
    }
}