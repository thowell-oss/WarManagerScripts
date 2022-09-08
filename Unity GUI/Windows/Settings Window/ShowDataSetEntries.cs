
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Unity3D.ObjectPooling;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles Showing the dataset entries
    /// </summary>
    [Notes.Author("Handles Showing the dataset entries")]
    public class ShowDataSetEntries : MonoBehaviour
    {

        /// <summary>
        /// can the user search?
        /// </summary>
        public bool CanSearch;

        /// <summary>
        /// the amount of items to add at once
        /// </summary>
        public float entryUILength = 500;

        /// <summary>
        /// the keyword to search for
        /// </summary>
        private string typedKeyWord = "";


        /// <summary>
        /// the data
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <typeparam name="DataEntry"></typeparam>
        /// <returns></returns>
        private Dictionary<int, DataEntry> _data = new Dictionary<int, DataEntry>();

        /// <summary>
        /// the integer of the last button location (in an indexed list)
        /// </summary>
        public int lastCreatedLocation = 0;

        /// <summary>
        /// the call back when a button is pressed
        /// </summary>
        private Action<DataEntry> _selectedEntryCallBack;

        /// <summary>
        /// the selected dataset
        /// </summary>
        private DataSet _selectedDataSet;

        /// <summary>
        /// the search input field
        /// </summary>
        public TMPro.TMP_InputField SearchInputField;

        /// <summary>
        /// the filter button prefab
        /// </summary>
        public Button FilterButton;

        /// <summary>
        /// the content object (parent of all button objects)
        /// </summary>
        public RectTransform SearchItemContentObj;

        /// <summary>
        /// the location anchor (at the top of the extended view panel)
        /// </summary>
        public RectTransform LocationAnchor;

        /// <summary>
        /// the button prefab
        /// </summary>
        public SearchItemButton ButtonPrefab;

        /// <summary>
        /// the list of button objects currently being used
        /// </summary>
        /// <typeparam name="SearchItemButton"></typeparam>
        /// <returns></returns>
        public List<SearchItemButton> ActivePoolObjects = new List<SearchItemButton>();

        /// <summary>
        /// how high is the button prefab?
        /// </summary>
        public int buttonPrefabHeight;

        /// <summary>
        /// the extended slide view controller reference
        /// </summary>
        private ExtendedSlideViewController controller;

        /// <summary>
        /// the main show dataset entries
        /// </summary>
        public static ShowDataSetEntries Main;

        void Awake()
        {
            Main = this;
        }

        /// <summary>
        /// start called by monobehavior
        /// </summary>
        void Start()
        {
            controller = this.GetComponent<ExtendedSlideViewController>();
        }

        /// <summary>
        /// Set the search parameters
        /// </summary>
        /// <param name="selectedDataSet">the selected dataset to get data from</param>
        /// <param name="selectedEntryCallBack">the selected entry call back</param>
        public void SetSearch(DataSet selectedDataSet, Action<DataEntry> selectedEntryCallBack)
        {

            if (controller.IsOpen)
            {
                controller.Close();

                LeanTween.delayedCall(controller.TransitionSpeed, () => { SetSearch(selectedDataSet, selectedEntryCallBack); });
                return;
            }

            _selectedDataSet = selectedDataSet;
            _selectedEntryCallBack = selectedEntryCallBack;
            controller.Open();
        }

        /// <summary>
        /// Show entries
        /// </summary>
        private void ShowEntries()
        {

            Reset();

            if (typedKeyWord == string.Empty)
            {
                CreateFullList();
            }
            else
            {
                CreateSelectedList();
            }

            float contentHeight = _data.Count * entryUILength;
            var presentationCol = _selectedDataSet.SelectedView.GetListPresentationColumn();

            CreateUIItems();
        }

        /// <summary>
        /// create the full list
        /// </summary>
        private void CreateFullList()
        {
            int id = 0;
            foreach (var x in _selectedDataSet.SearchableList)
            {
                _data.Add(id, x.Value);
                id++;
            }
        }


        /// <summary>
        /// create a selected list
        /// </summary>
        private void CreateSelectedList()
        {
            int id = 0;

            foreach (var x in _selectedDataSet.SearchableList)
            {
                var d = x.Key;

                int i = 0;
                bool found = false;
                while (i < d.Count && !found)
                {
                    if (d[i].Contains(typedKeyWord))
                    {
                        _data.Add(id, x.Value);
                        id++;
                        found = true;
                    }

                    i++;
                }
            }
        }


        /// <summary>
        /// Clear the objects from the list
        /// </summary>
        private void Reset()
        {
            lastCreatedLocation = 0;
            _data.Clear();

            foreach (var x in ActivePoolObjects)
            {
                if (x.PrefabKey == null)
                    x.PrefabKey = ButtonPrefab.gameObject;
                x.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle the ability to search for items (on or off)
        /// </summary>
        /// <param name="search">can the user search for items?</param>
        public void ToggleCanSearch(bool search)
        {
            CanSearch = search;
            FilterButton.interactable = search;
            SearchInputField.interactable = search;

            ActivePoolObjects.Clear();

            if (search)
            {
                //InitSearch(_selectedDataSet, (x) => { Debug.Log(string.Join(", ", x.GetRawData())); });
                SearchInputField.text = "";
                ShowEntries();
            }
        }

        /// <summary>
        /// Create the UI items
        /// </summary>
        private void CreateUIItems()
        {
            int i = lastCreatedLocation;
            float offset = 0;

            var column = _selectedDataSet.SelectedView.GetListPresentationColumn();

            string alphabetType = "";

            while (i < _data.Count && i - lastCreatedLocation < entryUILength)
            {
                DataEntry entryData = _data[i];
                DataValue[] values = entryData.GetAllowedDataValues();

                string str = "<empty>";

                string alphabetTypeCheck = "";

                if (values != null && values.Length > 0)
                {
                    if (values[0] != null)
                        str = values[0].ToString();

                    if (str != null && str.Length > 0)
                        alphabetTypeCheck = "" + str[0];
                }


                if (!alphabetType.StartsWith(alphabetTypeCheck) && alphabetType != string.Empty)
                {
                    offset += buttonPrefabHeight;
                }

                alphabetType = alphabetTypeCheck;

                SearchItemButton button = PoolManager.Main.CheckOutGameObject<SearchItemButton>(ButtonPrefab.gameObject, true, SearchItemContentObj);

                Vector2 pos1 = new Vector2(LocationAnchor.anchoredPosition.x, LocationAnchor.anchoredPosition.y - i * buttonPrefabHeight + offset);

                button.Init(i, str, this, pos1);
                ActivePoolObjects.Add(button);
                i++;
            }

            Vector2 pos = new Vector2(LocationAnchor.anchoredPosition.x, LocationAnchor.anchoredPosition.y + i * buttonPrefabHeight);

            lastCreatedLocation = i;
            SearchItemContentObj.sizeDelta = new Vector2(SearchItemContentObj.sizeDelta.x, pos.y);
        }


        /// <summary>
        /// Select a certain item 
        /// </summary>
        /// <param name="item">the item</param>
        public void SelectItem(int item)
        {
            _selectedEntryCallBack(_data[item]);
        }

        /// <summary>
        /// Search for entries in the dataset
        /// </summary>
        /// <param name="searchString"></param>
        public void Search(string searchString)
        {
            if (searchString == null)
                typedKeyWord = "";

            typedKeyWord = searchString.Trim();



            ShowEntries();
        }
    }
}
