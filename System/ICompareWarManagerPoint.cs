/* IGridComperable.cs
 * Author: Taylor Howell
 */

namespace WarManager
{
    /// <summary>
    /// Compare grid coordiantes
    /// </summary>
    public interface ICompareWarManagerPoint
    {
        /// <summary>
        /// The war manager id of the object
        /// </summary>
        /// <value></value>
        public string ID { get; }

        /// <summary>
        /// The point in the grid
        /// </summary>
        public Point point { get; set; }

        /// <summary>
        /// The layer the object resides in
        /// </summary>
        /// <value></value>
        public Layer Layer { get; set; }

        /// <summary>
        /// Compare the x coordiante
        /// </summary>
        /// <param name="x">the x coordiante to compare</param>
        /// <returns>-1 if x is less than, 0 if x is equal, 1 if x is greater</returns>
        int CompareX(float x);

        /// <summary>
        /// Compare the y coordinate
        /// </summary>
        /// <param name="y">the y coordinate to compare</param>
        /// <returns>-1 if y is less than , 0 if y is equal, 1 if y is greater</returns>
        int CompareY(float y);


        /// <summary>
        /// Compare the y coordinate
        /// </summary>
        /// <param name="y">the y coordinate to compare</param>
        /// <returns>-1 if y is less than , 0 if y is equal, 1 if y is greater</returns>
        int CompareLayer(Layer layer);
    }
}
