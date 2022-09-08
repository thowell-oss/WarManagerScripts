
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Unity3D.ObjectPooling
{
    /// <summary>
    /// Efficiently handles the lifecycle of a list of instantiated objects
    /// </summary>
    [Serializable]
    [Notes.Author("Efficiently handles the lifecycle of a list of instantiated objects")]
    public sealed class ObjectPool : IComparable<ObjectPool>, IEnumerable
    {

        /// <summary>
        /// the total amount of objects in the pool
        /// </summary>
        public int Capacity => _activeObjects.Count + _disabledObjects.Count;

        /// <summary>
        /// private backing field
        /// </summary>
        [SerializeField] GameObject _prefab;

        /// <summary>
        /// private backing field for the parent
        /// </summary>
        [SerializeField] Transform _parent;

        /// <summary>
        /// The prefab object
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// The parent object that the prefab will spawn as a child to
        /// </summary>
        public Transform Parent => _parent;

        /// <summary>
        /// the queue of disabled objects
        /// </summary>
        private Queue<GameObject> _disabledObjects = new Queue<GameObject>();

        /// <summary>
        /// the amount of objects in the disabled queue
        /// </summary>
        public int DisabledCount => _disabledObjects.Count;

        /// <summary>
        /// the list of enabled objects
        /// </summary>
        private List<GameObject> _activeObjects = new List<GameObject>();

        /// <summary>
        /// the total number of objects in the active objects queue
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// A reference to the last gameObject that was checked out
        /// </summary>
        public GameObject LastCheckedOutGameObject { get; private set; } = null;

        /// <summary>
        /// instantiate function
        /// </summary>
        Func<GameObject, Transform, GameObject> InstantiateAction;

        /// <summary>
        /// destroy function
        /// </summary>
        Action<GameObject> DestroyAction;

        /// <summary>
        /// Is this object a poolableObject? (decided upon initialization for performance reasons)
        /// </summary>
        /// <value></value>
        private bool _isPoolObject { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prefab">the prefab key (that will be the template for the instantiation)</param>
        /// <param name="parent">the parent that the prefab will spawn under</param>
        /// <param name="startingAmount">the starting amount of objects to create</param>
        /// <param name="instantiateAction">the instantiating function</param>
        /// <param name="destroyAction">the destroy action</param>
        public ObjectPool(GameObject prefab, Transform parent, int startingAmount, Func<GameObject, Transform, GameObject> instantiateAction, Action<GameObject> destroyAction)
        {

            if (prefab == null)
                throw new NullReferenceException("The prefab cannot be null");

            if (parent == null)
                throw new NullReferenceException("The parent cannot be null");

            if (instantiateAction == null)
                throw new NullReferenceException("The instantiate action cannot be null");

            if (destroyAction == null)
                throw new NullReferenceException("The destroy action cannot be null");

            _prefab = prefab;

            _parent = parent;

            InstantiateAction = instantiateAction;
            DestroyAction = destroyAction;

            PoolableObject obj = prefab.GetComponent<PoolableObject>();
            var x = obj.GetComponent<PoolableObject>();

            if (x != null)
                _isPoolObject = true;
            else
            {
                Debug.LogWarning("Warning! You might want to make your prefab object inherit from PoolableObject");
            }

            CreateNewObjects(startingAmount);
        }

        /// <summary>
        /// Create new objects
        /// </summary>
        /// <param name="amount">the amount of objects to create</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown when the parameter is less than or equal to zero</exception>
        private void CreateNewObjects(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            for (int i = 0; i < amount; i++)
            {
                // GameObject obj = Instantiate<GameObject>(Prefab, this.gameObject.transform) as GameObject;
                GameObject obj = InstantiateAction(Prefab, Parent);
                _disabledObjects.Enqueue(obj);

                if (_isPoolObject)
                {
                    PoolableObject pool = obj.GetComponent<PoolableObject>();

                    pool.Parent = Parent;
                    pool.PrefabKey = Prefab;

                }
            }
        }

        /// <summary>
        /// Clear all active objects from the list
        /// </summary>
        public void Clear()
        {
            while (_activeObjects.Count > 0)
            {
                _disabledObjects.Enqueue(_activeObjects[0]);

                if (_activeObjects[0] is GameObject obj)
                    obj.SetActive(false);
                _activeObjects.RemoveAt(0);
            }
        }

        /// <summary>
        /// Remove all the objects from the world and destroy them
        /// </summary>
        public void DestroyAllReferences()
        {
            Clear();
            while (_disabledObjects.Count > 0)
            {
                // Destroy(_disabledObjects.Dequeue());
                DestroyAction(_disabledObjects.Dequeue());
            }
        }

        /// <summary>
        /// Checkout the object to use
        /// </summary>
        /// <param name="enableObject">should the object be enabled when received?</param>
        /// <returns>returns the gameObject</returns>
        public GameObject CheckoutObject(bool enableObject = true)
        {
            if (_activeObjects.Count >= _activeObjects.Count + _disabledObjects.Count / 2)
            {
                CreateNewObjects((_activeObjects.Count + _disabledObjects.Count) * 2);
            }

            GameObject obj = _disabledObjects.Dequeue();
            obj.SetActive(enableObject);

            _activeObjects.Add(obj);

            LastCheckedOutGameObject = obj;

            return obj;
        }

        /// <summary>
        /// Checkout the object to use
        /// </summary>
        /// <param name="enableObject">should the object be enabled when received?</param>
        /// <typeparam name="TComponent">The specific component of the object</typeparam>
        /// <returns>returns the gameObject</returns>
        public TComponent CheckoutObject<TComponent>(bool enableObject = true)
        {
            GameObject obj = CheckoutObject(enableObject);
            var x = obj.GetComponent<TComponent>();

            if (x == null)
                throw new ArgumentNullException("Cannot find the object with the specific component");

            return x;
        }

        /// <summary>
        /// check in the object after use
        /// </summary>
        /// <param name="obj">the object</param>
        /// <exception cref="obj">thrown when the gameObject is null</exception>
        public void CheckInObject(GameObject obj)
        {


            if (obj == null)
                throw new NullReferenceException("the object parameter is null");

            var x = _activeObjects.Find(x => x == obj);

            if (x != null)
            {
                x.SetActive(false);
                _disabledObjects.Enqueue(x);
                _activeObjects.Remove(x);
            }
        }

        void OnDestroy()
        {
            DestroyAllReferences();
        }

        public override string ToString()
        {
            if (Prefab != null)
                return $"Object Pool for {Prefab.name} - Capacity: {Capacity}, Active Count: {ActiveCount}. ";
            else
                return base.ToString();
        }

        public int CompareTo(ObjectPool other)
        {
            return Prefab.GetHashCode().CompareTo(other.Prefab.GetHashCode());
        }

        public IEnumerator GetEnumerator()
        {
            return _activeObjects.GetEnumerator();
        }
    }
}
