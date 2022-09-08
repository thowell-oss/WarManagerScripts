using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Unity3D;
using WarManager.Unity3D.ObjectPooling;

namespace WarManager.Testing
{
    /// <summary>
    /// The object pool testing class
    /// </summary>
    public class ObjectPoolTester : MonoBehaviour
    {
        private List<GameObject> objects = new List<GameObject>();

        public GameObject Prefab1;

        PoolManager Manager;


        [SerializeField] bool _addPrefab;
        [SerializeField] bool _checkout;

        [SerializeField] bool _checkin;

        [SerializeField] bool _clear;


        void Start() { Manager = GetComponent<PoolManager>(); }

        void Update()
        {

            if (_addPrefab)
            {
                _addPrefab = false;
                AddPrefab();
            }

            if (_checkout)
            {
                _checkout = false;
                CheckoutPrefab();
            }

            if (_checkin)
            {
                _checkin = false;
                CheckinObject();
            }

            if(_clear)
            {
                _clear = false;
                ClearPool();
            }
        }

        void AddPrefab()
        {
           Manager.RegisterPrefab(Prefab1, 5);
        }

        void CheckoutPrefab()
        {
            GameObject obj = Manager.CheckOutObject(Prefab1, true);
            obj.transform.position = GetRandom(0, 10);
            obj.transform.rotation = Quaternion.Euler(GetRandom(0, 180));
            objects.Add(obj);
        }

        Vector3 GetRandom(float min, float max)
        {
            return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
        }


        void CheckinObject()
        {
            if (objects.Count > 0)
            {
                Manager.TryCheckInObject(Prefab1, objects[0]);
                objects.RemoveAt(0);
            }
        }

        void ClearPool()
        {
            Manager.TryClearObjectPool(Prefab1);
        }
    }
}
