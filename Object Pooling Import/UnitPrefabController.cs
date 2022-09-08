using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Sirenix;
using Sirenix.OdinInspector;

using WarManager;

namespace WarManager.Unity3D.ObjectPooling
{

    public class UnitPrefabController : MonoBehaviour
    {
        public ObjectPoolGroup PoolGroup;

        [TabGroup("Editing")]
        [SerializeField] bool _testSpawn = false;

        [TabGroup("Editing")]
        [SerializeField] bool _checkinObjects;

        [TabGroup("Details")]
        public bool GetRotationFromSpawnLocation;
        [TabGroup("Details")]
        public bool UseSameObjectForAllLocations;
        [TabGroup("Details")]
        public List<Transform> ItemSpawnLocations = new List<Transform>();

        private List<(GameObject key, GameObject obj)> SpawnedItems = new List<(GameObject, GameObject)>();

        private bool started = false;

        public void Start() => CheckinGroups();

        public void CheckinGroups()
        {
            throw new System.NotImplementedException("Not using groups in War Manager at this time");
        }

        void Update()
        {
            if (started)
            {
                started = false;
                SpawnItems();
            }

            if (_testSpawn)
            {
                _testSpawn = false;
                SpawnItems();
            }

            if (_checkinObjects)
            {
                _checkinObjects = false;

                CheckinGroups();
            }
        }

        /// <summary>
        /// Spawn the items
        /// </summary>
        public void SpawnItems()
        {
            ClearItems();

            if (UseSameObjectForAllLocations)
            {
                ObjectInfo objectInfo = PoolGroup.SelectRandomObjectByRarity();

                for (int i = 0; i < ItemSpawnLocations.Count; i++)
                {
                    PlaceItem(objectInfo, ItemSpawnLocations[i]);
                }
            }
            else
            {

                for (int i = 0; i < ItemSpawnLocations.Count; i++)
                {
                    ObjectInfo objectInfo = PoolGroup.SelectRandomObjectByRarity();
                    PlaceItem(objectInfo, ItemSpawnLocations[i]);
                }
            }
        }


        /// <summary>
        /// Place the item in the world
        /// </summary>
        /// <param name="objectInfo"></param>
        /// <param name="spawnLocation"></param>
        private void PlaceItem(ObjectInfo objectInfo, Transform spawnLocation)
        {
            // if (objectInfo != null && objectInfo.Prefab != null && spawnLocation != null)
            // {
            //     WorldManager.PoolManager.TryCheckoutObject(objectInfo.Prefab, out var obj);

            //     if (obj != null)
            //     {
            //         SpawnedItems.Add((objectInfo.Prefab, obj));
            //         obj.transform.position = spawnLocation.position;
            //         obj.transform.parent = spawnLocation.transform;

            //         if (!GetRotationFromSpawnLocation)
            //             obj.transform.rotation = objectInfo.GetRandomRotation();
            //         else obj.transform.rotation = spawnLocation.transform.rotation;

            //         obj.transform.localScale = objectInfo.GetRandomScale();
            //         obj.transform.position += objectInfo.GetRandomOffset();
            //     }
            // }

            throw new System.NotImplementedException("Not using units at this time");
        }


        /// <summary>
        /// clear all visible items
        /// </summary>
        private void ClearItems()
        {
            for (int i = 0; i < SpawnedItems.Count; i++)
            {
                WorldManager.PoolManager.TryCheckInObject(SpawnedItems[i].key, SpawnedItems[i].obj);
            }
        }

        private void OnEnable()
        {
            started = true;
        }
    }
}
