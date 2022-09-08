/* UnityWarManagerTool.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarManager
{
    /// <summary>
    /// Manages tool menu behavior (GUI)
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UnityWarManagerTool : MonoBehaviour
    {
        [SerializeField] private Color SelectedColor;
        [SerializeField] private Color DeselectedColor; 
        [SerializeField] private Color DisabledColor = Color.grey;
        [SerializeField] ToolTypes _toolButton;

        [SerializeField] Image _img;

        

        public void DriveToolSelect()
        {
            ToolsManager.SelectTool(_toolButton);
        }

        public void OnSelectTool(ToolTypes tool)
        {

            if (_img == null)
            {
                _img = GetComponent<Image>();
            }

            Color c = Color.white;

            if (tool == _toolButton)
            {
                c = SelectedColor;
            }
            else
            {
                if (tool == ToolTypes.None)
                {
                    c = DisabledColor;
                }
                else
                {
                    c = DeselectedColor;
                }
            }

            LeanTween.value(this.gameObject, ChangeColor, _img.color, c, .25f).setEaseInExpo();
        }

        private void ChangeColor(Color c)
        {
            _img.color = c;
        }

        public void OnEnable()
        {
            ToolsManager.OnToolSelected += OnSelectTool;
        }

        public void OnDisable()
        {
            ToolsManager.OnToolSelected -= OnSelectTool;
        }
    }
}
