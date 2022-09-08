using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace WarManager.Unity3D
{
    public class UnityCardInputManager : MonoBehaviour
    {
        public InputMode InputMode { get; private set; }

        public UnityEvent OnPressDown;
        public UnityEvent OnPressUp;
        public UnityEvent OnHoverStart;
        public UnityEvent OnHoverExit;
        public UnityEvent OnDoublePress;
        public UnityEvent OnTriplePress;

        private int currentPhase = -1;

        [SerializeField] private int clickCount = 0;
        [SerializeField] private float ResetClickTimeLength = .35f;
        [SerializeField] private Vector3 lastClickLocation = -Vector3.one;

        [SerializeField] private int lastButtonClicked = -1;

        private bool doubleClicked;
        private bool tripleClicked;

        private void Init()
        {

        }

        void Update()
        {

            return;

            if (InputMode != InputMode.Touch)
            {
                return;
            }

            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)// && currentPhase == -1)
                {
                    if (OnPressDown != null)
                    {
                        OnPressDown.Invoke();
                    }

                    Debug.Log("begin");
                    currentPhase = 0;
                }

                if (touch.phase == TouchPhase.Ended)// && currentPhase == 0)
                {
                    if (OnPressUp != null)
                    {
                        OnPressUp.Invoke();
                    }
                    Debug.Log("end");
                    // currentPhase = -1;
                }

                Debug.Log("Tap count " + touch.tapCount);
            }
        }

        void OnMouseEnter()
        {
            Init();

            if (InputMode == InputMode.MouseAndKeyboard)
            {
                if (OnHoverStart != null)
                {
                    OnHoverStart.Invoke();
                }
            }
        }

        void OnMouseExit()
        {
            Init();

            if (InputMode == InputMode.MouseAndKeyboard)
            {
                if (OnHoverExit != null)
                {
                    OnHoverExit.Invoke();
                    HandleClickCount(-1, true);
                }
            }
        }

        void OnMouseDown()
        {
            Init();

            // if (InputMode == InputMode.MouseAndKeyboard)
            // {
            if (OnPressDown != null)
            {
                OnPressDown.Invoke();
            }
            // }
        }

        void OnMouseUp()
        {
            Init();


            // if (InputMode == InputMode.MouseAndKeyboard)
            // {

            if (OnPressUp != null)
            {
                OnPressUp.Invoke();
            }
            // }

            HandleClickCount(0, false);

        }

        /// <summary>
        /// Called when the input mode is changed
        /// </summary>
        /// <param name="mode"></param>
        void OnChangeInputMode(InputMode mode)
        {
            InputMode = mode;
            // Debug.Log("Input mode changed to " + mode.ToString());
        }

        void OnEnable()
        {
            InputSystem.OnChangeInputMode += OnChangeInputMode;
            HandleClickCount(-1, true);
        }

        void OnDisable()
        {
            InputSystem.OnChangeInputMode -= OnChangeInputMode;
        }

        private void HandleClickCount(int button, bool reset)
        {
            if (!CheckLastButtonClicked(button))
            {
                reset = true;
            }

            StopCoroutine(ClickResetTimer());

            if (reset)
            {
                clickCount = 0;
                lastClickLocation = Input.mousePosition;
                lastButtonClicked = -1;

                tripleClicked = false;
                doubleClicked = false;

                // Debug.Log("reset");
            }
            else
            {
                lastClickLocation = Input.mousePosition;
                clickCount++;
                StartCoroutine(ClickResetTimer());

                // Debug.Log("count up " + clickCount);

                if (clickCount % 2 == 0 && clickCount != 0)
                {
                    if (OnDoublePress != null)
                    {
                        OnDoublePress.Invoke();
                    }

                    //Debug.Log("double clicked");
                }

                if (clickCount % 3 == 0 && clickCount != 0)
                {
                    tripleClicked = true;
                }
            }
        }

        private bool CheckLastButtonClicked(int button)
        {
            if (lastButtonClicked == button)
            {
                return true;
            }
            else
            {
                if (lastButtonClicked == -1)
                {
                    lastButtonClicked = button;
                    return true;
                }
                else
                {
                    lastButtonClicked = -1;
                }

                return false;
            }
        }

        IEnumerator ClickResetTimer()
        {
            // Debug.Log("reset timer started");
            yield return new WaitForSecondsRealtime(ResetClickTimeLength);
            HandleClickCount(-1, true);
            //Debug.Log("reset timer ended");
            // CheckClicks();
        }

        private void CheckClicks()
        {
            if (tripleClicked)
            {
                doubleClicked = false;
                tripleClicked = false;

                if (OnTriplePress != null)
                {
                    OnTriplePress.Invoke();
                }

                Debug.Log("triple clicked");
            }

            if (doubleClicked)
            {
                doubleClicked = false;
                tripleClicked = false;

                if (OnDoublePress != null)
                {
                    OnDoublePress.Invoke();
                }

                Debug.Log("double clicked");
            }
        }
    }
}
