using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager.Help
{
    //Dictates the behavior of the 'Start Here' help slide card
    [Notes.Author("Dictates the behavior of the 'Start Here' help slide card")]
    public class StartHereController : MonoBehaviour
    {

        public Transform offLocation;
        public Transform OnLocation;
        [SerializeField] private TMPro.TMP_Text _textMesh;
        [SerializeField] private string _text = "Start Here";

        bool on = false;

        bool stillUse = false;

        void Start()
        {
            transform.position = offLocation.position;
            on = false;

            LeanTween.delayedCall(3, () =>
            {
                if (SheetsManager.SheetCount < 2)
                    Show();
            });
        }

        void Show()
        {
            if (stillUse)
                return;

            _textMesh.text = _text;

            LeanTween.move(gameObject, OnLocation.position, .75f).setEaseInOutBack();
            on = true;
            // Debug.Log("showing");

            LeanTween.delayedCall(4, Off);
        }

        void Off()
        {
            if (!on)
                return;

            //Debug.Log("off");

            LeanTween.cancel(this.gameObject);
            LeanTween.move(gameObject, offLocation, .75f).setEaseInOutBack();
            on = false;
        }

        public void Used()
        {
            stillUse = true;

            if (on)
                Off();
        }
    }
}
