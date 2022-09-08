
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using WarManager;
using WarManager.Unity3D;
using WarManager.Unity3D.Windows;

namespace WarManager.Backend
{
    /// <summary>
    /// Controls the process of merging data
    /// </summary>
    [Notes.Author("Controls the process of merging data")]
    public class DataMergeController
    {

        private string _newFilePath = "";
        private int _newIdLocation = -1;
        private DataFileInstance _newFileInstance;

        private string _datasetName = "";
        private int _oldIdLocation = -1;
        private DataFileInstance _oldFileInstance;


        /// <summary>
        /// Empty merge
        /// </summary>
        public void MergeFiles()
        {
            MergeFiles(null);
        }

        /// <summary>
        /// Merge the files
        /// </summary>
        /// <param name="set">The data set being merged</param>
        /// <param name="newFilePath">The new file location</param>
        public void MergeFiles(DataSet set)
        {
            if (set == null)
            {
                return;
            }

            List<SlideWindow_Element_ContentInfo> content = new List<SlideWindow_Element_ContentInfo>();

            content.Add(new SlideWindow_Element_ContentInfo(20));
            content.Add(new SlideWindow_Element_ContentInfo("Merge", null));
            content.Add(new SlideWindow_Element_ContentInfo("", "Merge data from a new comma delimited (CSV) file to an existing data set."));
            content.Add(new SlideWindow_Element_ContentInfo(20));


            if (set != null)
            {
                _datasetName = set.DatasetName;
                _oldFileInstance = set.DataFile;
            }

            content.Add(new SlideWindow_Element_ContentInfo("Existing Data", 20));
            content.Add(new SlideWindow_Element_ContentInfo("Data Set", _datasetName));

            if (_oldFileInstance != null)
            {
                content.Add(new SlideWindow_Element_ContentInfo("", "Tell War Manager what column is always unique (names, phone numbers, emails, ids, etc)."));
                content.Add(new SlideWindow_Element_ContentInfo("Identity Column", _oldIdLocation.ToString(), (x) =>
                {

                    if (int.TryParse(x, out var loc))
                    {
                        if (loc > 0 && loc < _newFileInstance.HeaderLength)
                        {
                            _oldIdLocation = loc;
                        }
                        else
                        {
                            MessageBoxHandler.Print_Immediate("The column location must match to an actual column", "Error");
                        }
                    }
                    else
                    {
                        MessageBoxHandler.Print_Immediate("Could not interperate a csv column location from " + x, "Error");
                    }

                    MergeFiles(set);
                }));
            }
            else
            {

                content.Add(new SlideWindow_Element_ContentInfo("Identity Column", "Disabled - file not found"));
            }

            content.Add(new SlideWindow_Element_ContentInfo(50));

            content.Add(new SlideWindow_Element_ContentInfo("New Data", 20));
            content.Add(new SlideWindow_Element_ContentInfo("Location", _newFilePath, (x) =>
             {
                 _newFilePath = x;
                 _newFileInstance = CSVSerialization.Deserialize(_newFilePath);

                 MergeFiles(set);
             }));

            if (_newFileInstance != null)
            {
                content.Add(new SlideWindow_Element_ContentInfo("", "Tell War Manager what column is always unique (names, phone numbers, emails, ids, etc). " +
                    "The information in this column must match with the column above (not the same column location but the data in the column)"));
                content.Add(new SlideWindow_Element_ContentInfo("Identity Column", _newIdLocation.ToString(), (x) =>
                {

                    if (int.TryParse(x, out var loc))
                    {
                        if (loc > 0 && loc < _newFileInstance.HeaderLength)
                        {
                            _newIdLocation = loc;
                        }
                        else
                        {
                            MessageBoxHandler.Print_Immediate("The column location must match to an actual column", "Error");
                        }
                    }
                    else
                    {
                        MessageBoxHandler.Print_Immediate("Could not interperate a csv column location from \'" + x + "\'", "Error");
                    }

                    MergeFiles(set);
                }));
            }
            else
            {

                content.Add(new SlideWindow_Element_ContentInfo("Identity Column", "Disabled - file not found"));
            }

            content.Add(new SlideWindow_Element_ContentInfo(50));
            if (_oldFileInstance != null && _newFileInstance != null && _oldIdLocation > 0 && _oldIdLocation < _oldFileInstance.HeaderLength && _newIdLocation > 0 && _newIdLocation < _newFileInstance.HeaderLength)
            {
                content.Add(new SlideWindow_Element_ContentInfo("Merge", 0, (x) => { }, null));
            }
            else
            {

            }

            SlideWindowsManager.main.AddPropertiesContent(content, true);
        }

        /// <summary>
        /// Merge the two files together
        /// </summary>
        /// <param name="existingFilePath">the current file that is being used</param>
        /// <param name="newFilePath">the new file of information</param>
        /// <param name="mergedFileDirectory">the directory of the new file</param>
        /// <param name="mergeFileName">the name of the new file</param>
        /// <exception cref="FileNotFoundException">The file has not been found</exception>
        /// <exception cref="DirectoryNotFoundException">The directory could not be found</exception>
        /// <exception cref="Exception">Triggers when anything else goes wrong</exception>
        private void MergeFiles(string existingFilePath, string newFilePath, string mergedFileDirectory, string mergeFileName)
        {
            try
            {
                CSVMergeHandler mergeHandler = new CSVMergeHandler(existingFilePath, newFilePath, mergedFileDirectory, mergeFileName, new List<int>(), new List<int>());
                mergeHandler.Merge();
            }
            catch (FileNotFoundException notFoundEx)
            {
                MessageBoxHandler.Print_Immediate(notFoundEx.Message, "File Error");
            }
            catch (DirectoryNotFoundException drNotFoundEx)
            {
                MessageBoxHandler.Print_Immediate(drNotFoundEx.Message, "Folder Error");
            }
            catch (Exception ex)
            {
                MessageBoxHandler.Print_Immediate(ex.Message, "Error");
            }
        }
    }
}
