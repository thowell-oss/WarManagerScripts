/*LoginSceneController.cs
 *Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TMPro;

using WarManager.Sharing;
using WarManager.Sharing.Security;
using StringUtility;

using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace WarManager.Unity3D
{
    /// <summary>
    /// Controls the login proces for the login scene
    /// </summary>
    public class LoginSceneController : MonoBehaviour
    {

        public Image[] images;
        private int currentImage;

        public GameObject wmLogo;
        public GameObject windowLoginLocation;
        public GameObject loginWindow;
        public RectTransform OnLocation;

        [SerializeField] private List<GameObject> _loginObjects = new List<GameObject>();

        /// <summary>
        /// The username input field
        /// </summary>
        public TMP_InputField UserNameInputField;

        /// <summary>
        /// The password input field
        /// </summary>
        public TMP_InputField keyInputField;

        public Button LoginButton;

        public GameObject loading;

        public GameObject MessageBoxObject;

        public GameObject loginWindowGameObject;

        private LoginCredentialsManager _credentialsManager = new LoginCredentialsManager();

        public Button NewUserButton;

        public TMP_Text ErrorText;
        public TMP_Text WelcomeText;

        [Space]
        public TMP_Text ConnectionString;
        public Image ServerLogoImage;
        public Image ServerInfoBackground;

        /// <summary>
        /// can the account be logged in?
        /// </summary>
        private bool _isLoggedIn = false;
        private int serverConnectionCode = -1;

        public void Start()
        {
            //LeanTween.delayedCall(1, () =>
            //{
            LeanTween.scale(wmLogo, new Vector2(1, 1), .5f).setEaseInOutCubic();
            LeanTween.scale(images[0].gameObject, new Vector2(1, 1), 10f).setEaseOutCubic();
            LeanTween.move(loginWindowGameObject.gameObject, windowLoginLocation.transform.position, .75f).setEaseOutCubic();

            //});

            UserNameInputField.ActivateInputField();
            StartCoroutine(ChangeBackground());

            if (CheckConnection())
            {
                TryHandleAutoLogin();
            }
        }

        private bool CheckConnection()
        {
            int errorCode = 0;
            string serverName = "";
            try
            {
                errorCode = WarSystem.HandleConnection(out serverName);
            }
            catch (System.Exception ex)
            {
                errorCode = -5;
            }

            NewUserButton.enabled = errorCode == 0;
            serverConnectionCode = 0;

            if (errorCode != 0)
            {
                string error = "";

                LeanTween.delayedCall(1, () =>
                {
                    if (errorCode == -1)
                    {

                        MessageBoxHandler.Print_Immediate("Server version incorrect - do you need to update?\nError code: Butterfly", "Error");
                        WarSystem.WriteToDev("Butterfly Error! (Server Version Incorrect)", Logging.MessageType.error);

                        error = "Butterfly Error";
                    }
                    else if (errorCode == -2)
                    {

                        MessageBoxHandler.Print_Immediate("It seems like the server is down, most likely for matinence. \nContact support. Error code: Bat", "Error");
                        WarSystem.WriteToDev("Bat Error! (Server Down)", Logging.MessageType.error);
                        error = "Bat Error";
                    }
                    else if (errorCode == -3)
                    {

                        MessageBoxHandler.Print_Immediate("Welcome! First you need to enter the server location. \nContact support if you need help. Error Code: Atlas", "Note");
                        WarSystem.WriteToDev("Atlas Error! (Player Prefs does not contain a server path)", Logging.MessageType.error);

                        error = "Atlas Error";
                    }
                    else if (errorCode == -4)
                    {

                        MessageBoxHandler.Print_Immediate("Server does not exist at specified path. \nContact support. Error code: Ghost", "Error");
                        WarSystem.WriteToDev("Ghost Error! (Server does not exist at specified path)", Logging.MessageType.error);

                        error = "Ghost Error";
                    }
                    else if (errorCode == -5)
                    {

                        MessageBoxHandler.Print_Immediate("Cannot connect to server. \nContact support. Error code: " + errorCode, "Error");
                        WarSystem.WriteToDev("Error " + errorCode + " (some other error)", Logging.MessageType.error);

                        error = $"Error Code {errorCode}";
                    }

                    serverConnectionCode = errorCode;
                    SetConnectionBar("<Not Connected> " + error, null, Color.black);
                });



                return false;
            }
            else
            {
                SetBarToConnectedServer(serverName);
            }

            return true;
        }

        private void SetBarToConnectedServer(string serverName)
        {
            string str = serverName;
            Sprite serverIcon = null;
            Color backgroundColor = Color.black;

            if (UnityEngine.ColorUtility.TryParseHtmlString(WarSystem.ConnectedServerColor, out var col))
            {
                backgroundColor = col;
            }

            if (ServerIconsManager.TryGetIcon(WarSystem.ConnectedServerLogo, out var icon))
            {
                serverIcon = icon;
            }

            SetConnectionBar(str, serverIcon, backgroundColor);
        }


        /// <summary>
        /// Handles control of the connection bar
        /// </summary>
        /// <param name="serverName">the message or server name</param>
        /// <param name="icon">the icon</param>
        /// <param name="backgroundColor">the bar background color</param>
        private void SetConnectionBar(string serverName, Sprite icon, Color backgroundColor)
        {
            ConnectionString.text = serverName;
            if (icon == null)
            {
                ServerLogoImage.gameObject.SetActive(false);
            }
            else
            {
                ServerLogoImage.sprite = icon;
                ServerLogoImage.gameObject.SetActive(true);
            }
            ServerInfoBackground.color = backgroundColor;
        }

        public void AttemptLogin()
        {
            StartCoroutine(WaitToLogin());
        }

        void Update()
        {
            bool passwordVerified = _credentialsManager.VerifyPassword(keyInputField.text);

            LoginButton.interactable = passwordVerified && UserNameInputField.text.Length > 5 && GeneralSettings.Save_Location_Server != null && GeneralSettings.Save_Location_Server.Length > 0;

            if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) && passwordVerified)
            {
                AttemptLogin();
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SetServerLocation();
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {

                if (EventSystem.current.currentSelectedGameObject == UserNameInputField.gameObject)
                {
                    Selectable s = UserNameInputField.FindSelectableOnDown();

                    var inputField = s.GetComponent<TMP_InputField>();

                    if (inputField != null)
                    {
                        inputField.ActivateInputField();
                        UserNameInputField.DeactivateInputField();
                    }
                }

                // if (EventSystem.current.currentSelectedGameObject == keyInputField.gameObject)
                // {
                //     Selectable s = keyInputField.FindSelectableOnDown();

                //     var button = s.GetComponent<Button>();


                // }
            }
        }

        public void SetServerLocation()
        {
            string oldRoot = GeneralSettings.Save_Location_Server;

            EditTextMessageBoxController.OpenModalWindow("", "Server Location:", (x) =>
            {
                GeneralSettings.Save_Location_Server = x;

                PlayerPrefs.SetString("server root", GeneralSettings.Save_Location_Server);
                PlayerPrefs.Save();

                Debug.Log("server location " + x);

                if (WarSystem.HandleConnection(out string serverName) == 0)
                {
                    MessageBoxHandler.Print_Immediate("Connected to " + serverName, "Success!");
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Could not connect to " + GeneralSettings.Save_Location_Server + " reverting back to old server path", "Error");
                    GeneralSettings.Save_Location_Server = oldRoot;
                }

                EmailClient.SendNotificationSMTPEmailToDev("Directory changed ", "The Directory was changed:\nFrom: " + oldRoot + "\nTo: " + GeneralSettings.Save_Location_Server + "\n" + WarSystem.ConnectedDeviceStamp);

                PlayerPrefs.SetString("server root", GeneralSettings.Save_Location_Server);
                PlayerPrefs.Save();

                SetServerLocation();

            });
        }

        public void ToggleAccountLoginWindow()
        {
            loginWindow.SetActive(!loginWindow.activeInHierarchy);
        }

        void FixedUpdate()
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (i != currentImage)
                {
                    images[i].color = Color.Lerp(images[i].color, new Color(images[i].color.r, images[i].color.g, images[i].color.b, 0), .025f);
                }
                else
                {
                    images[i].color = Color.Lerp(images[i].color, new Color(images[i].color.r, images[i].color.g, images[i].color.b, 1), .025f);
                }
            }
        }

        IEnumerator WaitToLogin()
        {

            IsLoading(true);

            CheckCredentials();

            yield return new WaitForSeconds(5f);


            keyInputField.text = "";

            if (_isLoggedIn)
            {

                MessageBoxObject.SetActive(false);
                SceneManager.LoadScene("Sheet Editor Scene");
            }
            else
            {
                IsLoading(false);
            }
        }

        private void IsLoading(bool isLoading)
        {
            foreach (var x in _loginObjects)
            {
                x.SetActive(!isLoading);
            }

            loading.SetActive(isLoading);
        }

        /// <summary>
        /// Check the credentials of the login info
        /// </summary>
        private void CheckCredentials()
        {
            if (_credentialsManager.VerifyPassword(keyInputField.text) && !_isLoggedIn && serverConnectionCode == 0)
            {

                string userName = UserNameInputField.text;
                string password = keyInputField.text;

                keyInputField.text = "";

                //string str = Account.LogIn(userName, password, out var a);

                if (Account.AttemptLoginToWarManager(userName, password, out var a, out var message))
                {
                    _isLoggedIn = true;
                    ErrorText.text = "";
                    LeanTween.delayedCall(4, () =>
                    {
                        WelcomeText.text = "Welcome!";
                    });
                }
                else
                {
                    ErrorText.text = message;
                }
            }
            else if (serverConnectionCode != 0)
            {
                ErrorText.text = "Cannot login to server - Error Code: " + serverConnectionCode;
            }
        }

        private void TryHandleAutoLogin()
        {
            var UserPrefs = UserPreferencesHandler.GetPrefsWithThisMachineID();

            if (UserPrefs.Length == 1)
            {

                LeanTween.delayedCall(1, () =>
                {

                    MessageBoxHandler.Print_Immediate($"Attempting to login " + UserPrefs[0].UserName, "Note");


                    LeanTween.delayedCall(1, () =>
                    {
                        WarSystem.HandleConnection(out var serverName);
                        WarSystem.LoadPermissions(); // do we need this?
                        var accounts = Account.GetAccountsList();

                        Account acct = accounts.Find(x => x.UserName.Trim() == UserPrefs[0].UserName.Trim());

                        if (acct != null)
                        {
                            WarSystem.Login(acct);
                            _isLoggedIn = true;
                            AttemptLogin();
                        }
                        else
                        {
                            LeanTween.delayedCall(3, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Could not log user in ", "Note");
                            });
                        }
                    });
                });
            }
            else if (UserPrefs.Length > 1)
            {
                MessageBoxHandler.Print_Immediate("For security, only one account can use automatic login on this machine. Users:\n" + string.Join<UserPreferences>(", ", UserPrefs), "Error Automatically logging in");
            }

        }

        /// <summary>
        /// Send message to developer
        /// </summary>
        /// <param name="message">the message</param>
        public void MessageDev()
        {
            EditTextMessageBoxController.OpenModalWindow("", "Contact Support", (message) =>
            {
                if (message != null || message != string.Empty)
                {
                    LeanTween.delayedCall(1, () =>
                    {
                        EditTextMessageBoxController.OpenModalWindow("", "What is your Email?", (contactInfo) =>
                        {
                            if (contactInfo != null || contactInfo != string.Empty)
                            {
                                LeanTween.delayedCall(1, () =>
                                {
                                    MessageBoxHandler.Print_Immediate("Are you sure you want to send your message?", "Question", (x) =>
                                    {
                                        if (x)
                                        {
                                            EmailClient.SendQuickEmail("howelltaylor195@gmail.com", "Contact Support from Login Screen", message + "\n\n" + contactInfo);

                                            LeanTween.delayedCall(1, () =>
                                            {
                                                MessageBoxHandler.Print_Immediate("Message Sent", "Note");
                                            });
                                        }
                                    });
                                });
                            }
                        });
                    });
                }
            });
        }

        IEnumerator ChangeBackground()
        {
            yield return new WaitForSeconds(30);
            StopCoroutine(ChangeBackground());
            Cycle();
        }

        public void Cycle()
        {
            currentImage++;

            if (currentImage >= images.Length)
            {
                currentImage = 0;
            }

            StartCoroutine(ChangeBackground());
        }

        public void Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


        public void NewUserMessage()
        {
            MessageBoxHandler.Print_Immediate("Due to security concerns, this has not been implemented yet. Please contact support.", "Notice");
        }
    }
}
