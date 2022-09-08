using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend.CardsElementData;
using WarManager.Backend;
using WarManager;
using WarManager.Unity3D;

using System;


namespace WarManager.Cards.Elements
{

    public class UnityCardLinkButtonElement : PoolableObject, IUnityCardElement
    {
        public int ID { get; private set; }

        public string ElementTag { get; } = "link";

        [SerializeField] TooltipTrigger _tooltipTrigger;

        public float doubleClickTimeConstraint { get; set; } = .5f;
        private float clickTime = .5f;
        private bool clickedOnce;

        public bool Active
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        public bool Critical { get; set; }

        public CardElementViewData ElementViewData { get; set; }

        RectTransform _rect;
        BoxCollider2D collider;
        public TMPro.TMP_Text titleTextElement;
        public Image BackroundImage;

        private Vector3 _location = Vector2.zero;
        private Vector2 _scale = Vector2.one;

        private string _linkText = "";

        private string _titleText = "Click Me";
        private int _titleTextFontSize = 12;

        private Color _titleColor = Color.black;
        private Color _backgroundColor = Color.white;

        public bool hasClicked = false;
        public float ClickTime = .5f;
        private WarManager.Unity3D.UnityCard _unityCard;

        void Start()
        {

        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {

        }

        public void Refresh()
        {
            if (_rect == null)
                _rect = GetComponent<RectTransform>();

            _rect.sizeDelta = _scale;
            _rect.anchoredPosition = _location;

            if (collider == null)
                collider = GetComponent<BoxCollider2D>();

            collider.size = _scale;

            titleTextElement.text = _titleText;
            titleTextElement.fontSize = _titleTextFontSize;
            titleTextElement.color = _titleColor;

            // _tooltipTrigger = GetComponent<TooltipTrigger>();
            _tooltipTrigger.contentText = _linkText + "\n(alt + click to activate)";
            _tooltipTrigger.headerText = _titleText;

            _tooltipTrigger.size = _rect.sizeDelta / 2;

            BackroundImage.color = _backgroundColor;
        }

        public void OnClickButtonlistener()
        {
            MessageBoxHandler.Print_Immediate("Taking you to:\n\n<size=12>" + _linkText + "\n\n</size> would you like to proceed?", "Notice", (x) =>
            {

                if (x)
                {
                    LeanTween.delayedCall(1, ActivateLink);
                }
            });
        }

        private void ActivateLink()
        {
            _linkText = _linkText.Trim();

            if (_linkText != null && _linkText.Length > 0)
            {
                if (_linkText.ToLower().Trim().StartsWith("sheet:"))
                {

                    _linkText = _linkText.Remove(0, 6);
                    _linkText = _linkText.Trim();

                    // Debug.Log(_linkText);

                    var manifestData = SheetsServerManifest.GetServerSheetMetaData();

                    if (string.IsNullOrWhiteSpace(_linkText) || _linkText == string.Empty)
                    {
                        Debug.Log("Could not find sheet with no name");
                        // MessageBoxHandler.Print_Immediate("Could not find a sheet with no name", "Error");
                        NotificationHandler.Print("Could not find sheet with no name");
                        return;
                    }

                    foreach (var sheet in manifestData)
                    {
                        if (sheet.Data.SheetName == _linkText)
                        {
                            // Debug.Log(dat.Data.SheetName);

                            try
                            {
                                if (WarManager.Sharing.FileControl<SheetMetaData>.TryGetServerFile(sheet, WarSystem.ServerVersion, WarSystem.CurrentActiveAccount, out var file))
                                {
                                    string path = GeneralSettings.Save_Location_Server + @"\Sheets\" + file.SheetName + SheetsManager.CardSheetExtension;
                                    SheetsManager.OpenCardSheet(path, SheetsManager.SystemEncryptKey, out var id);

                                    // Debug.Log("found " + dat.Data.SheetName);

                                    return;
                                }
                                else
                                {
                                    NotificationHandler.Print("You do not have permission to view \'" + _linkText + "\'");
                                    return;
                                }
                            }
                            catch (System.Exception ex)
                            {
                                NotificationHandler.Print("Error locating or opening sheet " + ex.Message);
                            }
                        }
                    }

                    NotificationHandler.Print("Could not find \'" + _linkText + "\'");
                    return;

                }

                Application.OpenURL(_linkText);
            }
        }

        public static bool TrySetButtonLink(UnityCardLinkButtonElement element, CardButtonElementData data, WarManager.Unity3D.UnityCard unityCard, DataEntry entry)
        {
            #region old

            // if (element == null)
            //     throw new NullReferenceException("The element cannot be null");

            // if (data == null)
            //     throw new NullReferenceException("the data cannot be null");

            // if (element.ElementTag == null)
            //     throw new NullReferenceException("the element tag cannot be null");

            // if (element.ElementTag != data.ElementTag)
            //     return false;

            // element.gameObject.SetActive(true);

            // element.ID = data.ID;
            // element._location = new Vector2(data.Location[0], data.Location[1]);
            // element.Critical = data.Critical;
            // element._unityCard = unityCard;

            // if (data.Payload != null && data.Payload != string.Empty)
            // {
            //     string[] payload = data.Payload.Split(',');

            //     Vector2 size = new Vector2(5, 5);

            //     if (payload.Length >= 1)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[0], out amt))
            //         {
            //             size = new Vector2(amt, size.y);
            //         }
            //         else
            //         {
            //             string message = "Property 1 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 2)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[1], out amt))
            //         {
            //             size = new Vector2(size.x, amt);
            //         }
            //         else
            //         {
            //             string message = "Property 2 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     element._scale = size;

            //     if (payload.Length >= 3)
            //     {
            //         int amt = 4;
            //         if (Int32.TryParse(payload[2], out amt))
            //         {
            //             element._titleTextFontSize = amt;
            //         }
            //         else
            //         {
            //             string message = "Property 3 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }


            //     if (payload.Length >= 4)
            //     {
            //         string backgroundColor = payload[3].Trim();

            //         Color col = Color.white;

            //         if (ColorUtility.TryParseHtmlString(backgroundColor, out var color))
            //         {
            //             col = color;
            //         }

            //         element._backgroundColor = col;
            //     }
            //     else
            //     {
            //         string message = "Property 4 of the glance bar was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }

            //     if (payload.Length >= 5)
            //     {
            //         string backgroundColor = payload[4].Trim();

            //         Color col = Color.black;

            //         if (ColorUtility.TryParseHtmlString(backgroundColor, out var color))
            //         {
            //             col = color;
            //         }

            //         element._titleColor = col;
            //     }
            //     else
            //     {
            //         string message = "Property 5 of the glance bar was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }

            //     if (data.DisplayInfo != null && data.DisplayInfo.Length > 1)
            //     {
            //         if (data.DisplayInfo[0] != null)
            //         {
            //             element._titleText = data.DisplayInfo[0];
            //         }

            //         if (data.DisplayInfo[1] != null)
            //         {
            //             element._linkText = data.DisplayInfo[1];
            //         }
            //     }
            // }

            #endregion

            element._titleColor = Color.blue;
            element._titleTextFontSize = data.FontSize;
            element._location = new Vector3((float)data.Location[0], (float)data.Location[1], (float)data.Location[2]);
            element._scale = new Vector2((float)data.Scale[0], (float)data.Scale[1]);
            element._unityCard = unityCard;

            element.ElementViewData = data;

            ColorUtility.TryParseHtmlString(data.BackgroundColor, out var color);
            element._backgroundColor = color;

            if (entry.TryGetValueAt(data.ToColumnArray[0], out var value))
            {
                element._titleText = value.ParseToParagraph();
            }

            if (entry.TryGetValueAt(data.ToColumnArray[1], out var linkValue))
            {
                element._linkText = linkValue.ParseToParagraph();
            }

            element.Refresh();

            return true;
        }

        IEnumerator ClickTimer()
        {
            hasClicked = true;
            yield return new WaitForSeconds(ClickTime);
            hasClicked = false;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                OnClickButtonlistener();
            }
        }

