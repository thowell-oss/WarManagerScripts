/* ToolsManager.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;

namespace WarManager
{
    /// <summary>
    /// Handles tools selection and associated events
    /// </summary>
    public static class ToolsManager
    {
        public static WarMode Mode;

        //event setup
        public delegate void toolSelected(ToolTypes type);
        public static event toolSelected OnToolSelected;

        /// <summary>
        /// stores the current selected tool
        /// </summary>
        private static ToolTypes _selectedTool;

        /// <summary>
        /// The currently selected tool
        /// </summary>
        public static ToolTypes SelectedTool
        {
            get
            {
                return _selectedTool;
            }

            set
            {
                // UnityEngine.Debug.Log("Changing selected tool to " + value);

                if (Mode == WarMode.Sheet_Editing)
                    SelectTool(value);
            }
        }

        /// <summary>
        /// The previous selected tool
        /// </summary>
        public static ToolTypes PreviousTool { get; private set; }

        /// <summary>
        /// Call a event when a new tool is selected
        /// </summary>
        /// <param name="type">the tool selected</param>
        public static void SelectTool(ToolTypes type, bool Override = false)
        {
            if ((SelectedTool == ToolTypes.None || Mode != WarMode.Sheet_Editing) && !Override)
                return;

            if (OnToolSelected != null)
            {
                OnToolSelected.Invoke(type);
            }

            PreviousTool = _selectedTool;
            _selectedTool = type;

        }
    }
}

