
using WarManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend;
using System;
using System.Linq;

using StringUtility;
using System.IO;
using WarManager.Sharing.Security;
using WarManager.Sharing;

namespace WarManager.Unity3D.Windows
{
    [Notes.Author("Card Sheet UI Element for the slide windows (to try to increase important information without having to go accross many menus")]
    public class SlideWindow_Element_SheetElement : MonoBehaviour, ISlideWindow_Element
    {
        [SerializeField] TMPro.TMP_Text TitleText;
        [SerializeField] TMPro.TMP_Text DetailsText;
        [SerializeField] TagsBubblesManager TagsBubblesManager;
        [SerializeField] GameObject SelectedBar;
        [Space]
        [SerializeField] Image StatusImage;
        [SerializeField] Sprite ViewingSprite;
        [SerializeField] Color ViewingSpriteColor;
        [SerializeField] Sprite OpenSheetSprite;
        [SerializeField] Sprite OpenFormsSprite;
        [SerializeField] Color OpenSpriteColor;
        [SerializeField] Sprite ClosedSheetSprite;
        [SerializeField] Color ClosedSpriteColor;
        [Space]
        [SerializeField] Sprite ErrorSprite;
        [SerializeField] Color ErrorColor = Color.yellow;
        [SerializeField] Image Favorite;
        [SerializeField] private RectTransform FavoriteRect;
        [SerializeField] Sprite EnabledFavoriteSprite, DisabledFavoriteSprite;
        [SerializeField] Color StartColor;
        [SerializeField] Color EndColor;
        [SerializeField] Color DisabledColor = Color.gray;
        [SerializeField] Vector3 startSize;
        [SerializeField] Vector3 EndSize;

        TooltipTrigger _toolTip;


        public SlideWindow_Element_ContentInfo info { get; set; }

        public string SearchContent { get; private set; } = "sheet";

        public GameObject targetGameObject => this.gameObject;

        public void UpdateElement()
        {

            if (info == null)
                return;

            if (_toolTip == null)
            {
                _toolTip = GetComponent<TooltipTrigger>();
            }

            string title = "";
            string details = "";

            string[] tags = new string[0];


            if (info.SheetMetaData != null)
            {
                title = info.SheetMetaData.SheetName.SetStringQuotes();
                details = info.SheetMetaData.Owner + " " + info.SheetMetaData.LastTimeOpened.ToShortDateString();
                tags = info.SheetMetaData.Categories;
            }
            else
            {
                Debug.LogError("The sheet meta data is null");
            }

            TitleText.text = title;
            DetailsText.text = details;

            var permissions = Permissions.GetAllPermissionsWithCategoriesNonGreedy(Permissions.GetAllPermissions().ToArray(), tags);

            string[] permissionsStr = new string[permissions.Count];

            string final = "";

            for (int i = 0; i < permissionsStr.Length; i++)
            {
                permissionsStr[i] = permissions[i].Name;
            }

            if (tags.Contains("*"))
            {
                string[] tgs = new string[1] { "Everyone Can View" };

                Dictionary<string, Action> theTags = new Dictionary<string, Action>();
                theTags.Add("Everyone Can View", ShowAllAccounts);

                TagsBubblesManager.SetTags(theTags, false);
                final = "All";
            }
            else
            {
                Dictionary<string, Action> tagsDict = new Dictionary<string, Action>();
                for (int i = 0; i < permissions.Count; i++)
                {
                    int iterator = i;
                    tagsDict.Add(permissions[i].Name, () => { ShowAccountsThatCanSeeSheet(permissions[iterator]); });
                }

                TagsBubblesManager.SetTags(tagsDict);
                final = string.Join(", ", permissionsStr);
            }

            SearchContent = SearchContent + " " + title;

            SelectedBar.SetActive(SheetsManager.CurrentSheetID == info.SheetMetaData.ID);

            string id = info.SheetMetaData.ID;

            string text = TitleText.text.SetStringQuotes() + " sheet\n<size=60%>(click to download)";

            if (SheetsManager.IsSheetActive(id))
            {
                text = TitleText.text.SetStringQuotes() + " sheet\n<size=60%> (sheet opened - click to view)";
                if (SheetsManager.CurrentSheetID == id.Trim())
                {
                    text = TitleText.text.SetStringQuotes() + " sheet\n<size=60%> (selected sheet - click to reload)";
                }
            }

            if (!Favorite.gameObject.activeSelf)
                Favorite.gameObject.SetActive(true);

            if (info.isPinned)
            {
                Favorite.color = EndColor;
                Favorite.sprite = EnabledFavoriteSprite;

                // if (FavoriteRect == null)
                //     FavoriteRect = Favorite.GetComponent<RectTransform>();

                // FavoriteRect.localScale = startSize;

                // LeanTween.delayedCall(2, () =>
                // {
                //     LeanTween.value(Favorite.gameObject, (x) => { Favorite.color = x; }, StartColor, EndColor, 1).setEaseInOutCubic();
                //     FavoriteRect.LeanScale(EndSize, 1).setEaseOutCubic();
                // });
            }
            else
            {
                // if (LeanTween.isTweening(Favorite.gameObject))
                //     LeanTween.cancel(Favorite.gameObject);

                Favorite.color = DisabledColor;
                Favorite.sprite = DisabledFavoriteSprite;
            }

            string datasetList = "";
            string cardCount = "";

            if (SheetsManager.IsSheetActive(id))
            {
                List<DataSet> dataSets = WarSystem.DataSetManager.GetDataSetsFromSheet(id);

                if (dataSets != null && dataSets.Count > 0)
                {
                    string[] str = new string[dataSets.Count];

                    for (int i = 0; i < dataSets.Count; i++)
                    {
                        str[i] = dataSets[i].DatasetName;
                    }

                    datasetList = $"Datasets ({dataSets.Count}):\n <size=70%> {string.Join(", ", str)}";
                }

                if (info.OpenSheetReference != null)
                {
                    int amt = info.OpenSheetReference.CardCount;

                    cardCount = amt.ToString() + " card".ConvertQty(amt);

                    if (SheetsManager.CurrentSheetID != id.Trim())
                    {

                        if (SheetsManager.TryGetActiveSheet(id, out var theSheet))
                        {
                            if (theSheet.Persistent)
                            {
                                StatusImage.sprite = OpenSheetSprite;
                            }
                            else
                            {
                                StatusImage.sprite = OpenFormsSprite;
                            }

                            StatusImage.color = OpenSpriteColor;
                        }
                        else
                        {
                            StatusImage.sprite = ErrorSprite;
                            StatusImage.color = Color.yellow;
                            Favorite.gameObject.SetActive(false);
                        }

                    }
                    else
                    {
                        StatusImage.sprite = ViewingSprite;
                        StatusImage.color = ViewingSpriteColor;
                    }
                }
                else
                {
                    StatusImage.sprite = ErrorSprite;
                    StatusImage.color = Color.yellow;
                    Favorite.gameObject.SetActive(false);
                }

            }
            else
            {
                if (!File.Exists(info.SheetMetaData.SheetPath))
                {
                    StatusImage.sprite = ErrorSprite;
                    StatusImage.color = Color.yellow;
                }
                else
                {
                    StatusImage.sprite = ClosedSheetSprite;
                    StatusImage.color = ClosedSpriteColor;
                }
            }

            _toolTip.headerText = text;
            string descriptionString = info.SheetMetaData.SheetDescription;
            if (string.IsNullOrEmpty(descriptionString))
                descriptionString = "<No Description>";
            _toolTip.contentText = $"{descriptionString}\nCategories: <size=70%>{final}";
        }

