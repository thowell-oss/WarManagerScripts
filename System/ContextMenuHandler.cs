/* ContextMenuHandler.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Unity3D;
using WarManager.Cards;

namespace WarManager
{
    /// <summary>
    /// Handles what the context menu can show
    /// </summary>
    public static class ContextMenuHandler
    {
        public delegate void ShowContextMenu_Delegate(bool active, ContextMenuButtonState lockState,
        ContextMenuButtonState hideState, ContextMenuButtonState removeState, ContextMenuButtonState horizontalSelectState, ContextMenuButtonState verticalSelectState,
        ContextMenuButtonState shiftLeft, ContextMenuButtonState shiftRight, ContextMenuButtonState shiftUp, ContextMenuButtonState shiftDown);
        public static event ShowContextMenu_Delegate OnShowContextMenu;

        private static SelectedCardsListManager _selectedCardsManager;

        /// <summary>
        /// Can all cards selected be hidden?
        /// </summary>
        public static ContextMenuButtonState HideButtonState;

        /// <summary>
        /// Can all cards selected be locked?
        /// </summary>
        public static ContextMenuButtonState LockButtonState;

        /// <summary>
        /// Cann all cards selected be removed?
        /// </summary>
        public static ContextMenuButtonState RemoveButtonState;

        /// <summary>
        /// Select a row of cards
        /// </summary>
        public static ContextMenuButtonState HorizontalSelectState;

        /// <summary>
        /// Select a column of cards
        /// </summary>
        public static ContextMenuButtonState VerticalSelectState;

        public static ContextMenuButtonState ShiftLeft;
        public static ContextMenuButtonState ShiftRight;
        public static ContextMenuButtonState ShiftUp;
        public static ContextMenuButtonState ShiftDown;


        /// <summary>
        /// Get the options for the selected card
        /// </summary>
        public static void Refresh(SelectedCardsListManager cards)
        {
            _selectedCardsManager = cards;

            if (_selectedCardsManager == null || _selectedCardsManager.Count < 1)
            {
                HideContextMenu();
                return;
            }

            if (_selectedCardsManager.Count > 0)
            {
                HorizontalSelectState = ContextMenuButtonState.Active;
                VerticalSelectState = ContextMenuButtonState.Active;
            }
            else
            {
                HorizontalSelectState = ContextMenuButtonState.Disabled;
                VerticalSelectState = ContextMenuButtonState.Disabled;
            }


            ShiftLeft = SetShiftingState(Point.left);
            ShiftRight = SetShiftingState(Point.right);
            ShiftDown = SetShiftingState(Point.up);
            ShiftUp = SetShiftingState(Point.down);


            LockButtonState = ContextMenuButtonState.Active;
            RemoveButtonState = ContextMenuButtonState.Active;
            HideButtonState = ContextMenuButtonState.Active;

            List<Card> cardLockedList = new List<Card>();
            List<Card> cardUnlockedList = new List<Card>();

            List<Card> cardHiddenlist = new List<Card>();
            List<Card> cardNotHiddenlist = new List<Card>();

            List<Card> cardCanRemoveList = new List<Card>();
            List<Card> cardCannotRemoveList = new List<Card>();

            foreach (var card in _selectedCardsManager.GetCards())
            {
                if (card.CardLocked && card.CanLockOrUnlock)
                    cardLockedList.Add(card);

                if (!card.CardLocked && card.CanLockOrUnlock)
                    cardUnlockedList.Add(card);

                if (card.CardHidden && card.CanHide)
                    cardHiddenlist.Add(card);

                if (!card.CardHidden && card.CanHide)
                    cardNotHiddenlist.Add(card);

                if (card.CanRemove)
                    cardCanRemoveList.Add(card);

                if (!card.CanRemove)
                    cardCannotRemoveList.Add(card);
            }

            LockButtonState = SetState(cardLockedList, cardUnlockedList);
            HideButtonState = SetState(cardHiddenlist, cardNotHiddenlist);
            RemoveButtonState = SetState(cardCanRemoveList, cardCannotRemoveList, true);

            ShowContextMenu();
        }

        private static ContextMenuButtonState SetShiftingState(Point direction)
        {
            if (direction == Point.left || direction == Point.right)
                if (!GeneralSettings.AllowSideShifting)
                    return ContextMenuButtonState.Disabled;

            if (direction == Point.up || direction == Point.down)
                if (!GeneralSettings.AllowUpDownShifting)
                    return ContextMenuButtonState.Disabled;

            if (_selectedCardsManager.MenuVisible && _selectedCardsManager.Count > 0)
            {
                var cards = _selectedCardsManager.GetCards();

                foreach (var card in cards)
                {
                    if (CardUtility.CanShift(card, direction, 1))
                    {
                        return ContextMenuButtonState.Active;
                    }
                }
            }

            return ContextMenuButtonState.Disabled;
        }

        private static ContextMenuButtonState SetState(List<Card> activeList, List<Card> passiveList, bool setDisabled = false)
        {
            if (activeList == null || passiveList == null)
                return ContextMenuButtonState.Disabled;

            if (!_selectedCardsManager.MenuVisible)
                return ContextMenuButtonState.Disabled;

            if (activeList.Count > 0)
            {
                if (_selectedCardsManager.Count == 1)
                {
                    return ContextMenuButtonState.Active;
                }

                if (passiveList.Count > 0)
                {
                    return ContextMenuButtonState.Mixed;
                }
                else
                {
                    return ContextMenuButtonState.Active;
                }

            }
            else
            {
                if (passiveList.Count > 0 && !setDisabled)
                {
                    return ContextMenuButtonState.Passive;
                }
                else
                {
                    return ContextMenuButtonState.Disabled;
                }
            }
        }

        /// <summary>
        /// Hide the context menu
        /// </summary>
        private static void HideContextMenu()
        {

            LockButtonState = ContextMenuButtonState.Disabled;
            HideButtonState = ContextMenuButtonState.Disabled;
            RemoveButtonState = ContextMenuButtonState.Disabled;
            HorizontalSelectState = ContextMenuButtonState.Disabled;
            VerticalSelectState = ContextMenuButtonState.Disabled;
            ShiftLeft = ContextMenuButtonState.Disabled;
            ShiftRight = ContextMenuButtonState.Disabled;
            ShiftUp = ContextMenuButtonState.Disabled;
            ShiftDown = ContextMenuButtonState.Disabled;

            if (OnShowContextMenu != null)
                OnShowContextMenu(false, LockButtonState, HideButtonState, RemoveButtonState, HorizontalSelectState, VerticalSelectState, ShiftLeft, ShiftRight, ShiftDown, ShiftUp);
        }

        private static void ShowContextMenu()
        {
            if (OnShowContextMenu != null)
                OnShowContextMenu(true, LockButtonState, HideButtonState, RemoveButtonState, HorizontalSelectState, VerticalSelectState, ShiftLeft, ShiftRight, ShiftDown, ShiftUp);
        }

        public static void Action_Lock(ContextMenuButtonState state)
        {
            bool lockCard = false;

            if (state == ContextMenuButtonState.Passive)
            {
                lockCard = true;
            }

            var cards = _selectedCardsManager.GetCards();

            // Debug.Log(cards.Count);

            foreach (var card in cards)
            {
                card.Lock(lockCard);
            }

            Refresh(_selectedCardsManager);
            SimpleUndoRedoManager.main.NewSnapShot();
        }

        public static void Action_Hide(ContextMenuButtonState state)
        {

            return; //not properly implemented yet

            bool hideCard = false;

            if (state == ContextMenuButtonState.Passive)
            {
                hideCard = true;
            }

            foreach (var card in _selectedCardsManager.GetCards())
            {
                card.SetHide(hideCard);
            }

            Refresh(_selectedCardsManager);
            SimpleUndoRedoManager.main.NewSnapShot();
        }

        public static void Action_Remove(ContextMenuButtonState state)
        {
            if (ToolsManager.SelectedTool != ToolTypes.Edit)
                return;

            if (state == ContextMenuButtonState.Active || state == ContextMenuButtonState.Mixed)
            {
                var cards = _selectedCardsManager.GetCards();

                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    cards[i].Remove();
                }
            }

            Refresh(_selectedCardsManager);
            SimpleUndoRedoManager.main.NewSnapShot();
        }

        public static void Action_SelectRow(ContextMenuButtonState state)
        {
            SelectLine(Point.right);
            SimpleUndoRedoManager.main.NewSnapShot();
        }

        public static void Action_SelectColumn(ContextMenuButtonState state)
        {
            SelectLine(Point.down);
            SimpleUndoRedoManager.main.NewSnapShot();
        }

        /// <summary>
        /// Shift the card right one grid point
        /// </summary>
        public static void Action_ShiftRight(ContextMenuButtonState state)
        {
            Shift(Point.right);
        }

        /// <summary>
        /// Shift the card left one grid point
        /// </summary>
        public static void Action_ShiftLeft(ContextMenuButtonState state)
        {
            Shift(Point.left);
        }

        /// <summary>
        /// shift the card up one grid point
        /// </summary>
        public static void Action_ShiftUp(ContextMenuButtonState state)
        {

            Shift(Point.up);
        }

        /// <summary>
        /// shift the card down one grid point
        /// </summary>
        public static void Action_ShiftDown(ContextMenuButtonState state)
        {
            Shift(Point.down);
        }

        /// <summary>
        /// Select a line of cards
        /// </summary>
        /// <param name="direction"></param>
        private static void SelectLine(Point direction)
        {
            List<Card> cardsToSelect = new List<Card>();

            foreach (var selectedCard in _selectedCardsManager.GetCards())
            {
                cardsToSelect.AddRange(CardUtility.GetAdjacentCardsLine(selectedCard.point, direction, -1, selectedCard.Layer, selectedCard.SheetID));
            }

            foreach (var card in cardsToSelect)
            {
                card.Select(true);
            }

            Refresh(_selectedCardsManager);
        }

        /// <summary>
        /// Shift a list of cards by vector
        /// </summary>
        /// <param name="direction"></param>
        private static void Shift(Point direction)
        {
            var cards = _selectedCardsManager.GetCards();
            CardUtility.TryShiftCards(cards, direction, 1, true);
            Refresh(_selectedCardsManager);
        }
    }
}
