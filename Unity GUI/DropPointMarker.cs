
using WarManager;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles the location of the drop point and shows the grid size")]
    public class DropPointMarker : PoolableObject
    {

        [SerializeField] private bool DefaultDropPoint = false;

        [SerializeField] private GameObject[] cardObjects;

        /// <summary>
        /// The size of the rectangle
        /// </summary>
        /// <value></value>
        public WarManager.Rect rect { get; private set; }

        /// <summary>
        /// The sheet grid
        /// </summary>
        public WarGrid Grid { get; set; }

        /// <summary>
        /// private location of the drop point
        /// </summary>
        private Point _currentPoint;


        /// <summary>
        /// The current status of the card
        /// </summary>
        [SerializeField] private CardState _cardStatus = CardState.invisible;


        /// <summary>
        /// The current status of the card
        /// </summary>
        /// <value></value>
        public CardState CardStatus
        {
            get
            {
                return _cardStatus;
            }
            private set
            {
                _cardStatus = value;
                UpdateCardStatus();
            }
        }

        /// <summary>
        /// the location of the drop point
        /// </summary>
        /// <value></value>
        public Point CurrentPoint
        {
            get
            {
                return _currentPoint;
            }

            set
            {
                SetLocation(value);
            }
        }


        void Start() => UpdateCardStatus();


        /// <summary>
        /// Set the card to reflect the current set status of the card
        /// </summary>
        private void UpdateCardStatus()
        {

            switch (_cardStatus)
            {
                case CardState.visible:

                    foreach (var oj in cardObjects)
                    {
                        oj.gameObject.SetActive(true);
                    }

                    // Point p = SheetDropPointManager.GetDropPoint();

                    // if (p != CurrentPoint)
                    // {
                    //     CurrentPoint = p;
                    // }

                    break;

                default:
                    foreach (var oj in cardObjects)
                    {
                        oj.gameObject.SetActive(false);
                        oj.transform.localScale = new Vector3(.1f, .1f, .1f);
                    }

                    break;
            }
        }


        /// <summary>
        /// Animate the move to the new point
        /// </summary>
        public void AnimateMoveToPoint(Point p, float speed)
        {
            if (p == null)
                return;

            if (!p.IsInGridBounds)
            {
                NotificationHandler.Print("The selected drop location " + p.ToString() + " is not a valid position");
                return;
            }

            _currentPoint = p;

            var worldPos = GetWorldPosition(_currentPoint);
            LeanTween.move(this.gameObject, worldPos, speed).setEaseOutCubic();

            //Debug.Log("animating drop point " + _currentPoint);
        }

        /// <summary>
        /// Set the location of the point
        /// </summary>
        /// <param name="p">the point of the location</param>
        public void SetLocation(Point p)
        {

            _cardStatus = CardState.invisible;

            if (p == null)
                return;

            if (!p.IsInGridBounds)
            {
                NotificationHandler.Print("The selected drop location " + p.ToString() + " is not a valid position");
                return;
            }

            _currentPoint = p;

            var worldPos = GetWorldPosition(_currentPoint);
            transform.position = worldPos;

            _cardStatus = CardState.visible;
        }


        /// <summary>
        /// Get the world position from a given point
        /// </summary>
        /// <param name="p">the given point of the world position</param>
        /// <returns>returns a new vector2 of the drop point</returns>
        private Vector2 GetWorldPosition(Point p)
        {

            if (Grid == null)
                return Vector2.zero;

            if (p == null)
                p = Point.zero;

            Pointf worldPos = Pointf.GridToWorld(p, Grid);

            return new Vector2(worldPos.x, worldPos.y);
        }

        /// <summary>
        /// Actions to preform when the sheet changes
        /// </summary>
        /// <param name="sheetid"></param>
        public void OnChangeSheet(string sheetid)
        {

            if (sheetid == null || sheetid == string.Empty || SheetsManager.SheetCount < 1)
            {
                CardStatus = CardState.invisible;
                return;
            }
            else
            {
                CardStatus = CardState.visible;
                CurrentPoint = Point.zero;
            }

            var offset = SheetsManager.GetGridOffset(sheetid);
            var cardMult = SheetsManager.GetGridScale(sheetid);

            if (offset == null)
                throw new System.NullReferenceException("Offset is null");

            if (cardMult == null)
                throw new System.NullReferenceException("Card multiplier");

            Grid = new WarGrid(offset, cardMult);

            CurrentPoint = SheetDropPointManager.GetDropPoint(sheetid);
        }

        /// <summary>
        /// Call event to change the drop point
        /// </summary>
        /// <param name="sheetId">the id of the sheet</param>
        /// <param name="point">the new drop point</param>
        public void OnChangeDropPointLocation(string sheetId, Point point)
        {
            if (sheetId == null)
                throw new System.NullReferenceException("the sheet id is null");

            if (point == null)
            {
                throw new System.NullReferenceException("the point is null");
            }

            if (sheetId.Trim() == "")
            {
                throw new System.Exception("the sheet id is empty");
            }

            if (!point.IsInGridBounds)
                throw new System.Exception("the point is out of bounds");

            if (sheetId == SheetsManager.CurrentSheetID)
            {
                CurrentPoint = point;
                UpdateCardStatus();
            }
        }

        /// <summary>
        /// Action performed when a sheet is closed
        /// </summary>
        /// <param name="id"></param>
        private void OnCloseCardSheet(string id)
        {
            if (SheetsManager.SheetCount < 1)
            {
                CardStatus = CardState.invisible;
            }
        }

        public void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnChangeSheet;
            SheetsManager.OnCloseCardSheet += OnCloseCardSheet;

            if (DefaultDropPoint)
                SheetDropPointManager.OnDropPointChangeLocation += OnChangeDropPointLocation;
        }

        public void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnChangeSheet;
            SheetsManager.OnCloseCardSheet -= OnCloseCardSheet;

            if (DefaultDropPoint)
                SheetDropPointManager.OnDropPointChangeLocation -= OnChangeDropPointLocation;

            CheckIn();
        }

        void OnMouseDown()
        {
            Debug.Log("Clicked on drop point marker");
        }
    }

    /// <summary>
    /// The states of the non data card
    /// </summary>
    public enum CardState
    {
        invisible,
        visible,
    }
}
