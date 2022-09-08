using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class DropMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool prevActiveState = false;

        public bool ManipulateWarMode = false;

        public GameObject MenuObject;
        public MenuBarManager MenuBarManager;

        public Image Icon;
        public Button IconButton;
        public Color IconActiveColor;
        public Color IconInActiveColor;

        public bool DisableButtonWhenNoSheetsAreActive;

        public Vector2 closeLocation;
        public Vector2 OpenLocation;

        private bool _activeMenu;

        public UnityEvent<bool> OnToggleMenu;

        public bool ActiveMenu
        {
            get
            {
                return _activeMenu;
            }
            set
            {
                _activeMenu = value;
            }
        }

        void Start()
        {

            UpdateMenu();
            Icon.color = IconInActiveColor;
            SetButtonInteractible(string.Empty);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ToolsManager.SelectedTool = ToolTypes.None;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
        }

        private void ButtonIsActive()
        {
            if (SheetsManager.GetActiveCardSheets().Length < 1 && DisableButtonWhenNoSheetsAreActive)
            {
                if (ActiveMenu)
                    ActiveMenu = false;

                if (IconButton != null)
                    IconButton.interactable = false;
            }
            else
            {
                if (IconButton != null)
                    IconButton.interactable = true;
            }
        }

        void UpdateMenu()
        {
            if (ActiveMenu)
            {
                if (prevActiveState != ActiveMenu)
                {
                    LeanTween.cancel(MenuObject);
                    prevActiveState = ActiveMenu;

                    Icon.color = IconActiveColor;
                    MenuObject.SetActive(ActiveMenu);

                    if (OnToggleMenu != null)
                        OnToggleMenu.Invoke(ActiveMenu);

                    if (MenuBarManager != null)
                        MenuBarManager.ActivateDropMenu(this);
                }

                LeanTween.moveLocal(MenuObject, OpenLocation, .5f).setEaseOutExpo();
            }
            else
            {
                if (prevActiveState != ActiveMenu)
                {
                    LeanTween.cancel(MenuObject);
                    prevActiveState = ActiveMenu;

                    Icon.color = IconInActiveColor;
                    StartCoroutine(ToggleOff());

                    if (OnToggleMenu != null)
                        OnToggleMenu.Invoke(ActiveMenu);
                }

                LeanTween.moveLocal(MenuObject, closeLocation, .5f).setEaseOutExpo();
            }
        }

        void SetButtonInteractible(string sheetId)
        {
            if (DisableButtonWhenNoSheetsAreActive)
            {
                ButtonIsActive();
            }
        }

        public void ToggleActive()
        {
            _activeMenu = !_activeMenu;

            if (ManipulateWarMode)
            {
                if (_activeMenu)
                {
                    ToolsManager.Mode = WarMode.Menu;
                }
                else
                {
                    ToolsManager.Mode = WarMode.Sheet_Editing;
                }
            }

            UpdateMenu();
        }

        IEnumerator ToggleOff()
        {
            yield return new WaitForSeconds(.1f);
            MenuObject.SetActive(false);
            StopCoroutine(ToggleOff());
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += SetButtonInteractible;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= SetButtonInteractible;
        }

    }
}
