/* SideWindow_Element_Types.cs
 * Author: Taylor Howell
 */

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Identifyer to show all types of ways to show content
    /// </summary>
    public enum SideWindow_Element_Types
    {
        /// <summary>
        /// Not a valid element type
        /// </summary>
        None,
        /// <summary>
        /// Contains a label
        /// </summary>
        Header,
        /// <summary>
        /// contains a sub-label with a resulting content
        /// </summary>
        Label,
        /// <summary>
        /// This gives a visual gap between elements in the content
        /// </summary>
        Spacer,

        /// <summary>
        /// This is a button element with a callback
        /// </summary>
        Button,

        ///<summary>
        /// Works like a label, except much more text can be added
        ///</summary>
        Paragraph,

        ///<summary>
        /// Advanced tag-system calclator
        ///</summary>
        Calculator,

        ///<summary>
        /// A label/paragraph to represent a card
        ///</summary>
        CardRep,

        ///<summary>
        /// A label/content pair with an edit callback feature - the content can be changed
        ///</summary>
        EditLabel,

        ///<summary>
        /// A Sheet Element object that shows more information about the sheet
        ///</summary>
        SheetElement,
    }
}
