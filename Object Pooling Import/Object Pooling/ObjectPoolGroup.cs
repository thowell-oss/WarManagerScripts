
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace WarManager.Unity3D.ObjectPooling
{
    /// <summary>
    /// Handles groups of objects to pool for biomes
    /// </summary>
    [CreateAssetMenu(fileName = "New Object Pool Group", menuName = "New/Object Pool Group")]
    public class ObjectPoolGroup : ScriptableObject, IComparable<ObjectPoolGroup>
    {
        /// <summary>
        /// The name of the Object Group
        /// </summary>
        [SerializeField]
        [BoxGroup("Group Information")]
        string Name;

        /// <summary>
        /// The Group Tag
        /// </summary>
        [SerializeField]
        [BoxGroup("Group Information")]
        string Tag;

        /// <summary>
        /// The prefabs in the group
        /// </summary>
        /// <typeparam name="GameObject"></typeparam>
        /// <returns></returns>
        [Space]
        [BoxGroup("Prefabs")]
        public List<ObjectInfo> Prefabs = new List<ObjectInfo>();

        public int CompareTo(ObjectPoolGroup other)
        {
            if (other == null || other.Tag == null)
                return 1;

            return Tag.CompareTo(other.Tag);
        }

        /// <summary>
        /// Selects a key from the selected rarity value
        /// </summary>
        /// <returns>returns the selected key</returns>
        public ObjectInfo SelectRandomObjectByRarity()
        {
            if (Prefabs == null)
                throw new System.NullReferenceException("The prefabs list is null");

            if (Prefabs.Count == 0)
                throw new ArgumentException("There are no prefabs in the list");

            if (Prefabs.Count == 1)
                return Prefabs[0];

            float total = 0;
            List<float> randomItems = new List<float>();

            for (int i = 0; i < Prefabs.Count; i++)
            {
                randomItems.Add(total + Prefabs[i].SpawnRarity);
                total += Prefabs[i].SpawnRarity;
            }

            float random = UnityEngine.Random.Range(0, total);

            if (random < randomItems[0])
                return Prefabs[0];

            for (int i = 1; i < randomItems.Count; i++)
            {
                if (randomItems[i - 1] < random && randomItems[i] > random)
                {
                    return Prefabs[i];
                }
            }

            return Prefabs[Prefabs.Count - 1];
        }
    }
}
