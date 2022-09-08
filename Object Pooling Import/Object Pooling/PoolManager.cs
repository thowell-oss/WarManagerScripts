
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix;
using Sirenix.Serialization;

using WarManager.Unity3D.ObjectPooling;

namespace WarManager
{
    /// <summary>
    /// handles a series of object pools
    /// </summary>
    [Notes.Author("handles a series of object pools")]
    public sealed class PoolManager : MonoBehaviour
    {
        /// <summary>
        /// The dictionary of object pools
        /// </summary>
        private Dictionary<(GameObject, Transform), ObjectPool> _objectPools = new Dictionary<(GameObject, Transform), ObjectPool>();

        /// <summary>
        /// The list of the keys
        /// </summary>
        /// <typeparam name="GameObject"></typeparam>
        /// <returns></returns>
        private List<(GameObject prefab, Transform parent)> _poolKeys = new List<(GameObject, Transform)>();

        /// <summary>
        /// The default parent of all instantiated objects
        /// </summary>
        [SerializeField] private Transform _defaultParent;

        /// <summary>
        /// the count of object pools
        /// </summary>
        public int Count => _objectPools.Count;

        #region singleton
        /// <summary>
        /// singleton pattern
        /// </summary>
        public static PoolManager Main;


        /// <summary>
        /// called by monoBehavior
        /// </summary>
        void Awake()
        {
            if (Main != null)
            {
                Destroy(this);
            }
            else
            {
                Main = this;
            }
        }

        #endregion

        /// <summary>
        /// called by monoBehavior
        /// </summary>
        void Start()
        {
            if (_defaultParent == null)
                _defaultParent = this.transform;

        }

        /// <summary>
        /// Add new prefab to the list of prefabs. The parent is the parent selected in the inspector. See "_defaultParent".
        /// </summary>
        /// <param name="prefab">the prefab to use</param>
        /// <param name="startingCapacity">the starting capacity of the objects (the amount of objects created initially)</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">thrown when starting capacity is less than or equal to zero</exception>
        /// <exception cref="System.ArgumentException">thrown when the instantiated transform parent is null</exception>
        public void RegisterPrefab(GameObject prefab, int startingCapacity)
        {
            RegisterPrefab(prefab, startingCapacity, _defaultParent);
        }

