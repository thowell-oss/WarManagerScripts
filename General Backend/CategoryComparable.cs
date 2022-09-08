
using System;
using System.Collections;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;


using UnityEngine;

namespace WarManager
{
    public abstract class CategoryComparable
    {
        /// <summary>
        /// Is the Comparable Greedy? 
        /// (Greedy = all cateogories must be the same, 
        /// not Greedy = only one has to be the same)
        /// </summary>
        [JsonIgnore]
        public bool Greedy { get; protected set; } = true;

        /// <summary>
        /// The Categories that the implemented class holds to
        /// </summary>
        [JsonIgnore]
        public string[] PermissionCategories { get; protected set; }

        /// <summary>
        /// Compare the Cateogories
        /// </summary>
        /// <param name="permissionCategoriesToCompare">the categories to compare</param>
        /// <returns>returns true if the categories have been found, false if not</returns>
        public virtual bool CompareCategories(string[] permissionCategoriesToCompare)
        {
            if (permissionCategoriesToCompare == null)
                throw new NullReferenceException("the categories perameter is null");

            if (permissionCategoriesToCompare.Length == 0)
            {
                throw new ArgumentException("there are no elements in the categories list");
            }

            if (PermissionCategories == null)
            {
                throw new NullReferenceException("The Permission Categories for the class is null");
            }


            int compareAmt = 0;

            for (int i = 0; i < permissionCategoriesToCompare.Length; i++)
            {
                for (int j = 0; j < PermissionCategories.Length; j++)
                {
                    if (permissionCategoriesToCompare[i] == PermissionCategories[j])
                    {
                        compareAmt++;
                        if (!Greedy)
                            return true;
                    }
                }
            }


            if(Greedy)
            {
                if(compareAmt == PermissionCategories.Length)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
