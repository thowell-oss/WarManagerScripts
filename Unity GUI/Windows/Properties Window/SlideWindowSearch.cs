using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace WarManager.Unity3D.Windows
{
    [RequireComponent(typeof(SlideWindowController))]
    public class SlideWindowSearch : MonoBehaviour
    {
        public Dictionary<string[], ISlideWindow_Element> SearchableElements = new Dictionary<string[], ISlideWindow_Element>();

        public string SearchString = "";
        public List<string> FilterKeywords = new List<string>();

        public RectTransform SearchBar;

        public TMP_InputField SearchInputField;

        public bool SearchBarOpen = false;
        public bool PanelActive = false;

        public RectTransform FilterContainer;
        public TMP_InputField FilterKeywordPrefab;
        public GameObject FilterButtons;
        public List<TMP_InputField> FilterInputFields = new List<TMP_InputField>();

        public List<int> SystemKeywordLocations = new List<int>();

        SlideWindowController windowController;

        bool interacted = false;

        public bool AllowSystemFilterFlyoutOnLoading = true;

        /// <summary>
        /// Turn the search bar on or off, if the search bar is on, turn the text inputfield on as well
        /// </summary>
        /// <param name="active"></param>
        public void SetSearchPanelActive(bool active)
        {

            SearchBarOpen = active;

            if (active)
            {
                SearchBar.gameObject.SetActive(active);
                LeanTween.cancel(SearchBar.gameObject);
                SearchBar.LeanSetLocalPosX(476);
                LeanTween.moveLocalX(SearchBar.gameObject, 4, .125f).setEaseInOutQuad();

                LeanTween.delayedCall(.125f, () =>
                {
                    SearchInputField.ActivateInputField();
                });
            }
            else
            {
                LeanTween.cancel(SearchBar.gameObject);
                SearchInputField.DeactivateInputField();

                float delayTime = 0;

                if (FilterContainer.gameObject.activeSelf)
                {
                    DeactivateFilterPanel();
                    delayTime = .15f;
                }

                LeanTween.delayedCall(delayTime, () =>
                {
                    LeanTween.delayedCall(.125f, () => { SearchBar.gameObject.SetActive(false); });

                    SearchBar.LeanSetLocalPosX(4);
                    LeanTween.moveLocalX(SearchBar.gameObject, 476, .125f).setEaseInOutQuad();
                });
            }

            interacted = true;
        }

        public void ToggleFilterPanelActive()
        {
            bool active = !FilterContainer.gameObject.activeSelf;
            SetFilterPanelActive(active);
        }

        public void UpdateSearchValue()
        {
            SearchString = SearchInputField.text;
            OnValuesChanged();
        }

        public void SetFilterPanelActive(bool active)
        {
            if (active)
            {
                ActivateFilterPanel();
            }
            else
            {
                DeactivateFilterPanel();
            }
        }

        public void ActivateFilterPanel()
        {
            LeanTween.cancel(FilterContainer.gameObject);
            FilterContainer.gameObject.SetActive(true);

            FilterContainer.LeanSetLocalPosY(10);
            LeanTween.moveLocalY(FilterContainer.gameObject, -5f, .125f).setEaseInExpo();

            interacted = true;
        }

        public void DeactivateFilterPanel()
        {
            for (int i = 0; i < FilterInputFields.Count; i++)
            {
                FilterInputFields[i].DeactivateInputField();
            }

            LeanTween.cancel(FilterContainer.gameObject);
            LeanTween.delayedCall(.125f, () => { FilterContainer.gameObject.SetActive(false); });

            FilterContainer.LeanSetLocalPosY(-5f);
            LeanTween.moveLocalY(FilterContainer.gameObject, 10, .125f).setEaseOutExpo();

            interacted = true;
        }

        public void AddFilterKeyword()
        {
            var keyword = Instantiate<TMP_InputField>(FilterKeywordPrefab, FilterContainer.transform);

            RemoveIndividualKeywordHandler removeIndividualKeywordHandler = keyword.GetComponent<RemoveIndividualKeywordHandler>();
            removeIndividualKeywordHandler.index = FilterInputFields.Count;
            removeIndividualKeywordHandler.SearchController = this;

            FilterInputFields.Add(keyword);
            keyword.ActivateInputField();
            keyword.onValueChanged.AddListener(delegate { OnValuesChanged(); });

            FilterKeywords.Add("");

            FilterButtons.transform.SetAsLastSibling();

            interacted = true;
        }

        public void RemoveFilterKeyword(int keyword, bool refreshFilter = true)
        {
            Destroy(FilterInputFields[keyword].gameObject);
            FilterInputFields.RemoveAt(keyword);

            for (int i = 0; i < SystemKeywordLocations.Count; i++)
            {
                if (SystemKeywordLocations[i] == keyword)
                {
                    SystemKeywordLocations.RemoveAt(i);
                    break;
                }
            }

            FilterKeywords.RemoveAt(keyword);

            for (int i = keyword; i < FilterInputFields.Count; i++)
            {
                RemoveIndividualKeywordHandler removeIndividualKeywordHandler = FilterInputFields[i].GetComponent<RemoveIndividualKeywordHandler>();
                removeIndividualKeywordHandler.index--;
            }

            for (int i = 0; i < SystemKeywordLocations.Count; i++)
            {
                if (SystemKeywordLocations[i] >= keyword)
                {
                    SystemKeywordLocations[i]--;
                }
            }

            if (refreshFilter)
            {
                OnValuesChanged();
            }

            interacted = true;
        }

        /// <summary>
        /// programmatically add filter keywords
        /// </summary>
        /// <param name="values"></param>
        public void AddFilterKeywords(string[] values, bool addToSystemKeywords = true)
        {
            RemoveAllSystemKeywords();

            SetSearchPanelActive(true);
            ActivateFilterPanel();

            int addedKeywords = 0;
            if (FilterInputFields.Count > 0)
            {
                addedKeywords = FilterInputFields.Count - 1;
            }

            for (int i = 0; i < values.Length; i++)
            {
                AddFilterKeyword();

                FilterInputFields[i + addedKeywords].text = values[i];

                if (addToSystemKeywords)
                {
                    SystemKeywordLocations.Add(i + addedKeywords);
                }
            }

            interacted = false;

            if (!interacted)
            {
                float time = 3;

                if (!AllowSystemFilterFlyoutOnLoading)
                    time = .5f;

                LeanTween.delayedCall(time, () =>
                {
                    DeactivateFilterPanel();
                });
            }
        }

        /// <summary>
        /// Removes all keywords added by War Manager
        /// </summary>
        public void RemoveAllSystemKeywords()
        {
            if (SystemKeywordLocations.Count < 1)
                return;

            for (int i = SystemKeywordLocations.Count - 1; i >= 0; i--)
            {
                RemoveFilterKeyword(SystemKeywordLocations[i], false);
            }

            SystemKeywordLocations = new List<int>();

            OnValuesChanged();
        }

        public void RemoveAllFilterKeywords()
        {
            interacted = true;

            for (int i = FilterInputFields.Count - 1; i >= 0; i--)
            {
                RemoveFilterKeyword(i, false);
            }

            OnValuesChanged();
        }


        public void OnValuesChanged()
        {
            if (windowController == null)
                windowController = GetComponent<SlideWindowController>();

            windowController.OnSearchValuesChanged();
        }

        public void Update()
        {
            if (!PanelActive)
                return;


            if (Input.GetKey(KeyCode.Tab))
            {
                if (!SearchBarOpen)
                    SetSearchPanelActive(true);
            }
        }


        /// <summary>
        /// Convert csv a csv filter string into an array of filter strings
        /// </summary>
        /// <param name="csvString">the string to use to filter keywords</param>
        public void AddFilterKeywordsFromCSVString(string csvString)
        {
            if (csvString != null && csvString != string.Empty)
            {
                if (csvString.Contains("\""))
                {
                    //string regexString = "(\"(.?)+\")";
                    string splitString = ",";

                    string[] strArray = Regex.Split(csvString, splitString, RegexOptions.IgnoreCase);

                    //Debug.Log(string.Join("|", strArray));

                    List<string> filterOutList = new List<string>();

                    foreach (string str in strArray)
                    {
                        string final = str;


                        if (final.Length >= 3)
                        {
                            final = final.Trim();
                            final = final.Remove(0, 1);
                            final = final.Remove(final.Length - 1, 1);
                            final = final.ToLower();
                        }
                        else
                        {
                            final = "";
                        }

                        // Debug.Log("filter: \'" + final + "\'");

                        filterOutList.Add(final);
                    }
                    AddFilterKeywords(filterOutList.ToArray());
                }
                else
                {
                    if (csvString.Length > 0)
                    {
                        AddFilterKeywords(new string[1] { csvString });
                    }
                }
            }

        }
    }
}

