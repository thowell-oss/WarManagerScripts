
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    /// <summary>
    /// The actor class
    /// </summary>
    [Notes.Author("the actor class")]
    public abstract class Actor : IComparable<Actor>, IEquatable<Actor>
    {
        /// <summary>
        /// The name of the actor
        /// </summary>
        /// <value></value>
        public abstract string Name { get; }

        /// <summary>
        /// The description of the actor
        /// </summary>
        /// <value></value>
        public abstract string Description { get; }

        /// <summary>
        /// The id of the Actor
        /// </summary>
        /// <value></value>
        public Guid ID { get; private set; }


        /// <summary>
        /// The dataset ID. Usually would come from the card, but if it is an action card, then it is handled by the child class
        /// </summary>
        public abstract string DataSetID { get; }

        /// <summary>
        /// Is the actor sleeping?
        /// </summary>
        /// <value></value>
        public bool Sleeping { get; private set; } = true;

        /// <summary>
        /// Has the actor been initialized?
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Is the actor card enabled?
        /// </summary>
        public bool Enabled => GeneralSettings.EnableActors;

        /// <summary>
        /// The card associated with the actor
        /// </summary>
        /// <value></value>
        public Card Card { get; private set; }

        /// <summary>
        /// There was an error
        /// </summary>
        /// <value></value>
        public bool Error { get; protected set; } = false;

        private string _currentSheetID = "";

        /// <summary>
        /// The id of the current sheet
        /// </summary>
        /// <value></value>
        protected string CurrentSheetID
        {
            get => _currentSheetID;

            private set
            {
                if (value != null && value.Trim().Length > 0 && value != _currentSheetID)
                {
                    _currentSheetID = value;
                    OnChangeSheet();
                }
            }
        }

        /// <summary>
        /// Is the actor card being dragged?
        /// </summary>
        /// <value></value>
        public bool Dragging { get; private set; }

        #region lifetime

        /// <summary>
        /// Called on initialization
        /// </summary>
        public virtual void OnInit(Card card)
        {
            if (card == null)
                throw new NullReferenceException("the card cannot be null");

            _currentSheetID = SheetsManager.CurrentSheetID;

            Card = card;
            ID = Guid.NewGuid();

            //Debug.Log("Actor initialized " + Name);

            Initialized = true;

            //check to make sure this can be run a.k.a the selected view says viewable and editable...
        }

        /// <summary>
        /// Called as many times as possible during the life of the Actor
        /// </summary>
        public virtual void Tick()
        {
            if (!Initialized)
                throw new ArgumentException("The Actor has not been initialized");

            //Debug.Log("Actor ticking " + Name);

            CurrentSheetID = SheetsManager.CurrentSheetID;

            if (SheetsManager.CurrentSheetID == Card.SheetID)
            {
                TickOnSheet();
            }

        }

        /// <summary>
        /// Called when the sheet that the card is on is the current sheet
        /// </summary>
        public virtual void TickOnSheet()
        {

            if (!Initialized)
                throw new ArgumentException("The Actor has not been initialized");

            //Debug.Log("ticking on current sheet");
        }

        public virtual void LateTick()
        {

            if (!Initialized)
                throw new ArgumentException("The Actor has not been initialized");

            //Debug.Log("Actor late ticking " + Name);
        }

        /// <summary>
        /// Called when the actor needs to be removed from the sheet (but not deleted)
        /// </summary>
        public void OnRemoveFromSheet()
        {
            OnRemove(false);
        }

        /// <summary>
        /// Delete the actor (and its data)
        /// </summary>
        public void OnDestroy()
        {
            OnRemove(true);
        }

        /// <summary>
        /// Remove the card
        /// </summary>
        /// <param name="deleteAllData">should all the data be deleted?</param>
        protected virtual void OnRemove(bool deleteAllData)
        {
            //Debug.Log("Removed actor " + Name + " " + ID.ToString());
            Card = null;
            Initialized = false;

            if (deleteAllData)
                RemoveEntry();
        }

        /// <summary>
        /// Remove the entry from the list of cards
        /// </summary>
        public abstract void RemoveEntry();

        /// <summary>
        /// Called when the user updates the input information
        /// </summary>
        /// <param name="str">the string array of information</param>
        public virtual void OnUpdateInput(List<ValueTypePair> values)
        {

            //Debug.Log("Updating");

            if (Card.CardLocked || Card.CardHidden)
            {
                NotificationHandler.Print("Card locked or hidden, cannot change data.");
                return;
            }

            foreach (var val in values)
            {
                //Debug.Log(Card.Entry.ValueCount + " " + val.Value + " " + (val.ColumLocation - 1));

                Card.Entry.UpdateValueAt(val, val.ColumLocation);
            }

            SaveInfo(Card.DataSet, Card.Entry);
        }

        #endregion

        #region visibility

        /// <summary>
        /// Called when the actor is visible
        /// </summary>
        public virtual void OnAwake()
        {
            Sleeping = false;

            //Debug.Log("Actor awake " + Name);
        }

        /// <summary>
        /// Called when the actor has been visible
        /// </summary>
        public virtual void Act()
        {

            if (!Initialized)
                throw new ArgumentException("The Actor has not been initialized");

            if (Sleeping)
            {
                OnAwake();
            }

            //Debug.Log("Actor working " + Name);
        }

        /// <summary>
        /// Called when the actor is no longer visible
        /// </summary>
        public virtual void OnSleep()
        {
            if (Sleeping)
                return;

            Sleeping = true;

            //Debug.Log("Actor sleeping " + Name);
        }

        #endregion

        #region events

        /// <summary>
        /// Called when the sheet is changed (after initialization)
        /// </summary>
        public virtual void OnChangeSheet()
        {
            //Debug.Log("Sheet changed");
        }

        /// <summary>
        /// called when the card is just starting to being dragged
        /// </summary>
        public virtual void OnStartDrag()
        {
            //Debug.Log("start drag " + Name);
            Dragging = true;
        }

        /// <summary>
        /// The card is being dragged - called like tick()
        /// </summary>
        public virtual void Drag()
        {
            //Debug.Log("Dragging " + Name);
        }

        /// <summary>
        /// called when the card is dropped
        /// </summary>
        public virtual void OnDrop()
        {
            //Debug.Log("dropped " + Name);
            Dragging = false;
        }

        /// <summary>
        /// called when the mouse is hovering over the card
        /// </summary>
        public virtual void OnHover()
        {

        }

        /// <summary>
        /// Called when a drag card state has changed
        /// </summary>
        /// <param name="dragCard">is the card being dragged?</param>
        /// <param name="startCard">the card the dragging is being acted upon</param>
        /// <param name="sender">the sender object</param>
        public virtual void OtherCardDragStateChanged(bool drag, Card card, object sender)
        {
            //Debug.Log("Other card being dragged " + card.ID + " " + drag);
        }

        /// <summary>
        /// Called when other cards are being dragged (general method called during a tick() - not very reliable)
        /// </summary>
        /// <param name="dragging">are the cards dragging?</param>
        public virtual void OtherCardsDragStateChanged(bool dragging)
        {
            //Debug.Log("other cards being dragged " + dragging);
        }

        /// <summary>
        /// Update the UI
        /// </summary>
        protected void UpdateUI()
        {
            Card.MakeUp.UpdateUI();
        }

        /// <summary>
        /// Update the UI and save the data after updating a selected value
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="ColumnLocation">the column location to update</param>
        protected void UpdateData(ValueTypePair pair, int ColumnLocation)
        {
            Card.Entry.UpdateValueAt(pair, ColumnLocation);
            UpdateUI();
            SaveInfo(Card.DataSet, Card.Entry);
        }

        /// <summary>
        /// Update the ui and save data after updating a list of values
        /// </summary>
        /// <param name="data">the data to update</param>
        protected void UpdateData(List<(ValueTypePair pair, int columnLocation)> data)
        {

            Debug.Log("updating the data");

            List<int> cols = new List<int>();

            foreach (var d in data)
            {
                Card.Entry.UpdateValueAt(d.pair, d.columnLocation);
                cols.Add(d.columnLocation);
            }

            OnUpdateData(cols);

        }

        /// <summary>
        /// Called when the data is updated via reference window
        /// </summary>
        /// <param name="updatedColumns">the list of updated columns</param>
        protected virtual void OnUpdateData(List<int> updatedColumns)
        {

            SaveInfo(Card.DataSet, Card.Entry);
            UpdateUI();
        }

        /// <summary>
        /// Get an element with a specified ID
        /// </summary>
        /// <param name="colId">the column id</param>
        /// <typeparam name="T">the element desired</typeparam>
        /// <returns>returns the element</returns>
        public T GetElementWidthId<T>(int colId) where T : CardElementViewData
        {
            var elements = Card.MakeUp.CardElementArray;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is T element)
                {
                    if (element.GetFirstColumn() == colId)
                    {
                        return element;
                    }
                }
            }

            throw new CardElementNotFoundException("Cannot find the card element with the selected id " + colId);
        }

        #endregion



        /// <summary>
        /// Get the data entry instance of this actor card
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract DataEntry GetDataEntry(string rowID, string args);

        /// <summary>
        /// Get the default data values
        /// </summary>
        /// <returns>returns the list of default data values</returns>
        protected abstract List<DataValue> GetDefaultDataValues(string rowID);

        /// <summary>
        /// Get the card element view data
        /// </summary>
        /// <returns></returns>
        protected abstract List<CardElementViewData> GetElementViewData();

        /// <summary>
        /// Create a new data entry
        /// </summary>
        /// <param name="rowID">the row id</param>
        /// <param name="dataValues">the data values</param>
        /// <param name="set">the data set</param>
        /// <param name="actor">the actor class</param>
        protected DataEntry CreateNewDataEntry<T>(string rowID, DataSet set, T actor) where T : Actor
        {
            var entry = new DataEntry(rowID, GetDefaultDataValues(rowID).ToArray(), set)
            { Actor = actor };

            return entry;
        }

        /// <summary>
        /// Get the full data set id of the action card
        /// </summary>
        /// <returns></returns>
        protected string GetDataSetId()
        {
            return "sys" + DataSetID + ":";
        }

        /// <summary>
        /// System data set
        /// </summary>
        /// <param name="id">the id of the system data set</param>
        /// <param name="elementData">the list of element data</param>
        /// <returns>returns the data set</returns>
        protected DataSet SystemDataSet(string id, List<CardElementViewData> elementData)
        {
            return GetDataSet(id, elementData, new DataFileInstance(), new List<string>());
        }

        /// <summary>
        /// Get the dataset for the actor
        /// </summary>
        /// <param name="id">the id of the actor</param>
        /// <param name="elementData">the element data</param>
        /// <param name="dataFilePath">the file location of the data file instance</param>
        /// <param name="allowedTags">the tags allowed to be viewed/edited</param>
        /// <returns>returns the data set</returns>
        protected DataSet GetDataSet(string id, List<CardElementViewData> elementData, string dataFilePath, List<string> allowedTags, string[] allTags)
        {
            var dataFile = CSVSerialization.OpenOrCreateDataFileInstance(dataFilePath, allTags);
            return GetDataSet(id, elementData, dataFile, allowedTags);
        }

        /// <summary>
        /// Get the csv data file path <general settings server action data path>\<actor name>.csv
        /// </summary>
        /// <returns>returns the string of the data file path</returns>
        protected string GetDataFilePath()
        {
            return GeneralSettings.Save_Location_Server_ActionData + @"\" + Name + ".csv";
        }

        /// <summary>
        /// Get the dataset for the actor
        /// </summary>
        /// <param name="dataSetId">the id of the actor</param>
        /// <param name="elementData">the element data</param>
        /// <param name="data">the data file instance</param>
        /// <param name="allowedTags">the tags allowed to be viewed/edited</param>
        /// <returns>returns the data set</returns>
        protected DataSet GetDataSet(string dataSetId, List<CardElementViewData> elementData, DataFileInstance data, List<string> allowedTags)
        {

            List<ColumnInfo> infos = new List<ColumnInfo>();
            var values = GetDefaultDataValues(Guid.NewGuid().ToString());

            for (int i = 0; i < values.Count; i++)
            {
                ColumnInfo info = values[i].GetColumnInfo();
                infos.Add(info);
            }

            var set = new DataSet("System", "War Manager Assistant", allowedTags, string.Empty,
                new List<string>(), true, string.Empty, "#D90000", 1.0, 2.0, dataSetId, new List<string>() { "Default" }, string.Empty,
                new List<DataSetView>() { new DataSetView("System", "System View", "This view is for the system cards", Guid.NewGuid().ToString(),
                true, true, elementData, new string[1] {"Default"},
                true, 1)}, data, infos);

            return set;
        }

        /// <summary>
        /// Get actor file instance
        /// </summary>
        /// <param name="header">the header tags</param>
        /// <returns>returns the data file instance</returns>
        protected DataFileInstance GetActorFileInstance(string[] header)
        {
            string filePath = GeneralSettings.Save_Location_Server_ActionData + @"\" + DataSetID + ".csv";
            DataFileInstance instance = new DataFileInstance(filePath, header, new List<string[]>(), false);
            return instance;
        }

        /// <summary>
        /// Save the data in the actor class
        /// </summary>
        /// <param name="set">the data set</param>
        /// <param name="entry">the data entry</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        protected bool SaveInfo(DataSet set, DataEntry entry)
        {

            if (set.EntryExists(entry.RowID))
                return set.ReplaceEntry(entry);
            else
                return set.AppendEntry(entry);
        }

        public int CompareTo(Actor other)
        {
            if (other == null)
                return 1;

            if (other.ID == null && ID == null)
                return 1;
            else if (ID != null && ID != null)
            {
                return ID.CompareTo(other.ID);
            }
            else
            {
                return 1;
            }
        }

        public bool Equals(Actor other)
        {
            if (other == null)
                return false;

            if (other.ID == null && ID == null)
                return false;
            else if (ID != null && ID != null)
            {
                return ID == other.ID;
            }
            else
            {
                return false;
            }
        }
    }

}
