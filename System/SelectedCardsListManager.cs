/* SelectedCardsListManager.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Unity3D;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles the list of selected cards
    /// </summary>
    public class SelectedCardsListManager
    {
        /// <summary>
        /// The list of selected objects
        /// </summary>
        /// <typeparam name="Card"></typeparam>
        /// <returns></returns>
        private List<Card> _listOfSelectedObj = new List<Card>();

        /// <summary>
        /// The cards
        /// </summary>
        /// <value></value>
        public IEnumerable<Card> Cards
        {
            get
            {
                return _listOfSelectedObj;
            }
        }

        public delegate void cardDraggingDelegate(bool dragging, Card card, object sender);
        public static event cardDraggingDelegate OnCardDragChanged;

        /// <summary>
        /// Can the sheet be edited?
        /// </summary>
        /// <value></value>
        public bool IsEditable { get; private set; }

        private bool _menuVisible = true;

        /// <summary>
        /// Can the context menu be visible?
        /// </summary>
        /// <value></value>
        public bool MenuVisible
        {
            get
            {
                if (!IsEditable || Count < 1)
                    return false;

                return _menuVisible;
            }

            set
            {
                _menuVisible = value;
                Refresh_UI();
            }
        }

        /// <summary>
        /// The count of cards selected
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                return _listOfSelectedObj.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="canEdit">can the sheet be edited?</param>
        public SelectedCardsListManager(bool canEdit)
        {
            IsEditable = canEdit;
        }

        /// <summary>
        /// Add a card to the list of selected cards
        /// </summary>
        /// <param name="card"></param>
        public void AddCard(Card card)
        {
            _listOfSelectedObj.Add(card);
            card.SelectGroupDragCallBack = DragCard;

            Refresh_UI();
            CardPropertiesDisplayManager.DisplayCards(_listOfSelectedObj);
        }

        /// <summary>
        /// Remove a card from the list of selected cards
        /// </summary>
        /// <param name="card"></param>
        public void RemoveCard(Card card)
        {
            _listOfSelectedObj.Remove(card);
            card.SelectGroupDragCallBack = null;

            Refresh_UI();
            CardPropertiesDisplayManager.DisplayCards(_listOfSelectedObj);
        }

        /// <summary>
        /// Refresh the options in the context menu and the slide windows manager
        /// </summary>
        public void Refresh_UI()
        {
            ContextMenuHandler.Refresh(this);
            // CardPropertiesDisplayManager.DisplayCards(_listOfSelectedObj);
        }

        /// <summary>
        /// Returns a selected card if there is only one in the list
        /// </summary>
        /// <returns>returns a selected card if the operation is possible, null if not</returns>
        public Card GetSelectedCard()
        {
            if (_listOfSelectedObj.Count != 1)
                return null;

            return _listOfSelectedObj[0];
        }


        /// <summary>
        /// Clear all selected cards
        /// </summary>
        /// <param name="deselect">should the selected cards be deselected?</param>
        public void ClearSelection(bool deselect = true)
        {
            if (deselect)
            {
                for (int i = _listOfSelectedObj.Count - 1; i >= 0; i--)
                {
                    _listOfSelectedObj[i].Deselect();
                }
            }

            _listOfSelectedObj.Clear();
            Refresh_UI();
        }

        /// <summary>
        /// Toggle grouped cards
        /// </summary>
        public void ToggleGroupCards()
        {
            Card[] cardCopy = new Card[_listOfSelectedObj.Count];
            System.Array.Copy(_listOfSelectedObj.ToArray(), cardCopy, cardCopy.Length);

            for (int i = 0; i < _listOfSelectedObj.Count; i++)
            {
                _listOfSelectedObj[i].ToggleGroupCards(cardCopy);
            }

            ClearSelection();
        }

        /// <summary>
        /// Get the list of selected cards
        /// </summary>
        /// <returns>returns the list of selected cards</returns>
        public List<Card> GetCards()
        {
            return _listOfSelectedObj;
        }

        /// <summary>
        /// Dragging capabilities when a card is dragged
        /// </summary>
        /// <param name="drag"></param>
        public void DragCard(bool drag, Card startCard)
        {
            if (!drag)
            {
                var grid = WarManager.Backend.SheetsManager.GetWarGrid(startCard.SheetID);


                // Debug.Log("start card " + startCard.DragFinalLocation);
                CardUtility.MoveCards(_listOfSelectedObj, Point.WorldToGrid(startCard.DragFinalLocation, grid),
                startCard.Layer, startCard.SheetID, true);

                OnCardDragChanged?.Invoke(false, startCard, this);

                Refresh_UI();
            }

            for (int i = 0; i < Count; i++)
            {
                if (_listOfSelectedObj[i] != startCard)
                    if (drag)
                    {
                        _listOfSelectedObj[i].StartDrag(true);
                    }
                    else
                    {

                        _listOfSelectedObj[i].EndDrag(true);
                    }
            }

            if (drag)
            {
                GhostCardManager.WasDragging = true;
                OnCardDragChanged?.Invoke(true, startCard, this);
            }

            GhostCardManager.Visible = !drag;

        }

        public void Print()
        {
            // if (_listOfSelectedObj == null || _listOfSelectedObj.Count < 1)
            // {
            //     MessageBoxHandler.Print_Immediate("Nothing selected to print", "Error");
            //     return;
            // }

            if (_listOfSelectedObj == null || _listOfSelectedObj.Count < 1)
            {
                CopyPasteDuplicate.Main.SelectAll(true);
            }

            EditTextMessageBoxController.OpenModalWindow("New Plan", "What is your plan title?", PrintCallBack);
        }


        /// <summary>
        /// Print out the plan
        /// </summary>
        /// <param name="title">the name (or title) of the printable plan</param>
        private void PrintCallBack(string title)
        {
            try
            {
                CardUtility.SortByGridAndVector(_listOfSelectedObj, Point.northEast);

                string fileName = title;

                if (fileName.Contains(@"\"))
                {
                    NotificationHandler.Print("Cannot create a plan with a \'\\\' in the name - \'" + title + "\' - please use a different character.");
                    return;
                }

                fileName = title.Replace('/', '-');

                HTMLPrintCards printer = new HTMLPrintCards(title, _listOfSelectedObj);

                ActiveSheetsDisplayer.main.FilePicker.SetStartingPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop));
                ActiveSheetsDisplayer.main.FilePicker.Init(new string[0], true, (x) =>
                {
                    SaveFile(x, fileName, printer);
                });
            }
            catch (System.Exception ex)
            {
                NotificationHandler.Print("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="save">is the file being saved</param>
        /// <param name="fileName">the name of the file</param>
        /// <param name="printer">the printer</param>
        private void SaveFile(bool save, string fileName, HTMLPrintCards printer)
        {
            if (save)
            {
                string path = ActiveSheetsDisplayer.main.FilePicker.SelectedPath + @"\" + fileName + " (" +
                System.Guid.NewGuid().ToString().Substring(0, 5) + ").html";

                Debug.Log(path);

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path))
                {
                    writer.Write(printer.Print());
                }

                MessageBoxHandler.Print_Immediate("File \'" + fileName + "\' saved at: " + path, "File Saved");
            }
        }
    }
}
