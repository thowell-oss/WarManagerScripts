
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace WarManager.Unity3D.Windows
{
    [Notes.Author("The button to search with")]
    public class SearchItemButton : PoolableObject
    {
        public Button Button;
        public TMPro.TMP_Text Title;
        public TMPro.TextMeshProUGUI TitleGui;

        private ShowDataSetEntries _entrySearch;
        private int _selection = 0;

        public RectTransform rectTransform;

        public void Init(int selection, string title, ShowDataSetEntries entrySearch, Vector2 location)
        {
            if (title == null || title.Trim() == string.Empty)
                title = "<empty>";

            _selection = selection;
            Title.text = title;
            _entrySearch = entrySearch;
            rectTransform.anchoredPosition = location;
        }

        public void HighlightLetters(string letters)
        {
            int k = 0;

            for (int i = 0; i < Title.text.Length; i++)
            {
                string x = "";
                k = 0;

                while (letters[k] == Title.text[i + k] && k < letters.Length && i + k < Title.text.Length)
                {

                    x += letters[k];
                    if (k == letters.Length - 1)
                    {
                        HighlightCharacters(k, i);
                    }

                    k++;
                }

            }
        }

        private void HighlightCharacters(int start, int count)
        {

        }

        public void OnSelect()
        {
            _entrySearch.SelectItem(_selection);
        }
    }
}
