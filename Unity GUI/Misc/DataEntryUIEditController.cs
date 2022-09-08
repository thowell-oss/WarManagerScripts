
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Unity3D.Windows;


namespace WarManager.Unity3D
{
    /// <summary>
    /// The link between the front end UI and backend processing when editing data
    /// </summary>
    [Notes.Author(1.2, "The link between the front end UI and backend processing when editing data")]
    public class DataEntryUIEditController
    {
        /// <summary>
        /// The Entry being edited
        /// </summary>
        /// <value></value>
        public DataEntry EntryToEdit { get; private set; }

        /// <summary>
        /// The value type pair being edited to (then copied to the data entry upon save)
        /// </summary>
        /// <typeparam name="ValueTypePair"></typeparam>
        /// <returns></returns>
        private List<ValueTypePair> _currentEditedValues { get; set; } = new List<ValueTypePair>();

        /// <summary>
        /// The data values from the entry, used for reference
        /// </summary>
        /// <typeparam name="DataValue"></typeparam>
        /// <returns></returns>
        private List<DataValue> _currentDataValues { get; set; } = new List<DataValue>();

        /// <summary>
        /// cancel action
        /// </summary>
        private Action<int> _cancel;

        /// <summary>
        /// save action
        /// </summary>
        private Action<int> _save;

        /// <summary>
        /// the type of way to save the data
        /// </summary>
        private DataAction _saveAction;

        private Sprite _backIcon;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entryToEdit">the data entry that is being edited</param>
        /// <param name="updateChanges">should the changes be updated in the UI (action)?</param>
        /// <param name="saveAction">how should the changed be saved?</param>
        public DataEntryUIEditController(DataEntry entryToEdit, Action<bool> updateChanges, DataAction saveAction = DataAction.Replace)
        {
            if (entryToEdit == null)
                throw new System.NullReferenceException("The data entry cannot be null");

            if (updateChanges == null)
            {
                throw new NullReferenceException("The showChanges() action cannot be null");
            }

            // WarSystem.WriteToLog("Editing Data Set " + entryToEdit.DataSet.ID + " entry " + entryToEdit.Row + " : " + EntryToEdit.GetRawData());

            EntryToEdit = entryToEdit;
            _currentEditedValues.AddRange(EntryToEdit.GetAllowedValueTypePairs());
            _currentDataValues.AddRange(EntryToEdit.GetAllowedDataValues());

            _save = (x) =>
            {
                // WarSystem.WriteToLog("Attempting to save changes " + entryToEdit.DataSet.ID + " " + entryToEdit.Row + " : " + EntryToEdit.GetRawData());
                SaveChanges();
                EditValues();

                WarSystem.WriteToLog("Data changes - Data Set ID: " + entryToEdit.DataSet.ID + " Row: " + entryToEdit.RowID + " New Data: " + EntryToEdit.GetRawData(), Logging.MessageType.logEvent);
            };

            _cancel = (x) =>
            {
                updateChanges(false);
                //WarSystem.WriteToLog("Data Changes Canceled " + entryToEdit.DataSet.ID + " " + entryToEdit.Row + " : " + EntryToEdit.GetRawData());
            };

            _saveAction = saveAction;
        }

        /// <summary>
        /// Edit the values of the existing data entry
        /// </summary>
        public void EditValues()
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();
            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Back", -1, _cancel, null));

            if (_currentEditedValues == null || _currentEditedValues.Count == 0 || _currentDataValues == null || _currentDataValues.Count == 0)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Nothing to edit", null));
                UpdateProperties(content);
                return;
            }

            WarSystem.WriteToLog("Attempting to edit " + EntryToEdit.DataSet.ID + " entry " + EntryToEdit.RowID + " : " + EntryToEdit.GetRawData(), Logging.MessageType.logEvent);

            string str = "";

            for (int i = 0; i < _currentEditedValues.Count; i++)
            {

                str += _currentDataValues[i].ParseToParagraph() + ", ";

                string val = "";

                int someVal = i;

                if (_currentEditedValues[someVal].Value != null)
                {
                    val = _currentEditedValues[someVal].Value.ToString();
                }


                var valueEditor = new SlideWindow_Element_ContentInfo(_currentDataValues[someVal].HeaderName, val, (x) =>
                    {
                        //values[iterator].ReplaceData(new ValueTypePair(x, values[iterator].ValueType));
                        _currentEditedValues[someVal].Value = x;

                        //Debug.Log(x);


                    }, _currentDataValues[i].GetColumnInfo().KeywordValues);

                valueEditor.DescriptionHeader = _currentDataValues[i].GetColumnInfo().HeaderName;
                valueEditor.DescriptionInfo = _currentDataValues[i].GetColumnInfo().Description;
                valueEditor.ContentType = _currentDataValues[i].ValueType;

                if (valueEditor.ContentType == ColumnInfo.GetValueTypeOfCustom)
                    valueEditor.Regex = _currentDataValues[i].GetColumnInfo().CustomRegex;

                if (valueEditor.ContentType == ColumnInfo.GetValueTypeOfPhone)
                    if (UnitedStatesPhoneNumber.TryParse(valueEditor.Content, out var phone))
                    {
                        valueEditor.PhoneNumber = phone;
                        valueEditor.Content = phone.FullNumberUS;
                        _currentEditedValues[someVal].Value = phone.FullNumberUS;
                    }

                content.Add(valueEditor);
            }

