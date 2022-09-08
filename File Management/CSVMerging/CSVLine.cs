
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Backend
{

    public class CSVLine : IComparable<CSVLine>
    {
        /// <summary>
        /// The row data
        /// </summary>
        /// <value></value>
        public string[] Row { get; private set; }

        /// <summary>
        /// The score given to the row data
        /// </summary>
        /// <value></value>
        public float Score { get; private set; }


        public CSVLine(string[] rowData, float score)
        {
            Row = rowData;
            Score = score;
        }

        public int CompareTo(CSVLine other)
        {
            if (other == null)
            {
                return 1;
            }

            return Score.CompareTo(other.Score);
        }

        public override string ToString()
        {
            return string.Join(",", Row) + " (" + Score + "%)";
        }
    }
}