
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles a list of tags and puts it into a drop down")]
    public class UnityListTagSelectDropdown : MonoBehaviour
    {
        private bool _open;

        private bool _update = false;
        public bool Open
        {
            get
            {
                return _open;
            }

            set
            {
                _open = value;

                if (_open)
                {
                    OpenTagSelector();
                }
                else
                {
                    CloseTagSelector();
                }

                _update = true;
            }

        }

        //public TMPro.TMP_InputField SelectedText;

        public Transform ContentViewer;
        public GameObject DropDown;
        public SelectTagButton ButtonPrefab;

        public Vector2 OffLocation;
        public Vector2 OnLocation;

        List<TagValuePair<int, string>> tags = new List<TagValuePair<int, string>>();
        List<SelectTagButton> Buttons = new List<SelectTagButton>();

        /// <summary>
        /// Get or set the tag/value that has been selected
        /// </summary>
        /// <value></value>
        public (string tag, string value) SelectedTagAndValue { get; set; } = ("Not Selected", "");

        void Awake()
        {
            //SelectedText.text = SelectedTagAndValue.tag;
            Open = false;
        }


        // Update is called once per frame
        void Update()
        {
            if (_update)
            {
                if (Open)
                {
                    LeanTween.cancel(DropDown);
                    LeanTween.moveLocal(DropDown, OnLocation, .25f).setEaseInOutExpo();
                }
                else
                {
                    LeanTween.cancel(DropDown);
                    LeanTween.moveLocal(DropDown, OffLocation, .25f).setEaseInOutExpo();
                }

                _update = false;
            }
        }

        [Obsolete("do not use")]
        private void OpenTagSelector()
        {
            Debug.Log("opening");

            try
            {
                tags = GetTags();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }


            for (int i = 0; i < tags.Count; i++)
            {
                if (i >= Buttons.Count)
                {
                    GameObject b = Instantiate(ButtonPrefab.gameObject, ContentViewer.transform) as GameObject;
                    SelectTagButton button = b.GetComponent<SelectTagButton>();
                    Buttons.Add(button);
                }

                Buttons[i].gameObject.SetActive(true);
                Buttons[i].SetTagButton(tags[i].Tag, this);
            }

            DropDown.SetActive(true);
        }
        [Obsolete("do not use")]
        private void CloseTagSelector()
        {
            StartCoroutine(Close());
        }

        public void Toggle()
        {
            Open = !Open;
        }
        [Obsolete("do not use")]
        public void SelectTag(string tag)
        {
            var foundTag = tags.Find((x) => x.Tag == tag);

            if (foundTag != null)
            {
                Open = false;
                SelectedTagAndValue = (foundTag.Tag, foundTag.Value);

                //SelectedText.text = SelectedTagAndValue.tag;
            }
        }

        public void Cancel()
        {
            Open = false;
        }

        public string GetValue()
        {
            return SelectedTagAndValue.value;
        }

        IEnumerator Close()
        {
            yield return new WaitForSeconds(.2f);
            DropDown.SetActive(false);
            foreach (var b in Buttons)
                b.gameObject.SetActive(false);
            StopCoroutine(Close());
        }

        [Obsolete("do not use")]
        private List<TagValuePair<int, string>> GetTags()
        {
            // var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();

            // List<TagValuePair<int, string>> tags = new List<TagValuePair<int, string>>();

            // foreach (var card in cards)
            // {
            //     var row = card.DataRepID;

            //     var dataset = card.DataSet;
            //     DataPiece p = dataset.GetData(row);

            //     foreach (var t in p.GetSets())
            //     {
            //         if (tags.Find((x) => x == t) == null)
            //         {
            //             tags.Add(t);
            //         }
            //     }

            //     //Debug.Log("tags " + tags.Count);
            // }

            // return tags;

            return null;
        }
    }
}
