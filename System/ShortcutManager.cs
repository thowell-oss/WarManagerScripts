/* KeyboardShortcutManager.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using WarManager.Unity3D;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager
{
    /// <summary>
    /// Manages the keyboardshortcuts for the warmanager
    /// </summary>
    public class ShortcutManager : MonoBehaviour
    {
        /// <summary>
        /// Controls when certian shortcuts can be applied
        /// </summary>
        public WarMode CurrentMode { get; set; }

        /// <summary>
        /// The previous tool that was used
        /// </summary>
        private ToolTypes PrevTool;

        public CopyPasteDuplicate cardCommands;

        public DropMenu SearchMenu;

        /// <summary>
        /// boolean to go back to prev tool or not
        /// </summary>
        private bool canSelectPrevTool;

        /// <summary>
        /// trigger to select the previous tool
        /// </summary>
        private bool selectPrevTool;

        void Update() //called every frame
        {
            if (selectPrevTool)
            {
                ToolsManager.SelectTool(PrevTool);
                selectPrevTool = false;
            }



            if (CurrentMode == WarMode.Sheet_Editing && ToolsManager.SelectedTool != ToolTypes.None)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (UndoAndRedo())
                        return;

                    if (Find())
                        return;

                    if (Save())
                        return;

                    if (Duplicate())
                        return;

                    if (SelectAll())
                        return;

                    if (ToggleGroupCards())
                        return;

                    if (Print())
                        return;

                    if (Lock())
                        return;

                    if (Delete())
                        return;

                    if (Copy())
                        return;

                    if (Paste())
                        return;

                    if (Quit())
                        return;
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (NewCard())
                        return;

                    if (NewTitleCard())
                        return;

                    if (NewNoteCard())
                        return;
                }
                else
                {
                    SelectATool();
                }
            }
        }

        /// <summary>
        /// Select tool shortcuts
        /// </summary>
        private void SelectATool()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                ToolsManager.SelectTool(ToolTypes.Edit);
                OnScreenInputGUI.main.PrintOnScreenInfo("V - Edit");
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                ToolsManager.SelectTool(ToolTypes.Pan);
                OnScreenInputGUI.main.PrintOnScreenInfo("H - Pan");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                ToolsManager.SelectTool(ToolTypes.Highlight);
                OnScreenInputGUI.main.PrintOnScreenInfo("B - Highlight");
            }
        }

        /// <summary>
        /// Open the new card menu
        /// </summary>
        /// <returns></returns>
        private bool NewCard()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("New card");
                GhostCardManager.NewCard();
                OnScreenInputGUI.main.PrintOnScreenInfo("Shift + A - New Card");
                return true;
            }

            return false;
        }

        /// <summary>
        ///Drops the note card
        /// </summary>
        /// <returns></returns>
        private bool NewNoteCard()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                OnScreenInputGUI.main.PrintOnScreenInfo("Shift + N - New Note Card");
                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    CardUtility.TryDropNoteCard(sheet, GhostCardBehavior.Main.Location, sheet.CurrentLayer, out var id);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Drops the title card
        /// </summary>
        /// <returns></returns>
        private bool NewTitleCard()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                OnScreenInputGUI.main.PrintOnScreenInfo("Shift + T - New Title Card");
                if (SheetsManager.TryGetCurrentSheet(out var sheet))
                {
                    CardUtility.TryDropTitleCard(sheet, GhostCardBehavior.Main.Location, sheet.CurrentLayer, out var id);
                }
                return true;
            }

            return false;
        }



        /// <summary>
        /// Undo and redo shortcuts
        /// </summary>
        private bool UndoAndRedo()
        {


            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    //UndoRedo.Redo();
                    SimpleUndoRedoManager.main.Redo();
                    OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + Shift + Z - Redo");
                    return true;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    //UndoRedo.Undo();
                    SimpleUndoRedoManager.main.Undo();
                    OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + Z - Undo");
                    // Debug.Log("undo");
                    return true;
                }
            }

            return false;

        }

        private bool Copy()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                cardCommands.CopySelectedCards();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + C - Copy");
                return true;
            }

            return false;
        }

        private bool Paste()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                cardCommands.Paste();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + V - Paste");
                return true;
            }

            return false;
        }

        private bool Lock()
        {
            bool lockCards = false;
            bool completed = false;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    lockCards = false;
                    completed = true;
                    OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + Alt + L - Unlock");
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    lockCards = true;
                    completed = true;
                    OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + L - Lock");
                }
            }

            if (completed)
            {
                cardCommands.Lock(lockCards);
                SimpleUndoRedoManager.main.NewSnapShot();
            }


            return completed;
        }

        private bool Save()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SheetsManager.SaveAllSheets();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + S - Save");
                return true;
            }

            return false;
        }

        private bool Delete()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                cardCommands.Remove();
                SimpleUndoRedoManager.main.NewSnapShot();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + Del - Remove");
                return true;
            }

            return false;
        }

        private bool Duplicate()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                cardCommands.Duplicate();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + D - Duplicate");
                return true;
            }

            return false;
        }

        private bool SelectAll()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                cardCommands.SelectAll(SheetsCardSelectionManager.Main.CardTotal < 1);
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + A - Toggle Select");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Group cards together - they will function and together even if they're not similar
        /// </summary>
        /// <returns></returns>
        private bool ToggleGroupCards()
        {
            if (Input.GetKey(KeyCode.G))
            {
                //SheetsCardSelectionManager.Main.ToggleGroupCards();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Find a specific card on a sheet (shortcut)
        /// </summary>
        private bool Find()
        {
            if (Input.GetKey(KeyCode.F))
            {
                if (!SearchMenu.ActiveMenu && SheetsManager.SheetCount > 0)
                {
                    SearchMenu.ToggleActive();
                    OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + F - Find");
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// print out data from sheet
        /// </summary>
        /// <returns></returns>
        private bool Print()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SheetsCardSelectionManager.Main.Print();
                OnScreenInputGUI.main.PrintOnScreenInfo("Ctrl + R - Generate Dashboard");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Quit War Manager
        /// </summary>
        /// <returns></returns>
        private bool Quit()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Application.Quit();

            }

            return true;
        }
    }

    /// <summary>
    /// The types of sitations where the keyboard shortcuts may or may not apply
    /// </summary>
    public enum WarMode
    {
        /// <summary>
        /// The Focus is sheet editing 
        /// </summary>
        Sheet_Editing,
        /// <summary>
        /// The Focus is on a menu
        /// </summary>
        Menu,
    }
}