            // Debug.Log(str);

            if (EntryToEdit.DataSet.ID.StartsWith("sys") && _saveAction == DataAction.Replace)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Save", -1, (x) =>
                {
                    var actor = EntryToEdit.Actor;
                    if (actor != null)
                    {
                        actor.OnUpdateInput(_currentEditedValues);
                    }
                }, null));
            }
            else if (!EntryToEdit.DataSet.ID.StartsWith("sys"))
            {
                content.Add(new SlideWindow_Element_ContentInfo("Save", -1, _save, null));


                if (_saveAction == DataAction.Replace && !EntryToEdit.DataSet.ID.StartsWith("sys")) //leave alone what the action is...
                {

                    content.Add(new SlideWindow_Element_ContentInfo(30));

                    content.Add(new SlideWindow_Element_ContentInfo("Delete Forever", -1, (x) =>
                   {
                       MessageBoxHandler.Print_Immediate("Are you sure you want to delete this forever?", "Question", (x) =>
                       {
                           if (x)
                           {
                               DeleteEntry();
                               _cancel(-1);
                           }
                       });

                   }, ActiveSheetsDisplayer.main.DeleteForeverSprite));
                }
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));

            UpdateProperties(content);
        }


        /// <summary>
        /// Update the properties window with new changes
        /// </summary>
        /// <param name="contentInfos"></param>
        private void UpdateProperties(List<SlideWindow_Element_ContentInfo> contentInfos)
        {
            SlideWindowsManager.main.AddPropertiesContent(contentInfos, true, "");
        }


        /// <summary>
        /// Save the changes
        /// </summary>
        private void SaveChanges()
        {
            switch (_saveAction)
            {
                case DataAction.Replace:
                    ReplaceEntry();
                    break;

                case DataAction.Delete: //not used...
                    DeleteEntry();
                    break;

                default:
                    AppendEntry();
                    break;
            }
        }

        /// <summary>
        /// Replace an old entry with a new one
        /// </summary>
        private void ReplaceEntry()
        {
            // Debug.Log("replacing entry");

            foreach (var val in _currentEditedValues)
            {
                EntryToEdit.UpdateValueAt(val, val.ColumLocation);
            }

            EntryToEdit.DataSet.ReplaceEntry(EntryToEdit);

            WarSystem.WriteToLog($"Entry replaced to: {EntryToEdit.GetRawData()} ", Logging.MessageType.logEvent);
        }


        /// <summary>
        /// Append an entry
        /// </summary>
        private void AppendEntry()
        {
            string row = Guid.NewGuid().ToString();

            _saveAction = DataAction.Replace;

            // var newEntry = EntryToEdit.Copy(row);
            // EntryToEdit = newEntry;

            foreach (var item in _currentEditedValues)
            {
                EntryToEdit.UpdateValueAt(item, item.ColumLocation);
            }

            EntryToEdit.UpdateRowID(row);
            EntryToEdit.DataSet.AppendEntry(EntryToEdit);
            //  EntryToEdit.UpdateRowID(EntryToEdit.DataSet.GetEntry(EntryToEdit.DataSet.DataCount - 1).Row + 1);

            //Debug.Log("appending entry");
            WarSystem.WriteToLog($"Entry appended: {EntryToEdit.GetRawData()} ", Logging.MessageType.logEvent);

        }

        /// <summary>
        /// Remove an entry from the list of entries
        /// </summary>
        private void DeleteEntry()
        {

            SheetsManager.RemoveEntryFromAllOpenSheets(EntryToEdit);

            var set = EntryToEdit.DataSet;
            set.RemoveEntry(EntryToEdit);

            LeanTween.delayedCall(1, () =>
            {
                MessageBoxHandler.Print_Immediate("Entry Deleted", "Note");
                DataSetViewer.main.ShowDataSet(set.ID, ActiveSheetsDisplayer.main.ViewReferences);
            });

            WarSystem.WriteToLog($"Entry deleted: {EntryToEdit.GetRawData()} ", Logging.MessageType.logEvent);
        }
    }

    /// <summary>
    /// Eum handles the different types of ways to handle the data
    /// </summary>
    public enum DataAction
    {
        Replace,
        Append,
        Delete,
    }
}
