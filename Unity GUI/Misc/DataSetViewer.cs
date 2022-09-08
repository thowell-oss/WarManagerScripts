

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;
using WarManager.Cards;

using Sirenix.OdinInspector;
using StringUtility;
using WarManager.Sharing.Security;
using WarManager.Sharing;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles communication between the backend and unity GUI
    /// </summary>
    [Notes.Author("Handles communication between the backend and unity GUI")]
    public class DataSetViewer : MonoBehaviour
    {
        [TabGroup("Icons")]
        public Sprite BackSprite;
        [TabGroup("Icons")]
        public Sprite DataSetSprite;
        [TabGroup("Icons")]
        public Sprite DropCardIcon;
        [TabGroup("Icons")]
        public Sprite DropLocationSpriteIcon;
        [TabGroup("Icons")]
        public Sprite SearchIcon;
        [TabGroup("Icons")]
        public Sprite AddIcon;
        [TabGroup("Icons")]
        public Sprite EditIcon;

        private Action<string, string> _cardInfo;
        private Point _defaultDropLocation;
        private Point _setDropLocation;
        private List<Point> _setDropLocations = new List<Point>();
        private int _nextDropIterator = 0;

        private Action selectedDataset;

        private bool dropAtDefault = false;

        private string[] data = new string[0];

        [SerializeField] SheetsCommands Commands;

        DataEntry newDataEntry;

        private int addLocation = -1;

        public delegate void addNewCard_delegate(Point p, string id);
        public static event addNewCard_delegate OnAddNewCard;

        [Space]
        [PropertyTooltip("Handles detecting if the reference tab is showing the 'home' tab: showing all available datasets/sheets")]
        public bool OnHomeScreen = true;

        #region singleton
        public static DataSetViewer main;

        /// <summary>
        /// called by unity
        /// </summary>
        public void Awake()
        {
            if (main != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                main = this;
            }
        }

        #endregion


        public void QuickDropCard(string sheetId, Point dropPoint, DataSet set)
        {
            SheetDropPointManager.SetNewCustomPoints(sheetId, new List<Point>() { dropPoint });

            _setDropLocation = dropPoint;

            _setDropLocations.Clear();
            _setDropLocations.Add(dropPoint);


            ShowDataSet(set.ID, () => { SlideWindowsManager.main.CloseWindows(); });
            selectedDataset = () => ShowDataSet(set.ID, () => { DropCardMainMenu(sheetId, (x, y) => { SlideWindowsManager.main.CloseWindows(); }, new List<Point>() { dropPoint }, false); });

        }

        /// <summary>
        /// Gives options to drop a selected card onto the sheet
        /// </summary>
        /// <param name="sheetId"></param>
        /// <param name="dataSetRepIdCallBack"></param>
        /// <param name="dropLocations"></param>
        public void DropCardMainMenu(string sheetId, Action<string, string> dataSetRepIdCallBack, List<Point> dropLocations, bool resetDropPointsIterator = true, bool moveCamera = true)
        {

            if (moveCamera)
            {
                if (dropLocations.Count > 1)
                {
                    var bounds = Rect.DrawRectFromListOfSpaces(dropLocations);
                    Pointf worldCenter = bounds.GetWorldCenter(SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID));
                    WarManagerCameraController.MainController.MoveCamera(worldCenter);
                }
                else if (dropLocations.Count == 1)
                {
                    WarManagerCameraController.MainController.MoveCamera(Point.GridToWorld(dropLocations[0], SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID)));
                }
            }

            SheetDropPointManager.SetNewCustomPoints(SheetsManager.CurrentSheetID, dropLocations);

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(10));

            // if (dropLocations.Count > 1)
            // {
            //     content.Add(new SlideWindow_Element_ContentInfo(" ", "Stretched cards are not supported at this time")); //TODO: let users select multiple locations and choose one dataset
            //     SlideWindowsManager.main.CloseWindows();
            //     SlideWindowsManager.main.AddPropertiesContent(content, true);

            //     CompleteDrop(null);
            //     return;
            // }

            _cardInfo = dataSetRepIdCallBack;
            _setDropLocation = dropLocations[0];

            _setDropLocations.Clear();
            _setDropLocations.AddRange(dropLocations);

            if (resetDropPointsIterator)
                _nextDropIterator = 0;

            content.Add(new SlideWindow_Element_ContentInfo("Close", -1, (x) =>
            {
                CompleteDrop(null, Point.zero);
                SlideWindowsManager.main.CloseWindows();
                SlideWindowsManager.main.ClearProperties();
                ActiveSheetsDisplayer.main.ViewReferences();
            }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(30));

            content.Add(new SlideWindow_Element_ContentInfo("Select a Data Set", 15));

            content.Add(new SlideWindow_Element_ContentInfo("Action", -1, (x) =>
            {
                ShowActions(() => DropCardMainMenu(sheetId, dataSetRepIdCallBack, dropLocations, false));
                selectedDataset = () => ShowActions(() => { DropCardMainMenu(sheetId, dataSetRepIdCallBack, dropLocations, false); });
            }, DataSetSprite));

            string[] datasetIds = new string[0];

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                datasetIds = sheet.GetDatasetIDs();
            }

            List<DataSet> dataSets = WarSystem.DataSetManager.GetDataSetsFromSheet(sheetId);

            dataSets = WarSystem.DataSetManager.GetDataSetsFromSheet(sheetId);


            if (dataSets != null && dataSets.Count > 0)
            {
                foreach (var dataset in dataSets)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(dataset.DatasetName, -1, (x) =>
                    {
                        ShowDataSet(dataset.ID, () => { DropCardMainMenu(sheetId, dataSetRepIdCallBack, dropLocations, false); });
                        selectedDataset = () => ShowDataSet(dataset.ID, () => { DropCardMainMenu(sheetId, dataSetRepIdCallBack, dropLocations, false); });
                    }, DataSetSprite));
                }
            }

            content.Add(new SlideWindow_Element_ContentInfo(50));

            content.Add(new SlideWindow_Element_ContentInfo("Default drop point", _defaultDropLocation.ToString()));
            content.Add(new SlideWindow_Element_ContentInfo("Make " + dropLocations[0] + " my new default drop point", -1, (x) =>
            {
                _defaultDropLocation = dropLocations[0];

                SheetDropPointManager.SetDropPoint(_defaultDropLocation, sheetId);
                DropCardMainMenu(sheetId, dataSetRepIdCallBack, dropLocations);

                // if (_dropPointMarker != null)
                //     _dropPointMarker.CurrentPoint = dropLocations[0];

            }, DropLocationSpriteIcon));

            SlideWindowsManager.main.CloseWindows();
            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Show the system cards to spawn
        /// </summary>
        /// <param name="back"></param>
        public void ShowActions(System.Action back)
        {
            var SystemCardsManager = WarSystem.DataSetManager.ActionCardsManager;

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(10));
            content.Add(new SlideWindow_Element_ContentInfo("", 0, (x) => { back(); }, BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(50));
            content.Add(new SlideWindow_Element_ContentInfo("Cards", null));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            foreach (var x in WarSystem.DataSetManager.ActionCardsManager.ActionCards)
            {
                content.Add(new SlideWindow_Element_ContentInfo(x.Name, () =>
                {
                    string id = Guid.NewGuid().ToString();
                    ShowDataEntryInfo(SystemCardsManager.ParseID("sys" + x.DataSetID + ":", id), () => { ShowActions(back); }, "Action");
                }, DropCardIcon));
            }

            // content.Add(new SlideWindow_Element_ContentInfo("Interaction", 20));
            // content.Add(new SlideWindow_Element_ContentInfo("Duplicate Card Finder", 0, (x) =>
            // {
            //     string id = Guid.NewGuid().ToString();
            //     ShowDataEntryInfo(SystemCardsManager.ParseID("sys5bc9541b-27ee-4a70-8d06-cef398325e65:", id),
            //  () => { ShowActions(back); }, "Action");
            // }, DropCardIcon));
            // content.Add(new SlideWindow_Element_ContentInfo("Keyword Counter", () =>
            // {
            //     string id = Guid.NewGuid().ToString();
            //     ShowDataEntryInfo(SystemCardsManager.ParseID("sys69d9e1a9-1b32-4fb8-a7d5-2a2f3c6a36ef:", id), () =>
            //     {
            //         ShowActions(back);
            //     }, "Action");
            // }));
            // content.Add(new SlideWindow_Element_ContentInfo("Keyword Checker", () =>
            // {
            //     string id = Guid.NewGuid().ToString();
            //     ShowDataEntryInfo(SystemCardsManager.ParseID("sysb9697564-52e2-4ec8-9e58-3914232b2b4c:", id), () => { ShowActions(back); }, "Action");
            // }, DropCardIcon));
            // content.Add(new SlideWindow_Element_ContentInfo("Card Counter", 0, (x) =>
            // {
            //     string id = Guid.NewGuid().ToString();
            //     ShowDataEntryInfo(SystemCardsManager.ParseID("sys941d5a04-03fe-4bfe-b4d1-f73440f02e79:", id),
            //  () => { ShowActions(back); }, "Action");
            // }, DropCardIcon));
            // content.Add(new SlideWindow_Element_ContentInfo(20));
            // content.Add(new SlideWindow_Element_ContentInfo("Presentation", 20));
            // content.Add(new SlideWindow_Element_ContentInfo("Note", 0, (x) => { MessageBoxHandler.Print_Immediate("Coming Soon", "Note"); }, DropCardIcon));
            // content.Add(new SlideWindow_Element_ContentInfo("Title", 0, (x) => { MessageBoxHandler.Print_Immediate("Coming Soon", "Note"); }, DropCardIcon));
            // content.Add(new SlideWindow_Element_ContentInfo(20));
            // content.Add(new SlideWindow_Element_ContentInfo("Other", 20));
            // content.Add(new SlideWindow_Element_ContentInfo("Weather", 0, (x) =>
            // {
            //     string id = Guid.NewGuid().ToString();
            //     ShowDataEntryInfo(SystemCardsManager.ParseID("sys7fa7474f-5536-4fdd-b970-b435a6280584:", id),
            //  () => { ShowActions(back); }, "Action");
            // }, DropCardIcon));

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Display all the datapieces of the dataset
        /// </summary>
        /// <param name="datasetId">the id of the dataset</param>
        /// <param name="back">handles going back to the previous menu</param>
        public void ShowDataSet(string datasetId, Action back)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            OnHomeScreen = false;

            content.Add(new SlideWindow_Element_ContentInfo(20));

            // content.Add(new SlideWindow_Element_ContentInfo("", -1, (x) =>
            // {
            //     CompleteDrop(null, Point.zero);
            //     SlideWindowsManager.main.ClearReference();
            //     ActiveSheetsDisplayer.main.ViewReferences();
            //     OnHomeScreen = true;
            // }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo("", -1, (x) => { back(); }, BackSprite));

            string searchFilterString = "-";


            if (WarSystem.DataSetManager.TryGetDataset(datasetId, out DataSet foundDataSet))
            {
                if (foundDataSet.SelectedView.CanViewCard)
                {

                    searchFilterString = foundDataSet.SearchFilterString;

                    content.Add(new SlideWindow_Element_ContentInfo(foundDataSet.DatasetName, null));

                    if (foundDataSet.SelectedView.CanEditData && foundDataSet.Tags.Count() > 0)
                        content.Add(new SlideWindow_Element_ContentInfo("New \'" + foundDataSet.DatasetName + "\'", -1,
                         (x) => { AddToDataSet(foundDataSet, () => { ShowDataSet(datasetId, back); }, foundDataSet.DatasetName, Guid.NewGuid().ToString()); }, AddIcon));

                    // Debug.Log("Dataset " + dataset.DatasetName + " count " + dataset.DataCount);

                    List<SlideWindow_Element_ContentInfo> cardContent = new List<SlideWindow_Element_ContentInfo>();


                    if (!datasetId.StartsWith("sys"))
                    {
                        int i = 0;
                        foreach (var entry in foundDataSet.Entries)
                        {
                            var dataEntry = entry;

                            SlideWindow_Element_ContentInfo card = new SlideWindow_Element_ContentInfo(entry,
                             (x) => { ShowDataEntryInfo(dataEntry, () => { ShowDataSet(datasetId, back); }, foundDataSet.DatasetName); });

                            var values = entry.GetAllowedDataValues();

                            StringBuilder sortString = new StringBuilder();

                            for (int j = 0; j < values.Length; j++)
                            {
                                sortString.Append((string)values[j].Value);
                                sortString.Append(" ");
                            }

                            card.SortString = sortString.ToString();

                            cardContent.Add(card);

                            i++;
                        }


                        // for (int i = 1; i < selectedDataSet.DataCount; i++)
                        // {
                        //     var card = new SlideWindow_Element_ContentInfo(selectedDataSet.ID, i, (x) => { ShowDataEntryInfo(selectedDataSet.GetEntry(x)); });
                        //     cardContent.Add(card);

                        //     bool start = true;

                        //     foreach (var element in selectedDataSet.SelectedView.ElementDataArray)
                        //     {
                        //         if ((element.ElementTag == "text" || element.ElementTag == "link") && start)
                        //         {
                        //             card.SortString = selectedDataSet.GetData(i).GetData(element.ID);
                        //             start = false;
                        //         }
                        //     }
                        // }

                        cardContent.Sort();

                        char c = (char)47;


                        for (int j = 0; j < cardContent.Count; j++)
                        {
                            if (cardContent.Count > 50)
                            {

                                if (cardContent[j].SortString != null && cardContent[j].SortString.Trim().Length > 0)
                                {
                                    while (cardContent[j].SortString.ToUpper()[0] != c && (int)c > 47 && (int)c <= 122)
                                    {
                                        if (Mathf.Abs((int)c - (int)cardContent[j].SortString.ToUpper()[0]) > 3)
                                        {
                                            c = cardContent[j].SortString.ToUpper()[0];
                                        }
                                        else
                                        {

                                            int x = (int)c + 1;
                                            c = (char)x;
                                        }

                                        if ((int)c <= 57 || ((int)c >= 65 && (int)c <= 90) || (int)c > 97)
                                        {
                                            // Debug.Log((int)c + " " + c);
                                            content.Add(new SlideWindow_Element_ContentInfo(c.ToString(), 15));
                                        }
                                    }


                                    if (cardContent[j].SortString.ToUpper()[0] != c)
                                    {
                                        c = cardContent[j].SortString.ToUpper()[0];
                                        // cardContent.Insert(j, new SlideWindow_Element_ContentInfo(c.ToString(), 15));
                                    }
                                }
                            }

                            content.Add(cardContent[j]);
                        }
                    }
                }
                else
                {
                    content.Add(new SlideWindow_Element_ContentInfo($"Cannot View {foundDataSet.DatasetName}", "You are not allowed to see this Data Set - check your views if you think you should be able to."));
                    content.Add(new SlideWindow_Element_ContentInfo($"{foundDataSet.DatasetName} views", 0, (x) =>
                        {
                            Action backAction = () => { ShowDataSet(datasetId, back); };

                            ShowViews(backAction, foundDataSet.DatasetName, foundDataSet);
                        }, null));
                }
            }
            else
            {
                NotificationHandler.Print("Could not load a dataset");
            }

            SlideWindowsManager.main.CloseWindows(true);
            SlideWindowsManager.main.AddReferenceContent(content, true, searchFilterString);
        }


        /// <summary>
        /// Add an entry to the data set
        /// </summary>
        /// <param name="set">the data set</param>
        /// <param name="back">the back action</param>
        /// <param name="backTitle">the title of the back button</param>
        /// <param name="rowLocation">the location of the entry</param>
        public void AddToDataSet(DataSet set, Action back, string backTitle, string rowLocation)
        {
            if (set == null || set.Tags.Length < 1)
            {
                NotificationHandler.Print("There was an error attempting to add an item to the Dataset");
                return;
            }

            string[] tags = set.Tags;

            data = new string[tags.Length];

            List<DataValue> values = new List<DataValue>();


            // DataEntry entry = set.GetTemplateDataEntry(rowLocation);

            // EditEntry(entry, false, entry.GetAllowedDataValues());

            DataEntryUIEditController UIEditController = null;

            UIEditController = new DataEntryUIEditController(set.GetTemplateDataEntry(rowLocation), (x) =>
            {
                if (x)
                {
                    ShowDataEntryInfo(UIEditController.EntryToEdit, back, backTitle);
                }
                else
                {
                    SlideWindowsManager.main.OpenReference(true);
                }
            }, DataAction.Append);

            UIEditController.EditValues();
        }

        #region old

        /// <summary>
        /// Tweak data from a specific data piece
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="set"></param>
        // private void EditEntry(DataEntry entry, bool editingEntry, Action back, string backTitle, DataValue[] valuesToEdit)
        // {
        //     if (entry == null)
        //         throw new NullReferenceException("The data entry cannot be null");

        //     List<string> str = new List<string>();

        //     foreach (var v in entry.Values)
        //     {
        //         str.Add(v.Value.ToString());
        //     }

        //     Debug.Log(string.Join(",", str));

        //     // Debug.Log("refreshed");

        //     List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

        //     content.Add(new SlideWindow_Element_ContentInfo(20));

        //     content.Add(new SlideWindow_Element_ContentInfo("Cancel", -1, (x) =>
        //     {
        //         try
        //         {
        //             if (editingEntry)
        //             {
        //                 ShowDataEntryInfo(entry, back, backTitle, true);
        //             }
        //             else
        //             {
        //                 SlideWindowsManager.main.OpenReference(true);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             Debug.LogError(ex.Message);
        //             NotificationHandler.Print(ex.Message);
        //         }
        //         // ShowDataSet(piece.DataSetId);
        //         addLocation = -1;
        //         entry = null;

        //     }, BackSprite));

        //     content.Add(new SlideWindow_Element_ContentInfo("Edit", null));

        //     if (!entry.DataSet.CanEditData)
        //     {
        //         return;
        //     }

        //     var newDataEntry = entry.Copy();

        //     var newValues = newDataEntry.GetAllowedDataValues();

        //     var values = new DataValue[newValues.Length];

        //     Array.Copy(valuesToEdit, newValues, values.Length);

        //     for (int i = 0; i < values.Length; i++)
        //     {
        //         int loc = i;

        //         content.Add(new SlideWindow_Element_ContentInfo(values[i].HeaderName, values[i].ParseToParagraph(), (x) =>
        //         {
        //             values[loc].ReplaceData(new ValueTypePair(x, values[loc].ValueType));
        //             EditEntry(entry, true, back, backTitle, values);
        //         }));
        //     }

        //     content.Add(new SlideWindow_Element_ContentInfo(50));
        //     content.Add(new SlideWindow_Element_ContentInfo("Save", -1, (x) =>
        //     {
        //         DataEntry newEntry = new DataEntry(entry.Row, valuesToEdit, entry.DataSet);

        //         if (addLocation <= 0 || addLocation >= entry.DataSet.DataCount)
        //         {
        //             if (entry.DataSet.AppendEntries(newEntry))
        //             {
        //                 // NotificationHandler.Print("Data set successfully appended");
        //             }
        //             else
        //             {
        //                 NotificationHandler.Print("Error appending file - Error code: Bat");
        //             }
        //         }
        //         else
        //         {
        //             if (entry.DataSet.ReplaceEntry(newEntry))
        //             {
        //                 // NotificationHandler.Print("Data set entry successfully replaced");
        //             }
        //             else
        //             {
        //                 NotificationHandler.Print("Error rewriting file - Error code: Bat");
        //             }
        //         }

        //         ShowDataEntryInfo(entry, back, backTitle);

        //         addLocation = -1;
        //         entry = null;

        //         WarSystem.DeveloperPushNotificationHandler.EditedData = true;

        //     }, AddIcon));

        //     content.Add(new SlideWindow_Element_ContentInfo(50));

        //     SlideWindowsManager.main.AddReferenceContent(content, true);
        // }
        #endregion

        /// <summary>
        /// Show the data piece from a data set
        /// </summary>
        /// <param name="set">the data set the piece is coming from</param>
        /// <param name="repId">the id of the data piece</param>
        public void ShowDataEntryInfo(DataEntry dataEntry, Action back, string backTitle, bool forceOpen = true)
        {
            if (dataEntry == null)
            {
                Debug.Log("data entry is null");
                return;
            }

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            if (back != null)
            {
                content.Add(new SlideWindow_Element_ContentInfo(backTitle, -1, (x) => { back(); }, BackSprite));
                content.Add(new SlideWindow_Element_ContentInfo("Back to References", () => { ActiveSheetsDisplayer.main.ViewReferences(); }, ActiveSheetsDisplayer.main.FolderSprite));
                // Debug.Log("back not null");
            }
            else
            {
                //Debug.Log("back is null");
            }

            if (dataEntry.DataSet != null)
            {

                if (!dataEntry.DataSet.SelectedView.CanViewCard)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("", "Cannot view data"));
                    SlideWindowsManager.main.AddReferenceContent(content, forceOpen, dataEntry.DataSet.SearchFilterString);
                    return;
                }

                if (dataEntry.Actor != null)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(20));
                    content.Add(new SlideWindow_Element_ContentInfo(dataEntry.Actor.Name, null));
                    content.Add(new SlideWindow_Element_ContentInfo("", dataEntry.Actor.Description));
                }
                else
                {
                    content.Add(new SlideWindow_Element_ContentInfo(dataEntry.DataSet.DatasetName + " - " + dataEntry.RowID, null));
                }

                content.Add(new SlideWindow_Element_ContentInfo(20));

                if (SheetsManager.SheetCount > 0)
                {
                    if (dataEntry != null)
                    {
                        DataEntryUIEditController editController = new DataEntryUIEditController(dataEntry, (x) =>
                        {
                            ShowDataEntryInfo(dataEntry, back, backTitle);

                            if (x)
                                Debug.Log("going back - saved");
                        });

                        if (dataEntry.DataSet.SelectedView.CanEditData)
                        {

                            if ((dataEntry.DataSet.ID.StartsWith("sys") && dataEntry.Actor.Card != null) || !dataEntry.DataSet.ID.StartsWith("sys"))
                            {

                                content.Add(new SlideWindow_Element_ContentInfo("Edit", -1, (x) =>
                                {
                                    // addLocation = dataEntry.Row;
                                    // EditEntry(dataEntry, true, dataEntry.GetAllowedDataValues());
                                    editController.EditValues();
                                    // editController.EditValues();

                                }, EditIcon));
                            }
                        }
                        else
                        {
                            content.Add(new SlideWindow_Element_ContentInfo(20));
                            content.Add(new SlideWindow_Element_ContentInfo("No Editing", "You do not have permission to edit - check your views if you think you have access."));
                            content.Add(new SlideWindow_Element_ContentInfo($"{dataEntry.DataSet.DatasetName} Views", 0, (x) =>
                            {
                                Action a = () => ShowDataEntryInfo(dataEntry, back, backTitle, forceOpen);
                                ShowViews(a, $"{dataEntry.DataSet.DatasetName}", dataEntry.DataSet);
                            }, null));
                            content.Add(new SlideWindow_Element_ContentInfo(20));
                        }
                    }
                }

                DataValue[] allowedValues = dataEntry.GetAllowedDataValues();
                //Debug.Log(string.Join<DataValue>(",", allowedValues));

                for (int i = 0; i < allowedValues.Length; i++)
                {
                    string valueContent = allowedValues[i].ParseToParagraph();

                    if (valueContent == null && valueContent.Trim() == string.Empty)
                        valueContent = "<empty>";

                    var label = new SlideWindow_Element_ContentInfo(allowedValues[i].HeaderName, valueContent);
                    label.ContentType = allowedValues[i].GetColumnInfo().ValueType;
                    label.DescriptionHeader = allowedValues[i].GetColumnInfo().HeaderName;
                    label.DescriptionInfo = allowedValues[i].GetColumnInfo().Description;

                    if (allowedValues[i].GetColumnInfo().ValueType == ColumnInfo.GetValueTypeOfPhone)
                    {
                        label.PhoneNumber = allowedValues[i].ParseToPhone();
                    }

                    content.Add(label);
                }

                content.Add(new SlideWindow_Element_ContentInfo(30));
                content.Add(new SlideWindow_Element_ContentInfo("Drop Options", 20));
                if (SheetsManager.TryGetCurrentSheet(out var currentSheet))
                {
                    if ((currentSheet.ContainsDataSet(dataEntry.DataSet.ID) || dataEntry.DataSet.ID.StartsWith("sys"))) //need to check if the sheet can be edited or not
                    {
                        if (_cardInfo != null)
                        {
                            if (_setDropLocations.Count > 1)
                            {
                                if (_nextDropIterator < _setDropLocations.Count)
                                    content.Add(new SlideWindow_Element_ContentInfo("Drop one at: " + _setDropLocations[_nextDropIterator] + " (" + _nextDropIterator + ")", -1, (x) =>
                                    {
                                        dropAtDefault = false;
                                        CompleteDrop(dataEntry, _setDropLocations[_nextDropIterator], false);
                                        _nextDropIterator++;
                                        selectedDataset();

                                    }, DropCardIcon));

                                content.Add(new SlideWindow_Element_ContentInfo("Drop all the same cards: " + string.Join(",", _setDropLocations), -1, (x) =>
                                {
                                    for (int i = 0; i < _setDropLocations.Count; i++)
                                    {
                                        dropAtDefault = false;
                                        CompleteDrop(dataEntry, _setDropLocations[i]);
                                    }
                                }, DropCardIcon));
                            }

                            content.Add(new SlideWindow_Element_ContentInfo(30));
                        }

                        content.Add(new SlideWindow_Element_ContentInfo("Drop at: " + _setDropLocation, -1, (x) =>
                            {
                                dropAtDefault = false;
                                CompleteDrop(dataEntry, _setDropLocation);
                            }, DropCardIcon));

                        content.Add(new SlideWindow_Element_ContentInfo(20));

                        content.Add(new SlideWindow_Element_ContentInfo("Drop at: " + SheetDropPointManager.GetDropPoint().ToString() + " (Default)", -1, (x) =>
                                      {
                                          _defaultDropLocation = SheetDropPointManager.GetDropPoint();

                                          dropAtDefault = true;
                                          CompleteDrop(dataEntry, SheetDropPointManager.GetDropPoint());
                                      }, DropCardIcon));
                    }
                    else
                    {
                        //if the sheet can be edited...
                        content.Add(new SlideWindow_Element_ContentInfo(20));
                        content.Add(new SlideWindow_Element_ContentInfo($"No dropping this card...", $"The {dataEntry.DataSet.DatasetName.SetStringQuotes()} data set is not associated with {currentSheet.Name.SetStringQuotes()}"));
                        content.Add(new SlideWindow_Element_ContentInfo(20));
                        content.Add(new SlideWindow_Element_ContentInfo($"Add {dataEntry.DataSet.DatasetName.SetStringQuotes()} to {currentSheet.Name}", 0, (x) =>
                        {
                            ActiveSheetsDisplayer.main.SheetProperties(() =>
                            {
                                ShowDataEntryInfo(dataEntry, back, backTitle, forceOpen);
                            }, dataEntry.DataSet.DatasetName, currentSheet.ID); //band-aid fix for now...
                        }, AddIcon));
                    }
                }

                #region developer features

                // if (WarSystem.CurrentActiveAccount.Permissions.ContainsKeywordPermission("Developer"))
                // {
                //     content.Add(new SlideWindow_Element_ContentInfo(70));

                //     content.Add(new SlideWindow_Element_ContentInfo("Developer Info", null));

                //     var row = dataEntry.Row.ToString();

                //     if (row == null)
                //     {
                //         row = "";
                //     }

                //     content.Add(new SlideWindow_Element_ContentInfo("Data ID", row));

                //     if (SheetsManager.SheetCount > 0)
                //     {
                //         content.Add(new SlideWindow_Element_ContentInfo("Cards containing the row id", null));

                //         var cards = CardUtility.GetCardsFromCurrentSheet();

                //         foreach (var item in cards)
                //         {
                //             if ((int)item.DataRepID == dataEntry.Row && item.DatasetID == dataEntry.DataSet.ID)
                //             {
                //                 content.Add(new SlideWindow_Element_ContentInfo("Card ID", item.DataRepID.ToString()));

                //                 // geDebug.Log("found id " + item.DataRepID);
                //             }

                //             // Debug.Log("id " + item.DataRepID + " " + item.point.ToString() + " data rep id " + item.DataRepID);
                //         }
                //     }
                // }
                #endregion

            }
            else
            {
                content.Add(new SlideWindow_Element_ContentInfo("Could not find any data set for entry", null));
            }

            SlideWindowsManager.main.AddReferenceContent(content, forceOpen, dataEntry.DataSet.SearchFilterString);
        }

        /// <summary>
        /// Complete the dropping of the card
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="repId"></param>
        private void CompleteDrop(DataEntry entry, Point dropLocation, bool clearDropLocations = true)
        {
            if (entry != null)
            {
                if (string.IsNullOrEmpty(entry.RowID))
                    entry.UpdateRowID(Guid.NewGuid().ToString());
                CreateDataCard(entry, dropLocation);
            }
            else
            {
                _cardInfo = null;
            }

            if (clearDropLocations && SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                SheetDropPointManager.SetNewCustomPoints(sheet.ID, new List<Point>());
            }
        }


        /// <summary>
        /// Create the card
        /// </summary>
        /// <param name="entry">the data entry</param>
        /// <param name="dropLocation">the selected drop location</param>
        private void CreateDataCard(DataEntry entry, Point dropLocation)
        {
            // Point dropLocation = SheetDropPointManager.GetDropPoint();

            // if (!dropAtDefault)
            // {
            //     dropLocation = _setDropLocation;
            // }
            // else
            // {
            //     dropAtDefault = false;
            // }

            if (_cardInfo != null)
            {
                _cardInfo(entry.DataSet.ID, entry.RowID);
            }

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var layer = sheet.CurrentLayer;

                if (CardUtility.TryDropCard(sheet, dropLocation, layer, entry, out string cardID))
                {
                    // if (OnAddNewCard != null)
                    // {
                    //     OnAddNewCard(dropLocation, cardID);
                    // }

                    var card = CardUtility.GetCard(dropLocation, sheet.CurrentLayer, sheet.ID);
                    ShowDataEntryInfo(card.Entry, () => { ShowDataSet(card.Entry.DataSet.ID, () => { ActiveSheetsDisplayer.main.ViewReferences(); }); }, card.Entry.DataSet.DatasetName, true);

                    WarManagerCameraController.MainController.MoveCamera(dropLocation);
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Could not add card to the sheet. " +
                    "Make sure there is enough room for the card to be added (ie. locked cards in the way of other cards shifting).", "Error");
                }
            }
        }

        // public void ShowAllCardsInSheet(string sheetId)
        // {
        //     if (SheetsManager.TryGetActiveSheet(sheetId, out var sheet))
        //     {
        //         var cards = sheet.GetAllObj();

        //         List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

        //         foreach (var card in cards)
        //         {
        //             content.Add(new SlideWindow_Element_ContentInfo(card.DatasetID, (int)card.DataRepID, x =>
        //             {
        //                 if (WarSystem.DataSetManager.TryGetDataset(card.DatasetID, out var dataSet))
        //                 {
        //                     ShowDataEntryInfo(dataSet, (int)card.DataRepID);
        //                 }
        //             }));
        //         }
        //     }
        // }

        /// <summary>
        /// Show the properties of the War Manager
        /// </summary>
        /// <param name="set">the data set</param>
        public void ShowDataSetProperties(Action back, string backTitle, DataSet set)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, 0, (x) =>
            {
                back();
            }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo("Data Set Properties", null));

            content.Add(new SlideWindow_Element_ContentInfo("Name", set.DatasetName));
            content.Add(new SlideWindow_Element_ContentInfo("Default Search Filter", set.SearchFilterString));
            content.Add(new SlideWindow_Element_ContentInfo("Allowed Columns", string.Join(", ", set.AllowedTags)));
            content.Add(new SlideWindow_Element_ContentInfo("Owner", set.Owner));
            content.Add(new SlideWindow_Element_ContentInfo("Views...", 0, (x) =>
            {
                ShowViews(() => { ShowDataSetProperties(back, backTitle, set); }, $"{set.DatasetName} Properties", set);
            }, null));

            if (WarSystem.CurrentActiveAccount.IsAdmin)
            {
                content.Add(new SlideWindow_Element_ContentInfo(20));
                content.Add(new SlideWindow_Element_ContentInfo("Administrator Privileges", 20));
                if (set.DataFile != null)
                    content.Add(new SlideWindow_Element_ContentInfo("File Location (data)", set.DataFile.FilePath));

                if (WarSystem.CurrentActiveAccount.IsAdmin)
                    content.Add(new SlideWindow_Element_ContentInfo($"View JSON (code)", 0, (x) =>
                    {
                        Application.OpenURL(set.DataSetLocation);
                    }, ActiveSheetsDisplayer.main.CodeSprite));

                content.Add(new SlideWindow_Element_ContentInfo(20));
            }

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Show the views
        /// </summary>
        /// <param name="back">back button action</param>
        /// <param name="backTitle">the back title</param>
        /// <param name="set">the data set in view</param>
        public void ShowViews(Action back, string backTitle, DataSet set)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, 0, (x) =>
            {
                back();
            }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo("Views", 20));
            content.Add(new SlideWindow_Element_ContentInfo(set.SelectedView.ViewName, set.SelectedView.ViewDescription));

            content.Add(new SlideWindow_Element_ContentInfo(30));
            foreach (DataSetView view in set.GetAllowedViews(WarSystem.CurrentActiveAccount))
            {
                if (set.SelectedView != view)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(view.ViewName.SetStringQuotes(), view.ViewDescription));

                    string nameOfViewWithQuotes = view.ViewName.SetStringQuotes();

                    content.Add(new SlideWindow_Element_ContentInfo($"Use {nameOfViewWithQuotes}", 0, (x) =>
                    {
                        SetView(set, view);
                        ShowViews(back, backTitle, set);
                    }, null));
                    content.Add(new SlideWindow_Element_ContentInfo(20));
                }
            }


            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo("Data Set View Properties", 20));

            foreach (DataSetView view in set.GetAllowedViews(WarSystem.CurrentActiveAccount))
                content.Add(new SlideWindow_Element_ContentInfo(view.ViewName.SetStringQuotes() + " properties", () =>
                                    {
                                        ShowViewProperties(view, set, () => { ShowViews(back, backTitle, set); }, "Views");
                                    }, null));

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Show the properties of a certian view
        /// </summary>
        /// <param name="view">the view</param>
        /// <param name="set">the data set</param>
        /// <param name="back">back</param>
        /// <param name="backTitle">back title</param>
        private void ShowViewProperties(DataSetView view, DataSet set, Action back, string backTitle)
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo(backTitle, back, ActiveSheetsDisplayer.main.BackSprite));
            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo(view.ViewName.SetStringQuotes(), view.ViewDescription));
            content.Add(new SlideWindow_Element_ContentInfo("Version", view.Version.ToString()));
            content.Add(new SlideWindow_Element_ContentInfo("Data Set", set.DatasetName));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Currently Selected?", (set.SelectedView == view).ToString()));
            content.Add(new SlideWindow_Element_ContentInfo("Detected Error?", view.ErrorCreatingCardManual.ToString()));
            content.Add(new SlideWindow_Element_ContentInfo(30));
            content.Add(new SlideWindow_Element_ContentInfo("Permissions", 20));
            List<Account> account = Account.GetAccountsListGreedy(view.Categories);
            content.Add(new SlideWindow_Element_ContentInfo("Categories", string.Join(", ", view.Categories)));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            foreach (var x in account)
            {

                string admin = "";
                if (x.IsAdmin)
                {
                    admin = "(admin)";
                }

                content.Add(new SlideWindow_Element_ContentInfo(x.UserName, x.PermissionsName + " " + admin));
            }

            content.Add(new SlideWindow_Element_ContentInfo(50));
            content.Add(new SlideWindow_Element_ContentInfo("Elements", 20));
            foreach (var x in view.ElementDataArray)
            {
                content.Add(new SlideWindow_Element_ContentInfo("---" + x.ColumnType + "---", ""));
                content.Add(new SlideWindow_Element_ContentInfo("Data Columns", string.Join(",", x.Columns)));
                content.Add(new SlideWindow_Element_ContentInfo("Location", string.Join<double>(",", x.Location)));
                content.Add(new SlideWindow_Element_ContentInfo("Rotation", string.Join<double>(",", x.Rotation)));
                content.Add(new SlideWindow_Element_ContentInfo("Scale", string.Join<double>(",", x.Scale)));
                if (x.Layout != null)
                    content.Add(new SlideWindow_Element_ContentInfo("Layout (extra)", "<empty>"));
                content.Add(new SlideWindow_Element_ContentInfo(20));
                if (x is CardElementTextData t)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Font Size", t.FontSize.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Color", t.ColorHex));
                    content.Add(new SlideWindow_Element_ContentInfo("Justification", t.TextJustification));
                    content.Add(new SlideWindow_Element_ContentInfo("Italics", t.Italics.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Bold", t.Bold.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Strike Through", t.StrikeThrough.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Underline", t.Underline.ToString()));
                }
                else if (x is CardButtonElementData b)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Background Color", b.BackgroundColor));
                    content.Add(new SlideWindow_Element_ContentInfo("Font Size", b.FontSize.ToString()));
                }
                else if (x is CardGlanceElementData g)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Max Icon Count", g.MaxIconCount.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Icons Size", string.Join(",", g.IconSize)));
                    content.Add(new SlideWindow_Element_ContentInfo("Flow Direction", g.FlowDirection.ToString()));

                    content.Add(new SlideWindow_Element_ContentInfo(20));
                    content.Add(new SlideWindow_Element_ContentInfo(20));
                    var iconProp = new SlideWindow_Element_ContentInfo("Icons", "");
                    iconProp.DescriptionHeader = "Icons";
                    iconProp.DescriptionInfo = "Icons are images that pop up when a certain key word is in the excel column.";
                    content.Add(iconProp);

                    foreach (var y in g.GlanceIcons)
                    {
                        content.Add(new SlideWindow_Element_ContentInfo("Tag Id", y.TagId));
                        content.Add(new SlideWindow_Element_ContentInfo("Color", y.HexColor));
                        content.Add(new SlideWindow_Element_ContentInfo("Show if tag does exist?", y.ShowIfTagExists.ToString()));
                        content.Add(new SlideWindow_Element_ContentInfo("Path", y.IconPath));
                        content.Add(new SlideWindow_Element_ContentInfo(20));
                    }
                }
                else if (x is CardDialElementData d)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Minimum Value", d.SmallestValue.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Maximum Value", d.LargestValue.ToString()));
                    content.Add(new SlideWindow_Element_ContentInfo("Background Dial Color", d.DialBackgroundColor));
                    content.Add(new SlideWindow_Element_ContentInfo("Fallback Color", d.DialFallBackColor));
                    content.Add(new SlideWindow_Element_ContentInfo("Text Color", d.TextColor));
                    content.Add(new SlideWindow_Element_ContentInfo("Font Size", d.TextFontSize.ToString()));
                }
                else if (x is CardStickerElementData s)
                {
                    var iconProp = new SlideWindow_Element_ContentInfo("Icons", "");
                    iconProp.DescriptionHeader = "Icons";
                    iconProp.DescriptionInfo = "Icons are images that pop up when a certain key word is in the excel column.";
                    content.Add(iconProp);
                    content.Add(new SlideWindow_Element_ContentInfo(20));
                    foreach (var y in s.StickersData)
                    {
                        content.Add(new SlideWindow_Element_ContentInfo("Tag Id", y.TagId));
                        content.Add(new SlideWindow_Element_ContentInfo("Color", y.HexColor));
                        content.Add(new SlideWindow_Element_ContentInfo("Path", y.IconPath));
                        content.Add(new SlideWindow_Element_ContentInfo(20));
                    }
                }
                else if (x is CardBackgroundElementData background)
                {
                    content.Add(new SlideWindow_Element_ContentInfo("Background Color", background.ColorHex));
                    content.Add(new SlideWindow_Element_ContentInfo("Border Thickness", background.BorderThickness.ToString()));
                }

                content.Add(new SlideWindow_Element_ContentInfo(30));
            }

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        /// <summary>
        /// Set the view
        /// </summary>
        /// <param name="set">the data set to set the view to</param>
        /// <param name="viewToSet">the data set view</param>
        private void SetView(DataSet set, DataSetView viewToSet)
        {
            if (set == null)
                throw new NullReferenceException("the data set cannot be null");

            if (viewToSet == null)
                throw new NullReferenceException("the view to set cannot be null");

            //Debug.Log("dataset " + set.ID + " " + viewToSet.ID);

            if (set.TrySetView(WarSystem.CurrentActiveAccount, viewToSet.ID))
            {
                // Debug.Log("set view " + viewToSet.ViewName);
            }
            else
            {
                // Debug.Log("could not set view " + viewToSet.ViewName);
            }
        }
    }
}
