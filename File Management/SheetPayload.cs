
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Sharing.Security;
using WarManager.Backend;

namespace WarManager.Sharing
{
    public class SheetPayload<Tobj> where Tobj : ICompareWarManagerPoint, IFileContentInfo
    {
        /// <summary>
        /// Pre-built sheet
        /// </summary>
        /// <value></value>
        public Sheet<Tobj> Sheet { get; private set; }


        /// <summary>
        /// Sheet card data
        /// </summary>
        /// <value></value>
        public List<string[]> Data { get; private set; }


        /// <summary>
        /// Creates an immutable sheet payload
        /// </summary>
        /// <param name="sheet">the sheet</param>
        /// <param name="data">the card data for the sheet</param>
        public SheetPayload(Sheet<Tobj> sheet, List<string[]> data)
        {
            if (sheet == null)
                throw new NullReferenceException("The sheet data cannot be null");

            if (data == null)
                throw new NullReferenceException("The data cannot be null");

            Sheet = sheet;
            Data = data;
        }
    }
}
