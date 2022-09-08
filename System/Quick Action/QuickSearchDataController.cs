
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StringUtility;

using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Unity3D.Windows;
using WarManager.Sharing;

namespace WarManager
{
    /// <summary>
    /// quick search controller gathers all data to be presented to the quick search menu
    /// </summary>
    [Notes.Author("Quick search controller gathers all data to be presented to the quick search menu")]
    public class QuickSearchDataController : MonoBehaviour
    {
        /// <summary>
        /// The data set icon 
        /// </summary>
        public Sprite DatasetIcon;

        /// <summary>
        /// data set color
        /// </summary>
        public Color DataSetColor;

        /// <summary>
        /// Data Entry Icon
        /// </summary>
        public Sprite DataEntryIcon;

        /// <summary>
        /// Data entry color
        /// </summary>
        public Color DataEntryColor;

        /// <summary>
        /// The command icon
        /// </summary>
        public Sprite CommandIcon;

        /// <summary>
        /// Command color
        /// </summary>
        public Color CommandColor;

        /// <summary>
        /// The command icon
        /// </summary>
        public Sprite ContextCommandIcon;

        /// <summary>
        /// Command color
        /// </summary>
        public Color ContextCommandColor;

        public Sprite ActionCardIcon;
        public Color ActionCardColor;

        /// <summary>
        /// The threshold at which items appear in the list
        /// </summary>
        private double _appearanceThreshold = .3;

        public NotificationsUIController tasksAndMessagesUI;
        public SimpleUndoRedoManager undoRedoManager;

        /// <summary>
        /// the collected search data
        /// </summary>
        /// <typeparam name="QuickSearchData"></typeparam>
        /// <returns></returns>
        private Dictionary<DataSet, QuickSearchData> _searchData = new Dictionary<DataSet, QuickSearchData>();
        private QuickSearchData _actionCardData;

        private List<(string name, string description, Action primaryAction)> _commands = new List<(string name, string description, Action primaryAction)>();
        private List<(string name, string description, Action<Point> primaryAction)> _contextCommands = new List<(string name, string description, Action<Point> primaryAction)>();

        void Start()
        {
            SetCommands();
        }

        public void Init()
        {

            _searchData.Clear();

            foreach (var x in WarSystem.DataSetManager.Datasets)
            {
                _searchData.Add(x, new QuickSearchData(x.DatasetName, x.Color, x.SearchableList));
            }

            _actionCardData = WarSystem.DataSetManager.ActionCardsManager.GetActionSearchData();
        }

        /// <summary>
        /// Search War Manger for items with a keyword
        /// </summary>
        /// <param name="str">the string</param>
        /// <param name="returnLimit">the limit of items to return (0 == all)</param>
        /// <returns>returns a string name, with the action call back, a list of secondary actions, an icon of the type of primary action</returns>
        public List<QuickActionData> Search(string str, Point locationOfInterest, int returnLimit, Action<string> placeDataAction, Action closeAction, out string searchType)
        {

            List<QuickActionData> quickActionData = new List<QuickActionData>();
            List<(double score, DataEntry entry, int colLocation)> foundDataEntries = new List<(double score, DataEntry entry, int colLocation)>();
            List<(double score, DataEntry entry)> foundActionCards = new List<(double score, DataEntry entry)>();
            List<(double, DataSet)> foundDataSets = new List<(double, DataSet)>();
            List<(double score, string name, string description, Action<Point> action)> foundCommands = new List<(double score, string name, string description, Action<Point> action)>();

            if (IsCertainDataSet(str, WarSystem.DataSetManager.GetDataSets(new List<DataSet>()), out string dataSetName, out string searchString))
            {
                if (dataSetName != null && dataSetName.Length > 0 && dataSetName != string.Empty && WarSystem.DataSetManager.TryGetDataSetByName_CaseInsensitive(dataSetName, out var set))
                {
                    var searchData = new QuickSearchData(set.DatasetName, set.Color, set.SearchableList);
                    foundDataEntries = SearchEntries(searchString, returnLimit, new List<QuickSearchData>() { searchData });

                    searchType = dataSetName + " ONLY";
                }
                else
                {
                    foundDataSets = GetDataSets(searchString, returnLimit);

                    searchType = "All Data Sets";
                }
            }
            else if (IsCommand(searchString, out var commandSearchResult))
            {

                foundCommands = SearchCommands(commandSearchResult, returnLimit);

                searchType = "Commands ONLY";
            }
            else
            {
                foundDataSets = GetDataSets(searchString, returnLimit);
                foundDataEntries = GetEntriesFromSheet(str, returnLimit);
                foundDataEntries.AddRange(SearchEntries(searchString, returnLimit, new List<QuickSearchData>() { _actionCardData }));
                foundCommands = SearchCommands(searchString, returnLimit);
                searchType = "Entries in Sheet, All Data Sets, Commands";
            }

            if (str.Trim() == string.Empty)
            {
                searchType = "options  'd:DataSet'  'c:Command'";
            }

            foreach (var x in foundDataEntries)
            {

                string dataName = "";

                bool systemCard = false;

                if (x.colLocation >= 0)
                {
                    dataName = x.entry.GetAllowedDataValues()[x.colLocation].Value.ToString();
                }
                else if (x.entry.Actor != null)
                {
                    dataName = x.entry.Actor.Name;
                    systemCard = true;
                    //Debug.Log("getting entry name");
                }

                if (dataName.Trim() == string.Empty)
                {
                    dataName = "<empty>";
                }

                Action dropCardAction = () =>
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        CardUtility.TryDropCard(sheet, locationOfInterest, sheet.CurrentLayer, x.entry, out var id);
                    }

                    closeAction();
                };

