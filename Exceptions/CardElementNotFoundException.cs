

using System;

namespace WarManager
{
    /// <summary>
    /// Throw exception when the card element is not found
    /// </summary>
    [Notes.Author("Throw exception when the card element is not found")]
    public class CardElementNotFoundException : Exception
    {
        public override string Message { get; }

        /// <summary>
        /// Empty constructor 
        /// </summary>
        public CardElementNotFoundException()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">the message</param>
        public CardElementNotFoundException(string message)
        {
            Message = message;
        }
    }
}