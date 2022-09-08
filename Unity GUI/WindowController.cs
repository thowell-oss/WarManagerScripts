/* WindowControler.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles general window properties like opening and closing
    /// </summary>
    [Notes.Author("Handles general window properties like opening and closing")]
    public class WindowController : MonoBehaviour
    {
        Animator anim;
        GameObject background;
        GameObject windowBackground;

        WindowTabsController tabsController;


        public void Awake()
        {
            anim = GetComponent<Animator>();
            background = transform.GetChild(1).gameObject;
            windowBackground = transform.GetChild(0).gameObject;
            tabsController = windowBackground.GetComponent<WindowTabsController>();
        }

        /// <summary>
        /// Set the window to be active or disabled
        /// </summary>
        /// <param name="active">the window is active (can use it) or disabled (gone)</param>
        public void ActivateWindow(bool active)
        {

            if (anim == null)
                return;

            anim.SetBool("Open", active);

            if (active)
            {
                background.SetActive(active);
                windowBackground.SetActive(active);

                if (tabsController != null)
                {
                    tabsController.ToggleNewTab(0);
                }
                ToolsManager.SelectedTool = ToolTypes.None;
            }
            else
            {
                ToolsManager.SelectTool(ToolsManager.PreviousTool, true);
                StartCoroutine(WaitToClose());
            }
        }

        /// <summary>
        /// fixes closing animation issues (close anim needs time to disappear).
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitToClose()
        {
            yield return new WaitForSeconds(.5f);
            background.SetActive(false);
            windowBackground.SetActive(false);
        }
    }
}