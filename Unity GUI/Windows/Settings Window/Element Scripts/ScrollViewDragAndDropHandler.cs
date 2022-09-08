using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewDragAndDropHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool Draggable;

    private bool _draggingSlot;

    public UnityEvent<bool> OnActivateDrag;

    [SerializeField] private ScrollRect scrollRect;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Draggable)
        {
            return;
        }

        StartCoroutine(StartTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(0.5f);
        _draggingSlot = true;

        if (OnActivateDrag != null)
        {
            OnActivateDrag.Invoke(true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (scrollRect == null)
        {
            scrollRect = transform.parent.parent.parent.GetComponent<ScrollRect>();
        }

        if (scrollRect != null)
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(eventData.delta.normalized.y > 1 && eventData.delta.normalized.x < .5f)
        {
            if(!_draggingSlot)
            {
                CancelDrag();
            }
        }

        if (_draggingSlot)
        {
            //DO YOUR DRAGGING HERE
            Debug.Log("dragging " + eventData.delta);
        }
        else
        {
            //OR DO THE SCROLLRECT'S
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        
        if (_draggingSlot)
        {
            //END YOUR DRAGGING HERE
            
            CancelDrag();
        }
    }

    private void CancelDrag()
    {
        StopAllCoroutines();
        _draggingSlot = false;

        if(OnActivateDrag != null)
        {
            OnActivateDrag.Invoke(false);
        }
    }
}
