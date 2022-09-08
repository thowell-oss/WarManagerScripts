
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

using Sirenix.OdinInspector;
using Sirenix;
using WarManager.Unity3D;
using System.Threading.Tasks;
using WarManager.Sharing.Security;

namespace WarManager.Sharing
{
    /// <summary>
    /// initializes the sheet editor
    /// </summary>
    [Notes.Author("initializes the sheet editor")]
    public class WarManagerStartup : MonoBehaviour
    {

        /// <summary>
        /// should the frame rate be capped?
        /// </summary>
        [TabGroup("App Settings")]
        public bool CapFrameRate = false;

        /// <summary>
        /// The frame rate of the application
        /// </summary>
        [TabGroup("App Settings")]
        [ShowIf(nameof(CapFrameRate))]
        [Range(30, 120)]
        public int FrameRate = 90;

        [BoxGroup("Dev")]
        public bool DeveloperLogin = false;
        [Space]
        [BoxGroup("Dev")]
        [InfoBox("Custom login optional", nameof(DeveloperLogin))]
        [ShowIf(nameof(DeveloperLogin))]
        public string CustomLogin;

        private string _key;

        [BoxGroup("Dev")]
        [ShowIf(nameof(DeveloperLogin))]
        public string CustomLoginKey;
        [Space]
        /// <summary>
        /// Should the testing forms controller be on?
        /// </summary>
        [TabGroup("Testing")]
        public bool TestFormsController;

        /// <summary>
        /// Handles the non persistent sheet testing
        /// </summary>
        [TabGroup("Testing")]
        public bool TestAddNonPersistentSheet;

        [TabGroup("Testing")]
        public bool testJsonLoggerOnStartup;

        [TabGroup("Testing")]
        public bool ActivateFilePickerOnStartup;

        [TabGroup("Testing")]
        public bool TestSMS;

        [TabGroup("Testing")]
        public bool TestSubmitFile;

        [TabGroup("Testing")]
        public bool TestSerializeDataSet;

        [TabGroup("Testing")]
        public bool TestGismos;
        [TabGroup("Testing")]
        public GismosManager GismoManager;

        private bool dev = false;

        [SerializeField] GameObject LoadingSessionUI;

