
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Backend
{

    public class CSVLineAnalyzer
    {

        CSVMergeHandler _handler;

        public List<int> ColumnAnchors { get; private set; } = new List<int>();

        /// <summary>
        /// csv line analyzer constrctor
        /// </summary>
        /// <param name="handler"></param>
        public CSVLineAnalyzer(CSVMergeHandler handler, List<int> columnAnchors)
        {
            if (handler == null)
                throw new NullReferenceException("the csv merge handler cannot be null");

            if (columnAnchors == null)
            {
                throw new NullReferenceException("the column anchors cannot be null or empty");
            }

            if (columnAnchors.Count < 1)
                throw new Exception("the column anchor count cannot be less than 1");


            if (ColumnAnchors.Count > 1)
                ColumnAnchors.Sort((x, y) =>
                {
                    return x.CompareTo(y);
                });

            for (int i = columnAnchors.Count - 1; i >= 0; i--)
            {

                int j = 0;

                int len = handler.OldFile.Header.Length;

                if (len > handler.NewFile.Header.Length)
                    len = handler.NewFile.Header.Length;

                bool removed = false;

                while (j < len - 1 && !removed)
                {

                    // Console.WriteLine(j + ") " + string.Join(",", handler.OldFile[j]) + " " + handler.OldFile[i].Length);
                    // Console.WriteLine(j + ") " + string.Join(",", handler.NewFile[j]) + " " + handler.NewFile[i].Length);
                    // // Console.WriteLine("");

                    // if (handler.OldFile[j].Length > 0 && handler.NewFile[j].Length > 0)
                    // {

                    //     if (columnAnchors[i] >= handler.OldFile[j].Length || columnAnchors[i] >= handler.NewFile[j].Length || columnAnchors[i] < 0)
                    //     {
                    //         columnAnchors.RemoveAt(i);
                    //         removed = true;

                    //         Console.WriteLine("\t\tremoved anchor at " + j + " location");
                    //     }
                    // }

                    j++;
                }
            }

            if (columnAnchors.Count > 0)
            {
                _handler = handler;
                ColumnAnchors = columnAnchors;
            }
            else
            {
                throw new Exception("There are no column anchors within both bounds of each csv file");
            }
        }


        /// <summary>
        /// Compares the two columns for 
        /// </summary>
        /// <param name="aColumns">a column</param>
        /// <param name="bColumns">b column</param>
        /// <returns>returns true if the column syntax is similar enough</returns>
        public static bool CheckColumnSyntax(string[] aColumns, string[] bColumns)
        {

            Debug.Log(aColumns.Length + " " + bColumns.Length);

            if (aColumns.Length != bColumns.Length)
            {
                return false;
            }

            for (int i = 0; i < aColumns.Length; i++)
            {
                float score = GetTokenRatio(aColumns[i], bColumns[i]);

                Debug.Log(i + ") " + aColumns[i] + " " + bColumns[i] + " (" + score + ")");

                if (score < 85)
                {
                    return false;
                }
            }

            return true;
        }

        public static float GetTokenRatio(string aString, string bString)
        {
            bool acheck = string.IsNullOrWhiteSpace(aString);
            bool bcheck = string.IsNullOrWhiteSpace(bString);

            if ((acheck || bcheck))
            {
                return 0;
            }

            string[] a = aString.Split(' ');
            string[] b = bString.Split(' ');

            List<float> scores = new List<float>();

            a = CleanStringList(a);
            b = CleanStringList(b);


            //find any unions
            //find remainder a
            //find remainder b

            List<string> unionList = new List<string>();
            List<string> aRemainderList = new List<string>();
            List<string> bRemainderList = new List<string>();

            for (int i = 0; i < a.Length; i++)
            {
                int j = 0;
                bool found = false;

                while (j < b.Length && !found)
                {
                    if (a[i] == b[j])
                    {
                        unionList.Add(a[i]);
                        found = true;
                    }

                    j++;
                }

                if (!found)
                {
                    aRemainderList.Add(a[i]);
                }
            }

            for (int i = 0; i < b.Length; i++)
            {
                int j = 0;
                bool found = false;

                while (j < a.Length && !found)
                {
                    if (b[i] == a[j])
                    {
                        // union = union + " " + b[i];
                        found = true;
                    }

                    j++;
                }

                if (!found)
                {
                    bRemainderList.Add(b[i]);
                }
            }

            List<float> ratios = new List<float>();

            string union = "";
            string aRemainder = "";
            string bRemainder = "";

            unionList.Sort();
            aRemainderList.Sort();
            bRemainderList.Sort();

            foreach (var x in unionList)
            {
                union += " " + x;
            }

            foreach (var y in aRemainderList)
            {
                aRemainder += " " + y;
            }

            foreach (var z in bRemainderList)
            {
                bRemainder += " " + z;
            }

            // Console.WriteLine("Union: " + union);
            // Console.WriteLine("Remainder A " + aRemainder);
            // Console.WriteLine("Remainder B " + bRemainder);

            if (string.IsNullOrWhiteSpace(aRemainder) && string.IsNullOrWhiteSpace(bRemainder))
                return 100;

            //if (aRemainder.Length > 0)
            ratios.Add(GetRatio(union, union + " " + aRemainder));

            //if (bRemainder.Length > 0)
            ratios.Add(GetRatio(union, union + " " + bRemainder));

            //if (bRemainder.Length > 0 && aRemainder.Length > 0)
            ratios.Add(GetRatio(union + " " + aRemainder, union + " " + bRemainder));

            foreach (var x in ratios)
            {
                // Console.WriteLine("Result: " + x);
            }

            // Console.WriteLine();

            ratios.Sort();

            foreach (var x in ratios)
            {
                //Console.WriteLine("Result: " + x);
            }

            if (ratios.Count > 0)
                return ratios[ratios.Count - 1];
            else
                return 0;
        }

        private static float GetRatio(string a, string b)
        {

            int lenA = a.Length;
            int lenB = b.Length;

            int match = 0;

            int matchLength = lenA;

            if (matchLength > lenB)
            {
                matchLength = lenB;
            }

            //Console.WriteLine("a " + lenA + " b " + lenB);

            for (int i = 0; i < matchLength; i++)
            {
                if (a[i] == b[i])
                {
                    match++;
                }
                else
                {
                    break;
                }
            }

            // Console.WriteLine("a " + lenA + " b " + lenB + " m " + match);

            float final = (2 * ((float)match / (float)(lenA + lenB)) * 100);

            //Console.WriteLine(final);

            return final;
        }

        private static string[] CleanStringList(string[] str)
        {
            List<string> stringList = new List<string>();
            stringList.AddRange(str);

            for (int i = stringList.Count - 1; i >= 0; i--)
            {
                if (isValidString(stringList[i]))
                {
                    stringList[i].Trim();
                }
                else
                {
                    stringList.RemoveAt(i);
                }
            }

            return stringList.ToArray();
        }

        private static bool isValidString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            return true;
        }
    }
}