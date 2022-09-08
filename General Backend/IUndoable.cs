using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager
{
    public interface IUndoable
    {
        /// <summary>
        /// The type of undoable
        /// </summary>
        /// <value></value>
        UndoRedoTag Tag { get; }
        
        /// <summary>
        /// Undo the action
        /// </summary>
        void UndoAction();

        /// <summary>
        /// Redo the action
        /// </summary>
        void RedoAction();
    }
}
