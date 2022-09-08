using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles the press and hold functionality for the card rep buttons
    /// </summary>
    [RequireComponent(typeof(SlideWindow_Element_CardRep))]
    [Notes.Author("Handles the press and hold functionality for the card rep buttons")]
    public class SlideWindow_Element_CardRep_DragController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private SlideWindow_Element_CardRep _cardRep;

        private float _clickHoldTime = .75f;
        private float _pointerDownTime = 0;

        private bool _pointerDown = false;

        public UnityEvent OnPointerPressAndHold;
        
        private bool pressAndHoldTriggered = false;

        void Start()
        {
            _cardRep = GetComponent<SlideWindow_Element_CardRep>();
        }

        void Update()
        {
            if (_pointerDown)
            {
                _pointerDownTime += 1 * Time.deltaTime;

                if (_pointerDownTime >= _clickHoldTime && !pressAndHoldTriggered)
                {
                    OnPointerPressAndHold?.Invoke();
                    pressAndHoldTriggered = true;
                }
            }
            else
            {
                _pointerDownTime = 0;
                pressAndHoldTriggered = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pointerDown = false;
            pressAndHoldTriggered = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerDown = false;
            pressAndHoldTriggered = false;
        }
    }
}