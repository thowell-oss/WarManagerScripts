using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager;

using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarManager.Unity3D
{
    public class QuickSearchUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] TMPro.TMP_InputField InputField;
        [SerializeField] private TMPro.TMP_Text ParameterContextText;
        public List<QuickActionButton> Buttons = new List<QuickActionButton>();

        public QuickSearchDataController SearchDataController;

        public bool isOn = false;

        public Transform TopLeft, TopRight, BottomLeft, BottomRight;

        public Vector2 OffScale, OnScale;
        public float TransitionSpeed;

        RectTransform rectTransform;

        public List<GameObject> items = new List<GameObject>();

        private ContentSizeFitter SizeFitter;

        private VerticalLayoutGroup layoutGroup;

        private Point LocationOfInterest = Point.zero;

        bool scaleUp = false;

        private bool canClose = false;

        public KeyboardSelectableGroup SelectableGroup;

        private bool mouseOver = false;

        private List<QuickActionData> _suggestions = new List<QuickActionData>();

        private bool _setLocationOfInterest_Auxiliary;

        void Start()
        {
            Close();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space) && ToolsManager.SelectedTool == ToolTypes.Edit)
            {
                if (!isOn)
                {
                    Open(Input.mousePosition);
                    if (!_setLocationOfInterest_Auxiliary)
                    {
                        Vector2 worldLocation = WarManagerCameraController.MainCamera.ScreenToWorldPoint(Input.mousePosition);

                        Pointf f = new Pointf(worldLocation.x, worldLocation.y);
                        LocationOfInterest = Point.WorldToGrid(f, SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID));
                    }
                }
            }

            if (isOn && (((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !mouseOver) || Input.GetKeyUp(KeyCode.Escape)))
            {
                Close();
            }


            if (InputField.text.Length == 0)
            {
                ShowSuggestions();
            }
        }

        void Open(Vector3 location)
        {

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            // Vector2 screenLocation = WarManagerCameraController.MainCamera.ScreenToWorldPoint(location);
            HandleRect(location);

            if (isOn)
                return;

            // Vector2 worldLocation = WarManagerCameraController.MainCamera.ScreenToWorldPoint(location);

            // Pointf f = new Pointf(worldLocation.x, worldLocation.y);
            // LocationOfInterest = Point.WorldToGrid(f, SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID));

            ParameterContextText.text = "";


            LeanTween.value(rectTransform.gameObject, SetBackgroundScale, rectTransform.sizeDelta, OnScale, TransitionSpeed).setEaseInOutCubic();
            LeanTween.delayedCall(TransitionSpeed, () =>
            {
                SlideWindowsManager.main.CloseWindows();
                ToolsManager.SelectedTool = ToolTypes.None;
                isOn = true;
                SetItemsActive(true);
                OnStartSearch();
            });
        }

        private void HandleRect(Vector3 position)
        {
            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            if (pivotX < .5f)
            {
                pivotX = 0f;
            }
            else
            {
                pivotX = 1f;
            }

            if (pivotY < .5f)
            {
                pivotY = 0f;
            }
            else
            {
                pivotY = 1f;
            }

            if (layoutGroup == null)
                layoutGroup = GetComponent<VerticalLayoutGroup>();


            layoutGroup.reverseArrangement = pivotY < .4f;
            scaleUp = pivotY < .4f;
            // SelectableGroup.SetInverted(pivotY < .4f);


            rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        void Close()
        {

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (!isOn) return;

            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);

            SetItemsActive(false);
            foreach (var x in Buttons)
                x.gameObject.SetActive(false);

            LeanTween.value(rectTransform.gameObject, SetBackgroundScale, rectTransform.sizeDelta, OffScale, TransitionSpeed).setEaseInOutCubic();
            isOn = false;
        }

        public void OnSearch(string str)
        {
            if (str == null || str.Trim() == string.Empty)
            {
                foreach (var x in Buttons)
                    x.gameObject.SetActive(false);
            }

            if (str == null)
                return;

            List<QuickActionData> data = SearchDataController.Search(str, LocationOfInterest, Buttons.Count, PlaceData, Close, out var searchParameters);

            ParameterContextText.text = searchParameters;
            AddButtons(data, true);
        }

        private void ShowSuggestions()
        {
            int i = 0;

            List<QuickActionData> selectedSuggestedData = new List<QuickActionData>();

            while (i < Buttons.Count && _suggestions.Count - i > 0)
            {
                var x = _suggestions[_suggestions.Count - i];
                selectedSuggestedData.Add(x);

                i++;
            }

            // if (selectedSuggestedData.Count > 0)
            //     AddButtons(selectedSuggestedData, false);
        }

        private void AddButtons(List<QuickActionData> data, bool addSuggestionData)
        {
            for (int i = 0; i < data.Count; i++)
            {
                // if (addSuggestionData)
                // {
                //     if (data[i].SuggestionData != null && !_suggestions.Contains(data[i].SuggestionData))
                //     {
                //         data[i].AddAction(() =>
                //         {
                //             _suggestions.Add(data[i].SuggestionData);
                //         });
                //     }
                // }

                Buttons[i].gameObject.SetActive(true);
                Buttons[i].SetButton(data[i]);
            }

            for (int k = data.Count; k < Buttons.Count; k++)
            {
                Buttons[k].gameObject.SetActive(false);
            }

            // Debug.Log(_suggestions.Count);
        }

        private void PlaceData(string str)
        {
            InputField.text = str;
            OnSearch(str);
            InputField.ActivateInputField();
            InputField.caretPosition = str.Length;
        }

        void OnStartSearch()
        {
            InputField.text = "";
            OnSearch("");
            InputField.ActivateInputField();
        }

        private void SetBackgroundScale(Vector2 scale)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            rectTransform.sizeDelta = scale;
        }

        private void SetItemsActive(bool active)
        {
            foreach (var x in items)
            {
                x.gameObject.SetActive(active);
            }

            if (SizeFitter == null)
                SizeFitter = GetComponent<ContentSizeFitter>();

            SizeFitter.enabled = active;
        }

        public void FindSelectableOnDown()
        {

            if (!scaleUp)
                InputField.FindSelectableOnDown().Select();
        }

        public void FindSelectableOnUp()
        {
            if (!scaleUp)
                InputField.FindSelectableOnUp().Select();
        }

        public void ToggleSetLocationOfInterest(bool toggle)
        {
            _setLocationOfInterest_Auxiliary = toggle;
        }

        public void SetLocationOfInterest(Point location)
        {
            if (_setLocationOfInterest_Auxiliary)
                LocationOfInterest = location;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseOver = false;
        }
    }
}
