
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Unity3D.Windows;
using WarManager;
using WarManager.Backend;

namespace WarManager.Unity3D
{

    public class KeywordSelectionUIHandler : MonoBehaviour
    {

        private List<KeyValuePair<string, DataSet>> _allKeys = new List<KeyValuePair<string, DataSet>>();

        private List<KeyValuePair<string, DataSet>> _lastSelectedKeys = new List<KeyValuePair<string, DataSet>>();

        private List<KeyValuePair<string, DataSet>> _selectedKeys = new List<KeyValuePair<string, DataSet>>();
        private Action<List<KeyValuePair<string, DataSet>>> _callBack;

        private string _title;

        /// <summary>
        /// Select the keys get back keys from call back
        /// </summary>
        /// <param name="title">the name of the title</param>
        /// <param name="allKeys">all keys options</param>
        /// <param name="currentSelectedKeys">the current selected keys</param>
        /// <param name="callback">the call back</param>
        public void SelectKeys(string title, List<KeyValuePair<string, DataSet>> allKeys, List<KeyValuePair<string, DataSet>> currentSelectedKeys, Action<List<KeyValuePair<string, DataSet>>> callback)
        {


            if (allKeys == null)
                throw new NullReferenceException();

            if (allKeys.Count < 1)
                throw new ArgumentException("no keys to select in the list");

            if (callback == null)
                throw new NullReferenceException("the call back is null");

            if (currentSelectedKeys != null)
            {
                _selectedKeys = currentSelectedKeys;
                _lastSelectedKeys = currentSelectedKeys;
            }

            if (title == null)
                throw new NullReferenceException("the title cannot be null");

            _title = title;

            _callBack = callback;

            _allKeys = allKeys;

            SelectKeysUI();

        }

        /// <summary>
        /// UI for selecting keys
        /// </summary>
        private void SelectKeysUI()
        {
            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(_title, 20));
            content.Add(new SlideWindow_Element_ContentInfo("Cancel", 0, (x) =>
            {
                _callBack(_lastSelectedKeys);
            }, ActiveSheetsDisplayer.main.BackSprite));

            content.Add(new SlideWindow_Element_ContentInfo(20));

            for (int i = 0; i < _selectedKeys.Count; i++)
            {

                int iterator = i;

                content.Add(new SlideWindow_Element_ContentInfo(iterator.ToString() + ") " + _selectedKeys[iterator].Key + $"({_selectedKeys[iterator].Value.DatasetName})", () =>
                {
                    _selectedKeys.Remove(_selectedKeys[iterator]);
                    SelectKeysUI();
                }, ActiveSheetsDisplayer.main.DeleteSprite));
            }

            List<KeyValuePair<string, DataSet>> notSelectedData = GetNotSelectedKeys();

            List<string> data = new List<string>();

            for (int i = 0; i < notSelectedData.Count; i++)
            {
                data.Add(i + ") " + notSelectedData[i].Key + $" ({notSelectedData[i].Value.DatasetName})");
            }

            if (notSelectedData.Count > 0)
            {

                SlideWindow_Element_ContentInfo dropDownContentInfo = new SlideWindow_Element_ContentInfo("Unselected Tags", "", (x) =>
                {
                    string[] info = x.Split(')');

                    if (Int32.TryParse(info[0], out var result))
                    {
                        _selectedKeys.Add(notSelectedData[result]);
                        SelectKeysUI();
                    }

                }, data.ToArray());

                dropDownContentInfo.ContentType = ColumnInfo.GetValueTypeOfKeyword;

                content.Add(dropDownContentInfo);
            }

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Done", () =>
            {
                _callBack(_selectedKeys);
            }));

            SlideWindowsManager.main.AddReferenceContent(content, true);
        }

        private List<KeyValuePair<string, DataSet>> GetNotSelectedKeys()
        {

            List<KeyValuePair<string, DataSet>> result = new List<KeyValuePair<string, DataSet>>();

            for (int i = 0; i < _allKeys.Count; i++)
            {
                if (!_selectedKeys.Contains(_allKeys[i]))
                {
                    result.Add(_allKeys[i]);
                }
            }

            return result;
        }
    }
}
