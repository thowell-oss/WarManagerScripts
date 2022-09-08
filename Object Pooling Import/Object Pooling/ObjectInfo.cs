using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using System.Linq;

namespace WarManager.Unity3D.ObjectPooling
{
    /// <summary>
    /// Handles information related to the prefab key
    /// </summary>
    [Serializable]
    public sealed class ObjectInfo : IComparable<ObjectInfo>
    {
        /// <summary>
        /// The Prefab
        /// </summary>
        [ValueDropdown("GetAllPossiblePrefabs")]
        public GameObject Prefab;

        [TabGroup("Internal")]
        /// <summary>
        /// The Starting spawn amount (this needs to be higher if there are a lot of items that will spawn quickly)
        /// </summary>
        public int StartingCapacity = 10;

        [TabGroup("Internal")]
        /// <summary>
        /// The rarity the item will spawn
        /// </summary>
        [Range(0, 1)]
        public float SpawnRarity = .5f;


        [TabGroup("Spawning")]
        /// <summary>
        /// Minumum random rotation
        /// </summary>
        /// <returns></returns>
        public Vector3 RandomRotationMin = new Vector3(0, -45, 0);

        /// <summary>
        /// Maximum random rotation
        /// </summary>
        /// <returns></returns>
        [TabGroup("Spawning")]
        public Vector3 RandomRotationMax = new Vector3(0, 45, 0);

        [Space]
        /// <summary>
        /// Minumum random rotation
        /// </summary>
        /// <returns></returns>
        [TabGroup("Spawning")]
        public Vector3 RandomScaleMin = new Vector3(.1f, .1f, .1f);

        /// <summary>
        /// Maximum random rotation
        /// </summary>
        /// <returns></returns>
        [TabGroup("Spawning")]
        public Vector3 RandomScaleMax = new Vector3(.125f, .125f, .125f);

        [Space]

        /// <summary>
        /// Minumum random offset
        /// </summary>
        /// <returns></returns>
        [TabGroup("Spawning")]
        public Vector3 RandomOffsetMin = new Vector3(0, 0, 0);

        /// <summary>
        /// Maximum random offset
        /// </summary>
        /// <returns></returns>
        [TabGroup("Spawning")]
        public Vector3 RandomOffsetMax = new Vector3(0, 0, 0);

        public int CompareTo(ObjectInfo other)
        {
            if (other == null || other.Prefab == null)
            {
                return 1;
            }

            return Prefab.name.CompareTo(other.Prefab.name);
        }

        public override string ToString()
        {
            return Prefab.name + " (" + SpawnRarity * 100 + "% drop rate).";
        }

#if UNITY_EDITOR

        private static IEnumerable GetAllPossiblePrefabs()
        {
            return AssetDatabase.FindAssets("t:Prefab")
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .Select(x => new ValueDropdownItem(
                x,
                AssetDatabase.LoadAssetAtPath<GameObject>(x)
            ));
        }

#endif

        /// <summary>
        /// Get the random rotation within the rotation bounds already set
        /// </summary>
        /// <returns>returns a random vector3</returns>
        public Quaternion GetRandomRotation()
        {
            float x = UnityEngine.Random.Range(RandomRotationMin.x, RandomRotationMax.x);
            float y = UnityEngine.Random.Range(RandomRotationMin.y, RandomRotationMax.y);
            float z = UnityEngine.Random.Range(RandomRotationMin.z, RandomRotationMax.z);

            return Quaternion.Euler(new Vector3(x, y, z));
        }

        /// <summary>
        /// Get the random scale within the random scale bounds already set
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomScale()
        {

            float x = UnityEngine.Random.Range(RandomScaleMin.x, RandomScaleMax.x);
            float y = UnityEngine.Random.Range(RandomScaleMin.y, RandomScaleMax.y);
            float z = UnityEngine.Random.Range(RandomScaleMin.z, RandomScaleMax.z);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Get the random scale within the random scale bounds already set
        /// </summary>
        /// <returns></returns>
        public Vector3 GetRandomOffset()
        {

            float x = UnityEngine.Random.Range(RandomOffsetMin.x, RandomOffsetMax.x);
            float y = UnityEngine.Random.Range(RandomOffsetMin.y, RandomOffsetMax.y);
            float z = UnityEngine.Random.Range(RandomOffsetMin.z, RandomOffsetMax.z);

            return new Vector3(x, y, z);
        }
    }
}