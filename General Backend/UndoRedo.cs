/* UndoRedo.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using WarManager.Cards;

namespace WarManager
{
	/// <summary>
	/// Undo/Redo capabilities for the war manager
	/// </summary>
	public static class UndoRedo
	{
		/// <summary>
		/// the undo stack
		/// </summary>
		private static Stack<IUndoable> _undoStack = new Stack<IUndoable>();

		/// <summary>
		/// the redo stack
		/// </summary>
		private static Stack<IUndoable> _redoStack = new Stack<IUndoable>();

		/// <summary>
		/// The current Iundoable
		/// </summary>
		private static IUndoable _currentIUndoable;

		/// <summary>
		/// Is undoing an action possible?
		/// </summary>
		public static bool CanUndo
		{
			get
			{
				if (_undoStack.Count > 0)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Is redoing an action possible?
		/// </summary>
		public static bool CanRedo
		{
			get
			{
				if (_redoStack.Count > 0)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Creates a new snapshot and applies the last snapshot to the the stack of undos
		/// </summary>
		public static void CreateNewSnapShot(IUndoable action)
		{

			if (_currentIUndoable != null)
			{
				_undoStack.Push(_currentIUndoable);
			}

			_currentIUndoable = action;

			_redoStack = new Stack<IUndoable>();
		}


		/// <summary>
		/// Undo
		/// </summary>
		public static void Undo()
		{
			if (CanUndo)
			{
				IUndoable currentIUndoable = _undoStack.Pop();

				currentIUndoable.UndoAction();

				if (_currentIUndoable != null)
				{
					_redoStack.Push(_currentIUndoable);
				}

				_currentIUndoable = currentIUndoable;
			}
		}

		/// <summary>
		/// Redo
		/// </summary>
		public static void Redo()
		{
			if (CanRedo)
			{
				IUndoable currentIUndoable = _redoStack.Pop();
				currentIUndoable.RedoAction();

				if (_currentIUndoable != null)
				{
					_undoStack.Push(_currentIUndoable);
				}

				_currentIUndoable = currentIUndoable;
			}
		}
	}
}
