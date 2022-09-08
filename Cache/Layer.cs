/* Layer.cs
 * Author: Taylor Howell
 */

using System;
using System.Text;

using StringUtility;

namespace WarManager
{
    /// <summary>
    /// The Layer information
    /// </summary>
    public class Layer : IComparable<Layer>//, IEquatable<Layer>
    {
        /// <summary>
        /// The GUID of the layer
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The user defined name of the layer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The sort order of the layer
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// The color of the layer 
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// Can the layer be deleted?
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Layer constructor
        /// </summary>
        /// <param name="name">the name of the layer</param>
        /// <param name="sortOrder">the sort order of the layer</param>
        /// <param name="color">the color of the layer (used for highlighting objects to help the user understand what layer is being used)</param>
        public Layer(string name, int sortOrder, string color)
        {
           //UnityEngine.Debug.Log(name);

            Name = name.ValidateWord();

            ID = Guid.NewGuid().ToString().Substring(0, 5);
            SortOrder = sortOrder;

            Color = color.CleanString_Color_Hex();
        }

        /// <summary>
        /// Create an existing layer
        /// </summary>
        /// <param name="name">the name of the layer</param>
        /// <param name="sortOrder">the sort order of the layer</param>
        /// <param name="color">the color of the layer</param>
        /// <param name="id">the id of the layers</param>
        public Layer(string name, int sortOrder, string color, string id)
        {
            ID = id;
            SortOrder = sortOrder;
            Name = name.ValidateWord();

            Color = color;
        }

        public override string ToString()
        {
            return ID + "," + Name + "," + SortOrder + "," + Color + "," + CanDelete;
        }

        public int CompareTo(Layer other)
        {
            return SortOrder.CompareTo(other.SortOrder);
        }


        // public bool Equals(Layer other)
        // {
        //     if((object)other == null)
        //         return false;

        //     return other.ID == this.ID;
        // }

        // public override bool Equals(object obj)
        // {
        //     if ((Layer)obj != null)
        //     {
        //         return this == (Layer)obj;
        //     }

        //     return false;
        // }

        // public override int GetHashCode()
        // {
        //     return base.GetHashCode();
        //     //throw new NotImplementedException("Check the ID instead");
        // }

        // public static bool operator ==(Layer a, Layer b)
        // {
        //     if ((object)a == null && (object)b == null)
        //         return true;

        //     if ((object)a == null || (object)b == null)
        //         return false;

        //     if (a.ID == null || b.ID == null)
        //         return false;

        //     if (a.ID == b.ID)
        //     {
        //         return true;
        //     }

        //     return false;
        // }

        // public static bool operator !=(Layer a, Layer b)
        // {
        //     return !(a == b);
        // }
    }
}
