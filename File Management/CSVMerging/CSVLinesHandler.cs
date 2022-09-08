using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Backend
{
    public class CSVLinesHandler
    {
        /// <summary>
        /// The base key of all the csv lines
        /// </summary>
        /// <value></value>
        public string[] Key { get; private set; }

        /// <summary>
        /// The list of csv lines to handle
        /// </summary>
        /// <typeparam name="CSVLine"></typeparam>
        /// <returns></returns>
        List<CSVLine> _lines { get; set; } = new List<CSVLine>();

        /// <summary>
        /// The best score of all the stored lines
        /// </summary>
        /// <value></value>
        public float BestScore { get; private set; } = 0;

        /// <summary>
        /// Base construtor
        /// </summary>
        /// <param name="key"></param>
        public CSVLinesHandler(string[] key)
        {
            Key = key;
        }


        /// <summary>
        /// Add a key and import the first csv line
        /// </summary>
        /// <param name="key"></param>
        /// <param name="firstLine"></param>
        public CSVLinesHandler(string[] key, CSVLine firstLine)
        {
            Key = key;
            AddLine(firstLine);
        }

        /// <summary>
        /// Add a line to the list
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(CSVLine line)
        {
            _lines.Add(line);

            if (BestScore < line.Score)
            {
                BestScore = line.Score;
            }
        }

        /// <summary>
        /// Get the best line
        /// </summary>
        /// <returns></returns>
        public CSVLine GetBestLine()
        {
            _lines.Sort();
            if (_lines.Count > 0)
                return _lines[_lines.Count - 1];
            else
                return new CSVLine(Key, 0);
        }

        /// <summary>
        /// Get all the lines being stored in the lines manager
        /// </summary>
        /// <returns></returns>
        public CSVLine[] GetAllLines()
        {
            _lines.Sort();
            return _lines.ToArray();
        }
    }
}
