/* UnityCardElementsManger.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;
using WarManager.Cards.Elements;
using WarManager.Cards;


using Sirenix.OdinInspector;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Manages the unity card elements on the card
    /// </summary>
    public class UnityCardElementsManager : MonoBehaviour
    {
        [PropertyTooltip("The unique id of the card instance")]
        [BoxGroup("Identity")]
        [SerializeField] private string unityCardId;
        [BoxGroup("Identity")]
        [PropertyTooltip("The id of the data set the card is supposed to represent")]
        public string datasetId = "d";

        [Space]
        [TabGroup("Initialization")]
        public Transform InstantiationElementParent;

        [TabGroup("Initialization")]
        public UnityCardText TextPrefab;
        [TabGroup("Initialization")]
        public UnityCardImage StickerPrefab;
        [TabGroup("Initialization")]
        public GameObject GlancePrefab;
        [TabGroup("Initialization")]
        public UnityCardModernRadialDial ModernDialPrefab;
        [TabGroup("Initialization")]
        public UnityCardLinkButtonElement LinkButtonElement;
        [TabGroup("Initialization")]
        [Space]
        public List<PoolableObject> UnityCardElements;


        [TabGroup("Initialization")]
        [SerializeField] GameObject CannotEditIcon;
        [TabGroup("Initialization")]
        [SerializeField] GameObject DataSetColorBar;


        public UnityCardBackgroundController BackgroundController;
        public GameObject Background;
        public GameObject LoadingAnim;

        [TabGroup("Runtime")]
        [SerializeField]
        private List<UnityCardText> TextPrefabElements = new List<UnityCardText>();
        [TabGroup("Runtime")]
        [SerializeField]
        private List<UnityCardImage> StickerPrefabElements = new List<UnityCardImage>();
        [TabGroup("Runtime")]
        [SerializeField]
        private List<UnityCardGlanceIconsBar> GlancePrefabElements = new List<UnityCardGlanceIconsBar>();
        [TabGroup("Runtime")]
        [SerializeField]
        private List<UnityCardModernRadialDial> ModernDialElements = new List<UnityCardModernRadialDial>();
        [TabGroup("Runtime")]
        [SerializeField]
        private List<UnityCardLinkButtonElement> LinkElements = new List<UnityCardLinkButtonElement>();
        [Space]

        private List<PoolableObject> _cardElements = new List<PoolableObject>();

        /// <summary>
        /// The reference to the associated unity card
        /// </summary>
        private UnityCard _unityCard;

        /// <summary>
        /// The backend reference to the card
        /// </summary>
        /// <value></value>
        private Card _card
        {
            get
            {
                if (_unityCard != null)
                    return _unityCard.Card;

                return null;
            }
        }

        /// <summary>
        /// The backend reference to the display information
        /// </summary>
        /// <value></value>
        private CardMakeup _makeup
        {
            get
            {
                if (_unityCard != null)
                {
                    return _unityCard.Card.MakeUp;
                }

                return null;
            }
        }

        private bool update = false;

        /// <summary>
        /// The dataset being represented
        /// </summary>
        /// <value></value>
        private string _currentDatasetID
        {
            get
            {
                return _unityCard.Card.DatasetID;
            }
        }

        /// <summary>
        /// The actual data being displayed
        /// </summary>
        /// <value></value>
        private string _dataID
        {
            get
            {
                return _unityCard.Card.RowID;
            }
        }

        /// <summary>
        /// the data entry
        /// </summary>
        private DataEntry _dataEntry;

        private List<string> searchInfo = new List<string>();

        public void RefreshCardIdentity(UnityCard unityCard, string sheetID)
        {
            if (unityCard == null)
                throw new System.NullReferenceException("the unity card cannot be null");

            if (WarSystem.DataSetManager == null)
            {
                throw new System.NullReferenceException("The dataset manager is null");
            }

            _unityCard = unityCard;
            unityCardId = _unityCard.ID;

            _dataEntry = unityCard.Card.Entry;

            if (sheetID != null)
            {
                var sheet = SheetsManager.GetActiveSheet(sheetID);
                if (sheet != null)
                {
                    // var datasetList = sheet.GetDatasetIDs();
                    TriggerLoad();
                }
                else
                {
                    throw new System.NullReferenceException("Sheet is null");
                }
            }
            else
            {
                throw new System.NullReferenceException("Sheet ID is null");
            }
        }

        /// <summary>
        /// Legacy load trigger system
        /// </summary>
        public void TriggerLoad()
        {
            LoadingAnim.SetActive(true);
            update = true;

            //Debug.Log("loading");
        }

        void Update()
        {
            if (update)
            {
                CannotEditIcon.SetActive(false);
                Unload();
                update = false;
                StartCoroutine(Load());
            }
        }

        private void Unload(bool includeBackground = true)
        {

            #region old
            // for (int i = 0; i < TextPrefabElements.Count; i++)
            // {
            //     TextPrefabElements[i].gameObject.SetActive(false);
            // }

            // for (int k = 0; k < StickerPrefabElements.Count; k++)
            // {
            //     StickerPrefabElements[k].gameObject.SetActive(false);
            // }

            // for (int j = 0; j < GlancePrefabElements.Count; j++)
            // {
            //     GlancePrefabElements[j].gameObject.SetActive(false);
            // }

            // for (int l = 0; l < ModernDialElements.Count; l++)
            // {
            //     ModernDialElements[l].gameObject.SetActive(false);
            // }

            // for (int m = 0; m < LinkElements.Count; m++)
            // {
            //     LinkElements[m].gameObject.SetActive(false);
            // }

            #endregion

            foreach (PoolableObject x in _cardElements)
            {
                x.gameObject.SetActive(false);
            }

            if (includeBackground)
            {
                Background.gameObject.SetActive(false);
                BackgroundController.Size = new Vector2(1f, 1f);
                BackgroundController.BackgroundColor = Color.clear;
                BackgroundController.BorderThickness = .25f;
            }
        }

        IEnumerator Load()
        {
            yield return new WaitUntil(Refresh);
            yield return DispatchData();
            LoadingAnim.SetActive(false);

            StopCoroutine(Load());
        }


        private bool Refresh()
        {
            try
            {
                if (_card != null)
                {

                    var newDataset = _card.DataSet;

                    SetCard_Edit(_card, newDataset.SelectedView.CanEditCard && newDataset.SelectedView.CanViewCard);


                    if (!newDataset.ID.StartsWith("sys"))
                        CreateSearchInfo(newDataset);

                    BackgroundController.DataSetBarColor = newDataset.Color;

                    _unityCard.BackgroundController.Actor = _dataEntry.Actor != null;

                    return true;
                }

            }
            catch (System.Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Create and setup the elements
        /// </summary>
        /// <param name="selectedDataset"></param>
        private IEnumerator DispatchData()
        {

            Find(SheetsManager.CurrentFindText); // this is so that the new cards will update if a find query was already created

            CardElementViewData[] elements = _makeup.CardElementArray;

            var entry = _makeup.Entry;

            for (int i = 0; i < elements.Length; i++)
            {

                var elementData = elements[i];

                try
                {
                    #region old

                    //     if (_makeup.InfoToDisplay.Count > i)
                    //         elements[i].DisplayInfo = _makeup.InfoToDisplay[i];

                    //     switch (elements[i].ElementTag)
                    //     {
                    //         case "text":
                    //             SetTextElement(elements[i]);
                    //             break;

                    //         case "background":
                    //             SetBackground(elements[i]);
                    //             break;

                    //         case "sticker":
                    //             SetSticker(elements[i]);
                    //             break;

                    //         case "glance":
                    //             SetGlance(elements[i]);
                    //             break;

                    //         case "dial":
                    //             SetModernDial(elements[i]);
                    //             break;

                    //         case "link":
                    //             SetButtonLink(elements[i]);
                    //             break;

                    //         default:
                    //             Debug.LogWarning("Data element " + elements[i].ElementTag + " is not recognized.");
                    //             NotificationHandler.Print($"Data Element tag not recognized {elements[i].ElementTag}");
                    //             break;
                    //     }

                    #endregion

                    if (elementData is CardElementTextData text)
                    {
                        SetTextElement(text, entry);
                    }
                    else if (elementData is CardBackgroundElementData background)
                    {
                        SetBackground(background, entry);
                    }
                    else if (elementData is CardStickerElementData sticker)
                    {
                        SetSticker(sticker, entry);
                    }
                    else if (elementData is CardGlanceElementData glance)
                    {
                        SetGlance(glance, entry);
                    }
                    else if (elementData is CardDialElementData dial)
                    {
                        SetModernDial(dial, entry);
                    }
                    else if (elementData is CardButtonElementData button)
                    {
                        SetButtonLink(button, entry);
                    }
                    else if (elementData is LoadingBarElementData bar)
                    {
                        if (GetElement<LoadingBarElement>(bar.ColumnType, out var e))
                        {
                            SetElementProperties<LoadingBarElementData, LoadingBarElement>(bar, e, entry);
                        }
                    }
                    else if (elementData is PieChartElementData pieChart)
                    {
                        if (GetElement<PieChartCardElement>(pieChart.ColumnType, out var e))
                        {
                            SetElementProperties<PieChartElementData, PieChartCardElement>(pieChart, e, entry);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex.Message);
                }

                _cardElements.Sort((x, y) =>
                {

                    double? a = null;
                    double? b = null;

                    if (x is IUnityCardElement e)
                    {
                        if (e.ElementViewData != null)
                        {
                            if (e.ElementViewData.Location != null && e.ElementViewData.Location.Length >= 3)
                            {
                                a = e.ElementViewData.Location[2];
                            }
                        }
                    }

                    if (y is IUnityCardElement f)
                    {
                        if (f.ElementViewData != null)
                        {
                            if (f.ElementViewData.Location != null && f.ElementViewData.Location.Length >= 3)
                            {
                                b = f.ElementViewData.Location[2];
                            }
                        }
                    }

                    if (a != null && b != null)
                    {
                        if (a > b)
                            return -1;
                        if (a == b)
                            return 0;
                        if (a < b)
                            return 1;
                    }

                    return 1;
                });

                foreach (var x in _cardElements)
                {
                    x.transform.SetAsFirstSibling();
                }

                yield return new WaitForSeconds(.05f);
            }

            yield return null;
        }

        /// <summary>
        /// Get the selected element from the list
        /// </summary>
        /// <typeparam name="T">the type of element being looked for</typeparam>
        /// <param name="element">the element out peramter</param>
        /// <returns>returns true if the item is found false if not </returns>
        private bool GetElement<T>(string elementTag, out T element) where T : PoolableObject, IUnityCardElement
        {

            for (int i = 0; i < UnityCardElements.Count; i++)
            {
                if (UnityCardElements[i] is IUnityCardElement e)
                {

                    //Debug.Log(e.ElementTag + " " + elementTag);


                    if (UnityCardElements[i] is T t)
                    {
                        element = t;
                        return true;
                    }
                    else
                    {

#if UNITY_EDITOR
                        Debug.Log("found element but was not the type " + elementTag);
#endif

                        element = default(T);
                        return false;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("A unity card element does not contain the interface " + nameof(IUnityCardElement) + " at location " + i + " in the list.");
#endif
                }
            }

#if UNITY_EDITOR
            Debug.LogError("Could not find the element " + elementTag);
#endif

            element = default(T);
            return false;
        }

        /// <summary>
        /// Set the card ability to be edited or not edited
        /// </summary>
        /// <param name="c">the card to set</param>
        /// <param name="manual">the card manual instructions</param>
        private void SetCard_Edit(Card c, bool canEdit)
        {

            // Debug.Log("Setting card edit " + c.point + "  " + canEdit);

            if (!canEdit)
            {
                c.Lock(true);
                c.SetHide(false);
                c.CanLockOrUnlock = false;

                CannotEditIcon.gameObject.SetActive(true);
                Background.SetActive(false);
                DataSetColorBar.SetActive(false);
            }
            else
            {
                c.CanLockOrUnlock = true;
                CannotEditIcon.gameObject.SetActive(false);
                Background.SetActive(true);
                DataSetColorBar.SetActive(true);
            }
        }


        /// <summary>
        /// Set a text element to display a datapiece
        /// </summary>
        /// <param name="data">the data to display</param>
        private void SetTextElement(CardElementTextData data, DataEntry entry)
        {
            if (data == null)
                throw new System.NullReferenceException("The cardElementData is null");

            UnityCardText text = PoolManager.Main.CheckOutGameObject<UnityCardText>(TextPrefab.gameObject, true, InstantiationElementParent);
            _cardElements.Add(text);

            if (text == null)
                WarSystem.WriteToLog("something went wrong creating a text element", Logging.MessageType.critical);

            UnityCardText.TrySetProperties(text, data, entry);
        }


        /// <summary>
        /// Set the sticker properties
        /// </summary>
        /// <param name="data"></param>
        public void SetSticker(CardStickerElementData data, DataEntry entry)
        {

            if (data == null)
                throw new System.NullReferenceException("The cardElementData is null");

            UnityCardImage sticker = PoolManager.Main.CheckOutGameObject<UnityCardImage>(StickerPrefab.gameObject, true, InstantiationElementParent);
            _cardElements.Add(sticker);

            if (sticker == null)
                WarSystem.WriteToLog("something went wrong getting a sticker element", Logging.MessageType.critical);

            UnityCardImage.TrySetSticker(sticker, data, entry);
        }

        /// <summary>
        /// set the glance icon properties
        /// </summary>
        /// <param name="data"></param>
        public void SetGlance(CardGlanceElementData data, DataEntry entry)
        {

            if (data == null)
                throw new System.NullReferenceException("The cardElementData cannot be null");

            UnityCardGlanceIconsBar glance = PoolManager.Main.CheckOutGameObject<UnityCardGlanceIconsBar>(GlancePrefab.gameObject, true, InstantiationElementParent);
            _cardElements.Add(glance);

            if (glance == null)
                WarSystem.WriteToLog("something went wrong creating a glance element", Logging.MessageType.critical);

            UnityCardGlanceIconsBar.TrySetGlance(glance, data, entry);
        }

        public void SetModernDial(CardDialElementData data, DataEntry entry)
        {
            if (data == null)
                throw new System.NullReferenceException("The cardElementData cannot be null");

            UnityCardModernRadialDial dial = PoolManager.Main.CheckOutGameObject<UnityCardModernRadialDial>(ModernDialPrefab.gameObject, true, InstantiationElementParent);
            _cardElements.Add(dial);
            if (dial == null)
                WarSystem.WriteToLog("something went wrong creating a dial element", Logging.MessageType.critical);


            UnityCardModernRadialDial.TrySetDial(dial, data, entry);
        }

        public void SetButtonLink(CardButtonElementData data, DataEntry entry)
        {

            if (data == null)
                throw new System.NullReferenceException("The cardElementData cannot be null");

            UnityCardLinkButtonElement link = PoolManager.Main.CheckOutGameObject<UnityCardLinkButtonElement>(LinkButtonElement.gameObject, true, InstantiationElementParent);
            _cardElements.Add(link);
            if (link == null)
            {
                WarSystem.WriteToLog("the card element button link is null", Logging.MessageType.critical);
            }


            UnityCardLinkButtonElement.TrySetButtonLink(link, data, _unityCard, entry);
        }


        /// <summary>
        /// Set the properties of a selected element <typeparamref name="Telement"/>
        /// </summary>
        /// <typeparam name="Tdata">the data type of the element</typeparam>
        /// <typeparam name="Telement">the element</typeparam>
        /// <param name="data">the data being passed to the element</param>
        /// <param name="dataElement">the element</param>
        /// <param name="entry">the data entry which provides context for the element</param>
        /// <exception cref="System.NullReferenceException">thrown when <paramref name="data"/> is null or <paramref name="dataElement"/> is null</exception>
        private void SetElementProperties<Tdata, Telement>(Tdata data, Telement dataElement, DataEntry entry) where Tdata :
            CardElementViewData where Telement : PoolableObject, IUnityCardElement
        {
            if (data == null)
                throw new System.NullReferenceException("The card element data cannot be null");

            if (entry == null)
                throw new System.NullReferenceException("the data entry cannot be null");

            try
            {
                var element = PoolManager.Main.CheckOutGameObject<Telement>(dataElement.gameObject, true, InstantiationElementParent);

                if (element is IUnityCardElement unityCardElement)
                {

                    if (element == null)
                    {
                        WarSystem.WriteToLog("the card element is null (" + data.ColumnType + ")", Logging.MessageType.critical);
                    }
                    else
                    {
                        _cardElements.Add(element);

                        bool success = unityCardElement.SetElementProperties<Tdata>(data, entry);

                        if (!success)
                        {
                            throw new System.Exception("Could not complete the creation of the element " + data.ColumnType);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Set the card background
        /// </summary>
        /// <param name="data">the data to display</param>
        private void SetBackground(CardBackgroundElementData data, DataEntry entry)
        {
            Background.SetActive(true);
            UnityCardBackgroundController.TrySetProperties(BackgroundController, data);
        }

        /// <summary>
        /// Clear all the elements on the card
        /// </summary>
        public void ClearElements()
        {
            Unload(false);
        }

        private void Find(string text)
        {
            if (_dataEntry == null)
                return;

            List<string> str = new List<string>();

            var valueTypePairs = _dataEntry.GetAllowedValueTypePairs();

            for (int i = 0; i < valueTypePairs.Length; i++)
            {
                str.Add(valueTypePairs[i].Value.ToString());
            }

            str.AddRange(searchInfo);

            bool found = false;

            List<string> searchCriteria = new List<string>();

            if (text.Contains(","))
            {
                searchCriteria.AddRange(text.Split(','));
            }
            else
            {
                searchCriteria.Add(text);
            }

            foreach (var search in searchCriteria)
            {
                foreach (var s in str)
                {
                    if (search.StartsWith("ID: "))
                    {
                        if (_card != null)
                        {
                            if (_card.ID == search.Remove(0, 4))
                            {
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        string lowerS = s.ToLower();
                        string find = search.ToLower();

                        if (lowerS.Contains(find) && find != string.Empty)
                        {
                            found = true;
                            break;
                        }
                    }
                }
            }

            _unityCard.CardFound = found;
            _unityCard.UpdateBorder();
        }

        private void CreateSearchInfo(DataSet set)
        {
            List<string> str = new List<string>();

            if (_card != null)
            {
                str.Add(_card.ID);
                str.Add(_card.DatasetID);
                str.Add((_card.RowID).ToString());
                str.Add(set.DatasetName);

                if (!string.IsNullOrEmpty(_card.RowID))
                {
                    // DataPiece p = set.GetData((int)_card.DataRepID + 1);

                    // for (int j = 0; j < p.TagCount; j++)
                    // {
                    //     str.Add(p.GetTag(j));
                    // }

                    // for (int i = 0; i < p.DataCount; i++)
                    // {
                    //     str.Add(p.GetData(i));
                    // }

                    DataEntry entry = set.GetEntry(_card.RowID);
                    var values = entry.GetAllowedValueTypePairs();

                    foreach (var v in values)
                    {
                        str.Add(v.Value.ToString());
                        str.Add(v.Type.ToString());
                    }
                }
            }

            searchInfo = str;
        }

        void OnEnable()
        {
            //SheetsManager.OnSetSheetCurrent += RefreshCardIdentity;
            SheetsManager.OnFindText += Find;
        }

        void OnDisable()
        {
            //SheetsManager.OnSetSheetCurrent -= RefreshCardIdentity;
            SheetsManager.OnFindText -= Find;
        }
    }
}