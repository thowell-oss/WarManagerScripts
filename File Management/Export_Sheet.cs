/* Export_Sheet.cs
 * Author: Taylor Howell
 */

using System;
using System.IO;
using System.Text;

using System.Text.Json;
using System.Collections.Generic;

using WarManager.Backend;
using WarManager.Sharing.Security;

using UnityEngine;


/// <summary>
/// Handles aspects of exporting, importing and coverting files
/// </summary>
namespace WarManager.Sharing
{
    /// <summary>
    /// Export a sheet
    /// </summary>
    public class Export_Sheet<Tobj> where Tobj : ICompareWarManagerPoint, IFileContentInfo
    {
        private string backupFileLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\War Manager\Offline";

        /// <summary>
        /// Export the file sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <returns>returns true if the file was successfuly saved, false if not</returns>
        public bool ExportSheet(Sheet<Tobj> sheet, byte[] key)
        {
            if (sheet == null)
                throw new NullReferenceException("Cannot export a null sheet");

            string saveLocation = GeneralSettings.Save_Location_Server_Sheets;

            if (WarSystem.IsConnectedToServer)
            {

                bool canWriteFile = true;

                if (canWriteFile)
                {
                    if (sheet.FileName != null && sheet.FileName != string.Empty)
                    {
                        WarSystem.WriteToLog($"Attempting to write a local file {saveLocation}\\{sheet.FileName}.wmsht", Logging.MessageType.info);
                        return EncryptFile(sheet, saveLocation, sheet.FileName, key);
                    }
                    else
                    {
                        WarSystem.WriteToLog($"Attempt to write a local file {saveLocation}\\{sheet.FileName}.wmsht rejected", Logging.MessageType.critical);
                    }
                }
            }
            else
            {
                //NotificationHandler.Print("Attempt to save \'" + sheet.Name + "\' failed. Not connected to server. Please connect to the server again to save.");
                MessageBoxHandler.Print_Immediate("Attempt to save \'" + sheet.Name + "\' failed. Not connected to server. Please connect to the server again to save.", "Critical Error");
            }

            return false;

        }

        /// <summary>
        /// Encrypts and writes a file to a specified save path
        /// </summary>
        /// <param name="sheet">the sheet to save</param>
        /// <param name="savePath">the path to save the sheet</param>
        /// <param name="fileName">the name of the file</param>
        /// <param name="key">the password hash key</param>
        /// <returns>returns true if the file was saved successfully, false if not</returns>
        private bool EncryptFile(Sheet<Tobj> sheet, string savePath, string fileName, byte[] key)
        {
            bool success = false;
            try
            {
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    savePath = backupFileLocation;
                    WarSystem.WriteToLog("Using Offline Directory " + savePath, Logging.MessageType.info);
                }

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                string fileWritePath = savePath + @"\" + fileName + ".wmsht";
                // Debug.Log(fileWritePath);
                string fileSetup = GenerateFileSetup();
                // Debug.Log(fileSetup);
                string generalSheetSetup = GetGeneralFileSettings_CSV(sheet);
                // Debug.Log(generalSheetSetup);
                string datasetIds = GetDatasetIDs(sheet);
                // Debug.Log(datasetIds);
                string layerInfo = GetSheetLayers_CSV(sheet);
                // Debug.Log(layerInfo);
                string objInfo = WriteObjects_CSV(sheet.GetAllGridPartitions());
                // Debug.Log(objInfo);

                string file = fileSetup + generalSheetSetup + "" + datasetIds + "\n" + layerInfo + "\n" + objInfo;

                WarSystem.WriteToLog("File decrypted contents:\n" + file, Logging.MessageType.info);

                string checkSum = Encryption.GetMD5Hash(file);

                string decryptedFile = checkSum + "" + file;


                byte[] IV = Guid.NewGuid().ToByteArray();
                byte[] encryptedFile = Encryption.EncryptStringToBytes_Aes(decryptedFile, key, IV);


                string ivStr = "";
                for (int i = 0; i < IV.Length; i++)
                {
                    ivStr = ivStr + ((int)IV[i]).ToString() + ",";
                }

                //File.WriteAllBytes(fileWritePath, encryptedFile);

                //File.WriteAllBytes(GeneralSettings.Save_Location_Offline + "\\" + "text.txt", IV);

                if (File.Exists(fileWritePath))
                {
                    File.Delete(fileWritePath);
                }

                using (FileStream stream = new FileStream(fileWritePath, FileMode.Create))
                {
                    stream.Write(IV, 0, IV.Length);

                    for (int i = 0; i < encryptedFile.Length; i++)
                    {
                        stream.WriteByte(encryptedFile[i]);
                    }
                }
                success = true;
                // UnityEngine.Debug.Log($"File save successful ({fileWritePath})");
                WarSystem.WriteToLog($"File save successful ({fileWritePath})", Logging.MessageType.info);

            }
            catch (Exception ex)
            {
                WarSystem.WriteToLog($"Error: Could not save file {ex.Message}", Logging.MessageType.critical);
                //send me an email here
            }

