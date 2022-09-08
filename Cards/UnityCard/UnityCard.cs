/* UnityCard.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Cards;
using WarManager.Backend;

using Sirenix;
using Sirenix.OdinInspector;
using WarManager.Unity3D.Windows;

using StringUtility;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Unity Card display 2.0
    /// </summary>
    [RequireComponent(typeof(UnityCardBackgroundController))]
    [RequireComponent(typeof(UnityCardElementsManager))]
    public class UnityCard : MonoBehaviour, IEquatable<UnityCard>, IComparable<UnityCard>
    {

        [SerializeField] private string dispId = "";


        /// <summary>
        /// The id of the unity card (which is the same as the card being tracked)
        /// </summary>
        /// <value></value>
        [TabGroup("System")]
        public string ID
        {
            get
            {
                if (Card != null)
                {
                    dispId = Card.ID;
                    return Card.ID;
                }
                else
                {
                    return null;
                }
            }
        }
        [TabGroup("Other")]
        public Vector2 CardHoverSize = new Vector2(.25f, .25f);

        [TabGroup("Other")]
        /// <summary>
        /// The reference to the camera in the scene
        /// </summary>
        public WarManagerCameraController WarCamera;

        /// <summary>
        /// Handles the sheet grid
        /// </summary>
        public WarGrid Grid;

        [TabGroup("Border Colors")]
        public Color BorderSelectedColor;

        [TabGroup("Border Colors")]
        public Color Border_FindColor;

        [TabGroup("Border Colors")]
        public Color Border_LockedColor;

        [TabGroup("Border Colors")]
        public Color Border_CardGroupedColor = Color.green;

        /// <summary>
        /// The card the unity card display is tracking
        /// </summary>
        /// <value></value>
        public Card Card { get; private set; }
        private bool mouseDown = false;

        Vector2 mouseCardDragOffset = Vector3.zero;
        private bool canDrag;

        [TabGroup("System")]
        /// <summary>
        /// Handles highlighting the card when the find tool is being utilized
        /// </summary>
        public bool CardFound = false;
        private bool mouseHover = false;

        public UnityCardBackgroundController BackgroundController;
        private UnityCardElementsManager _elementsManager;

        /// <summary>
        /// Initialize the UnityCard
        /// </summary>
        /// <param name="value">the card to track</param>
        public void ResetCard(Card value)
        {
            Card = value;

            if (value != null && !value.CardRepresented)
            {
                SetBorderColor(false, Color.clear);
                Card.RemoveCallBack = DeleteCard;
                Card.CardSelectedCallBack = SelectCard;
                Card.LockCallBack = LockCard;
                value.CardHideCallBack = HideCard;
                value.CardShiftCallback = ShiftCard;
                value.UpdateCallback = UpdateCard;
                value.DragCard = SetCardDrag;
                value.Action_SetUICardToInputLocation = SetCardToInputLocation;


                //if (Card.CardStretched)
                //{
                //    foreach (var card in Card.GroupStretchCardsList)
                //    {
                //        card.CardRepresented = true;
                //    }
                //}

                UpdateDisplay(CardMovementBehavior.Instant);

                if (_elementsManager == null)
                    _elementsManager = GetComponent<UnityCardElementsManager>();

                _elementsManager.RefreshCardIdentity(this, Card.SheetID);
            }
            else
            {
                HandleCardVisibility();
            }
        }

        #region event/delegate called card update systems

        /// <summary>
        /// Check to see if the card is representing a backend reference
        /// </summary>
        private bool HandleCardVisibility()
        {
            if (Card == null)
            {
                // if (dragging)
                //     OnEndDrag();

                gameObject.SetActive(false);
                return false;
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);


            return true;
        }

        /// <summary>
        /// update the card
        /// </summary>
        /// <param name="LOD">how detailed should the update process be?</param>
        public void UpdateDisplay(CardMovementBehavior behavior)
        {
            if (!HandleCardVisibility())
                return;

            Pointf position = new Pointf(0, 0);


            if (behavior == CardMovementBehavior.Instant || behavior == CardMovementBehavior.Twean)
            {
                if (Card.CardStretched)
                {
                    position = SetStretch(Card.CardStretchBounds);
                }
                else
                {
                    position = Point.GridToWorld(Card.point, Grid);
                }
            }

            switch (behavior)
            {
                case CardMovementBehavior.Twean:

                    MoveCardToGivenPosition(new Vector2(position.x, position.y), false, 0);
                    break;

                case CardMovementBehavior.Instant:

                    MoveCardToGivenPosition(new Vector2(position.x, position.y), true, 0);
                    break;

                case CardMovementBehavior.Drag:

                    //Debug.Log("Dragging");

                    Vector3 mousePos = WarCamera.GetCamera.ScreenToWorldPoint(Input.mousePosition);

                    Vector2 final = mousePos - new Vector3(mouseCardDragOffset.x, mouseCardDragOffset.y, 0);

                    MoveCardToGivenPosition(final, true, -4);
                    break;

                default:

                    break;
            }

            UpdateBorder();

            if (behavior == CardMovementBehavior.FrontEnd)
            {
                //Debug.Log("updating front end");
                _elementsManager.TriggerLoad();
            }
        }


        /// <summary>
        /// Move the card to its specified backend position
        /// </summary>
        /// <param name="instant">should the card be tweened or instantly appear in it's specified location?</param>
        /// <param name="zValue">how far forward (or backward) on the z axis must the card object be?</param>
        private void MoveCardToGivenPosition(Vector2 position, bool instant, float zValue)
        {
            if (Card == null)
                return;

            if (instant)
            {
                transform.position = new Vector3(position.x, position.y, zValue);
            }
            else
            {
                LeanTween.cancel(gameObject);
                LeanTween.move(gameObject, new Vector3(position.x, position.y, zValue), .25f).setEaseOutExpo();
            }
        }

        public void UpdateBorder()
        {
            if (Card == null)
                return;

            if (Card.Selected)
            {
                if (!CardFound)
                {
                    SetBorderColor(true, BorderSelectedColor);
                }
            }
            else
            {
                if (CardFound)
                {
                    SetBorderColor(true, Border_FindColor);
                }
                else if (Card.CardLocked)
                {
                    SetBorderColor(true, Border_LockedColor);
                }
                else if (Card.Grouped)
                {
                    SetBorderColor(true, Border_CardGroupedColor);
                }
                else
                {
                    SetBorderColor(true, Color.clear);
                    // SetBorderColor(false);
                }

            }
        }

        /// <summary>
        /// Set the border color/visibility
        /// </summary>
        /// <param name="active">set the border to be active or disabled</param>
        /// <param name="color">set the color of the border</param>
        public void SetBorderColor(bool active, Color color = default(Color))
        {
            if (color != default(Color))
            {
                BackgroundController.BorderColor = Color.blue;
            }

            if (BackgroundController == null)
                BackgroundController = GetComponent<UnityCardBackgroundController>();

            BackgroundController.BorderColor = color;
            BackgroundController.BorderActive = active;
        }


        /// <summary>
        /// Set the stretch size of the card
        /// </summary>
        /// <param name="rect">the rectangle information</param>
        public Pointf SetStretch(Rect rect)
        {
            BackgroundController._unityCard = this;

            BackgroundController.StretchInfo = rect;
            BackgroundController.Refresh();

            return rect.GetWorldCenter(Grid.Offset, Grid.GridScale);
        }

        #endregion

        void Update()
        {
            if (Card == null)
                return;

            if (BackgroundController == null)
                BackgroundController = GetComponent<UnityCardBackgroundController>();


            if (_elementsManager == null)
                _elementsManager = GetComponent<UnityCardElementsManager>();


            if (!Card.CanLockOrUnlock && Card.CardLocked)
                Card.Deselect();


            if (Card.CardDragging && canDrag)
            {
                DragCard();
            }
        }

        public void PressDown()
        {
            if (DrawRectOnCanvas.MouseContextStatus == MouseStatus.toolBar || DrawRectOnCanvas.MouseContextStatus == MouseStatus.contextMenu || DrawRectOnCanvas.MouseContextStatus == MouseStatus.menu)
                return;

            if (Card.CardDragging || ToolsManager.SelectedTool == ToolTypes.None || ToolsManager.SelectedTool == ToolTypes.Pan)
                return;

            if (Card.Selected)
            {
                Card.StartDrag(false);
            }
        }

        public void PressUp()
        {
            if (Card.Selected)
            {
                Card.EndDrag(false);
            }
        }

        public void HoverStart()
        {
            if (DrawRectOnCanvas.MouseContextStatus == MouseStatus.toolBar || DrawRectOnCanvas.MouseContextStatus == MouseStatus.contextMenu || DrawRectOnCanvas.MouseContextStatus == MouseStatus.menu)
                return;

            if (Card.CardDragging || ToolsManager.SelectedTool == ToolTypes.None || ToolsManager.SelectedTool == ToolTypes.Pan)
                return;

            mouseHover = true;

            if (Card != null && !Card.CardLocked)
            {
                if (BackgroundController == null)
                    BackgroundController = GetComponent<UnityCardBackgroundController>();
                BackgroundController.AnimateBackgroundSize(CardHoverSize, true);
            }
        }

        public void HoverEnd()
        {
            if (Card.CardDragging)
                return;

            mouseHover = false;

            if (Card != null)
            {

                BackgroundController.AnimateBackgroundSize(Vector2.zero, true);
            }
        }

        #region Actions

        /// <summary>
        /// Show card selected/not selected
        /// </summary>
        /// <param name="selected">has the card been selected</param>
        private void SelectCard(bool selected, Card c)
        {
            UpdateDisplay(CardMovementBehavior.Twean);

            Vector2 position = transform.position;
            if (Card != null)
            {
                Card = c;
            }

            if (Card != null)
            {
                Card.SetCardFrontEndPosition(new Pointf(position.x, position.y));
            }
        }


        /// <summary>
        /// Shift a card to a new location
        /// </summary>
        /// <param name="newPosition">the new position</param>
        private void ShiftCard(Point newPosition)
        {
            UpdateDisplay(CardMovementBehavior.Twean);
        }

        /// <summary>
        /// Set the card to be locked
        /// </summary>
        /// <param name="locked"></param>
        private void LockCard(bool locked)
        {
            UpdateDisplay(CardMovementBehavior.Instant);
        }

        /// <summary>
        /// Hide the card
        /// </summary>
        /// <param name="hidden"></param>
        private void HideCard(bool hidden)
        {
            UpdateDisplay(CardMovementBehavior.Instant);
        }

        /// <summary>
        /// Used to update the card (callback)
        /// </summary>
        /// <param name="lod"></param>
        private void UpdateCard(int x)
        {
            UpdateDisplay((CardMovementBehavior)x);
        }

        /// <summary>
        /// Set the card to drag status
        /// </summary>
        /// <param name="move"></param>
        /// <param name="c"></param>
        private void SetCardDrag(bool move, Card c)
        {
            if (move)
            {
                if (!canDrag)
                {
                    SetStartDrag();
                }
            }
            else
            {
                SetStopDrag();
            }
        }

        /// <summary>
        /// Prepare the card for dragging
        /// </summary>
        public void SetStartDrag()
        {

            Vector2 cameraPosition = WarCamera.GetCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseCardDragOffset = cameraPosition - new Vector2(transform.position.x, transform.position.y);

            Vector3 location = transform.position;
            Card.SetCardFrontEndPosition(new Pointf(location.x, location.y));

            canDrag = true;
        }


        /// <summary>
        /// Set the location of the GUI card to the position of the input
        /// </summary>
        public void SetCardToInputLocation()
        {
            if (Input.touchCount <= 0 && Input.mousePresent)
            {
                Vector3 mouseLocation = Input.mousePosition;
                mouseLocation.z = WarCamera.GetCamera.nearClipPlane + 1;

                this.gameObject.transform.position = WarCamera.GetCamera.ScreenToWorldPoint(mouseLocation);
            }
            else if (Input.touchCount > 0)
            {
                this.gameObject.transform.position = WarCamera.GetCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            else
            {
                throw new System.Exception("No input present");
            }
        }

        /// <summary>
        /// Drag the card
        /// </summary>
        public void DragCard()
        {
            UpdateDisplay(CardMovementBehavior.Drag);

            Vector3 location = transform.position;
            Card.SetCardFrontEndPosition(new Pointf(location.x, location.y));
        }

        /// <summary>
        /// End dragging
        /// </summary>
        public void SetStopDrag()
        {
            canDrag = false;
            UpdateDisplay(CardMovementBehavior.Twean);

            Vector3 location = transform.position;
            Card.SetCardFrontEndPosition(new Pointf(location.x, location.y));
        }

        /// <summary>
        /// called when someone double clicks or double taps on a card
        /// </summary>
        public void DoubleClick()
        {
            if (Input.touchCount > 0)
            {
                ActivatePickMenu_TouchScreen();
            }
            else
            {
                try
                {
                    if (Card.DataSet.SelectedView.CanViewCard)
                    {
                        if(Card.DataSet.SelectedView.CanEditCard)
                        {
                            if(!CardUtility.GetSelectedCardsOnCurrentSheet().Contains(Card))
                            {
                                CardUtility.SelectCard(Card);
                            }
                        }

                        DataSetViewer.main.ShowDataEntryInfo(Card.Entry, () =>
                        {
                            if (Card != null)
                                DataSetViewer.main.ShowDataSet(Card.Entry.DataSet.ID, ActiveSheetsDisplayer.main.ViewReferences);
                        }, Card.Entry.DataSet.DatasetName.SetStringQuotes());
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBoxHandler.Print_Immediate("Could not show card details " + ex.Message, "Error");
                }
            }
        }

        public void ActivatePickMenu_TouchScreen()
        {
            GhostCardBehavior.Main.SendResultsTouchScreen(1);
        }

        /// <summary>
        /// Delete the card
        /// </summary>
        private void DeleteCard()
        {
            StartCoroutine(DeleteCardCoroutine());
        }

        /// <summary>
        /// The coroutine to delete the card - fires an animation and removes the card from the sheet
        /// </summary>
        /// <returns></returns>
        IEnumerator DeleteCardCoroutine()
        {
            _elementsManager.ClearElements();
            BackgroundController.AnimateBackgroundSize(new Vector2(-2.5f, -2f), true, .25f, false, false);
            yield return new WaitForSeconds(.3f);
            Card = null;
            UpdateDisplay(CardMovementBehavior.Instant);
        }

        #endregion

        public void CheckTool(ToolTypes toolType)
        {
            if (toolType == ToolTypes.None || toolType == ToolTypes.Pan)
            {
                if (mouseHover)
                    HoverEnd();
            }
        }

        public void ChangeSheets(string str)
        {
            if (Card != null)
            {
                if (str != Card.SheetID)
                {
                    // if (dragging)
                    //     OnEndDrag();
                }
            }
        }

        void DetectMouse(MouseChangeEventSystem system, UnityEngine.EventSystems.PointerEventData data)
        {
            switch (system)
            {
                case MouseChangeEventSystem.mouseClick:
                    Card.ToggleSelect();
                    break;

                default:

                    break;
            }
        }

        void OnEnable()
        {
            //SheetMouseDetection.OnChangeMouse += DetectMouse;
            ToolsManager.OnToolSelected += CheckTool;
            SheetsManager.OnSetSheetCurrent += ChangeSheets;

        }

        void OnDisable()
        {
            //do stuff here to make sure the removal is a clean one...

            //SheetMouseDetection.OnChangeMouse -= DetectMouse;
            ToolsManager.OnToolSelected -= CheckTool;
            SheetsManager.OnSetSheetCurrent -= ChangeSheets;

            if (BackgroundController != null)
                BackgroundController.AnimateBackgroundSize(Vector3.one, false, 0, false, false);
        }


        #region Comparison
        public int CompareTo(UnityCard other)
        {
            return ID.CompareTo(other.ID);
        }

        public int CompareX(float x)
        {
            return Card.CompareX(x);
        }

        public int CompareY(float y)
        {
            return Card.CompareY(y);
        }

        public int CompareLayer(Layer layer)
        {
            return Card.CompareLayer(layer);
        }

        public bool Equals(UnityCard other)
        {
            if (other == null)
                return false;

            return other.ID == ID;
        }
        #endregion

    }

    /// <summary>
    /// The different behaviors the card will act on
    /// </summary>
    public enum CardMovementBehavior
    {
        Instant,
        Twean,
        Drag,
        FrontEnd,
    }
}
