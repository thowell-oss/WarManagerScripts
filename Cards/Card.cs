/* Card.cs
 * Author: Taylor Howell
 */
using System;
using System.Collections.Generic;
using System.Text;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

/// <summary>
/// Handles the cards behavior
/// </summary>
namespace WarManager.Cards
{
    /// <summary>
    /// The main card class
    /// </summary>
    [Notes.Author("Handles the backend part of the card info")]
    public class Card : ICompareWarManagerPoint, IComparable<Card>, IFileContentInfo, ISelectable
    {
        /// <summary>
        /// The card box calculations of both position and size
        /// </summary>
        public CardLayout Layout { get; private set; }

        /// <summary>
        /// The animator driver for the card
        /// </summary>
        public CardAnimation Animator { get; private set; }

        /// <summary>
        /// The information that the card is displaying
        /// </summary>
        public CardMakeup MakeUp { get; set; }

        /// <summary>
        ///  The card type/name other information
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The id of the dataset the card is displaying
        /// </summary>
        public string DatasetID { get; private set; }

        /// <summary>
        /// private backing field
        /// </summary>
        private DataSet _dataSet = null;

        /// <summary>
        /// Handles connections between cards to describe relationships and pass data
        /// </summary>
        /// <value></value>
        public ConnectionHandler CardConnectionHandler { get; private set; }

        /// <summary>
        /// The Data Set associated with this card
        /// </summary>
        /// <value></value>
        public DataSet DataSet
        {
            get
            {
                if (_dataSet == null && WarSystem.DataSetManager != null)
                {
                    _dataSet = WarSystem.DataSetManager.GetDataSetFromCard(this);
                }
                else if (WarSystem.DataSetManager == null)
                    throw new DataSetManagerMissingException("Cannot find the data set manager");


                return _dataSet;
            }
        }

        /// <summary>
        /// The specific row of data that is being displayed by the card.
        /// </summary>
        /// <value></value>
        public string RowID { get; private set; }

        /// <summary>
        /// private backing field
        /// </summary>
        private DataEntry _entry = null;

        /// <summary>
        /// The data entry associated with this card
        /// </summary>
        /// <value></value>
        public DataEntry Entry
        {
            get
            {
                if (_entry == null && WarSystem.DataSetManager != null)
                {
                    _entry = WarSystem.DataSetManager.GetDataEntryFromCard(this);
                }
                else if (WarSystem.DataSetManager == null)
                    throw new DataSetManagerMissingException("Cannot find dataset manager");

                return _entry;
            }
        }

        /// <summary>
        /// The sheet the card is located on
        /// </summary>
        /// <value></value>
        public string SheetID { get; set; }

        /// <summary>
        /// The card is being dragged?
        /// </summary>
        public bool CardMouseDrag { get; private set; }

        /// <summary>
        /// Can the card be hidden?
        /// </summary>
        public bool CanHide { get; set; } = true;

        /// <summary>
        /// Is the card hidden?
        /// </summary>
        public bool CardHidden { get; set; }

        /// <summary>
        /// Can the card be locked?
        /// </summary>
        public bool CanLockOrUnlock { get; set; } = true;

        /// <summary>
        /// Is the card locked?
        /// </summary>
        public bool CardLocked { get; set; }

        /// <summary>
        /// Can the card be removed?
        /// </summary>
        public bool CanRemove { get; set; } = true;

        /// <summary>
        /// private backing field
        /// </summary>
        private Pointf _curfecp;

        /// <summary>
        /// The world position that the card is located when it is being dragged
        /// </summary>
        public Pointf CurrentFrontEndCardPosition
        {
            get
            {
                return _curfecp;
            }

            set
            {
                _curfecp = value;

                // UnityEngine.Debug.Log("update: " + _curfecp);
            }
        }

        /// <summary>
        /// Gets a card location offset when dragging during card stretching
        /// </summary>
        public Pointf FrontEndOffset { get; set; }

        /// <summary>
        /// The final drag location of card in relation to the front end location of the unity card
        /// </summary>
        /// <value></value>
        public Pointf DragFinalLocation
        {
            get
            {
                if (!CardStretched)
                {
                    return CurrentFrontEndCardPosition;
                }
                else
                {
                    // UnityEngine.Debug.Log(CurrentFrontEndCardPosition + " " + FrontEndOffset);
                    return CurrentFrontEndCardPosition - FrontEndOffset;
                }
            }
        }

        /// <summary>
        /// The list of card groups the card is apart of
        /// </summary>
        public List<Card> GroupCardsList = new List<Card>();


