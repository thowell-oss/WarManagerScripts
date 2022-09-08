/* CardAnimationState.cs
 * Author: Taylor Howel
 */

namespace WarManager.Cards
{
    /// <summary>
    /// The card possible card animation states 
    /// </summary>
    public enum CardAnimationState
    {
        /// <summary>
        /// The Card is not in any specialized state
        /// </summary>
        Idle,
        /// <summary>
        /// A mouse is hovering over the card
        /// </summary>
        Hover,
        /// <summary>
        /// The card is being dragged
        /// </summary>
        Drag,
        /// <summary>
        /// The card needs to spread in order to make room
        /// </summary>
        Spread,
        /// <summary>
        /// The card is in its startup sequence
        /// </summary>
        Start,
        /// <summary>
        /// The card is closing itself
        /// </summary>
        End,
        /// <summary>
        /// The card is minimized/hidden
        /// </summary>
        Minimize,
        /// <summary>
        /// The card is locked
        /// </summary>
        Locked
    }
}
