using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.PickMenu
{
    [Notes.Author("Handles the access to using the pick menu from a global perspective")]
    public class PickMenuManger : MonoBehaviour
    {
        [SerializeField]
        private PickMenuController PickMenu;

        #region  singleton
        public static PickMenuManger main;
        void Awake()
        {
            if (main != null)
            {
                main = null;
                Debug.LogError("You can only have one pickmenu set up at one time");
            }

            main = this;
        }

        #endregion

        private List<(string, System.Action, bool)> Actions = new List<(string, System.Action, bool)>();

        void Update()
        {
            // if (Input.GetMouseButtonDown(1))
            // {
            //     OpenPickMenu(Actions);
            // }

            // if (Input.GetMouseButtonUp(0) && !PickMenu.MenuClosed)
            // {
            //     ClosePickMenu();
            // }

            if (Input.GetMouseButtonDown(2))
            {
                ClosePickMenu();
            }
        }

        /// <summary>
        /// Opens the pick menu
        /// </summary>
        /// <param name="actions">the list of buttons to display with correlated actions</param>
        public void OpenPickMenu(List<(string buttonTitle, System.Action Action, bool interactible)> actions)
        {

            PickMenu.gameObject.SetActive(true);
            OpenPickMenu(actions, Input.mousePosition);
        }

        /// <summary>
        /// Open the pick menu and specify the location of the pick menu
        /// </summary>
        /// <param name="actions">the button actions to spawn</param>
        /// <param name="spawnLocation">the spawn location of the menu</param>
        public void OpenPickMenu(List<(string buttonTitle, System.Action Action, bool interactible)> actions, Vector2 spawnLocation)
        {
            if (actions == null || actions.Count < 1)
                actions = new List<(string buttonTitle, System.Action Action, bool interactible)>() { ("<empty>", () => { }, false) };

            PickMenu.gameObject.SetActive(true);
            PickMenu.Open(actions, new Vector3(spawnLocation.x, spawnLocation.y, 0));
        }

        /// <summary>
        /// Close the pick menu
        /// </summary>
        public void ClosePickMenu()
        {
            PickMenu.Close();
        }
    }
}
