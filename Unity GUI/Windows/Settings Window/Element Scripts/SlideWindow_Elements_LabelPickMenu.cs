
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Sharing;

using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

using StringUtility;

namespace WarManager.Unity3D.Windows
{
    [Notes.Author(2.2, "the slide windows label pick menu")]
    public class SlideWindow_Elements_LabelPickMenu : MonoBehaviour
    {
        [SerializeField] private SlideWindow_Element_Label label;

        private static readonly string filePathRegex = @"(\.\w+$)"; //igm
        private static readonly string URIRegex = @"((\w+:\/\/)[-a-zA-Z0-9:@;?&=\/%\+\.\*!'\(\),\$_\{\}\^~\[\]`#|]+)"; //g

        public void OpenMenu()
        {
            if (IsLabelNull())
            {
                return;
            }

            List<(string, Action, bool)> options = new List<(string, Action, bool)>();

            if (!string.IsNullOrEmpty(label.info.Content))
            {
                options.Add(("Copy Content", Option_Copy, true));
                options.Add(("Text Me Content", Option_TextMe, true));

                if (label.info.ContentType == WarManager.Backend.ColumnInfo.GetValueTypeOfPhone)
                {
                    options.Add(("Message " + label.info.Content, Option_Text, true));
                }

                if (Directory.Exists(label.info.Content))
                {
                    options.Add(("Open File Browser", Option_FileLocation, true));
                }

                if (File.Exists(label.info.Content))
                {
                    options.Add(("Open", Option_FileLocation, true));
                }

                Regex webRegex = new Regex(URIRegex);
                if (webRegex.IsMatch(label.info.Content))
                {
                    options.Add(("Open Web Browser", Option_Link, true));
                }

                if (label.info.Content.StartsWith("Sheet:"))
                {
                    options.Add(("Open Sheet", Option_Sheet, true));
                }
            }
            else
            {

            }

            PickMenu.PickMenuManger.main.OpenPickMenu(options);
        }

        public void Option_Copy()
        {
            if (IsLabelNull())
            {
                return;
            }

            string copyInfo = label.info.Content;

            if (copyInfo != null && !string.IsNullOrEmpty(copyInfo))
                GUIUtility.systemCopyBuffer = copyInfo;
        }

        public void Option_TextMe()
        {
            if (IsLabelNull())
            {
                return;
            }

            TwilioSMSHandler sms = new TwilioSMSHandler();
            System.Threading.Tasks.Task.Run(() => sms.SendMessage(label.info.Content, WarSystem.AccountPreferences.PhoneNumber));
        }

        /// <summary>
        /// Send a quick text to this phone number
        /// </summary>
        public void Option_Text()
        {
            if (IsLabelNull())
            {
                return;
            }

            EditTextMessageBoxController.OpenModalWindow("", "", (x) =>
            {
                TwilioSMSHandler sms = new TwilioSMSHandler();
                string fullNumber = label.info.PhoneNumber.FullNumberUS;

                bool testing = false;

#if UNITY_EDITOR
                testing = true;
#endif

                if (testing)
                {
                    fullNumber = "+19137497477";
                    Debug.Log("Messaging number " + fullNumber);
                }

                System.Threading.Tasks.Task.Run(() => sms.SendMessage(x, fullNumber));
            });
        }

        /// <summary>
        /// Send a quick email to this email address
        /// </summary>
        public void Option_Email()
        {
            if (IsLabelNull())
                return;

            //may want to turn this into a slide window element situation
        }

        /// <summary>
        /// Activate a link
        /// </summary>
        public void Option_Link()
        {
            if (IsLabelNull())
                return;

            MessageBoxHandler.Print_Immediate("Taking you to " + label.info.Content, "Note", (x) =>
            {
                if (x) Application.OpenURL(label.info.Content);
            });
        }

        public void Option_FileLocation()
        {
            if (IsLabelNull())
                return;

            MessageBoxHandler.Print_Immediate("Taking you to " + label.info.Content, "Note", (x) =>
            {
                if (x) Application.OpenURL(label.info.Content);
            });
        }

        /// <summary>
        /// find and open the sheet (either open or closed) if possible
        /// </summary>
        public void Option_Sheet()
        {
            if (IsLabelNull())
                return;

            string name = label.info.Content.Remove(0, 6);

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
                        SheetsManager.SetSheetCurrent(data[0].ID);
                    });
            }
            else
            {
                MessageBoxHandler.Print_Immediate("Could not find " + name.SetStringQuotes(), "Error");
            }
        }

        private bool IsLabelNull()
        {
            if (label == null)
            {
                label = transform.parent.GetComponent<SlideWindow_Element_Label>();
            }

            //if (label == null)
            //    Debug.Log("Could not find label");
            //else
            //{
            //    Debug.Log("Found label");
            //}

            return label == null;
        }
    }
}
