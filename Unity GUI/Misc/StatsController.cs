
/* StatsController.cs
 * Author: Taylor Howell
 */


using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

namespace WarManager
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class StatsController : MonoBehaviour
    {
        private TMPro.TMP_Text text;

        public WarManagerCameraController cameraController;

        private string sheetId = null;

        private float zoomCheck = .25f;

        public void Start() => StartCoroutine(RoutineCheck());

        public void OnChangeCurrentSheet(string id)
        {
            if (text == null)
                text = GetComponent<TMPro.TMP_Text>();

            if (id != null)
            {
                text.text = "Loading sheet " + sheetId;

                sheetId = id;
            }
            else
            {
                text.text = "Loading sheet";
            }
        }

        IEnumerator RoutineCheck()
        {
            while (true)
            {
                Check();
                yield return new WaitForSeconds(1f);
            }
        }

        private void Check()
        {
            if (text == null)
                text = GetComponent<TMPro.TMP_Text>();

            if (sheetId != null && sheetId != string.Empty)
            {
                Sheet<Card> sheet = SheetsManager.GetActiveSheet(sheetId);

                var dataSets = WarSystem.DataSetManager.GetDataSetsFromSheet(SheetsManager.CurrentSheetID);


                if (dataSets.Count > 0)
                {
                    List<string> names = new List<string>();

                    foreach (var set in dataSets)
                    {

                        names.Add("\'" + set.DatasetName + "\'");
                    }

                    string final = String.Join<string>(", ", names);

                    string cardsSelectedString = "";

                    if (SheetsCardSelectionManager.Main.CardTotal > 0)
                    {
                        cardsSelectedString = " Selected: " + SheetsCardSelectionManager.Main.CardTotal.ToString() + " |";
                    }

                    text.text = ConvertZoomToString() + cardsSelectedString + " Sheet: " + sheet.Name + " - " + dataSets.Count + " Dataset(s) " + final;
                }
                else
                {
                    if (sheet != null)
                        text.text = sheet.Name + " - No Datasets!";
                    else
                        text.text = "No Sheet!";
                }

                return;


            }

            if (WarSystem.IsConnectedToServer)
            {
                text.text = ConvertZoomToString() + " " + WarSystem.ServerVersion + " - Connected to " + WarSystem.ConnectedServerName;
            }
            else
            {
                text.text = ConvertZoomToString() + " " + WarSystem.ServerVersion + " - Not connected to server";
            }
        }

        private string ConvertZoomToString()
        {
            if (SheetsManager.SheetCount > 0)
            {
                float x = (zoomCheck * 100);
                return "Zoom: " + Mathf.Round(x) + "% |";
            }
            else
            {
                return "";
            }
        }

        public void GetZoomChange()
        {
            zoomCheck = cameraController.ZoomAmtNormalized;
            text.text = ConvertZoomToString();
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnChangeCurrentSheet;
            WarManagerCameraController.OnCameraChange += GetZoomChange;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnChangeCurrentSheet;
            WarManagerCameraController.OnCameraChange -= GetZoomChange;
        }
    }
}
