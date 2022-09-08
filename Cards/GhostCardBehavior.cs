using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class GhostCardBehavior : MonoBehaviour
    {
        /// <summary>
        /// The location of the ghost card
        /// </summary>
        /// <value></value>
        public Point Location { get; private set; }

        /// <summary>
        /// The scale bounds of the ghost card
        /// </summary>
        /// <value></value>
        public Rect Bounds { get; private set; }

        [SerializeField] private WarManagerCameraController cameraController;

        [SerializeField] private SpriteRenderer Renderer;

        [SerializeField] private TMPro.TMP_Text LocationXText;
        [SerializeField] private TMPro.TMP_Text LocationYText;
        [SerializeField] private RectTransform InfoCanvas;

        [SerializeField] private Sprite Normal, Top, Left, Corner;

        /// <summary>
        /// Handles the sheet grid
        /// </summary>
        private WarGrid _grid;

        [SerializeField] Vector2 _gridOffset;

        private Point _mouseDownLoc;
        private Point _mouseUpLoc;

        private int moveID, scaleId;

        [SerializeField] private GhostCardBehaviorState _currentState;

        private GhostCardBehaviorState CurrentState
        {
            get => _currentState;

            set
            {
                _currentState = value;
            }
        }

        [SerializeField] private QuickSearchUIController QuickSearchUIController;

        private bool _usingKeyboard = false;

        private Vector3 prevLocation = Vector3.zero;
        private Vector3 currentLocation = Vector3.zero;
        private bool MouseMoved = false;

        #region  Singleton
        public static GhostCardBehavior Main;
        public void Awake()
        {
            if (Main != null)
            {
                Debug.LogError("You cannot have multiple ghost cards in on scene");
                Destroy(this.gameObject);
            }
            else
            {
                Main = this;
            }

            currentLocation = Input.mousePosition;
        }

        #endregion

        /// <summary>
        /// private backing field for unity
        /// </summary>

        [SerializeField] private bool _ghostCardVisible;

        /// <summary>
        /// Can the Ghost card be visible?
        /// </summary>
        /// <value></value>
        public bool GhostCardVisible { get { return _ghostCardVisible; } set { _ghostCardVisible = value; } }

        private bool drawStart = false;

        // Update is called once per frame
        void Update()
        {


            currentLocation = Input.mousePosition;

            if (_grid != null && SheetsManager.SheetCount > 0)
            {
                _gridOffset = new Vector2(_grid.GridScale.x, _grid.GridScale.y);
            }
            else
            {
                CurrentState = GhostCardBehaviorState.invisible;
                GhostCardVisible = false;
            }

            MouseInput();

            if (CurrentState == GhostCardBehaviorState.invisible || !GhostCardVisible)
            {
                drawStart = false;
                Renderer.gameObject.SetActive(false);

                return;
            }
            else
            {
                Renderer.gameObject.SetActive(true);
            }

            if (CurrentState == GhostCardBehaviorState.draw)
            {
                if (!drawStart)
                {
                    _mouseDownLoc = GetMKGridPosition();
                    drawStart = true;
                }
                DrawGhostCard();
                SetDragText();
            }

            if (CurrentState == GhostCardBehaviorState.visible)
            {
                drawStart = false;

                KeyboardInput();

                QuickSearchUIController.ToggleSetLocationOfInterest(_usingKeyboard);
                QuickSearchUIController.SetLocationOfInterest(Location);

                Point p = GetMKGridPosition();

                MoveGhostCard(p);
                SetLocationText(p);

                ScaleGhostCard(_grid.GridScale);
            }
        }

        private void KeyboardInput()
        {
            bool hitKeys = false;


            if (Input.GetKeyUp(KeyCode.UpArrow))
            {

                hitKeys = true;

                if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    MoveGhostCard(Location + Point.up);
                }
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) && (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)))
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        CardUtility.SwapOrShiftCard(Location, Point.up, sheet.CurrentLayer, sheet, true);
                        MoveGhostCard(Location + Point.up);
                    }
                }
                else
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        var card = CardUtility.GetCard(Location, sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                            CardUtility.TryShiftCard(card, Point.up, 1, true);

                        MoveGhostCard(Location + Point.up);
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {

                hitKeys = true;

                if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    MoveGhostCard(Location + Point.down);
                }
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) && (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)))
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        CardUtility.SwapOrShiftCard(Location, Point.down, sheet.CurrentLayer, sheet, true);
                        MoveGhostCard(Location + Point.down);
                    }
                }
                else
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        var card = CardUtility.GetCard(Location, sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                            CardUtility.TryShiftCard(card, Point.down, 1, true);

                        MoveGhostCard(Location + Point.down);
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow))
            {

                hitKeys = true;

                if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    MoveGhostCard(Location + Point.right);
                }
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) && (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)))
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        CardUtility.SwapOrShiftCard(Location, Point.right, sheet.CurrentLayer, sheet, true);
                        MoveGhostCard(Location + Point.right);
                    }
                }
                else
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        var card = CardUtility.GetCard(Location, sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                            CardUtility.TryShiftCard(card, Point.right, 1, true);

                        MoveGhostCard(Location + Point.right);
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
            {

                hitKeys = true;

                if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    MoveGhostCard(Location + Point.left);
                }
                else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) && ((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))))
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        CardUtility.SwapOrShiftCard(Location, Point.left, sheet.CurrentLayer, sheet, true);
                        MoveGhostCard(Location + Point.left);
                    }
                }
                else
                {
                    if (SheetsManager.TryGetCurrentSheet(out var sheet))
                    {
                        var card = CardUtility.GetCard(Location, sheet.CurrentLayer, sheet.ID);

                        if (card != null)
                            CardUtility.TryShiftCard(card, Point.left, 1, true);

                        MoveGhostCard(Location + Point.left);
                    }
                }
            }


            if (hitKeys)
            {
                _usingKeyboard = true;
                WarManagerCameraController.MainController.MoveCamera(Location, 20);
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                _usingKeyboard = false;
            }
        }

        private void MouseInput()
        {
            bool interact = false;

            if (!GhostCardVisible || SheetsManager.SheetCount < 1 || (DrawRectOnCanvas.MouseContextStatus != MouseStatus.editTool &&
    DrawRectOnCanvas.MouseContextStatus != MouseStatus.selectTool && DrawRectOnCanvas.MouseContextStatus != MouseStatus.calculateTool) || Input.touchCount > 1)
            {
                interact = false;
            }
            else
            {
                interact = true;
            }

            CheckMouseMoved();

            if (!interact)
            {
                CurrentState = GhostCardBehaviorState.invisible;
                return;
            }

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                CurrentState = GhostCardBehaviorState.draw;
                return;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                SendResults(0);
                return;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                SendResults(1);
                return;
            }



            CurrentState = GhostCardBehaviorState.visible;
        }

        /// <summary>
        /// Card was dragged around the area
        /// </summary>
        public void SendResults(int btn) // calculate the amount of spaces gathered -> if it is more than one, then it was a drag, otherwise it was a 'click'
        {
            GhostCardManager.Results(Bounds, btn);
        }

        /// <summary>
        /// Card was dragged around the area (touch screen input)
        /// </summary>
        /// <param name="btn"></param>
        public void SendResultsTouchScreen(int btn)
        {
            if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                return;

            GhostCardManager.Results(Bounds, btn);
        }

        private void SetLocationText(Point location)
        {
            if (location.IsInGridBounds)
            {
                LocationXText.text = location.x.ToString();
                LocationYText.text = location.y.ToString();
            }
            else
            {
                LocationXText.text = "<size=5>Out of range</size> - " + Point.GetNearestValidPoint(location).x.ToString();
                LocationYText.text = "<size=5>Out of range</size> - " + Point.GetNearestValidPoint(location).y.ToString();
            }
        }

        private void SetDragText()
        {
            LocationXText.text = "";
            LocationYText.text = "";
        }

        /// <summary>
        /// Get the mouse and keyboard position in grid coordinates
        /// </summary>
        /// <returns></returns>
        private Point GetMKGridPosition()
        {
            if (_usingKeyboard)
                return Location;

            Vector3 mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = cameraController.GetCamera.nearClipPlane;

            Vector3 loc = cameraController.GetCamera.ScreenToWorldPoint(mousePos);

            return Point.WorldToGrid(new Pointf(loc.x, loc.y), _grid);
        }

        private Vector2 GetWorldPosition(Point p)
        {
            Pointf worldPos = Pointf.GridToWorld(p, _grid);

            return new Vector2(worldPos.x, worldPos.y);
        }

        /// <summary>
        /// Move the ghost card
        /// </summary>
        private void MoveGhostCard(Point location)
        {
            if (!location.IsInGridBounds)
            {
                location = Point.GetNearestValidPoint(location);

            }

            SetSprite(location);

            if (location != Location)
            {
                Location = location;

                Vector2 worldLocation = GetWorldPosition(location);

                MoveCard(new Vector3(worldLocation.x, worldLocation.y, 5));
            }
        }

        private void CheckMouseMoved()
        {
            if (prevLocation != currentLocation)
            {
                prevLocation = currentLocation;
                MouseMoved = true;
                _usingKeyboard = false;
            }
        }

        private void SetSprite(Point location)
        {
            if (location.x <= 0)
            {
                if (location.y >= 0)
                {
                    Renderer.sprite = Corner;
                }
                else
                {
                    Renderer.sprite = Left;
                }

            }
            else
            {
                if (location.y >= 0)
                {
                    Renderer.sprite = Top;
                }
                else
                {
                    Renderer.sprite = Normal;
                }
            }
        }

        /// <summary>
        /// Move the card to a newly selected position
        /// </summary>
        /// <param name="position">the vector3 position</param>
        private void MoveCard(Vector3 position)
        {
            LeanTween.cancel(moveID);

            if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
            {
                LTDescr d = LeanTween.move(Renderer.gameObject, position, .125f).setEaseOutCubic();
                moveID = d.id;
            }
            else
            {
                Renderer.gameObject.transform.position = position;
            }
        }

        private void DrawGhostCard()
        {
            Point final = GetMKGridPosition();

            Rect r = Rect.DrawRect(_mouseDownLoc, final);
            Bounds = r;

            SetSprite(Bounds.TopLeftCorner);

            Pointf locCenter = r.GetWorldCenter(_grid.Offset, _grid.GridScale);

            Vector2 center = new Vector2(locCenter.x, locCenter.y);

            MoveCard(center);

            float x = Point.GridToWorld(r.Width, _grid.Offset.x, _grid.GridScale.x) + _grid.GridScale.x;
            float y = Point.GridToWorld(r.Height, _grid.Offset.y, _grid.GridScale.y) + _grid.GridScale.y;

            ScaleGhostCard(new Pointf(x, y));
        }

        private void ScaleGhostCard(Pointf scale)
        {
            if (scale.x < _grid.GridScale.x)
            {
                scale = new Pointf(_grid.GridScale.x, scale.y);
            }

            if (scale.y < _grid.GridScale.y)
            {
                scale = new Pointf(scale.x, _grid.GridScale.y);
            }

            Pointf size = new Pointf(Renderer.size.x, Renderer.size.y);

            if (scale != size)
            {
                LeanTween.cancel(scaleId);
                if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                {
                    LTDescr d = LeanTween.value(Renderer.gameObject, AnimateScale, Renderer.size, new Vector2(scale.x, scale.y), .125f).setEaseOutCubic();
                    scaleId = d.id;
                }
                else
                {
                    AnimateScale(new Vector2(scale.x, scale.y));
                }
            }
        }

        private void AnimateScale(Vector2 scale)
        {
            Renderer.size = scale;
            InfoCanvas.sizeDelta = scale;
        }

        public void OnOpenSheet(string id)
        {
            OnChangeSheet(id);
        }

        /// <summary>
        /// Actions to preform when the sheet changes
        /// </summary>
        /// <param name="sheetid"></param>
        public void OnChangeSheet(string sheetid)
        {

            //Debug.Log($"Ghost Card Behavior: Changing sheet: id - {sheetid}");

            if (sheetid == null || sheetid == string.Empty || SheetsManager.SheetCount < 1)
            {
                CurrentState = GhostCardBehaviorState.invisible;
                return;
            }
            else
            {
                CurrentState = GhostCardBehaviorState.visible;
                MoveGhostCard(Point.zero);
            }

            var offset = SheetsManager.GetGridOffset(sheetid);
            var cardMult = SheetsManager.GetGridScale(sheetid);

            _grid = new WarGrid(offset, cardMult);

        }

        private void OnCloseCardSheet(string id)
        {
            if (SheetsManager.SheetCount < 1)
            {
                CurrentState = GhostCardBehaviorState.invisible;
            }
        }

        public void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnChangeSheet;
            SheetsManager.OnOpenCardSheet += OnOpenSheet;
            SheetsManager.OnCloseCardSheet += OnCloseCardSheet;
        }

        public void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnChangeSheet;
            SheetsManager.OnOpenCardSheet -= OnOpenSheet;
            SheetsManager.OnCloseCardSheet -= OnCloseCardSheet;
        }
    }

    public enum GhostCardBehaviorState
    {
        invisible,
        visible,
        draw,
    }
}
