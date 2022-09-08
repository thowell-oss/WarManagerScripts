/* UnityGUIContextMenu.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Controls The Unity GUI context menu
    /// </summary>
    public class UnityGUIContextMenu : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Actions")]
        public ContextMenuAction Lock;
        public ContextMenuAction Hide;
        public ContextMenuAction Remove;

        [Header("Selection")]
        public ContextMenuAction SelectCol;
        public ContextMenuAction SelectRow;

        [Header("Shifting")]
        public ContextMenuAction ShiftLeft;
        public ContextMenuAction ShiftRight;

        public ContextMenuAction ShiftUp;
        public ContextMenuAction ShiftDown;

        [Header("Tweaning Properties")]
        public Vector2 offLocTop;
        public RectTransform offLocBottom;
        public Vector2 OnLoc;

        public Vector2 DefaultOnLocation;
        public float offSpeed, onSpeed;

        public Vector2 tweenToOffPositionThreshold;

        private bool prevClosedState;

        FormsHider _formsHider;

        private bool _closed;
        public bool Closed
        {
            get
            {
                return _closed;
            }

            set
            {
                _closed = value;
            }
        }

        #region  Dragging
        public RectTransform MenuBarRect;
        public Vector2 pointerOffset;

        #endregion

        // Start is called before the first frame update
        void Start() => Init();

        public void Init()
        {
            //LeanTween.init(1000);

            DefaultOnLocation = OnLoc;

            Closed = true;
            ShiftUp.Init(Action_ShiftUp);
            ShiftLeft.Init(Action_ShiftLeft);
            ShiftRight.Init(Action_ShiftRight);
            ShiftDown.Init(Action_ShiftDown);

            SelectCol.Init(Action_SelectColumn);
            SelectRow.Init(Action_SelectRow);

            Lock.Init(Action_Lock);
            Remove.Init(Action_Remove);
            Hide.Init(Action_Hide);

            _formsHider = GetComponent<FormsHider>();
        }

        void Update()
        {
            if (_formsHider.FormsActive)
            {
                if (!Closed)
                {
                    CloseMenu();
                    _closed = true;
                }
            }

            if (prevClosedState != Closed)
            {
                if (Closed)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }

                prevClosedState = Closed;
            }
        }

        public void CloseMenu()
        {
            //Debug.Log("Closing");

            if (InThreshold(true, MenuBarRect.anchoredPosition))
            {
                LeanTween.cancel(gameObject);
                LeanTween.moveLocal(gameObject, offLocTop, offSpeed).setEaseInOutCirc();
                OnLoc = DefaultOnLocation;
            }
            // else
            // {
            //     LeanTween.cancel(gameObject);
            //     LeanTween.moveLocal(gameObject, offLocBottom.position, offSpeed).setEaseInOutCirc();
            // }
        }

        private bool InThreshold(bool top, Vector2 position)
        {
            float x = 0;
            float y = 0;

            if (top)
            {
                x = offLocTop.x - position.x;
                y = offLocTop.y - position.y;


            }

            x = Mathf.Abs(x);
            y = Mathf.Abs(y);

            if (x <= tweenToOffPositionThreshold.x)
            {
                if (y <= tweenToOffPositionThreshold.y)
                {

                    return true;
                }
            }

            return false;
        }

        public void OpenMenu()
        {
            LeanTween.cancel(gameObject);

            if (InThreshold(true, MenuBarRect.anchoredPosition))
            {
                LeanTween.moveLocal(gameObject, OnLoc, onSpeed).setEaseInOutCirc();
            }
        }

        public void Action_Lock(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_Lock(state);
        }

        public void Action_Remove(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_Remove(state);
        }

        public void Action_Hide(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_Hide(state);
        }

        public void Action_SelectRow(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_SelectRow(state);
        }

        public void Action_SelectColumn(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_SelectColumn(state);
        }

        public void Action_ShiftRight(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_ShiftRight(state);
        }

        public void Action_ShiftLeft(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_ShiftLeft(state);
        }

        public void Action_ShiftUp(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_ShiftUp(state);
        }

        public void Action_ShiftDown(ContextMenuButtonState state)
        {
            ContextMenuHandler.Action_ShiftDown(state);
        }

        public void RefreshMenu(bool active, ContextMenuButtonState lockedState, ContextMenuButtonState hideState, ContextMenuButtonState removeState,
        ContextMenuButtonState horizontalState, ContextMenuButtonState verticalState, ContextMenuButtonState left, ContextMenuButtonState right,
         ContextMenuButtonState up, ContextMenuButtonState down)
        {

            Closed = !active;

            Lock.SetState(lockedState);
            Hide.SetState(hideState);
            Remove.SetState(removeState);
            SelectRow.SetState(horizontalState);
            SelectCol.SetState(verticalState);

            ShiftLeft.SetState(left);
            ShiftRight.SetState(right);
            ShiftUp.SetState(up);
            ShiftDown.SetState(down);
        }


        void OnEnable()
        {
            ContextMenuHandler.OnShowContextMenu += RefreshMenu;
        }

        private void OnDisable()
        {
            ContextMenuHandler.OnShowContextMenu -= RefreshMenu;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + pointerOffset;

            if (!InThreshold(true, pos))
            {
                OnLoc = pos;
                MenuBarRect.anchoredPosition = OnLoc;
            }
            else
            {
                OnLoc = DefaultOnLocation;
                LeanTween.moveLocal(gameObject, OnLoc, .125f).setEaseInOutCirc();
            }

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            pointerOffset = MenuBarRect.anchoredPosition - new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            ToolsManager.SelectedTool = ToolTypes.None;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (InThreshold(true, MenuBarRect.anchoredPosition))
            {
                OnLoc = DefaultOnLocation;
            }

            if (Closed)
                CloseMenu();

            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        }

    }
}
