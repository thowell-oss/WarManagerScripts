/* TitleBarTextManager.cs
*  Author: Taylor Howell
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend;
using WarManager.Cards;
using WarManager;

using StringUtility;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles the title text at the top of the title bar")]
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class TitleBarTextManager : MonoBehaviour
    {
        TMPro.TMP_Text text;

        [SerializeField] Image EditTypeImage;
        [SerializeField] Sprite SheetSprite;
        [SerializeField] Sprite NoneSprite;
        [SerializeField] Sprite NotPeristentSprite;

        [SerializeField] Sprite HomeSprite;

        [SerializeField] Button _pageLeft;
        [SerializeField] Button _pageRight;

        [SerializeField] TMPro.TMP_Text _pageRightText;
        [SerializeField] TMPro.TMP_Text _pageLeftText;

        [SerializeField] Button CloseButton;


        [SerializeField] TooltipTrigger PageLeftToolTipTrigger;
        [SerializeField] TooltipTrigger PageRightToolTipTrigger;

        List<Sheet<Card>> sheets = new List<Sheet<Card>>();
        Sheet<Card> CurrentSelectedSheet;
        int location = -1;

        private bool _allowPagination = true;

        public void Awake()
        {
            text = GetComponent<TMPro.TMP_Text>();
            text.text = "No Sheet Selected";
            CloseButton.interactable = false;
            CloseButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called during an event in order to change the title bar to the sheet name
        /// </summary>
        /// <param name="sheetId"></param>
        public void OnChangeCurrentSheet(string sheetId)
        {
            if (sheetId == null || sheetId == string.Empty)
            {
                OnCloseCardSheet("");
                return;
            }

            CurrentSelectedSheet = SheetsManager.GetActiveSheet(sheetId);

            text.text = CurrentSelectedSheet.Name.SetStringQuotes();

            if (CurrentSelectedSheet == null)
            {
                EditTypeImage.sprite = NoneSprite;

                CloseButton.gameObject.SetActive(true);
                CloseButton.interactable = false;
            }
            else
            {
                if (CurrentSelectedSheet.Name == GeneralSettings.HomeSheetName)
                {
                    EditTypeImage.sprite = HomeSprite;

                    CloseButton.gameObject.SetActive(true);
                    CloseButton.interactable = false;
                }
                else
                {
                    if (CurrentSelectedSheet.Persistent)
                    {
                        EditTypeImage.sprite = SheetSprite;

                        CloseButton.gameObject.SetActive(true);
                        CloseButton.interactable = true;
                    }
                    else
                    {
                        EditTypeImage.sprite = NotPeristentSprite;
                        CloseButton.interactable = false;
                        CloseButton.gameObject.SetActive(false);
                    }
                }
            }

            HandlePagination();
        }

        public void OnOpenCardSheet(string sheetId)
        {
            text.text = SheetsManager.GetActiveSheet(sheetId).Name;
            // EditTypeImage.sprite = SheetSprite;

            // HandlePagination();
        }

        public void OnCloseCardSheet(string id)
        {
            text.text = "No Current Active Sheet";
            CloseButton.interactable = false;
            CloseButton.gameObject.SetActive(false);
            //EditTypeImage.sprite = NoneSprite;

            // HandlePagination();
        }

        private void HandlePagination()
        {
            sheets.Clear();
            sheets.AddRange(SheetsManager.GetActiveCardSheets());

            sheets.Sort(delegate (Sheet<Card> a, Sheet<Card> b)
            {

                if (a.Name == GeneralSettings.HomeSheetName)
                {
                    return -1;
                }
                else if (b.Name == GeneralSettings.HomeSheetName)
                {
                    return 1;
                }

                return a.Name.ToLower().CompareTo(b.Name.ToLower());
            });


            for (int i = 0; i < sheets.Count; i++)
            {
                if (sheets[i].ID.Trim() == CurrentSelectedSheet.ID.Trim())
                {

                    if (sheets.Count <= 2)
                    {
                        _pageRight.interactable = i < sheets.Count - 1;
                        _pageLeft.interactable = i > 0;
                    }
                    else
                    {
                        _pageRight.interactable = true;
                        _pageLeft.interactable = true;
                    }

                    location = i;

                    if (_pageRight.interactable)
                    {
                        string name = "";

                        if (i < sheets.Count - 1)
                            name = sheets[location + 1].Name.SetStringQuotes();
                        else if (sheets.Count > 2)
                            name = sheets[0].Name.SetStringQuotes();

                        _pageRightText.text = name;

                        PageRightToolTipTrigger.contentText = "Click to view " + name;
                    }
                    else
                    {
                        _pageRightText.text = "";
                        PageRightToolTipTrigger.contentText = "(No sheets to view) ";
                    }

                    if (_pageLeft.interactable)
                    {
                        string name = "";

                        if (i > 0)
                            name = sheets[location - 1].Name.SetStringQuotes();
                        else if (sheets.Count > 2)
                            name = sheets[sheets.Count - 1].Name.SetStringQuotes();

                        _pageLeftText.text = name;
                        PageLeftToolTipTrigger.contentText = "Click to view " + name;
                    }
                    else
                    {
                        _pageLeftText.text = "";
                        PageLeftToolTipTrigger.contentText = "(No sheets to view) ";
                    }

                    return;
                }
            }

            _pageRight.interactable = false;
            _pageLeft.interactable = false;
            location = -1;
        }

        public void PageRight()
        {

            if (!_allowPagination)
                return;

            string id = null;
            if (location + 1 < sheets.Count)
            {
                id = sheets[location + 1].ID;
            }
            else if (sheets.Count > 2)
            {
                id = sheets[0].ID;
            }

            if (id != null)
                SheetsManager.SetSheetCurrent(id);
        }

        public void PageLeft()
        {
            if (!_allowPagination)
                return;

            string id = null;

            if (location - 1 >= 0)
            {
                id = sheets[location - 1].ID;
            }
            else if (sheets.Count > 2)
            {
                id = sheets[sheets.Count - 1].ID;
            }

            if (id != null)
            {
                SheetsManager.SetSheetCurrent(id);
            }
        }

        public void SetPaginationAllowed(bool allowed)
        {
            _allowPagination = allowed;

            _pageLeft.enabled = allowed;
            _pageRight.enabled = allowed;

            _pageRightText.gameObject.SetActive(allowed);
            _pageLeft.gameObject.SetActive(allowed);
        }

        public void CloseSheet()
        {
            if (sheets.Count > 1)
                SheetsManager.CloseSheet(SheetsManager.CurrentSheetID);
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnChangeCurrentSheet;
            SheetsManager.OnOpenCardSheet += OnOpenCardSheet;
            SheetsManager.OnCloseCardSheet += OnCloseCardSheet;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnChangeCurrentSheet;
            SheetsManager.OnOpenCardSheet -= OnOpenCardSheet;
            SheetsManager.OnCloseCardSheet -= OnCloseCardSheet;
        }
    }
}
