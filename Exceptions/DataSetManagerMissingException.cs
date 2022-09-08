

using System;

namespace WarManager
{

    /// <summary>
    /// Throw exception when the data set manager cannot be found
    /// </summary>
    [Notes.Author("Throw exception when the data set manager cannot be found")]
    public class DataSetManagerMissingException : Exception
    {
        
        public override string Message { get; }


        /// <summary>
        /// Empty constructor 
        /// </summary>
        public DataSetManagerMissingException()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">the message</param>
        public DataSetManagerMissingException(string message)
        {
            Message = message;
        }
    }
}