            return success;
        }

        /// <summary>
        /// Generates the file type
        /// </summary>
        /// <returns></returns>
        private string GenerateFileSetup()
        {
            string fileVersion = GeneralSettings.Save_Sheet_Version.ToString();
            return fileVersion.Length.ToString() + SheetsManager.CardSheetExtension + GeneralSettings.Save_Sheet_Version;
        }


        /// <summary>
        /// Convert a list of grid partition objects into a csv file string
        /// </summary>
        /// <param name="gridPartitions">the grid partitions from the sheet</param>
        /// <returns>returns a string</returns>
        private string WriteObjects_CSV(List<GridPartition<Tobj>> gridPartitions)
        {
            if (gridPartitions == null || gridPartitions.Count < 1)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            foreach (var partition in gridPartitions)
            {
                if (!partition.placeHolder)
                {
                    var objects = partition.GetAllObjects();

                    if (objects.Length > 0)
                    {

                        string objectsContent = "";

                        string[] cleaner = null;

                        string prevResults = null;

                        foreach (var obj in objects)
                        {
                            if (obj != null)
                            {
                                List<string> str = new List<string>();
                                str.AddRange(obj.GetContent());
                                int len = str.Count;

                                bool removed = false;
                                List<int> removedList = new List<int>();
                                List<int> keptList = new List<int>();
                                if (cleaner == null)
                                {
                                    cleaner = str.ToArray();
                                }
                                else
                                {
                                    string[] copy = new string[str.Count];
                                    Array.Copy(str.ToArray(), copy, str.Count);

                                    for (int i = cleaner.Length - 1; i >= 0; i--)
                                    {
                                        if (i < str.Count)
                                        {
                                            if (cleaner[i] == str[i])
                                            {
                                                str.RemoveAt(i);
                                                removed = true;
                                                removedList.Add(i);
                                            }
                                            else
                                            {
                                                keptList.Add(i);
                                            }
                                        }
                                    }

                                    if (removed)
                                        cleaner = copy;
                                }

                                if (!removed)
                                {
                                    if (objectsContent.Length < 1 || objectsContent[objectsContent.Length - 1] == ',')
                                    {
                                        objectsContent = objectsContent + String.Join(",", str);
                                    }
                                    else
                                    {
                                        objectsContent = objectsContent + "," + String.Join(",", str);
                                    }
                                }
                                else
                                {
                                    string results;
                                    if (str.Count / len > 1f)
                                    {
                                        results = ">";

                                        for (int i = 0; i < removedList.Count; i++)
                                        {
                                            results = results + removedList[i].ToString();

                                            if (i == removedList.Count - 1)
                                            {
                                                results += ">";
                                            }
                                            else
                                            {
                                                results += ",";
                                            }
                                        }

                                    }
                                    else
                                    {
                                        results = "<";

                                        for (int i = 0; i < keptList.Count; i++)
                                        {
                                            results = results + keptList[i].ToString();

                                            if (i == keptList.Count - 1)
                                            {
                                                results += "<";
                                            }
                                            else
                                            {
                                                results += ",";
                                            }
                                        }
                                    }


                                    if (prevResults != results)
                                    {
                                        prevResults = results;
                                        objectsContent = objectsContent + results + String.Join(",", str);
                                        //Debug.Log("results changed");
                                    }
                                    else
                                    {
                                        objectsContent = objectsContent + "," + String.Join(",", str);
                                    }

                                }
                            }

                        }

                        builder.Append(objectsContent + "\n");
                    }
                }
            }

            return builder.ToString();
        }

        private string GetDatasetIDs(Sheet<Tobj> sheet)
        {
            string[] ids = sheet.GetDatasetIDs();

            return string.Join(",", ids);
        }

        /// <summary>
        /// Convert the general settings of the sheet to get ready to export
        /// </summary>
        /// <param name="sheet">the sheet to export</param>
        /// <returns>returns a string</returns>
        private string GetGeneralFileSettings_CSV(Sheet<Tobj> sheet)
        {
            string id = sheet.ID;
            string name = sheet.Name;
            string FileName = sheet.FileName;
            string dpId = "*";

            string finalStr = id + "," + name + "," + FileName + "," + dpId + "\n";

            return finalStr;
        }

        /// <summary>
        /// Convert the layers of the sheet into a string
        /// </summary>
        /// <param name="sheet">the sheet</param>
        /// <returns>returns a csv style string of all the layers in the sheet</returns>
        private string GetSheetLayers_CSV(Sheet<Tobj> sheet) // does the default layer always come first?
        {
            var layers = sheet.GetAllLayers();

            string finalString = string.Empty;

            for (int i = 1; i < layers.Count; i++)
            {
                var layer = layers[i];

                string id = layer.ID;
                string name = layer.Name;
                string color = layer.Color;

                string layerString = id + "," + name + "," + color;

                finalString = finalString + layerString + ",";
            }

            if (finalString != string.Empty)
            {
                return finalString.Remove(finalString.Length - 1);
            }
            else
            {
                return finalString;
            }
        }
    }
}
