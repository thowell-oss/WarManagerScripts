using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;
namespace WarManager
{
    /// <summary>
    /// Error to be thrown when the sheet is not persistent
    /// </summary>
    [Notes.Author("Error to be thrown when the sheet is not persistent")]
    public class SheetNotPersistentException : Exception
    {
        /// <summary>
        /// The sheet causing the exception
        /// </summary>
        public Sheet<Card> Sheet { get; private set; }

        /// <summary>
        /// private backing field
        /// </summary>
        private string _message;

        /// <summary>
        /// The message of the exception
        /// </summary>
        public override string Message
        {
            get
            {
                if (Sheet != null)
                {
                    if(!string.IsNullOrEmpty(_message))
                    {
                        return "Sheet is not persistent " + Sheet.Name + " (" + Sheet.ID + ") " + _message;
                    }

                    return "Sheet is not persistent " + Sheet.Name + " (" + Sheet.ID + ")";
                }
                   

                return "Sheet is not persistent";
            }
        }

        public SheetNotPersistentException()
        {
            //empty constructor
        }

        public SheetNotPersistentException(string message)
        {
            _message = message;
        }

        public SheetNotPersistentException(Sheet<Card> sheet)
        {
            Sheet = sheet;
        }

        public SheetNotPersistentException(Sheet<Card> sheet, string message)
        {
            Sheet = sheet;
            _message = message;
        }
    }
}