        /// <summary>
        /// Card stretching bounds
        /// </summary>
        public Rect CardStretchBounds;

        /// <summary>
        /// Can the Card be stretched?
        /// </summary>
        /// <value></value>
        public bool CanStretch { get; set; } = true;

        /// <summary>
        /// Is the Card Stretched?
        /// </summary>
        /// <value></value>
        public bool CardStretched
        {
            get
            {
                if (CardStretchBounds != null)
                    return CardStretchBounds.SpacesTaken().Count > 1;
                return false;
            }
        }

        /// <summary>
        /// Is this card grouped with other cards?
        /// </summary>
        /// <value></value>
        public bool Grouped
        {
            get
            {
                if (GroupCardsList != null && GroupCardsList.Count > 0)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Can the card be shifted?
        /// </summary>
        /// <value></value>
        public bool CanShift
        {
            get
            {
                if (CardLocked || CardHidden)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Can the user view details?
        /// </summary>
        public bool CanViewDetails { get; set; }

        /// <summary>
        /// The card reference location in the grid
        /// </summary>
        public Point point
        {
            get
            {
                if (Layout != null)
                    return Layout.point;

                return new Point(0, 0);
            }

            set
            {
                if (Layout != null)
                {
                    Layout.point = value;

                    if (CardStretched)
                    {
                        UpdateStretchRect(true);
                    }
                }
            }
        }

        private Layer _currentLayer;

        public Layer Layer
        {
            get
            {
                return _currentLayer;
            }

            set
            {
                _currentLayer = value;
                //change to a different layer here
            }
        }

        /// <summary>
        /// Is the card in the selection state
        /// </summary>
        /// <value></value>
        public bool Selected { get; private set; }

        /// <summary>
        /// Is the card being represented in the front end?
        /// </summary>
        /// <value></value>
        public bool CardRepresented { get; set; }

        /// <summary>
        /// Is the card being dragged?
        /// </summary>
        /// <value></value>
        public bool CardDragging { get; private set; }

        /// <summary>
        /// The call back which is called when the card is removed from the sheet
        /// </summary>
        public Action RemoveCallBack;

        /// <summary>
        /// The call back which is called when the card is set locked or unlocked
        /// </summary>
        public Action<bool> LockCallBack;

        /// <summary>
        /// The call back when the card is selected/deselected
        /// </summary>
        public Action<bool, Card> CardSelectedCallBack;

        /// <summary>
        /// Call back when the card is hidden/not hidden
        /// </summary>
        public Action<bool> CardHideCallBack;

        /// <summary>
        /// Called when the card gets shifted to a new point
        /// </summary>
        public Action<Point> CardShiftCallback;

        /// <summary>
        /// called when the card is being dragged
        /// </summary>
        public Action<bool, Card> DragCard;

        /// <summary>
        /// When the card changes (int is LOD)
        /// </summary>
        public Action<int> UpdateCallback;

        public Action Action_SetUICardToInputLocation;

        /// <summary>
        /// The group drag call back 
        /// </summary>
        public Action<bool, Card> SelectGroupDragCallBack;

        public delegate void SelectCardDelegate(bool isSelected, bool multipleSelected, object sender);
        public static event SelectCardDelegate OnSelectCard;

        #region Instantiation

        /// <summary>
        /// Blank card
        /// </summary>
        public Card()
        {
            Init(new Point(0, 0), Guid.NewGuid().ToString());
        }

        public Card(string[] args)
        {
            SetContent(args);
        }

        /// <summary>
        /// Setup card with location
        /// </summary>
        /// <param name="position">the location of the card</param>
        public Card(Point position)
        {
            Guid g = Guid.NewGuid();

            Init(position, g.ToString());
        }

        /// <summary>
        /// Setup card with location and id
        /// </summary>
        /// <param name="position">the grid position of the card</param>
        /// <param name="id">the id name of the card</param>
        public Card(Point position, string id, string sheetId, string layerId)
        {
            Init(position, id, sheetId, layerId);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">the position of the card</param>
        /// <param name="id">the id of the card</param>
        /// <param name="sheetId">the id of the sheet</param>
        /// <param name="layerId">the layer of the sheet</param>
        /// <param name="dataSetID">the data set id the card will represent</param>
        /// <param name="dataRepId">the data representation id</param>
        public Card(Point position, string id, string sheetId, string layerId, string dataSetID, string dataRepId)
        {
            Init(position, id, sheetId, layerId, dataSetID, dataRepId);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">the position of the card</param>
        /// <param name="id">the id of the card</param>
        /// <param name="sheetId">the id of the sheet</param>
        /// <param name="layerId">the layer of the sheet</param>
        /// <param name="entry">the data entry</param>
        public Card(Point position, string id, string sheetId, string layerId, DataEntry entry)
        {
            Init(position, id, sheetId, layerId, entry.DataSet.ID, entry.RowID.ToString());
            _entry = entry;
            _dataSet = entry.DataSet;
        }

        /// <summary>
        /// Initialize card
        /// </summary>
        /// <param name="cardPoint">the card position</param>
        /// <param name="cardId">the card id</param>
        /// <param name="dataTag">the data tag (which database did this come from?)</param>
        /// <param name="displayElements">the associated display elements that the card will display</param>
        private void Init(Point cardPoint, string cardId, string sheetID = "n", string layerId = "n", string dataTag = "n", string rowID = "n", CardElementData[] displayElements = null)
        {
            // UnityEngine.Debug.Log(dataRepresentation);

            ToolsManager.OnToolSelected += ChangeTool;

            if (!cardPoint.IsInGridBounds)
            {
                NotificationHandler.Print("card point " + cardPoint + " is not in bounds - redirecting location to (0,0).");
            }

            Layout = new CardLayout(cardPoint.x, cardPoint.y);


            if (!WarSystem.DataSetManager.ContainsDataSetID(dataTag))
            {
                MessageBoxHandler.Print_Immediate("Cannot add a card from a dataset you cannot access", "Error");
                return;
            }

            if (WarSystem.DataSetManager.TryGetDataset(dataTag, out var set))
            {

                if (set.SelectedView.CanEditCard && set.SelectedView.CanViewCard)
                {

                    ID = cardId;
                    DatasetID = dataTag;

                    // UnityEngine.Debug.Log(DatasetID);

                    // if (long.TryParse(dataRepresentation, out var dataIdTemp))
                    // {
                    //     DataRepID = dataIdTemp;
                    // }
                    // else
                    // {
                    //     DataRepID = -1;
                    // }

                    //UnityEngine.Debug.Log("rep id " + rowID);

                    if (rowID != null && rowID != string.Empty)
                        RowID = rowID;
                    else
                        RowID = string.Empty;

                    if (sheetID != "n" && layerId != "n")
                    {
                        var sheet = SheetsManager.GetActiveSheet(sheetID);
                        var sheetMetaData = SheetsManager.GetSheetMetaData(sheetID);

                        if (sheet != null && sheetMetaData.CanEdit)
                        {
                            SheetID = sheetID;
                            if (sheet.LayerExists(layerId))
                            {
                                Layer = sheet.GetLayer(layerId);

                                if (MakeUp == null)
                                {
                                    MakeUp = new CardMakeup();
                                    MakeUp.Initialize(this);

                                    CardConnectionHandler = new ConnectionHandler();
                                    CardConnectionHandler.Init(this);

                                    //Unity3D.CardPoolManager.Main.ReconfigureCard(this, sheetID);
                                }
                                else
                                {
                                    throw new NullReferenceException("the info display is already created");
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.Log("Layer does not exist " + layerId);
                            }
                        }
                        else if (sheet != null && !sheetMetaData.CanEdit)
                        {
                            MessageBoxHandler.Print_Immediate("Cannot add a card to a sheet that cannot be edited", "Error");
                            UnityEngine.Debug.Log("Cannot add a card to a sheet that cannot be edited");
                        }
                        else
                        {
                            throw new NullReferenceException("the sheet you are trying to add this card to is null");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Sheet id and/or layer id null " + sheetID + " " + layerId);
                    }
                }
                else
                {
                    MessageBoxHandler.Print_Immediate("Cannot add a card from a data set you cannot view or edit", "Error");
                    UnityEngine.Debug.Log("Cannot add a card from a data set you cannot view or edit");
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the card id and location
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Card: {ID} - ({Layout.point.x}, {Layout.point.y})";
        }

        /// <summary>
        /// Modified ToString Method -> shows the card id and location on the grid
        /// </summary>
        /// <param name="idLength">the length of the ID to show</param>
        /// <returns>retrns the resulting string</returns>
        public string ToString(int idLength)
        {
            return $"Card: {ID.Substring(0, idLength)} - ({Layout.point.x}, {Layout.point.y})";
        }

        /// <summary>
        /// Pass to update the objects values over time (designed for fixed update)
        /// </summary>
        public void CallUpdate()
        {

        }

        /// <summary>
        /// Set the card to a drag state
        /// </summary>
        /// <param name="drag"></param>
        public void SetCardDrag(bool drag) // for dragging purposes
        {
            CardMouseDrag = drag;
            //Animator.SetCardDrag();
        }


        /// <summary>
        /// Forces the card to set itself equal to the input location on screen
        /// </summary>
        public void SetCardToInputPosition()
        {
            if (Action_SetUICardToInputLocation != null)
                Action_SetUICardToInputLocation();
        }

        /// <summary>
        /// Get reference the content from the card to be inserted into the sheet
        /// </summary>
        /// <returns></returns>
        public string[] GetContent() // convert a usable card into a stringBuilder
        {
            string pnt = point.ToString(false);
            return new string[] { point.x.ToString(), (-point.y).ToString(), Layer.ID, DatasetID, RowID.ToString() };
        }

        public bool SetContent(string[] args) // convert a string builder into a usable card
        {

            //WarSystem.WriteToLog("Set Card Content: \n" + string.Join(",", args), logging.MessageType.info);

            var sheetId = args[0];
            var xStr = args[1];
            var yStr = args[2];
            var layerID = args[3];
            var dataSetId = args[4];
            var dataRepId = args[5];

            string id = xStr + "" + yStr;

            int x = 0;
            int y = 0;

            //UnityEngine.Debug.Log("(" + xStr + ", " + yStr + ")");


            if (int.TryParse(xStr, out x))
            {
                if (int.TryParse(yStr, out y))
                {
                    //get data set stuff here

                    Init(new Point(x, -y), id, sheetId, layerID, dataSetId, dataRepId, null);
                    //UnityEngine.Debug.Log("Added data");

                    return true;
                }
            }

            UnityEngine.Debug.Log("Could not add data");

            return false;
        }

        /// <summary>
        /// Generate and return a snap shot of the card for reference
        /// </summary>
        /// <param name="nextPosition">the next position of the card (if moving the card)</param>
        /// <param name="nextLayer">the next layer of the card (if moving the card)</param>
        /// <param name="nextSheet">the next sheet of the card (if moving the card)</param>
        /// <param name="removed">was the card removed?</param>
        /// <param name="added">was the card added?</param>
        /// <returns></returns>
        public SnapShot GetSnapShot(bool removed, bool added)
        {
            if (added && removed)
                throw new NullReferenceException("A card cannot be added and removed at the same time");

            SnapShot s = new SnapShot();
            s.CardReference = this;
            s.Locked = CardLocked;
            s.Hidden = CardHidden;
            s.Selected = Selected;
            s.Removed = removed;
            s.Added = added;

            s.CurrentPoint = point;
            s.CurrentLayer = Layer;
            s.CurrentSheet = SheetID;

            return s;
        }

        /// <summary>
        /// Apply the snapshot to the card
        /// </summary>
        /// <param name="snapShot">the snapshot</param>
        public void ApplySnapShot(SnapShot snapShot)
        {
            if (snapShot == null)
                throw new NullReferenceException("The snapshot cannot be null");

            if (snapShot.CardReference != this)
                throw new NotSupportedException("The card reference is not the same as the card the snapshot is being applied to");

            CardLocked = snapShot.Locked;
            CardHidden = snapShot.Hidden;

            point = snapShot.CurrentPoint;
            Layer = snapShot.CurrentLayer;
            SheetID = snapShot.CurrentSheet;


            if (snapShot.Selected)
            {
                Select(true); //fix the selection issue
            }
            else
            {
                Deselect();
            }

            UpdateCallback(2);
        }

        /// <summary>
        /// Delete a card from the sheet entirely
        /// </summary>
        public void Remove()
        {
            if (!CanRemove || !Selected || CardLocked || CardHidden)
                return;

            if (CardUtility.RemoveCard(point, Layer, SheetID))
            {

                if (Selected)
                    Deselect();

                if (Entry.Actor != null)
                {
                    Entry.Actor.OnSleep();

                    WarSystem.ActiveCardActors.Remove(this);
                }

                // if (RemoveCallBack != null) //activated from the card utility
                //     RemoveCallBack();

            }
            else
            {
                UnityEngine.Debug.LogError("Could not delete card");
            }
        }

        /// <summary>
        /// Set the card to be locked or unlocked
        /// </summary>
        /// <param name="setLock"></param>
        public void Lock(bool setLock)
        {
            if (!CanLockOrUnlock || CardHidden)
                return;

            // UnityEngine.Debug.Log("Locking card: " + setLock);

            CardLocked = setLock;

            CanRemove = !setLock;

            if (LockCallBack != null)
            {
                LockCallBack(setLock);
            }
            else
            {
                UnityEngine.Debug.Log("Lock call back not ready");
            }
        }

        public void SetHide(bool hidden)
        {
            if (CardLocked || !CanHide || ToolsManager.SelectedTool != ToolTypes.Edit) // && ToolsManager.SelectedTool != ToolTypes.Calculate)) <- this is actually the select tool for some reason...
                return;

            if (Selected)
                Deselect();

            if (CardHideCallBack != null)
                CardHideCallBack(hidden);
        }

        /// <summary>
        /// Toggle select and deselect
        /// </summary>
        public void ToggleSelect()
        {
            if (Selected)
            {
                Deselect();
            }
            else
            {
                Select(false);
            }
        }

        /// <summary>
        /// Handles getting selected
        /// </summary>
        /// <returns>returns true if the card is selected</returns>
        public bool Select(bool selectMultiple)
        {
            if (CardHidden)
            {
                return false;
            }

            if (!CanLockOrUnlock && CardLocked)
            {
                return false;
            }

            if (Selected)
            {
                return true;
            }


            Selected = true;


            if (OnSelectCard != null)
                OnSelectCard(true, selectMultiple, this);

            if (Grouped)
            {
                foreach (var card in GroupCardsList)
                {
                    card.Select(true);
                }
            }

            if (CardSelectedCallBack != null)
                CardSelectedCallBack(true, this);

            return true;
        }

        /// <summary>
        /// Calls the shift card action delegate
        /// </summary>
        /// <param name="direction">the direction of the shift</param>
        /// <param name="distance">the distance of the shift</param>
        /// <returns>returns true if the shift was successful</returns>
        public bool CardShifted(Point newPosition)
        {
            if (CardLocked || CardHidden)
                return false;


            if (CardShiftCallback != null)
                CardShiftCallback(newPosition);

            return true;
        }


        /// <summary>
        /// Start dragging the card
        /// </summary>
        /// <returns></returns>
        public bool StartDrag(bool calledFromAnotherCard)
        {
            if (!Selected)
                return false;

            if (CardDragging)
                return true;

            if (DragCard != null)
                DragCard(true, this);

            //UnityEngine.Debug.Log("dragging");

            if (CardStretched)
            {
                // UnityEngine.Debug.Log(GroupStretchCardsList.Count);

                var grid = SheetsManager.GetWarGrid(SheetID);




                //foreach (var card in GroupStretchCardsList)
                //{
                //    Pointf cardLoc = Pointf.GridToWorld(card.point, grid);

                //    card.FrontEndOffset = new Pointf(CurrentFrontEndCardPosition.x - cardLoc.x, CurrentFrontEndCardPosition.y - cardLoc.y);
                //    UnityEngine.Debug.Log("Start drag " + card.FrontEndOffset + " " + CurrentFrontEndCardPosition + " " + cardLoc);
                //}
            }

            if (SelectGroupDragCallBack != null && !calledFromAnotherCard)
            {
                SelectGroupDragCallBack(true, this);
            }

            CardDragging = true;

            try
            {
                if (Entry.Actor != null)
                {
                    Entry.Actor.OnStartDrag();
                }
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }

            return true;
        }

        /// <summary>
        /// End the card dragging
        /// </summary>
        public void EndDrag(bool calledFromAnotherCard)
        {
            if (!CardDragging)
                return;

            if (CardStretched && !calledFromAnotherCard)
            {
                UpdateStretchRect(true);
            }

            if (DragCard != null)
                DragCard(false, this);

            if (SelectGroupDragCallBack != null && !calledFromAnotherCard)
                SelectGroupDragCallBack(false, this);

            try
            {
                if (Entry.Actor != null)
                {
                    Entry.Actor.OnDrop();
                }
            }
            catch (Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }

            CardDragging = false;
        }

        public void ChangeTool(ToolTypes types)
        {
            // if (ToolsManager.SelectedTool == ToolTypes.Edit || ToolsManager.SelectedTool == ToolTypes.Calculate) //EDIT is actually pan for some reason
            //     Deselect();
        }

        /// <summary>
        /// Handles getting deselected
        /// </summary>
        /// <returns>returns true if the card is not selected</returns>
        public bool Deselect()
        {
            if (!Selected)
                return true;

            Selected = false;

            if (OnSelectCard != null)
                OnSelectCard(false, false, this);

            if (CardSelectedCallBack != null)
                CardSelectedCallBack(false, this);

            return true;
        }

        /// <summary>
        /// Toggle grouping
        /// </summary>
        /// <param name="cards">the list of cards to group</param>
        /// <returns>returns true if the cards have been grouped, false if the cards ungrouped</returns>
        public bool ToggleGroupCards(Card[] cards)
        {
            bool toggled = false;

            if (!Grouped && !CardLocked)
            {
                GroupCardsList.AddRange(cards);
                toggled = true;
            }
            else if (Grouped)
            {
                GroupCardsList.Clear();
            }

            if (!CardLocked)
                UpdateCallback(1);


            return toggled;
        }

        /// <summary>
        /// update the stretch rect (based on the cards apart of the stretch rect)
        /// </summary>
        /// <param name="start"></param>
        public void UpdateStretchRect(bool start)
        {
            if (CardStretched && UpdateCallback != null)
            {
                List<Point> pointsList = new List<Point>();

                //foreach (var card in GroupStretchCardsList)
                //{
                //    var grid = SheetsManager.GetWarGrid(card.SheetID);
                //    Point p = Point.WorldToGrid(card.DragFinalLocation, grid);
                //    pointsList.Add(p);
                //}

                CardStretchBounds = Rect.DrawRectFromListOfSpaces(pointsList);
                UpdateCallback(1);
            }
            else if (!start)
            {
                //foreach (var card in GroupStretchCardsList)
                //{
                //    card.UpdateStretchRect(false);
                //}
            }
        }

        /// <summary>
        /// Set the front end location
        /// </summary>
        /// <param name="location">the location of the front end</param>
        public void SetCardFrontEndPosition(Pointf location)
        {
            CurrentFrontEndCardPosition = location;

            if (CardStretched)
            {
                //foreach (var card in GroupStretchCardsList)
                //{
                //    card.CurrentFrontEndCardPosition = location;
                //}
            }
        }


        public int CompareX(float x) // IGridComparable
        {
            if (x < Layout.point.x)
                return -1;
            if (x == Layout.point.x)
                return 0;

            return 1;
        }

        public int CompareY(float y) // IGridComparable
        {
            if (y < Layout.point.y)
                return -1;
            if (y == Layout.point.y)
                return 0;

            return 1;
        }

        public int CompareLayer(Layer layer) //IGrid Comparable
        {
            return Layer.CompareTo(layer);
        }

        public int CompareTo(Card obj) // compare cards used for sorting lists
        {

            if (point.x > obj.point.x)
            {
                return 1;
            }
            else if (point.x == obj.point.x)
            {
                if (point.y < obj.point.y)
                {
                    return 1;
                }
                else if (point.y == obj.point.y)
                {
                    return 0;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the all allowed data from the data entry and convert it into a string
        /// </summary>
        /// <returns>returns a string</returns>
        public string GetAllowedEntryToString(bool psudoCSV = false)
        {
            StringBuilder b = new StringBuilder();

            if (WarSystem.DataSetManager != null && WarSystem.DataSetManager.TryGetDataset(DatasetID, out var dataset))
            {

                UnityEngine.Debug.Log(DataSet.DatasetName);


                var data = dataset.GetEntry(RowID);
                UnityEngine.Debug.Log(RowID);

                var values = data.GetHeaderValuePairs();
                var allowedTags = dataset.AllowedTags;

                if (psudoCSV)
                {
                    foreach (var v in allowedTags)
                    {
                        if (values.ContainsKey(v))
                        {
                            b.Append(values[v].Value.ToString());
                            b.Append(",");
                        }
                    }
                }
                else
                {
                    foreach (var v in allowedTags)
                    {
                        if (values.ContainsKey(v))
                        {
                            b.Append(values[v].HeaderName);
                            b.Append(": ");
                            b.Append(values[v].Value.ToString());
                            b.Append("\n");
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("does not contain " + v);
                        }
                    }
                }
            }

            return b.ToString();
        }

        public void ClearCache()
        {
            _entry = null;
            _dataSet = null;
        }
    }

    /// <summary>
    /// How is the card related to other cards?
    /// </summary>
    public enum CardStretching
    {
        /// <summary>
        /// The card is apart of a stretched card group
        /// </summary>
        strectched,

        /// <summary>
        /// This card can participate in card stretching
        /// </summary>
        enabled,

        /// <summary>
        /// This card does not stretch to other cards
        /// </summary>
        disabled,
    }
}
