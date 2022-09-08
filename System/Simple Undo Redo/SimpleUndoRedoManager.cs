using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using WarManager.Backend;

namespace WarManager
{
    public class SimpleUndoRedoManager : MonoBehaviour
    {
        private Stack<SimpleUndoRedoSnapshot> _undoStack = new Stack<SimpleUndoRedoSnapshot>();
        private Stack<SimpleUndoRedoSnapshot> _redoStack = new Stack<SimpleUndoRedoSnapshot>();

        public int UndoCount;
        public int RedoCount;

        [SerializeField] private Button UndoButton;
        [SerializeField] private Button RedoButton;

        private SimpleUndoRedoSnapshot _current;

        public bool doNotListenForNewSheet = false;

        public delegate void OnUpdateUndoRedo_delegate();
        public static event OnUpdateUndoRedo_delegate OnUndoRedo;

        #region  singleton
        public static SimpleUndoRedoManager main;

        public void Awake()
        {
            if (main != null)
            {
                Destroy(this);
            }
            else
            {
                main = this;
            }
        }
        #endregion

        public void Undo()
        {
            if (_undoStack.Count <= 0)
                return;

            var snapshot = _undoStack.Pop();

            if (_current != null)
                _redoStack.Push(_current);

            SetSnapShot(snapshot);
        }

        /// <summary>
        /// Set a new snapshot
        /// </summary>
        public void NewSnapShot()
        {
            ClearRedo();

            if (_current != null)
            {
                _undoStack.Push(_current);
            }

            _current = new SimpleUndoRedoSnapshot();

            UpdateGUI();
        }

        /// <summary>
        /// Clear the undo stack
        /// </summary>
        public void ClearUndo()
        {
            _undoStack = new Stack<SimpleUndoRedoSnapshot>();
        }

        /// <summary>
        /// Clear the redo stack
        /// </summary>
        public void ClearRedo()
        {
            _redoStack = new Stack<SimpleUndoRedoSnapshot>();
        }

        /// <summary>
        /// Clear the undo and redo stack
        /// </summary>
        public void Clear()
        {
            ClearUndo();
            ClearRedo();
        }

        public void Redo()
        {
            if (_redoStack.Count <= 0)
                return;

            var snapshot = _redoStack.Pop();

            if (_current != null)
                _undoStack.Push(_current);

            SetSnapShot(snapshot);
        }

        private void SetSnapShot(SimpleUndoRedoSnapshot snapshot)
        {
            _current = snapshot;

            doNotListenForNewSheet = true;
            _current.SetSnapShot();

            if (OnUndoRedo != null)
            {
                OnUndoRedo();
            }

            UpdateGUI();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NewSnapShot();
            }

            UndoCount = _undoStack.Count;
            RedoCount = _redoStack.Count;
        }

        private void UpdateGUI()
        {
            UndoButton.interactable = _undoStack.Count > 0;
            RedoButton.interactable = _redoStack.Count > 0;
        }

        private void SetSheetCurrent(string sheetId)
        {
            if (!doNotListenForNewSheet)
            {
                NewSnapShot();
            }
            else
            {
                doNotListenForNewSheet = false;
            }
        }


        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += SetSheetCurrent;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= SetSheetCurrent;
        }
    }
}
