
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace WarManager.Backend
{
    public class CSVMergeHandler
    {
        /// <summary>
        /// The information for the old file (the file getting updated)
        /// </summary>
        /// <value></value>
        public DataFileInstance OldFile { get; private set; }


        /// <summary>
        /// The information for the new file (the file getting imported)
        /// </summary>
        /// <value></value>
        public DataFileInstance NewFile { get; private set; }


        /// <summary>
        /// The information for the merged data
        /// </summary>
        /// <value></value>
        public DataFileInstance MergedFile { get; private set; }

        /// <summary>
        /// handes analyzing the incoming csv data
        /// </summary>
        /// <value></value>
        private CSVLineAnalyzer Analyzer { get; set; }


        public List<int> OldFileColumnAnchors { get; private set; }

        /// <summary>
        /// Shows the list of column anchors to drop for more precise comparisons
        /// </summary>
        /// <value></value>
        public List<int> NewFileColumnAnchors { get; private set; }

        /// <summary>
        /// Are the column anchors getting used in the merging calculation
        /// </summary>
        /// <value></value>
        public bool UseColumnAnchors { get; set; }

        /// <summary>
        /// The merge class constructor
        /// </summary>
        /// <param name="oldFile">the path of the old file</param>
        /// <param name="newFile">the path of the new file</param>
        /// <param name="directoryToSaveMergeCSVFile">the folder where the merged file will go after merging</param>
        /// <param name="mergedCSVFileName">the name of the merged file (without the csv extension)</param>
        public CSVMergeHandler(string oldFile, string newFile, string directoryToSaveMergeCSVFile, string mergedCSVFileName, List<int> oldFileColumnAnchors, List<int> newFileColumnAnchors)
        {
            if (!File.Exists(oldFile))
            {
                throw new FileNotFoundException("File not found " + oldFile);
            }

            if (!File.Exists(newFile))
            {
                throw new FileNotFoundException("file not found " + newFile);
            }

            if (!Directory.Exists(directoryToSaveMergeCSVFile))
            {
                throw new DirectoryNotFoundException("Cannot find the folder " + directoryToSaveMergeCSVFile);
            }

            // if (columnAnchors == null || columnAnchors.Count < 1)
            //     throw new System.Exception("The column anchors list cannot be null or less than one");

            FileInfo oldInfo = new FileInfo(oldFile);
            FileInfo newInfo = new FileInfo(newFile);

            if (oldInfo.Extension != ".csv")
            {
                throw new FileNotFoundException("Incorrect file type (not a csv file)" + oldFile);
            }

            if (newInfo.Extension != ".csv")
            {
                throw new FileNotFoundException("incorrect file type (not a csv file) " + newFile);
            }


            //OldFile = new CSVFileInfo()
            //{
            //    FilePath = oldFile,
            //    FileName = oldInfo.Name,
            //    File = CSVSerialization.GetCSVFile(oldFile)
            //};

            //NewFile = new CSVFileInfo()
            //{
            //    FilePath = newFile,
            //    FileName = newInfo.Name,
            //    File = CSVSerialization.GetCSVFile(newFile)
            //};


            //MergedFile = new CSVFileInfo()
            //{
            //    FilePath = directoryToSaveMergeCSVFile + @"\" + mergedCSVFileName + ".csv",
            //    FileName = mergedCSVFileName + ".csv",
            //    File = new List<string[]>()
            //};

            OldFile = CSVSerialization.Deserialize(oldFile);
            NewFile = CSVSerialization.Deserialize(newFile);
            MergedFile = CSVSerialization.Deserialize(directoryToSaveMergeCSVFile + @"\" + mergedCSVFileName + ".csv");

            Analyzer = new CSVLineAnalyzer(this, newFileColumnAnchors);

            // var oldList = GetSortedList(oldcsvFile);
            // var newList = GetSortedList(newcsvFile);

            // oldcsvFile.AddRange(newcsvFile);

            //var mergedList = GetSortedList(oldcsvFile);

            if (newFileColumnAnchors == null)
            {
                newFileColumnAnchors = new List<int>();
            }

            if (oldFileColumnAnchors == null)
            {
                oldFileColumnAnchors = new List<int>();
            }

            NewFileColumnAnchors = newFileColumnAnchors;
            OldFileColumnAnchors = oldFileColumnAnchors;
        }

        public bool CompareColumnHeaders()
        {
            return CSVLineAnalyzer.CheckColumnSyntax(OldFile.Header, NewFile.Header);
        }

        public SortedList<string, string[]> GetSortedList(List<string[]> csv, bool containsHeader = true)
        {
            Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
            SortedList<string, string[]> sortedList = new SortedList<string, string[]>();

            for (int i = 0; i < csv.Count - 1; i++)
            {
                if (containsHeader && i != 0)
                {
                    string key = string.Join(" ", csv[i]).Trim();

                    if (dict.ContainsKey((key)))
                    {
                        key = key + " " + Guid.NewGuid().ToString() + "~~~";
                    }

                    // Console.WriteLine( i.ToString() + ")" + key + " " + string.Join(",", csv[i]));

                    dict.Add(key, csv[i]);
                }
            }

            sortedList = new SortedList<string, string[]>(dict);

            return sortedList;
        }


        /// <summary>
        /// Merge the two csv files together
        /// </summary>
        public void Merge()
        {
            List<string[]> data = new List<string[]>();
            List<string[]> temp = new List<string[]>();

            List<CSVLinesHandler> linesHandlers = new List<CSVLinesHandler>();

            string[] header = OldFile.Header;

            // if (!CompareColumnHeaders())
            // {
            //     Console.WriteLine("The column headers are not the same - please re-order and rename the columns, and try again.");
            //     return;
            // }
            // else
            // {
            //     Console.WriteLine("Columns compared - success!");
            // }

            //OldFile.File.RemoveAt(0);
            //NewFile.File.RemoveAt(0);

            foreach (var x in OldFile.Data)
            {
                data.Add(x);
            }

            foreach (var y in NewFile.Data)
            {
                data.Add(y);
            }

            Console.WriteLine("added data");

            Dictionary<string, CSVLinesHandler> csvlines = new Dictionary<string, CSVLinesHandler>();

            while (data.Count > 0)
            {
                string currentRow = string.Join(" ", data[0]);

                //Console.WriteLine("\tKEY:" + currentRow);

                CSVLinesHandler linesHandler = null;

                int iterator = data.Count - 1;

                Console.WriteLine("\n\tCompare: " + currentRow);

                while (iterator >= 1 && iterator <= data.Count - 1)
                {
                    float compareScore = CSVLineAnalyzer.GetTokenRatio(currentRow, string.Join(" ", data[iterator]));

                    int compareScoreThreshold = 80;

                    if (compareScore >= compareScoreThreshold)
                    {
                        if (linesHandler == null)
                        {
                            linesHandler = new CSVLinesHandler(data[0], new CSVLine(data[iterator], compareScore));
                        }
                        else
                        {
                            linesHandler.AddLine(new CSVLine(data[iterator], compareScore));
                        }

                        Console.WriteLine(compareScore + ") " + string.Join(",", data[iterator]));
                    }

                    //Console.WriteLine(compareScore + ") " + string.Join(",", data[iterator]));

                    if (compareScore >= 80)
                    {
                        data.RemoveAt(iterator);
                    }

                    iterator--;
                }

                if (linesHandler == null)
                {
                    linesHandler = new CSVLinesHandler(data[0]);
                    // Console.WriteLine("adding to temp " + string.Join(",", currentRow));
                }

                if (!string.IsNullOrWhiteSpace(currentRow))
                    csvlines.Add(currentRow, linesHandler);

                data.RemoveAt(0);
            }

            // Console.WriteLine("count " + data.Count);

            SortedList<string, CSVLinesHandler> handlers = new SortedList<string, CSVLinesHandler>(csvlines);
            SerializeSortedList(header, handlers);

            // MergedFileInfo.File = data;
            // SeralizeMergedCSVFile();
        }

        /// <summary>
        /// Handles the Merging analysis with the column anchors
        /// </summary>
        public void MergeWithColumnAnchors()
        {

            if (!UseColumnAnchors)
            {
                return;
            }

            List<string[]> data = new List<string[]>();
            List<string[]> temp = new List<string[]>();

            List<CSVLinesHandler> linesHandlers = new List<CSVLinesHandler>();

            string[] header = OldFile.Header;

            // if (!CompareColumnHeaders())
            // {
            //     Console.WriteLine("The column headers are not the same - please re-order and rename the columns, and try again.");
            //     return;
            // }
            // else
            // {
            //     Console.WriteLine("Columns compared - success!");
            // }

            //OldFile.File.RemoveAt(0); // think about this...
            //NewFile.File.RemoveAt(0);//think about this...

            foreach (var x in OldFile.Data)
            {
                for (int i = 0; i < OldFileColumnAnchors.Count; i++)
                {
                    string[] str = new string[1] { x[i] };
                    data.Add(str);
                }
            }

            foreach (var x in OldFile.Data)
            {
                for (int i = 0; i < OldFileColumnAnchors.Count; i++)
                {
                    string[] str = new string[1] { x[i] };
                    data.Add(str);
                }
            }

            //Console.WriteLine("added data");

            Dictionary<string, CSVLinesHandler> csvlines = new Dictionary<string, CSVLinesHandler>();

            while (data.Count > 0)
            {
                string currentRow = string.Join(" ", data[0]);

                //Console.WriteLine("\tKEY:" + currentRow);

                CSVLinesHandler linesHandler = null;

                int iterator = data.Count - 1;

                Console.WriteLine("\n\tCompare: " + currentRow);

                while (iterator >= 1 && iterator <= data.Count - 1)
                {
                    float compareScore = CSVLineAnalyzer.GetTokenRatio(currentRow, string.Join(" ", data[iterator]));

                    int compareScoreThreshold = 80;

                    if (compareScore >= compareScoreThreshold)
                    {
                        if (linesHandler == null)
                        {
                            linesHandler = new CSVLinesHandler(data[0], new CSVLine(data[iterator], compareScore));
                        }
                        else
                        {
                            linesHandler.AddLine(new CSVLine(data[iterator], compareScore));
                        }

                        Console.WriteLine(compareScore + ") " + string.Join(",", data[iterator]));
                    }

                    //Console.WriteLine(compareScore + ") " + string.Join(",", data[iterator]));

                    if (compareScore >= 80)
                    {
                        data.RemoveAt(iterator);
                    }

                    iterator--;
                }

                if (linesHandler == null)
                {
                    linesHandler = new CSVLinesHandler(data[0]);
                    // Console.WriteLine("adding to temp " + string.Join(",", currentRow));
                }

                if (!string.IsNullOrWhiteSpace(currentRow))
                    csvlines.Add(currentRow, linesHandler);

                data.RemoveAt(0);
            }

            // Console.WriteLine("count " + data.Count);

            SortedList<string, CSVLinesHandler> handlers = new SortedList<string, CSVLinesHandler>(csvlines);
            SerializeSortedList(header, handlers);

            // MergedFileInfo.File = data;
            // SeralizeMergedCSVFile();
        }

        void SerializeSortedList(string[] header, SortedList<string, CSVLinesHandler> sortedList)
        {
            List<string[]> final = new List<string[]>();

            foreach (var i in sortedList)
            {
                if (i.Value != null)
                    final.Add(i.Value.GetBestLine().Row);
            }

            CSVSerialization.Serialize(MergedFile.FilePath, header, final);
        }

        public void SeralizeMergedCSVFile()
        {
            var info = new List<string[]>();

            foreach (var i in MergedFile.Data)
            {
                info.Add(i);
            }

            CSVSerialization.Serialize(MergedFile.FilePath, MergedFile.Header, info);
        }
    }
}