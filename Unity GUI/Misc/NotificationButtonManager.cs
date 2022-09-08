
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    /// <summary>
    /// The notification popup manager
    /// </summary>
    [Notes.Author("The notification popup manager")]
    public class NotificationButtonManager : MonoBehaviour
    {

        public RectTransform NotificationBox;
        public Transform NotificationGrid;
        public GameObject NotificationTextPrefab;
        public GameObject NotificationButtonTextPrefab;

        [SerializeField] GameObject NoMessagesText = null;

        public UnityEngine.UI.Image IconImage;
        public Sprite SomeNotificationsIcon;
        public Sprite EmptyNotificationsIcon;

        public float openLocation;
        public float closeLocation = -50f;

        public Transform ClearTextButton;

        public List<GameObject> Notifications = new List<GameObject>();

        public TMPro.TMP_Text NotificationCountText;

        //collapse tools

        private string _lastText = "";
        private TMPro.TMP_Text _lastNotificationText = null;
        private int _collapseIterator = 0;

        //

        public float delayTime = 2;

        /// <summary>
        /// Is the notification box open?
        /// </summary>
        /// <value></value>
        public bool IsOpen { get; private set; }


        void Start() => StartCoroutine(WaitToStart());

        private IEnumerator WaitToStart()
        {
            yield return new WaitForSeconds(delayTime);
            ClearNotifications();

            NotificationHandler.readyToDisplay = true;
            ToggleNotificationBox();

            StopCoroutine(WaitToStart());
        }

        /// <summary>
        /// Open/close the notification box
        /// </summary>
        public void ToggleNotificationBox()
        {
            IsOpen = !IsOpen;

            if (IsOpen)
            {
                NotificationBox.gameObject.SetActive(IsOpen);
                LeanTween.cancel(this.gameObject);
                LeanTween.moveLocalX(NotificationBox.gameObject, openLocation, .2f).setEaseOutBack();
            }
            else
            {
                LeanTween.cancel(this.gameObject);
                LeanTween.moveLocalX(NotificationBox.gameObject, closeLocation, .2f).setEaseOutExpo();
                StartCoroutine(WaitToDisable());
            }
        }

        IEnumerator WaitToDisable()
        {
            yield return new WaitForSeconds(.05f);
            IsOpen = false;
            NotificationBox.gameObject.SetActive(IsOpen);
            StopCoroutine(WaitToDisable());
        }

        /// <summary>
        /// write a message to the notification box
        /// </summary>
        /// <param name="text">the message to show</param>
        public void Print(string text)
        {

            if (_lastNotificationText != null && text == _lastText)
            {
                _collapseIterator++;
                _lastNotificationText.text = _collapseIterator + ")\t" + _lastText;
            }
            else
            {

                _collapseIterator = 1;


                GameObject go = Instantiate(NotificationTextPrefab, NotificationGrid) as GameObject;
                TMPro.TMP_Text notificationText = go.GetComponent<TMPro.TMP_Text>();
                notificationText.text = text;
                Notifications.Add(go);

                _lastNotificationText = notificationText;
                _lastText = text;

                HandleNotificationButton();
            }
        }

        /// <summary>
        /// write a button messsage to the notification box (not setup yet).
        /// </summary>
        /// <param name="message">the message to show</param>
        /// <param name="callback">the action call back to execute once the button is pressed</param>
        public void PaintButton(string message, string callToAction, Action callBack)
        {

            _lastNotificationText = null;
            _collapseIterator = 1;
            _lastText = "";

            GameObject go = Instantiate(NotificationButtonTextPrefab, NotificationGrid) as GameObject;
            Notifications.Add(go);

            Action x = () =>
            {
                callBack();
                if (IsOpen)
                    ToggleNotificationBox();
            };

            var c = go.GetComponent<NotificationPopupButtonController>();
            c.SetNotification(message, callToAction, x);

            HandleNotificationButton();
        }

        /// <summary>
        /// Handle the notification button (in order to toggle the popup)
        /// </summary>
        private void HandleNotificationButton()
        {

            if (NoMessagesText != null)
                Destroy(NoMessagesText);

            IconImage.sprite = SomeNotificationsIcon;
            LeanTween.color(IconImage.GetComponent<RectTransform>(), new Color(1, 0, 0, 1), .25f);
            ClearTextButton.SetAsLastSibling();

            if (Notifications.Count > 0 && Notifications.Count <= 99)
                NotificationCountText.text = Notifications.Count.ToString();

            if (Notifications.Count > 99)
            {
                NotificationCountText.text = "99+";
            }
        }

        /// <summary>
        /// Close the notification box
        /// </summary>
        public void CloseNotificationBox()
        {
            if (NotificationBox.gameObject.activeSelf)
            {
                ToggleNotificationBox();

                if (Notifications.Count < 1 && NoMessagesText == null)
                {
                    GameObject go = Instantiate(NotificationTextPrefab, NotificationGrid) as GameObject;
                    TMPro.TMP_Text notificationText = go.GetComponent<TMPro.TMP_Text>();
                    notificationText.text = "<no messages>";
                    NoMessagesText = notificationText.gameObject;
                    notificationText.alignment = TMPro.TextAlignmentOptions.Center;
                    ClearTextButton.SetAsLastSibling();
                }
            }
        }

        public void ClearNotifications()
        {
            for (int i = Notifications.Count - 1; i >= 0; i--)
            {
                Destroy(Notifications[i]);
                Notifications.RemoveAt(i);
            }

            IconImage.sprite = EmptyNotificationsIcon;
            LeanTween.color(IconImage.GetComponent<RectTransform>(), new Color(1, 1, 1, .1f), .5f).setEaseOutExpo();
            NotificationCountText.text = "";
            CloseNotificationBox();

            _lastText = "";
            _collapseIterator = 1;
            _lastNotificationText = null;
        }

        public void OnEnable()
        {
            NotificationHandler.OnPrintText += Print;
            NotificationHandler.OnSendAction += PaintButton;
        }

        public void OnDisable()
        {
            NotificationHandler.OnPrintText -= Print;
            NotificationHandler.OnSendAction -= PaintButton;
            NotificationHandler.readyToDisplay = false;
        }
    }
}
