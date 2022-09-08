
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Unity3D;
using WarManager.Unity3D.Windows;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager.Sharing
{
    /// <summary>
    /// Handles the GUI for and during exporting from War Manager
    /// </summary>
    [Notes.Author("Handles the GUI for and during exporting from War Manager")]
    public class ExportOptionsManager
    {

        ScheduleModel model = new ScheduleModel();
        private bool openBrowser = true;

        private bool useCustomTags = false;

        private List<KeyValuePair<string, DataSet>> _selectedHeaderTags = new List<KeyValuePair<string, DataSet>>();
        public void Export(Action back, string backTitle)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, 0, (x) =>
            {
                back();
            }, ActiveSheetsDisplayer.main.BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo("Get link for selected cards", 0, (x) =>
            {
                GetLink(() => { Export(back, backTitle); }, "Back");
            }, ActiveSheetsDisplayer.main.ShareSprite));

            content.Add(new SlideWindow_Element_ContentInfo("Export (local) Web Page...", 0, (x) =>
            {
                SheetsCardSelectionManager.Main.Print();
            }, ActiveSheetsDisplayer.main.CodeSprite));

            content.Add(new SlideWindow_Element_ContentInfo("*Export CSV (Excel)...", 0, (x) =>
            {
                MessageBoxHandler.Print_Immediate("Coming soon", "Note");
            }, ActiveSheetsDisplayer.main.FileSprite));


            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Export the list of cards to csv
        /// </summary>
        /// <param name="cards">the list of cards</param>
        /// <param name="directoryPath">the directory path</param>
        /// <param name="fileName">the name of the file</param>
        private void ExportToCSV(List<Card> cards, string directoryPath, string fileName, bool separate)
        {
            if (cards == null)
                throw new NullReferenceException("the cards list cannot be null");

            if (cards.Count == 0)
                return;

            List<DataEntry> entries = new List<DataEntry>();

            for (int i = 0; i < cards.Count; i++)
            {
                entries.Add(cards[i].Entry);
            }


            if (separate)
            {

                entries.Sort((x, y) =>
                {
                    return x.DataSet.ID.CompareTo(y.DataSet.ID);
                });

                List<DataEntry> sortedEntries = new List<DataEntry>();
                sortedEntries.Add(entries[0]);

                for (int i = 1; i < entries.Count; i++)
                {
                    if (entries[i].DataSet.ID != entries[i - 1].DataSet.ID)
                    {
                        WriteFile(sortedEntries.ToArray(), directoryPath, fileName);
                        sortedEntries.Clear();
                    }

                    sortedEntries.Add(entries[i]);
                }
            }

        }

        /// <summary>
        /// Write the csv file
        /// </summary>
        /// <param name="entries">the array of entries</param>
        /// <param name="directoryPath">the file path</param>
        /// <param name="fileName">the name of the file</param>
        private void WriteFile(DataEntry[] entries, string directoryPath, string fileName)
        {
            List<string[]> file = new List<string[]>();

            for (int i = 0; i < entries.Length; i++)
            {
                var x = entries[i].GetAllowedDataValues();
                string[] csvLine = new string[x.Length];

                for (int k = 0; k < csvLine.Length; k++)
                {
                    csvLine[k] = x[k].Value.ToString();
                }

                file.Add(csvLine);
            }

            string filePath = directoryPath + @"\" + fileName + " - " + entries[0].DataSet.DatasetName +
             " " + Guid.NewGuid().ToString().Substring(0, 5);

            DataSetTags tags = entries[0].DataSet.AllowedTagsHandler;
            DataFileInstance instance = new DataFileInstance(filePath, tags.SerializedTags.ToArray(), file);
            CSVSerialization.Serialize(instance);
        }

        /// <summary>
        /// UI used to generate a link and submit it to the azure server via a function
        /// </summary>
        /// <param name="back">the back action</param>
        /// <param name="backTitle">the back title</param>
        public void GetLink(Action back, string backTitle)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(backTitle, 0, (x) =>
            {
                back();
            }, ActiveSheetsDisplayer.main.BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo("Get Link", 20));
            var docTitle = new SlideWindow_Element_ContentInfo("Document Title", model.Title, (x) =>
            {
                if (x != null && x != string.Empty)
                    model.Title = x;

                GetLink(back, backTitle);
            });

            docTitle.DescriptionHeader = "Title";
            docTitle.DescriptionInfo = "The title of the document you are about to create";
            content.Add(docTitle);

            var cards = SheetsCardSelectionManager.Main.GetSelectedCards(SheetsManager.CurrentSheetID);

            if (cards.Count < 1)
            {
                CopyPasteDuplicate cardCommands = new CopyPasteDuplicate();
                cardCommands.SelectAll(true);
                cards = SheetsCardSelectionManager.Main.GetSelectedCards(SheetsManager.CurrentSheetID);
            }

            if (cards.Count > 0)
            {

                string usingTags = "No";

                if (useCustomTags)
                {
                    usingTags = "Yes";
                }

                SlideWindow_Element_ContentInfo contentInfo = new SlideWindow_Element_ContentInfo("Use Custom Tags", usingTags, (x) =>
                {
                    if (x == "Yes")
                    {
                        useCustomTags = true;
                    }
                    else
                    {
                        useCustomTags = false;
                    }

                    GetLink(back, backTitle);
                }, new string[2] { "No", "Yes" });

                contentInfo.ContentType = ColumnInfo.GetValueTypeOfBoolean;
                content.Add(contentInfo);

                if (useCustomTags)
                {

                    KeywordSelectionUIHandler handler = new KeywordSelectionUIHandler();

                    if (_selectedHeaderTags != null && _selectedHeaderTags.Count > 0)
                    {

                        List<string> tags = new List<string>();

                        foreach (var x in _selectedHeaderTags)
                        {
                            tags.Add(x.Key + " (" + x.Value.DatasetName + ")");
                        }

                        content.Add(new SlideWindow_Element_ContentInfo("Selected Tags", string.Join(",", tags)));
                    }
                    else
                        content.Add(new SlideWindow_Element_ContentInfo("Selected Tags (optional)", "None Selected"));
                    content.Add(new SlideWindow_Element_ContentInfo("Select Tags...", () =>
                    {
                        var allowedTags = CardUtility.GetAllowedTagsFromCards(cards);

                        handler.SelectKeys("Tags", allowedTags, new List<KeyValuePair<string, DataSet>>(), (x) =>
                        {
                            _selectedHeaderTags = x;
                            GetLink(back, backTitle);
                        });
                    }, null));
                }

                content.Add(new SlideWindow_Element_ContentInfo(20));

                var browserQuestion = new SlideWindow_Element_ContentInfo("Open Browser?", "No", (x) =>
                {
                    openBrowser = x == "Yes";
                });

                browserQuestion.ContentOptions = new List<string>() { "Yes", "No" };
                browserQuestion.ContentType = ColumnInfo.GetValueTypeOfBoolean;
                browserQuestion.DescriptionHeader = "Note";
                browserQuestion.DescriptionInfo = "Due to some cyber security concerns, War Manager might stop working when this setting is set to true.";
                content.Add(browserQuestion);

                bool textMe = false;

                if (UnitedStatesPhoneNumber.TryParse(WarSystem.AccountPreferences.PhoneNumber, out var phone))
                {

                    textMe = true;

                    var textMeQuestion = new SlideWindow_Element_ContentInfo("Text me?", "Yes", (x) =>
                    {
                        textMe = x == "Yes";
                    });

                    textMeQuestion.ContentOptions = new List<string>() { "Yes", "No" };
                    textMeQuestion.ContentType = ColumnInfo.GetValueTypeOfBoolean;
                    textMeQuestion.DescriptionHeader = "What is this?";
                    textMeQuestion.DescriptionInfo = $"After the schedule is created, a link will be sent to you ({WarSystem.AccountPreferences.PhoneNumber}).";
                    content.Add(textMeQuestion);
                }
                else
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Cannot text link", "Your phone number has not been verified in account settings. If you need help, contact support."));
                }


                content.Add(new SlideWindow_Element_ContentInfo(20));
                content.Add(new SlideWindow_Element_ContentInfo("Get Link", 0, (x) =>
                {

                    if ((_selectedHeaderTags == null || _selectedHeaderTags.Count < 1) && useCustomTags)
                    {
                        MessageBoxHandler.Print_Immediate("Cannot create link - make sure you add the tags that you want to see", "Error");
                        return;
                    }

                    if (cards.Count < 1)
                    {
                        MessageBoxHandler.Print_Immediate("Can't get a link - there are no cards that are selected.", "Note");
                        return;
                    }

                    SlideWindowsManager.main.CloseProperties(true);

                    LeanTween.delayedCall(1, () =>
                    {

                        if (!useCustomTags)
                            _selectedHeaderTags = new List<KeyValuePair<string, DataSet>>();

                        SubmitDocToServer doc = new SubmitDocToServer();
                        doc.SubmitCards(model.Title, cards, textMe, openBrowser, _selectedHeaderTags);


                    });

                }, null));
            }
            else
            {

                content.Add(new SlideWindow_Element_ContentInfo(20));
                content.Add(new SlideWindow_Element_ContentInfo("No Cards Selected", "Please select the cards you want to share"));
                content.Add(new SlideWindow_Element_ContentInfo(20));
            }
            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        public Action MainMenuBackAction()
        {
            return () =>
            {
                Export(() => { SlideWindowsManager.main.CloseProperties(true); }, "Back");
            };
        }
    }
}