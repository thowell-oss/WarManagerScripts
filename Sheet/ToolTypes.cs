/* ToolTypes.cs
 * Author: Taylor Howell
 */

namespace WarManager
{
    /// <summary>
    /// The types of tools that can be used
    /// </summary>
    public enum ToolTypes
    {
        /// <summary>
        /// Highlight tool used to draw rect (bucket tool??)
        /// </summary>
        Highlight,
        /// <summary>
        /// Select tool used to interact with cards
        /// </summary>
        Edit,
        /// <summary>
        /// Used to move around the environment
        /// </summary>
        Pan,
        /// <summary>
        /// Used to calculate selected card values
        /// </summary>
        Calculate,

        /// <summary>
        /// Draw and create notes on sheet
        /// </summary>
        Annotate,

        /// <summary>
        /// No tool is active at this time
        /// </summary>
        None,
    }
}
