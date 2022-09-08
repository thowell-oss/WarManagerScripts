using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;
using WarManager.Unity3D.Windows;
using WarManager.Cards;

namespace WarManager.Unity3D
{
    public class SheetPropertiesPanel : MonoBehaviour
    {
        /// <summary>
        /// Show the current sheet properties
        /// </summary>
        /// <param name="sheetID">sheet id</param>
        public void ShowCurrentSheetProperties(string sheetID)
        {

            if (SheetsManager.TryGetActiveSheet(sheetID, out var sheet))
            {
                List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

                content.Add(new SlideWindow_Element_ContentInfo(sheet.Name + " properties", 20));
                content.Add(new SlideWindow_Element_ContentInfo(20));

                foreach (var x in sheet.Partitions)
                {
                    content.Add(new SlideWindow_Element_ContentInfo(x.Key.Name, 20));
                    content.Add(new SlideWindow_Element_ContentInfo(20));

                    var gridPartition = x.Value;

                    List<Card> cards = new List<Card>();

                    for (int i = 0; i < gridPartition.Length; i++)
                    {
                        for (int k = 0; k < gridPartition[i].Length; k++)
                        {
                            cards.AddRange(gridPartition[i][k].GetAllObjects());
                        }
                    }

                    foreach (var card in cards)
                    {
                        content.Add(new SlideWindow_Element_ContentInfo(card.Entry, (x) => { }));
                    }

                    content.Add(new SlideWindow_Element_ContentInfo(20));
                }
                SlideWindowsManager.main.AddPropertiesContent(content, true);
            }
        }
    }
}
