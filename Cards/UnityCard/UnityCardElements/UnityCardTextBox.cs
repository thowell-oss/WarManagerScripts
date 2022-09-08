using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using WarManager.Backend.CardsElementData;
using WarManager.Backend;
using WarManager;

namespace WarManager.Cards.Elements
{

    public class UnityCardTextBox : PoolObject, IUnityCardElement
    {
        /// <summary>
        /// Is this object active in the scene?
        /// </summary>
        /// <value></value>
        public bool Active
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        public bool Critical { get; set; }

        [SerializeField] private int _ID;
        [SerializeField] private string _elementTag = "text box";

        /// <summary>
        /// The type of element being displayed
        /// </summary>
        /// <value></value>
        public string ElementTag
        {
            get
            {
                return _elementTag;
            }
            set
            {
                _elementTag = value;
            }
        }

        /// <summary>
        /// The card element id
        /// </summary>
        /// <value></value>
        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }


        public CardElementViewData ElementViewData { get; private set; }

        /// <summary>
        /// scale 
        /// </summary>
        public Vector2 _scale;

        /// <summary>
        /// rotation
        /// </summary>
        private Vector2 _rotation;

        /// <summary>
        /// The location of the object
        /// </summary>
        /// <returns></returns>
        public Vector3 Location = new Vector3(0, 0, 0);


        public void Refresh()
        {

        }

        public static bool TrySetProperties(UnityCardTextBox box, CardElementTextData text, DataEntry entry)
        {
            box.ElementViewData = text;
            return true;
        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {
            if (!Critical)
                Active = false;
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardElementTextData d)
            {
                return TrySetProperties(this, d, entry);
            }

            return false;
        }
    }
}
