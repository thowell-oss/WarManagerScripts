/* SheetDriver.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using System.Text.Json;

using UnityEngine;

using WarManager.Cards;
using WarManager.Sharing;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Drives the war manager card editor
    /// </summary>
    public class WarManagerDriver : MonoBehaviour
    {
        /// <summary>
        /// The Camera responsible for the given sheet
        /// </summary>
        public Camera SheetCamera;

        /// <summary>
        /// A constant offset that all cards adjust to
        /// </summary>
        public Vector2 Offset = new Vector2(-7.5f, 0f);

        /// <summary>
        /// Dicatates the amount of distance between cards
        /// </summary>
        public Vector2 Scale = new Vector2(4.7f, 1.7f);

        public float CardSpeed = .1f;

        /// <summary>
        /// The distance from the final value where the card will snap to its value (and would not be updated next frame).
        /// </summary>
        public static float sleepThreshHold = .05f;

        /// <summary>
        /// The global parent where all cards will child under
        /// </summary>
        [SerializeField] private GameObject _cardParent;

        /// <summary>
        /// The card prefab
        /// </summary>
        [SerializeField] private GameObject _cardPrefab;

        /// <summary>
        /// The selected hover card
        /// </summary>
        public SelectedCard<UnityCardDisplay> HoverCard
        {
            get
            {
                return SelectHandler.SelectedHoverCard;
            }

            private set
            {

                SelectHandler.SelectedHoverCard = value;
            }
        }

        /// <summary>
        /// for your viewing pleasure
        /// </summary>
        public List<UnityCardDisplay> SelectedCards = new List<UnityCardDisplay>();

        /// <summary>
        /// The selected card handler
        /// </summary>
        public CardSelectionHandler<UnityCardDisplay> SelectHandler { get; private set; } = new CardSelectionHandler<UnityCardDisplay>();

        /// <summary>
        /// Cards that are not needing to be updated currently
        /// </summary>
        public List<UnityCardDisplay> _unityCards = new List<UnityCardDisplay>();


        [Header("GUI Initializer")]
        public StatsGUI StatsObject;
        public LoadingGUI Loader;
        public GUIGridPointMarkersDriver gridPointMarkersDriver;


        public GameObject FullScreenLoadingCanvas;

        #region static main
        /// <summary>
        /// The static reference to the sheet driver
        /// </summary>
        public static WarManagerDriver Main;

        private Forms.FormsController _formsController;

        void Awake()
        {
            if (Main != null)
            {
                throw new NotSupportedException("Only one sheet driver can exist!");
            }
            else
            {
                Main = this;
                StartCoroutine(GeneralUpdate_Corutine());
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {


            //_formsController = new Forms.FormsController();

            //LeanTween.delayedCall(1, () =>
            //{
            //    if (SheetsManager.SheetCount < 1)
            //        SheetsManager.SetHomeCardSheetCurrent();
            //});

            // SetApplicationSettings();

            UnityGUIDriver.SetStatsObject(StatsObject);
            UnityGUIDriver.SetLoadingGUI(Loader);
            UnityGUIDriver.pointMarkersDriver = gridPointMarkersDriver;


            ToolsManager.SelectedTool = ToolTypes.None;
            ToolsManager.SelectTool(ToolTypes.Edit, true);
        }

        

        #region previous version stuff

        /// <summary>
        /// Properly creates a unity card display (important if you do not want slient freezes with no warning).
        /// </summary>
        /// <param name="display">the unity card display to instantiate</param>
        /// <returns>returns a reference to the instantated unity card display</returns>
        [Obsolete]
        private UnityCardDisplay CreateUnityCardDisplay(GameObject prefab)
        {
            GameObject go = Instantiate(prefab, _cardParent.transform) as GameObject;
            UnityCardDisplay ucd = go.GetComponent<UnityCardDisplay>();
            _unityCards.Add(ucd);

            return ucd;
        }

        /// <summary>
        /// Properly creates a unity card display (important if you do not want slient freezes with no warning).
        /// </summary>
        /// <param name="display">the unity card display to instantiate</param>
        /// <returns>returns a reference to the instantated unity card display</returns>
        [Obsolete]
        private UnityCardDisplay CreateUnityCardDisplay(UnityCardDisplay display)
        {
            return CreateUnityCardDisplay(display.gameObject);
        }

        [Obsolete]
        public void SpawnCard(UnityCardDisplay display, Card c)
        {
            //if (c == null)
            //    Debug.LogError("Card is null");

            //ToolsManager.SelectedTool = ToolTypes.Edit;

            //CardUtility.TryAddCard(c);

            //var instantiatedDisplay = CreateUnityCardDisplay(display);

            //if (instantiatedDisplay == null)
            //    Debug.LogError("instantiated display is null");

            //instantiatedDisplay.Card = c;

            //Vector3 pos = Vector3.zero;
            //pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2);

            //instantiatedDisplay.transform.position = Camera.main.ScreenToWorldPoint(pos);
            //instantiatedDisplay.OnStartDragging();
        }


        /// <summary>
        /// The card the mouse is hovering over
        /// </summary>
        /// <param name="guiCard">the gui card to add</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        [Obsolete]
        public bool AddHoverCard(UnityCardDisplay guiCard)
        {
            //if (HoverCard == null)
            //{
            //    HoverCard = new SelectedCard<UnityCardDisplay>(guiCard.Card, guiCard);
            //    return true;
            //}
            //else
            return false;
        }

        /// <summary>
        /// Removes the hover card from the select handler
        /// </summary>
        /// <param name="guiCard">the card to remove</param>
        /// <returns>returns true if the hover card was found, false if not</returns>
        [Obsolete]
        public bool RemoveHoverCard(UnityCardDisplay guiCard)
        {
            //if (HoverCard != null)
            //{
            //    if (HoverCard.Card != null)
            //    {
            //        if (HoverCard.Card.ID == guiCard.ID)
            //        {
            //            SheetDriver.Main.HoverCard = null;
            //            return true;
            //        }
            //    }
            //}

            return false;
        }


        /// <summary>
        /// Adds a new selected card to the list of selected cards
        /// </summary>
        /// <param name="disp">the unity card display (front end)</param>
        /// <param name="backend">the backend refrence</param>
        [Obsolete]
        public void AddSelectedCard(UnityCardDisplay disp, bool deselectPrevious)
        {
            //if (disp.Card != null)
            //{
            //    SelectedCard<UnityCardDisplay> newCard = new SelectedCard<UnityCardDisplay>(disp.Card, disp);

            //    if (deselectPrevious)
            //        SelectHandler.Clear();

            //    SelectHandler.AddCard(newCard);
            //    SlideWindowsManager.main.SetCardsProperties(SelectHandler.GetAllFrontendCards().ToArray());

            //    WarSystem.WriteToLog("display card not null");
            //}
            //else
            //{
            //    WarSystem.WriteToLog("display card is null");
            //    throw new NullReferenceException("display card is null");
            //}
        }

        /// <summary>
        /// Updates the sheet when the cards are being deselected
        /// </summary>
        public void DeselectCard()
        {
            SlideWindowsManager.main.SetCardsProperties(SelectHandler.GetAllFrontendCards().ToArray());
        }

        /// <summary>
        /// Remove a selected card from the list of selected cards
        /// </summary>
        /// <param name="disp">the front end id reference</param>
        public void RemoveSelectedCardFromSelection(UnityCardDisplay disp)
        {
            SelectHandler.RemoveCard(disp.ID);

            SlideWindowsManager.main.SetCardsProperties(SelectHandler.GetAllFrontendCards().ToArray());
        }

        /// <summary>
        /// Calls the general update event every 5 seconds
        /// </summary>
        /// <returns></returns>
        public IEnumerator GeneralUpdate_Corutine()
        {
            while (true)
            {
                CardUtility.CallGeneralUpdate_Event();
                yield return new WaitForSecondsRealtime(5);
            }
        }

        #region tuple conversion
        /// <summary>
        /// Convert a tuple to Vector 2
        /// </summary>
        /// <param name="t">the tuple</param>
        /// <returns>returns a vector2</returns>
        public static Vector2 ConvertTupleToVector2((float, float) t)
        {
            float x = t.Item1;
            float y = t.Item2;

            return new Vector2(x, y);
        }

        public static Vector2 ConvertPointToVector2(Point p)
        {
            return new Vector2(p.x, p.y);
        }

        /// <summary>
        /// Convert a tuple to Vector 3
        /// </summary>
        /// <param name="t">the tuple</param>
        /// <returns>returns a vector3</returns>
        public static Vector3 ConvertTupleToVector3((float, float, float) t)
        {
            float x = t.Item1;
            float y = t.Item2;
            float z = t.Item3;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Converts a vector2 into a (float, float) tuple
        /// </summary>
        /// <param name="v">the vector2</param>
        /// <returns>returns a tuple</returns>
        public static (float, float) ConvertVector2ToTuple(Vector2 v)
        {
            float x = v.x;
            float y = v.y;

            return (x, y);
        }

        /// <summary>
        /// Converts a vector3 into a (float, float, float) tuple
        /// </summary>
        /// <param name="v">the vector3</param>
        /// <returns>returns a tuple</returns>
        public static (float, float, float) ConvertVector3ToTuple(Vector3 v)
        {
            float x = v.x;
            float y = v.y;
            float z = v.z;

            return (x, y, z);
        }

        public (float, float) GetGlobalOffsetTuple()
        {
            return ConvertVector2ToTuple(Offset);
        }

        public (float, float) GetCardMultiplierTuple()
        {
            return ConvertVector2ToTuple(Scale);
        }
        #endregion

        public void SetSheetCurrent_EventListener(string id)
        {
            if (id == null)
                return;

            var grid = WarManager.Backend.SheetsManager.GetGridScale(id);

            Scale.x = (float)grid[0];
            Scale.y = (float)grid[1];
        }

        #endregion

        private void OnEnable()
        {
            CardSelectionHandler<UnityCardDisplay>.OnDeselect += DeselectCard;
            WarManager.Backend.SheetsManager.OnSetSheetCurrent += SetSheetCurrent_EventListener;
        }

        private void OnDisable()
        {
            CardSelectionHandler<UnityCardDisplay>.OnDeselect -= DeselectCard;
            WarManager.Backend.SheetsManager.OnSetSheetCurrent -= SetSheetCurrent_EventListener;
        }
    }
}
