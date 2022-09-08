/* CardDisplayType.cs
 * Author: Taylor Howell
 */
namespace WarManager.Cards
{
    /// <summary>
    /// Tags the ICardDispalyable so that its easier to figure out what kind of data to display
    /// </summary>
    public enum CardDisplayType
    {
        /// <summary>
        /// The display type is not initialized
        /// </summary>
        NotInitialized,
        /// <summary>
        /// Display a string
        /// </summary>
        stringDisplay,
        /// <summary>
        /// Display an int
        /// </summary>
        intDisplay,
        /// <summary>
        /// Display a float
        /// </summary>
        floatDisplay,
        /// <summary>
        /// Display other (get the toString)
        /// </summary>
        other
    }
}
