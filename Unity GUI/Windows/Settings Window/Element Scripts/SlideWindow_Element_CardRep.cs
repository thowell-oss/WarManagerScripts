
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using WarManager;
using WarManager.Cards;
using WarManager.Backend.CardsElementData;
using WarManager.Backend;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles the card displaying and other UI interaction with the entry data in the slide window
    /// </summary>
    [Notes.Author("Handles the card displaying and other UI interaction with the entry data in the slide window")]
    public class SlideWindow_Element_CardRep : MonoBehaviour, ISlideWindow_Element
    {
        public TMPro.TMP_Text Data;
        public TMPro.TMP_Text RepIdDisplay;

        public Image IDColorBlock;

        public SlideWindow_Element_ContentInfo info { get; set; }

        Action<int> callBack;

        public GameObject targetGameObject => this.gameObject;

        private string _searchContent = "Card";

        public TooltipTrigger Trigger;

        private DataEntry selectedEntry;

        public string SearchContent
        {
            get
            {
                return _searchContent;
            }
        }

        public void UpdateElement()
        {
            Data.text = "";

            callBack = info.Callback;

            string dataSetId = info.Label;
            selectedEntry = info.Entry;

            if (selectedEntry != null && selectedEntry.RowID != null)
                RepIdDisplay.text = selectedEntry.RowID;
            else
            {
                selectedEntry = new DataEntry();
            }

            List<string> output = new List<string>();

            _searchContent += dataSetId + " " + selectedEntry.RowID;

            DataSet set = info.Entry.DataSet;

            if (set != null && WarSystem.DataSetManager.TryGetDataset(info.Label, out set))
            {
                IDColorBlock.color = set.Color;

                RepIdDisplay.text = set.DatasetName + " - " + RepIdDisplay.text;

                var element = info.Entry;

                var selectedData = set.SelectedView.ElementDataArray;

                string elementTextInfo = "";

                for (int i = 0; i < selectedData.Length; i++)
                {
                    List<int> columns = new List<int>();

                    if (selectedData[i] is CardElementTextData text)
                    {
                        if (text.ToColumnArray.Length > 0)
                        {
                            columns.AddRange(text.ToColumnArray);
                            if (element.TryGetValueAt(columns[0], out var value))
                            {
                                elementTextInfo = value.ParseToParagraph();
                            }
                            else
                            {
                                elementTextInfo = "<empty>";
                            }
                        }
                    }

                    if (selectedData[i] is CardButtonElementData link)
                    {
                        if (link.ToColumnArray.Length > 0)
                        {
                            columns.AddRange(link.ToColumnArray);

                            if (element.TryGetValueAt(columns[0], out var value))
                            {
                                elementTextInfo = value.ParseToParagraph();
                            }
                            else
                            {
                                elementTextInfo = "<empty>";
                            }
                        }
                    }

                    Trigger.contentText = elementTextInfo;

                    // if (selectedData[i].ElementTag == "text" || selectedData[i].ElementTag == "link")
                    // {
                    //     int col = selectedData[i].ID;

                    //     if (element.TryGetValueAt(col, out var value))
                    //     {
                    //         elementTextInfo = value.ParseToParagraph();
                    //     }

                    //     if (selectedData[i].PayloadLength >= 4)
                    //     {
                    //         string[] payload = selectedData[i].Payload.Split(',');

                    //         for (int j = 3; j < payload.Length; j++)
                    //         {
                    //             if (Int32.TryParse(payload[j], out int x))
                    //             {
                    //                 if (element.TryGetValueAt(x, out var someValue))
                    //                 {
                    //                     elementTextInfo = elementTextInfo + " " + someValue.ParseToParagraph();
                    //                 }
                    //             }
                    //         }
                    //     }

                    // }

                    var allowedValues = element.GetAllowedDataValues();

                    foreach (var x in allowedValues)
                    {
                        _searchContent = _searchContent + " " + x.Value + " " + x.HeaderName;
                    }

                    Data.text = elementTextInfo;
                }
            }

            // if (set.DataCount > _repId)
            // {
            //     var data = set.SelectedView.ElementDataArray;

            //         var piece = set.GetData(_repId);
            //         if (piece != null)
            //         {
            //             bool[] selectedCol = new bool[piece.DataCount];

            //             foreach (var element in data)
            //             {
            //                 if (element.ElementTag == "text" || element.ElementTag == "link")
            //                 {
            //                     int col = element.ID;

            //                     string str = "";

            //                     if (piece.DataCount > col)
            //                     {
            //                         str = piece.GetData(col);
            //                         selectedCol[col] = true;
            //                     }

            //                     if (element.PayloadLength >= 4)
            //                     {
            //                         string[] payload = element.Payload.Split(',');

            //                         for (int i = 3; i < payload.Length; i++)
            //                         {
            //                             if (Int32.TryParse(payload[i], out int x))
            //                             {
            //                                 if (x < piece.DataCount)
            //                                 {
            //                                     str = str + " " + piece.GetData(x);
            //                                     selectedCol[col] = true;
            //                                 }
            //                             }
            //                         }
            //                     }
            //                     output.Add(str);
            //                 }
            //             }

            //             for (int i = 0; i < piece.DataCount; i++)
            //             {
            //                 if (!selectedCol[i])
            //                 {
            //                     if (set.AllowedTags.Find((x) => x == piece.GetTag(i)) != null)
            //                     {
            //                         _searchContent += piece.GetData(i);
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }

            // if (output.Count > 0)
            // {
            //     Data.text = string.Join("\n", output);
            //     _searchContent += string.Join(" ", output);
            // }
        }

        public void FireCallback()
        {
            if (callBack != null)
            {
                callBack(0);
                Debug.Log("clicking");
            }
        }

        /// <summary>
        /// Spawn a card when the dragging starts
        /// </summary>
        /// <param name="dragging"></param>
        public void HandleDragEvent()
        {

            // Debug.Log("drag event");


            if (WarManager.Backend.SheetsManager.TryGetCurrentSheet(out var currentSheet))
            {
                if (CardUtility.TryInsertDragCard(currentSheet, currentSheet.CurrentLayer, info.Label, info.Entry.RowID, out string id))
                {
                    SlideWindowsManager.main.CloseWindows();

                    //success!!
                }
            }
        }
    }
}
