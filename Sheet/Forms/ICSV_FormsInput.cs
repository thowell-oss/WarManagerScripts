
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Backend;

namespace WarManager.Forms
{
    /// <summary>
    /// Handles the location where all the csv forms input should be stored (Author: Taylor Howell, Version: 1)
    /// </summary>
    public interface ICSV_FormsInput
    {
        public List<string> GetDataSetIDs();
        public ICSV_FormsInputOptions InputOptions { get; set; }
        public List<Point> ExistingEditLocation { get; set; }

        public DataValue GetDataValue();
    }

    [Flags]
    public enum ICSV_FormsInputOptions
    {
        NewEntry,
        ExistingEntry
    }
}
