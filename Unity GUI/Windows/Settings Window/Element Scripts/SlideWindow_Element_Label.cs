/* SlideWindow_Element_Label.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StringUtility;
using UnityEngine;
using UnityEngine.UI;
using WarManager.Backend;
using WarManager.Cards;
using WarManager.Sharing;

namespace WarManager.Unity3D.Windows
{
    public class SlideWindow_Element_Label : MonoBehaviour, ISlideWindow_Element
    {

        private static readonly string URIRegex = @"((\w+:\/\/)[-a-zA-Z0-9:@;?&=\/%\+\.\*!'\(\),\$_\{\}\^~\[\]`#|]+)"; //g

        public TMPro.TMP_Text LabelText;
        public TMPro.TMP_Text ContentText;

        [SerializeField] private TooltipTrigger _trigger;

        [SerializeField] private TooltipTrigger _moreInfoToolTip;

        [SerializeField] TMPro.TMP_InputField _inputField;

        public int Height;

        public SlideWindow_Element_ContentInfo info { get; set; }

        public GameObject targetGameObject => this.gameObject;

        public string SearchContent => info.Content + info.Label + info.ElementType;

        public RectTransform rect;

        [SerializeField] VerticalLayoutGroup LayoutGroup;

        private bool isLink = false;
        private bool isSheetLink = false;

        public void UpdateElement()
        {

            LabelText.text = info.Label;

            CheckURI();

            if (_trigger == null)
            {
                _trigger = GetComponent<TooltipTrigger>();
            }

            if (_inputField != null)
            {
                _inputField.gameObject.SetActive(false);
                _inputField.text = "";
            }

            ContentText.gameObject.SetActive(true);

            if (info.ContentType != ColumnInfo.GetValueTypeOfPhone || info.PhoneNumber.Error)
            {
                ContentText.text = info.Content.Replace("\n", " ");
            }
            else
                ContentText.text = info.PhoneNumber.NumberUS;


            if (_trigger != null)
            {
                _trigger.headerText = info.Label;
                _trigger.contentText = info.Content;
            }

            if (_moreInfoToolTip != null && LayoutGroup != null)
            {

                if (info.DescriptionHeader != null && info.DescriptionHeader.Length > 0 && info.DescriptionInfo != null && info.DescriptionInfo.Length > 0)
                {
                    _moreInfoToolTip.gameObject.SetActive(true);
                    _moreInfoToolTip.headerText = info.DescriptionHeader;
                    _moreInfoToolTip.contentText = info.DescriptionInfo;

                    LayoutGroup.padding.right = 100;
                }
                else
                {
                    _moreInfoToolTip.gameObject.SetActive(false);
                    LayoutGroup.padding.right = 50;
                }
            }
        }

        private void CheckURI()
        {
            Regex webRegex = new Regex(URIRegex);

            if (Directory.Exists(info.Content) || File.Exists(info.Content) || webRegex.IsMatch(info.Content) || info.Content.StartsWith("Sheet:"))
            {
                ContentText.fontStyle = TMPro.FontStyles.Underline;

                if (Directory.Exists(info.Content) || File.Exists(info.Content) || webRegex.IsMatch(info.Content)) isLink = true;

                if (info.Content.StartsWith("Sheet:")) isSheetLink = true;
            }
            else
            {
                ContentText.fontStyle = TMPro.FontStyles.Normal;
                isLink = false;
                isSheetLink = false;
            }
        }

        public override string ToString()
        {
            return info.ToString() + " (" + info.ElementID + ") ";
        }

        public void OnClick()
        {
            if (isLink)
                MessageBoxHandler.Print_Immediate($"Taking you to {info.Content}", "Note", (x) =>
                {
                    if (x)
                    {
                        if (x) Application.OpenURL(info.Content);
                    }
                });

            if (isSheetLink)
                Option_Sheet();
        }


        /// <summary>
        /// find and open the sheet (either open or closed) if possible
        /// </summary>
        public void Option_Sheet()
        {

            string name = info.Content.Remove(0, 6);

            if (SheetsManager.TryGetCurrentSheet(out var currentSheet))
            {
                if (currentSheet.Name == name)
                {
                    MessageBoxHandler.Print_Immediate($"You are already viewing {name.SetStringQuotes()}", "Note");
                    return;
                }
            }

            IEnumerable<Sheet<Card>> activeSheets = new List<Sheet<Card>>();
            activeSheets = from sheet in SheetsManager.Sheets
                           where sheet.Name.Equals(name)
                           select sheet;

            IEnumerable<FileControl<SheetMetaData>> closedSheets = new List<FileControl<SheetMetaData>>();
            closedSheets = from fileControl in WarSystem.CurrentSheetsManifest.Sheets
                           where fileControl.Data.SheetName.Equals(name)
                           select fileControl;


            if (activeSheets.Count() > 1 || closedSheets.Count() > 1)
            {
                MessageBoxHandler.Print_Immediate("There are too many sheets with the name " +
                name.SetStringQuotes(), "Error");
            }
            else if (activeSheets.Count() == 0)
            {
                if (closedSheets.Count() == 1)
                {
                    List<FileControl<SheetMetaData>> data = new List<FileControl<SheetMetaData>>();
                    data.AddRange(closedSheets);

                    if (FileControl<SheetMetaData>.TryGetServerFile(data[0], "v1", WarSystem.CurrentActiveAccount, out var sheet))
                    {
                        MessageBoxHandler.Print_Immediate("Taking you to " + name, "Note", (x) =>
                            {
                                if (x)
                                    SheetsManager.OpenCardSheet(sheet.SheetPath, SheetsManager.SystemEncryptKey, out var id);
                            });
                    }
                }
            }
            else if (activeSheets.Count() == 1)
            {
                List<Sheet<Card>> data = new List<Sheet<Card>>();
                data.AddRange(activeSheets);

                MessageBoxHandler.Print_Immediate("Taking you to " + name, "Note", (x) =>
                    {
                        if (x)
                            SheetsManager.SetSheetCurrent(data[0].ID);
                    });
            }
            else
            {
                MessageBoxHandler.Print_Immediate("Could not find " + name.SetStringQuotes(), "Error");
            }
        }

    }
}