        void OnMouseDown()
        {
            StartCoroutine(ClickTimer());
            _unityCard.PressDown();
        }

        void OnMouseDrag()
        {
            StopCoroutine(ClickTimer());
            hasClicked = false;

            //Debug.Log("drag");

            // var x = _unityCard.Card.DragFinalLocation;
            // _unityCard.
        }

        void OnMouseEnter()
        {
            _unityCard.HoverStart();
        }

        void OnMouseExit()
        {
            _unityCard.HoverEnd();
        }

        void OnMouseUp()
        {

            if (clickedOnce)
            {
                // OnClickButtonlistener();
                hasClicked = false;
                clickedOnce = false;
                StopCoroutine(ResetDoubleClickTime());
                // Debug.Log("clicked twice");
            }

            if (!clickedOnce)
            {
                StartCoroutine(ResetDoubleClickTime());
                // Debug.Log("clicked once");
                clickedOnce = true;
                hasClicked = false;
            }

            StopCoroutine(ClickTimer());

            _unityCard.PressUp();
        }


        IEnumerator ResetDoubleClickTime()
        {
            yield return new WaitForSecondsRealtime(doubleClickTimeConstraint);
            clickedOnce = false;
            // Debug.Log("double click ended");
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            throw new NotImplementedException();
        }
    }
}
