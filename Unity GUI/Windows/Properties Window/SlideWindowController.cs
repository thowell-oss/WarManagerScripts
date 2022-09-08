/* PropertiesWindowController.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace WarManager.Unity3D.Windows
{
    [RequireComponent(typeof(RectTransform))]
    public class SlideWindowController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        RectTransform window;

        public float CloseLocation, OpenLocation, time;
        public RectTransform CloseIcon;
        public RectTransform TabTypeIcon;
        public RectTransform UserNudgeIcon;

        public Image ActiveIndicator;
        public Color ActiveColor = Color.red;
        public Color PassiveColor = Color.clear;

        public Image SearchBarImage;
        public Color SearchColor;
        public Color FilterColor;

        public Sprite FilterSprite;
        public Sprite SearchSprite;

        public Image SearchFilterButtonImage;

        public NotificationButtonManager NotificationButtonManager;

        public Scrollbar VerticalScrollBar;

        private bool _closed = true;

        public bool isReferenceSlideWindow = false;
        private bool start = false;

        public SlideWindowSearch searchController;

        public bool Closed
        {
            get
            {
                return _closed;
            }
            set
            {
                if (_closed != value)
                {
                    _closed = value;
                    updateToggle = true;
                }
            }
        }
        public bool loading;

        public GameObject LoadingObject;
        public GameObject InfomationObject;

        private bool upColor;

        private bool updateOnOpen;

        [Space]
        public bool Test;
        public Transform ContentViewportParent;

        [Header("Settings")]
        public bool CanForcePaneOpen = true;
        public bool UseNudge = true;
        public bool IgnoreGeneralSettingsForcePane;

        [Header("Content Display Types")]
        public GameObject LabelPrefab;
        public GameObject HeaderPrefab;
        public GameObject SpacerPrefab;
        public GameObject ButtonPrefab;
        public GameObject ParagraphPrefab;
        public GameObject CardRepPrefab;
        public GameObject EditLabelPrefab;
        public GameObject SheetElementPrefab;

        [Header("Active Content Elements")]
        public List<ISlideWindow_Element> ActiveObjects = new List<ISlideWindow_Element>();

        #region deactivated items

        public Transform UIObjectPool;

        public Queue<SlideWindow_Element_Label> DeactivatedLabels = new Queue<SlideWindow_Element_Label>();
        public Queue<SlideWindow_Element_Header> DeactivatedHeaders = new Queue<SlideWindow_Element_Header>();
        public Queue<SlideWindow_Element_Spacer> DeactivatedSpacers = new Queue<SlideWindow_Element_Spacer>();
        public Queue<SlideWindowElement_Button> DeactivatedButtons = new Queue<SlideWindowElement_Button>();
        public Queue<SlideWindow_Element_Label> DeactivatedParagraphs = new Queue<SlideWindow_Element_Label>();
        public Queue<SlideWindow_Element_CardRep> DeactivatedCards = new Queue<SlideWindow_Element_CardRep>();
        public Queue<SlideWindow_Element_SheetElement> DeactivatedSheetElements = new Queue<SlideWindow_Element_SheetElement>();
        public Queue<SlideWindow_Element_EditableLabel> DeactivatedEditLabels = new Queue<SlideWindow_Element_EditableLabel>();

        #endregion

        bool filterSet = false;
        public string WindowName;

        private bool updateToggle = false;

        /// <summary>
        /// The information that gets filtered out
        /// </summary>
        /// <value></value>
        public string FilterInfo { get; set; } = "";

        /// <summary>
        /// The infromation that gets searched for (filtered in)
        /// </summary>
        /// <value></value>
        public string SearchInfo { get; set; } = "Test";
        public TMPro.TMP_InputField SearchBar;

        /// <summary>
        /// Is the filter being shown?
        /// </summary>
        public bool FilterShown { get; private set; } = false;

        /// <summary>
        /// Filters out any non desired objects
        /// </summary>
        public bool ApplyFilterOnOpen = true;

        WindowContentQueue content;

        public UI_ScrollRectOcclusion occl;

        void Start()
        {
            StartCoroutine(UpdateWindow());
            occl = InfomationObject.GetComponent<UI_ScrollRectOcclusion>();
            searchController = GetComponent<SlideWindowSearch>();

            if (ActiveIndicator != null)
                ActiveIndicator.color = PassiveColor;
        }


        IEnumerator UpdateWindow()
        {
            while (true)
            {
                if (updateToggle)
                {
                    UpdateWindowStatus();
                    updateToggle = false;
                }

                yield return new WaitForSeconds(.125f);
            }
        }

        private void UpdateWindowStatus()
        {
            if (_closed)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        #region window behavior

        /// <summary>
        /// open and stay open state
        /// </summary>
        public void Open()
        {
            //SlideWindowsManager.main.CloseWindows(true);

            _closed = false;

            if (window == null)
                window = GetComponent<RectTransform>();

            ActiveIndicator.color = ActiveColor;

            if (UserNudgeIcon != null)
                UserNudgeIcon.gameObject.SetActive(false);

            VerticalScrollBar.gameObject.SetActive(true);

            if (ActiveObjects.Count < 100)
                LeanTween.moveX(window, OpenLocation, time).setEaseOutExpo();
            else
                LeanTween.moveX(window, OpenLocation, .01f).setEaseOutExpo();

            if (CloseIcon != null && TabTypeIcon != null)
            {
                LeanTween.color(CloseIcon, new Color(1, 1, 1, 1), time / 6);
                LeanTween.color(TabTypeIcon, new Color(1, 1, 1, 0), time / 6);
            }

            InfomationObject.SetActive(true);


            ContentViewportParent.gameObject.SetActive(true);

            if (updateOnOpen)
                UpdateElements();

            transform.SetAsLastSibling();

            NotificationButtonManager.CloseNotificationBox();

            searchController.PanelActive = true;


            // occl.Init();
            // occl.SetEnabled(true);
        }

        /// <summary>
        /// Close and stay closed state
        /// </summary>
        public void Close()
        {
            _closed = true;

            if (searchController != null)
                searchController.PanelActive = false;

            ActiveIndicator.color = PassiveColor;

            // if (occl == null)
            //     Debug.LogError("occl is null");

            // occl.SetEnabled(false);

            if (window == null)
                window = GetComponent<RectTransform>();

            LeanTween.moveX(window, CloseLocation, time).setEaseOutExpo();

            if (ActiveObjects.Count < 100)
                LeanTween.moveX(window, CloseLocation, time).setEaseOutExpo();
            else
                LeanTween.moveX(window, CloseLocation, .01f).setEaseOutExpo();

            if (CloseIcon != null && TabTypeIcon != null)
            {
                LeanTween.color(CloseIcon, new Color(1, 1, 1, 0), time / 6);
                LeanTween.color(TabTypeIcon, new Color(1, 1, 1, 1), time / 6);
            }

            InfomationObject.SetActive(false);
            ContentViewportParent.gameObject.SetActive(false);

            VerticalScrollBar.gameObject.SetActive(false);

            LeanTween.delayedCall(.125f, () => { EventSystem.current.SetSelectedGameObject(null, null); });
        }

        public void ButtonToggleTab()
        {
            if (isReferenceSlideWindow && !start)
            {
                ActiveSheetsDisplayer.main.ViewReferences();
                start = true;
            }

            ToggleTab();
        }

        /// <summary>
        /// Toggle open and close the tab
        /// </summary>
        public void ToggleTab()
        {
            Closed = !Closed;

            try
            {
                if (LeanTween.isTweening(this.window))
                {
                    LeanTween.cancel(this.window);
                }
            }
            catch (System.Exception ex)
            {
                // NotificationHandler.Print("There is an error opening/closing the tab" + ex.Message);
            }

            if (!Closed)
            {
                if (UserNudgeIcon != null && UseNudge)
                {
                    UserNudgeIcon.gameObject.SetActive(false);
                }
            }

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolsManager.SelectTool(ToolTypes.None);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        }

        #endregion

        #region content management
        /// <summary>
        /// Add content to the slide window
        /// </summary>
        /// <param name="content">the info content being added to the window</param>
        public void AddContent(WindowContentQueue content)
        {
            if (loading)
            {
                Debug.LogError("the content is still loading, cannot add more content at this time");
                return;
            }

            // StopCoroutine(AddContentOverTime(content));
            // ClearContent(false, false);

            LoadingObject.SetActive(true);
            StartCoroutine(AddContentOverTime(content));
        }

        /// <summary>
        /// Add slide window content (like input text boxes) over time
        /// </summary>
        /// <param name="newContentQueue"></param>
        /// <returns></returns>
        IEnumerator AddContentOverTime(WindowContentQueue newContentQueue)
        {

            // Debug.Log("adding content " + WindowName);

            occl.scrollOcclEnabled = false;
            var sizeFitter = ContentViewportParent.GetComponent<ContentSizeFitter>();
            sizeFitter.enabled = true;

            var vertical = ContentViewportParent.GetComponent<VerticalLayoutGroup>();
            vertical.enabled = true;

            StopCoroutine(DisplayLoadBackground(0));


            if (newContentQueue.Count > 30)
                SetLoadingBackgroundActive_Timer(newContentQueue.Count * .25f);

            loading = true;

            if (Closed && GeneralSettings.ForceWindowPaneOpen && CanForcePaneOpen || Closed && IgnoreGeneralSettingsForcePane && CanForcePaneOpen)
            {
                ToggleTab();
            }
            else if (Closed)
            {
                if (UserNudgeIcon != null && UseNudge)
                    UserNudgeIcon.gameObject.SetActive(true);
            }

            int contentCount = newContentQueue.Count;

            int contentAmt = 0;

            while (newContentQueue.Count > 0)
            {
                var newContent = newContentQueue.DequeueContent();

                // try
                // {
                if (newContent.ElementType == SideWindow_Element_Types.Label)
                {
                    SpawnLabel(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.EditLabel)
                {
                    SpawnEditLabel(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Header)
                {
                    SpawnHeader(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Spacer)
                {
                    SpawnSpacer(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Button)
                {
                    SpawnButton(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Paragraph)
                {
                    SpawnParagraph(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.CardRep)
                {
                    SpawnCard(newContent);
                }

                if (newContent.ElementType == SideWindow_Element_Types.SheetElement)
                {
                    SpawnSheetElement(newContent);
                }

                // }
                // catch (System.Exception ex)
                // {
                //     NotificationHandler.Print(ex.Message);
                // }

                contentAmt++;

                if (newContentQueue.Count % 5 == 0)
                    yield return null;
            }

            LoadingObject.SetActive(false);

            LeanTween.delayedCall(.5f, () =>
            {
                occl.scrollOcclEnabled = true;
                VerticalScrollBar.value = 1;

                loading = false;
            });
        }


        /// <summary>
        /// Create or get a disabled sheet element
        /// </summary>
        /// <param name="content">the content to manage</param>
        private void SpawnSheetElement(ISideWindowContent content)
        {

            if (DeactivatedSheetElements.Count > 0)
            {

                var element = DeactivatedSheetElements.Dequeue();
                element.targetGameObject.gameObject.SetActive(true);
                //element.targetGameObject.transform.SetParent(ContentViewportParent);
                element.targetGameObject.transform.SetAsLastSibling();

                SetSheetElementValues(element, content);
            }
            else
            {

                GameObject go = Instantiate(SheetElementPrefab, ContentViewportParent);
                var element = go.GetComponent<SlideWindow_Element_SheetElement>();

                SetSheetElementValues(element, content);
            }
        }


        /// <summary>
        /// Set the sheet element values
        /// </summary>
        /// <param name="element">the element</param>
        /// <param name="content">the values to set</param>
        private void SetSheetElementValues(SlideWindow_Element_SheetElement element, ISideWindowContent content)
        {
            if (element != null)
            {
                try
                {

                    ActiveObjects.Add(element);
                    element.targetGameObject.SetActive(true);

                    SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;
                    element.info = info;
                    element.UpdateElement();

                }
                catch (System.Exception ex)
                {
                    int repId = element.info.CallBackActionType;
                    NotificationHandler.Print("Some of the data may be inserted incorrectly at line " + repId + " Error: " + ex.Message);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("The element is null");
#endif
            }
        }

        /// <summary>
        ///  Uses a object pooled card or creates a new one, sets it in the specified sliding window panel
        /// </summary>
        /// <param name="content">the data to display</param>
        private void SpawnCard(ISideWindowContent content)
        {
            if (DeactivatedCards.Count > 0)
            {
                var card = DeactivatedCards.Dequeue();
                card.targetGameObject.gameObject.SetActive(true);
                //card.targetGameObject.transform.SetParent(ContentViewportParent);
                card.targetGameObject.transform.SetAsLastSibling();

                SetCardValues(card, content);
            }
            else
            {
                GameObject cardObj = Instantiate(CardRepPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_CardRep card = cardObj.GetComponent<SlideWindow_Element_CardRep>();

                SetCardValues(card, content);
            }
        }

        private void SetCardValues(SlideWindow_Element_CardRep card, ISideWindowContent content)
        {
            if (card != null)
            {
                ActiveObjects.Add(card);

                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;
                card.info = info;
                card.UpdateElement();

                try
                {


                }
                catch (System.Exception ex)
                {
                    int repId = card.info.CallBackActionType;
                    NotificationHandler.Print("Some of the data may be inserted incorrectly at line " + repId + " Error: " + ex.Message);
                }
            }
        }

        public void SpawnLabel(ISideWindowContent content)
        {
            if (DeactivatedLabels.Count > 0)
            {
                var label = DeactivatedLabels.Dequeue();
                label.targetGameObject.gameObject.SetActive(true);
                //label.targetGameObject.transform.SetParent(ContentViewportParent);

                label.targetGameObject.transform.SetAsLastSibling();

                ApplyLabelProperties(label, content);

                // var info = (SlideWindow_Element_ContentInfo)content;
                // Debug.Log(info.Label + " deployed");
            }
            else
            {
                GameObject go = Instantiate(LabelPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_Label label = go.GetComponent<SlideWindow_Element_Label>();

                ApplyLabelProperties(label, content);

                // var info = (SlideWindow_Element_ContentInfo)content;
                // Debug.Log(info.Label + " created");
            }
        }

        private void ApplyLabelProperties(SlideWindow_Element_Label label, ISideWindowContent content)
        {

            if (label != null)
            {
                ActiveObjects.Add(label);

                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                label.info = info;
                label.UpdateElement();

                // Debug.Log(info.Label + " done");
            }
            else
            {
                Debug.Log("label is null");
            }
        }

        public void SpawnEditLabel(ISideWindowContent content)
        {
            if (DeactivatedEditLabels.Count > 0)
            {
                var label = DeactivatedEditLabels.Dequeue();
                label.targetGameObject.SetActive(true);
                //label.targetGameObject.transform.SetParent(ContentViewportParent);

                label.targetGameObject.transform.SetAsLastSibling();

                ApplyEditLabelProperties(label, content);
            }
            else
            {
                GameObject go = Instantiate(EditLabelPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_EditableLabel label = go.GetComponent<SlideWindow_Element_EditableLabel>();

                ApplyEditLabelProperties(label, content);
            }
        }


        private void ApplyEditLabelProperties(SlideWindow_Element_EditableLabel label, ISideWindowContent content)
        {

            if (label != null)
            {
                ActiveObjects.Add(label);

                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                label.info = info;
                label.UpdateElement();
            }
        }

        public void SpawnParagraph(ISideWindowContent content)
        {
            if (DeactivatedParagraphs.Count > 0)
            {
                var paragraph = DeactivatedParagraphs.Dequeue();
                paragraph.targetGameObject.SetActive(true);
                //paragraph.targetGameObject.transform.SetParent(ContentViewportParent);
                paragraph.targetGameObject.transform.SetAsLastSibling();

                ApplyParagraphProperties(paragraph, content);
            }
            else
            {
                GameObject go = Instantiate(ParagraphPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_Label paragraph = go.GetComponent<SlideWindow_Element_Label>();
                ApplyParagraphProperties(paragraph, content);
            }
        }

        private void ApplyParagraphProperties(SlideWindow_Element_Label paragraph, ISideWindowContent content)
        {
            if (paragraph != null)
            {
                ActiveObjects.Add(paragraph);

                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                paragraph.info = info;
                paragraph.UpdateElement();
            }
        }

        private void SpawnHeader(ISideWindowContent content)
        {
            if (DeactivatedHeaders.Count > 0)
            {
                var header = DeactivatedHeaders.Dequeue();
                header.targetGameObject.gameObject.SetActive(true);
                //header.targetGameObject.transform.SetParent(ContentViewportParent);

                header.targetGameObject.transform.SetAsLastSibling();

                ApplyHeaderProperties(header, content);
            }
            else
            {
                GameObject go = Instantiate(HeaderPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_Header header = go.GetComponent<SlideWindow_Element_Header>();
                ApplyHeaderProperties(header, content);
            }
        }

        /// <summary>
        /// Instantiate a header
        /// </summary>
        /// <param name="content">the content properties</param>
        private void ApplyHeaderProperties(SlideWindow_Element_Header header, ISideWindowContent content)
        {
            if (header != null)
            {
                ActiveObjects.Add(header);
                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                header.info = info;
                header.UpdateElement();
            }
        }

        public void SpawnSpacer(ISideWindowContent content)
        {
            if (DeactivatedSpacers.Count > 0)
            {
                var spacer = DeactivatedSpacers.Dequeue();
                spacer.targetGameObject.SetActive(true);
                //spacer.targetGameObject.transform.SetParent(ContentViewportParent);

                spacer.targetGameObject.transform.SetAsLastSibling();

                ApplySpacerProperties(spacer, content);
            }
            else
            {
                GameObject go = Instantiate(SpacerPrefab, ContentViewportParent) as GameObject;
                SlideWindow_Element_Spacer spacer = go.GetComponent<SlideWindow_Element_Spacer>();
                ApplySpacerProperties(spacer, content);
            }
        }

        /// <summary>
        /// Instantiate a spacer
        /// </summary>
        /// <param name="content">the content properties</param>
        public void ApplySpacerProperties(SlideWindow_Element_Spacer spacer, ISideWindowContent content)
        {
            if (spacer != null)
            {
                ActiveObjects.Add(spacer);
                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                spacer.info = info;
                spacer.UpdateElement();
            }

        }

        public void SpawnButton(ISideWindowContent content)
        {
            if (DeactivatedButtons.Count > 0)
            {
                var button = DeactivatedButtons.Dequeue();
                button.targetGameObject.SetActive(true);
                //button.targetGameObject.transform.SetParent(ContentViewportParent);

                button.targetGameObject.transform.SetAsLastSibling();

                ApplyButtonProperties(button, content);
            }
            else
            {
                GameObject go = Instantiate(ButtonPrefab, ContentViewportParent) as GameObject;
                SlideWindowElement_Button button = go.GetComponent<SlideWindowElement_Button>();
                ApplyButtonProperties(button, content);
            }
        }

        /// <summary>
        /// Instantiate a button
        /// </summary>
        /// <param name="content">the content button properties</param>
        public void ApplyButtonProperties(SlideWindowElement_Button button, ISideWindowContent content)
        {
            if (button != null)
            {
                ActiveObjects.Add(button);
                SlideWindow_Element_ContentInfo info = (SlideWindow_Element_ContentInfo)content;

                button.info = info;
                button.UpdateElement();
            }
        }

        /// <summary>
        /// Turn the loading screen on for a certian amount of time
        /// </summary>
        /// <param name="time"></param>
        public void SetLoadingBackgroundActive_Timer(float time)
        {
            StartCoroutine(DisplayLoadBackground(time));
        }

        IEnumerator DisplayLoadBackground(float amt)
        {
            // LoadingObject.SetActive(true);
            yield return new WaitForSecondsRealtime(amt);
            // LoadingObject.SetActive(false);
        }

        /// <summary>
        /// Search the window (or use filter depending on mode)
        /// </summary>
        /// <param name="input">the search bar</param>
        public void Search(TMPro.TMP_InputField input)
        {
            // if (FilterShown)
            // {
            //     SetFilter(input.text);
            // }
            // else
            // {
            //     Search(input.text);
            // }
        }

        public void OnSearchValuesChanged()
        {
            if (!loading)
                Search(searchController.SearchString);
            else
            {
                StopCoroutine(WaitToSearch());
                StartCoroutine(WaitToSearch());
            }
        }

        IEnumerator WaitToSearch()
        {
            if (loading)
            {
                yield return null;
            }
            else
            {
                Search(searchController.SearchString);
            }
        }

        /// <summary>
        /// Search for elements with the correct labels or content
        /// </summary>
        /// <param name="search">the current text</param>
        /// <returns>Returns the first string as a suggestion</returns>
        public void Search(string search)
        {
            if (loading)
            {
                return;
            }

            occl.scrollOcclEnabled = false;
            var sizeFitter = ContentViewportParent.GetComponent<ContentSizeFitter>();
            sizeFitter.enabled = true;

            var vertical = ContentViewportParent.GetComponent<VerticalLayoutGroup>();
            vertical.enabled = true;

            if (search == null)
                return;

            search = search.Trim();

            string[] filterOut = searchController.FilterKeywords.ToArray();

            search = search.ToLower();

            foreach (ISlideWindow_Element go in ActiveObjects)
            {
                if (go != null && go.info != null)
                {
                    bool allow = false;

                    if (go.SearchContent.ToLower().Contains(search))
                    {
                        bool found = false;
                        foreach (var f in filterOut)
                        {
                            if (f != null)
                            {
                                if (go.SearchContent.ToLower().Contains(f))
                                {
                                    found = true;
                                }
                            }
                        }

                        if (found)
                        {
                            allow = false;
                        }
                        else
                        {
                            allow = true;
                        }
                    }

                    if (allow)
                    {
                        go.targetGameObject.SetActive(true);
                        // Debug.Log("found object " + search);
                    }
                    else
                    {
                        go.targetGameObject.SetActive(false);
                        // Debug.Log("object not correct " + search);
                    }

                }
            }

            LeanTween.delayedCall(.25f * Mathf.Pow(ActiveObjects.Count, 2), () =>
            {
                occl.scrollOcclEnabled = true;
                VerticalScrollBar.value = 1;
            });
        }


        public void SetFilter(string str)
        {
            FilterInfo = str;
            searchController.AddFilterKeywordsFromCSVString(str);
            filterSet = true;
        }


        /// <summary>
        /// Sets all elements active to be viewed
        /// </summary>
        public void CancelSearch()
        {

            LeanTween.delayedCall(.5f, () =>
            {
                if (filterSet)
                {
                    filterSet = false;
                    return;
                }

                searchController.SetSearchPanelActive(false);

                foreach (ISlideWindow_Element go in ActiveObjects)
                {
                    go.targetGameObject.SetActive(true);
                }
            });
        }

        /// <summary>
        /// Update all the elements in the pane
        /// </summary>
        public void UpdateElements()
        {
            if (Closed)
            {
                updateOnOpen = true;
                return;
            }

            foreach (ISlideWindow_Element e in ActiveObjects)
            {
                e.UpdateElement();
            }

            updateOnOpen = false;
        }

        /// <summary>
        /// Clear content from the window pane
        /// </summary>
        public void ClearContent(bool cancelSearch = true, bool RemoveAllSystemFilterKeywords = true)
        {
            if (loading)
            {
                StopCoroutine(AddContentOverTime(null));
                loading = false;
            }

            if (RemoveAllSystemFilterKeywords)
            {
                if (searchController != null)
                    searchController.RemoveAllSystemKeywords();
            }

            // occl.SetEnabled(false);

            if (cancelSearch)
                CancelSearch();

            RemoveObjects();
        }

        private void RemoveObjects()
        {
            if (ActiveObjects.Count < 1)
                return;

            for (int i = ActiveObjects.Count - 1; i >= 0; i--)
            {
                var obj = ActiveObjects[i];
                //obj.targetGameObject.transform.SetParent(UIObjectPool);
                ActiveObjects.RemoveAt(i);

                var newContent = obj.info;

                if (newContent.ElementType == SideWindow_Element_Types.Label)
                {
                    DeactivatedLabels.Enqueue((SlideWindow_Element_Label)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.EditLabel)
                {
                    DeactivatedEditLabels.Enqueue((SlideWindow_Element_EditableLabel)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Header)
                {
                    DeactivatedHeaders.Enqueue((SlideWindow_Element_Header)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Spacer)
                {
                    DeactivatedSpacers.Enqueue((SlideWindow_Element_Spacer)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Button)
                {
                    DeactivatedButtons.Enqueue((SlideWindowElement_Button)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.Paragraph)
                {
                    DeactivatedParagraphs.Enqueue((SlideWindow_Element_Label)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.CardRep)
                {
                    DeactivatedCards.Enqueue((SlideWindow_Element_CardRep)obj);
                }

                if (newContent.ElementType == SideWindow_Element_Types.SheetElement)
                {
                    DeactivatedSheetElements.Enqueue((SlideWindow_Element_SheetElement)obj);
                }

                obj.targetGameObject.gameObject.SetActive(false);
            }


            ActiveObjects.Clear();

        }

        #endregion

    }
}
