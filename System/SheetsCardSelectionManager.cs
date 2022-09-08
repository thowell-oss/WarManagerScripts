/* SheetsCardSelectionManager.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Unity3D;
using WarManager.Backend;

namespace WarManager
{
    /// <summary>
    /// handles selecting and managing the selection of cards for each sheet
    /// </summary>
    public class SheetsCardSelectionManager : MonoBehaviour
    {

        /// <summary>
        /// The dictionary of card selection handlers per sheet
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, SelectedCardsListManager> _selectedCardLists = new Dictionary<string, SelectedCardsListManager>();

        private Vector3 LastHoverPointerPosition;
        [SerializeField] private float PointerDragDistanceThreshold = 10;
        [SerializeField] private WarManagerCameraController CameraController;

        /// <summary>
        /// The amount of selected card lists in the dictionary
        /// </summary>
        public int SelectedCardListTotal
        {
            get
            {
                return _selectedCardLists.Count;
            }
        }

        /// <summary>
        /// Debugging... use card total instead
        /// </summary>
        [SerializeField] private int _totalCardAmt;

        /// <summary>
        /// The total amount of cards selected
        /// </summary>
        public int CardTotal
        {
            get
            {
                if (_currentList != null)
                {
                    return _currentList.Count;
                }

                return 0;
            }
        }

        /// <summary>
        /// The current selected sheet card selection handler
        /// </summary>
        private SelectedCardsListManager _currentList;

        public IEnumerable<Card> Cards
        {
            get => _currentList.Cards;
        }

        #region singleton
        public static SheetsCardSelectionManager Main;

        void Awake()
        {
            if (Main != null)
            {
                Destroy(this);
            }
            else
            {
                Main = this;
            }
        }
        #endregion


        void Update() //handle mouse dragging/selection issues
        {
            if (_currentList == null)
                return;

            if (_currentList != null)
                _totalCardAmt = _currentList.Count;

            var cards = _currentList.GetCards();

            for (int i = 0; i < _currentList.Count; i++)
            {
                Pointf p = Point.GridToWorld(cards[i].point, SheetsManager.GetWarGrid(cards[i].SheetID));

                Vector2 loc = new Vector2(p.x, p.y);

            }
        }


        /// <summary>
        /// Adds a new selection handler when a sheet is added to the cache
        /// </summary>
        /// <param name="id"></param>
        public void AddSheetSelectionHandler_EventListener(string id)
        {
            if (id != null && id != string.Empty)
            {

                if (WarSystem.CurrentSheetsManifest.TryGetFileControlSheet(id, out var manifest))
                {
                    var s = new SelectedCardsListManager(manifest.Data.CanEdit);
                    _selectedCardLists.Add(id, s);

                    OnSheetSelected_EventListener(id);
                }
                else
                {
                    throw new System.NullReferenceException("The sheet manifest is null");
                }
            }
        }

        /// <summary>
        /// Get the selected cards from a sheet
        /// </summary>
        /// <param name="id">the id</param>
        /// <returns>the list of cards</returns>
        public List<Card> GetSelectedCards(string id)
        {
            if (id == null) throw new NullReferenceException("the id is null");
            if (id == string.Empty) throw new ArgumentException("the id is empty");

            if (_selectedCardLists.ContainsKey(id))
            {
                return _selectedCardLists[id].GetCards();
            }

            return new List<Card>();
        }

        /// <summary>
        /// Changes the selected card selection handler depending on what sheet is selected
        /// </summary>
        /// <param name="id"></param>
        public void OnSheetSelected_EventListener(string id)
        {
            if (id == null || id == string.Empty)
                return;

            if (_selectedCardLists.TryGetValue(id, out var val))
            {
                _currentList = val;
                _currentList.Refresh_UI();
            }
        }

        /// <summary>
        /// Removes a selection handler when a sheet is removed from the cache
        /// </summary>
        /// <param name="id"></param>
        public void RemoveSheetSelectionHandler_EventListener(string id)
        {
            if (id != null && id != string.Empty)
            {
                _selectedCardLists.Remove(id);
            }
        }

        /// <summary>
        /// Card Select even listener
        /// </summary>
        /// <param name="selected">has the card been selected or deselected</param>
        /// <param name="c">the card being acted upon</param>
        public void CardSelect_EventListener(bool selected, bool selectMultiple, object sender)
        {
            if (sender is Card c)
            {

                if (selected)
                {
                    if (!selectMultiple)
                        _currentList.ClearSelection();

                    AddSelectedCard(c);
                    //CardPropertiesDisplayManager.DisplayRecentSelected(c);
                }
                else
                {
                    RemoveSelectedCard(c);
                }
            }

        }

        /// <summary>
        /// Add a selected card to the list of selected cards
        /// </summary>
        /// <param name="card">the selected card group</param>
        public void AddSelectedCard(Card card)
        {
            _currentList.AddCard(card);
        }

        /// <summary>
        /// Remove a selected card from the list of selected cards
        /// </summary>
        /// <param name="id">the card id</param>
        public void RemoveSelectedCard(Card c)
        {
            _currentList.RemoveCard(c);
        }

        /// <summary>
        /// Refresh the context menu
        /// </summary>
        public void RefreshContextMenu()
        {
            _currentList.Refresh_UI();
        }

        /// <summary>
        /// Group Cards
        /// </summary>
        public void ToggleGroupCards()
        {
            _currentList.ToggleGroupCards();
        }


        /// <summary>
        /// Deselect cards from the current sheet
        /// </summary>
        public void DeselectCurrent()
        {
            if (_currentList != null)
                _currentList.ClearSelection();
        }

        /// <summary>
        /// Get the list of selected cards from the current sheet
        /// </summary>
        /// <returns>returns a list of cards, an empty list if there are no cards selected</returns>
        public List<Card> GetCurrentSelectedCards()
        {
            if (_currentList != null)
            {
                return _currentList.GetCards();
            }
            else
            {
                return new List<Card>();
            }
        }

        public void Print()
        {
            _currentList.Print();
        }

        /// <summary>
        /// if not in edit mode or highlight mode, the context menu should not show
        /// </summary>
        /// <param name="toolTypes"></param>
        public void CheckTools(ToolTypes toolTypes)
        {
            if (_currentList != null)
            {
                if (toolTypes != ToolTypes.Edit)
                {
                    _currentList.MenuVisible = false;
                }
                else
                {
                    _currentList.MenuVisible = true;
                }
            }
        }

        public void OnEnable()
        {
            ToolsManager.OnToolSelected += CheckTools;
            Card.OnSelectCard += CardSelect_EventListener;
            SheetsManager.OnSetSheetCurrent += OnSheetSelected_EventListener;
            SheetsManager.OnOpenCardSheet += AddSheetSelectionHandler_EventListener;
            SheetsManager.OnCloseCardSheet += RemoveSheetSelectionHandler_EventListener;
        }

        public void OnDisable()
        {
            if (_currentList != null)
                _currentList.ClearSelection();

            ToolsManager.OnToolSelected -= CheckTools;
            Card.OnSelectCard -= CardSelect_EventListener;
            SheetsManager.OnSetSheetCurrent -= OnSheetSelected_EventListener;
            SheetsManager.OnOpenCardSheet -= AddSheetSelectionHandler_EventListener;
            SheetsManager.OnCloseCardSheet -= RemoveSheetSelectionHandler_EventListener;
        }
    }
}
