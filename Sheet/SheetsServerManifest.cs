
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using WarManager.Sharing;

namespace WarManager.Backend
{
    /// <summary>
    /// Contains information about all the sheets on the server
    /// </summary>
    public class SheetsServerManifest
    {
        /// <summary>
        /// The list of sheet meta data
        /// </summary>
        /// <typeparam name="SheetMetaData"></typeparam>
        /// <returns></returns>
        private List<FileControl<SheetMetaData>> _sheetMetaDataList = new List<FileControl<SheetMetaData>>();


        /// <summary>
        /// The amount of sheet meta data files in the server manifest (-1 means that the list of sheet meta data is null)
        /// </summary>
        /// <value></value>
        public int Count
        {
            get
            {
                if (_sheetMetaDataList != null)
                    return _sheetMetaDataList.Count;

                return -1;
            }
        }

        /// <summary>
        /// Get the array of sheets
        /// </summary>
        /// <value></value>
        public FileControl<SheetMetaData>[] Sheets
        {
            get
            {
                var final = new FileControl<SheetMetaData>[_sheetMetaDataList.Count];
                Array.Copy(_sheetMetaDataList.ToArray(), final, _sheetMetaDataList.Count);

                return final;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sheets">the sheets contained in the server</param>
        public SheetsServerManifest(List<FileControl<SheetMetaData>> sheets)
        {
            _sheetMetaDataList = sheets;
            _sheetMetaDataList.Sort();
        }

        /// <summary>
        /// Replace sheet meta data by id
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <param name="newData">the new data</param>
        /// <returns></returns>
        public bool ReplaceMetaData(FileControl<SheetMetaData> newData)
        {
            var data = _sheetMetaDataList.Find((x) => x.Data.ID == newData.Data.ID);

            if (data != null)
            {
                _sheetMetaDataList.Remove(data);
                _sheetMetaDataList.Add(newData);
                _sheetMetaDataList.Sort();
                return true;
            }

            return false;
        }

        // <summary>
        /// Replace sheet meta data by id
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <param name="newData">the new data</param>
        /// <returns></returns>
        public bool ReplaceMetaData(SheetMetaData newData)
        {
            var data = _sheetMetaDataList.Find(x => x.Data.ID == newData.ID);
            if (data != null)
            {
                if (_sheetMetaDataList.Remove(data))
                {

                    FileControl<SheetMetaData> file = new FileControl<SheetMetaData>(newData, false,
                    "1", newData.Categories, newData.Owner, data.CreationTime, newData.LastTimeOpened);

                    _sheetMetaDataList.Add(file);
                    _sheetMetaDataList.Sort();

                    SaveServerMetaData(_sheetMetaDataList);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add a sheet to the meta data list
        /// </summary>
        /// <param name="m"></param>
        public void AddSheet(FileControl<SheetMetaData> m)
        {
            if (m == null)
                return;

            _sheetMetaDataList.Add(m);
        }

        /// <summary>
        /// Remove sheet meta data from the list
        /// </summary>
        /// <param name="id">the id of the sheet meta data to find</param>
        public bool TryRemoveSheetFromList(string id, out FileControl<SheetMetaData> sheetData)
        {
            if (id == null)
                throw new NullReferenceException("The id is null");

            if (id == string.Empty)
                throw new NotSupportedException("the id cannot be empty");

            var data = _sheetMetaDataList.Find(x => x.Data.ID == id);
            if (data != null)
            {
                if (_sheetMetaDataList.Remove(data))
                {
                    sheetData = data;

                    // UnityEngine.Debug.Log("removed meta data " + sheetData.Data.SheetName);

                    return true;
                }
            }

            sheetData = null;
            return false;
        }

        /// <summary>
        /// Attempt to get the file control of the meta data sheet
        /// </summary>
        /// <param name="id">the id of the sheet</param>
        /// <param name="sheet">the resulting sheet</param>
        /// <returns>returns true if the sheet was found, false if not</returns>
        public bool TryGetFileControlSheet(string id, out FileControl<SheetMetaData> sheet)
        {
            if (_sheetMetaDataList == null || _sheetMetaDataList.Count < 1)
            {
                sheet = null;
                return false;
            }

            var someSheet = _sheetMetaDataList.Find((x) => x.Data.ID == id);

            sheet = someSheet;

            if (someSheet == null)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Attempt to get the meta data sheet 
        /// </summary>
        /// <param name="id">the id of the sheet</param>
        /// <param name="sheet">the out sheet</param>
        /// <returns>returns true if the getting the sheet was successful, false if not</returns>
        public bool TryGetSheet(string id, out SheetMetaData sheet)
        {
            if (TryGetFileControlSheet(id, out var fileControl))
            {
                if (FileControl<SheetMetaData>.TryGetServerFile(fileControl, WarSystem.ServerVersion, WarSystem.CurrentActiveAccount, out var file))
                {
                    sheet = file;
                    return true;
                }
            }

            sheet = null;
            return false;
        }

        /// <summary>
        /// Get the sheet server manifest
        /// </summary>
        /// <returns></returns>
        public SheetsServerManifest GetSheetsServerManifest()
        {
            return new SheetsServerManifest(GetServerSheetMetaData());
        }

        /// <summary>
        /// Get the sheet meta data from the cloud
        /// </summary>
        /// <param name="account">the account</param>
        /// <returns>returns the list of sheet meta data in the cloud</returns>
        public List<SheetMetaData> GetClosedSheets(Account account)
        {
            if (account == null)
                throw new NullReferenceException("the account cannot be null");


            List<string> filterOutIds = new List<string>();
            foreach (var x in SheetsManager.GetActiveCardSheets())
            {
                filterOutIds.Add(x.ID);
            }

            return GetFilteredListOfSheetMetaData(filterOutIds, account);
        }

        /// <summary>
        /// Returns a list of sheet meta data that does not contain any sheet meta data contained in the given filter out list
        /// </summary>
        /// <param name="filterOutSheetMetaDataIds">the sheet meta data that should not be in the resulting list</param>
        /// <returns>the list of sheets</returns>
        public List<SheetMetaData> GetFilteredListOfSheetMetaData(List<string> filterOutSheetMetaDataIds, Account account)
        {
            if (account == null)
                throw new NullReferenceException("The account cannot be null");

            if (filterOutSheetMetaDataIds == null)
                throw new NullReferenceException("The filter out sheet meta data ids list cannot be null");

            List<SheetMetaData> resultingSheetMetaData = new List<SheetMetaData>();

            filterOutSheetMetaDataIds.Sort();

            if (WarSystem.IsConnectedToServer)
            {
                foreach (var fileControlSheet in _sheetMetaDataList)
                {
                    var sheet = filterOutSheetMetaDataIds.Find((x) => x == fileControlSheet.Data.ID);

                    if (sheet == null && FileControl<SheetMetaData>.TryGetServerFile(fileControlSheet, WarSystem.ServerVersion, account, out var file))
                    {
                        resultingSheetMetaData.Add(fileControlSheet.Data);
                    }
                }
            }

            resultingSheetMetaData.Sort((x, y) =>
            {
                return x.SheetName.ToLower().CompareTo(y.SheetName.ToLower());
            });

            return resultingSheetMetaData;
        }

        /// <summary>
        /// returns a list of sheet meta data
        /// </summary>
        /// <returns></returns>
        public static List<FileControl<SheetMetaData>> GetServerSheetMetaData()
        {
            if (!WarSystem.IsConnectedToServer)
                return new List<FileControl<SheetMetaData>>();

            string path = GeneralSettings.Save_Location_Server + @"\Sheets";
            var files = Directory.GetFiles(path);

            List<FileControl<SheetMetaData>> data = new List<FileControl<SheetMetaData>>();

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                if (file.EndsWith(" Meta.json"))
                {

                    // Debug.Log(file);

                    try
                    {
                        string str = File.ReadAllText(file);

                        var md = SheetMetaData.CreateMetaDataFromJson(str);

                        if (md != null)
                        {
                            FileControl<SheetMetaData> f = new FileControl<SheetMetaData>(md, false, md.Version.ToString(), md.Categories,
                         md.Owner, File.GetCreationTime(file), File.GetLastAccessTime(file));
                            data.Add(f);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Checks to see if any sheets have been added/removed from the list on the server and machine. Updates the server and war manager accordingly
        /// </summary>
        /// <param name="currentManifest">the current manifest to compare the newly generated manifest to</param>
        /// <returns>returns the most updated manifest</returns>
        public static SheetsServerManifest Refresh(SheetsServerManifest currentManifest)
        {
            if (!WarSystem.IsConnectedToServer)
            {

                if (currentManifest == null)
                {
                    return new SheetsServerManifest(new List<FileControl<SheetMetaData>>());
                }

                return currentManifest;
            }

            //Debug.Log("Refreshing manifest");

            SheetsServerManifest newManifest = new SheetsServerManifest(GetServerSheetMetaData());


            if (currentManifest == null)
                return newManifest;

            List<string> sheetIds = new List<string>();
            List<Sheet<Cards.Card>> sheetsList = new List<Sheet<Cards.Card>>();

            foreach (var sheet in sheetsList)
            {
                sheetIds.Add(sheet.ID);
            }

            var notActiveSheets = WarSystem.CurrentSheetsManifest.GetFilteredListOfSheetMetaData(sheetIds, WarSystem.CurrentActiveAccount);

            List<FileControl<SheetMetaData>> sheetMetaDataList = currentManifest._sheetMetaDataList;

            for (int i = 0; i < sheetMetaDataList.Count; i++)
            {
                var sheet = notActiveSheets.Find(x => x.ID == sheetMetaDataList[i].Data.ID);

                if (sheet == null)
                {
                    if (newManifest.TryGetFileControlSheet(sheetMetaDataList[i].Data.ID, out var metaData))
                    {
                        currentManifest.ReplaceMetaData(metaData);
                    }
                }
                else
                {
                    SheetMetaData newMetaData = new SheetMetaData(sheetMetaDataList[i].Data.SheetName, sheetMetaDataList[i].Data.Owner, sheetMetaDataList[i].Data.Version,
                    sheetMetaDataList[i].Data.ID, sheetMetaDataList[i].Data.SheetPath, sheetMetaDataList[i].Data.CanEdit, sheetMetaDataList[i].Data.GridSize,
                    sheetMetaDataList[i].Data.Categories, DateTime.Now, null, null);

                    FileControl<SheetMetaData> newFileControlSheetMetaData = newMetaData.GetFileControl(sheetMetaDataList[i].CreationTime, DateTime.Now);

                    currentManifest.ReplaceMetaData(newFileControlSheetMetaData);
                }
            }

            foreach (var newSheet in newManifest.Sheets)
            {
                var sheet = sheetMetaDataList.Find(x => x.Data.ID == newSheet.Data.ID);

                if (sheet == null)
                {
                    currentManifest.AddSheet(newSheet);
                }
            }

            return currentManifest;
        }


        /// <summary>
        /// Save a list of sheet meta data back to the server
        /// </summary>
        /// <param name="controls">the list of file controlled meta data</param>
        /// <returns></returns>
        public static bool SaveServerMetaData(List<FileControl<SheetMetaData>> controls)
        {
            bool success = true;

            foreach (var control in controls)
            {
                if (!SaveServerMetaData(control))
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Save the server meta data to the server
        /// </summary>
        /// <returns>returns true if the file being written was saved successfully</returns>
        public static bool SaveServerMetaData(FileControl<SheetMetaData> control)
        {
            if (!WarSystem.IsConnectedToServer)
                return false;

            RecordMetaDataBeforeSave(control);

            string name = control.Data.SheetName;
            string json = control.Data.GetJson();

            // UnityEngine.Debug.Log(json);

            string path = GeneralSettings.Save_Location_Server_Sheets + @"\" + name + " Meta.json";

            // UnityEngine.Debug.Log("saving sheet " + path);

            File.WriteAllText(path, json);
            WarSystem.WriteToLog("Saved server meta data " + name, Logging.MessageType.logEvent);
            return true;
        }

        /// <summary>
        /// Record some extra data to the meta data right before saving
        /// </summary>
        /// <param name="sheet">the file controlled sheet</param>
        private static void RecordMetaDataBeforeSave(FileControl<SheetMetaData> sheet)
        {

            if (sheet == null || sheet.Data == null)
                return;

            if (SheetsManager.Camera != null)
            {

                if (SheetsManager.Camera == null)
                    Debug.Log("sheets manager camera is null");

                if (sheet.Data.ID == null)
                    Debug.Log("id is null");

                if (SheetsManager.Camera.GetCameraLocation(sheet.Data.ID) == null)
                    Debug.Log("camera location is null");

                sheet.Data.LastCameraLocation = SheetsManager.Camera.GetCameraLocation(sheet.Data.ID).Location.ConvertToDouble();


                string x = sheet.Data.LastCameraLocation[0].ToString();
                string y = sheet.Data.LastCameraLocation[1].ToString();
                // Debug.Log("saved camera location to the meta data " + x + ", " + y);
            }
            else
            {
                sheet.Data.LastDropPointLocation = new int[2] { 40, -20 };
            }

            sheet.Data.LastDropPointLocation = SheetDropPointManager.GetDropPoint(sheet.Data.ID).ConvertToIntArray();
        }


        /// <summary>
        /// Get the sheets last opened in a set amount of days
        /// </summary>
        /// <param name="days"></param>
        public SheetMetaData[] GetSheetsOpenedThisWeek(int days)
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int startOfWeek = today - DayOfWeek.Sunday;

            List<SheetMetaData> sheetsOpenedThisWeekList = new List<SheetMetaData>();

            foreach (var fileControlSheet in Sheets)
            {
                if (FileControl<SheetMetaData>.TryGetServerFile(fileControlSheet, fileControlSheet.FileVersion,
                    WarSystem.CurrentActiveAccount, out var sheet))
                {

                    if (DateTime.Now.Day - sheet.LastTimeOpened.Day < startOfWeek)
                    {
                        sheetsOpenedThisWeekList.Add(sheet);
                    }
                }
            }

            return sheetsOpenedThisWeekList.ToArray();
        }
    }
}
