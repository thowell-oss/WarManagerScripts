using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    public class MenuBarManager : MonoBehaviour
    {
        public List<DropMenu> DropMenus = new List<DropMenu>();

        public void ActivateDropMenu(DropMenu DropMenu)
        {
            for (int i = 0; i < DropMenus.Count; i++)
            {
                if (DropMenus[i] != DropMenu)
                {
                    if (DropMenus[i].ActiveMenu)
                        DropMenus[i].ToggleActive();
                }
            }
        }
    }
}
