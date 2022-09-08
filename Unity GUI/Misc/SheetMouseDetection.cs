
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace WarManager
{
    public class SheetMouseDetection : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
     IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public MouseChangeEventSystem LastMouseEvent;

        public delegate void changemouse_delegate(MouseChangeEventSystem s, PointerEventData data);
        public static event changemouse_delegate OnChangeMouse;

        public delegate void mouseOverWorkSpace_delegate(bool over);
        public static event mouseOverWorkSpace_delegate OnMouseOverWorkSpace;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.mouseClick;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.mouseDown;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.mouseEnter;
                OnChangeMouse(LastMouseEvent, eventData);
            }

            if(OnMouseOverWorkSpace != null)
            {
                OnMouseOverWorkSpace(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.mouseExit;
                OnChangeMouse(LastMouseEvent, eventData);
            }

             if(OnMouseOverWorkSpace != null)
            {
                OnMouseOverWorkSpace(false);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.mouseUp;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.drag;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                LastMouseEvent = MouseChangeEventSystem.beginDrag;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (OnChangeMouse != null)
            {
                 LastMouseEvent = MouseChangeEventSystem.endDrag;
                OnChangeMouse(LastMouseEvent, eventData);
            }
        }
    }

    public enum MouseChangeEventSystem
    {
        mouseDown,
        mouseUp,
        mouseClick,
        mouseEnter,
        mouseExit,
        drag,
        beginDrag,
        endDrag,
    }
}
