using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;

namespace WarManager.Backend
{
    /// <summary>
    /// Utility commands for the data entry
    /// </summary>
    [Notes.Author("Utility commands for the data entry")]
    public class DataEntryUtility
    {
        /// <summary>
        /// Get the list of data entries from a list of cards
        /// </summary>
        /// <param name="cards">the list of cards</param>
        /// <returns>returns a list of data entries</returns>
        public List<DataEntry> GetEntriesFromCards(List<Card> cards)
        {
            List<DataEntry> entries = new List<DataEntry>();

            foreach (var x in cards)
            {
                if (x != null)
                {
                    entries.Add(x.Entry);
                }
            }

            return entries;
        }

        /// <summary>
        /// Gets data values with the same value and value type, and puts them together
        /// </summary>
        /// <param name="entries">the data entries</param>
        /// <returns>returns the list</returns>
        public List<DataValue> GetSimilarDataValuesFromEntries(List<DataEntry> entries)
        {
            List<int> skip = new List<int>();
            List<DataValue> values = new List<DataValue>();

            for (int i = 0; i < entries.Count; i++)
            {
                for (int k = 0; k < entries.Count; k++)
                {
                    if (i != k)
                    {
                        var firstData = entries[i].GetAllowedDataValues();
                        var secondData = entries[k].GetAllowedDataValues();

                        foreach (var x in firstData)
                        {
                            foreach (var y in secondData)
                            {
                                if (x.ValueType == y.ValueType && x.Value == y.Value)
                                {
                                    values.Add(x);
                                }
                                else
                                {
                                    skip.Add(i);
                                }
                            }
                        }
                    }
                }
            }

            return values;
        }
    }
}
