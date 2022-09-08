using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Sharing;
using WarManager.Cards;
using WarManager.Backend;

using StringUtility;

namespace WarManager.Forms
{
    [Notes.Author("Controls the War Manager Forms Experience")]
    public class FormsController
    {
        /// <summary>
        /// is War Manager currently using forms?
        /// </summary>
        public bool UsingForms { get; private set; }

        public delegate void FormsUsed(object sender, bool used);
        public static event FormsUsed OnForms;

        /// <summary>
        /// the forms sheet being currently used
        /// </summary>
        public Sheet<Card> ActiveFormsSheet { get; private set; }

        /// <summary>
        /// the current forms sheets 
        /// </summary>
        private List<Sheet<Card>> _currentFormsSheets = new List<Sheet<Card>>();

        private string _lastCurrentSheetID;

        /// <summary>
        /// the forms sheets list IEnumerator
        /// </summary>
        public IEnumerator FormsSheet
        {
            get
            {
                yield return _currentFormsSheets;
            }
        }

        /// <summary>
        /// Start using the forms system
        /// </summary>
        /// <param name="formsSheet">the forms sheet being used</param>
        /// <exception cref="ArgumentException">thrown when a sheet is persistent (not allowed)</exception>
        public void StartForms(Sheet<Card> formsSheet)
        {
            if (formsSheet.Persistent)
                throw new ArgumentException("The forms sheet cannot be persistent");

            if (_currentFormsSheets.Contains(formsSheet))
            {
                _currentFormsSheets.Add(formsSheet);
            }

            ActiveFormsSheet = formsSheet;

            var sheet = SheetsManager.GetActiveSheet(SheetsManager.PreviousCurrentSheetID);

            if (sheet.Persistent)
                _lastCurrentSheetID = SheetsManager.PreviousCurrentSheetID;

            if (OnForms != null)
            {
                OnForms(this, true);
                UsingForms = true;
            }
        }

        /// <summary>
        /// Stop using the forms system and cancel the form
        /// </summary>
        public void CancelForms()
        {

            string formName = "the form";

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
                formName = sheet.Name.SetStringQuotes();

            MessageBoxHandler.Print_Immediate($"Cancel {formName}?\nAll progress will be lost.", "Question", (x) =>
            {
                if (x)
                {
                    SheetsManager.CloseSheet(ActiveFormsSheet.ID, false, true);

                    // if (_lastCurrentSheetID != null)
                    //     SheetsManager.SetSheetCurrent(_lastCurrentSheetID);
                }
            });
        }

        /// <summary>
        /// Stop using the forms system (even system)
        /// </summary>
        public void StopFormsEvent()
        {
            if (OnForms != null)
            {
                OnForms(this, false);
                UsingForms = false;
            }
        }
    }
}