                string dataSetSource = " (" + x.entry.DataSet.DatasetName.SetStringQuotes() + " )";

                Sprite icon = DataEntryIcon;
                Color col = DataEntryColor;

                if (systemCard)
                {
                    dataSetSource = "";
                    icon = ActionCardIcon;
                    col = ActionCardColor;
                }

                var newData = new QuickActionData(x.score, dataName + dataSetSource,
                 string.Join(", ", x.entry.GetAllowedValues_PrettyPrint()), dropCardAction, icon, col, new List<(string name, Action action, bool enabled)>());

                var dataSet = foundDataSets.Find(y => y.Item2.ID == x.entry.DataSet.ID);

                if (dataSet.Item2 != null)
                {
                    var data = new QuickActionData(dataSet.Item1, dataSet.Item2.DatasetName, string.Join(", ", dataSet.Item2.AllowedTags), () => { placeDataAction($"d:{dataSet.Item2.DatasetName} "); }, DatasetIcon, DataSetColor,
                 new List<(string name, Action action, bool enabled)>());

                    newData.SuggestionData = data;
                }

                quickActionData.Add(newData);
            }

            foreach (var x in foundDataSets)
            {
                string inSheet = "(not in sheet)";

                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    if (sheet.ContainsDataSet(x.Item2.ID))
                    {
                        inSheet = "(in sheet)";
                    }
                }