        void Start()
        {

#if UNITY_EDITOR
            if (DeveloperLogin)
            {
                DevelopmentLogin();
            }
            dev = true;
#endif

            SetApplicationSettings();


            LeanTween.delayedCall(1, RuntimeDelayTesting);

            if (WarSystem.AccountPreferences.LastCurrentSheet != null && WarSystem.AccountPreferences.LastCurrentSheet != string.Empty &&
             WarSystem.AccountPreferences.LastCurrentSheet.Length > 1)
            {
                LoadingSessionUI.gameObject.SetActive(true);

                LeanTween.delayedCall(.25f, SheetsManager.AttemptLoadSheetPreferences);
            }
            else
                LeanTween.delayedCall(.25f, SheetsManager.OpenHomeCardSheet_Startup);

            LeanTween.delayedCall(.5f, () =>
            {
                if (SheetsManager.SheetCount < 1)
                    SheetsManager.OpenHomeCardSheet_Startup();
            });

            LeanTween.delayedCall(2f, () =>
            {
                LoadingSessionUI.gameObject.SetActive(false);
            });

            if (testJsonLoggerOnStartup)
                LeanTween.delayedCall(1, () =>
                {
                    WarSystem.WriteToLog("testing 123", Logging.MessageType.debug);
                });

            StartCoroutine(ActorsTick());

            if (ActivateFilePickerOnStartup)
            {
                LeanTween.delayedCall(2, () =>
                {
                    ActiveSheetsDisplayer.main.FilePicker.Init(new string[0], false, (x) =>
                    {
                        if (x)
                        {
                            MessageBoxHandler.Print_Immediate(ActiveSheetsDisplayer.main.FilePicker.SelectedPath, "Path");
                        }
                        else
                        {
                            LeanTween.delayedCall(1, () =>
                            {
                                MessageBoxHandler.Print_Immediate("Finding path canceled", "Note");
                            });
                        }
                    });
                });
            }

            if (TestSMS)
            {
                MessageBoxHandler.Print_Immediate("Send SMS?", "Question", (x) =>
                {
                    if (x)
                    {
                        TwilioSMSHandler handler = new TwilioSMSHandler();
                        Task.Run(() => handler.SendMessage("Hey there!\nWar Manager Assistant", "+19137497477"));
                    }
                });
            }


            if (TestSerializeDataSet)
            {
                string id = "326bd75f-12cc-407c-8275-09ce8b5206dd";

                if (WarSystem.DataSetManager.TryGetDataset(id, out var set))
                {

                    DataSetSerializer setSerializer = new DataSetSerializer();
                    setSerializer.SerializeDataSet(set);
                }
            }

            // string word = WarManager.Special.RandomWordFetcher.GetRandomWord();
            // word += WarManager.Special.RandomWordFetcher.GetRandomWord();
            // word += "@Wm25";

            if (TestGismos)
            {

                LeanTween.delayedCall(5, () =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            var gismo = GismoManager.SetWorldGismo(new Point(i, -k), false);

                            if (gismo != null)
                                gismo.SetWorldGismoContent("test " + new Point(i, k).ToString(), Color.green, 12);

                            //Debug.Log(new Point(i, -k).ToString());
                        }
                    }
                });
            }
        }

        void Update()
        {
            if (TestFormsController)
                HandleTestToggleFormsController();

            if (TestSubmitFile)
            {
                TestSubmitFile = false;

                var cards = CardUtility.GetCardsFromCurrentSheet();
                SubmitDocToServer documentSubmitter = new SubmitDocToServer();

                throw new NotImplementedException();

                //documentSubmitter.SubmitCards("test", cards, false);
            }
        }

        /// <summary>
        /// development login
        /// </summary>
        private void DevelopmentLogin()
        {
            Debug.Log("dev login");

            LoadingSessionUI.gameObject.SetActive(false);

            if (WarSystem.CurrentActiveAccount != null) return;

            string user = CustomLogin;

            if (!string.IsNullOrEmpty(CustomLoginKey))
                _key = CustomLoginKey;

            CustomLoginKey = null;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(_key))
            {
                user = System.Environment.GetEnvironmentVariable("DEV_USER_NAME", EnvironmentVariableTarget.User);
                _key = System.Environment.GetEnvironmentVariable("DEV_USER_PASSWORD", EnvironmentVariableTarget.User);
            }

            Account.AttemptLoginToWarManager(user, _key, out var account, out var message);

            //GroupMeDeveloperPushNotificationHandler gs = new GroupMeDeveloperPushNotificationHandler();
        }

        /// <summary>
        /// startup systems, delay run time testing to allow War Manager to finish initializing
        /// </summary>
        private void RuntimeDelayTesting()
        {
            if (TestAddNonPersistentSheet && dev)
            {
                LeanTween.delayedCall(4, HandleTestingNonPersistentSheet);
            }

        }


        /// <summary>
        /// handle the test forms controller
        /// </summary>
        private void HandleTestToggleFormsController()
        {
            if (!dev) return;

            //if (Input.GetKeyDown(KeyCode.F1))
            //{
            //    if (WarSystem.FormsController.UsingForms)
            //    {
            //        WarSystem.FormsController.StopForms();
            //    }
            //    else
            //    {
            //        WarSystem.FormsController.StartForms();
            //    }
            //}
        }

        /// <summary>
        /// handle test non persistent sheet
        /// </summary>
        public void HandleTestingNonPersistentSheet()
        {
            string id = SheetsManager.NewCardSheet("Test persistent sheet", new string[1] { "Developer" }, new double[2] { 5, 5 }, new string[0], false, null);

            var sheet = SheetsManager.GetActiveSheet(id);

            DuplicateCardFinderActor actor = new DuplicateCardFinderActor();
            var entry = actor.GetDataEntry(Guid.NewGuid().ToString(), string.Empty);

            if (CardUtility.TryDropCard(sheet, new Point(5, -5), sheet.CurrentLayer, entry, out var cardId))
            {
                Debug.Log("success");
                Debug.Log(CardUtility.GetCard(cardId).point.ToString());
            }
        }

        /// <summary>
        /// Set misc. settings that pertain to the screen and application (not the software itself).
        /// </summary>
        void SetApplicationSettings()
        {
            // Screen.SetResolution(1920, 1080, false);
            QualitySettings.vSyncCount = 1;

            if (CapFrameRate)
                Application.targetFrameRate = 90; // how to find low preformance devices...
        }

        IEnumerator ActorsTick()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(1);

                if (WarSystem.ActiveCardActors != null)
                {
                    WarSystem.ActiveCardActors.Tick();
                }
            }
        }
    }
}
