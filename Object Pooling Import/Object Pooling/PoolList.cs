using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using WarManager.Backend;
using WarManager.Cards;

using WarManager.Unity3D.ObjectPooling;
using WarManager.Unity3D;

namespace WarManager
{
    /// <summary>
    /// Simplify the object pooling system - handles some of the boiler plate code
    /// </summary>
    /// <typeparam name="T">the poolable object</typeparam>
    [Notes.Author("Simplify the object pooling system")]
    public class PoolList<T> : IEnumerable<T> where T : PoolableObject
    {
        /// <summary>
        /// the prefab key
        /// </summary>
        private T _prefabKey;

        /// <summary>
        /// the parent
        /// </summary>
        private Transform _parent;

        /// <summary>
        /// The list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<T> _activeObj = new List<T>();



        /// <summary>
        /// iterate through the list of pool objects        
        /// </summary>
        /// <value></value>
        public IEnumerable<T> PoolObjects
        {
            get => _activeObj;
        }

        /// <summary>
        /// the count of how many active objects there are in the list
        /// </summary>
        public int Count => _activeObj.Count;

        public T this[int index] { get => _activeObj[index]; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabKey">the object key</param>
        /// <param name="parent">the parent</param>
        public PoolList(T prefabKey, Transform parent)
        {
            _prefabKey = prefabKey;
            _parent = parent;
            if (!PoolManager.Main.IsRegistered(prefabKey.gameObject, parent))
                PoolManager.Main.RegisterPrefab(prefabKey.gameObject, 5, parent);
        }

        /// <summary>
        /// Clear the list of active objects
        /// </summary>
        public void Clear()
        {
            foreach (var x in _activeObj)
            {
                bool y = PoolManager.Main.TryCheckInObject(_prefabKey.gameObject, x.gameObject, _parent);
            }

            _activeObj.Clear();

            Debug.Log("clearing");
        }

        /// <summary>
        /// Check that enough elements have been spawned and are ready to be placed
        /// </summary>
        /// <param name="amt"></param>
        public void CheckAmount(int amt)
        {
            if (amt > Count)
            {
                Debug.Log("added " + (amt - Count) + " objects");
                AddMoreActiveObjects(amt - Count);
            }
            else if (amt < Count)
            {
                Debug.Log("removed " + (Count - amt) + " objects");
                RemoveActiveObjects(Count - amt);
            }
        }

        /// <summary>
        /// Get more objects to be active in the list of active objects
        /// </summary>
        /// <param name="amt"></param>
        public void AddMoreActiveObjects(int amt)
        {
            if (amt <= 0)
                return;

            for (int i = 0; i < amt; i++)
            {
                var obj = PoolManager.Main.CheckOutGameObject<T>(_prefabKey.gameObject, false, _parent);
                obj.gameObject.SetActive(true);
                _activeObj.Add(obj);
            }
        }


        public void RemoveActiveObjects(int amt)
        {
            if (amt <= 0)
                return;

            if (amt >= _activeObj.Count)
                Clear();

            var finalAmount = _activeObj.Count - amt;

            for (int i = _activeObj.Count - 1; i >= finalAmount; i--)
            {
                if (PoolManager.Main.TryCheckInObject(_prefabKey.gameObject, _activeObj[i].gameObject, _parent))
                    _activeObj.RemoveAt(i);
            }
        }

        /// <summary>
        /// Get an array of all objects in the list of active objects
        /// </summary>
        /// <returns>returns the array</returns>
        public T[] ToArray()
        {
            return _activeObj.ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _activeObj.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _activeObj.GetEnumerator();
        }
    }
}