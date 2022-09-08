using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    public class ServerManifestViewer : MonoBehaviour
    {
        public List<string> AvailableSheets = new List<string>();
        public int Count_AvailableSheets;

        public List<string> sheetsOnServer = new List<string>();

        public List<string> SheetsInManager = new List<string>();

        public bool updateList = false;

        void Update()
        {
            if (updateList)
            {
                var sheetManifest = WarSystem.CurrentSheetsManifest;

                var sheets = sheetManifest.Sheets;

                AvailableSheets.Clear();
                foreach (var sheet in sheets)
                {
                    AvailableSheets.Add(sheet.Data.SheetName);
                }

                Count_AvailableSheets = AvailableSheets.Count;

                var serverSheets = SheetsServerManifest.GetServerSheetMetaData();


                sheetsOnServer.Clear();
                foreach (var fileControl in serverSheets)
                {
                    sheetsOnServer.Add(fileControl.Data.SheetName);
                }

                var sheetsFromManager = SheetsManager.GetActiveCardSheets();

                SheetsInManager.Clear();

                foreach (var sheet in sheetsFromManager)
                {
                    SheetsInManager.Add(sheet.Name + " " + sheet.ID);
                }
            }
        }
    }
}
