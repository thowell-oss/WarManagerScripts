using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WarManager;

namespace WarManager.Unity3D
{
    public class QuickActionButton : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text _title;
        [SerializeField] Image iconImage;
        Action _primaryAction;
        List<(string name, Action action, bool enabled)> _secondaryActions = new List<(string name, Action action, bool enabled)>();

        public Button SecondaryButton;

        private TooltipTrigger tooltipTrigger;


        /// <summary>
        /// set the button values
        /// </summary>
        /// <param name="data">the quick action data</param>
        public void SetButton(QuickActionData data)
        {
            _title.text = data.Name;
            iconImage.sprite = data.TypeIcon;
            iconImage.color = data.TypeColor;
            _primaryAction = data.PrimaryAction;

            _secondaryActions.Clear();
            _secondaryActions.AddRange(data.SecondaryActionsArray);

            if (tooltipTrigger == null)
                tooltipTrigger = GetComponent<TooltipTrigger>();

            tooltipTrigger.headerText = data.Name;
            tooltipTrigger.contentText = data.Description;
        }


        private void SelectDown()
        {
            Button b = GetComponent<Button>();


            var selectable = b.FindSelectableOnDown();
            selectable.Select();
        }

        private void SelectUp()
        {
            Button b = GetComponent<Button>();

            var selectable = b.FindSelectableOnUp();
            selectable.Select();
            selectable.OnSelect(null);
        }

        public void PrimaryAction()
        {
            _primaryAction();
        }

        public void More()
        {
            PickMenu.PickMenuManger.main.OpenPickMenu(_secondaryActions, SecondaryButton.transform.position);
        }
    }
}
