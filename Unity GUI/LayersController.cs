/* LayersController.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Backend;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Controls the layers and the interaction at the slide window
    /// </summary>
    [Notes.Author("Controls the layers and the interaction at the slide window")]
    public class LayersController : MonoBehaviour
    {
        List<string> layerIds = new List<string>();
        public Sprite LayerSprite;
        public Sprite AddLayerSprite;

        public void Start()
        {
            // List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();
            // contentList.Add(new SlideWindow_Element_ContentInfo("", "No sheets"));
            // SlideWindowsManager.main.AddLayersContent(contentList);
        }

        public void OnChangeCurrentSheet(string sheetId)
        {
            var sheet = SheetsManager.GetActiveSheet(sheetId);
            List<SlideWindow_Element_ContentInfo> contentList = new List<SlideWindow_Element_ContentInfo>();
            if (sheet != null)
            {
                var layers = sheet.GetAllLayers();


                contentList.Add(new SlideWindow_Element_ContentInfo($"Layers for {sheet.Name}", null));

                for (int i = 0; i < layers.Count; i++)
                {
                    var button = new SlideWindow_Element_ContentInfo(layers[i].Name, i, (x) => { ChangeLayer(x); }, LayerSprite);
                    layerIds.Add(layers[i].ID);
                    contentList.Add(button);
                }
                contentList.Add(new SlideWindow_Element_ContentInfo("Add New Layer", 0, (x) => { AddNewLayer(x); }, AddLayerSprite));

            }
            else
            {
                contentList.Add(new SlideWindow_Element_ContentInfo("Error", "Sheet not availble"));
            }

            SlideWindowsManager.main.AddLayersContent(contentList);
        }

        private void ChangeLayer(int x)
        {
            var sheet = SheetsManager.GetActiveSheet(SheetsManager.CurrentSheetID);
            sheet.SetCurrentLayer(layerIds[x]);
        }

        private void AddNewLayer(int x)
        {
            var sheet = SheetsManager.GetActiveSheet(SheetsManager.CurrentSheetID);
            sheet.AddNewLayer(new Layer("Layer", 1, "#FFAAFF"));
            SheetsManager.SetSheetCurrent(sheet.ID);
        }

        void OnEnable()
        {
            //SheetsManager.OnSetSheetCurrent += OnChangeCurrentSheet;
        }

        void OnDisable()
        {
           // SheetsManager.OnSetSheetCurrent -= OnChangeCurrentSheet;
        }
    }
}