        /// <summary>
        /// Handles the main (background) button
        /// </summary>
        public void MainButton_EventHandler()
        {
            string id = info.SheetMetaData.ID;
            if (id == SheetsManager.CurrentSheetID)
            {
                SheetsManager.ReloadCurrentSheet();
            }
            else
            {
                if (SheetsManager.IsSheetActive(id))
                {
                    SheetsManager.SetSheetCurrent(id);
                }
                else
                {
                    SheetsManager.OpenCardSheet(info.SheetMetaData.SheetPath, SheetsManager.SystemEncryptKey, out var sheetId);
                }
            }

            ActiveSheetsDisplayer.main.ViewReferences();
        }


        /// <summary>
        /// The more button event handler
        /// </summary>
        public void MoreButton_EventHandler()
        {
            List<(string, Action, bool)> actions = new List<(string, Action, bool)>();

            var id = info.SheetMetaData.ID;
            bool pinned = false;
            if (WarSystem.AccountPreferences.PinnedSheets.Contains(id))
            {
                pinned = true;
            }

            if (pinned)
            {
                actions.Add(("Unpin", () =>
                {
                    WarSystem.AccountPreferences.PinnedSheets.Remove(id);
                    ActiveSheetsDisplayer.main.ViewReferences();
                }, pinned));
            }
            else
            {
                actions.Add(("Pin", () =>
                {
                    WarSystem.AccountPreferences.PinnedSheets.Add(id);
                    ActiveSheetsDisplayer.main.ViewReferences();
                }, !pinned));
            }

            if (info.OpenSheetReference == null && info.SheetMetaData != null)
            {
                actions.Add(("Open", () => SheetsManager.OpenCardSheet(info.SheetMetaData.SheetPath, SheetsManager.SystemEncryptKey, out var id), true));
            }

            if (info.OpenSheetReference != null && info.SheetMetaData != null)
            {
                actions.Add(("View", () => SheetsManager.SetSheetCurrent(info.SheetMetaData.ID), true));
                actions.Add(("Data Sets...", null, false));
                actions.Add(("Properties...", () => ActiveSheetsDisplayer.main.SheetProperties(info.SheetMetaData.ID), true));
                actions.Add(("Close", () => SheetsManager.CloseSheet(info.SheetMetaData.ID), true));
            }

            PickMenu.PickMenuManger.main.OpenPickMenu(actions);
        }

        void OnEnable()
        {
            UpdateElement();
        }

        public void TogglePin()
        {
            if (WarSystem.AccountPreferences != null)
            {

                Debug.Log("toggling pin");

                if (WarSystem.AccountPreferences.PinnedSheets.Contains(info.SheetMetaData.ID))
                    WarSystem.AccountPreferences.PinnedSheets.Remove(info.SheetMetaData.ID);
                else
                    WarSystem.AccountPreferences.PinnedSheets.Add(info.SheetMetaData.ID);

                ActiveSheetsDisplayer.main.ViewReferences();

                UserPreferencesHandler.SavePreferences();
            }
        }

        /// <summary>
        /// When the user wants to see who can see what...
        /// </summary>
        /// <param name="selectedPermissions">the permissions</param>
        public void ShowAccountsThatCanSeeSheet(Permissions selectedPermissions)
        {
            PermissionsUI ui = new PermissionsUI();
            ui.ShowAccounts(selectedPermissions.GetAccounts(false), selectedPermissions.Name);
        }

        /// <summary>
        /// Show all the accounts
        /// </summary>
        public void ShowAllAccounts()
        {
            PermissionsUI ui = new PermissionsUI();
            ui.ShowAccounts(Account.GetAccountsList(), "Any");
        }

    }
}
