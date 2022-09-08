
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles communication between the backend and unity GUI")]
    public class SheetsCommands : MonoBehaviour
    {
        public List<Button> DisableOnHomeSheetButtons = new List<Button>();

        [SerializeField] DropMenu SearchDropMenu;
        [SerializeField] TMPro.TMP_InputField SearchInputField;

        public bool onHomeSheet;

        /// <summary>
        /// Rename the sheet
        /// </summary>
        public void RenameSheet()
        {
            if (SheetsManager.SheetCount < 1)
                return;

            var sheet = SheetsManager.GetActiveSheet(SheetsManager.CurrentSheetID);
            string name = sheet.Name;

            string id = SheetsManager.CurrentSheetID;

            var renameAction = new Action<string>((x) => SheetsManager.RenameSheet(x, id));

            EditTextMessageBoxController.OpenModalWindow(name, "Rename Sheet", renameAction);
        }

        /// <summary>
        /// Removes the current sheet from the list of active sheets
        /// </summary>
        public void CloseSheet_buttonListener()
        {
            if (SheetsManager.SheetCount <= 1)
            {
                MessageBoxHandler.Print_Immediate("Currently, one sheet must be open at all times", "Notice");
                return;
            }

            byte[] b = WarSystem.CurrentActiveAccount.GetCurrentAccountHashKeyByteArray();

            SheetsManager.CloseSheet(SheetsManager.CurrentSheetID);
        }

        public void DeleteSheet_buttonListener()
        {
            // if (SheetsManager.SheetCount <= 1)
            // {
            //     MessageBoxHandler.Print_Immediate("Currently, one sheet must be open at all times.\n\n In order to delete a sheet, please open or create another sheet and then delete this one.", "Notice");
            //     return;
            // }

            if (SheetsManager.CurrentSheetID != null || SheetsManager.CurrentSheetID != string.Empty)
            {
                SheetsManager.DeleteSheet(SheetsManager.CurrentSheetID);
            }
        }

        /// <summary>
        /// Find cards with a specific text
        /// </summary>
        /// <param name="text">the text to find</param>
        public void Find(string text)
        {
            if (SheetsManager.SheetCount < 1)
                return;

            SheetsManager.Find(text);
        }

        // public void FindCards(List<WarManager.Cards.Card> cards)
        // {
        //     List<string> ids = new List<string>();

        //     foreach (var card in cards)
        //     {
        //         ids.Add("ID: " + card.ID);
        //     }

        //     string final = string.Join(",", ids);

        //     if (!SearchDropMenu.ActiveMenu)
        //     {
        //         SearchDropMenu.ToggleActive();
        //     }

        //     SearchInputField.text = final;
        //     SlideWindowsManager.main.CloseWindows();
        // }

        /// <summary>
        /// Cancel finding something
        /// </summary>
        public void CancelFind()
        {
            if (SheetsManager.SheetCount < 1)
                return;

            SheetsManager.CancelFind();
            ToolsManager.Mode = WarMode.Sheet_Editing;
        }

        /// <summary>
        /// Attempt to quit the session normally
        /// </summary>
        public void QuitSession()
        {
            Application.Quit();
        }

        public void CheckHomeSheet(string id)
        {

            // if (SheetsManager.HomeSheetID != null)
            //     Debug.Log("checking sheet " + SheetsManager.HomeSheetID + " " + id);

            for (int i = 0; i < DisableOnHomeSheetButtons.Count; i++)
            {
                DisableOnHomeSheetButtons[i].interactable = id != SheetsManager.HomeSheetID && SheetsManager.HomeSheetID != null;
            }
        }

        public void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += CheckHomeSheet;
        }

        public void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= CheckHomeSheet;
        }
    }
}
