using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardSelectableGroup : MonoBehaviour
{
    public List<Selectable> selectables = new List<Selectable>();


    /// <summary>
    /// use the mouse wheel to cycle up and and down?
    /// </summary>
    public bool UseMouseWheel = false;
    public bool UseArrows = true;

    /// <summary>
    /// Invert the selected Selectable List
    /// </summary>
    public bool Inverted = false;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            // Navigate backward when holding shift, else navigate forward.
            if (Inverted)
            {
                this.HandleHotkeySelect(!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)), true);

            }
            else
            {
                this.HandleHotkeySelect(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), true);
            }

            return;
        }

        if ((Input.GetKey(KeyCode.Escape)))
        {
            EventSystem.current.SetSelectedGameObject(null, null);
            return;
        }


        if (UseArrows)
        {
            if (Inverted)
            {
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    this.HandleHotkeySelect(true, true);
                    return;
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    this.HandleHotkeySelect(false, true);
                    return;
                }

            }
            else
            {
                if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    this.HandleHotkeySelect(true, true);
                    return;
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    this.HandleHotkeySelect(false, true);
                    return;
                }
            }
        }

        if (UseMouseWheel)
        {

            if (Inverted)
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    this.HandleHotkeySelect(false, true);
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    this.HandleHotkeySelect(true, true);
                }

            }
            else
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    this.HandleHotkeySelect(false, true);
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    this.HandleHotkeySelect(true, true);
                }
            }
        }
    }

    private void HandleHotkeySelect(bool isNavigateBackward, bool isWrapAround)
    {
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject != null && selectedObject.activeInHierarchy) // Ensure a selection exists and is not an inactive object.
        {
            Selectable currentSelection = selectedObject.GetComponent<Selectable>();
            if (currentSelection != null)
            {
                Selectable nextSelection = this.FindNextSelectable(
                    selectables.IndexOf(currentSelection), isNavigateBackward, isWrapAround);
                if (nextSelection != null)
                {
                    nextSelection.Select();
                }
            }
            else
            {
                this.SelectFirstSelectable();
            }
        }
        else
        {
            this.SelectFirstSelectable();
        }
    }

    private void SelectFirstSelectable()
    {
        if (selectables != null && selectables.Count > 0)
        {
            Selectable firstSelectable = selectables[0];
            firstSelectable.Select();
        }
    }

    /// <summary>
    /// Looks at ordered selectable list to find the selectable we are trying to navigate to and returns it.
    /// </summary>
    private Selectable FindNextSelectable(int currentSelectableIndex, bool isNavigateBackward, bool isWrapAround)
    {
        Selectable nextSelection = null;

        int totalSelectables = selectables.Count;
        if (totalSelectables > 1)
        {
            if (isNavigateBackward)
            {
                if (currentSelectableIndex == 0)
                {
                    nextSelection = (isWrapAround) ? selectables[totalSelectables - 1] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex - 1];
                }
            }
            else // Navigate forward.
            {
                if (currentSelectableIndex == (totalSelectables - 1))
                {
                    nextSelection = (isWrapAround) ? selectables[0] : null;
                }
                else
                {
                    nextSelection = selectables[currentSelectableIndex + 1];
                }
            }
        }

        return nextSelection;
    }

    public void SetInverted(bool invert)
    {
        Inverted = invert;
    }
}