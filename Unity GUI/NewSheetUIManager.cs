
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using WarManager.Unity3D.Windows;

using Sirenix.OdinInspector;

using StringUtility;

using WarManager;
using WarManager.Sharing;
using WarManager.Sharing.Security;
using WarManager.Backend;


namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles UI logic for adding sheets in War Manager
    /// </summary>
    [Notes.Author("Handles ui logic for adding sheets in War Manager")]
    public class NewSheetUIManager
    {
        private ActiveSheetsDisplayer _display;
        private string _sheetName = "New Sheet";

        private string _sheetDescription { get; set; } = "";

        private bool _formTemplate { get; set; } = false;
        private string _formTemplateString = "No";

        private double[] _sheetScale = GeneralSettings.DefaultGridScale;

        private List<DataSet> _selectedDataSets = new List<DataSet>();

        private string[] _categories = new string[0];

        private bool Advanced = false;

        private List<Permissions> _selectedPermissions = new List<Permissions>();

        private List<Account> _accounts = new List<Account>();
        private List<Account> _selectedAccounts = new List<Account>();

        #region sprites
        private Sprite cardSheetSprite => ActiveSheetsDisplayer.main.cardSheetSprite;
        private Sprite editCardSheetSprite => ActiveSheetsDisplayer.main.editCardSheetSprite;
        private Sprite DataSetSprite => ActiveSheetsDisplayer.main.DataSetSprite;
        private Sprite BackSprite => ActiveSheetsDisplayer.main.BackSprite;
        private Sprite RefreshSprite => ActiveSheetsDisplayer.main.RefreshSprite;
        private Sprite DeleteSprite => ActiveSheetsDisplayer.main.DeleteSprite;
        private Sprite AddSprite => ActiveSheetsDisplayer.main.AddSprite;
        private Sprite AddSheetSprite => ActiveSheetsDisplayer.main.AddSheetSprite;
        private Sprite MergeSprite => ActiveSheetsDisplayer.main.MergeSprite;
        private Sprite ViewSprite => ActiveSheetsDisplayer.main.ViewSprite;
        private Sprite PermissionsSprite => ActiveSheetsDisplayer.main.PermissionsSprite;
        public Sprite CodeSprite => ActiveSheetsDisplayer.main.CodeSprite;

        private Action _back;
        private string _cancelTitle;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public NewSheetUIManager()
        {

        }

        /// <summary>
        /// Open the UI for creating a new sheet
        /// </summary>
        public void SetNewSheet(Action back, string backTitle)
        {
            _categories = WarSystem.CurrentActiveAccount.FullAccessCategories;
            _selectedPermissions.Clear();
            _selectedDataSets.Clear();
            _accounts = Account.GetAccountsList();
            _sheetName = "New Sheet";
            _selectedAccounts.Clear();

            foreach (var x in _accounts)
            {
                if (x.IsAdmin || x.ContainsAllCategoriesAccessCharacter)
                {
                    _selectedAccounts.Add(x);
                }
            }

            _back = back;
            _cancelTitle = backTitle;

            NewSheet();
        }

        /// <summary>
        /// Set the UI for the new sheet
        /// </summary>
        private void NewSheet()
        {
            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();

            contentList.Add(new SlideWindow_Element_ContentInfo(_cancelTitle, -1, (x) => { CreateAndReset(true, null); _back(); }, BackSprite));

            contentList.Add(new SlideWindow_Element_ContentInfo("Create Sheet", null));

            contentList.Add(new SlideWindow_Element_ContentInfo("Name", _sheetName, (x) =>
            {
                _sheetName = x;
                NewSheet();
            }));
            contentList.Add(new SlideWindow_Element_ContentInfo("Description", _sheetDescription, (x) =>
            {
                _sheetDescription = x;
                NewSheet();
            }));

            var formTempUIElement = new SlideWindow_Element_ContentInfo("Form Template?", _formTemplateString, (x) =>
            {
                if (x == "Yes" || x == "No")
                {
                    _formTemplateString = x;

                    if (x == "Yes")
                    {
                        _formTemplate = true;
                    }
                    else if (x == "No")
                    {
                        _formTemplate = false;
                    }
                }
            });

            formTempUIElement.DescriptionHeader = "Forms and Form Templates";
            formTempUIElement.DescriptionInfo = "Form templates are used for custom inputs. If you don't know what Forms are, just leave it to \'No\'.";

            string other = "No";

            if (_formTemplateString == other)
                other = "Yes";

            formTempUIElement.ContentOptions = new List<string>() { _formTemplateString, other };
            contentList.Add(formTempUIElement);

            contentList.Add(new SlideWindow_Element_ContentInfo(50));
            contentList.Add(new SlideWindow_Element_ContentInfo("Selected Data Sets", 18));
            contentList.Add(new SlideWindow_Element_ContentInfo("Add Data Set", -1, (x) =>
                        {
                            SelectDataSets(NewSheet);
                        }, AddSprite));

            contentList.Add(new SlideWindow_Element_ContentInfo(50));
            contentList.Add(new SlideWindow_Element_ContentInfo("Permissions", 20));

            contentList.Add(new SlideWindow_Element_ContentInfo("Select Permissions", 0, (x) =>
            {
                SelectPermissions(NewSheet);
            }, PermissionsSprite));

            contentList.Add(new SlideWindow_Element_ContentInfo(20));

            #region advanced

            // string advancedStr = "Show Advanced";
            // if (Advanced)
            //     advancedStr = "Hide Advanced";

            // contentList.Add(new SlideWindow_Element_ContentInfo(advancedStr, (x) =>
            //                 {
            //                     Advanced = !Advanced;
            //                     NewSheet();
            //                 }, CodeSprite));

            // if (Advanced)
            // {
            //     contentList.Add(new SlideWindow_Element_ContentInfo(20));
            //     contentList.Add(new SlideWindow_Element_ContentInfo("Set Permissions Local (Default)", (x) =>
            //     {
            //         _categories = WarSystem.CurrentActiveAccount.FullAccessCategories;
            //         NewSheet();
            //     }, PermissionsSprite));

            //     contentList.Add(new SlideWindow_Element_ContentInfo("Set Permissions Global (So everyone can see your sheet)", (x) =>
            //     {
            //         _categories = Permissions.GetAllCategories(WarSystem.CurrentPermissions);
            //         NewSheet();
            //     }, PermissionsSprite));


            //     contentList.Add(new SlideWindow_Element_ContentInfo("Current Categories", string.Join(",", _categories), (x) =>
            //     {
            //         if (x != string.Join(",", _categories))
            //         {
            //             _categories = x.Split(',');

            //             for (int i = 0; i < _categories.Length; i++)
            //             {
            //                 _categories[i] = _categories[i].Trim();
            //             }

            //             NewSheet();
            //         }
            //     }));
            //}

            #endregion

            contentList.Add(new SlideWindow_Element_ContentInfo(50));

            contentList.Add(new SlideWindow_Element_ContentInfo("Grid Scale", 20));

            contentList.Add(new SlideWindow_Element_ContentInfo("X Axis", _sheetScale[0].ToString(), (x) =>
            {
                if (double.TryParse(x, out var dou))
                {
                    if (dou <= 0 || dou >= 10)
                    {
                        MessageBoxHandler.Print_Immediate("The value cannot be less than 0 or greater than 10", "Error");
                    }
                    else
                    {
                        _sheetScale[0] = dou;
                        NewSheet();
                    }
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Could not format the letters into a number", "Error");
                }
            }));

            contentList.Add(new SlideWindow_Element_ContentInfo("Y Axis", _sheetScale[1].ToString(), (x) =>
            {
                if (double.TryParse(x, out var dou))
                {
                    if (dou <= 0 || dou >= 10)
                    {
                        MessageBoxHandler.Print_Immediate("The value cannot be less than 0 or greater than 10", "Error");
                    }
                    else
                    {
                        _sheetScale[1] = dou;
                        NewSheet();
                    }
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Could not format the letters into a number", "Error");
                }
            }));

            contentList.Add(new SlideWindow_Element_ContentInfo(50));
            contentList.Add(new SlideWindow_Element_ContentInfo("Review", 20));
            contentList.Add(new SlideWindow_Element_ContentInfo(20));


            foreach (var set in _selectedDataSets)
            {
                contentList.Add(new SlideWindow_Element_ContentInfo(set.DatasetName, -1, (x) =>
                {
                    _selectedDataSets.Remove(set);
                    NewSheet();
                }, DeleteSprite));
            }

            if (_selectedDataSets.Count < 1)
                contentList.Add(new SlideWindow_Element_ContentInfo(" ", "No Data Sets added"));

            contentList.Add(new SlideWindow_Element_ContentInfo(20));

            var currentUser = WarSystem.CurrentActiveAccount;

            contentList.Add(new SlideWindow_Element_ContentInfo($"You - {currentUser.UserName}", $"{currentUser.FirstName} {currentUser.LastName} - {currentUser.PermissionsName}"));

            foreach (var x in _selectedAccounts)
            {
                if (x.UserName != WarSystem.CurrentActiveAccount.UserName)
                {
                    string permissionsName = "";

                    if (x.IsAdmin)
                    {
                        permissionsName = $"{x.PermissionsName} - Admin";
                    }
                    else if (x.ContainsAllCategoriesAccessCharacter)
                    {
                        permissionsName = $"{x.Permissions} - Full Access";
                    }
                    else
                    {
                        permissionsName = x.PermissionsName;
                    }

                    contentList.Add(new SlideWindow_Element_ContentInfo(x.UserName, permissionsName));
                }
            }

            contentList.Add(new SlideWindow_Element_ContentInfo(50));
            contentList.Add(new SlideWindow_Element_ContentInfo("Create Sheet", -1, (x) => { CreateAndReset(false, NewSheet); }, AddSheetSprite));
            contentList.Add(new SlideWindow_Element_ContentInfo(20));
            SlideWindowsManager.main.CloseWindows();
            SlideWindowsManager.main.AddPropertiesContent(contentList, true);
        }

        public void SelectPermissions(Action back)
        {
            var unSelectedPermissions = Permissions.GetAllPermissions();

            foreach (var perm in _selectedPermissions)
            {
                unSelectedPermissions.Remove(perm);
            }

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("", 0, (x) =>
                    {
                        back();
                    }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            content.Add(new SlideWindow_Element_ContentInfo("Selected Permissions", 20));
            content.Add(new SlideWindow_Element_ContentInfo("Deselect All Permissions", () =>
            {
                unSelectedPermissions.AddRange(_selectedPermissions);
                _selectedPermissions.Clear();
                SelectPermissions(back);

            }, null));


            content.Add(new SlideWindow_Element_ContentInfo(20));
            for (int i = 0; i < _selectedPermissions.Count; i++)
            {
                content.Add(new SlideWindow_Element_ContentInfo(_selectedPermissions[i].Name, i, (x) =>
                    {
                        _selectedPermissions.RemoveAt(x);
                        SelectPermissions(back);
                    }, DeleteSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Unselected Permissions", 20));
            if (unSelectedPermissions.Count > 0)
                content.Add(new SlideWindow_Element_ContentInfo("Select All Permissions", () =>
                {

                    _selectedPermissions.AddRange(unSelectedPermissions);
                    unSelectedPermissions.Clear();
                    SelectPermissions(back);

                }, null));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            for (int i = 0; i < unSelectedPermissions.Count; i++)
            {
                content.Add(new SlideWindow_Element_ContentInfo(unSelectedPermissions[i].Name, i, (x) =>
                   {
                       _selectedPermissions.Add(unSelectedPermissions[x]);
                       SelectPermissions(back);
                   }, AddSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(40));
            content.Add(new SlideWindow_Element_ContentInfo("Users that can see your sheet", 20));

            var currentUser = WarSystem.CurrentActiveAccount;

            content.Add(new SlideWindow_Element_ContentInfo($"You - {currentUser.UserName}", $"{currentUser.FirstName} {currentUser.LastName} - {currentUser.PermissionsName}"));

            _selectedAccounts.Clear();

            foreach (var x in _accounts)
            {
                bool foundAccount = false;

                if (x.IsAdmin || x.ContainsAllCategoriesAccessCharacter)
                {
                    foundAccount = true;
                }
                else
                {
                    for (int i = 0; i < x.Permissions.PermissionCategories.Length; i++)
                    {
                        for (int j = 0; j < _selectedPermissions.Count; j++)
                        {
                            if (_selectedPermissions[j].PermissionCategories.Contains(x.Permissions.PermissionCategories[i]))
                                foundAccount = true;
                        }
                    }
                }

                if (foundAccount && x.UserName != currentUser.UserName)
                {
                    string permissionsName = "";

                    if (x.IsAdmin)
                    {
                        permissionsName = $"{x.PermissionsName} - Admin";
                    }
                    else if (x.ContainsAllCategoriesAccessCharacter)
                    {
                        permissionsName = $"{x.Permissions} - Full Access";
                    }
                    else
                    {
                        permissionsName = x.PermissionsName;
                    }

                    content.Add(new SlideWindow_Element_ContentInfo(x.UserName, permissionsName));
                    _selectedAccounts.Add(x);
                }
            }

            SlideWindowsManager.main.AddPropertiesContent(content);
        }

        public void SelectDataSets(Action back)
        {
            List<DataSet> unSelectedDataSets = new List<DataSet>();

            foreach (var x in WarSystem.DataSetManager.Datasets)
            {
                if (!_selectedDataSets.Contains(x))
                    unSelectedDataSets.Add(x);
            }


            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("", 0, (x) =>
                    {
                        back();
                    }, BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Data Sets", null));
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Selected Data Sets", 20));

            foreach (var x in _selectedDataSets)
            {
                content.Add(new SlideWindow_Element_ContentInfo(x.DatasetName, 0, (z) =>
                   {
                       _selectedDataSets.Remove(x);
                       SelectDataSets(back);
                   }, DeleteSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Unselected Data Sets", 20));

            foreach (var x in unSelectedDataSets)
            {
                content.Add(new SlideWindow_Element_ContentInfo(x.DatasetName, 0, (z) =>
                {
                    _selectedDataSets.Add(x);
                    SelectDataSets(back);
                }, AddSprite));
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));

            SlideWindowsManager.main.AddPropertiesContent(content);
        }

        /// <summary>
        /// Selecting accounts that the new sheet will be shared with
        /// </summary>
        /// <param name="back"></param>
        public void SelectUserAccountsToShare(Action back)
        {
            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();
            contentList.Add(new SlideWindow_Element_ContentInfo(20));
            contentList.Add(new SlideWindow_Element_ContentInfo("Create Sheet", -1, (x) => { back(); }, BackSprite));
        }


        private void CreateAndReset(bool cancel, Action back)
        {
            if (!cancel)
            {
                if (_sheetName == "New Sheet")
                {

                    MessageBoxHandler.Print_Immediate("Are you sure you want your sheet to be named the default: \'" + _sheetName + "\'", "Question", (x) =>
                    {
                        if (x)
                        {
                            FinalPush(back);
                        }
                        else
                        {
                            back();
                        }
                    });
                }
                else
                {
                    FinalPush(back);
                }
            }

            SlideWindowsManager.main.CloseWindows();
            SlideWindowsManager.main.ClearProperties();
        }

        private void FinalPush(Action back)
        {
            MessageBoxHandler.Print_Immediate($"Create \'{_sheetName}\'?", "Final Wrap Up", (x) =>
                {
                    if (x)
                    {
                        List<string> dataSetIds = new List<string>();

                        foreach (var set in _selectedDataSets)
                        {
                            dataSetIds.Add(set.ID);
                        }

                        SheetsManager.NewCardSheet(_sheetName, _categories, _sheetScale, dataSetIds.ToArray());
                        WarSystem.DeveloperPushNotificationHandler.CreatedSheets = true;
                    }
                    else
                    {
                        back();
                    }
                });
        }

        /// <summary>
        /// Select a dataset by filtering out sets
        /// </summary>
        /// <param name="filterOutSets"></param>
        private void SelectDataSet(List<DataSet> filterOutSets)
        {
            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();
            contentList.Add(new SlideWindow_Element_ContentInfo("Back", -1, (x) => { NewSheet(); }, BackSprite));
            contentList.Add(new SlideWindow_Element_ContentInfo("Add Data Set", null));

            foreach (var dataset in WarSystem.DataSetManager.Datasets)
            {
                if (filterOutSets.Find((x) => x.ID == dataset.ID) == null)
                {
                    contentList.Add(new SlideWindow_Element_ContentInfo(dataset.DatasetName, -1, (x) =>
                    {
                        _selectedDataSets.Add(dataset);
                        NewSheet();
                    }, AddSprite));
                }
            }

            SlideWindowsManager.main.CloseWindows();
            SlideWindowsManager.main.AddPropertiesContent(contentList, true);
        }
    }
}
