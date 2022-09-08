using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles movement of the buttons when switching from forms to sheet editing frameworks and vice versa.
    /// </summary>
    [Notes.Author("Handles movement of the buttons when switching from forms to sheet editing frameworks and vice versa.")]
    public class MenuBarFormsManager : MonoBehaviour
    {
        /// <summary>
        /// The list of buttons to turn off/on
        /// </summary>
        /// <typeparam name="Button"></typeparam>
        /// <returns></returns>
        [SerializeField] List<Button> buttons = new List<Button>();

        /// <summary>
        /// Are the buttons sheet buttons or form buttons
        /// </summary>
        [SerializeField] bool SheetButtons = true;

        /// <summary>
        /// The location that the buttons should be when the forms framework is being used
        /// </summary>
        [SerializeField] Transform FormsLocation;

        /// <summary>
        /// The location that the buttons should be when the sheet editing framework is being used
        /// </summary>
        [SerializeField] Transform SheetLocation;


        /// <summary>
        /// The transition speed between sheet and forms location (for beauty purposes).
        /// </summary>
        [SerializeField] float MovementSpeed = .25f;


        /// <summary>
        /// Set the menus for forms
        /// </summary>
        public void SetFormMenus()
        {
            LeanTween.move(this.gameObject, FormsLocation.position, MovementSpeed).setEaseOutCubic();
            SetButtonsInteractable(!SheetButtons);
        }


        /// <summary>
        /// Set the menus for sheet editing
        /// </summary>
        public void SetSheetMenus()
        {
            LeanTween.move(this.gameObject, SheetLocation.position, MovementSpeed).setEaseOutCubic();
            SetButtonsInteractable(SheetButtons);
        }

        public void ToggleForms(bool forms)
        {
            if (forms)
            {
                SetFormMenus();
            }
            else
            {
                SetSheetMenus();
            }
        }


        /// <summary>
        /// Set the buttons interactible
        /// </summary>
        /// <param name="interactable"></param>
        private void SetButtonsInteractable(bool interactable)
        {
            foreach (var button in buttons)
            {
                if (interactable)
                {
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;
                }
            }
        }
    }
}