        /// <summary>
        /// Add new prefab to the list of prefabs
        /// </summary>
        /// <param name="prefab">the prefab to use</param>
        /// <param name="startingCapacity">the starting capacity of the objects (the amount of objects created initially)</param>
        /// <param name="parent">the parent of the registered prefab</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab is null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">thrown when starting capacity is less than or equal to zero</exception>
        /// <exception cref="System.ArgumentException">thrown when the instantiated transform parent is null</exception>
        public void RegisterPrefab(GameObject prefab, int startingCapacity, Transform parent)
        {
            if (prefab == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (startingCapacity <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(startingCapacity));

            // if (_defaultParent == null)
            //     throw new System.ArgumentException("the instantiated parent is not set");

            if (_poolKeys.Contains((prefab, parent))) throw new System.ArgumentException("The prefab is already in the pool dictionary");

            if (parent == null)
                throw new NullReferenceException("The parent cannot be null");

            ObjectPool newPool = new ObjectPool(prefab, parent, startingCapacity, InstantiateObject_Func, DestroyGameObject_Action);

            var tuple = (prefab, parent);

            _objectPools.Add(tuple, newPool);

            _poolKeys.Add((prefab, parent));
        }


        /// <summary>
        /// Checkout a gameObject, even if it is not registered. 
        /// (If the object is not registered, then it will be created). The parent is the default parent
        /// </summary>
        /// <param name="prefabKey">the prefab</param>
        /// <param name="enabled">should the gameObject be enabled?</param>
        /// <returns>returns the game object</returns>
        public GameObject CheckOutObject(GameObject prefabKey, bool enabled)
        {
            return CheckOutObject(prefabKey, enabled, _defaultParent);
        }


        /// <summary>
        /// Checkout a gameObject, even if it is not registered. 
        /// (If the object is not registered, then it will be created)
        /// </summary>
        /// <param name="prefabKey">the prefab</param>
        /// <param name="enabled">should the gameObject be enabled?</param>
        /// <param name="parent">the transform parent</param>
        /// <returns>returns the game object</returns>
        public GameObject CheckOutObject(GameObject prefabKey, bool enabled, Transform parent)
        {
            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (parent == null) throw new NullReferenceException("the parent cannot be null");


            if (_objectPools.TryGetValue((prefabKey, parent), out var x))
            {
                return x.CheckoutObject(enabled);
            }
            else
            {
                RegisterPrefab(prefabKey, 5, parent);

                return _objectPools[(prefabKey, parent)].CheckoutObject(enabled);
            }
        }

        /// <summary>
        /// Checkout a gameObject with a specific component, regardless if the gameObject is registered or not. 
        /// (If the object is not registered, then it will be created). The parent is the default parent. 
        /// </summary>
        /// <param name="prefabKey">the prefab key</param>
        /// <param name="enabled">should the object be enabled when it is created?</param>
        /// <param name="parent">the parent that the prefab will spawn as a child under</param>
        /// <typeparam name="T">the type of component to get</typeparam>
        /// <returns>returns the gameObject parsed to the component</returns>
        public T CheckOutGameObject<T>(GameObject prefabKey, bool enabled)
        {
            return CheckOutGameObject<T>(prefabKey, enabled, _defaultParent);
        }

        /// <summary>
        /// Checkout a gameObject with a specific component, regardless if the gameObject is registered or not. (If the object is not registered, then it will be created)
        /// </summary>
        /// <param name="prefabKey">the prefab key</param>
        /// <param name="enabled">should the object be enabled when it is created?</param>
        /// <param name="parent">the parent that the prefab will spawn as a child under</param>
        /// <typeparam name="T">the type of component to get</typeparam>
        /// <returns>returns the gameObject parsed to the component</returns>
        public T CheckOutGameObject<T>(GameObject prefabKey, bool enabled, Transform parent)
        {
            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (parent == null)
                throw new NullReferenceException("The parent cannot be null");

            if (_objectPools.TryGetValue((prefabKey, parent), out var x))
            {
                return x.CheckoutObject<T>(enabled);
            }
            else
            {
                RegisterPrefab(prefabKey, 5, parent);
                return _objectPools[(prefabKey, parent)].CheckoutObject<T>(enabled);
            }
        }

        /// <summary>
        /// Check in the object after use. The parent is the default parent
        /// </summary>
        /// <param name="prefabKey">the prefab object</param>
        /// <param name="obj">the object</param>
        /// <param name="parent">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryCheckInObject(GameObject prefabKey, GameObject obj)
        {
            return TryCheckInObject(prefabKey, obj, _defaultParent);
        }

        /// <summary>
        /// Check in the object after use
        /// </summary>
        /// <param name="prefabKey">the prefab object</param>
        /// <param name="obj">the object</param>
        /// <param name="parent">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryCheckInObject(GameObject prefabKey, GameObject obj, Transform parent)
        {

            //Debug.Log("attempting to check in object");

            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (obj == null)
                throw new System.NullReferenceException("the obj cannot be null");

            if (_objectPools.TryGetValue((prefabKey, parent), out var x))
            {
                x.CheckInObject(obj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check in the object after use
        /// </summary>
        /// <param name="prefabKey">the prefab object</param>
        /// <param name="objects">the IEnumerable set of objects</param>
        /// <param name="parent">the parent</param>
        /// <returns>returns true iff all objects are successfully checked in, false if not</returns>
        public bool CheckInObjects<T>(GameObject prefabKey, IEnumerable<T> objects, Transform parent)
        {

            bool success = true;

            foreach (var x in objects)
            {
                if (x != null && x is GameObject obj)
                    if (!TryCheckInObject(prefabKey, obj, parent))
                    {
                        success = false;
                    }
            }

            return success;
        }

        /// <summary>
        /// Clear the object pool. The parent is the default parent
        /// </summary>
        ///<param name="prefabKey">the prefab</param>
        /// <param name="parent">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryClearObjectPool(GameObject prefabKey)
        {
            return TryClearObjectPool(prefabKey, _defaultParent);
        }

        /// <summary>
        /// Clear the object pool
        /// </summary>
        ///<param name="prefabKey">the prefab</param>
        /// <param name="parent">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryClearObjectPool(GameObject prefabKey, Transform parent)
        {
            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (_objectPools.TryGetValue((prefabKey, parent), out var x))
            {
                x.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// clear the object pool and remove it from the dictionary. The parent is the default parent
        /// </summary>
        ///<param name="prefabKey">the prefab</param>
        ///<param name="prefabKey">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryDestroyObjectPool(GameObject prefabKey)
        {
            return TryDestroyObjectPool(prefabKey, _defaultParent);
        }

        /// <summary>
        /// clear the object pool and remove it from the dictionary
        /// </summary>
        ///<param name="prefabKey">the prefab</param>
        ///<param name="prefabKey">the parent</param>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public bool TryDestroyObjectPool(GameObject prefabKey, Transform parent)
        {
            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (!TryClearObjectPool(prefabKey, parent))
                return false;

            if (_objectPools.ContainsKey((prefabKey, parent)))
            {
                _poolKeys.Remove((prefabKey, parent));
                return _objectPools.Remove((prefabKey, parent));
            }

            return false;
        }

        /// <summary>
        /// Get the count of the object pool active list. The parent is the default parent
        /// </summary>
        /// <param name="prefabKey">the prefab key</param>
        /// <param name="parent">the parent the prefab is spawned as a child under</param>
        /// <returns>returns an integer of the active list count, -1 if it is not found</returns>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public int GetObjectPoolActiveCount(GameObject prefabKey)
        {
            return GetObjectPoolActiveCount(prefabKey, _defaultParent);
        }

        /// <summary>
        /// Get the count of the object pool active list
        /// </summary>
        /// <param name="prefabKey">the prefab key</param>
        /// <param name="parent">the parent the prefab is spawned as a child under</param>
        /// <returns>returns an integer of the active list count, -1 if it is not found</returns>
        /// <exception cref="System.NullReferenceException">thrown when the prefab key is null</exception>
        public int GetObjectPoolActiveCount(GameObject prefabKey, Transform parent)
        {
            if (prefabKey == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            if (_objectPools.TryGetValue((prefabKey, parent), out var x))
            {
                return x.ActiveCount;
            }

            return -1;
        }

        /// <summary>
        /// Get all the pool keys
        /// </summary>
        /// <returns>returns an array of gameObject pool keys</returns>
        public (GameObject key, Transform parent)[] GetAllKeys()
        {
            return _poolKeys.ToArray();
        }

        /// <summary>
        /// Is the prefab registered?
        /// </summary>
        /// <param name="prefabKey">the prefab in question</param>
        /// <returns>returns true if the key is found, false if not</returns>
        public bool IsRegistered(GameObject prefabKey, Transform parent)
        {
            return _poolKeys.Find(x => x.prefab == prefabKey && x.parent == parent).prefab != null;
        }

        #region for the object pools

        /// <summary>
        /// Instantiate the object
        /// </summary>
        /// <param name="prefab">the prefab object</param>
        /// <returns>returns the instantiated object</returns>
        private GameObject InstantiateObject_Func(GameObject prefab, Transform parent)
        {
            if (prefab == null)
                throw new System.NullReferenceException("the prefab cannot be null");

            var x = Instantiate(prefab, parent) as GameObject;
            x.SetActive(false);
            return x;
        }

        /// <summary>
        /// Destroy the object
        /// </summary>
        /// <param name="obj">the object</param>
        private void DestroyGameObject_Action(GameObject obj)
        {
            if (obj == null)
                return;

            Destroy(obj);

        }
        #endregion

        #region lifecycle



        /// <summary>
        /// Calling disable disables all pooling objects
        /// </summary>
        void OnDisable()
        {
            try
            {
                foreach (var x in _poolKeys)
                {
                    TryClearObjectPool(x.prefab, x.parent);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        #endregion
    }
}