                quickActionData.Add(new QuickActionData(x.Item1, x.Item2.DatasetName + " " + inSheet, x.Item2.SelectedView.ViewName + "\n" + string.Join(", ", x.Item2.AllowedTags), () => { placeDataAction($"d:{x.Item2.DatasetName} "); }, DatasetIcon, DataSetColor,
                 new List<(string name, Action action, bool enabled)>()));
            }

            foreach (var x in foundCommands)
            {
                var newCommand = new QuickActionData(x.score, x.name, x.description, () => { x.action(locationOfInterest); closeAction(); }, CommandIcon, CommandColor, new List<(string name, Action action, bool enabled)>());
                newCommand.SuggestionData = newCommand;
                quickActionData.Add(newCommand);
            }

            quickActionData.Sort((a, b) =>
            {
                return -a.Score.CompareTo(b.Score);
            });

            List<QuickActionData> result = new List<QuickActionData>();

            int k = 0;
            while (k < quickActionData.Count && k < returnLimit)
            {
                result.Add(quickActionData[k]);
                k++;
            }

            return result;
        }

        private bool IsCertainDataSet(string search, List<DataSet> dataSets, out string dataSetName, out string correctedSearch)
        {


            if (search.ToLower().StartsWith("d:"))
            {
                search = search.Remove(0, 2);
                search = search.Trim();

                correctedSearch = FindSearchStr(dataSets, search, out var dataSet);

                // if (dataset.StartsWith(" "))
                //     dataset = dataset.Remove(0, 1);

                // dataSetName = dataset;
                // correctedSearch = search.Remove(0, dataset.Length);

                // if (correctedSearch == null)
                //     correctedSearch = "";

                // Debug.Log("found correct d: " + "\'" + dataSet + "\'" + " \'" + correctedSearch + "\'");

                dataSetName = dataSet;

                return true;
            }

            dataSetName = "";
            correctedSearch = search;
            return false;
        }


        private bool IsCommand(string search, out string result)
        {
            if (search.ToLower().StartsWith("c:"))
            {
                result = search.Remove(0, 2);
                return true;
            }

            result = search;
            return false;
        }

        /// <summary>
        /// Search commands
        /// </summary>
        /// <param name="name"></param>
        /// <param name="str"></param>
        /// <param name="returnLimit"></param>
        /// <returns></returns>
        private List<(double score, string description, string name, Action<Point> action)> SearchCommands(string str, int returnLimit)
        {

            List<(double percent, string name, string description, Action<Point> action)> data = new List<(double percent, string name, string description, Action<Point> action)>();


            foreach (var x in _commands)
            {

                var chance = x.name.CalculateSimilarity(str);

                if (chance >= _appearanceThreshold / 2)
                {
                    data.Add((chance, x.name, x.description, (y) => { x.primaryAction(); }));
                }
            }

            foreach (var x in _contextCommands)
            {
                var chance = x.name.CalculateSimilarity(str);

                if (chance >= _appearanceThreshold / 2)
                {
                    data.Add((chance, x.name, x.description, (y) => { x.primaryAction(y); }));
                }
            }

            data.Sort((a, b) =>
            {
                return -a.percent.CompareTo(b.percent);
            });

            List<(double score, string name, string description, Action<Point> action)> result = new List<(double score, string name, string description, Action<Point> action)>();

            int i = 0;
            while (i < data.Count && i < returnLimit)
            {
                result.Add(data[i]);
                i++;
            }

            return result;
        }

        private string FindSearchStr(List<DataSet> dataSets, string search, out string proposedDataSetString)
        {
            string resultSetName = "";
            double best = 0;

            search = search.ToLower();

            foreach (var x in dataSets)
            {

                double value = search.CalculateSimilarity(x.DatasetName);


                if (search.Trim().StartsWith(x.DatasetName.ToLower().Trim()) && best < value)
                {
                    best = value;
                    resultSetName = x.DatasetName;
                }
            }

            proposedDataSetString = resultSetName;

            return search.Remove(0, resultSetName.Length);
        }

        /// <summary>
        /// Get data from the datasets of the sheet
        /// </summary>
        /// <param name="str">the search string</param>
        /// <param name="returnLimit">how many items do you want returned?</param>
        /// <returns>returns the list</returns>
        private List<(double, DataEntry, int colToSearch)> GetEntriesFromSheet(string str, int returnLimit)
        {
            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var sets = SheetsManager.GetDataSets(sheet);
                List<QuickSearchData> data = new List<QuickSearchData>();
                for (int i = 0; i < sets.Count; i++)
                {
                    if (_searchData.TryGetValue(sets[i], out var quickSearchData))
                    {
                        data.Add(quickSearchData);
                    }
                }

                return SearchEntries(str, returnLimit, data);
            }

            return new List<(double, DataEntry, int)>();
        }

        /// <summary>
        /// Search for items in the data entries
        /// </summary>
        /// <param name="str">the search string</param>
        /// <param name="returnLimit">the return limit</param>
        /// <param name="dataToSearch">the quick search data to search through</param>
        /// <returns>returns a list of entries</returns>
        private List<(double, DataEntry, int)> SearchEntries(string str, int returnLimit, List<QuickSearchData> dataToSearch)
        {
            SortedList<double, (DataEntry, int colToSearch)> data = new SortedList<double, (DataEntry, int)>();

            foreach (var x in dataToSearch)
            {
                var dict = x.GetTopEntries(str);

                foreach ((double percentResult, DataEntry entry, int colToSearch) y in dict)
                {
                    if (y.percentResult >= _appearanceThreshold)
                    {

                        double result = y.percentResult;
                        while (data.ContainsKey(result))
                        {
                            result -= .0001;
                        }

                        data.Add(result, (y.entry, y.colToSearch));

                        // Debug.Log(result + " " + y.entry.GetAllowedValues()[0].ToString());
                    }
                }
            }

            List<(double, DataEntry, int)> entries = new List<(double, DataEntry, int)>();

            int i = 0;

            var listOfData = data.ToList();

            listOfData.Reverse();

            if (returnLimit > 0)
            {
                foreach (var x in listOfData)
                {
                    if (entries.Count < returnLimit)
                    {
                        entries.Add((x.Key, x.Value.Item1, x.Value.Item2));
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                foreach (var x in listOfData)
                {
                    entries.Add((x.Key, x.Value.Item1, x.Value.Item2));
                }
            }

            return entries;
        }

        /// <summary>
        /// Get the datasets
        /// </summary>
        /// <param name="search">the search string</param>
        /// <returns>returns a list of datasets</returns>
        private List<(double, DataSet)> GetDataSets(string search, int returnLimit)
        {
            List<(double, DataSet)> data = new List<(double, DataSet)>();

            int lim = 0;

            foreach (var x in WarSystem.DataSetManager.Datasets)
            {
                if (lim >= returnLimit)
                    break;

                double result = search.Trim().CalculateSimilarity(x.DatasetName.Trim());
                if (result >= _appearanceThreshold / 2)
                {
                    data.Add((result, x));
                    lim++;
                }
            }

            return data;
        }

        private void ChangeSheets(string id)
        {
            Init();
            SetCommands();
        }

        /// <summary>
        /// Set the commands for the quick search bar
        /// </summary>
        private void SetCommands()
        {
            _commands.Clear();
            _contextCommands.Clear();

            _commands.Add(("New Sheet", "Create and open a new sheet", () => { WarManager.Unity3D.ActiveSheetsDisplayer.main.NewSheet(); }));
            _commands.Add(("Quit", "Quit War Manager", () => { WarSystem.AttemptQuit(); }));
            _commands.Add(("View Home", "View the home sheet", () => { SheetsManager.SetSheetCurrent(SheetsManager.HomeSheetID); }));
            _commands.Add(("References...", "", () => { WarManager.Unity3D.ActiveSheetsDisplayer.main.ViewReferences(); SlideWindowsManager.main.OpenReference(true); }));
            _commands.Add(("Get Link...", $"Get a mobile friendly link of all the selected cards to share with others", () =>
            {
                ExportOptionsManager optionsManager = new ExportOptionsManager();
                optionsManager.GetLink(optionsManager.MainMenuBackAction(), "Back");
            }
            ));
            _commands.Add(("Refresh", "Reload the data sets from the server and refresh the active sheet.", () =>
            {
                WarSystem.RefreshLoadedDataSets();
                SheetsManager.ReloadCurrentSheet();
            }
            ));

            _commands.Add(("Settings...", "View your account settings", () =>
            {
                ActiveSheetsDisplayer.main.ShowAccountInfo(true);
            }
            ));

            _commands.Add(("New Task...", "Create a new task in the Tasks and Messages Panel", () =>
            {
                tasksAndMessagesUI.CreateMessage(() => { tasksAndMessagesUI.ShowAllNotifications(); }, new UserNotification()
                {
                    From = WarSystem.CurrentActiveAccount.UserName,
                    Title = "New Message",
                    IsTaskMessage = true,
                });
            }
            ));

            _commands.Add(("New Message...", "Create a new message in the Tasks and Messages Panel", () =>
            {
                tasksAndMessagesUI.CreateMessage(() => { tasksAndMessagesUI.ShowAllNotifications(); }, new UserNotification()
                {
                    From = WarSystem.CurrentActiveAccount.UserName,
                    Title = "New Message",
                    IsTaskMessage = false,
                });
            }
            ));

            _commands.Add(("Undo", "Undo an action in the sheet", () =>
            {
                if (undoRedoManager.UndoCount > 0)
                    undoRedoManager.Undo();
            }
            ));

            _commands.Add(("Redo", "Redo an action in the sheet", () =>
            {
                if (undoRedoManager.RedoCount > 0)
                    undoRedoManager.Redo();
            }
            ));

            if (WarSystem.DataSetManager != null && WarSystem.DataSetManager.Datasets != null)
            {
                foreach (var x in WarSystem.DataSetManager.Datasets)
                {
                    _commands.Add((x.DatasetName.SetStringQuotes() + " properties", $"view all entries in {x.DatasetName}", () =>
                    {
                        DataSetViewer.main.ShowDataSetProperties(() => { ActiveSheetsDisplayer.main.ViewReferences(); }, "References", x);
                    }
                    ));
                }

                foreach (var x in WarSystem.DataSetManager.Datasets)
                {
                    _contextCommands.Add(("New " + x.DatasetName.SetStringQuotes(), $"create a new entry and add the card to the sheet {x.DatasetName}", (y) =>
                    {
                        DataSetViewer.main.QuickDropCard(SheetsManager.CurrentSheetID, y, x);
                    }
                    ));
                }
            }

            _contextCommands.Add(("Set Default Drop Point", $"Set this location as the default drop point for all cards", (y) =>
                {
                    SheetDropPointManager.SetDropPoint(y, SheetsManager.CurrentSheetID);
                }
            ));

            foreach (var x in SheetsManager.Sheets)
            {
                if (!SheetsManager.IsSheetCurrent(x.ID) && x.ID != SheetsManager.CurrentSheetID)
                {
                    _commands.Add(($"View {x.Name.SetStringQuotes()} sheet", "View the cached sheet that is already opened.", () => { SheetsManager.SetSheetCurrent(x.ID); }));
                }
            }

            var closedSheets = WarSystem.CurrentSheetsManifest.GetClosedSheets(WarSystem.CurrentActiveAccount);

            foreach (var x in closedSheets)
            {
                _commands.Add(($"Open {x.SheetName.SetStringQuotes()} sheet", "Download the sheet from the cloud view it", () => { SheetsManager.OpenCardSheet(x.SheetPath, SheetsManager.SystemEncryptKey, out var id); }));
            }

        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += ChangeSheets;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= ChangeSheets;
        }
    }
}
