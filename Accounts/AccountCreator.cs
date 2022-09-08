using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.UI;
using TMPro;

using UnityEngine.Events;
using UnityEngine.EventSystems;

using WarManager.Backend;
using WarManager.Sharing.Security;

namespace WarManager.Sharing
{
    public class AccountCreator : MonoBehaviour
    {

        public TMP_InputField UserNameTextBox;
        public TMP_InputField FirstNameTextBox;
        public TMP_InputField LastNameTextBox;
        public TMP_Dropdown Dropdown;
        public TMP_InputField PasswordTextBox;
        public TMP_InputField Password2TextBox;
        public TMP_Text StatusText;

        public GameObject LoadingGameObject;

        public Button CreateAccountButton;
        private float loadingTime = 5;

        private bool _newAccountVerified;

        private Permissions selectedPermissions = null;

        private Dictionary<int, Permissions> PermissionsDictionary = new Dictionary<int, Permissions>();

        private bool _newAccount = true;

        public void Awake()
        {
            OnStartCreateNewAccount();
        }

        public void OnStartCreateNewAccount()
        {
            WarSystem.AttemptConnectionToServer();
            var permissionsList = Permissions.GetAllPermissions();

            List<TMP_Dropdown.OptionData> dropDownList = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < permissionsList.Count; i++)
            {
                var data = new TMP_Dropdown.OptionData(permissionsList[i].Name);
                dropDownList.Add(data);
                PermissionsDictionary.Add(i, permissionsList[i]);
            }

            Dropdown.AddOptions(dropDownList);
        }

        void Update()
        {
            if (UserNameTextBox.text != string.Empty && FirstNameTextBox.text != string.Empty && LastNameTextBox.text != string.Empty && selectedPermissions != (Permissions)null &&
            PasswordTextBox.text != string.Empty && Password2TextBox.text != string.Empty)
            {
                CreateAccountButton.interactable = true;

                if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                {
                    VerifyAccount();
                }
            }
            else
            {
                CreateAccountButton.interactable = false;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (EventSystem.current.currentSelectedGameObject.gameObject != null)
                {
                    var input = EventSystem.current.currentSelectedGameObject.gameObject.GetComponent<TMP_InputField>();

                    if (input != null)
                    {
                        input.DeactivateInputField();
                        var s = input.FindSelectableOnDown();

                        if (s != null)
                        {
                            var sInput = s.GetComponent<TMP_InputField>();
                            if (sInput != null)
                            {
                                sInput.ActivateInputField();
                            }
                            else
                            {
                                Debug.Log("sInput == null");
                            }
                        }
                        else
                        {
                            Debug.Log("s == null");
                        }
                    }
                    else
                    {
                        Debug.Log("inputfield == null");
                    }
                }
                else
                {
                    Debug.Log("eventsyst == null");
                }


            }
        }

        /// <summary>
        /// Called using unity events
        /// </summary>
        public void VerifyNewAccount()
        {
            StartCoroutine(DelayVerify());
        }

        IEnumerator DelayVerify()
        {
            LoadingGameObject.SetActive(true);
            yield return new WaitForSeconds(loadingTime);
            VerifyAccount();
            LoadingGameObject.SetActive(false);
        }

        /// <summary>
        /// Vertify the account is valid
        /// </summary>
        /// <returns>returns true if the account can be verified</returns>
        public bool VerifyAccount()
        {
            if (PasswordTextBox.text != Password2TextBox.text)
            {
                _newAccountVerified = false;
                StatusText.text = "The two passwords typed are not the same.";
                return false;
            }

            LoginCredentialsManager credentialsManager = new LoginCredentialsManager();

            if (!credentialsManager.VerifyPassword(PasswordTextBox.text))
            {
                _newAccountVerified = false;
                StatusText.text = LoginCredentialsManager.passwordInstructions;
                return false;
            }

            Import_CSV importer = new Import_CSV();
            importer.Import(GeneralSettings.Save_Location_Server + @"\Data\War System\Accts.csv");

            for (int i = 0; i < importer.Height; i++)
            {
                Debug.Log(i);
                if (UserNameTextBox.text == importer.GetCell(1, 0))
                {
                    StatusText.text = "There is already an account with the selected user name";
                    return false;
                }
            }

            if (selectedPermissions == (Permissions)null)
            {
                StatusText.text = "Please select your role";
            }

            string emailResponse = credentialsManager.VerifyEmail(UserNameTextBox.text);

            if (emailResponse != UserNameTextBox.text)
            {
                _newAccountVerified = false;
                StatusText.text = emailResponse;
                return false;
            }

            StatusText.text = "Success! - Please Enter Administrator Credentials";
            _newAccountVerified = true;

            AdminstratorVerificationHandler.Main.RequestVerification(CreateAccountVerificationCallBack, $"Create New User?\n User Name: {UserNameTextBox.text}\nPermissions: {selectedPermissions}");

            return true;
        }

        public void SelectPermissions()
        {
            int selected = Dropdown.value;

            if (selected - 1 >= 0 && selected - 1 < PermissionsDictionary.Count)
            {
                selectedPermissions = PermissionsDictionary[selected - 1];
                // Debug.Log("Permissions selected " + selectedPermissions.Name);

                var s = Dropdown.FindSelectableOnDown();

                if (s != null)
                {
                    var sInput = s.GetComponent<TMP_InputField>();

                    if (sInput != null)
                    {
                        sInput.ActivateInputField();
                    }
                }

            }
            else
            {
                selectedPermissions = null;
            }
        }

        private void CreateAccountVerificationCallBack(bool canCreate, string adminUserName)
        {
            if (canCreate)
            {
                if (Account.CreateAccount(UserNameTextBox.text, PasswordTextBox.text, selectedPermissions.Name, "English", FirstNameTextBox.text, LastNameTextBox.text, adminUserName))
                {
                    MessageBoxHandler.Print_Immediate("Account Created Successfully: " + UserNameTextBox.text, "Success!");
                    gameObject.SetActive(false);
                }
            }
            else
            {
                MessageBoxHandler.Print_Immediate("Cannot create account because the administrator credentials have not been verified.", "Error");
            }
        }

        /// <summary>
        /// Set this as a new account or an account that is being edited
        /// </summary>
        /// <param name="newAcct"></param>
        public void SetAsNewAccount(bool newAcct = true)
        {
            _newAccount = newAcct;
        }
    }
}