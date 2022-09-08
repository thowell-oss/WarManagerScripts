
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Unity3D;


namespace WarManager.Unity3D.PickMenu
{
    public class CardPickMenuOptionsManager : MonoBehaviour
    {
        
        private List<(string title, System.Action action, bool active)> actions = new List<(string title, Action action, bool active)>();

        void Update()
        {
            if(Input.GetMouseButtonDown(1) && GhostCardBehavior.Main.GhostCardVisible)
            {


                // PickMenuManger.main.OpenPickMenu(actions);
            }
        }
    }
}
