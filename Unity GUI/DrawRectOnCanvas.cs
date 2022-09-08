using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using WarManager.Cards;
using WarManager.Tools;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class DrawRectOnCanvas : MonoBehaviour
    {
        public RectTransform DrawRectImage;
        public RectTransform SelectRectArea, ContextMenu, ToolBar;

        public Camera cam;

        private Vector2 a;

        [SerializeField] Vector2 _offset;

        public Vector2 Offset
        {
            get
            {
                _offset = WarManagerDriver.Main.Offset;
                return WarManagerDriver.Main.Offset;
            }
        }

        [SerializeField] Vector2 _scale;

        public Vector2 CardGridMultiplier
        {
            get
            {
                _scale = WarManagerDriver.Main.Scale;

                return WarManagerDriver.Main.Scale;
            }
        }

        /// <summary>
        /// Can the user draw a rect?
        /// </summary>
        public bool CanDrawRect { get; set; } = true;

        /// <summary>
        /// Is the user currently drawing a rect on the screen?
        /// </summary>
        public bool DrawingRect { get; private set; }

        public delegate void CancelDrawRect();
        public static event CancelDrawRect OnCancelDrawRect;

        public delegate void FinishDrawRect();
        public static event FinishDrawRect OnFinishDrawRect;

        public delegate void StartDrawRect();
        public static event StartDrawRect OnStartDrawRect;

        private (int, int) lastGridPosition;

        [SerializeField] MouseStatus CurrentStatus;

        public static MouseStatus MouseContextStatus;

        Vector3 startMousePos;
        Vector3 modifiedMousePos;

        public void Start() => StartCoroutine(CheckMenuContext());

        IEnumerator CheckMenuContext()
        {
            while (true)
            {
                MouseContextStatus = MousePositionContext();
                CurrentStatus = MouseContextStatus;
                yield return new WaitForSeconds(.25f);
            }
        }

        public void Update()
        {
            //DrawRect();
        }

        public MouseStatus MousePositionContext()
        {

            if (RectTransformUtility.RectangleContainsScreenPoint(ContextMenu, Input.mousePosition))
            {
                return MouseStatus.contextMenu;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(ToolBar, Input.mousePosition))
            {
                return MouseStatus.toolBar;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(SelectRectArea, Input.mousePosition))
            {
                if (ToolsManager.SelectedTool == ToolTypes.Edit)
                {
                    return MouseStatus.editTool;
                }
                else if (ToolsManager.SelectedTool == ToolTypes.Highlight)
                {
                    return MouseStatus.selectTool;
                }
                else if (ToolsManager.SelectedTool == ToolTypes.Calculate)
                {
                    return MouseStatus.calculateTool;
                }
                else if (ToolsManager.SelectedTool == ToolTypes.Annotate)
                {
                    return MouseStatus.annotateTool;
                }
                else
                {
                    return MouseStatus.panTool;
                }

            }

            return MouseStatus.menu;
        }



        private void DrawRect()
        {
            if (ToolsManager.SelectedTool == ToolTypes.Pan || ToolsManager.SelectedTool == ToolTypes.None || SheetsManager.SheetCount < 1)
            {
                CancelRect();
                //Debug.Log("done --");
            }

            if ((Input.GetMouseButton(0) && ToolsManager.SelectedTool != ToolTypes.Highlight) || Input.GetMouseButton(2) || (!CanDrawRect && DrawingRect))
            {
                CancelRect();
                //Debug.Log("Cancel");
            }

            if (!CanDrawRect)
                return;

            if (Input.GetMouseButtonUp(1) || (Input.GetMouseButtonUp(0) && ToolsManager.SelectedTool == ToolTypes.Highlight))
            {
                CompleteRect();
                //Debug.Log("done");
            }

            if ((Input.GetMouseButtonDown(1) && (!Input.GetMouseButton(0) && ToolsManager.SelectedTool != ToolTypes.Highlight) && !Input.GetMouseButton(2)) || (ToolsManager.SelectedTool == ToolTypes.Highlight && Input.GetMouseButtonDown(0)))
            {
                Vector3 modifiedMousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y - 20, Input.mousePosition.z);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(SelectRectArea, modifiedMousePos, null, out a);
                DrawingRect = true;

                startMousePos = modifiedMousePos;

                if (OnStartDrawRect != null)
                {
                    OnStartDrawRect.Invoke();
                }

                //Debug.Log("draing rect");
            }

            if (((Input.GetMouseButton(1) && !Input.GetMouseButton(0) && ToolsManager.SelectedTool != ToolTypes.Highlight && !Input.GetMouseButton(2)) || (ToolsManager.SelectedTool == ToolTypes.Highlight && Input.GetMouseButton(0))) && DrawingRect)
            {
                Vector2 b;

                modifiedMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y - 20);

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(SelectRectArea, modifiedMousePos, null, out b))
                {
                    DrawRect(a, b);
                }

                //Debug.Log("continuing to draw rect");
            }
        }

        /// <summary>
        /// Draw a rectangle on the screen
        /// </summary>
        /// <param name="a">the first location</param>
        /// <param name="b">the final location</param>
        public void DrawRect(Vector2 a, Vector2 b)
        {
            DrawRectImage.gameObject.SetActive(true);

            Vector2 midPoint = new Vector2((a.x - b.x) / 2, (a.y - b.y) / 2);

            DrawRectImage.sizeDelta = new Vector2(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

            DrawRectImage.anchoredPosition = new Vector2(midPoint.x + b.x, midPoint.y + b.y);
        }

        /// <summary>
        /// Cancel the drawing rect operation
        /// </summary>
        public void CancelRect()
        {
            DrawingRect = false;
            DrawRectImage.gameObject.SetActive(false);

            if (OnCancelDrawRect != null)
                OnCancelDrawRect.Invoke();

            //Debug.Log("Canceled");
        }

        public void CompleteRect()
        {
            CancelRect();

            SelectCards(startMousePos, modifiedMousePos);

            if (OnFinishDrawRect != null)
                OnFinishDrawRect.Invoke();

            //Debug.Log("Completed");
        }

        /// <summary>
        /// Select cards on the sheet whose center location is inside a rectangle
        /// </summary>
        /// <param name="startMousePositon">the start mouse position (first (x, y) coordinate)</param>
        /// <param name="mousePosition">the final mouse position (last (x.y) coordiante</param>
        public void SelectCards(Vector2 startMousePositon, Vector2 mousePosition)
        {
            WarManagerDriver.Main.SelectHandler.Clear();

            Vector2 dist = new Vector2(startMousePositon.x - mousePosition.x, startMousePositon.y - mousePosition.y);

            if (dist.sqrMagnitude < 1)
            {
                SheetsCardSelectionManager.Main.DeselectCurrent();
                return;
            }

            Vector3 endPos;
            Vector3 startPos;

            startPos = cam.ScreenToWorldPoint(startMousePositon);
            endPos = cam.ScreenToWorldPoint(mousePosition);

            var cards = CardUtility.GetCardsFromCurrentSheet();

            foreach (Card card in cards)
            {

                (float x, float y) position = WarManager.CardLayout.GetCardGlobalLocation(card.point, (0, 0), (Offset.x, Offset.y), (CardGridMultiplier.x, CardGridMultiplier.y));

                if ((position.x < endPos.x && position.x > startPos.x && endPos.x > startPos.x) || (position.x > endPos.x && position.x < startPos.x && endPos.x < startPos.x))
                {
                    if ((position.y < endPos.y && position.y > startPos.y && endPos.y > startPos.y) || (position.y > endPos.y && position.y < startPos.y && endPos.y < startPos.y))
                    {
                        card.Select(true);
                    }
                    else
                    {
                        if (!Input.GetKey(KeyCode.LeftCommand))
                            card.Deselect();
                    }
                }
                else
                {
                    if (!Input.GetKey(KeyCode.LeftCommand))
                        card.Deselect();
                }
            }
        }
    }

    public enum MouseStatus
    {
        editTool,
        panTool,
        selectTool,
        calculateTool,
        annotateTool,
        contextMenu,
        toolBar,
        menu,
    }
}


