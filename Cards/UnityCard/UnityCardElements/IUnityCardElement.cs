using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    public interface IUnityCardElement : IComparable<IUnityCardElement>
    {
        /// <summary>
        /// The id of the card element
        /// </summary>
        /// <value></value>
        public int ID { get; }

        /// <summary>
        /// The type of element the object is
        /// </summary>
        /// <value></value>
        public string ElementTag { get; }

        /// <summary>
        /// Dictates wether the element can be seen or not
        /// </summary>
        /// <value></value>
        public bool Active { get; set; }

        /// <summary>
        /// Does this element need to be shown at all times?
        /// </summary>
        /// <value></value>
        public bool Critical { get; set; }

        /// <summary>
        /// The Card Element Data
        /// </summary>
        public CardElementViewData ElementViewData { get; }

        /// <summary>
        /// Disable the nonessential elements
        /// </summary>
        void DisableNonEssential();

        /// <summary>
        /// Set the element properties
        /// </summary>
        bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData;

    }
}