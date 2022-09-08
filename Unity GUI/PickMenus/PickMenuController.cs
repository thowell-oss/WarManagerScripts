
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using WarManager.Tools;

namespace WarManager.Unity3D.PickMenu
{

    [RequireComponent(typeof(RectTransform))]
    [Notes.Author("Controls the interaction with the pick menu")]
    public class PickMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] PickMenuButton PickMenuButtonPrefab;

        private Queue<PickMenuButton> UnusedButtons = new Queue<PickMenuButton>();
        private List<PickMenuButton> ActiveButtons = new List<PickMenuButton>();
        public int ButtonStartAmt = 20;

        [SerializeField] GameObject BottomRedBorder;
        [SerializeField] GameObject TopRedBorder;

        [SerializeField] GameObject MenuObject;

        private RectTransform MenuRect;

        private int buttonTotal = 0;

        [SerializeField] bool Closed = false;

        private bool _wasClosed = false;

        private bool _awake = true;

        public bool MenuClosed
        {
            get
            {
                return Closed;
            }
        }

        void Start()
        {
            for (int i = 0; i < ButtonStartAmt; i++)
            {
                PickMenuButton b = Instantiate<PickMenuButton>(PickMenuButtonPrefab, MenuObject.transform);
                b.gameObject.SetActive(false);
                UnusedButtons.Enqueue(b);
            }

            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            _awake = true;
            // Debug.Log("Awake");
        }

        void Update()
        {

            // if (ToolsManager.SelectedTool == ToolTypes.None && !Closed)
            // {
            //     Close();
            //     ToolsManager.SelectedTool = ToolTypes.None;
            //     return;
            // }
            // else if (ToolsManager.SelectedTool == ToolTypes.None)
            // {
            //     return;
            // }

            if (!Application.isFocused && !Closed)
                Close();

            if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
            {
                if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1)) && !Closed)
                {
                    if (_wasClosed)
                    {
                        _wasClosed = false;
                        // Debug.Log("Was Closed");
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else if (InputSystem.Main.InputMode == InputMode.Touch)
            {
                try
                {

                    if (Input.GetTouch(0).phase == TouchPhase.Began && !Closed)
                    {
                        if (_wasClosed)
                        {
                            _wasClosed = false;
                            Debug.Log("Was Closed");
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.Log(ex.Message);
#endif
                }
            }
        }

        private void Display_ScaleMenuDown()
        {
            // BottomRedBorder.SetActive(true);
            TopRedBorder.SetActive(false);

            Display_Animate();
        }

        private void Display_ScaleMenuUp()
        {
            BottomRedBorder.SetActive(false);
            //TopRedBorder.SetActive(true);

            Display_Animate();
        }

        private void Display_Animate()
        {
            if (!LeanTween.isTweening(gameObject))
            {
                MenuObject.SetActive(false);

                MenuRect.sizeDelta = new Vector2(1, 1);
                LeanTween.value(this.gameObject, (x) => { ScaleOnX(x); }, 0, 200, .25f).setEaseOutCubic();

                LeanTween.delayedCall(.1f, () =>
                {
                    LeanTween.value(this.gameObject, (x) => { ScaleOnY(x); }, 0, (30 * buttonTotal), .25f).setEaseOutCubic();

                    LeanTween.delayedCall(.1f, () =>
                    {
                        MenuObject.SetActive(true);
                    });

                });
            }
        }

        private void Cancel_Animate()
        {
            // Debug.Log("cancel animate");

            LeanTween.cancel(gameObject);

            LeanTween.value(this.gameObject, (x) => { ScaleOnY(x); }, (30 * buttonTotal), 0, .25f).setEaseOutCubic();

            LeanTween.delayedCall(.1f, () =>
                {
                    LeanTween.value(this.gameObject, (x) => { ScaleOnX(x); }, 200, 0, .25f).setEaseOutCubic();

                    LeanTween.delayedCall(.15f, () =>
                                        {
                                            if (!_wasClosed)
                                                gameObject.SetActive(false);

                                            Closed = true;
                                            buttonTotal = 0;

                                            if (ToolsManager.SelectedTool == ToolTypes.None)
                                            {
                                                // Debug.Log("resetting tools manager");
                                                ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
                                            }
                                        });

                });
        }

        private void ScaleOnY(float value)
        {
            if (MenuRect == null)
                MenuRect = GetComponent<RectTransform>();

            MenuRect.sizeDelta = new Vector2(MenuRect.sizeDelta.x, value);
        }

        private void ScaleOnX(float value)
        {
            if (MenuRect == null)
                MenuRect = GetComponent<RectTransform>();

            MenuRect.sizeDelta = new Vector2(value, MenuRect.sizeDelta.y);
        }

        private void SetButtons(List<(string title, Action action, bool interactible)> actions)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (UnusedButtons.Count > 0)
                {
                    var button = UnusedButtons.Dequeue();
                    button.gameObject.SetActive(true);
                    button.transform.SetAsLastSibling();

                    if (actions[i].action != null && actions[i].interactible)
                    {
                        button.SetButton(actions[i].title, actions[i].action, actions[i].interactible);
                    }
                    else
                    {
                        button.SetButton(actions[i].title, null, false);
                    }

                    ActiveButtons.Add(button);
                }

                if (UnusedButtons.Count < 1)
                {
                    throw new Exception("No Buttons left to use. Please check the code or change the amount of buttons to load during initialization");
                }
            }
        }

        private void ClearButtons()
        {
            // Debug.Log("unused check " + ActiveButtons.Count);

            for (int i = ActiveButtons.Count - 1; i >= 0; i--)
            {
                UnusedButtons.Enqueue(ActiveButtons[i]);
                ActiveButtons[i].gameObject.SetActive(false);

                // Debug.Log("unused " + UnusedButtons.Count);
            }

            ActiveButtons.Clear();
        }

        public void Open(List<(string title, Action action, bool interactible)> buttons, Vector3 spawnLocation)
        {
            if (buttons == null || buttons.Count < 1)
                return;

            // if (ToolsManager.SelectedTool == ToolTypes.None)
            //     return;

            // if (ToolsManager.Mode == WarMode.Menu)
            //     return;

            if (!_awake)
                _wasClosed = true;
            else
                _wasClosed = false;

            _awake = false;

            ClearButtons();
            buttonTotal = buttons.Count;
            OpenMenu(spawnLocation);
            SetButtons(buttons);
            Closed = false;
        }

        public void Close()
        {
            if (Closed)
                return;

            Cancel_Animate();
            // Debug.Log("Closing");
        }

        private void OpenMenu(Vector3 position)
        {

            if (MenuRect == null)
                MenuRect = GetComponent<RectTransform>();

            float pivotX = position.x / Screen.width;
            float pivotY = position.y / Screen.height;

            if (pivotX < .5f)
            {
                pivotX = -.025f;
            }
            else
            {
                pivotX = 1.025f;
            }

            if (pivotY < .5f)
            {
                pivotY = -.025f;
                Display_ScaleMenuDown();
            }
            else
            {
                pivotY = 1.025f;
                Display_ScaleMenuUp();
            }

            MenuRect.pivot = new Vector2(pivotX, pivotY);
            transform.position = position;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolsManager.SelectedTool = ToolTypes.None;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        }
    }
}
