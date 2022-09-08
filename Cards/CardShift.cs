/* CardShift.cs
 * Author: Taylor Howell
 */
using System;

using UnityEngine;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles storing information for a card shift
    /// </summary>
    public class CardShift
    {
        /// <summary>
        /// The center location where the shift is made
        /// </summary>
        public Point ShiftGridPoint { get; private set; }

        /// <summary>
        /// The direction the shift was made
        /// </summary>
        public Point ShiftVector { get; private set; }

        /// <summary>
        /// The resulting shift grid point (where the card is after the shift was made).
        /// </summary>
        public Point ShiftResultPoint
        {
            get
            {
                return new Point(ShiftGridPoint.x + ShiftVector.x,
                    ShiftGridPoint.y + ShiftVector.y);
            }
        }

        public Layer Layer { get; private set; }

        /// <summary>
        /// This is the card that was shifted
        /// </summary>
        public Card AffectedCard { get; private set; }

        /// <summary>
        /// Constructor, shifts the cards and stores associated information
        /// </summary>
        /// <param name="shiftPoint">the grid point where the card was shifted</param>
        /// <param name="shiftVector">the direction the cards shifted</param>
        /// <param name="affectedCard">A reference to the card that was shifted</param>
        public CardShift(Card affectedCard, Point shiftVector)
        {
            Point shiftPoint = affectedCard.Layout.point;
            Layer = affectedCard.Layer;

            ShiftGridPoint = shiftPoint;
            ShiftVector = shiftVector;
            AffectedCard = affectedCard;

            CardUtility.ShiftCards(shiftPoint, affectedCard.Layer, shiftVector, affectedCard.SheetID);
        }

        /// <summary>
        /// Get the reverse of the shift Vector
        /// </summary>
        /// <returns>returns a tuple Point of the vector</returns>
        public Point GetReverseVector()
        {
            return new Point(-ShiftVector.x, -ShiftVector.y);
        }

        /// <summary>
        /// Shift the cards back to their original position
        /// </summary>
        public void ShiftCardsBack()
        {
            CardUtility.ShiftCards(ShiftResultPoint, Layer, GetReverseVector(), AffectedCard.SheetID, false);
        }

        /// <summary>
        /// Is the gridShiftPoint and shiftvector the same as this card shift?
        /// </summary>
        /// <param name="gridShiftPoint">the grid shift point</param>
        /// <param name="shiftVector">the direction the cards need to shift</param>
        /// <returns></returns>
        public bool IsSameAffect(Point gridShiftPoint, Point shiftVector)
        {
            if (gridShiftPoint == this.ShiftGridPoint && shiftVector == ShiftVector)
                return true;

            return false;
        }

        public override string ToString()
        {
            string str = "";

            if (AffectedCard == null)
                str = $"Shift({ShiftGridPoint}, {ShiftVector}, (null)";
            else
                str = $"Shift({ShiftGridPoint}, {ShiftVector}, {AffectedCard.ID}";

            return str;
        }

    }
}
