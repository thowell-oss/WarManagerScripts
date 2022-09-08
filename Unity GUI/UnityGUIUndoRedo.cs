using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WarManager;

namespace WarManager.Unity3D
{
    public class UnityGUIUndoRedo : MonoBehaviour
    {
        public Button _uxUndo;
        public Button _uxRedo;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _uxUndo.interactable = UndoRedo.CanUndo;
            _uxRedo.interactable = UndoRedo.CanRedo;
        }

        public void Undo()
		{
            UndoRedo.Undo();
		}

        public void Redo()
		{
            UndoRedo.Redo();
		}
    }
}
