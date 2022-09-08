/* Import_Sheet.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Threading;
using System.Threading.Tasks;

using System.Text;
using System.Text.RegularExpressions;

using System.Security;
using System.Security.Cryptography;

using UnityEngine;

using WarManager.Sharing.Security;
using WarManager.Backend;

namespace WarManager.Sharing
{
    /// <summary>
    /// Loads a sheet into memory
    /// </summary>
    public class Import_Sheet<Tobj> where Tobj : ICompareWarManagerPoint, IFileContentInfo
    {

        /// <summary>
        /// The resulting sheet payload after the file has been imported
        /// </summary>
        /// <value></value>
        public static SheetPayload<Tobj> Payload { get; private set; }


        /// <summary>
        /// Removes the payload from memory
        /// </summary>
        public static void ClearPayload()
        {
            Payload = null;
        }


        /// <summary>
        /// Attempts to open the file from local means
        /// </summary>
        /// <param name="filePath">the file path (where the file is located)</param>
        /// <param name="dataPieces">the qnty pieces of information saved in the file for each object</param>
        /// <param name="importSheet">the out sheet that was imported</param>
        /// <param name="importedObjData">the data to be configured and refernced in the sheet</param>
        /// <returns>returns true if the operation was successfull, false if not</returns>
        public static SheetPayload<Tobj> ImportLocalSheet(string filePath, byte[] key, int dataPieces)
        {
            WarSystem.WriteToLog($"Attempting to open local wmsht file {filePath}", Logging.MessageType.info);

            Sheet<Tobj> sheet;
            List<string[]> objInfo = new List<string[]>();

            if (!filePath.EndsWith(".wmsht"))
                throw new System.NotSupportedException("Cannot open file that does not end with \'.wmsht\'");

            string b = DecryptFile(filePath, key);

            if (b == null || b.Length < 32)
                throw new FileLoadException("Cannot read file");

            if (TryGetCardSheet(b, dataPieces, out sheet, out objInfo))
            {
                WarSystem.WriteToLog($"wmsht file opened {filePath}", Logging.MessageType.logEvent);

                Payload = new SheetPayload<Tobj>(sheet, objInfo);

                return Payload;
            }

            WarSystem.WriteToLog($"wmsht file not opened {filePath}", Logging.MessageType.error);

            MessageBoxHandler.Print_Immediate("Could not open sheet. The file might be corrupted.", "Error");

            return null;
        }

        private static string DecryptFile(string filepath, byte[] key)
        {

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("Could not find the file - contact support. \n<size=75%>' " + filepath + " '</size>", "Error Opening File");
            }

            var bytes = File.ReadAllBytes(filepath);

            byte[] IV = new byte[16];
            int i = 0;
            while (i < 16)
            {
                IV[i] = bytes[i];
                i++;
            }

            byte[] finishedFile = new byte[bytes.Length - 16];

            while (i < bytes.Length)
            {
                finishedFile[i - 16] = bytes[i];
                i++;
            }

            var dec = Encryption.DecryptStringFromBytes_Aes(finishedFile, key, IV);
            WarSystem.WriteToLog($"Attempting wmsht file decrypted {filepath}", Logging.MessageType.info);
            return dec;
        }

        private static async Task<string> DecryptFileAsync(string filepath, byte[] key, CancellationToken ct)
        {


            byte[] bytes = new byte[0];

            try
            {
                using (FileStream SourceStream = File.Open(filepath, FileMode.Open))
                {
                    bytes = new byte[SourceStream.Length];
                    await SourceStream.ReadAsync(bytes, 0, (int)SourceStream.Length, ct);
                }

            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Failed to open file:/n" + ex.Message);
                //EmailClient.SendNotificationSMTPEmailToDev("Attempt to open file failed", "Attempt to open file failed./nAccount: " + WarSystem.CurrentActiveAccount.ToString() + "/n" + ex.Message);
                WarSystem.WriteToDev("Attempt to open file failed./nAccount: " + WarSystem.CurrentActiveAccount.ToString() + "/n" + ex.Message, Logging.MessageType.critical);
                return null;
            }

            byte[] IV = new byte[16];
            int i = 0;
            while (i < 16)
            {
                IV[i] = bytes[i];
                i++;
            }

            byte[] finishedFile = new byte[bytes.Length - 16];

            while (i < bytes.Length)
            {
                finishedFile[i - 16] = bytes[i];
                i++;
            }

            try
            {
                var dec = Encryption.DecryptStringFromBytes_Aes(finishedFile, key, IV);
                WarSystem.WriteToLog($"Attempting wmsht file decrypted {filepath}", Logging.MessageType.info);

                return dec;
            }
            catch (Exception ex)
            {
                NotificationHandler.Print("Failed to decypt file:/n" + ex.Message);
                EmailClient.SendNotificationSMTPEmailToDev("Attempt to open file failed", "Attempt to open file failed./nAccount: " + WarSystem.CurrentActiveAccount.ToString() + "/n" + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Load a sheet into memory
        /// </summary>
        /// <param name="fileName">the file name</param>
        /// <returns>returns the loaded sheet</returns>
        private static bool TryGetCardSheet(string file, int dataPieces, out Sheet<Tobj> objSheet, out List<string[]> objSetupInfo)
        {

            string[] fileLines = file.ToString().Split('\n');
            int fileLineLength = fileLines.Length;

            if (fileLineLength < 1)
                throw new FileLoadException("Cannot read file");

            string fileInfo = fileLines[0];

            string sheetInfo = "";

            WarSystem.WriteToLog("File Contents: \n" + file, Logging.MessageType.info);

            if (!TryRemoveFileInfo(fileInfo, file.ToString(), out sheetInfo))
            {
                throw new NotSupportedException("Cannot open file - file is not a war manager sheet");
            }
            //Debug.Log("Found sheet");
            var tokens = sheetInfo.Split(',');
            if (tokens.Length != 4)
            {
                throw new NotSupportedException("Cannot open file - the token length is incorrect");
            }

            var sheetId = tokens[0];
            var sheetUserDefinedName = tokens[1];
            var sheetFileName = tokens[2];
            var sheetCategory = tokens[3];

            //Debug.Log(sheetId);

            if (SheetsManager.GetActiveSheet(sheetId) != null)
            {
                throw new NotSupportedException("File already open");
            }

            //Debug.Log("No active sheet with this id");

            Sheet<Tobj> sheet = new Sheet<Tobj>(sheetId, sheetUserDefinedName, sheetFileName, true);
            objSheet = sheet;

            //Debug.Log("Created sheet");

            int datatsetLine = 1;
            if (fileLineLength > datatsetLine)
            {
                string datasetInfo = fileLines[datatsetLine];

                if (TryGetDataSetIDs(datasetInfo, out var ids))
                {
                    foreach (var id in ids)
                    {
                        sheet.AddDataset(id);
                        // Debug.Log("added dataset" + id);
                    }
                }
            }

            string layerLine = null;

            int nextLayerLine = 2;

            if (fileLineLength > nextLayerLine)
            {
                layerLine = fileLines[nextLayerLine];
            }

            Layer[] layers = null;

            if (TryGetLayers(layerLine, out layers))
            {
                sheet.AddLayers(layers);
            }

            int nextLine = 3;
            List<string> cardStrings = new List<string>();

            while (nextLine < fileLineLength)
            {
                if (!string.IsNullOrEmpty(fileLines[nextLine]))
                    cardStrings.Add(fileLines[nextLine]);

                nextLine++;
            }

            List<string[]> SetupInfo = new List<string[]>();

            if (cardStrings.Count > 0)
            {
                if (TryGetObjStr(cardStrings.ToArray(), sheet.ID, dataPieces, out SetupInfo))
                {
                    objSetupInfo = SetupInfo;
                    // Debug.Log("added obj data");

                    return true;
                }
                else
                {
                    Debug.LogError("Could not find objects");
                    objSetupInfo = new List<string[]>();
                    objSheet = sheet;
                    return false;
                }
            }
            else
            {
                objSheet = sheet;
                objSetupInfo = new List<string[]>();
                return true;
            }

        }

        /// <summary>
        /// Check to see that the opened sheet is a war manager sheet
        /// </summary>
        /// <param name="line">the line that contains the file string</param>
        /// <returns>returns true if the file properties are correct, false if not</returns>
        private static bool TryRemoveFileInfo(string line, string fileInput, out string removedVersion)
        {

            if (string.IsNullOrWhiteSpace(line) || !Regex.IsMatch(line, @"^[a-z0-9]{32}[0-9]+(.wmsht)(\w)+", RegexOptions.IgnoreCase))
            {
                removedVersion = line;

                Debug.LogError("Could not match " + line);

                return false;
            }

            var checkSum = line.Substring(0, 32);
            fileInput = fileInput.Remove(0, 32);

            if (!Encryption.VerifyMD5Hash(fileInput, checkSum))
            {
                throw new FileLoadException("File corrupted");
            }

            line = line.Remove(0, 32);

            var match = Regex.Match(line, @"^[0-9]+(.wmsht)", RegexOptions.IgnoreCase);
            string str = match.Value;

            string parseStr = match.Value;
            string vLength = parseStr.Remove(parseStr.Length - 6);

            int len = 0;
            int.TryParse(vLength, out len);

            if (len > 0)
            {
                string fileVersion = GeneralSettings.Save_Sheet_Version.ToString();
                string fileCompareVersion = new string(line.ToCharArray(), parseStr.Length, len);
                // Debug.Log(fileCompareVersion);

                if (fileCompareVersion != fileVersion)
                {
                    removedVersion = line;
                    Debug.LogError("the versions are incorrect " + "\'" + fileCompareVersion + "\'");
                    return false;
                }
                else
                {
                    removedVersion = line.Remove(0, parseStr.Length + len);

                    return true;
                }
            }

            Debug.LogError("len is 0");

            removedVersion = line;
            return false;
        }


        /// <summary>
        /// Attempt to get the dataset ids associated with the sheet
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dataSetIds"></param>
        /// <returns></returns>
        private static bool TryGetDataSetIDs(string input, out string[] dataSetIds)
        {
            if (input == null || input == string.Empty)
            {
                dataSetIds = new string[0];
                return false;
            }

            dataSetIds = input.Split(',');
            return true;
        }


        /// <summary>
        /// Add the layer information to the sheet
        /// </summary>
        /// <param name="sheet">the newly created sheet</param>
        private static bool TryGetLayers(string input, out Layer[] createdLayers)
        {

            if (input == null || input.Length < 1)
            {
                createdLayers = null;
                return false;
            }

            string[] splitStr = input.Split(',');

            if (splitStr.Length % 3 != 0)
            {
                throw new FileLoadException("Layers corrupted");
            }

            int len = (int)(splitStr.Length / 3);
            createdLayers = new Layer[len];

            int sortOrder = 1;

            for (int i = 0; i < splitStr.Length; i += 3)
            {
                sortOrder = i + 1;

                var layerId = splitStr[i];
                var layerName = splitStr[i + 1];
                var colorStr = splitStr[i + 2];

                Layer l = new Layer(layerName, sortOrder, colorStr, layerId);
                createdLayers[i / 3] = l;
            }



            return true;
        }

        /// <summary>
        /// Add cards to sheet
        /// </summary>
        /// <param name="input">the string information of cards</param>
        /// <returns>returns true if there were no issues reading cards, false if there were </returns>
        private static bool TryGetObjStr(string[] input, string sheetId, int dataPieces, out List<string[]> objStrInfo)
        {
            if (input == null || input.Length < 1)
            {
                objStrInfo = null;

                Debug.LogError("input is null or missing");

                return false;
            }

            //Debug.Log(String.Join("|", input));

            objStrInfo = new List<string[]>();

            for (int i = 0; i < input.Length; i++)
            {
                if (Regex.IsMatch(input[i], @"^[\w:\,\*\-]+((<([0-9]\,?)+<[\w:\,\*\-]+)+)?$"))
                {

                    //Debug.Log(input[i]);

                    string[] fileInfo = Regex.Split(input[i], @"<([0-9]\,?)+<", RegexOptions.ExplicitCapture);
                    string initTemplate = fileInfo[0];

                    objStrInfo.Add(fileInfo[0].Split(','));

                    //Debug.Log(String.Join(",", initTemplate));

                    //Debug.Log(String.Join("||", fileInfo));


                    var matchVectors = Regex.Matches(input[i], @"<([0-9]\,?)+<");
                    string[] vectors = new string[matchVectors.Count];

                    for (int j = 0; j < matchVectors.Count; j++)
                    {
                        vectors[j] = matchVectors[j].Value;
                        vectors[j] = vectors[j].Replace("<", string.Empty);
                        vectors[j] = vectors[j].Replace(">", string.Empty);
                    }

                    //Debug.Log(String.Join("|", vectors));

                    string temp = initTemplate;

                    for (int k = 0; k < vectors.Length; k++)
                    {
                        string nextTemp = "";
                        objStrInfo.AddRange(ExportReplacedGroups(temp, vectors[k], fileInfo[k + 1], out nextTemp));

                        temp = nextTemp;
                    }

                }
                else
                {
                    //Debug.LogError("Regex does not match");
                    NotificationHandler.Print("File might be corrupted");
                }
            }

            // for(int i= 0; i < objStrInfo.Count; i++)
            // {
            //     Debug.Log( i + ") " + string.Join(",", objStrInfo[i]));
            // }

            if (objStrInfo.Count > 0)
                return true;

            return false;

        }

        private static List<string[]> ExportReplacedGroups(string template, string vectorInstructions, string deltas, out string newTemplate)
        {
            List<string[]> groups = new List<string[]>();

            string[] temp = template.Split(',');

            int splitAmt = 1;
            string[] instructions;

            if (vectorInstructions.Length > 1)
            {
                instructions = vectorInstructions.Split(',');
                splitAmt = instructions.Length;
            }
            else
            {
                instructions = new string[1] { vectorInstructions };
            }

            string[] deltasSplit = deltas.Split(',');

            //Debug.Log(vectorInstructions);
            //Debug.Log(String.Join(",", instructions) + " " + splitAmt + " " + deltasSplit.Length);

            if (deltasSplit.Length % splitAmt != 0)
            {
                throw new FileLoadException("Cannot load file - error parsing deltas");
            }

            for (int i = 0; i < deltasSplit.Length; i += splitAmt)
            {
                int k = 0;
                for (int j = splitAmt - 1; j >= 0; j--)
                {
                    //Debug.Log((i - j) + " " + k);
                    int location = 0;

                    if (Int32.TryParse(instructions[j], out location))
                    {
                        temp[location] = deltasSplit[i + k];

                    }
                    else
                    {
                        throw new FileLoadException("Cannot load file - error parsing deltas");
                    }

                    k++;
                }

                //Debug.Log("temp " + i + " " + String.Join("," , temp));

                string[] copy = new string[temp.Length];

                Array.Copy(temp, copy, temp.Length);

                groups.Add(copy);
                //Debug.Log(String.Join(",", temp));
            }

            if (groups.Count > 0)
            {
                newTemplate = String.Join(",", groups[groups.Count - 1]);

                return groups;
            }
            else
            {
                Debug.LogError("Something went wrong");
                newTemplate = template;
                return null;
            }
        }

    }
}
