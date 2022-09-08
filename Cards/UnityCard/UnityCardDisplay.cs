/* UnityCardDisplay.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

using WarManager.Cards;


namespace WarManager.Unity3D
{
    /// <summary>
    /// The card that appears in the GUI
    /// </summary>
    public class UnityCardDisplay : MonoBehaviour, IEquatable<string>, IComparable<UnityCardDisplay>,
     IPointerClickHandler, IBeginDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The reference ID that the unity sprite adheres to when displaying behavior, animation, etc.
        /// </summary>
        public string ID
        {
            get
            {
                if (Card == null)
                {
                    throw new NullReferenceException("No card associated with the unity card display");
                }
                else
                {
                    return Card.ID;
                }
            }
        }

        [Header("Setup")]
        [SerializeField] private UnityCardBackgroundHandler _background;
        [SerializeField] private Canvas _elementsCanvas;

        [Header("Info Elements")]
        [SerializeField] private GameObject _stringElementPrefab;

        private Dictionary<string, (GameObject, ICardDisplayable)> _cardInfoElements = new Dictionary<string, (GameObject, ICardDisplayable)>();

        /// <summary>
        /// When the associated backend card was created, was is assigned to a sheet?
        /// </summary>
        public bool CardNotAssignedToSheet;

        /// <summary>
        /// The card backend refrence
        /// </summary>
        public Card Card { get; set; }

        /// <summary>
        /// The drag card class info reference
        /// </summary>
        private DragCard _drag;

        /// <summary>
        /// Used for setting up the dragging systems
        /// </summary>
        private bool _startDrag = false;

        /// <summary>
        /// Is the card currently being dragged by the mouse?
        /// </summary>
        public bool Drag_Mouse { get; set; }

        /// <summary>
        /// The card's offset location from the mouse
        /// </summary>
        public Vector2 Drag_Mouse_Offset { get; private set; }

        /// <summary>
        /// The layer of the card (may be used for later purposes)
        /// </summary>
        private int _layer = 0;

        /// <summary>
        /// Set the card to be deleted
        /// </summary>
        public bool DeleteCard { get; set; } = false;

        /// <summary>
        /// Is the card currently changing?
        /// </summary>
        public bool ActiveCard { get; set; }

        public float disableThreashold = 10;
        private bool _awayFromSheetCamera;

        //private Vector2 scaleLocation = new Vector2();

        /// <summary>
        /// is the mouse hovering over the 
        /// </summary>
        private bool _mouseOver = false;

        private bool _cardSelected = false;

        public bool start = false;

        void Update()
        {
            if (Card == null)
            {
                WarSystem.WriteToLog("Card is null", Logging.MessageType.error);
                return;
            }

            if (!ActiveCard)
                return;

            if (!CheckCardReference())
                return;

            if (DeleteCard)
                return;

            DisableThreshold();

            if (Drag_Mouse)
            {
                //Debug.Log("dragging");

                DragCard();
            }
            else
            {
                //Debug.Log("updating");

                SetSize();
                UpdatePosition();
                ActiveCard = false;
            }

            Card.CallUpdate();
        }

        /// <summary>
        /// Set the size of the rectangle background
        /// </summary>
        /// <param name="animated">should the card scale be procedurally animated?</param>
        public void SetSize(bool animated = true)
        {
            (float x, float y) size = Card.Layout.GetScaledCardDimensions();

            if (animated && !start)
            {
                _background.ScaleCard(new Vector2(size.x * 2, size.y * 2));
            }
            else
            {
                _background.ScaleCard(new Vector2(size.x * 2, size.y * 2));
            }
        }

        /// <summary>
        /// Remove the unity card from the sheet
        /// </summary>
        public void RemoveCardFromSheet()
        {
            DeleteCard = true;
            Destroy(this.gameObject);
        }

        /// <summary>
        /// turns off the card depending on the distance away from the camera
        /// </summary>
        void DisableThreshold()
        {
            Vector3 camLoc = WarManagerDriver.Main.SheetCamera.transform.position;

            Vector3 dist = new Vector3(camLoc.x - transform.position.x, camLoc.y - transform.position.y, 0);

            float Fdist = dist.sqrMagnitude;

            if (Fdist > disableThreashold)
            {
                //_background.gameObject.SetActive(false);
                _awayFromSheetCamera = true;

                if (_awayFromSheetCamera) //to remove warnings in the compiler -> not used for anything specific
                {

                }
            }
            else
            {
                //_background.gameObject.SetActive(true);
                _awayFromSheetCamera = false;
            }

        }

        /// <summary>
        /// Flash the border of the unity card display
        /// </summary>
        /// <param name="active">set the unity card border flash active</param>
        /// <param name="col">the color of the border</param>
        public void FlashBorder(bool active, Color col)
        {
            _background.FlashingBorder = active;
            _background.SetBorderColor(col);
        }

        /// <summary>
        /// Update the position of the card
        /// </summary>
        void UpdatePosition()
        {
            //Vector2 loc = SheetDriver.ConvertTupleToVector2(Card.Layout.GetCardGlobalLocation(SheetDriver.Main.GetGlobalOffsetTuple(), SheetDriver.Main.GetCardMultiplierTuple()));

            // if (new Vector3(loc.x, loc.y, transform.position.z) != transform.position)
            // {
            //     if (start)// || _awayFromSheetCamera)
            //     {
            //         transform.position = Vector3.Lerp(transform.position, new Vector3(loc.x, loc.y, transform.position.z + _layer), .4f);
            //     }
            //     else
            //     {
            //         LeanTween.move(this.gameObject, new Vector3(loc.x, loc.y, transform.position.z + _layer), SheetDriver.Main.CardSpeed).setEaseOutBack();
            //     }
            // }
        }

        /// <summary>
        /// Handles dragging the card around the scene
        /// </summary>
        private void DragCard()
        {
            //Debug.Log("dragging");

            ActiveCard = true;

            //if (!SheetDriver.Main.CompareHoverCard(this))
            //	return;

            //Debug.Log("still dragging--");

            if (ToolsManager.SelectedTool != ToolTypes.Edit)
            {
                OnMouseExit();
                return;
            }

            if (_startDrag && Card != null)
            {
                _drag = new DragCard(Card);
                _startDrag = false;
            }

            Debug.Log("dragging" + ID);

            Vector3 newLoc = WarManagerDriver.Main.SheetCamera.ScreenToWorldPoint(Input.mousePosition);
            newLoc = new Vector3(newLoc.x, newLoc.y, transform.position.z);

            Vector3 offset = Drag_Mouse_Offset;

            transform.position = new Vector3(newLoc.x + offset.x, newLoc.y + offset.y, -2f);

            UnityGUIDriver.SetPointMarkersActive(transform, true);
            _drag.Drag(WarManagerDriver.ConvertVector2ToTuple(transform.position), Card.Layer, WarManagerDriver.Main.GetGlobalOffsetTuple(), WarManagerDriver.Main.GetCardMultiplierTuple());

            Card.SetCardDrag(Drag_Mouse);
        }

        #region Card UI Elements
        /// <summary>
        /// Instantiate a new element to the card
        /// </summary>
        /// <param name="element"></param>
        public void SetNewElement(ICardDisplayable element)
        {
            switch (element.DisplayType)
            {
                case CardDisplayType.stringDisplay:
                    GameObject go = Instantiate(_stringElementPrefab, _elementsCanvas.transform) as GameObject;

                    TMPro.TMP_Text strElemnt = go.GetComponent<TMPro.TMP_Text>();

                    CardTextDisplay textDisplay = (CardTextDisplay)element.GetDetails();

                    go.transform.localScale = new Vector3(textDisplay.Scale * .025f, textDisplay.Scale * .025f, 1);
                    go.transform.localPosition = new Vector3(element.DisplayLocation.Item1, element.DisplayLocation.Item2, transform.position.z - .001f);

                    strElemnt.text = textDisplay.Text;

                    (float, float, float, float) ColorRGBA = textDisplay.GetNormalizedRGBA();

                    strElemnt.color = new Color(ColorRGBA.Item1, ColorRGBA.Item2,
                        ColorRGBA.Item3, ColorRGBA.Item4);

                    TMPro.TMP_Text txt = go.GetComponent<TMPro.TMP_Text>();

                    txt.fontSize = textDisplay.FontSize;

                    switch (textDisplay.Alignment)
                    {
                        case TextAlignment.Center:
                            txt.alignment = TMPro.TextAlignmentOptions.Center;
                            break;

                        case TextAlignment.LeftJustified:
                            txt.alignment = TMPro.TextAlignmentOptions.Left;
                            break;

                        case TextAlignment.RightJustified:
                            txt.alignment = TMPro.TextAlignmentOptions.Right;
                            break;

                        case TextAlignment.Justified:
                            txt.alignment = TMPro.TextAlignmentOptions.Justified;
                            break;
                    }

                    //TODO: support different fonts

                    if (textDisplay.Bold)
                    {
                        if (textDisplay.Underline)
                        {
                            if (textDisplay.Italic)
                            {
                                txt.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.Underline | TMPro.FontStyles.Italic;
                            }
                            else
                            {
                                txt.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.Underline;
                            }
                        }
                        else
                        {
                            txt.fontStyle = TMPro.FontStyles.Bold;
                        }

                    }
                    else if (textDisplay.Underline)
                    {
                        if (textDisplay.Italic)
                        {
                            txt.fontStyle = TMPro.FontStyles.Underline | TMPro.FontStyles.Italic;
                        }
                        else
                        {
                            txt.fontStyle = TMPro.FontStyles.Underline;
                        }
                    }
                    else if (textDisplay.Italic)
                    {
                        txt.fontStyle = TMPro.FontStyles.Underline | TMPro.FontStyles.Italic;
                    }
                    else
                    {
                        txt.fontStyle = TMPro.FontStyles.Normal;
                    }

                    _cardInfoElements.Add(element.ID, (go, element));
                    break;
            }
        }


        /// <summary>
        /// Sets the background color to the given tuple RGBA normalized color
        /// </summary>
        /// <param name="color">RGBA normalized tuple (float, float, float, float)</param>
        public void SetBackgroundImage((float r, float g, float b, float a) color)
        {
            _background.SetBackgroundColor(new Color(color.r, color.b, color.g, color.a));
        }

        #endregion

        /// <summary>
        /// The pointer has entered the card
        /// </summary>
        private void EnterCard()
        {
            if (ToolsManager.SelectedTool != ToolTypes.Edit)
            {
                OnMouseExit();
                return;
            }

            if (Card == null)
            {
                Card = CardUtility.GetCard(ID);

                if (Card == null)
                    return;
            }

            Debug.Log("attempting to hover");

            if (Card.Animator.CurrentAnimationState == CardAnimationState.Start)
                Card.Animator.SetCardIdle();

            transform.position = new Vector3(transform.position.x, transform.position.y, -5);

            Debug.Log("Hovering" + ID);

            ActiveCard = true;

            if (!WarManagerDriver.Main.AddHoverCard(this))
            {
                Card.Animator.SetCardHover();
                _mouseOver = true;

                if (_mouseOver)  //used to get rid of warnings -> not used for anything specific
                {

                }

                return;
            }

        }

        /// <summary>
        /// The pointer has left the card
        /// </summary>
        private void ExitCard()
        {
            if (Card == null)
                return;

            var animator = Card.Animator;
            animator.SetCardIdle();

            if (!Drag_Mouse)
            {
                WarManagerDriver.Main.RemoveHoverCard(this);
            }

            Debug.Log("Exit" + ID);

            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            _mouseOver = false;
        }

        /// <summary>
        /// Select the card in the sheet editor
        /// </summary>
        public void SelectCard(bool deselectPreviousSelected = true)
        {
            bool deselectPrevious = deselectPreviousSelected;

            if (deselectPreviousSelected)
                deselectPrevious = !Input.GetKey(KeyCode.LeftControl);


            if (_cardSelected)
                return;

            WarSystem.WriteToLog("attempting to select card", Logging.MessageType.info);

            CardUtility.AddCardToUpdateQueue(Card);
            ActiveCard = true;

            (float r, float g, float b) c = GeneralSettings.Select_Card_Color;

            _background.SetSolidBorder(.3f, new Color(c.r, c.g, c.b));
            WarManagerDriver.Main.AddSelectedCard(this, deselectPrevious);
            _cardSelected = true;

            Debug.Log("Selected_Card" + ID);
            WarSystem.WriteToLog("card was selected", Logging.MessageType.logEvent);
        }

        /// <summary>
        /// Deselect the card in the sheet editor
        /// </summary>
        public void DeselectCard()
        {
            if (_cardSelected)
            {
                _background.SolidBorder = false;
                _cardSelected = false;
                ActiveCard = false;

                if (Drag_Mouse)
                {
                    StopDragging();
                }

                Debug.Log("Card Deselected" + ID);
            }
        }

        /// <summary>
        /// Checks to see if the card can be referenced
        /// </summary>
        private bool CheckCardReference()
        {
            if (Card == null)
            {
                if (Drag_Mouse)
                {
                    if (_drag != null)
                        Card = _drag.CardBeingDragged;
                }
                else
                {
                    Card = CardUtility.GetCard(ID);
                }
            }

            if (Card == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// The card has started being dragged
        /// </summary>
        public void OnStartDragging()
        {
            SelectCard();

            Vector3 pos = Input.mousePosition;
            pos = new Vector3(pos.x, pos.y, transform.position.z);
            Drag_Mouse_Offset = transform.position - Camera.main.ScreenToWorldPoint(pos);

            _startDrag = true;

            ActiveCard = true; //IF YOU ARE HAVING SILENT CRASHES, its because the card is not being placed in a card list??
            WarSystem.WriteToLog("attempting to close windows", Logging.MessageType.info);
            SlideWindowsManager.main.CloseWindows();

            Debug.Log("Drag_Start" + ID);
            WarSystem.WriteToLog("drag start", Logging.MessageType.logEvent);
        }

        /// <summary>
        /// The card has stopped being dragged
        /// </summary>
        public void StopDragging()
        {
            if (_drag != null)
            {
                _drag.EndDrag();
                _drag = null;

                Drag_Mouse = false;

                _startDrag = false;
                UnityGUIDriver.SetPointMarkersActive(transform, false);

                Debug.Log("Drag_Stop" + ID);

                CardUtility.ConnectCards();
            }
        }

        public bool Equals(string other) //for IEquatable
        {
            return ID.Equals(other);
        }

        public int CompareTo(UnityCardDisplay other)
        {
            return string.Compare(other.ID, ID);
        }

        public void OnEnable()
        {
            CardSelectionHandler<UnityCardDisplay>.OnDeselect += DeselectCard;
        }

        public void OnDisable()
        {
            CardSelectionHandler<UnityCardDisplay>.OnDeselect -= DeselectCard;
        }

        #region MouseInteraction

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Drag_Mouse)
                SelectCard();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnStartDragging();

            foreach (var disp in WarManagerDriver.Main.SelectHandler.SelectedCards)
            {
                if (disp.GUICard != this)
                {
                    disp.GUICard.EnterCard();
                    disp.GUICard.OnStartDragging();
                }
            }

        }

        public void OnDrag(PointerEventData eventData)
        {
            Drag_Mouse = true;

            foreach (var disp in WarManagerDriver.Main.SelectHandler.SelectedCards)
            {
                if (disp.GUICard != this)
                    disp.GUICard.Drag_Mouse = true;
            }

        }

        public void OnDrop(PointerEventData eventData)
        {
            StopDragging();

            foreach (var disp in WarManagerDriver.Main.SelectHandler.SelectedCards)
            {
                if (disp.GUICard != this)
                {
                    disp.GUICard.StopDragging();
                    disp.GUICard.ExitCard();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EnterCard();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ExitCard();
        }

        #endregion

        #region LegacyMouseInteraction
        public void OnMouseOver() //detect mouse hovering over a card
        {


        }

        public void OnMouseExit() // detect mouse leaving the boundary of a card
        {

        }

        public void OnMouseDrag() // detect mouse dragging card
        {

        }

        public void OnMouseDown()//detect mouse button down
        {

        }

        public void OnMouseUp() //detect mouse button up
        {

        }

        #endregion
    }
}
