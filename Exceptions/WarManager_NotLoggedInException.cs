
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager
{
    /// <summary>
    /// An exception to be thrown when the user is not logged in.
    /// </summary>
    [Notes.Author("An exception to be thrown when the user is not logged in.")]
    public class WarManager_NotLoggedInException : Exception
    {
        public override string Message
        {
            get => "The user is not logged in.";
        }
    }
}
