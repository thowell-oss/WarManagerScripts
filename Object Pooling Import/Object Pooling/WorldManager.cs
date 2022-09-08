
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WarManager.Unity3D.ObjectPooling
{
    /// <summary>
    /// The world
    /// </summary>
    public static class WorldManager
    {
        /// <summary>
        /// backing field
        /// </summary>
        private static WarManager.PoolManager _poolManagerBackingField;

        /// <summary>
        /// The Group Object Pool Manager (can only be initialized once).
        /// </summary>
        /// <value></value>
        public static WarManager.PoolManager PoolManager
        {
            get
            {
                if (_poolManagerBackingField == null)
                    throw new NullReferenceException("The Pool Manager has not been assigned!");

                return _poolManagerBackingField;
            }
            set
            {
                if (_poolManagerBackingField == null)
                {
                    _poolManagerBackingField = value;
                }
                else
                {
                    throw new ArgumentException("the pool manager is already initialized");
                }
            }
        }

        /// <summary>
        /// Does the pool manager exist?
        /// </summary>
        public static bool PoolManagerExists => _poolManagerBackingField != null;

    }
}
