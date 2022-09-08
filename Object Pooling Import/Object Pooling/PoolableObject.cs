using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// Handles some functionality for the pool objects, making pool objects a little bit easier to work with
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour
    {
        /// <summary>
        /// the Prefab Key
        /// </summary>
        /// <value></value>
        public GameObject PrefabKey { get; set; }

        /// <summary>
        /// The Parent
        /// </summary>
        /// <value></value>
        public Transform Parent { get; set; }

        void Start() => StartCoroutine(CheckVisibility());

        IEnumerator CheckVisibility()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(5);

                if (!gameObject.activeInHierarchy)
                    CheckIn();
            }
        }

        void OnDisable() // make sure the objects actually get cleaned up
        {
            CheckIn();
        }

        /// <summary>
        /// Check in the object back into the pool manager
        /// </summary>
        protected virtual void CheckIn()
        {
            // if (PrefabKey == null)
            //     throw new System.Exception("Something went wrong when instantiating these objects");

            if (PrefabKey != null)
            {

                if (Parent != null)
                    PoolManager.Main.TryCheckInObject(PrefabKey, this.gameObject, Parent);
                else
                    PoolManager.Main.TryCheckInObject(PrefabKey, this.gameObject);
            }
        }
    }
}
